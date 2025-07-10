using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business
{
	#region Exception and Constants

	[Serializable]
	public class ReleaseBuildException : Exception
	{
		public ReleaseBuildException(string message)
			: base(message)
		{ }

#if NETFRAMEWORK
		protected ReleaseBuildException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	#endregion

	[SingleObjectAroundARow, CodeProperty(ReleaseBuild.Schema.ExeVersion),
	DescriptionProperty(ReleaseBuild.Schema.ReleaseDisplayText)]
	public partial class ReleaseBuild : AutoReleaseBuild
	{
		#region Schema

		public new abstract class Schema : AutoReleaseBuild.Schema
		{
			public const string ExeVersion = "ExeVersion";
			public const string ReleaseDisplayText = "ReleaseDisplayText";
		}

		#endregion

		public ReleaseBuild(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loading

		public override void OnLoaded()
		{
			base.OnLoaded();
			originalHL_Superceded = HL_Superceded;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HL_Superceded = false;
		}

		bool originalHL_Superceded;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Delete

		public override void Delete()
		{
			DeletePackage();
			base.Delete();
		}

		public override string CanReactivate()
		{
			return File.Exists(HL_PackagePath) ? null : "Master package file for this release has been permanently deleted";
		}

		#endregion

		#region Saved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && !HL_IsActive)
			{
				DeletePackage();
			}

			if (saveSucceeded && HL_Superceded && !originalHL_Superceded && IsCargoWiseProduct)
			{
				if (OnDeleteBuild())
				{
					RemoveBuildAndStopQueue();
				}
			}

			originalHL_Superceded = HL_Superceded;
		}

		public event CancelEventHandler DeleteBuild;

		bool OnDeleteBuild()
		{
			bool result = false;
			CancelEventHandler deleteBuild = DeleteBuild;
			if (deleteBuild != null)
			{
				CancelEventArgs args = new CancelEventArgs();
				deleteBuild(this, args);
				result = !args.Cancel;
			}
			return result;
		}

		#endregion

		#region Delete Build from FTP and Queue

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event FTPDeleteFailedEventHandler FTPDeleteFailed;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public delegate void FTPDeleteFailedEventHandler(List<string> clientCodesAndReasons);

		void OnFTPDeleteFailed(List<string> clientCodesAndReasons)
		{
			if (FTPDeleteFailed != null)
			{
				FTPDeleteFailed(clientCodesAndReasons);
			}
		}

		void RemoveBuildAndStopQueue()
		{
			BlockQueuedUpgrades();
			FailQueuedUpgradeEmails();
			DeleteBuildFromFTP();
			if (FTPFilesCannotBeDeleted != null && FTPFilesCannotBeDeleted.Count > 0)
			{
				OnFTPDeleteFailed(FTPFilesCannotBeDeleted);
			}

			fClientsSentToButNotApplied = null;
		}

#if DEBUG
		protected virtual
#endif
 string FTPGenericDirectoryName
		{
			get { return UpgradeConstants.WebServerGenericPath; }
		}

#if DEBUG
		protected virtual
#endif
 string FTPClientSpecificDirectoryName
		{
			get { return UpgradeConstants.WebServerClientSpecificPath; }
		}

		List<string> FTPFilesCannotBeDeleted;

		void DeleteBuildFromFTP()
		{
			FTPFilesCannotBeDeleted = new List<string>();

			string genericPath = Path.Combine(FTPGenericDirectoryName, PackageName);
			DeletePackageFromPath(genericPath);

			if (Directory.Exists(FTPClientSpecificDirectoryName))
			{
				string[] clientSpecificDirectories = Directory.GetDirectories(FTPClientSpecificDirectoryName);
				foreach (string clientDirectory in clientSpecificDirectories)
				{
					string clientSpecificPath = Path.Combine(clientDirectory, PackageName);
					DeletePackageFromPath(clientSpecificPath);
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void DeleteFile(string path)
		{
			File.Delete(path);
		}

		void DeletePackageFromPath(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					DeleteFile(path);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					FTPFilesCannotBeDeleted.Add(path + " - " + e.Message);
				}
			}
		}

		void BlockQueuedUpgrades()
		{
			UpgradesToClientCollection upgradesQueued = new UpgradesToClientCollection(Factory);
			ZQuery upgradesQuery = new ZQuery(UpgradesToClientSchema.L1_HL, PK);
			upgradesQuery.AddToFilter(UpgradesToClientSchema.L1_CurrentStatus, UpgradesToClientStatus.Codes.Queued);
			upgradesQueued.Load(upgradesQuery);
			foreach (UpgradesToClient queuedUpgrade in upgradesQueued)
			{
				queuedUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Blocked;
				Logs.AddNew(Events.Cancelled, queuedUpgrade.Header.OH_Code + " - " + queuedUpgrade.LicDatabase.LD_ServerCode);
				queuedUpgrade.Header.Logs.AddNew(Events.Cancelled, "Release Build " + PackageName);
			}

			Factory.Save();
		}

		void FailQueuedUpgradeEmails()
		{
			ZQuery mailQuery = new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, PackageName);
			ZQuery statusQuery = new ZQuery(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			statusQuery.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_Status, SQLComparisonOperator.Equal, MailStatus.QueuedWithAck);
			mailQuery.AddToFilter(statusQuery);
			MailItemCollection queuedMail = new StandardMailItemCollection(Factory, mailQuery);
			queuedMail.Load();
			foreach (MailItem mail in queuedMail)
			{
				mail.MI_Status = MailStatus.Failed;
			}

			Factory.Save();
		}

		#endregion

		#region Related Business Objects

		#region Licence Headers

		public LicenceHeaderCollection ReleaseLicences
		{
			get
			{
				if (releaseLicences == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
					ZDBOnlySubQuery licenceSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
					ZDBOnlySubQuery buildSubQuery = new ZDBOnlySubQuery(typeof(ReleaseBuild), ReleaseBuildSchema.PK);

					buildSubQuery.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, HL_MajorVersion);
					buildSubQuery.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, HL_MinorVersion);
					buildSubQuery.AddToFilter(ReleaseBuildSchema.HL_Release, HL_Release);

					licenceSubQuery.AddSubQuery(LicenceDatabaseSchema.LD_HL_CurrentRunningVersion, buildSubQuery, JoinCondition.And);
					query.AddSubQuery(licenceSubQuery, JoinCondition.And);

					var localReleaseLicences = new LicenceHeaderCollection(Factory, query);
					localReleaseLicences.Load();
					releaseLicences = localReleaseLicences;
					releaseLicences.SetReadOnlyIncludingChildren(true);
				}
				return releaseLicences;
			}
		}

		public FilteredLicenceHeaderCollection FilteredReleaseLicences
		{
			get
			{
				if (filteredReleaseLicences == null)
				{
					filteredReleaseLicences = new FilteredLicenceHeaderCollection(ReleaseLicences);
					filteredReleaseLicences.IncludeAllItems = false;
				}
				return filteredReleaseLicences;
			}
		}

		public LicenceHeaderCollection PatchLicences
		{
			get
			{
				if (patchLicences == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
					subQuery.AddToFilter(LicenceDatabaseSchema.LD_HL_CurrentRunningVersion, PK);
					query.AddSubQuery(subQuery, JoinCondition.And);

					var localPatchLicences = new LicenceHeaderCollection(Factory, query);
					localPatchLicences.Load();
					patchLicences = localPatchLicences;
					patchLicences.SetReadOnlyIncludingChildren(true);
				}
				return patchLicences;
			}
		}

		public FilteredLicenceHeaderCollection FilteredPatchLicences
		{
			get
			{
				if (filteredPatchLicences == null)
				{
					filteredPatchLicences = new FilteredLicenceHeaderCollection(PatchLicences);
					filteredPatchLicences.IncludeAllItems = false;
				}
				return filteredPatchLicences;
			}
		}

		public ZBool ShowCargoWiseLicences
		{
			get { return FilteredReleaseLicences.IncludeAllItems && FilteredPatchLicences.IncludeAllItems; }
			set
			{
				FilteredReleaseLicences.IncludeAllItems = value;
				FilteredPatchLicences.IncludeAllItems = value;
				ShowCargoWiseLicencesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowCargoWiseLicencesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowCargoWiseLicences)); }
		}

		LicenceHeaderCollection releaseLicences;
		LicenceHeaderCollection patchLicences;
		FilteredLicenceHeaderCollection filteredReleaseLicences;
		FilteredLicenceHeaderCollection filteredPatchLicences;

		#endregion

		#region Incidents Reported

		public SupportIncidentCollection IncidentsReported
		{
			get
			{
				if (fIncidentsReported == null)
				{
					ZQuery filter = new ZQuery(IncidentMainSchema.IM_HL_ClientReportedOnVersion, SQLComparisonOperator.Equal, PK);
					var localIncidentsReported = new SupportIncidentCollection(Factory, filter);
					localIncidentsReported.Load();
					fIncidentsReported = localIncidentsReported;
					fIncidentsReported.SetReadOnlyIncludingChildren(true);
				}

				return fIncidentsReported;
			}
		}

		SupportIncidentCollection fIncidentsReported;

		#endregion

		#region Clients Sent To But Not Applied

		public EDIOrgHeaderCollection ClientsSentToButNotApplied
		{
			get
			{
				if (fClientsSentToButNotApplied == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIOrgHeader));
					fClientsSentToButNotApplied = new EDIOrgHeaderCollection(Factory, query);
					fClientsSentToButNotApplied.SetReadOnlyIncludingChildren(true);

					if (HL_ExeVersionDate.IsValid)
					{
						string sqlText = string.Format(CultureInfo.CurrentCulture, @"
						{0} IN
						(
							SELECT {0}
							FROM {1}, {2}, {3}, {4}, {5} 
							WHERE {0} = {6}
							AND {7} = {8}
							AND {9} = {10}
							AND
							(
								{11} IS NULL
								OR {12} = {11}
							)
							AND {13} IN
							(
								SELECT {14}
								FROM {15}
								WHERE {16} = @SL_SE_NKEvent
								AND {17} = '{5}'
								AND {18} = @HL_PK
							)
							AND {13} NOT IN
							(
								SELECT SUBSTRING({14}, 1, LEN({14}) - 6)
								FROM {15}
								WHERE {16} = @CancelledEvent
								AND {17} = '{5}'
								AND {18} = @HL_PK
							)
							AND {12} != @HL_PK
							AND {19} < @HL_ExeVersionDate
						)",
							OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName,                                // 0, 1
							LicenceHeaderSchema.Constants.TableName, LicenceCompanySchema.Constants.TableName,                // 2, 3
							LicenceDatabaseSchema.Constants.TableName, ReleaseBuildSchema.Constants.TableName,                // 4, 5
							LicenceCompanySchema.Constants.LC_OH, LicenceCompanySchema.Constants.PK,                          // 6, 7
							LicenceHeaderSchema.Constants.LA_LC, LicenceDatabaseSchema.Constants.PK,                          // 8, 9
							LicenceHeaderSchema.Constants.LA_LD, LicenceDatabaseSchema.Constants.LD_HL_CurrentRunningVersion, // 10, 11
							ReleaseBuildSchema.Constants.PK, OrgHeaderSchema.Constants.OH_Code,                               // 12, 13
							StmALogSchema.Constants.SL_Reference, StmALogSchema.Constants.TableName,                          // 14, 15
							StmALogSchema.Constants.SL_SE_NKEvent, StmALogSchema.Constants.SL_Table,                          // 16, 17
							StmALogSchema.Constants.SL_Parent, ReleaseBuildSchema.Constants.HL_ExeVersionDate);               // 18, 19

						ZSqlParameterCollection parameters = new ZSqlParameterCollection();
						parameters.Add("@SL_SE_NKEvent", Events.Delivered.Code, StmALogSchema.SL_SE_NKEvent);
						parameters.Add("@CancelledEvent", Events.Cancelled.Code, StmALogSchema.SL_SE_NKEvent);
						parameters.Add("@HL_PK", PK, ReleaseBuildSchema.PK);
						parameters.Add("@HL_ExeVersionDate", HL_ExeVersionDate, ReleaseBuildSchema.HL_ExeVersionDate);

						query.AddFilterAndZSQLParameterCollection(sqlText, parameters);
						fClientsSentToButNotApplied.Load();
					}
				}

				return fClientsSentToButNotApplied;
			}
		}

		EDIOrgHeaderCollection fClientsSentToButNotApplied;

		#endregion

		public ReleaseBuild PreviousReleaseBuild
		{
			get
			{
				if (previousReleaseBuild == null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.LessThan, HL_MajorVersion);
					ZQuery subQuery = new ZQuery();
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.Equal, HL_MajorVersion);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, SQLComparisonOperator.LessThan, HL_MinorVersion);
					query.AddToFilter(subQuery, JoinCondition.Or);
					subQuery = new ZQuery();
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.Equal, HL_MajorVersion);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, SQLComparisonOperator.Equal, HL_MinorVersion);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_Release, SQLComparisonOperator.LessThan, HL_Release);
					query.AddToFilter(subQuery, JoinCondition.Or);
					subQuery = new ZQuery();
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.Equal, HL_MajorVersion);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, SQLComparisonOperator.Equal, HL_MinorVersion);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_Release, SQLComparisonOperator.Equal, HL_Release);
					subQuery.AddToFilter(ReleaseBuildSchema.HL_Patch, SQLComparisonOperator.LessThan, HL_Patch);
					query.AddToFilter(subQuery, JoinCondition.Or);
					query.OrderBy = ReleaseBuildSchema.HL_MajorVersion.Name + " DESC," +
									ReleaseBuildSchema.HL_MinorVersion.Name + " DESC," +
									ReleaseBuildSchema.HL_Release.Name + " DESC," +
									ReleaseBuildSchema.HL_Patch.Name + " DESC";
					previousReleaseBuild = Factory.LoadTop1<ReleaseBuild>(query);
				}
				return previousReleaseBuild;
			}
		}
		ReleaseBuild previousReleaseBuild;

		#endregion

		#region Package File

		void DeletePackage()
		{
			if (File.Exists(HL_PackagePath))
			{
				File.Delete(HL_PackagePath);
			}
		}

		#endregion

		#region Properties

		public VersionNumber VersionNumber
		{
			get { return new VersionNumber(HL_MajorVersion, HL_MinorVersion, HL_Release, HL_Patch); }
			set
			{
				HL_MajorVersion = value.Major;
				HL_MinorVersion = value.Minor;
				HL_Release = value.Release;
				HL_Patch = value.Patch;
			}
		}

#if DEBUG
		virtual
#endif
 public string PackageName
		{
			get { return GetPackageName(VersionNumber); }
		}

		public static string GetPackageName(VersionNumber versionNumber)
		{
			return StmUpgrade.EDPFileStartTag
				+ versionNumber.GetReleaseDate().ToString("yyyyMMdd_HHmm00", CultureInfo.CurrentCulture)
				+ "_" + versionNumber.Major.ToString(CultureInfo.InvariantCulture)
				+ "_" + versionNumber.Minor.ToString(CultureInfo.InvariantCulture)
				+ "_" + versionNumber.Release.ToString(CultureInfo.InvariantCulture)
				+ "_" + versionNumber.Patch.ToString(CultureInfo.InvariantCulture)
				+ StmUpgrade.EDPFileExtension;
		}

		static string GetTempAsserblyFolder(string version)
		{
			string tempPath = Env.TempPath;

			if (tempPath[tempPath.Length - 1] != '\\')
			{
				tempPath += "\\";
			}

			return tempPath + "CWO_Assembly_" + version + "\\";
		}

		public static void DeleteTempAssemblyFolder(string version)
		{
			string dir = GetTempAsserblyFolder(version);

			if (Directory.Exists(dir))
			{
				Directory.Delete(GetTempAsserblyFolder(version), true);
			}
		}

		public static void GetAssemblyAndPdbFiles(string version, List<string> fileList)
		{
			VersionNumber versionNumber = new VersionNumber(version);
			string versionPath = GetTempAsserblyFolder(version);
			Directory.CreateDirectory(versionPath);
			string edpFilePath = versionPath + GetPackageName(versionNumber);
			string extractPath = versionPath + "Distribution\\Application\\";

			ZQuery query = new ZQuery();
			query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.Equal, versionNumber.Major);
			query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, SQLComparisonOperator.Equal, versionNumber.Minor);
			query.AddToFilter(ReleaseBuildSchema.HL_Release, SQLComparisonOperator.Equal, versionNumber.Release);
			query.AddToFilter(ReleaseBuildSchema.HL_Patch, SQLComparisonOperator.Equal, versionNumber.Patch);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ReleaseBuild releaseBuild = factory.LoadTop1<ReleaseBuild>(query) ?? throw new ReleaseBuildException(string.Format(CultureInfo.CurrentCulture, "Software release build ver {0} not found in database", version));

			if (string.IsNullOrEmpty(releaseBuild.HL_PackagePath))
			{
				throw new ReleaseBuildException($"Software release build ver {version} does not have a valid Package Path.");
			}

			if (!releaseBuild.HL_IsActive)
			{
				throw new ReleaseBuildException($"Software release build version {version} has not been retained.");
			}

			File.Copy(releaseBuild.HL_PackagePath, edpFilePath);
			EdpFile.Unpack(edpFilePath, versionPath, null, createFilter(fileList));
			try
			{
				var files = Directory.GetFiles(extractPath);
				foreach (string file in files)
				{
					if (file.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
					{
						return;
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				throw new ReleaseBuildException("Could not find pdb files due to it's external library");
			}

			throw new ReleaseBuildException(string.Format(CultureInfo.CurrentCulture, "Could not find pdb files in software release build ver {0}", version));
		}

		static string createFilter(List<string> files)
		{
			var escapedFilter =
				string
				.Join(
					"|",
					files
					.Select(
						file => file.Replace(".", "\\.")
						)
					);
			return $".*/({escapedFilter})$";
		}

		public static string GetAssemblyFilePath(string version, string dllExeName)
		{
			string versionPath = GetTempAsserblyFolder(version);
			string extractPath = versionPath + "Distribution\\Application\\";
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dllExeName);
			string assemblyFilePath = extractPath + dllExeName;
			string pdbFilePath = extractPath + fileNameWithoutExtension + ".pdb";

			if (File.Exists(assemblyFilePath) && File.Exists(pdbFilePath))
			{
				return assemblyFilePath;
			}

			return "";
		}

		#region Product

		[List("Lookups.ProductTypeList")]
		public override ZString HL_Product
		{
			get
			{
				return base.HL_Product;
			}
			set
			{
				base.HL_Product = value;
			}
		}

		public bool IsCargoWiseProduct => HL_Product.EqualsIgnoringCase(ProductTypes.Codes.Enterprise) || HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWise) || HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext);

		public bool HL_Product_ReadOnly
		{
			get { return IsCargoWiseProduct; }
		}

		public ZString ProductDescription
		{
			get
			{
				if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.Enterprise))
				{
					return HL_MajorVersion < 14 ? ProductTypes.Descriptions.Enterprise : ProductTypes.Descriptions.CargoWiseOne;
				}
				else
				{
					return Lookups.ProductTypeList.GetDescriptionFromCode(HL_Product);
				}
			}
		}

		public bool IsCompatibleProduct(ZString productType)
		{
			if (HL_Product.EqualsIgnoringCase(productType))
			{
				return true;
			}
			else if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWise) &&
				IsCompatibleWithCargoWiseReleaseBuild(productType))
			{
				return true;
			}
			else if (IsCargoWiseProduct &&
				(productType.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseOne)
				|| productType.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext)
				|| productType.EqualsIgnoringCase(ProductTypes.Codes.ProductivityWise)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool IsUpgradeableProduct(ZString productType)
		{
			if (HL_Product.EqualsIgnoringCase(productType))
			{
				return true;
			}
			else if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWise) &&
				IsCompatibleWithCargoWiseReleaseBuild(productType))
			{
				return true;
			}
			else if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.Enterprise) &&
				(productType.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseOne)
				|| productType.EqualsIgnoringCase(ProductTypes.Codes.ProductivityWise)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		bool IsCompatibleWithCargoWiseReleaseBuild(ZString productType)
		{
			return productType.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseOne)
					|| productType.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext)
					|| productType.EqualsIgnoringCase(ProductTypes.Codes.Enterprise)
					|| productType.EqualsIgnoringCase(ProductTypes.Codes.ProductivityWise);
		}

		#endregion

		#region Release Date

		public ZDateTime ReleaseDate
		{
			get
			{
				if (IsCargoWiseProduct)
				{
					return VersionNumber.GetReleaseDate();
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo ReleaseDateInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseDate)); }
		}

		#endregion

		#region Full Display Text

		public ZString FullDisplayText
		{
			get
			{
				if (VersionNumber.IsEmpty)
				{
					return string.Empty;
				}
				else if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext))
				{
					return $"{ProductDescription} - {VersionNumberDisplayText} - {ExeVersion} - {HL_ExeVersionDate.ToLongTimeString()}";
				}
				else
				{
					return $"{ProductDescription} - {ReleaseDisplayText} - {ExeVersion} - {HL_ExeVersionDate.ToLongTimeString()}";
				}
			}
		}

		public ZPropertyInfo FullDisplayTextInfo
		{
			get { return GetZPropertyInfo(nameof(FullDisplayText)); }
		}

		#endregion

		#region Short Display Text

		public ZString ShortDisplayText
		{
			get
			{
				if (VersionNumber.IsEmpty)
				{
					return string.Empty;
				}
				else if (HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext))
				{
					return $"{ProductDescription} - {VersionNumber.GetReleaseDate().ToShortDateString()} {ExeVersion} - {HL_ExeVersionDate.ToLongTimeString()}";
				}
				else
				{
					return $"{ProductDescription} - {HL_ReleaseStatus} {VersionNumber.GetReleaseDate().ToShortDateString()} {ExeVersion} - {HL_ExeVersionDate.ToLongTimeString()}";
				}
			}
		}

		public ZPropertyInfo ShortDisplayTextInfo
		{
			get { return GetZPropertyInfo(nameof(ShortDisplayText)); }
		}

		#endregion

		#region Release Client Count

		public ZInt ReleaseClientCount
		{
			get
			{
				if (!releaseClientCount.HasValue)
				{
					releaseClientCount = HL_Superceded ? 0 : ReleaseLicences.Count;
				}
				return releaseClientCount.Value;
			}
		}

		public ZPropertyInfo ReleaseClientCountInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseClientCount)); }
		}

		ZInt? releaseClientCount;

		#endregion

		#region Patch Client Count

		public ZInt PatchClientCount
		{
			get
			{
				if (!patchClientCount.HasValue)
				{
					bool calculatedFromParentCollection = false;
					if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
					{
						ReleaseBuildCollection parent = ((IBusinessObjectInternals)this).ParentCollections[0] as ReleaseBuildCollection;
						if (parent != null)
						{
							calculatedFromParentCollection = true;
							patchClientCount = parent.GetLicenceHeaderCount(this);
						}
					}
					if (!calculatedFromParentCollection)
					{
						patchClientCount = PatchLicences.Count;
					}
				}

				return patchClientCount.Value;
			}
		}

		public ZPropertyInfo PatchClientCountInfo
		{
			get { return GetZPropertyInfo(nameof(PatchClientCount)); }
		}

		ZInt? patchClientCount;

		#endregion

		#region Exe Version

		//User is forced to follow Format "0.0.0.0", thus ExeVersion can never equal ZString.Empty
		[BusinessObjectTestExclude]
		[MaxLength(50)]
		public ZString ExeVersion
		{
			get
			{
				return exeVersion.IsEmpty
					? new ZString(HL_MajorVersion + "." + HL_MinorVersion + "." + HL_Release + "." + HL_Patch)
					: exeVersion;
			}
			set
			{
				SetNonPersistentPropertyValue(ExeVersionInfo, ref exeVersion, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExeVersion();
				}
				if (!ExeVersionInfo.HasErrors())
				{
					var versionNumber = GetVersionNumberFromText(exeVersion);
					HL_MajorVersion = versionNumber.Major;
					HL_MinorVersion = versionNumber.Minor;
					HL_Release = versionNumber.Release;
					HL_Patch = versionNumber.Patch;
					exeVersion = ZString.Empty;
				}
				else
				{
					HL_MajorVersion = 0;
					HL_MinorVersion = 0;
					HL_Release = 0;
					HL_Patch = 0;
				}
			}
		}
		ZString exeVersion;

		public bool ExeVersion_ReadOnly
		{
			get { return IsCargoWiseProduct; }
		}

		public ZPropertyInfo ExeVersionInfo
		{
			get { return GetZPropertyInfo(nameof(ExeVersion)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		internal static VersionNumber GetVersionNumberFromText(ZString exeVersion)
		{
			ZInt majorVersion = 0;
			ZInt minorVersion = 0;
			ZInt release = 0;
			ZInt patch = 0;
			ZString[] exeVersionArray = exeVersion.Split('.');
			if (exeVersionArray.Length > 3)
			{
				ZInt.TryParse(exeVersionArray[0], out majorVersion);
				ZInt.TryParse(exeVersionArray[1], out minorVersion);
				ZInt.TryParse(exeVersionArray[2], out release);
				ZInt.TryParse(exeVersionArray[3], out patch);
			}

			return new VersionNumber(majorVersion, minorVersion, release, patch);
		}

		#endregion

		#region Release Display Text

		public ZString ReleaseDisplayText
		{
			get
			{
				if (IsCargoWiseProduct && releaseDisplayText.IsEmpty)
				{
					releaseDisplayText = ReleaseInfo.GetReleaseDisplayText(HL_ReleaseStatus, VersionNumber);
				}
				return releaseDisplayText;
			}
		}

		public ZPropertyInfo ReleaseDisplayTextInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseDisplayText); }
		}

		ZString releaseDisplayText;

		public ZString VersionNumberDisplayText => ReleaseInfo.GetReleaseDisplayText(string.Empty, VersionNumber);

		#endregion

		#region HL_MajorVersion

		public bool HL_MajorVersion_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region HL_MinorVersion

		public bool HL_MinorVersion_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region HL_Release

		public bool HL_Release_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region HL_Patch

		public bool HL_Patch_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region HL_ExeVersionDate

		public override ZDateTime HL_ExeVersionDate
		{
			get
			{
				return base.HL_ExeVersionDate;
			}
			set
			{
				if (!value.IsEmpty && value.IsValid)
				{
					base.HL_ExeVersionDate = new ZDateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
				}
				else
				{
					base.HL_ExeVersionDate = value;
				}
			}
		}

		public bool HL_ExeVersionDate_ReadOnly
		{
			get { return IsCargoWiseProduct; }
		}

		#endregion

		public bool HL_PackagePath_ReadOnly => true;

		protected override ZString HumanReadableNameCore => Res.GetString("91C52456-61EB-4DDA-90D7-8FEE82ABD65D", "Release Build");

		#endregion

		#region Client Specific Code

		public bool IsSpecificForClient(string enterpriseCode)
		{
			return ClientSpecificCodes.Contains(enterpriseCode.ToUpperInvariant());
		}

		public ReadOnlyCollection<string> ClientSpecificCodes
		{
			get
			{
				if (clientSpecificCodes == null)
				{
					if (!ClientSpecificCodesCache.TryGetValue(PK, out clientSpecificCodes))
					{
						ClientSpecificCodesCache[PK] = clientSpecificCodes = new ReadOnlyCollection<string>(CalculateClientSpecificCodes());
					}
				}
				return clientSpecificCodes;
			}
		}

#if DEBUG
		public void CallCalculateClientSpecificCodesForTest()
		{
			CalculateClientSpecificCodes();
		}

		protected virtual
#endif
 ReadOnlyCollection<string> CalculateClientSpecificCodes()
		{
			if (!HL_IsActive)
			{
				throw new InvalidOperationException("Attempted to get client specific codes from a deleted release build");
			}

			var result = new List<string>();
			var zClientWebFilePattern = @"Distribution/Application/ZClientWeb".ToUpperInvariant();
			var zClientFilePattern = @"Distribution/Application/ZClient".ToUpperInvariant();
			var documentsXmlFilePattern = "Documents.xml".ToUpperInvariant();

			for (int retries = 0; retries < 3; retries++)
			{
				try
				{
					GetCodesFromPackage(result, zClientWebFilePattern, zClientFilePattern, documentsXmlFilePattern);
					break;
				}
				catch (IOException ex) when (!ex.IsCriticalException())
				{
					if (ex.Message.Contains("The network path was not found.") && retries < 2)
					{
						Thread.Sleep(1000);
					}
					else
					{
						throw;
					}
				}
			}

			return result.AsReadOnly();
		}

		protected virtual void GetCodesFromPackage(List<string> result, string zClientWebFilePattern, string zClientFilePattern, string documentsXmlFilePattern)
		{
			using (var zip = new ZipArchive(File.OpenRead(HL_PackagePath), ZipArchiveMode.Read, false))
			{
				foreach (var entry in zip.Entries)
				{
					string entryFileName = entry.FullName.ToUpperInvariant();

					string code = null;
					if (entryFileName.StartsWith(zClientWebFilePattern, StringComparison.Ordinal) && entryFileName.Length == (zClientWebFilePattern.Length + 7))
					{
						code = entryFileName.Substring(zClientWebFilePattern.Length, 3);
					}
					else if (entryFileName.StartsWith(zClientFilePattern, StringComparison.Ordinal) && entryFileName.Length == (zClientFilePattern.Length + 7))
					{
						code = entryFileName.Substring(zClientFilePattern.Length, 3);
					}
					else if (entryFileName.EndsWith(documentsXmlFilePattern, StringComparison.Ordinal))
					{
						code = entryFileName.Substring(entryFileName.IndexOf(documentsXmlFilePattern, StringComparison.Ordinal) - 3, 3);
					}

					if (code != null && !result.Contains(code))
					{
						result.Add(code);
					}
				}
			}
		}

		static Dictionary<ZGuid, ReadOnlyCollection<string>> ClientSpecificCodesCache
		{
			get
			{
				if (clientSpecificCodesCache == null)
				{
					clientSpecificCodesCache = new Dictionary<ZGuid, ReadOnlyCollection<string>>();
				}
				return clientSpecificCodesCache;
			}
		}

		ReadOnlyCollection<string> clientSpecificCodes;

		[ThreadStatic]
		static Dictionary<ZGuid, ReadOnlyCollection<string>> clientSpecificCodesCache;

		#endregion

#if DEBUG
		public static ReleaseBuild NewForTesting(BusinessObjectFactory factory, string releaseStatus, bool superseded)
		{
			ReleaseBuild result = factory.New<ReleaseBuild>();
			result.HL_ReleaseStatus = releaseStatus;
			result.HL_Superceded = superseded;
			return result;
		}

		public static void SetClientSpecificCodesForTesting(ZGuid buildPk, params string[] clientCodes)
		{
			ClientSpecificCodesCache[buildPk] = new ReadOnlyCollection<string>(clientCodes);
		}
#endif
	}
}

