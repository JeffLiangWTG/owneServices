using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	/// <summary>
	/// Provides Security Logic for Enterprise.
	/// All checkpoints are created in Enterprise.Core.Security.
	/// </summary>
	class ZSecurity : SecurityCalculator, IZSecurity, IDisposable
	{
		#region Setup / Initialisation

		internal ZSecurity(GlbSecurityCollection securityData, GlbStaff staff, ZGuid branchPK, ZGuid departmentPK, ZGuid companyPK, bool reloadStaffOrGroupObject = true)
		{
			SetupSecurity(securityData, staff, branchPK, departmentPK, companyPK, reloadStaffOrGroupObject);
		}

		internal ZSecurity(GlbSecurityCollection securityData, GlbGroup group, ZGuid branchPK, ZGuid departmentPK, ZGuid companyPK, bool reloadStaffOrGroupObject = true)
		{
			SetupSecurity(securityData, group, branchPK, departmentPK, companyPK, reloadStaffOrGroupObject);
		}

		void SetupSecurity(GlbSecurityCollection securityData, GlbStaff staff, ZGuid securityBranchPK, ZGuid securityDepartmentPK, ZGuid securityCompanyPK, bool reloadStaffOrGroupObject = true)
		{
			SetupSecurity(securityData, securityBranchPK, securityDepartmentPK, securityCompanyPK);

			if (staff == null)
			{
				staffMemberPK = ZGuid.Empty;
			}
			else if (reloadStaffOrGroupObject)
			{
				staffMemberPK = staff.PK;
			}
			else
			{
				staffMember = staff;
			}
		}

		void SetupSecurity(GlbSecurityCollection securityData, GlbGroup group, ZGuid securityBranchPK, ZGuid securityDepartmentPK, ZGuid securityCompanyPK, bool reloadStaffOrGroupObject = true)
		{
			SetupSecurity(securityData, securityBranchPK, securityDepartmentPK, securityCompanyPK);

			if (group == null)
			{
				groupPK = ZGuid.Empty;
			}
			else if (reloadStaffOrGroupObject)
			{
				groupPK = group.PK;
			}
			else
			{
				groupMember = group;
			}
		}

		void SetupSecurity(GlbSecurityCollection securityData, ZGuid branchPK, ZGuid departmentPK, ZGuid companyPK)
		{
			if (ThreadSentry == null || !ThreadSentry.IsOwner)
			{
				ThreadSentry = ThreadSentryProvider.GetThreadSentry(true, new ZSecurityThreadSentryHelper(), allowChangingThreadOwnership: false);
			}

			this.securityData = securityData;

			this.branchPK = branchPK;
			this.departmentPK = departmentPK;
			this.companyPK = companyPK;
		}

		void InitialiseSecurityForGroup(GlbGroup group)
		{
			securityData = new GlbSecurityCollection(Factory);
			var filter = new ZQuery(GlbSecuritySchema.GU_GG, group.PK);
			securityData.Load(filter);
		}

		void InitialiseSecurityForStaff(GlbStaff staff)
		{
			securityData = new GlbSecurityCollection(Factory);
			if (staff != null)
			{
				var filter = new ZQuery(GlbSecuritySchema.GU_GS, staffMember.PK);
				filter.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, staffMember.ActiveGroups.Cast<GlbGroup>().Where(group => !group.IsDeleted).Select(group => group.PK));
				securityData.Load(filter);
			}
		}

		GlbSecurityCollection securityData
		{
			get
			{
				EnsureSecurityData();
				return fSecurityData;
			}
			set
			{
				fSecurityData?.RemoveAll();
				fSecurityData = value;
			}
		}
		GlbSecurityCollection fSecurityData;

		public IZGlbSecurityCollection GlbSecurityCollection => securityData;

		void EnsureSecurityData()
		{
			if (fSecurityData == null)
			{
				if (Group != null)
				{
					InitialiseSecurityForGroup(Group);
				}
				else
				{
					InitialiseSecurityForStaff(StaffMember);
				}
			}
		}

		public IDisposable SuspendLoadProviders() => new DisposableAction(() => suspendLoadProviders = true, () => suspendLoadProviders = false);
		bool suspendLoadProviders;

		public void PrepareForSearching()
		{
			// We load all GlbGroupLink and all GlbGroup because otherwise many thousands of queries will request specific rows (much slower)
			var allGlbGroupLink = Factory.Load<GlbGroupLink>(new ZQuery());
			var allGlbGroup = Factory.Load<GlbGroup>(new ZQuery());
		}

		readonly ThreadLocal<BusinessObjectFactory> ThreadLocalFactory = new ThreadLocal<BusinessObjectFactory>(GetNewFactory);

		static ReadOnlyBusinessObjectFactory GetNewFactory()
		{
			var factory = new ReadOnlyBusinessObjectFactory(allowChangingThreadOwnership: false);
			factory.NameForDebugging = "ZSecurity"; // SupressCodeSmell Reason = it is the factory name.
			if (!Globals.IsUserInteractive)
			{
				factory.RefreshEnabled = false;
			}
			return factory;
		}

		BusinessObjectFactory Factory { get { return ThreadLocalFactory.Value; } }

#if DEBUG
		internal BusinessObjectFactory Factory_Exposed { get { return Factory; } }
#endif

		public GlbStaff StaffMember
		{
			get
			{
				staffMember = staffMember ?? Factory.Load<GlbStaff>(staffMemberPK);
				return staffMember;
			}
			set
			{
				staffMember = value;
			}
		}
		GlbStaff staffMember;

		ZGuid staffMemberPK = ZGuid.Empty;

		public GlbGroup Group
		{
			get
			{
				groupMember = groupMember ?? Factory.Load<GlbGroup>(groupPK);
				return groupMember;
			}
		}
		GlbGroup groupMember;

		ZGuid groupPK = ZGuid.Empty;

		#endregion

		#region Properties

		#region Caching Enabled

		public bool CachingEnabled
		{
			get { return fCachingEnabled; }
			set { fCachingEnabled = value; }
		}

		bool fCachingEnabled = true;

		#endregion

		#region Company PK

		internal bool allowEmptyCompanyPK;

		public ZGuid CompanyPK
		{
			get
			{
				if (companyPK.IsEmpty && Env.CurrentCompany != null && !allowEmptyCompanyPK)
				{
					companyPK = Env.CurrentCompany.PK;
				}
				return companyPK;
			}
			set
			{
				if (fIsLocked)
				{
					throw new InvalidOperationException("CompanyPK cannot be changed while Security instance is Locked");
				}

				companyPK = value;
			}
		}

		ZGuid companyPK = ZGuid.Empty;

		#endregion

		#region DepartmentPK

		internal bool allowEmptyDepartmentPK;

		public ZGuid DepartmentPK
		{
			get
			{
				if (departmentPK.IsEmpty && Env.CurrentDepartment != null && !allowEmptyDepartmentPK)
				{
					departmentPK = Env.CurrentDepartment.PK;
				}
				return departmentPK;
			}
			set
			{
				if (fIsLocked)
				{
					throw new InvalidOperationException("DepartmentPK cannot be changed while Security instance is Locked");
				}
				departmentPK = value;
			}
		}

		ZGuid departmentPK = ZGuid.Empty;

		#endregion

		#region Branch PK

		internal bool allowEmptyBranchPK;

		public ZGuid BranchPK
		{
			get
			{
				if (branchPK.IsEmpty && Env.CurrentBranch != null && !allowEmptyBranchPK)
				{
					branchPK = Env.CurrentBranch.PK;
				}
				return branchPK;
			}
			set
			{
				if (fIsLocked)
				{
					throw new InvalidOperationException("BranchPK cannot be changed while Security instance is Locked");
				}
				branchPK = value;
			}
		}

		ZGuid branchPK = ZGuid.Empty;

		#endregion

		#region User PK

		public ZGuid UserPK
		{
			get { return StaffMember != null ? StaffMember.PK : ZGuid.Empty; }
			set { staffMember = Factory.Load<GlbStaff>(value); }
		}

		#endregion

		#region Group PK

		public ZGuid GroupPK
		{
			get { return groupMember != null ? groupMember.PK : ZGuid.Empty; }
			set { groupMember = Factory.Load<GlbGroup>(value); }
		}

		#endregion

		#endregion

		#region Registry Security

		internal const string RegistrySecurityPrefix = "RegASS";
		internal static string GetRegistrySecurityCheckPointCode(string identifier)
		{
			var maxLength = GlbSecuritySchema.GU_SecurityRight.MaxLength - RegistrySecurityPrefix.Length;
			if (identifier.Length > maxLength)
			{
				identifier = identifier.Substring(identifier.Length - maxLength, maxLength);
			}
			var result = RegistrySecurityPrefix + identifier;
			return result;
		}

		internal const string RegistryCategorySecurityPrefix = "RegCat";
		internal static string GetRegistryCategorySecurityCheckPointCode(string path)
		{
			var maxLength = GlbSecuritySchema.GU_SecurityRight.MaxLength - RegistryCategorySecurityPrefix.Length;
			if (path.Length > maxLength)
			{
				path = path.Substring(path.Length - maxLength, maxLength);
			}
			var result = RegistryCategorySecurityPrefix + path;
			return result;
		}

		public SecurityCheckpoint GetRegistryCheckPoint(string identifier, string displayName)
		{
			LoadRegistrySecurityCheckPoints();

			var registryCheckPoint = FindCheckPoint(GetRegistrySecurityCheckPointCode(identifier));

			if (registryCheckPoint != null)
			{
				return registryCheckPoint;
			}

			var registryEditSpecificSettings = FindCheckPoint(Env.Security.SystemRegistryEditSpecificSettings.Code);
			return new SecurityCheckpointNonOperationalAllowed(GetRegistrySecurityCheckPointCode(identifier), (NoResString)displayName, registryEditSpecificSettings, this);
		}

		public SecurityCheckpoint[] GetAllRegistryCheckPoint()
		{
			LoadRegistrySecurityCheckPoints();

			var registryEditSpecificSettings = FindCheckPoint(Env.Security.SystemRegistryEditSpecificSettings.Code);
			return registryEditSpecificSettings.SelectRecursive(x => x.ChildCheckPoints).ToArray();
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void LoadRegistrySecurityCheckPoints()
		{
			if (!registryAdded)
			{
				registryAdded = true;
				var registryItems = ObjectFactory.New<IRegistryItemSetLocator>().GetAllRegistryItems();

				var registryEditSpecificSettings = FindCheckPoint(Env.Security.SystemRegistryEditSpecificSettings.Code);

				foreach (var registryItem in registryItems)
				{
					try
					{
						if ((registryItem.Options &
						 (Integration.RegistryOptions.IsHidden
						 | Integration.RegistryOptions.IsOnlyForCargoWise
						 | Integration.RegistryOptions.IsOnlyForController
						 | Integration.RegistryOptions.IsOnlyForDevelopers
						 | Integration.RegistryOptions.IsReadOnly)) > 0)
						{
							continue;
						}

						if (registryItem.Options == Integration.RegistryOptions.IsOnlyForSupport && Env.Registry.RawRegistry.SystemEnterpriseCode.Value != "EDI")
						{
							// Everybody is a support user in ediProd.
							// We need to display support registry in ediProd then we could set security to specific user.
							continue;
						}
						if (registryItem.Options.HasFlag(Integration.RegistryOptions.IsOnlyEditableBySupportIfHosted)
							&& EnvProxy.IsHostedWithCargowise)
						{
							continue;
						}
					}
					catch (NotImplementedException) { } // LinkRegistryItem doesn't support Options.

					var code = GetRegistrySecurityCheckPointCode(registryItem.Name);
					if (FindCheckPoint(code) == null)
					{
						//A security checkpoint cannot be in more than one place, so arbitrarily pick the first category it is in and put it there.
						var categoryNode = GetOrCreateNodePath(registryEditSpecificSettings, registryItem.Categories[0], registryItem.CategoriesUntranslated[0]);
						new SecurityCheckpointNonOperationalAllowed(code, ((IMultilingualRegistryItem)registryItem).CaptionMultilingual, categoryNode, this);
					}
				}

				registryEditSpecificSettings.SortAlphabeticallyRecursively(true);
			}
		}
		bool registryAdded;

		SecurityCheckpoint GetOrCreateNodePath(SecurityCheckpoint baseCheckpoint, string category, string categoryUntranslated)
		{
			var categorySplit = category.Replace(@"\/", "&#92;").Split('/').Select(x => x.Replace("&#92;", "/")).ToArray();
			var categoryUntranslatedSplit = categoryUntranslated.Replace(@"\/", "&#92;").Replace(" ", "").Split('/').Select(x => x.Replace("&#92;", "/")).ToArray();
			for (var i = 0; i < categorySplit.Length; ++i)
			{
				var categoryCode = GetRegistryCategorySecurityCheckPointCode(categoryUntranslatedSplit.Take(i + 1).Aggregate((x, y) => x + "/" + y));
				var categoryName = categorySplit[i].Trim();
				var nextCheckpoint = FindCheckPoint(categoryCode) ?? new SecurityCheckpointNonOperationalAllowed(categoryCode, (NoResString)categoryName, baseCheckpoint, this);

				baseCheckpoint = nextCheckpoint;
			}

			return baseCheckpoint;
		}

		#endregion

		#region PrintQueue Security

		public SecurityCheckpoint GetPrintQueueCheckPoint(Guid printQueuePK, string displayName)
		{
			LoadPrintQueueSecurityCheckPoints();

			var printQueueCheckPoint = FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueuePK));

			if (printQueueCheckPoint != null)
			{
				return printQueueCheckPoint;
			}

			var printQueuesPrintTo = FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));
			return new SecurityCheckpointNonOperationalAllowed(StmPrintQueueSchema.Constants.TableName, (NoResString)displayName, printQueuesPrintTo, this, printQueuePK);
		}

		public SecurityCheckpoint[] GetAllPrintQueueCheckPoint()
		{
			LoadPrintQueueSecurityCheckPoints();

			return GetActivePrintQueues().Select(printQueue => GetPrintQueueCheckPoint(printQueue.PK.ToGuid(), printQueue.DisplayName)).ToArray();
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void LoadPrintQueueSecurityCheckPoints()
		{
			if (!printQueuesAdded)
			{
				printQueuesAdded = true;
				var printQueuesPrintTo = FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));

				var groupedPrintQueue = GetActivePrintQueues().GroupBy(printQueue => (printQueue.ServerPK, printQueue.ServerName));

				foreach (var serverNameGroup in groupedPrintQueue)
				{
					var serverSecurityCheckpoint = new SecurityCheckpointNonOperationalAllowed(StmPrintServerSchema.Constants.TableName, (NoResString)serverNameGroup.Key.ServerName, printQueuesPrintTo, this, serverNameGroup.Key.ServerPK.ToGuid());

					foreach (var printQueue in serverNameGroup)
					{
						new SecurityCheckpointNonOperationalAllowed(StmPrintQueueSchema.Constants.TableName, (NoResString)printQueue.DisplayName, serverSecurityCheckpoint, this, printQueue.PK.ToGuid());
					}
				}
			}
		}
		bool printQueuesAdded;

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void ReloadPrintCheckpoints()
		{
			printQueuesAdded = true;
			var printQueuesPrintTo = FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));

			foreach (var printQueue in GetActivePrintQueues())
			{
				var printQueueCheckPoint = FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue.PK.ToGuid()));

				if (printQueueCheckPoint == null)
				{
					var serverSecurityCheckpoint = (SecurityCheckpointNonOperationalAllowed)printQueuesPrintTo.ChildCheckPoints.FirstOrDefault(p => p.ItemGuid == printQueue.ServerPK) ?? new SecurityCheckpointNonOperationalAllowed(StmPrintServerSchema.Constants.TableName, (NoResString)printQueue.ServerName, printQueuesPrintTo, this, printQueue.ServerPK.ToGuid());

					new SecurityCheckpointNonOperationalAllowed(StmPrintQueueSchema.Constants.TableName, (NoResString)printQueue.DisplayName, serverSecurityCheckpoint, this, printQueue.PK.ToGuid());
				}
			}
		}

		internal class PrintQueue
		{
			public PrintQueue(ZGuid pK, string displayName, ZGuid serverPK, string serverName)
			{
				this.PK = pK;
				this.DisplayName = displayName;
				this.ServerPK = serverPK;
				this.ServerName = serverName;
			}

			public readonly ZGuid PK;
			public readonly string DisplayName;
			public readonly ZGuid ServerPK;
			public readonly string ServerName;
		}

		IEnumerable<PrintQueue> GetActivePrintQueues()
		{
			var printQueues = new List<PrintQueue>();

			var printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmPrintQueueCollection>(), Factory);
			printQueueCollection.Load(new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True));
			printQueueCollection.Sort(new SortInfo(StmPrintQueueSchema.SQ_DisplayName.Name, ListSortDirection.Ascending));

			printQueueCollection.FetchStrategy.FetchForView(printQueueCollection.ToArray(), new[] { new TableColumn(StmPrintQueueSchema.Constants.TableName, StmPrintQueueSchema.Constants.SQ_SPS_Server) });

			var spsServer = ZGuid.Empty;
			var serverName = ZString.Empty;
			foreach (IStmPrintQueue bizO in printQueueCollection)
			{
				if (bizO.SQ_SPS_Server != spsServer)
				{
					spsServer = bizO.SQ_SPS_Server;
					serverName = bizO.SQ_ServerName;
				}
				printQueues.Add(new PrintQueue(bizO.PK, bizO.SQ_DisplayName, spsServer, serverName));
			}

			printQueues = printQueues.OrderBy(printQueue => printQueue.DisplayName).ToList();

			return printQueues;
		}

		#endregion

		#region Document Type Security

		static string GetDocumentTypeUploadSecurityCheckPointCode(string rT_DocType)
		{
			return GetDocumentTypeSecurityCheckPointCode(rT_DocType, "eDocsUploadDocumentType");
		}

		static string GetDocumentTypeViewSecurityCheckPointCode(string rT_DocType)
		{
			return GetDocumentTypeSecurityCheckPointCode(rT_DocType, "eDocsViewDocumentType");
		}

		static string GetDocumentTypeCutSecurityCheckPointCode(string rT_DocType)
		{
			return GetDocumentTypeSecurityCheckPointCode(rT_DocType, "eDocsCutDocumentType");
		}

		static string GetDocumentTypePermanentDeleteCheckPointCode(string rT_DocType)
		{
			return GetDocumentTypeSecurityCheckPointCode(rT_DocType, "eDocsPermanentDelete");
		}

		static string GetDocumentTypeSecurityCheckPointCode(string rT_DocType, string checkPointPrefix)
		{
			return new ZString(checkPointPrefix + rT_DocType).Left(100);
		}

		public SecurityCheckpoint GetDocumentTypeUploadCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeCheckPoint(rT_DocType, GetDocumentTypeUploadSecurityCheckPointCode(rT_DocType), Env.Security.eDocsUploadSpecificDocumentType.Code);
		}

		public SecurityCheckpoint GetDocumentTypeViewCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeCheckPoint(rT_DocType, GetDocumentTypeViewSecurityCheckPointCode(rT_DocType), Env.Security.ViewSpecificEDocTypes.Code);
		}

		public SecurityCheckpoint GetDocumentTypeCutCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeCheckPoint(rT_DocType, GetDocumentTypeCutSecurityCheckPointCode(rT_DocType), Env.Security.CutSpecificEDocTypes.Code);
		}

		public SecurityCheckpoint GetDocumentTypePermanentDeleteCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeCheckPoint(rT_DocType, GetDocumentTypePermanentDeleteCheckPointCode(rT_DocType), Env.Security.eDocsPermanentDelete.Code);
		}

		SecurityCheckpoint GetDocumentTypeCheckPoint(string rT_DocType, string docTypeCheckPointCode, string parentCheckPointCode)
		{
			LoadDocumentTypeSecurityCheckPoints();

			var documentTypeCheckPoint = FindCheckPoint(docTypeCheckPointCode);

			if (documentTypeCheckPoint != null)
			{
				return documentTypeCheckPoint;
			}

			var parentCheckPoint = FindCheckPoint(new CheckpointLookupKey(parentCheckPointCode));
			return new SecurityCheckpointNonOperationalAllowed(docTypeCheckPointCode, (NoResString)rT_DocType, parentCheckPoint, this);
		}

		public SecurityCheckpoint[] GetAllDocumentTypeUploadCheckPoint()
		{
			LoadDocumentTypeSecurityCheckPoints();

			return GetActiveDocumentTypes().Select(documentType => GetDocumentTypeUploadCheckPoint(documentType)).ToArray();
		}

		public SecurityCheckpoint[] GetAllDocumentTypeViewCheckPoint()
		{
			LoadDocumentTypeSecurityCheckPoints();

			return GetActiveDocumentTypes().Select(documentType => GetDocumentTypeViewCheckPoint(documentType)).ToArray();
		}

		public SecurityCheckpoint[] GetAllDocumentTypeCutCheckPoint()
		{
			LoadDocumentTypeSecurityCheckPoints();

			return GetActiveDocumentTypes().Select(documentType => GetDocumentTypeCutCheckPoint(documentType)).ToArray();
		}

		public SecurityCheckpoint[] GetAllDocumentTypePermanentDeleteCheckPoint()
		{
			LoadDocumentTypeSecurityCheckPoints();

			return GetActiveDocumentTypes().Select(documentType => GetDocumentTypePermanentDeleteCheckPoint(documentType)).ToArray();
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void ReloadDocumentCheckpoints()
		{
			documentTypesAdded = true;
			var eDocsUploadSpecificDocumentType = FindCheckPoint(new CheckpointLookupKey(Env.Security.eDocsUploadSpecificDocumentType.Code));
			var viewSpecificEDocTypes = FindCheckPoint(new CheckpointLookupKey(Env.Security.ViewSpecificEDocTypes.Code));
			var cutSpecificEDocTypes = FindCheckPoint(new CheckpointLookupKey(Env.Security.CutSpecificEDocTypes.Code));
			var deletePermanentlySpecificEDocTypes = FindCheckPoint(new CheckpointLookupKey(Env.Security.eDocsPermanentDelete.Code));

			foreach (string rT_DocType in GetActiveDocumentTypes().Distinct())
			{
				var documentTypeUploadCheckPoint = FindCheckPoint(GetDocumentTypeUploadSecurityCheckPointCode(rT_DocType));
				var documentTypeViewCheckPoint = FindCheckPoint(GetDocumentTypeViewSecurityCheckPointCode(rT_DocType));
				var documentTypeCutCheckPoint = FindCheckPoint(GetDocumentTypeCutSecurityCheckPointCode(rT_DocType));
				var documentTypePermaDeleteCheckpoint = FindCheckPoint(GetDocumentTypePermanentDeleteCheckPointCode(rT_DocType));

				if (documentTypeUploadCheckPoint == null)
				{
					new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeUploadSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, eDocsUploadSpecificDocumentType, this);
				}

				if (documentTypeViewCheckPoint == null)
				{
					new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeViewSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, viewSpecificEDocTypes, this);
				}

				if (documentTypeCutCheckPoint == null)
				{
					new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeCutSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, cutSpecificEDocTypes, this);
				}

				if (documentTypePermaDeleteCheckpoint == null)
				{
					new SecurityCheckpointNonOperationalAllowed(GetDocumentTypePermanentDeleteCheckPointCode(rT_DocType), (NoResString)rT_DocType, deletePermanentlySpecificEDocTypes, this);
				}
			}
		}

		readonly object lockObject = new object();

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void LoadDocumentTypeSecurityCheckPoints()
		{
			if (!documentTypesAdded)
			{
				lock (lockObject)
				{
					if (!documentTypesAdding)
					{
						documentTypesAdding = true;
						var eDocsUploadSpecificDocumentType = FindCheckPoint(new CheckpointLookupKey(Env.Security.eDocsUploadSpecificDocumentType.Code));
						var viewSpecificEDocTypes = FindCheckPoint(new CheckpointLookupKey(Env.Security.ViewSpecificEDocTypes.Code));
						var cutSpecificEDocTypes = FindCheckPoint(new CheckpointLookupKey(Env.Security.CutSpecificEDocTypes.Code));
						var eDocsPermaDeleteSpecificDocumentType = FindCheckPoint(new CheckpointLookupKey(Env.Security.eDocsPermanentDelete.Code));

						foreach (string rT_DocType in GetActiveDocumentTypes().Distinct())
						{
							new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeUploadSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, eDocsUploadSpecificDocumentType, this);
							new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeViewSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, viewSpecificEDocTypes, this);
							new SecurityCheckpointNonOperationalAllowed(GetDocumentTypeCutSecurityCheckPointCode(rT_DocType), (NoResString)rT_DocType, cutSpecificEDocTypes, this);
							new SecurityCheckpointNonOperationalAllowed(GetDocumentTypePermanentDeleteCheckPointCode(rT_DocType), (NoResString)rT_DocType, eDocsPermaDeleteSpecificDocumentType, this);
						}
						documentTypesAdded = true;
					}
				}
			}
		}
		bool documentTypesAdded, documentTypesAdding;

		IEnumerable<string> GetActiveDocumentTypes()
		{
			var rT_DocTypes = new List<string>();

			var filter = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);
			var docTypeCollection = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefDocTypeCollection>(), new object[] { Factory, filter });
			docTypeCollection.ApplySort(new SortInfo(RefDocTypeSchema.RT_DocType.Name, ListSortDirection.Ascending));

			foreach (var bizO in docTypeCollection.ToArray())
			{
				rT_DocTypes.Add((ZString)bizO[RefDocTypeSchema.RT_DocType.Name]);
			}

			return rT_DocTypes;
		}

		#endregion

		#region Rates Security

		void LoadRatesSecurityCheckPoints()
		{
			if (!isLoadingRatesSecurityCheckPoints)
			{
				isLoadingRatesSecurityCheckPoints = true;
				try
				{
					var ratesSecurity = FindCheckPoint(new CheckpointLookupKey(Env.Security.RatesSecurity.Code));
					var registryRatesSecurities = OrganisationsDataRegistry.Instance.RatesSecurity.Value;

					foreach (ICodeDescription registryRatesSecurity in registryRatesSecurities)
					{
						var code = Env.Security.RatesSecurity.Code + registryRatesSecurity.Code;
						FindOrCreateCheckPoint(ratesSecurity, code, (NoResString)registryRatesSecurity.Code, Guid.Empty);
					}
				}
				finally
				{
					isLoadingRatesSecurityCheckPoints = false;
				}
			}
		}
		bool isLoadingRatesSecurityCheckPoints;

		#endregion

		#region Lazy hook

		Func<CheckpointLookupKey, ISecurityCheckpoint> getLazyCheckpoint;
		Action initAllLazyCheckpoints;

		void IZSecurity.SetLazyCheckpointPlugin(Func<CheckpointLookupKey, ISecurityCheckpoint> getCheckpoint)
		{
			getLazyCheckpoint = getCheckpoint;
		}

		void IZSecurity.SetInitAllLazyCheckpoints(Action initAllLazyCheckpoints)
		{
			this.initAllLazyCheckpoints = initAllLazyCheckpoints;
		}

		#endregion

		#region JobInvoicing Security

		#region Public API

		public void AddJobInvoicingPlugIn(ISecurityCheckpoint checkpoint, CheckpointBuilderModel model) => InvoicingPluginProvider.AddJobInvoicingPlugIn(checkpoint, model);
		public void SetJobInvoicingPluginBuilder(Func<SecurityCheckpointBuilder> makeBuilder) => InvoicingPluginProvider.SetJobInvoicingPluginBuilder(makeBuilder);
		public void BuildJobInvoicingPluginCheckpoints(ISecurityCheckpoint parent) => InvoicingPluginProvider.BuildJobInvoicingPluginCheckpoints(parent);

		#endregion

		#region Implementation

		JobInvoicingPluginProvider invoicingPluginProvider;
		JobInvoicingPluginProvider InvoicingPluginProvider
		{
			get => invoicingPluginProvider ?? (invoicingPluginProvider = new JobInvoicingPluginProvider(this));
		}

		sealed class JobInvoicingPluginProvider
		{
			internal JobInvoicingPluginProvider(ZSecurity zSecurity)
			{
				this.zSecurity = zSecurity;
			}

			readonly ZSecurity zSecurity;
			readonly Dictionary<ISecurityCheckpoint, CheckpointBuilderModel> jobInvoicingPlugins = new Dictionary<ISecurityCheckpoint, CheckpointBuilderModel>();
			Func<SecurityCheckpointBuilder> jobInvoicingPluginBuilderProvider;
			SecurityCheckpointBuilder jobInvoicingPluginBuilder;

			internal void BuildAllPlugins()
			{
				if (jobInvoicingPluginBuilder == null)
				{
					jobInvoicingPluginBuilder = jobInvoicingPluginBuilderProvider?.Invoke();
				}
				if (jobInvoicingPluginBuilder != null)
				{
					foreach (var pair in jobInvoicingPlugins)
					{
						jobInvoicingPluginBuilder.BuildChildren(pair.Key, zSecurity, pair.Value, (code, text, p, s) => new SecurityCheckpoint(code, text, p, s));
					}
					jobInvoicingPlugins.Clear();
				}
			}

			internal SecurityCheckpoint FindJobInvoicingCheckPoint(CheckpointLookupKey key)
			{
				var pluginRoot = jobInvoicingPlugins.FirstOrDefault(p => key.Code.StartsWith(p.Key.Code, StringComparison.OrdinalIgnoreCase)).Key;
				if (pluginRoot == null)
				{
					return null;
				}
				else
				{
					BuildJobInvoicingPluginCheckpoints(pluginRoot);
					return zSecurity.FindCheckPoint(key); // Ugly recursion, but still safe right now.
				}
			}

			public void AddJobInvoicingPlugIn(ISecurityCheckpoint checkpoint, CheckpointBuilderModel model)
			{
				jobInvoicingPlugins.Add(checkpoint, model);
			}

			public void SetJobInvoicingPluginBuilder(Func<SecurityCheckpointBuilder> makeBuilder)
			{
				jobInvoicingPluginBuilderProvider = makeBuilder;
				jobInvoicingPluginBuilder = null;
			}

			public void BuildJobInvoicingPluginCheckpoints(ISecurityCheckpoint parent)
			{
				if (jobInvoicingPlugins.TryGetValue(parent, out var model))
				{
					if (jobInvoicingPluginBuilder == null)
					{
						jobInvoicingPluginBuilder = jobInvoicingPluginBuilderProvider();
					}
					jobInvoicingPluginBuilder.BuildChildren(parent, zSecurity, model, (code, displayText, p, s) => new SecurityCheckpoint(code, displayText, p, s));
					jobInvoicingPlugins.Remove(parent);
				}
			}
		}

		#endregion

		#endregion

		#region CheckPoints

		public SecurityCheckpoint FindCheckPoint(CheckpointLookupKey key, bool loadProviders = true)
		{
			if (Env.Security != null)
			{
				if (!providersLoaded && loadProviders && !suspendLoadProviders)
				{
					providersLoaded = true;
					((IEnumerable)ObjectFactory.Get("SecurityCheckpointProviders")).Cast<ISecurityCheckpointProvider>().ForEach(p => p.LoadCheckpoints(Factory, this));
					LoadDocumentTypeSecurityCheckPoints();
					LoadPrintQueueSecurityCheckPoints();
					LoadRatesSecurityCheckPoints();
				}
				if (key.Code.StartsWith(Env.Security.RatesSecurity.Code, StringComparison.Ordinal))
				{
					LoadRatesSecurityCheckPoints();
				}
			}

			ISecurityCheckpoint result;
			if (!CheckPointLookUpTable.TryGetValue(key, out result))
			{
				return (SecurityCheckpoint)getLazyCheckpoint?.Invoke(key) ?? InvoicingPluginProvider.FindJobInvoicingCheckPoint(key);
			}
			else
			{
				return (SecurityCheckpoint)result;
			}
		}

		bool providersLoaded;

		public SecurityCheckpoint FindCheckPoint(string key)
		{
			return FindCheckPoint(new CheckpointLookupKey(key));
		}

		public bool AddCheckPoint(CheckpointLookupKey key, ISecurityCheckpoint checkpoint)
		{
			var result = !CheckPointLookUpTable.ContainsKey(key);
			CheckPointLookUpTable[key] = checkpoint;
			return result;
		}

		IEnumerable<ISecurityCheckpoint> IZSecurity.AllLoadedCheckPoints
		{
			get
			{
				InvoicingPluginProvider.BuildAllPlugins();
				if (initAllLazyCheckpoints != null)
				{
					initAllLazyCheckpoints();
					initAllLazyCheckpoints = null;
				}
				return CheckPointLookUpTable.Values;
			}
		}
#if DEBUG
		Dictionary<CheckpointLookupKey, ISecurityCheckpoint> IZSecurity.CheckPointLookUpTable_ForTest => CheckPointLookUpTable;

		void IZSecurity.ResetInvoicingPluginProvider() => invoicingPluginProvider = null;
#endif

		Dictionary<CheckpointLookupKey, ISecurityCheckpoint> CheckPointLookUpTable
		{
			get
			{
				if (fCheckPointLookUpTable == null)
				{
					fCheckPointLookUpTable = new Dictionary<CheckpointLookupKey, ISecurityCheckpoint>(CheckPointLookupTableCapacity);
				}
				return fCheckPointLookUpTable;
			}
		}

		internal const int CheckPointLookupTableCapacity = 6000; // Magic number is approximately 2x the number of security checkpoints today.
		Dictionary<CheckpointLookupKey, ISecurityCheckpoint> fCheckPointLookUpTable;

		#endregion

		#region Staff rights

		public SecurityState IsStaffAllowed(SecurityCheckpoint checkPoint)
		{
			EnsureCurrentThreadIsOwner();
			var result = SecurityState.Granted;

			if (Group != null)
			{
				result = SecurityState.Implicit;
			}
			else if (StaffMember == null || !StaffMember.GS_IsOperational)
			{
				result = SecurityState.Denied;
			}
			else if (StaffMember != null && StaffMember.GS_IsSystemAccount && StaffMember.GS_LoginName != User.SupportUserName)
			{
				// Bypass security check for the built-in system accounts
			}
			else if (checkPoint.Code != NoneSecurityCheckpoint.NoneCode && !StaffMember.GS_IsController)
			{
				result = IsAllowedBasedOnStaffRights(checkPoint);
			}
			return result;
		}

		SecurityState IsAllowedBasedOnStaffRights(SecurityCheckpoint checkPoint)
		{
			return IsAllowedBasedOnRights(checkPoint, false, StaffMember.PK);
		}

		#endregion

		#region Group rights

		public SecurityState IsGroupAllowed(SecurityCheckpoint checkpoint)
		{
			EnsureCurrentThreadIsOwner();
			if (Group != null)
			{
				return IsAllowedForGroup(checkpoint, Group);
			}

			foreach (GlbGroup staffGroup in StaffMember.ActiveGroups)
			{
				var groupState = IsAllowedForGroup(checkpoint, staffGroup);

				if (groupState == SecurityState.Granted || (groupState == SecurityState.Implicit && !staffGroup.IsNonSecurityGroup))
				{
					return groupState;
				}
			}

			return SecurityState.Denied;
		}

		public bool IsGroupExplicitlyAllowed(ISecurityCheckpoint checkpoint)
		{
			foreach (GlbGroup staffGroup in StaffMember.ActiveGroups)
			{
				var groupState = IsAllowedForGroup((SecurityCheckpoint)checkpoint, staffGroup);

				if (groupState == SecurityState.Granted)
				{
					return true;
				}
			}
			return false;
		}

		public GlbSecurityCollection GetGroupRightsWithImplicitRights(SecurityCheckpoint checkpoint, IZGlbSecurityCollection collection = null)
		{
			GlbSecurityCollection securityCollection;
			if (collection == null)
			{
				securityCollection = new GlbSecurityCollection(Factory);
			}
			else
			{
				securityCollection = (GlbSecurityCollection)collection;
				securityCollection.RemoveAll();
			}

			foreach (GlbGroup group in StaffMember.ActiveGroups)
			{
				if (group.IsDeleted)
				{
					continue;
				}
				var addedSecurities = new List<GlbSecurity>();

				var departments = new Dictionary<ZGuid, int>();
				var companies = new Dictionary<ZGuid, int>();
				var branches = new Dictionary<ZGuid, int>();

				int minimumFoundWeight = 100;

				/*
				 * Iterating from child to parent checkpoint then most specific to least specific security rules, we only care about new ones if they are
				 * 1) more broad than (lower weight) all records with the same departments OR branches OR companies as we've checked
				 * 2) equally as broad (same weight), but without an exact match in this checkpoint/child checkpoints
				 * And once we've found a global (weight 0) rule we're done, nothing in a parent can override that
				 */
				var currentCheckpoint = checkpoint;
				while (currentCheckpoint != null)
				{
					//using GlbSecurityCollection.Find for performance speedup
					foreach (var securityRight in securityData.Find(GlbSecurityRightsLookupKey.ForLookup(currentCheckpoint, group.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty), true).
								OrderByDescending(GetSecurityWeight))
					{
						int weight = GetSecurityWeight(securityRight);

						int minimumStoredWeight = 100;
						int storedWeight;
						if (departments.TryGetValue(securityRight.GU_GE, out storedWeight) && storedWeight < minimumStoredWeight)
						{
							minimumStoredWeight = storedWeight;
						}
						if (companies.TryGetValue(securityRight.GU_GC, out storedWeight) && storedWeight < minimumStoredWeight)
						{
							minimumStoredWeight = storedWeight;
						}
						if (branches.TryGetValue(securityRight.GU_GB, out storedWeight) && storedWeight < minimumStoredWeight)
						{
							minimumStoredWeight = storedWeight;
						}

						//Is this a more general security OR is it just as general as the ones we're doing, but not a duplicate?
						if (weight < minimumStoredWeight
							|| (weight == minimumStoredWeight && !addedSecurities.Any(x => x.GU_GE == securityRight.GU_GE
									&& x.GU_GC == securityRight.GU_GC
									&& x.GU_GB == securityRight.GU_GB)))
						{
							addedSecurities.Add(securityRight);
							if (weight < minimumFoundWeight)
							{
								minimumFoundWeight = weight;
							}
						}

						if (!departments.TryGetValue(securityRight.GU_GE, out storedWeight) || weight < storedWeight)
						{
							departments[securityRight.GU_GE] = weight;
						}
						if (!companies.TryGetValue(securityRight.GU_GC, out storedWeight) || weight < storedWeight)
						{
							companies[securityRight.GU_GC] = weight;
						}
						if (!branches.TryGetValue(securityRight.GU_GB, out storedWeight) || weight < storedWeight)
						{
							branches[securityRight.GU_GB] = weight;
						}
					}

					if (minimumFoundWeight == 0)
					{
						break;
					}

					currentCheckpoint = currentCheckpoint.Parent;
				}

				//if we never found a global rule we need to make one
				if (minimumFoundWeight != 0)
				{
					var groupState = IsAllowedForGroupTopAccessLevel(checkpoint, group);
					var isAllowedValue = (groupState == SecurityState.Granted) || (groupState == SecurityState.Implicit);
					var fakeRecord = Factory.New<GlbSecurity>();
					using (fakeRecord.GetValidationSuspender())
					{
						fakeRecord.GU_SecurityItemIsAllowed = isAllowedValue;
						fakeRecord.GU_SecurityRight = checkpoint.Code;
						fakeRecord.GU_ItemGUID = checkpoint.ItemGuid;
						fakeRecord.GU_GG = group.PK;
					}
					securityCollection.Add(fakeRecord);
				}
				securityCollection.AddRange(addedSecurities);
			}

			return securityCollection;
		}

		static int GetSecurityWeight(GlbSecurity security)
		{
			// GB + GE = 6
			// GC + GE = 5
			// GB      = 4
			// GC      = 3
			// GE      = 2
			// *       = 0
			return (!security.GU_GE.IsEmpty ? 2 : 0) + (!security.GU_GC.IsEmpty ? 3 : 0) + (!security.GU_GB.IsEmpty ? 4 : 0);
		}

		SecurityState IsAllowedForGroupTopAccessLevel(SecurityCheckpoint checkPoint, GlbGroup group)
		{
			var result = SecurityState.Implicit;

			var securityRights = securityData.Find(GlbSecurityRightsLookupKey.ForLookup(checkPoint, group.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));

			var groupRightsFound = (securityRights.Length > 0);

			if (groupRightsFound)
			{
				result = securityRights[0].GU_SecurityItemIsAllowed ? SecurityState.Granted : SecurityState.Denied;
			}
			else if (checkPoint.Parent != null)
			{
				result = IsAllowedForGroupTopAccessLevel(checkPoint.Parent, group);
			}
			return result;
		}

		SecurityState IsAllowedForGroup(SecurityCheckpoint checkPoint, GlbGroup group)
		{
			return IsAllowedBasedOnRights(checkPoint, true, group.PK);
		}

		#endregion

		#region Security Error

		public void ShowError(SecurityCheckpoint checkpoint)
		{
			Globals.Message.ShowError(GetErrorMessageForNotAllowed(checkpoint), ResString.GetMultilingualString("07E16132-D3C0-4D54-9C4F-3A282225C6D4", "Access Denied: {0}", checkpoint.HumanReadableName));
		}

		public void ShowError(SecurityCheckpoint[] checkpoints)
		{
			if (checkpoints.Length > 0)
			{
				Globals.Message.ShowError(GetErrorMessageForNotAllowed(checkpoints), ResString.GetMultilingualString("B2D8BB8B-D633-4C18-A85B-1B0E18C700EF", "Access Denied"));
			}
		}

		public MultilingualString GetErrorMessageForNotAllowed(SecurityCheckpoint checkpoint)
		{
			var result =
				checkpoint is ControllerOrExplicitAccessOnlyCheckpoint || (Env.CurrentUser?.IsOperational ?? false) ?
				MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, SecurityCore.SecurityErrorMessage, checkpoint.DisplayTextPathToSecurityRight) :
				SecurityCore.NonOperationalErrorMessage;
			return result;
		}

		public MultilingualString GetErrorMessageForNotAllowedInGLOW(SecurityCheckpoint checkpoint)
		{
			// Consistent with the logic of CW1, although GLOW has not yet implemented control over staff operatinal
			var result =
				checkpoint is ControllerOrExplicitAccessOnlyCheckpoint || (Env.CurrentUser?.IsOperational ?? false) ?
				MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, SecurityCore.SecurityErrorMessageInGLOW, checkpoint.DisplayTextPathToSecurityRight) :
				SecurityCore.NonOperationalErrorMessage;
			return result;
		}

		public MultilingualString GetErrorMessageForNotAllowed(SecurityCheckpoint[] checkpoints)
		{
			MultilingualString result = (NoResString)"";
			if (Env.CurrentUser?.IsOperational ?? false)
			{
				foreach (var checkpoint in checkpoints)
				{
					result = result.IsEmpty ? checkpoint.DisplayTextPathToSecurityRight :
						MultilingualString.Join(System.Environment.NewLine, result, checkpoint.DisplayTextPathToSecurityRight);
				}
				result = MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, SecurityCore.SecurityErrorMessage, result);
			}
			else
			{
				result = SecurityCore.NonOperationalErrorMessage;
			}

			return result;
		}

		#endregion

		#region Reset Security

		public void ResetData(GlbSecurityCollection securityData, GlbStaff staffMember, ZGuid newBranchPK, ZGuid newDepartmentPK, ZGuid newCompanyPK, bool reloadStaffOrGroupObject = true)
		{
			this.staffMember = null;
			groupMember = null;

			SetupSecurity(securityData, staffMember, newBranchPK, newDepartmentPK, newCompanyPK, reloadStaffOrGroupObject);

			foreach (var checkPoint in CheckPointLookUpTable.Values)
			{
				checkPoint.ClearIsAllowedCache();
			}
		}

		#endregion

		#region Shared Methods

		SecurityState IsAllowedBasedOnRights(SecurityCheckpoint checkPoint, bool groupOwner, ZGuid ownerPk)
		{
			var result = SecurityState.Implicit;
			var rightsArray = new List<ZBool>();

			if (DepartmentPK == ZGuid.Empty)
			{
				if (BranchPK == ZGuid.Empty)
				{
					if (CompanyPK == ZGuid.Empty)
					{
						LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
					}
					else
					{
						LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, CompanyPK);
						LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
					}
				}
				else
				{
					LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, BranchPK, CompanyPK);
					LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, BranchPK, ZGuid.Empty);
					LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, CompanyPK);
					LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
				}
			}
			else
			{
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, DepartmentPK, BranchPK, CompanyPK);
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, DepartmentPK, BranchPK, ZGuid.Empty);
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, DepartmentPK, ZGuid.Empty, CompanyPK);
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, BranchPK, ZGuid.Empty);
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, CompanyPK);
				LoadRightsInArray(checkPoint.Code, rightsArray, checkPoint, groupOwner, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			}

			bool rightsFound = (rightsArray.Count > 0);

			if (rightsFound)
			{
				result = rightsArray[0] ? SecurityState.Granted : SecurityState.Denied;
			}
			else if (checkPoint.Parent != null)
			{
				result = IsAllowedBasedOnRights(checkPoint.Parent, groupOwner, ownerPk);
			}

			return result;
		}

		void LoadRightsInArray(string securityRight, List<ZBool> rightsArray, SecurityCheckpoint checkPoint, bool groupOwner, ZGuid ownerPk, ZGuid departmentPk, ZGuid branchPk, ZGuid companyPk)
		{
			if (rightsArray.Count > 0)
			{
				return;
			}

			var state = GetSecurityStateForOneLevel(securityData, checkPoint, groupOwner, ownerPk, departmentPk, branchPk, companyPk);

			if (state == SecurityState.Granted)
			{
				rightsArray.Add(true);
			}
			else if (state == SecurityState.Denied)
			{
				rightsArray.Add(false);
			}
		}

		SecurityCheckpoint GetNewLocalAdministratorCheckpoint(CheckpointLookupKey key)
		{
			return new LocalAdminCheckpoint(key.Code, this, key.ItemGuid);
		}

		#endregion

		#region IZSecurity Members - For Core Passthrough

		SecurityState IZSecurity.IsGroupAllowed(ISecurityCheckpoint checkpoint)
		{
			return IsGroupAllowed((SecurityCheckpoint)checkpoint);
		}

		SecurityState IZSecurity.IsStaffAllowed(ISecurityCheckpoint checkPoint)
		{
			return IsStaffAllowed((SecurityCheckpoint)checkPoint);
		}

		ISecurityCheckpoint IZSecurity.FindCheckPoint(CheckpointLookupKey key)
		{
			return FindCheckPoint(key);
		}

		ISecurityCheckpoint IZSecurity.FindCheckPoint(string key)
		{
			return FindCheckPoint(key);
		}

		ISecurityCheckpoint[] IZSecurity.GetAllRegistryCheckPoint()
		{
			return GetAllRegistryCheckPoint();
		}
		ISecurityCheckpoint[] IZSecurity.GetAllPrintQueueCheckPoint()
		{
			return GetAllPrintQueueCheckPoint();
		}

		ISecurityCheckpoint[] IZSecurity.GetAllDocumentTypeUploadCheckPoint()
		{
			return GetAllDocumentTypeUploadCheckPoint();
		}

		ISecurityCheckpoint[] IZSecurity.GetAllDocumentTypeViewCheckPoint()
		{
			return GetAllDocumentTypeViewCheckPoint();
		}

		ISecurityCheckpoint[] IZSecurity.GetAllDocumentTypeCutCheckPoint()
		{
			return GetAllDocumentTypeCutCheckPoint();
		}

		ISecurityCheckpoint[] IZSecurity.GetAllDocumentTypePermanentDeleteCheckPoint()
		{
			return GetAllDocumentTypePermanentDeleteCheckPoint();
		}

		MultilingualString IZSecurity.GetErrorMessageForNotAllowed(ISecurityCheckpoint checkPoint)
		{
			return GetErrorMessageForNotAllowed((SecurityCheckpoint)checkPoint);
		}

		MultilingualString IZSecurity.GetErrorMessageForNotAllowedInGLOW(ISecurityCheckpoint checkPoint)
		{
			return GetErrorMessageForNotAllowedInGLOW((SecurityCheckpoint)checkPoint);
		}

		MultilingualString IZSecurity.GetErrorMessageForNotAllowed(ISecurityCheckpoint[] checkPoints)
		{
			return GetErrorMessageForNotAllowed(checkPoints.Cast<SecurityCheckpoint>().ToArray());
		}

		ISecurityCheckpoint IZSecurity.GetRegistryCheckPoint(string identifier, string displayName)
		{
			return GetRegistryCheckPoint(identifier, displayName);
		}

		ISecurityCheckpoint IZSecurity.GetPrintQueueCheckPoint(Guid printQueuePK, string displayName)
		{
			return GetPrintQueueCheckPoint(printQueuePK, displayName);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "By calling the constructor of the checkpoints, they get added to the parents list. We dont need to capture them to add them explicitly")]
		ISecurityCheckpoint IZSecurity.FindOrCreateConversationCheckpoint(ISecurityCheckpoint parent, ModuleIdentifier moduleId)
		{
			var rootCode = moduleId + SecurityCore.ConversationAutoGeneratedCode;

			var root = FindCheckPoint(rootCode);
			if (root == null)
			{
				root = new SecurityCheckpoint(rootCode, SecurityCore.ConversationAutoGeneratedDisplayText, parent, this);

				new SecurityCheckpoint(rootCode + SecurityCore.ConversationViewAutoGeneratedCode, SecurityCore.ConversationViewAutoGeneratedDisplayText, root, this);
				new SecurityCheckpoint(rootCode + SecurityCore.ConversationModifyExternalParticipantsAutoGeneratedCode, SecurityCore.ConversationModifyExternalParticipantsAutoGeneratedDisplayText, root, this);
				new SecurityCheckpoint(rootCode + SecurityCore.ConversationEditInternalParticipantsAutoGeneratedCode, SecurityCore.ConversationEditInternalParticipantsAutoGeneratedDisplayText, root, this);
				new SecurityCheckpoint(rootCode + SecurityCore.ConversationSendMessagesAutoGeneratedCode, SecurityCore.ConversationSendMessagesAutoGeneratedDisplayText, root, this);
			}

			return root;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateConversationCheckpoint(ISecurityCheckpoint parent, ModuleIdentifier moduleId, string childName)
		{
			var root = ((IZSecurity)this).FindOrCreateConversationCheckpoint(parent, moduleId);

			return FindCheckPoint(root.Code + childName);
		}

		ISecurityCheckpoint IZSecurity.GetDocumentTypeUploadCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeUploadCheckPoint(rT_DocType);
		}

		ISecurityCheckpoint IZSecurity.GetDocumentTypeViewCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeViewCheckPoint(rT_DocType);
		}

		ISecurityCheckpoint IZSecurity.GetDocumentTypeCutCheckPoint(string rT_DocType)
		{
			return GetDocumentTypeCutCheckPoint(rT_DocType);
		}

		ISecurityCheckpoint IZSecurity.GetDocumentTypePermanentDeleteCheckPoint(string rT_DocType)
		{
			return GetDocumentTypePermanentDeleteCheckPoint(rT_DocType);
		}

		void IZSecurity.ShowError(ISecurityCheckpoint checkPoint)
		{
			ShowError((SecurityCheckpoint)checkPoint);
		}

		void IZSecurity.ShowError(ISecurityCheckpoint[] checkPoints)
		{
			ShowError(checkPoints.Cast<SecurityCheckpoint>().ToArray());
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateChangeGroupSecurityCheckpoint(Guid securityGroupPK)
		{
			var key = new CheckpointLookupKey(GlbSecurity.ChangeOtherGroupSecurityRightName, securityGroupPK);
			return FindCheckPoint(key) ?? GetNewLocalAdministratorCheckpoint(key);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateChangeStaffSecurityCheckpoint(Guid staffPK)
		{
			var key = new CheckpointLookupKey(GlbSecurity.ChangeOtherStaffSecurityRightName, staffPK);
			return FindCheckPoint(key) ?? GetNewLocalAdministratorCheckpoint(key);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateGroupOwnerSecurityCheckpoint(Guid securityGroupPK)
		{
			var key = new CheckpointLookupKey(GlbSecurity.GroupOwnerSecurityRightName, securityGroupPK);
			return FindCheckPoint(key) ?? GetNewLocalAdministratorCheckpoint(key);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateAccessModuleCheckPoint(ISecurityCheckpoint parent)
		{
			if (parent is NoneSecurityCheckpoint)
			{ return parent; }
			if (parent == null)
			{ return null; }
			if (parent is SecurityCheckpointNonOperationalAllowed)
			{ return FindOrCreateNonOperationalAllowedCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.AccessModuleAutoGeneratedCode, SecurityCore.AccessModuleAutoGeneratedDisplayText, Guid.Empty); }
			if (parent is ControllerOrExplicitAccessOnlyCheckpoint)
			{ return FindOrCreateControllerOrExplicitAccessOnlyCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.AccessModuleAutoGeneratedCode, SecurityCore.AccessModuleAutoGeneratedDisplayText); }
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.AccessModuleAutoGeneratedCode, SecurityCore.AccessModuleAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateExportCheckPoint(ISecurityCheckpoint parent)
		{
			return (parent != null) ? FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ExportToExcelAutoGeneratedCode, SecurityCore.ExportToExcelAutoGeneratedDisplayText, Guid.Empty) : null;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateImportCheckPoint(ISecurityCheckpoint parent)
		{
			return (parent != null) ? FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ImportToSystemAutoGeneratedCode, SecurityCore.ImportToSystemAutoGeneratedDisplayText, Guid.Empty) : null;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateExportNativeXmlCheckPoint(ISecurityCheckpoint parent)
		{
			return (parent != null) ? FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ExportNativeXMLAutoGeneratedCode, SecurityCore.ExportNativeXMLAutoGeneratedDisplayText, Guid.Empty) : null;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateImportNativeXmlCheckPoint(ISecurityCheckpoint parent)
		{
			return (parent != null) ? FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ImportNativeXMLAutoGeneratedCode, SecurityCore.ImportNativeXMLAutoGeneratedDisplayText, Guid.Empty) : null;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateReportCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			if (parent != null && moduleId != null && !parent.Code.EndsWith(SecurityCore.RunReportsAutoGeneratedCode))
			{
				parent = Env.Security.FindOrCreateReportRunCheckpoint(moduleId, parent);
			}

			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, (NoResString)"Report", stmMenuItemName, stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateReportCustomizeCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, moduleId + SecurityCore.CustomizeReportsAutoGeneratedCode, SecurityCore.CustomizeReportsAutoGeneratedCodeDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateReportScheduleCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, moduleId + SecurityCore.ScheduleReportsAutoGeneratedCode, SecurityCore.ScheduleReportsAutoGeneratedCodeDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateReportRunCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, moduleId + SecurityCore.RunReportsAutoGeneratedCode, SecurityCore.RunReportsAutoGeneratedCodeDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateDocumentsCheckpoint(ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, (NoResString)"Doc" + moduleID + (NoResString)"Documents", RawDataRegistry.Categories.Documents, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateDocumentCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, (NoResString)"Doc" + moduleID, (NoResString)GetMenuItemDisplayName(stmMenuItemPK, stmMenuItemName), stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateDocumentOverrideCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, "DocVis" + moduleID, (NoResString)GetMenuItemDisplayName(stmMenuItemPK, stmMenuItemName), stmMenuItemPK);
		}

		string GetMenuItemDisplayName(Guid stmMenuItemPK, MultilingualString stmMenuItemName)
		{
			var stmMenuItem = Factory.Load<StmMenuItem>(stmMenuItemPK);
			var menuPath = stmMenuItem.SU_MenuPath.Replace("/", " -- ");
			var menuItemDisplayName = string.IsNullOrEmpty(menuPath) ? stmMenuItemName : (menuPath + " -- " + stmMenuItemName);
			return menuItemDisplayName;
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateVisualizerFormsCheckpoint(ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, (NoResString)"Form" + moduleID + (NoResString)"Forms", RawDataRegistry.Categories.Forms, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateVisualizerFormCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, (NoResString)"Form" + moduleID, stmMenuItemName, stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateVisualizerFormModifyCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, "FormModify" + moduleID, stmMenuItemName, stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateVisualizerFormDeliveryCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, "FormDelivery" + moduleID, stmMenuItemName, stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateVisualizerFormSendMessageCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, "FormSendMessage" + moduleID, stmMenuItemName, stmMenuItemPK);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateCopyCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UseCopyFunctionAutoGeneratedCode, SecurityCore.UseCopyFunctionAutoGeneratedDisplayText, Guid.Empty);
		}

		#region Universal Copy

		ISecurityCheckpoint IZSecurity.FindOrCreateUniversalCopyCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UniversalCopyAutoGeneratedCode, SecurityCore.UniversalCopyAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateUniversalCopyRunCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UniversalCopyRunAutoGeneratedCode, SecurityCore.UniversalCopyRunAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateUniversalCopyCEDPrivateCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UniversalCopyCreateEditDeletePrivateAutoGeneratedCode, SecurityCore.UniversalCopyCreateEditDeletePrivateAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateUniversalCopyEditPublishCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UniversalCopyEditPublishAutoGeneratedCode, SecurityCore.UniversalCopyEditPublishAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateUniversalCopyDeletePublishCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.UniversalCopyDeletePublishAutoGeneratedCode, SecurityCore.UniversalCopyDeletePublishAutoGeneratedDisplayText, Guid.Empty);
		}

		#endregion

		#region Operational Actions

		ISecurityCheckpoint IZSecurity.FindOrCreateOperationalActionsCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.Captions.OperationalActionsAutoGeneratedCode, SecurityCore.Captions.OperationalActions, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateOperationalActionsCustomiseCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)((IZSecurity)this).FindOrCreateOperationalActionsCheckpoint((SecurityCheckpoint)parentCheckpoint),
				parentCheckpoint.Code + SecurityCore.Captions.CustomiseActionsAutoGeneratedCode, SecurityCore.Captions.CustomiseActions, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateOperationalActionsAllowRunOnAllMatchingRecordsCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)((IZSecurity)this).FindOrCreateOperationalActionsCheckpoint((SecurityCheckpoint)parentCheckpoint),
				parentCheckpoint.Code + SecurityCore.Captions.AllowRunOnAllMatchingRecordsAutoGeneratedCode, SecurityCore.Captions.AllowRunOnAllMatchingRecords, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateOperationalActionsRunCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)((IZSecurity)this).FindOrCreateOperationalActionsCheckpoint((SecurityCheckpoint)parentCheckpoint),
				parentCheckpoint.Code + SecurityCore.Captions.RunActionsAutoGeneratedCode, SecurityCore.Captions.RunActions, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateOperationalActionsRunSpecificCheckpoint(MultilingualString name, ISecurityCheckpoint parentCheckpoint)
		{
			return DontLoadJustFindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, new ZString(parentCheckpoint.Code + name.GetUnresolvedString()).Right(100), name, Guid.Empty);
		}

		#endregion

		#region Template Record

		ISecurityCheckpoint IZSecurity.FindOrCreateTemplateRecordCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.TemplateRecordAutoGeneratedCode, SecurityCore.TemplateRecordAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateTemplateRecordAddCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.TemplateRecordAddAutoGeneratedCode, SecurityCore.TemplateRecordAddAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateTemplateRecordEditCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.TemplateRecordEditAutoGeneratedCode, SecurityCore.TemplateRecordEditAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateTemplateRecordDeleteCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.TemplateRecordDeleteAutoGeneratedCode, SecurityCore.TemplateRecordDeleteAutoGeneratedDisplayText, Guid.Empty);
		}

		#endregion

		#region ISACHK

		ISecurityCheckpoint IZSecurity.FindOrCreateISACHKCheckPoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ISACHKAutoGeneratedCode, SecurityCore.ISACHKAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateISACHKSendWithMessageErrorsCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, parent.Code + SecurityCore.ISACHKSendWithMessageErrorsAutoGeneratedCode, SecurityCore.ISACHKSendWithMessageErrorsAutoGeneratedDisplayText, Guid.Empty);
		}

		#endregion

		#region Workflow

		ISecurityCheckpoint IZSecurity.FindOrCreateWorkflowCheckpoint(ISecurityCheckpoint parent)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parent, GetWorkflowCheckpointCode(parent), SecurityCore.WorkflowAutoGeneratedDisplayText, Guid.Empty);
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateWorkflowItemCheckpoint(ISecurityCheckpoint parent, string workflowItemCheckpointCode)
		{
			var sourceCheckpoint = (SecurityCheckpoint)((IZSecurity)this).FindOrCreateWorkflowCheckpoint(parent);
			var workflowCheckpoint = FindParentWorkflowCheckpointForCode(sourceCheckpoint, workflowItemCheckpointCode);
			var displayText = GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode);
			return FindOrCreateCheckPoint(workflowCheckpoint, workflowCheckpoint.Code + workflowItemCheckpointCode, displayText, Guid.Empty);
		}

		SecurityCheckpoint FindParentWorkflowCheckpointForCode(SecurityCheckpoint sourceCheckpoint, string workflowItemCheckpointCode)
		{
			switch (workflowItemCheckpointCode)
			{
				case SecurityCore.WorkflowAddTasksAutoGeneratedCode:
				case SecurityCore.WorkflowDeleteTasksAutoGeneratedCode:
					return FindOrCreateCheckPoint(sourceCheckpoint,
													sourceCheckpoint.Code + SecurityCore.WorkflowTasksAutoGeneratedCode,
													GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode),
													Guid.Empty);

				case SecurityCore.WorkflowAddMilestonesAutoGeneratedCode:
				case SecurityCore.WorkflowDeleteMilestonesAutoGeneratedCode:
					return FindOrCreateCheckPoint(sourceCheckpoint,
													sourceCheckpoint.Code + SecurityCore.WorkflowMilestonesAutoGeneratedCode,
													GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode),
													Guid.Empty);

				case SecurityCore.WorkflowAddExceptionsAutoGeneratedCode:
				case SecurityCore.WorkflowDeleteExceptionsAutoGeneratedCode:
				case SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedCode:
					return FindOrCreateCheckPoint(sourceCheckpoint,
								sourceCheckpoint.Code + SecurityCore.WorkflowExceptionsAutoGeneratedCode,
								GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode),
								Guid.Empty);

				case SecurityCore.WorkflowAddTriggersAutoGeneratedCode:
				case SecurityCore.WorkflowDeleteTriggersAutoGeneratedCode:
					return FindOrCreateCheckPoint(sourceCheckpoint,
								sourceCheckpoint.Code + SecurityCore.WorkflowTriggersAutoGeneratedCode,
								GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode),
								Guid.Empty);
				case SecurityCore.WorkflowAddEventsAutoGeneratedCode:
				case SecurityCore.WorkflowCancelEventsAutoGeneratedCode:
					return FindOrCreateCheckPoint(sourceCheckpoint,
								sourceCheckpoint.Code + SecurityCore.WorkflowEventsAutoGeneratedCode,
								GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode),
								Guid.Empty);

				default:
					return sourceCheckpoint;
			}
		}

		ISecurityCheckpoint IZSecurity.FindOrCreateWorkflowSubItemCheckpoint(ISecurityCheckpoint workflowProviderCheckpoint, string workflowItemCheckpointCode)
		{
			var workflowCheckpoint = (SecurityCheckpoint)((IZSecurity)this).FindCheckPoint(workflowProviderCheckpoint.Code);
			var displayText = GetWorkflowItemCheckpointDisplayText(workflowItemCheckpointCode);
			return FindOrCreateCheckPoint(workflowCheckpoint, workflowProviderCheckpoint.Code + workflowItemCheckpointCode, displayText, Guid.Empty);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Don't see any complexity at all")]
		static MultilingualString GetWorkflowItemCheckpointDisplayText(string workflowItemCheckpointCode)
		{
			switch (workflowItemCheckpointCode)
			{
				case SecurityCore.WorkflowExceptionsAutoGeneratedCode:
					return SecurityCore.WorkflowExceptionsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowMilestonesAutoGeneratedCode:
					return SecurityCore.WorkflowMilestonesAutoGeneratedDisplayText;
				case SecurityCore.WorkflowTasksAutoGeneratedCode:
					return SecurityCore.WorkflowTasksAutoGeneratedDisplayText;
				case SecurityCore.WorkflowTriggersAutoGeneratedCode:
					return SecurityCore.WorkflowTriggersAutoGeneratedDisplayText;
				case SecurityCore.WorkflowAddExceptionsAutoGeneratedCode:
					return SecurityCore.WorkflowAddExceptionsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowAddMilestonesAutoGeneratedCode:
					return SecurityCore.WorkflowAddMilestonesAutoGeneratedDisplayText;
				case SecurityCore.WorkflowAddTasksAutoGeneratedCode:
					return SecurityCore.WorkflowAddTasksAutoGeneratedDisplayText;
				case SecurityCore.WorkflowAddTriggersAutoGeneratedCode:
					return SecurityCore.WorkflowAddTriggersAutoGeneratedDisplayText;
				case SecurityCore.WorkflowDeleteExceptionsAutoGeneratedCode:
					return SecurityCore.WorkflowDeleteExceptionsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowDeleteMilestonesAutoGeneratedCode:
					return SecurityCore.WorkflowDeleteMilestonesAutoGeneratedDisplayText;
				case SecurityCore.WorkflowDeleteTasksAutoGeneratedCode:
					return SecurityCore.WorkflowDeleteTasksAutoGeneratedDisplayText;
				case SecurityCore.WorkflowDeleteTriggersAutoGeneratedCode:
					return SecurityCore.WorkflowDeleteTriggersAutoGeneratedDisplayText;
				case SecurityCore.WorkflowExceptionsJustViewAutoGeneratedCode:
					return SecurityCore.WorkflowExceptionsJustViewAutoGeneratedDisplayText;
				case SecurityCore.WorkflowMilestonesJustViewAutoGeneratedCode:
					return SecurityCore.WorkflowMilestonesJustViewAutoGeneratedDisplayText;
				case SecurityCore.WorkflowTasksJustViewAutoGeneratedCode:
					return SecurityCore.WorkflowTasksJustViewAutoGeneratedDisplayText;
				case SecurityCore.WorkflowTriggersJustViewAutoGeneratedCode:
					return SecurityCore.WorkflowTriggersJustViewAutoGeneratedDisplayText;
				case SecurityCore.WorkflowEventsAutoGeneratedCode:
					return SecurityCore.WorkflowEventsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowEventsJustViewAutoGeneratedCode:
					return SecurityCore.WorkflowEventsJustViewAutoGeneratedDisplayText;
				case SecurityCore.WorkflowAddEventsAutoGeneratedCode:
					return SecurityCore.WorkflowAddEventsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowCancelEventsAutoGeneratedCode:
					return SecurityCore.WorkflowCancelEventsAutoGeneratedDisplayText;
				case SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedCode:
					return SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedDisplayText;
			}

			return (NoResString)workflowItemCheckpointCode;
		}

		string GetWorkflowCheckpointCode(ISecurityCheckpoint workflowProviderCheckpoint)
		{
			return workflowProviderCheckpoint.Code + SecurityCore.WorkflowAutoGeneratedCode;
		}

		#endregion

		#region Send Email

		ISecurityCheckpoint IZSecurity.FindOrCreateSendEmailCheckpoint(ISecurityCheckpoint parentCheckpoint)
		{
			return FindOrCreateCheckPoint((SecurityCheckpoint)parentCheckpoint, parentCheckpoint.Code + SecurityCore.SendEmailAutoGeneratedCode, SecurityCore.SendEmailAutoGeneratedDisplayText, Guid.Empty);
		}

		#endregion

		SecurityCheckpoint DontLoadJustFindOrCreateCheckPoint(SecurityCheckpoint parent, string code, MultilingualString displayName, Guid itemPK)
		{
			return FindCheckPoint(new CheckpointLookupKey(code, itemPK), false) ?? new SecurityCheckpoint(code, displayName, parent, this, itemPK);
		}

		SecurityCheckpoint FindOrCreateCheckPoint(SecurityCheckpoint parent, string code, MultilingualString displayName, Guid itemPK)
		{
			return FindCheckPoint(new CheckpointLookupKey(code, itemPK), true) ?? new SecurityCheckpoint(code, displayName, parent, this, itemPK);
		}

		SecurityCheckpoint FindOrCreateNonOperationalAllowedCheckPoint(SecurityCheckpoint parent, string code, MultilingualString displayName, Guid itemPK)
		{
			return FindCheckPoint(new CheckpointLookupKey(code, itemPK), true) ?? new SecurityCheckpointNonOperationalAllowed(code, displayName, parent, this, itemPK);
		}

		SecurityCheckpoint FindOrCreateControllerOrExplicitAccessOnlyCheckPoint(SecurityCheckpoint parent, string code, MultilingualString displayName)
		{
			return FindCheckPoint(new CheckpointLookupKey(code, Guid.Empty), true) ?? new ControllerOrExplicitAccessOnlyCheckpoint(code, displayName, parent, this);
		}

		void IZSecurity.ResetData(IZGlbSecurityCollection securityCollection, object resetStaff, Guid branchPK, Guid departmentPK, Guid companyPK)
		{
			((IZSecurity)this).ResetData(securityCollection, resetStaff, branchPK, departmentPK, companyPK, true);
		}

		void IZSecurity.ResetData(IZGlbSecurityCollection securityCollection, object resetStaff, Guid branchPK, Guid departmentPK, Guid companyPK, bool reloadStaffOrGroupObject)
		{
			GlbStaff newStaff = null;
			if (resetStaff is BusinessObject)
			{
				newStaff = (GlbStaff)resetStaff;
			}
			else if (resetStaff != null)
			{
				var pk = resetStaff is IUser user ? user.PK : (Guid)resetStaff;
				if (pk != Guid.Empty)
				{
					newStaff = Factory.Load<GlbStaff>(pk);
				}
			}
			ResetData((GlbSecurityCollection)securityCollection, newStaff, branchPK, departmentPK, companyPK, reloadStaffOrGroupObject);
		}

		Guid IZSecurity.UserPK
		{
			get { return UserPK != ZGuid.Empty && UserPK != ZGuid.Invalid ? UserPK.ToGuid() : Guid.Empty; }
			set { UserPK = value; }
		}

		object IZSecurity.GetGroupRightsWithImplicitRights(ISecurityCheckpoint checkPoint, IZGlbSecurityCollection collection)
		{
			return GetGroupRightsWithImplicitRights((SecurityCheckpoint)checkPoint, collection);
		}

		Guid IZSecurity.BranchPK
		{
			get { return BranchPK != ZGuid.Empty && BranchPK != ZGuid.Invalid ? BranchPK.ToGuid() : Guid.Empty; }
			set { BranchPK = value; }
		}

		Guid IZSecurity.DepartmentPK
		{
			get { return DepartmentPK != ZGuid.Empty && DepartmentPK != ZGuid.Invalid ? DepartmentPK.ToGuid() : Guid.Empty; }
			set { DepartmentPK = value; }
		}

		Guid IZSecurity.CompanyPK
		{
			get { return CompanyPK != ZGuid.Empty && CompanyPK != ZGuid.Invalid ? CompanyPK.ToGuid() : Guid.Empty; }
			set { CompanyPK = value; }
		}

		Guid IZSecurity.GroupPK
		{
			get { return GroupPK != ZGuid.Empty && GroupPK != ZGuid.Invalid ? GroupPK.ToGuid() : Guid.Empty; }
			set { GroupPK = value; }
		}

		void IZSecurity.LockDownUserCompanyDepartmentBranch()
		{
			fIsLocked = true;
		}

		bool fIsLocked;

		bool IZSecurity.IsSecurityAllowedForAllBranches(ISecurityCheckpoint checkpoint)
		{
			return new SecurityLocator(StaffMember, Factory).IsSecurityAllowedForAllBranches((SecurityCheckpoint)checkpoint);
		}

		#endregion

		#region Thread Safety

		public IThreadSentry ThreadSentry { get; private set; }

		public void EnsureCurrentThreadIsOwner() => ThreadSentry.EnsureCurrentThreadIsOwner();

		#endregion

		void IDisposable.Dispose()
		{
			if (ThreadLocalFactory != null)
			{
				ThreadLocalFactory.Dispose();
			}
		}

		#region Test Only Accessors
#if DEBUG

		internal GlbSecurityCollection SecurityDataForTesting
		{
			get { return securityData; }
			set { securityData = value; }
		}

#endif
		#endregion
	}
}
