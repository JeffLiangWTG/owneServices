using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.AutoDeploy
{
	public class PackageImporter : Disposable
	{
		public PackageImporter(BusinessObjectFactory factory, string packagePath)
		{
			Argument.NotNull(factory, "factory");
			this.factory = factory;
			this.packagePath = packagePath;
			this.releaseInfo = new Lazy<ReleaseInfo>(OpenReleaseInfo);
		}

		ReleaseInfo OpenReleaseInfo()
		{
			using (var packageStream = File.OpenRead(packagePath))
			using (var packageZip = new ZipArchive(packageStream, ZipArchiveMode.Read, false))
			{
				releaseInfoFile = TempFile.New();
				Func<ZipArchiveEntry, bool> predicate = entry => entry.Name.Equals(ReleaseInfo.XmlFileName, StringComparison.OrdinalIgnoreCase);
				if (packageZip.Entries.Count(predicate) != 1)
				{
					throw new InvalidDataException();
				}

				packageZip.Entries.Single(predicate).ExtractToFile(releaseInfoFile.Filename, true);
				return new ReleaseInfo(releaseInfoFile.Filename);
			}
		}

		public bool IsAlreadyImported()
		{
			var filter = new ZQuery();
			filter.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, releaseInfo.Value.VersionNumber.Major);
			filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, releaseInfo.Value.VersionNumber.Minor);
			filter.AddToFilter(ReleaseBuildSchema.HL_Release, releaseInfo.Value.VersionNumber.Release);
			filter.AddToFilter(ReleaseBuildSchema.HL_Patch, releaseInfo.Value.VersionNumber.Patch);
			return factory.ExistsInDatabase(ReleaseBuildSchema.Constants.TableName, filter);
		}

		public ReleaseBuild Import()
		{
			VersionNumber versionNumber = releaseInfo.Value.VersionNumber;

			var build = factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = releaseInfo.Value.ExeDate;
			build.HL_MajorVersion = versionNumber.Major;
			build.HL_MinorVersion = versionNumber.Minor;
			build.HL_Release = versionNumber.Release;
			build.HL_Patch = versionNumber.Patch;
			build.HL_ReleaseStatus = releaseInfo.Value.ReleaseRing;
			build.HL_PackagePath = packagePath;

			var cgwMinVersion = EDIDataRegistry.Instance.CargoWiseMinimumVersion.Value;
			if (!cgwMinVersion.IsNullOrEmpty() && LicenceDatabase.BuildIsSupported(versionNumber, cgwMinVersion))
			{
				build.HL_Product = ProductTypes.Codes.CargoWise;
			}
			else
			{
				var cwNextMinVersion = EDIDataRegistry.Instance.CargoWiseNextMinimumVersion.Value;
				if (!cwNextMinVersion.IsNullOrEmpty() && LicenceDatabase.BuildIsSupported(versionNumber, cwNextMinVersion))
				{
					build.HL_Product = ProductTypes.Codes.CargoWiseNext;
				}
			}

			ProcessSuperceedingRulesAndAssignWeeklyBuild(build);

			factory.Save();

			return build;
		}

		void ProcessSuperceedingRulesAndAssignWeeklyBuild(ReleaseBuild buildToCreate)
		{
			var queryToExistingSameRingReleaseBuilds = new ZQuery(ReleaseBuildSchema.HL_ReleaseStatus, releaseInfo.Value.ReleaseRing)
			{
				OrderBy = ReleaseBuildSchema.Constants.HL_MajorVersion + OrderByClause.Descending +
					", " + ReleaseBuildSchema.Constants.HL_MinorVersion + OrderByClause.Descending +
					", " + ReleaseBuildSchema.Constants.HL_Release + OrderByClause.Descending +
					", " + ReleaseBuildSchema.Constants.HL_Patch + OrderByClause.Descending
			};

			queryToExistingSameRingReleaseBuilds.AddToFilter(ReleaseBuildSchema.PK, SQLComparisonOperator.NotEqual, buildToCreate.PK);

			var latestExistingBuild = factory.LoadTop1<ReleaseBuild>(queryToExistingSameRingReleaseBuilds);

			if (latestExistingBuild == null)
			{
				buildToCreate.HL_Superceded = false;
				return;
			}

			// Set buildToCreate.HL_Superceded to true as an emergency fallback mechanism.
			// This ensures that if the latest existing build has been superseded(Manually set in the ReleaseBuild Module), it immediately stops the current update push for this ReleaseRing.
			// It also prevents any interference with other ReleaseRings.
			if (latestExistingBuild.HL_Superceded || latestExistingBuild.VersionNumber > buildToCreate.VersionNumber)
			{
				buildToCreate.HL_Superceded = true;
				return;
			}

			buildToCreate.HL_Superceded = false;

			queryToExistingSameRingReleaseBuilds.AddToFilter(ReleaseBuildSchema.HL_Superceded, false);
			queryToExistingSameRingReleaseBuilds.MaximumRows = 2;//there are not supposed to be more than 2 active builds within the same Ring

			var nonSupercededExistingSameRingReleaseBuilds = factory.Load<ReleaseBuild>(queryToExistingSameRingReleaseBuilds);

			DateTime lastCutOffBuildDateTime;
			ReleaseBuild weeklyCutOffBuild = null;
			var weeklyBuildCutOffSettings = new WeeklyCutOffBuildSettings();

			if (weeklyBuildCutOffSettings.IsRequired)
			{
				lastCutOffBuildDateTime = weeklyBuildCutOffSettings.GetLatestCutOffDateTime(releaseInfo.Value.ExeDate);

				var secondLatestExistingActiveBuild = nonSupercededExistingSameRingReleaseBuilds.Skip(1).FirstOrDefault();

				weeklyCutOffBuild =
					(secondLatestExistingActiveBuild == null || latestExistingBuild.HL_ExeVersionDate <= lastCutOffBuildDateTime)
					? latestExistingBuild
					: secondLatestExistingActiveBuild;

				weeklyCutOffBuild.HL_Comment = string.Format(CultureInfo.InvariantCulture, "Latest weekly {0}", releaseInfo.Value.ReleaseRing);

				if (weeklyCutOffBuild.Factory.HasContext(EDIConstants.BusinessContext.ImportBuildsServiceTask) && weeklyCutOffBuild.HL_CommentInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					weeklyCutOffBuild.Logs.AddNew(AutoEvents.EditedARecord, "Marked as weekly build by IBP Service Task");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}

			foreach (var bld in nonSupercededExistingSameRingReleaseBuilds.Where(b => b.PK != weeklyCutOffBuild?.PK))
			{
				bld.HL_Superceded = true;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			releaseInfoFile?.Dispose();
		}

		readonly BusinessObjectFactory factory;
		readonly string packagePath;
		readonly Lazy<ReleaseInfo> releaseInfo;
		TempFile releaseInfoFile;
	}
}

