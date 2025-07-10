using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Doesn't need [Serializable]")]
	public class LatestReleaseBuildsDictionary
	{
		public LatestReleaseBuildsDictionary(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		readonly Dictionary<ZString, Tuple<ZString, ZString>> CargoWiseReleaseRings = new Dictionary<ZString, Tuple<ZString, ZString>>()
		{
			{ ReleaseRings.Codes.DPR,new Tuple<ZString, ZString>( ZString.Empty, ReleaseRings.Codes.STD) },
			{ ReleaseRings.Codes.STD,new Tuple<ZString, ZString>( ReleaseRings.Codes.DPR, ReleaseRings.Codes.GP1) },
			{ ReleaseRings.Codes.GP1,new Tuple<ZString, ZString>( ReleaseRings.Codes.STD, ReleaseRings.Codes.GP2) },
			{ ReleaseRings.Codes.GP2,new Tuple<ZString, ZString>( ReleaseRings.Codes.GP1, ZString.Empty) }
		};

		Dictionary<ZString, ReleaseBuild> LatestBuildsByReleaseRing = new();
		Dictionary<ZString, ReleaseBuild> WeeklyBuildsByReleaseRing = new();
		Dictionary<(ZString ReleaseRing, ZInt MajorVersion, ZInt MinorVersion, ZInt Release), ReleaseBuild> LatestPatchesByVersion = new();
		Dictionary<(ZString ReleaseRing, ZInt MajorVersion, ZInt MinorVersion, ZInt Release), ReleaseBuild> WeeklyPatchesByVersion = new();
		Dictionary<ZString, ZString> LatestReleaseRingProducts = new();

		public void Load()
		{
			var latestAvailableBuildsQuery = new ZQuery(ReleaseBuildSchema.HL_Superceded, ZBool.False);
			latestAvailableBuildsQuery.AddToFilter(ReleaseBuildSchema.HL_IsActive, ZBool.True);
			latestAvailableBuildsQuery.AddToFilter(ReleaseBuildSchema.HL_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise });
			latestAvailableBuildsQuery.OrderBy = ReleaseBuildSchema.Constants.HL_ExeVersionDate + OrderByClause.Descending;

			var releaseBuilds = factory.Load<ReleaseBuild>(latestAvailableBuildsQuery);
			var releaseBuildsByRing = releaseBuilds.GroupBy(rb => rb.HL_ReleaseStatus); // group by Ring
			var releaseBuildsByVersion = releaseBuilds.GroupBy(rb => (rb.HL_ReleaseStatus, rb.HL_MajorVersion, rb.HL_MinorVersion, rb.HL_Release)); // group by Ring & Version

			LatestBuildsByReleaseRing = releaseBuildsByRing.ToDictionary(k => k.Key, g => g.FirstOrDefault());
			WeeklyBuildsByReleaseRing = releaseBuildsByRing.ToDictionary(k => k.Key, g => g.Skip(1).FirstOrDefault() ?? g.FirstOrDefault()); //if we need weekly build then take the second one active or the latest if the only one exists
			LatestPatchesByVersion = releaseBuildsByVersion.ToDictionary(k => k.Key, g => g.FirstOrDefault());
			WeeklyPatchesByVersion = releaseBuildsByVersion.ToDictionary(k => k.Key, g => g.Skip(1).FirstOrDefault() ?? g.FirstOrDefault()); //if we need weekly build then take the second one active or the latest if the only one exists

			var sqlFilter =
@$"{ReleaseBuildSchema.Constants.PK} IN (
select {ReleaseBuildSchema.Constants.PK}
from 
	(	
		select {ReleaseBuildSchema.Constants.PK}, Ranking = row_number() over(partition by {ReleaseBuildSchema.Constants.HL_ReleaseStatus} order by {ReleaseBuildSchema.Constants.HL_ExeVersionDate} desc)
		from dbo.{ReleaseBuildSchema.Constants.TableName}
		where {ReleaseBuildSchema.Constants.HL_ReleaseStatus} in ('{ReleaseRings.Codes.ALP}','{ReleaseRings.Codes.DPR}','{ReleaseRings.Codes.STD}','{ReleaseRings.Codes.GP1}','{ReleaseRings.Codes.GP2}')
	) Builds
where
	Ranking = 1
)";
			var latestBuildsQuery = new ZDBOnlyQuery(typeof(ReleaseBuild));
			latestBuildsQuery.AddFilterAndZSQLParameterCollection(sqlFilter, null);
			var latestBuilds = factory.Load<ReleaseBuild>(latestBuildsQuery);
			LatestReleaseRingProducts = latestBuilds.ToDictionary(k => k.HL_ReleaseStatus, v => v.HL_Product);
		}

		public IEnumerable<Tuple<ZString, ZString>> GetAvailableBuildProductAndReleaseRings(bool takeWeeklyBuildsInsteadOfLatest)
		{
			var builds = takeWeeklyBuildsInsteadOfLatest ? WeeklyBuildsByReleaseRing : LatestBuildsByReleaseRing;
			foreach (var key in builds.Keys)
			{
				yield return new Tuple<ZString, ZString>(builds[key].HL_Product, key);
			}
		}

		public bool GpcReleaseBuildIsAvailable
		{
			get
			{
				if (!LatestBuildsByReleaseRing.TryGetValue(ReleaseRings.Codes.GPC, out ReleaseBuild gpcBuild))
				{
					return false;
				}
				else if (!LatestBuildsByReleaseRing.TryGetValue(ReleaseRings.Codes.GPR, out ReleaseBuild gprBuild))
				{
					return true;
				}
				else
				{
					return (gpcBuild.VersionNumber > gprBuild.VersionNumber);
				}
			}
		}

		/// <summary>
		/// Locates the latest Release Build for the given Product and Ring Code.
		/// </summary>
		/// <param name="licensedProduct"></param>
		/// <param name="releaseRing"></param>
		/// <param name="takeWeeklyBuildsInsteadOfLatest"></param>
		/// <returns></returns>
		public ReleaseBuild GetLatestAvailableBuild(string licensedProduct, string releaseRing, bool takeWeeklyBuildsInsteadOfLatest)
		{
			isNeedSkipDeploy = false;
			if (string.IsNullOrEmpty(licensedProduct))
			{
				return null;
			}

			var buildsByReleaseRing = takeWeeklyBuildsInsteadOfLatest ? WeeklyBuildsByReleaseRing : LatestBuildsByReleaseRing;
			var latestReleaseBuild = GetLatestAvailableBuildByReleaseRing(releaseRing, takeWeeklyBuildsInsteadOfLatest);

			if (EDIDataRegistry.Instance.EnableCargoWiseNextTransitionVersionFallbackRule.Value)
			{
				if (licensedProduct == ProductTypes.Codes.CargoWiseOne && !IsProductSupportedInReleaseRing(licensedProduct, releaseRing))
				{
					var currentReleaseRing = releaseRing;
					while (!string.IsNullOrEmpty(currentReleaseRing))
					{
						if (CargoWiseReleaseRings.TryGetValue(currentReleaseRing, out var releaseRingInfo))
						{
							currentReleaseRing = releaseRingInfo.Item2; //Get higher release ring code

							buildsByReleaseRing.TryGetValue(currentReleaseRing, out var nextRingBuild);
							if (nextRingBuild != null && IsSameProduct(licensedProduct, nextRingBuild.HL_Product))
							{
								latestReleaseBuild = nextRingBuild;
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
				else if (licensedProduct == ProductTypes.Codes.CargoWiseNext && !IsProductSupportedInReleaseRing(licensedProduct, releaseRing))
				{
					var currentReleaseRing = releaseRing;

					while (!string.IsNullOrEmpty(currentReleaseRing))
					{
						if (CargoWiseReleaseRings.TryGetValue(currentReleaseRing, out var releaseRingInfo))
						{
							currentReleaseRing = releaseRingInfo.Item1; //Get lower release ring code

							buildsByReleaseRing.TryGetValue(currentReleaseRing, out var previousRingBuild);
							if (previousRingBuild != null && IsSameProduct(licensedProduct, previousRingBuild.HL_Product))
							{
								latestReleaseBuild = previousRingBuild;
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
			}

			if (latestReleaseBuild != null && !string.IsNullOrEmpty(licensedProduct) && !IsSameProduct(licensedProduct, latestReleaseBuild.HL_Product))
			{
				if (latestReleaseBuild.HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext) && licensedProduct.Equals(ProductTypes.Codes.CargoWiseOne))
				{
					isNeedSkipDeploy = true;
				}
				latestReleaseBuild = null;
			}

			return latestReleaseBuild;
		}

		public VersionNumber GetLatestAvailableBuildVersion(string releaseRing, bool takeWeeklyBuildsInsteadOfLatest)
		{
			var latestReleaseBuild = GetLatestAvailableBuildByReleaseRing(releaseRing, takeWeeklyBuildsInsteadOfLatest);
			return latestReleaseBuild?.VersionNumber ?? new VersionNumber();
		}

		ReleaseBuild GetLatestAvailableBuildByReleaseRing(string releaseRing, bool takeWeeklyBuildsInsteadOfLatest)
		{
			ReleaseBuild latestReleaseBuild;
			var buildsByReleaseRing = takeWeeklyBuildsInsteadOfLatest ? WeeklyBuildsByReleaseRing : LatestBuildsByReleaseRing;
			if (releaseRing == ReleaseRings.Codes.GPC && !GpcReleaseBuildIsAvailable)
			{
				buildsByReleaseRing.TryGetValue(ReleaseRings.Codes.GPR, out latestReleaseBuild);
			}
			else
			{
				buildsByReleaseRing.TryGetValue(releaseRing, out latestReleaseBuild);
			}
			return latestReleaseBuild;
		}

		/// <summary>
		/// Determines which release build is the best upgrade from the version currently installed.
		/// Returns the installed version when the installed version matches or is newer than the available release version.
		/// Returns latest build for ring when a build matching installedVersion does not exist.
		/// </summary>
		/// <param name="licensedProduct"></param>
		/// <param name="licensedReleaseRing"></param>
		/// <param name="installedVersion"></param>
		/// <param name="isPatchOnly"></param>
		/// <param name="takeWeeklyBuildsInsteadOfLatest"></param>
		/// <returns></returns>
		public ReleaseBuild GetLatestAvailableBuildForInstalledVersion(string licensedProduct, string licensedReleaseRing, VersionNumber installedVersion, bool isPatchOnly, bool takeWeeklyBuildsInsteadOfLatest)
		{
			isNeedSkipDeploy = false;
			if (string.IsNullOrEmpty(licensedProduct))
			{
				return null;
			}

			ReleaseBuild latestReleaseBuild = null;
			var installedBuild = GetReleaseBuild(installedVersion);

			if (!isPatchOnly)
			{
				latestReleaseBuild = GetLatestAvailableBuild(licensedProduct, licensedReleaseRing, takeWeeklyBuildsInsteadOfLatest);
				if (installedBuild != null
					&& (latestReleaseBuild == null || installedBuild.HL_Product.EqualsIgnoringCase(latestReleaseBuild.HL_Product))
					&& ShouldSendLatestGP2(installedBuild, licensedReleaseRing))
				{
					return GetLatestAvailableBuild(licensedProduct, ReleaseRings.Codes.GP2, takeWeeklyBuildsInsteadOfLatest);
				}
			}
			else
			{
				latestReleaseBuild = GetLatestAvailablePatch(licensedProduct, licensedReleaseRing, installedVersion, takeWeeklyBuildsInsteadOfLatest);
			}

			if (latestReleaseBuild == null)
			{
				return null;
			}
			var licensedRingVersion = latestReleaseBuild.VersionNumber;

			if (installedVersion > licensedRingVersion)
			{
				latestReleaseBuild = GetBuildFromPreviousReleaseRing(licensedReleaseRing, installedVersion, isPatchOnly, takeWeeklyBuildsInsteadOfLatest, latestReleaseBuild, installedBuild);
			}

			if (latestReleaseBuild != null && !IsSameProduct(licensedProduct, latestReleaseBuild.HL_Product))
			{
				if (latestReleaseBuild.HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext) && licensedProduct.Equals(ProductTypes.Codes.CargoWiseOne))
				{
					isNeedSkipDeploy = true;
				}
				latestReleaseBuild = null;
			}

			return latestReleaseBuild;
		}

		ReleaseBuild GetLatestAvailablePatch(string licensedProduct, string licensedReleaseRing, VersionNumber installedVersion, bool takeWeeklyBuildsInsteadOfLatest)
		{
			isNeedSkipDeploy = false;
			ReleaseBuild latestReleaseBuild = null;
			var patchesByVersion = takeWeeklyBuildsInsteadOfLatest ? WeeklyPatchesByVersion : LatestPatchesByVersion;
			patchesByVersion.TryGetValue((licensedReleaseRing, installedVersion.Major, installedVersion.Minor, installedVersion.Release), out latestReleaseBuild);

			if (EDIDataRegistry.Instance.EnableCargoWiseNextTransitionVersionFallbackRule.Value)
			{
				if (licensedProduct == ProductTypes.Codes.CargoWiseOne && (!IsProductSupportedInReleaseRing(licensedProduct, licensedReleaseRing) || latestReleaseBuild == null))
				{
					var currentReleaseRing = licensedReleaseRing;
					while (!string.IsNullOrEmpty(currentReleaseRing))
					{
						if (CargoWiseReleaseRings.TryGetValue(currentReleaseRing, out var releaseRingInfo))
						{
							currentReleaseRing = releaseRingInfo.Item2; //Get higher release ring code

							patchesByVersion.TryGetValue((currentReleaseRing, installedVersion.Major, installedVersion.Minor, installedVersion.Release), out var nextRingBuild);
							if (nextRingBuild != null && nextRingBuild.VersionNumber >= installedVersion && IsSameProduct(licensedProduct, nextRingBuild.HL_Product))
							{
								latestReleaseBuild = nextRingBuild;
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
				else if (licensedProduct == ProductTypes.Codes.CargoWiseNext && !IsProductSupportedInReleaseRing(licensedProduct, licensedReleaseRing))
				{
					var currentReleaseRing = licensedReleaseRing;

					while (!string.IsNullOrEmpty(currentReleaseRing))
					{
						if (CargoWiseReleaseRings.TryGetValue(currentReleaseRing, out var releaseRingInfo))
						{
							currentReleaseRing = releaseRingInfo.Item1; //Get lower release ring code

							patchesByVersion.TryGetValue((currentReleaseRing, installedVersion.Major, installedVersion.Minor, installedVersion.Release), out var previousRingBuild);
							if (previousRingBuild != null && previousRingBuild.VersionNumber >= installedVersion && IsSameProduct(licensedProduct, previousRingBuild.HL_Product))
							{
								latestReleaseBuild = previousRingBuild;
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
			}

			if (latestReleaseBuild != null && !string.IsNullOrEmpty(licensedProduct) && !IsSameProduct(licensedProduct, latestReleaseBuild.HL_Product))
			{
				if (latestReleaseBuild.HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext) && licensedProduct.Equals(ProductTypes.Codes.CargoWiseOne))
				{
					isNeedSkipDeploy = true;
				}
				latestReleaseBuild = null;
			}

			return latestReleaseBuild;
		}

		#region Version number check

		ReleaseBuild GetBuildFromPreviousReleaseRing(string licensedReleaseRing, VersionNumber installedVersion, bool isPatchOnly, bool takeWeeklyBuildsInsteadOfLatest, ReleaseBuild latestReleaseBuild, ReleaseBuild installedBuild)
		{
			if (isPatchOnly)
			{
				return GetReleaseBuild(installedVersion);
			}

			// Cannot return a build when the installed version is newer than the licensed version
			//  unless the installed version is from a newer (inner) ring.
			if (installedBuild != null)
			{
				// return the currently installed build unless a newer build can be located
				latestReleaseBuild = installedBuild;

				var outerRingCode = licensedReleaseRing;
				var innerRingCode = installedBuild.HL_ReleaseStatus;

				while (outerRingCode != innerRingCode)
				{
					outerRingCode = ReleaseRingsLookup.Instance.GetPreviousRing(outerRingCode);

					// GetPreviousRing returns an empty string when there is no previous ring.
					// Will form an infinite loop if not exit here
					if (string.IsNullOrWhiteSpace(outerRingCode))
					{
						break;
					}

					ReleaseBuild nextRingBuild = null;
					var buildsByReleaseRing = takeWeeklyBuildsInsteadOfLatest ? WeeklyBuildsByReleaseRing : LatestBuildsByReleaseRing;
					if (buildsByReleaseRing.TryGetValue(outerRingCode, out nextRingBuild) && nextRingBuild.VersionNumber >= installedVersion)
					{
						latestReleaseBuild = nextRingBuild;
						break;
					}
				}
			}

			return latestReleaseBuild;
		}

		ReleaseBuild GetReleaseBuild(VersionNumber releaseVersion)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, releaseVersion.Major);
			query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, releaseVersion.Minor);
			query.AddToFilter(ReleaseBuildSchema.HL_Release, releaseVersion.Release);
			query.AddToFilter(ReleaseBuildSchema.HL_Patch, releaseVersion.Patch);
			query.OrderBy = ReleaseBuildSchema.Constants.HL_ExeVersionDate + OrderByClause.Descending;
			return factory.LoadTop1<ReleaseBuild>(query);
		}

		#endregion

		#region GP2

		/// <summary>
		/// 1. Where the installedVersion ring is GP1 and the licensedReleaseRing is GP1 but we should send a GP2 release if the GP1 they are on is now GP2. 
		/// If the installed version release date or major/minor/release as the same as the current GP2
		/// E.g. current GP1 is 17-DEC-18 / 18.12.17.793, next week when we have a new GP1 the GP2 will be 17-DEC-18 / 18.12.17.??? 
		/// 
		/// 2. Same for when the installedVersion is GP2 and the licesenedReleaseRing is GP1, we should again send the GP2 not GP1.
		/// </summary>
		/// <param name="installedBuild"></param>
		/// <param name="licesenedReleaseRing"></param>
		/// <returns></returns>
		bool ShouldSendLatestGP2(ReleaseBuild installedBuild, ZString licesenedReleaseRing)
		{
			var result = false;

			if (installedBuild.HL_ReleaseStatus == ReleaseRings.Codes.GP1 && licesenedReleaseRing == ReleaseRings.Codes.GP1)
			{
				var query = new ZQuery();
				query.AddToFilter(ReleaseBuildSchema.HL_ReleaseStatus, ReleaseRings.Codes.GP2);
				query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, installedBuild.HL_MajorVersion);
				query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, installedBuild.HL_MinorVersion);
				query.AddToFilter(ReleaseBuildSchema.HL_Release, installedBuild.HL_Release);
				query.AddToFilter(ReleaseBuildSchema.HL_ExeVersionDate, installedBuild.HL_ExeVersionDate);
				return factory.ExistsInDatabase(ReleaseBuildSchema.Constants.TableName, query);
			}
			else if (installedBuild.HL_ReleaseStatus == ReleaseRings.Codes.GP2 && licesenedReleaseRing == ReleaseRings.Codes.GP1)
			{
				result = true;
			}

			return result;
		}

		#endregion

		#region Product Check

		static bool IsSameProduct(ZString licensedProduct, ZString releaseBuildProduct)
		{
			return releaseBuildProduct.EqualsIgnoringCase(licensedProduct)
				|| releaseBuildProduct.EqualsIgnoringCase(ProductTypes.Codes.CargoWise)
				|| (releaseBuildProduct.EqualsIgnoringCase(ProductTypes.Codes.Enterprise)
					&& licensedProduct.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseOne));
		}

		bool IsProductSupportedInReleaseRing(ZString licensedProduct, ZString licensedReleaseRing)
		{
			if (LatestReleaseRingProducts.TryGetValue(licensedReleaseRing, out ZString buildProduct))
			{
				return IsSameProduct(licensedProduct, buildProduct);
			}
			return false;
		}

		#endregion

		#region isNeedSkipDeploy

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public bool isNeedSkipDeploy { get; private set; }

		#endregion
	}
}

