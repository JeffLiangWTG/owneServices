using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.Shared.Dash.Common.Services;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Base Class for StorageDocs, StorageDocsUnallocated and StorageFile to inherit from
	/// </summary>
	public abstract class StorageDocsBase : StorageDocsWithS3Support, IDeliverable, IDisposable, IeDoc, ICanBeSavedByDocumentFactory, IDeliveryEmailAttachment
	{
		protected StorageDocsBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!(factory is NumberedBusinessObjectFactory))
			{
				throw new ArgumentException("StorageDocBase can only be loaded in a NumberedBusinessObjectFactory", nameof(factory));
			}

			if (!this.ReadOnly)
			{
				this.ReadOnly = SC_DocType != ZString.Empty && !Env.Security.GetDocumentTypeUploadCheckPoint(SC_DocType).IsAllowed;
			}
		}

		public static readonly StorageDocsTypeDecider TypeDecider = new StorageDocsTypeDecider();

		public static bool IsUsingExternalStorage => SystemDataRegistry.Instance.EDocsStorageProvider.Value != Core.Constants.EDocsStorageProviders.Code.DB;

		#region schema

		new public abstract class Schema : AutoStorageDocs.Schema
		{
			public const string SC_DescriptionForWeb = "SC_DescriptionForWeb";
			public const string SC_FriendlyFileDescriptionSchemaName = "SC_FriendlyFileDescription";
			public const string SC_FileNameForRenamingSchemaName = "SC_FileNameForRenaming";
			public const string SC_FileNameWithExtensionSchemaName = "SC_FileNameWithExtension";
			public const string SC_ImageDataHasValueSchemaName = "SC_ImageDataHasValue";

			public static class Indexes
			{
				public const string NR_RX__SC_ImageDataHasValue_SC_Date = "NR_RX__SC_ImageDataHasValue_SC_Date";
			}
		}

		#endregion

		protected override StorageDocsValidation GetNewValidation()
		{
			return new StorageDocsBaseValidation(this);
		}

		#region Properties

		#region Abstract Properties
#if DEBUG
		public
#else
		protected internal
#endif
		abstract bool SC_IsPublishedReadonlyDefault { get; }

		public abstract bool IsImageFile { get; }

		public MultilingualString SC_DescriptionForWeb => (NoResString)SC_FileNameWithExtension;
		public ZPropertyInfo SC_DescriptionForWebInfo => GetZPropertyInfo(nameof(SC_DescriptionForWeb));

		#endregion

		#region Lists

		public virtual CodeDescriptionPairList SC_DocType_List
		{
			get
			{
				if (docTypeList == null || (ParentMain != null && ParentMain.SM_Type != listSM_Type))
				{
					if (ParentMain != null)
					{
						listSM_Type = ParentMain.SM_Type;
					}
					docTypeList = BuildSC_DocType_List();
				}

				return docTypeList;
			}
		}
		CodeDescriptionPairList docTypeList;
		ZString listSM_Type;

		protected CodeDescriptionPairList BuildSC_DocType_List()
		{
			return DocScanningHelper.GetCategoryDocTypesFromJobType(GetDocTypeCategoryQuery(), MasterFactory, !suspendCheckSecurityRightsForDocTypes && !ReadOnly);
		}

		protected virtual DocTypeCategoryQuery GetDocTypeCategoryQuery()
		{
			var rT_ReferenceType = !SM_Type.IsEmpty
				? new ZString(AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(SM_Type))
				: new ZString(Core.Constants.ReferenceTypes.Unallocated); //ZString.Empty;

			return new DocTypeCategoryQuery(MasterFactory, rT_ReferenceType);
		}

		public virtual CodeDescriptionPairList SC_DocSource_List
		{
			get
			{
				if (docSourceList == null)
				{
					docSourceList = BuildSC_DocSource_List();
				}

				return docSourceList;
			}
		}
		CodeDescriptionPairList docSourceList;

		protected CodeDescriptionPairList BuildSC_DocSource_List()
		{
			return DocScanningHelper.GetDocSources(MasterFactory);
		}

		#endregion

		#region Company/Branch/Department Specific

#if DEBUG
		public
#endif
		LoginLocationBusinessObject LoginObj
		{
			get
			{
				if (loginObj == null)
				{
					var mainFactory = ((NumberedBusinessObjectFactory)Factory).MasterFactory.FactoryForEverythingExceptEDocs;
					loginObj = new LoginLocationBusinessObject(mainFactory);
					if (!SC_GC_Company.IsEmpty)
					{
						var company = mainFactory.Load<GlbCompany>(SC_GC_Company);
						if (company != null)
						{
							loginObj.CompanyCode = company.GC_Code;
						}
					}
					if (!SC_GB_Branch.IsEmpty)
					{
						var branch = mainFactory.Load<GlbBranch>(SC_GB_Branch);
						if (branch != null)
						{
							loginObj.BranchCode = branch.GB_Code;
						}
					}
					if (!SC_GE_Department.IsEmpty)
					{
						var department = mainFactory.Load<GlbDepartment>(SC_GE_Department);
						if (department != null)
						{
							loginObj.DepartmentCode = department.GE_Code;
						}
					}
				}

				return loginObj;
			}
		}
		LoginLocationBusinessObject loginObj;

		public bool IsCompanyBranchDepartmentSpecific
		{
			get => IsCompanySpecific || IsBranchSpecific || IsDepartmentSpecific;
			set
			{
				var oldValue = IsCompanySpecific || IsBranchSpecific || IsDepartmentSpecific;
				if (value != oldValue)
				{
					if (value)
					{
						CompanyCode = GlbCompany.CurrentCompany.GC_Code;
					}
					else
					{
						CompanyCode = ZString.Empty;
						BranchCode = ZString.Empty;
						DepartmentCode = ZString.Empty;
					}
				}
			}
		}

		#region Company

		public GlbCompanyCollection GlbCompanyList => LoginObj.Companies;

		[List("GlbCompanyList")]
		[MaxLength(3)]
		public ZString CompanyCode
		{
			get => LoginObj.CompanyCode;
			set
			{
				var oldValue = LoginObj.CompanyCode;
				LoginObj.CompanyCode = value;
				SC_GC_Company = LoginObj.Company == null ? ZGuid.Empty : LoginObj.Company.PK;
				if (!IsValidationSuspended)
				{
					((StorageDocsBaseValidation)Validation).ValidateCompanyCode();
				}
				CompanyCodeInfo.RefreshBinding(oldValue);
			}
		}

		public override ZGuid SC_GC_Company
		{
			get => base.SC_GC_Company;
			set
			{
				if (base.SC_GC_Company != value)
				{
					base.SC_GC_Company = value;
					if (!SC_GC_Company.IsEmpty)
					{
						var company = Factory.Load<GlbCompany>(SC_GC_Company);
						if (company != null && company.GC_Code != LoginObj.CompanyCode)
						{
							LoginObj.CompanyCode = company.GC_Code;
						}
					}
					else
					{
						LoginObj.CompanyCode = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo CompanyCodeInfo => GetZPropertyInfo(nameof(CompanyCode));

		public bool IsBelongingToCurrentLoginCompany => SC_GC_Company == ZGuid.Empty || SC_GC_Company == EnvProxy.Instance.CurrentCompany.PK;

		public virtual bool CompanyCode_ReadOnly => !IsCompanyBranchDepartmentSpecific;

		public bool IsCompanySpecific => !SC_GC_Company.IsEmpty;

		#endregion

		#region Branch

		public GlbBranchDependentCollection GlbBranchList => LoginObj.Branches;

		[List("GlbBranchList")]
		[MaxLength(3)]
		public ZString BranchCode
		{
			get => LoginObj.BranchCode;
			set
			{
				var oldValue = LoginObj.BranchCode;
				LoginObj.BranchCode = value;
				SC_GB_Branch = LoginObj.Branch == null ? ZGuid.Empty : LoginObj.Branch.PK;
				if (!IsValidationSuspended)
				{
					((StorageDocsBaseValidation)Validation).ValidateBranchCode();
				}
				BranchCodeInfo.RefreshBinding(oldValue);
			}
		}

		public override ZGuid SC_GB_Branch
		{
			get => base.SC_GB_Branch;
			set
			{
				if (base.SC_GB_Branch != value)
				{
					base.SC_GB_Branch = value;
					if (!SC_GB_Branch.IsEmpty)
					{
						var branch = Factory.Load<GlbBranch>(SC_GB_Branch);
						if (branch != null && branch.GB_Code != LoginObj.BranchCode)
						{
							LoginObj.BranchCode = branch.GB_Code;
						}
					}
					else
					{
						LoginObj.BranchCode = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo BranchCodeInfo => GetZPropertyInfo(nameof(BranchCode));

		public bool IsBelongingToCurrentLoginBranch => SC_GB_Branch == ZGuid.Empty || SC_GB_Branch == EnvProxy.Instance.CurrentBranch.PK;

		public virtual bool BranchCode_ReadOnly => !IsCompanyBranchDepartmentSpecific;

		public bool IsBranchSpecific => !SC_GB_Branch.IsEmpty;

		#endregion

		#region Department

		public GlbDepartmentCollection GlbDepartmentList => LoginObj.Departments;

		[List("GlbDepartmentList")]
		[MaxLength(3)]
		public ZString DepartmentCode
		{
			get => LoginObj.DepartmentCode;
			set
			{
				var oldValue = LoginObj.DepartmentCode;
				LoginObj.DepartmentCode = value;
				SC_GE_Department = LoginObj.Department == null ? ZGuid.Empty : LoginObj.Department.PK;
				if (!IsValidationSuspended)
				{
					((StorageDocsBaseValidation)Validation).ValidateDepartmentCode();
				}
				DepartmentCodeInfo.RefreshBinding(oldValue);
			}
		}

		public override ZGuid SC_GE_Department
		{
			get => base.SC_GE_Department;
			set
			{
				if (base.SC_GE_Department != value)
				{
					base.SC_GE_Department = value;
					if (!SC_GE_Department.IsEmpty)
					{
						var department = Factory.Load<GlbDepartment>(SC_GE_Department);
						if (department != null && department.GE_Code != LoginObj.DepartmentCode)
						{
							LoginObj.DepartmentCode = department.GE_Code;
						}
					}
					else
					{
						LoginObj.DepartmentCode = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo DepartmentCodeInfo => GetZPropertyInfo(nameof(DepartmentCode));

		public bool IsBelongingToCurrentLoginDepartment => SC_GE_Department == ZGuid.Empty || SC_GE_Department == EnvProxy.Instance.CurrentDepartment.PK;

		public virtual bool DepartmentCode_ReadOnly => !IsCompanyBranchDepartmentSpecific;

		public bool IsDepartmentSpecific => !SC_GE_Department.IsEmpty;

		#endregion

		ZString IeDoc.VisibleCompanyCode => CompanyCode;

		ZString IeDoc.VisibleBranchCode => BranchCode;

		ZString IeDoc.VisibleDepartmentCode => DepartmentCode;

		ZBool IeDoc.IsCustomisableDocTypes => !SC_Desc_ReadOnly;

		#endregion

		#region SC_FileNameForRenaming

		public void SetDefaultSC_FileNameForRenaming()
		{
			fSC_FileNameForRenaming = IsImageFile ? SC_FileName : SC_FileNameWithExtension;
		}

		[MaxLength(StorageDocs.Schema.SC_FileNameMaxLength + StorageDocs.Schema.SC_DataTypeMaxLength + 1)]
		public ZString SC_FileNameForRenaming
		{
			get { return fSC_FileNameForRenaming; }
			set
			{
				if (fSC_FileNameForRenaming != value)
				{
					CheckMaximumLength(SC_FileNameForRenamingInfo, value);
					SetNonPersistentPropertyValue(SC_FileNameForRenamingInfo, ref fSC_FileNameForRenaming, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSC_FileNameForRenaming();
					}
				}
			}
		}

		ZString fSC_FileNameForRenaming;

		public ZPropertyInfo SC_FileNameForRenamingInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.SC_FileNameForRenamingSchemaName);
			}
		}
		public abstract void SetNewFileName(string fileName);

		#endregion

		internal bool IsPrivateDocument => IsInternalDocType(SC_DocType);

		internal static bool IsInternalDocType(string docType)
		{
			return (docType == Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument
				|| docType == Core.Constants.RefDocTypes.InternallyCreatedPublicDocument);
		}

		public override bool SupportsNotes => false;

		protected override BusinessObjectFactory LogsFactory => MasterFactory;

		public bool IsObsolete
		{
			[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
			get { return !HasChanges && IsInDatabase && (SC_DocType.IsEmpty || SC_Desc.IsEmpty); }
		}

		public ZString TempFileName => tempFileName;
		protected string tempFileName;
		public ZDateTime TempFileLastWriteTime => tempFileLastWriteTime;
		protected DateTime tempFileLastWriteTime;

		public bool IsReadByUserInThisSession => isReadByUserInThisSession;
		bool isReadByUserInThisSession;

		public bool IsSupersededByNewVersion { get; set; }

		protected TimeFactory TimeFactory
		{
			get
			{
				if (timeFactory == null)
				{
					timeFactory = new TimeFactory();
				}
				return timeFactory;
			}
		}
		TimeFactory timeFactory;

		[MaxLength(3)]
		public virtual ZString SM_Type
		{
			get => ParentMain != null ? ParentMain.SM_Type : ZString.Empty;

			set
			{
				if (ParentMain != null)
				{
					ParentMain.SM_Type = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateSC_DocType();
					}

					if (ShouldUpdateDescAndPublishedFlagsWhileSettingDocType)
					{
						if (GetDescReadOnlyStatus() && SC_DocType != Core.Constants.RefDocTypes.MiscellaneousDocument)
						{
							UpdateDescriptionField(GetDocumentTypeLookupList());
						}

						UpdatePublishedAndSaveVersionFlags();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateSC_Desc();
					}
				}
			}
		}

		public virtual ZPropertyInfo SM_TypeInfo
		{
			get { return ParentMain == null ? GetZPropertyInfo(nameof(SM_Type)) : GetWrappedZPropertyInfo(nameof(SM_Type), x => ParentMain.SM_TypeInfo); }
		}

		public virtual StorageMain ParentMain
		{
			get
			{
				if (!IsDeleted && (fParentMain == null || fParentMain.PK != SC_SM))
				{
					UnRegisterListChangedCalledRefreshBinding(fParentMain);
					fParentMain = MasterFactory.Load<StorageMain>(SC_SM);
					RegisterListChangedCalledRefreshBinding(fParentMain);
				}
				return fParentMain;
			}
		}
		StorageMain fParentMain;

		public DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = ((NumberedBusinessObjectFactory)Factory).MasterFactory;
				}
				return fMasterFactory;
			}
		}

		DocumentFactory fMasterFactory;

		public RefDocType DocType => FindDocType(SC_DocType);

		RefDocType FindDocType(string docType)
		{
			if (string.IsNullOrEmpty(docType))
			{
				return null;
			}

			var query = new ZQuery(RefDocTypeSchema.RT_DocType, docType)
				.AddToFilter(GetDocTypeCategoryQuery(), JoinCondition.And);

			return MasterFactory.LoadTop1<RefDocType>(query);
		}

		public ZString SC_AddingUser => SC_SystemCreateUser;

		public ZPropertyInfo SC_AddingUserInfo => GetZPropertyInfo(nameof(SC_AddingUser));

		public ZString SC_SystemCreateUserFullName =>
			((IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, SC_SystemCreateUser))?.FullName;

		public ZPropertyInfo SC_SystemCreateUserFullNameInfo => GetZPropertyInfo(nameof(SC_SystemCreateUserFullName));
		
		public ZString SC_LastEditingUser => SC_SystemLastEditUser;

		public ZPropertyInfo SC_LastEditingUserInfo => GetZPropertyInfo(nameof(SC_LastEditingUser));

		public ZString SC_LastEditingUserFullName =>
			((IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, SC_SystemLastEditUser))?.FullName;

		public ZPropertyInfo SC_LastEditingUserFullNameInfo => GetZPropertyInfo(nameof(SC_LastEditingUserFullName));

		[BusinessObjectTestExclude] // new changes to prevent setting in OnLoaded means that this property won't test correctly in TestBizObjFields
		public override ZString SC_DocType
		{
			get => base.SC_DocType;
			set
			{
				if (base.SC_DocType != value)
				{
					base.SC_DocType = value;

					UpdateDescriptionIfRequired();
					UpdateParseType();
				}

				ApplyDefaultCompanyBranchDepartment();
			}
		}

		public override ZString SC_DataType
		{
			get => base.SC_DataType;
			set
			{
				var oldDataType = base.SC_DataType;
				base.SC_DataType = value;

				// Reset IsParsingEnabled when SC_DataType changes
				if (oldDataType != value)
				{
					IsParsingEnabled = IsParsingSupported;
				}
			}
		}

		public override ZString SC_RDS_NKDocSource
		{
			get => base.SC_RDS_NKDocSource;
			set
			{
				if (base.SC_RDS_NKDocSource != value)
				{
					base.SC_RDS_NKDocSource = value;
					UpdateDescriptionIfRequired();
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateParseType();
		}

#if DEBUG
		public
#endif
		void UpdateParseType()
		{
			var parseType = DocType?.RT_ParseType ?? ZString.Empty;
			ParseType = !parseType.IsEmpty && EDocsParsingHelper.IsParseTypeEnabled(parseType) ? parseType : ZString.Empty;
		}

		public ZString ParseType
		{
			get
			{
				if (string.IsNullOrEmpty(parseType))
				{
					UpdateParseType();
				}
				return parseType;
			}

#if !DEBUG
			private
#endif
			set
			{
				// Only update parse type when either parseType is not empty or value is not empty.
				// This would prevent the unnecessary update of ParseType and IsParsingEnabled when value is changed from null to empty string.
				if ((!string.IsNullOrEmpty(parseType) || !string.IsNullOrEmpty(value)) && parseType != value)
				{
					var oldParseType = parseType;
					parseType = value;
					ParseTypeInfo.RefreshBinding();

					if (EDocsParsingHelper.IsDocumentParsingEnabled())
					{
						// Initialise isParsingEnabled when parseType is initialised or reset IsParsingEnabled when ParseType changes
						IsParsingEnabled = oldParseType == null ? IsParsingEnabledOriginalValue : IsParsingSupported;
					}
				}
			}
		}
		string parseType;

		public bool ParseType_ReadOnly => true;

		public ZPropertyInfo ParseTypeInfo => GetZPropertyInfo(nameof(ParseType));

		bool IsParsingSupported => EDocsParsingHelper.IsDocumentParsingEnabled() && !ParseType.IsEmpty && IsDataTypeValidForParsing;

		internal bool IsDataTypeValidForParsing => SupportedDataTypes.Any(s => s.Equals(SC_DataType, StringComparison.OrdinalIgnoreCase));

		[BusinessObjectTestExclude]
		public ZBool IsParsingEnabled
		{
			get
			{
				return IsParsingSupported && isParsingEnabled;
			}

			set
			{
				if (isParsingEnabled != value)
				{
					isParsingEnabled = value;
					IsParsingEnabledInfo.RefreshBinding();
				}

				// Validate even if IsParsingEnabled value is not changed, e.g.
				// in Edit Properties form, we change doc type from MSC to CIV while file type is txt,
				// IsParsingEnabled is not changed, but we should validate it to show the warning message for CIV.
				if (!IsValidationSuspended)
				{
					((StorageDocsBaseValidation)Validation).ValidateIsParsingEnabled();
				}
			}
		}
		ZBool isParsingEnabled;

		public ZBool IsParsingEnabledValue => isParsingEnabled;

		ZBool IsParsingEnabledOriginalValue =>
			(ActiveShipamaxMessage == null || ActiveShipamaxMessage.EM_Status != EDIMessageStatusList.Codes.Cancelled) && !IsParsingDeniedByDocumentOwner;

		public bool IsParsingEnabled_ReadOnly => !IsParsingSupported || IsParsingNotRequired;

		public ZPropertyInfo IsParsingEnabledInfo => GetZPropertyInfo(nameof(IsParsingEnabled));

		ZBool IsParsingNotRequired => !IsParsingRequiredForNewOrDocChanges && IsDocParsingInProgressOrComplete;

		ZBool IsParsingRequiredForNewOrDocChanges =>
			!IsInDatabase || (SC_IsDeletedInfo.HasChanges && SC_IsDeleted == true) || SC_DocTypeInfo.HasChanges || SC_DataTypeInfo.HasChanges || hasSC_ImageDataBeenUpdated;

		ZBool IsDocParsingInProgressOrComplete =>
			ActiveShipamaxMessage != null &&
				(ActiveShipamaxMessage.EM_Status == EDIMessageStatusList.Codes.ProcessedOK ||
				ActiveShipamaxMessage.EM_Status == EDIMessageStatusList.Codes.PreProcessedOK ||
				ActiveShipamaxMessage.EM_Status == EDIMessageStatusList.Codes.Sent);

		public override ZBool SC_IsPublished
		{
			get => base.SC_IsPublished;
			set
			{
				SetPropertyValue(SC_IsPublishedInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSC_IsPublished();
					Validation.ValidateSC_DocType();
				}
			}
		}

		public bool ShouldPrintByDefault { get; set; }

		public bool SuppressDocEventLog { get; set; }

		public IEDocsParsingSupport EDocsParsingSupport
		{
			get
			{
				return eDocsParsingSupport ?? ParentMain?.DocumentOwner as IEDocsParsingSupport;
			}

#if !DEBUG
	private
#endif
			set
			{
				eDocsParsingSupport = value;
			}
		}

		IEDocsParsingSupport eDocsParsingSupport;

		internal bool IsParsingDeniedByDocumentOwner => EDocsParsingSupport?.DenySendForParsing(PK.ToGuid(), SC_DocType, SC_FileNameWithExtension) ?? false;

		void UpdateParentDocEvent()
		{
			var parentMain = ParentMain;
			if (parentMain != null && !parentMain.IsDeleted && !parentMain.IsArchiving && parentMain.DocumentOwner is IStmALogParent logOwner)
			{
				var needsNewUpdateLog = CancelPreviousEventIfNeeded(logOwner, false);
				if (needsNewUpdateLog && !SuppressDocEventLog && !IsDeleted && !SC_IsDeleted)
				{
					var docType = DocType;
					if (logOwner != null && !string.IsNullOrEmpty(docType?.RT_SE_NKDocumentReceivedEvent))
					{
						var eventToAdd = AutoEvents.All[docType.RT_SE_NKDocumentReceivedEvent];
						if (eventToAdd != null)
						{
							var reference = PK.ToString();
							if (!string.IsNullOrEmpty(docType.RT_LogMacro))
							{
								reference += "|" + EvaluateMacro(this);
							}

							previousEventAdded = logOwner.Logs.AddNew(eventToAdd, reference);
						}
					}
				}
			}
		}

		StmALog previousEventAdded;
		bool CancelPreviousEventIfNeeded(IStmALogParent logParent, bool isForDelete)
		{
			if (previousEventAdded == null)
			{
				if (!IsInDatabase)
				{
					return true;
				}

				previousEventAdded = FindPreviousEvent(logParent, Convert.ToString(SC_DocTypeInfo.OriginalValue, CultureInfo.InvariantCulture));
				if (previousEventAdded == null)
				{
					return true;
				}
			}

			var isDifferentEvent = false;
			var hasDifferentParent = false;

			bool isRowDetached = ((INeedRow)previousEventAdded).Row.RowState == DataRowState.Detached;
			if (!isRowDetached)
			{
				isDifferentEvent = !Equals(DocType?.RT_SE_NKDocumentReceivedEvent, previousEventAdded.SL_SE_NKEvent);
				hasDifferentParent = !Equals(ParentMain.SM_ParentFK, previousEventAdded.SL_Parent);
			}
			if (isDifferentEvent || hasDifferentParent || isForDelete || isRowDetached)
			{
				if (previousEventAdded.IsInDatabase)
				{
					previousEventAdded.Cancel();
				}
				else
				{
					previousEventAdded.Delete();
					previousEventAdded = null;
				}
				return true;
			}

			return false;
		}

		StmALog FindPreviousEvent(IStmALogParent logParent, string originalDocType)
		{
			if (!string.IsNullOrEmpty(originalDocType) && IsInDatabase)
			{
				var docType = FindDocType(originalDocType);

				if (!string.IsNullOrEmpty(docType?.RT_SE_NKDocumentReceivedEvent))
				{
					var eventQuery = new ZQuery(StmALogSchema.SL_Parent, logParent.LogsParentPK)
						.AddToFilter(StmALogSchema.SL_SE_NKEvent, docType.RT_SE_NKDocumentReceivedEvent)
						.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, PK.ToString());

					return MasterFactory.Load<StmALog>(eventQuery).FirstOrDefault();
				}
			}

			return null;
		}

		string EvaluateMacro(object dataSource)
		{
			var result = DocType.EvaluateLogMacro(dataSource);

			if (result.Item2.Any())
			{
				NotifyUserOfBadMacro(DocType, result.Item2);
			}

			return result.Item1;
		}

		#region SuppressResourceStringsCheckRegion

		void NotifyUserOfBadMacro(RefDocType docType, IEnumerable<ErrorMessage> errors)
		{
			var sb = new StringBuilder();
			sb.AppendFormat(CultureInfo.InvariantCulture, "There was a problem evaluating the Log Macro for the document type '{0}' with the category of '{1}'.", docType.RT_DocType, docType.RT_ReferenceType).AppendLine();
			sb.AppendFormat(CultureInfo.InvariantCulture, "The macro is: '{0}'", docType.RT_LogMacro).AppendLine();
			sb.AppendLine("The errors are: ");
			foreach (var error in errors)
			{
				sb.AppendFormat("{0}: {1}", error.Category, error.Message).AppendLine();
			}
			sb.AppendLine();
			sb.AppendFormat(CultureInfo.InvariantCulture, "The {0} event has been created, but the reference may not be what is expected. This may effect any triggers you have for that event. Please repair the problematic macro.", docType.RT_SE_NKDocumentReceivedEvent);

			var subject = string.Format(CultureInfo.InvariantCulture, "Error evaluating macro for document type {0}", docType.RT_DocType);

			try
			{
				var emailKey = string.Join("|", new object[] { "BadMacro", docType.PK, docType.RT_LogMacro });
				UnattendedUserNotification.Instance.ShowErrorOnceADay(emailKey, sb.ToString(), subject, sendToPostMaster: true);
			}
			catch (EmailHasNoRecipientsException) { } // Nothing we can do

			if (Globals.IsUserInteractive && !Db.Connection.IsInTransaction)
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("ef99bfbe-422c-47ea-a53f-eef963d80a40", "There was a problem creating the reference for the event which was added when adding this document. This may effect any associated triggers. You may need to amend the event's reference manually."));
			}
		}

		#endregion

		void ApplyDefaultCompanyBranchDepartment()
		{
			if (DocType != null && !IsCompanyBranchDepartmentSpecific)
			{
				if (DocType.RT_IsCompanySpecific)
				{
					CompanyCode = GlbCompany.CurrentCompany.GC_Code;
				}
				if (DocType.RT_IsBranchSpecific)
				{
					BranchCode = GlbBranch.CurrentBranch.GB_Code;
				}
				if (DocType.RT_IsDepartmentSpecific)
				{
					DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
				}
			}
		}

		[MaxLength(50)]
		public ZString SC_DocType_Description
		{
			get
			{
				if (SC_DocType_List.ContainsCode(SC_DocType))
				{
					return SC_DocType_List.GetDescriptionFromCode(SC_DocType);
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo SC_DocType_DescriptionInfo => GetZPropertyInfo(nameof(SC_DocType_Description));

		[MaxLength(RefDocSource.Schema.RDS_DescMaxLength)]
		public ZString SC_DocSource_Description
		{
			get
			{
				if (SC_DocSource_List.ContainsCode(SC_RDS_NKDocSource))
				{
					return SC_DocSource_List.GetDescriptionFromCode(SC_RDS_NKDocSource);
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo SC_DocSource_DescriptionInfo => GetZPropertyInfo(nameof(SC_DocSource_Description));

		protected virtual bool ShouldUpdateDescAndPublishedFlagsWhileSettingDocType => true;

		#region Suspend security check for DocTypes

		bool suspendCheckSecurityRightsForDocTypes;
		internal IDisposable SuspendCheckSecurityRightsForDocTypes()
		{
			docTypeList = null;
			suspendCheckSecurityRightsForDocTypes = true;
			return new DisposableAction(() =>
			{
				suspendCheckSecurityRightsForDocTypes = false;
				docTypeList = null;
			});
		}

		#endregion

		public bool SC_Desc_ReadOnly => SC_DocType.IsEmpty || GetDescReadOnlyStatus();

		public ZDateTime SC_DateLocalBranchTime
		{
			get
			{
				var env = EnvProxy.Instance;
				var result = (SC_Date.IsValid) ?
				env.Time.GetLocalTimeFromUtc(SC_Date.ToDateTime()) :
				ZDateTime.Empty;
				return result;
			}
		}

		public bool SC_IsPublished_ReadOnly => (DocType != null) ? !DocType.RT_IsPublishUpdatable : SC_IsPublishedReadonlyDefault;

		public override ZBlob SC_ImageData
		{
			get
			{
				if (IsVirusDetected)
				{
					throw new VirusDetectedException(SC_FileNameWithExtension);
				}

				var blob = base.SC_ImageData;

				if (IsVirusScanningEnabled && IsVirusScanningRequiredForGetter)
				{
					DoVirusScanningForGetter(blob);
				}

				return blob;
			}

			set
			{
				if (IsVirusScanningEnabled)
				{
					DoVirusScanningForSetter(value);
				}

				using (SuspendVirusScanningForGetter())
				{
					base.SC_ImageData = value;
				}

				if (ShouldDeleteTempFile)
				{
					DeleteTempFile();
				}
			}
		}

		protected virtual bool ShouldDeleteTempFile => true;

		public bool RequiresUserToRead
		{
			get
			{
				var result = false;

				var documentType = DocType;
				var parent = ParentMain;

				if (documentType != null
						&& parent != null
						&& parent.DocumentOwner != null
						&& documentType.RT_ForceUserToRead
						&& SC_AddingUser != GlbStaff.CurrentUser.GS_Code)
				{
					var ownerLogs = ParentMain.DocumentOwner.GetLogs();

					if (ownerLogs != null)
					{
						var readDocumentsLog = ownerLogs.MostRecentLogByEventTime(Events.RelatedEDocsRead, new ZQuery(StmALogSchema.SL_GS_NKUser, GlbStaff.CurrentUser.GS_Code));

						if (readDocumentsLog == null || SC_Date > readDocumentsLog.SL_PostedTimeUtc)
						{
							result = true;
						}
					}
				}

				return result;
			}
		}

		public bool IsUnallocated => !IsAllocated;

		public virtual bool IsAllocated => ParentMain != null && !ParentMain.SM_ParentFK.IsEmpty;

		[MaxLength(256)]
		public ZString SC_FriendlyFileDescription
		{
			get
			{
				if (friendlyFileDescription.IsEmpty || SC_DataType != dataTypeForFriendlyFileDescription)
				{
					dataTypeForFriendlyFileDescription = SC_DataType;
					friendlyFileDescription = FileAssociationInfo.GetFriendlyDocumentName(SC_DataType);
				}
				return friendlyFileDescription;
			}
		}
		ZString friendlyFileDescription;
		ZString dataTypeForFriendlyFileDescription;

		public ZPropertyInfo SC_FriendlyFileDescriptionInfo => GetZPropertyInfo(Schema.SC_FriendlyFileDescriptionSchemaName);

		[MaxLength(StorageDocs.Schema.SC_FileNameMaxLength + StorageDocs.Schema.SC_DataTypeMaxLength + 1)]
		public ZString SC_FileNameWithExtension
		{
			get
			{
				if (SC_DataType.IsEmpty)
				{
					return SC_FileName.Trim();
				}
				else
				{
					return SC_FileName.Trim() + '.' + SC_DataType.ToLower();
				}
			}
		}

		public ZPropertyInfo SC_FileNameWithExtensionInfo => GetZPropertyInfo(Schema.SC_FileNameWithExtensionSchemaName);

		FileAssociationRetriever FileAssociationInfo
		{
			get
			{
				if (fileAssociationInfo == null)
				{
					fileAssociationInfo = new FileAssociationRetriever();
				}
				return fileAssociationInfo;
			}
		}
		FileAssociationRetriever fileAssociationInfo;

		public virtual ZString EDocFormat => SC_DataType;

		[LinkedTranslatableDataField(typeof(RefDocType), RefDocType.Schema.RT_Desc, true)]
		public override ZString SC_Desc
		{
			get => base.SC_Desc;
			set => base.SC_Desc = value;
		}

		[MaxLength(Schema.SC_DescMaxLength)]
		public MultilingualString SC_DescMultilingual
		{
			get => GetMultilingual(SC_DescInfo);
			set => SC_Desc = value;
		}

		public ZPropertyInfo SC_DescMultilingualInfo => SC_DescInfo;

		public bool SC_DescMultilingual_ReadOnly => SC_DescInfo.ReadOnly;

		public override bool CanDelete => base.CanDelete && !HasEDIMessage;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;

				if (result.IsEmpty && !CanDelete)
				{
					result = ResString.GetMultilingualString("aa5fd997-f03b-4606-b0c3-01d7f9b8a5ce", "There are EDI Messages that have this eDocs file as attachment. You cannot delete this eDocs now.");
				}
				return result;
			}
		}

		bool HasEDIMessage
		{
			get
			{
				var query = new ZQuery(EDIMessageAttachSchema.EG_StorageDocsGuid, PK);
				return MasterFactory.LoadTop1<EDIMessageAttach>(query) != null;
			}
		}

#endregion

		#region Virus Scan

		public int LastVirusScanResult { get; set; } = Constants.VirusScanResult.NotScan;

		public bool IsVirusDetected => LastVirusScanResult == Constants.VirusScanResult.Detected;

		bool IsVirusScanningRequiredForGetter => !suspendVirusScanningForGetter && LastVirusScanResult == Constants.VirusScanResult.NotScan;

		bool IsVirusScanningEnabled => DocManagerRegistry.Instance.EnableEDocsVirusScanning.Value;

		bool suspendVirusScanningForGetter;
		IDisposable SuspendVirusScanningForGetter()
		{
			suspendVirusScanningForGetter = true;

			return new DisposableAction(() => suspendVirusScanningForGetter = false);
		}

		void DoVirusScanningForGetter(ZBlob blob)
		{
			if (AmsiHelper.IsMalware(SC_FileNameWithExtension, blob))
			{
				LastVirusScanResult = Constants.VirusScanResult.Detected;
				throw new VirusDetectedException(SC_FileNameWithExtension);
			}

			LastVirusScanResult = Constants.VirusScanResult.NotDetected;
		}

		void DoVirusScanningForSetter(ZBlob blob)
		{
			if (AmsiHelper.IsMalware(SC_FileNameWithExtension, blob))
			{
				if (!IsInDatabase)
				{
					SC_IsDeleted = true;
				}

				throw new VirusDetectedException(SC_FileNameWithExtension);
			}
		}

		#endregion

		#region Interface implementations

		#region IDeliverable Members

		ZString IDeliverable.DocumentDeliveredEventCode => "";
		ZString IDeliverable.DocumentPasswordInformationEventCode => "";

		ZBool includedInPrint = true;
		StmMenuItem menuItem;

		public DocDeliveryPrintDetails PrinterDetails => null;

		protected abstract DeliveryInfo.DeliveryFormats DeliveryFormats { get; }

		public ZString DeliveryMode => string.Join(",", GetSupportedDeliveryMethods());

		public ZPropertyInfo DeliveryModeInfo => GetZPropertyInfo(nameof(DeliveryMode));

		public ZString AllAvailableDeliveryModes => string.Join(",", GetSupportedDeliveryMethodsCore());

		public bool SupportsDeliveryMethod(string deliveryMethod)
		{
			return !string.IsNullOrEmpty(deliveryMethod) && GetSupportedDeliveryMethods().Contains(deliveryMethod.ToUpperInvariant());
		}

		StmMenuEDocs GetMenuEDocs()
		{
			if (menuItem != null && menuItem is StmMenuItemBase menuBase)
			{
				return menuBase.EDocsView.OfType<StmMenuEDocs>().FirstOrDefault(m => m.DocType != null && m.DocType.RT_DocType == DocumentTypeCode);
			}

			return null;
		}

		public IEnumerable<string> GetSupportedDeliveryMethods()
		{
			var result = GetSupportedDeliveryMethodsCore();
			var menuEDocs = GetMenuEDocs();
			if (menuEDocs != null && menuEDocs.SX_PrintCopyType != nameof(PrintCopyType.ALL))
			{
				result = result.Where(r => r == menuEDocs.SX_PrintCopyType);
			}
			return result;
		}

		protected abstract IEnumerable<string> GetSupportedDeliveryMethodsCore();

		public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType()
		{
			return GetSupportedDeliveryMethodsCore();
		}

		public ZString DocumentTypeCode => SC_DocType;

		public ZPropertyInfo DocumentTypeCodeInfo => GetWrappedZPropertyInfo(nameof(DocumentTypeCode), _ => SC_DocTypeInfo);

		public ZString DocumentTypeDescription => SC_DocType_Description;

		public ZPropertyInfo DocumentTypeDescriptionInfo => GetWrappedZPropertyInfo(nameof(DocumentTypeDescription), _ => SC_DocType_DescriptionInfo);

		public ZString DocumentSourceDescription => SC_DocSource_Description;

		public ZPropertyInfo DocumentSourceDescriptionInfo => GetWrappedZPropertyInfo(nameof(DocumentSourceDescription), _ => SC_DocSource_DescriptionInfo);

		ZString IDeliverable.FileExtension => HackedFileExtensionOnlyForIDeliverable;

		/// <summary>
		/// This is a hacked property to return TIF in StorageDocs override. This hack is only for IDeliverable interface.
		/// We should not use this property in any other place.
		/// We need this hack because DOD service in DocEngine can only add images with TIF extension to emails.
		/// </summary>
		protected abstract ZString HackedFileExtensionOnlyForIDeliverable { get; }

		ZGuid IDeliverable.DeliveryGroupID { get; set; }

		public ZBool IncludedInPrint
		{
			get => includedInPrint;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (includedInPrint != value)
					{
						SetNonPersistentPropertyValue(IncludedInPrintInfo, ref includedInPrint, value);

						var collection = ((IBusinessObjectInternals)this).ParentCollections;
						collection?.OfType<DeliverableCollectionView>().FirstOrDefault()?.ValidateDeliveryMethodForRecipients();

						((StorageDocsBaseValidation)Validation).ValidateIncludedInPrint();

						ValidateAllIndexes();
					}
				}
			}
		}

		public ZPropertyInfo IncludedInPrintInfo => GetZPropertyInfo(nameof(IncludedInPrint));

		public bool IncludedInPrint_ReadOnly { get; set; }

		public ZByte Index
		{
			get
			{
				return index;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (index != value)
					{
						SetNonPersistentPropertyValue(IndexInfo, ref index, value);

						ValidateAllIndexes();
					}
				}
			}
		}
		ZByte index;

		public ZPropertyInfo IndexInfo => GetZPropertyInfo(nameof(Index));

		internal EDocCollectionView EDocsToBeDelivered => ((IBusinessObjectInternals)this).ParentCollections?.OfType<EDocCollectionView>().FirstOrDefault();

		void ValidateAllIndexes()
		{
			if (!IsValidationSuspended)
			{
				EDocsToBeDelivered?.Cast<StorageDocsBase>().ForEach(s => ((StorageDocsBaseValidation)s.Validation).ValidateIndex());
			}
		}

		bool IDeliverable.IsDeliveredByEmail { get; set; }

		StmMenuItem IDeliverable.MenuItem
		{
			get => menuItem;
			set => menuItem = value;
		}

		public ZString Name => NameCore;

		protected abstract ZString NameCore { get; }

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		public virtual ZString NameForBinding => Name;

		public ZString JobNumber => ParentMain != null ? ParentMain.ParentHumanReadableName : ZString.Empty;

		DeliveryInfo IDeliverable.GetDeliveryInfo(bool isDraft)
		{
			var result = new DeliveryInfo(DeliveryFormats);

			var parentAssemblyData = (ParentMain != null && ParentMain.OwnerAssemblyData != null) ?
				ParentMain.OwnerAssemblyData : AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Unallocated);

			result.ParentTableName = BusinessObjectFactory.GetTableNameFromType(parentAssemblyData.BusinessObjectType);
			result.ParentGuid = ParentMain != null ? ParentMain.SM_ParentFK : ZGuid.Empty;
			result.FileContents.Write((byte[])SC_ImageData, 0, SC_ImageData.Length);

			var description = SC_DescMultilingual.IsEmpty ? Res.GetString("Document", "Document") : SC_DescMultilingual;
			string emailSubjectLine;
			if (SC_IsSystemGenerated)
			{
				emailSubjectLine = description;
			}
			else
			{
				EmailFormatter.UpdateDocumentName(description);
				var bizObjCode = ParentMain != null && !ParentMain.DocumentOwnerCode.IsEmpty ? " - " + ParentMain.DocumentOwnerCode : string.Empty;
				emailSubjectLine = EmailFormatter.GetEmailSubjectLine(RegistryEmailFormat.EmailSubjectFields) + bizObjCode;
			}

			result.EmailSubjectLine = emailSubjectLine;
			result.EmailSignature = EmailFormatter.GetEmailSignature(RegistryEmailFormat.EmailSignatureFields);
			result.Name = SC_FileNameWithExtension;
			result.AttachedFilename = SC_FileName;
			result.FileFormat = HackedFileExtensionOnlyForIDeliverable;
			result.DocumentType = SC_DocType;

			return result;
		}

		#region Email Formatter

		EmailFormatter EmailFormatter => emailFormatter ?? (emailFormatter = new EmailFormatter(GlbStaff.CurrentUser));
		EmailFormatter emailFormatter;

		EmailFormat RegistryEmailFormat => registryEmailFormat ?? (registryEmailFormat = DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		EmailFormat registryEmailFormat;

		#endregion

		void IDeliverable.Save(DocDeliveryContact deliveryContact, DocDeliveryContact originalContact, Stream fileContent)
		{
#if DEBUG
			runCount++;
#endif
			fileContent.Position = 0;
			SaveToStream(fileContent);
		}

#if DEBUG
		int runCount;

		int IDeliverable.RunCountForTesting => runCount;

		void IDeliverable.DeleteTempFilesForTesting()
		{
		}
#endif

		ZBool IDeliverable.CoverSheetRequired => false;

		bool IDeliverable.ContainsDataRows => false;

		ZGuid IDeliverable.SourcePivotPK { get; set; }

		ZGuid IDeliverable.MenuTemplatePivotPK { get; }

		ZGuid IDeliverable.IdentifiablePK { get; }

		public ZString Identifier => PK.ToString();

		#endregion

		#region IDocument Members

		string IDocument.DocumentDeliveryMethod => DeliveryMode;

		string IDocument.DocumentName => Name;

		bool IDocument.IncludeInPrint => IncludedInPrint;

		bool IDocument.CanIncludeInPrint
		{
			get => canIncludeInPrint;
			set => canIncludeInPrint = value;
		}
		bool canIncludeInPrint = true;

		#endregion

		#region IeDoc Members

		ZDateTime IeDocBase.LastEdited => SC_SystemLastEditTimeUtc;

		ZString IeDocBase.LastEditedUser => SC_LastEditingUser;

		ZDateTime IeDocBase.DateAdded
		{
			get => SC_Date;
			set => SC_Date = value;
		}

		ZString IeDocBase.DocType
		{
			get => SC_DocType;
			set => SC_DocType = value;
		}

		ZString IeDocBase.DocSource
		{
			get => SC_RDS_NKDocSource;
			set => SC_RDS_NKDocSource = value;
		}

		ZString IeDocBase.DocSourceDescription
		{
			get => SC_DocSource_Description;
		}

		ZString IeDocBase.Description
		{
			get => SC_DescMultilingual;
			set => SC_Desc = value;
		}

		ZString IeDocBase.FileName
		{
			get
			{
				var result = SC_FileNameWithExtension;
				if (IsImageFile && ((IeDocBase)this).FileNameOnly.IsEmpty)
				{
					result = SC_DescMultilingual + result;
				}
				return result;
			}
		}

		ZGuid IeDocBase.UniqueKey => PK;

		CodeDescriptionPairList IeDoc.DocType_List => SC_DocType_List;

		ZBlob IeDocBase.ImageData
		{
			get => SC_ImageData;
			set => SC_ImageData = value;
		}

		ZBool IeDocBase.IsSystemGenerated => SC_IsSystemGenerated;

		ZBool IeDocBase.IsDeleted
		{
			get => SC_IsDeleted;
			set => SC_IsDeleted = value;
		}

		ZBool IeDocBase.IsPublished
		{
			get => SC_IsPublished;
			set => SC_IsPublished = value;
		}

		BusinessObject IeDoc.ParentMain => ParentMain;

		ZString IeDocBase.DataType => SC_DataType;

		void IeDoc.SetValuesForTest(ZDateTime dateTime, ZString dataType)
		{
			SC_Date = dateTime;
			SC_DataType = dataType;
		}

		ZString IeDocBase.FileNameOnly => SC_FileName;

		public override Stream GetSC_ImageDataReader()
		{
			if (IsVirusDetected)
			{
				throw new VirusDetectedException(SC_FileNameWithExtension);
			}

			var stream = base.GetSC_ImageDataReader();

			if (IsVirusScanningEnabled && IsVirusScanningRequiredForGetter)
			{
				var copiedStream = stream.ToByteArray();
				DoVirusScanningForGetter(copiedStream);
			}

			return stream;
		}

		public override void SetSC_ImageDataSource(IStreamSource source)
		{
			if (IsVirusScanningEnabled)
			{
				DoVirusScanningForSetter(source.GetStream().ToByteArray());
			}
			base.SetSC_ImageDataSource(source);
		}

		Stream IeDocBase.GetImageDataReader() => GetSC_ImageDataReader();

		void IeDocBase.SetImageDataStream(Stream stream)
		{
			if (stream == imageDataStream)
			{
				return;
			}

			imageDataStream = stream;
			imageDataStreamSource?.Dispose();
			imageDataStreamSource = new StreamSource(stream);
			SetSC_ImageDataSource(imageDataStreamSource);
		}

		Stream imageDataStream;
		StreamSource imageDataStreamSource;

		ZDecimal IeDoc.FileSizeInMB => (ZDecimal)((IDeliveryEmailAttachment)this).FileSizeInBytes / (ZDecimal)(1024 * 1024);

		#endregion

		#region IDeliveryEmailAttachment

		long IDeliveryEmailAttachment.FileSizeInBytes => FileSizeInBytes;

		public long FileSizeInBytes
		{
			get
			{
				if (SC_UncompressedSize != 0)
				{
					return SC_UncompressedSize;
				}

				if (SC_ImageDataFromDb.IsEmpty && IsInDatabase)
				{
					if (!retrievedFileSizeInBytes.HasValue)
					{
						try
						{
							retrievedFileSizeInBytes = ExternalPersister?.GetObjectSize(PK);
						}
						//Swallow external storage exception here in order to keep the UI thread running, this property will be called many times when user scrolling the grid, don't think showing a message box is a good idea.
						catch (Exception ex) when (ex is ExternalStorageException)
						{
						}
					}

					return retrievedFileSizeInBytes ?? 0L;
				}

				UpdateUncompressedSizeWithSuspendingSettingHasChanges(SC_ImageDataFromDb.Length);

				return SC_UncompressedSize;
			}
		}

		long? retrievedFileSizeInBytes;

		string IDeliveryEmailAttachment.FileName => Name;

		bool IDeliveryEmailAttachment.ShouldBeAttached => IncludedInPrint;

		#endregion

		public ZString HumanReadableAttachmentSize => AttachmentSize(FileSizeInBytes);
#if DEBUG
		public
#else
		internal
#endif
		static ZString AttachmentSize(double len)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			var order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				len = len / 1024;
				++order;
			}
			return string.Format(CultureInfo.InvariantCulture, "{0:0.#}{1}", len, sizes[order]);
		}

		#endregion

		#region opening file

		void OpenInFileSystem(bool isEditable)
		{
			if (TempFileName.IsEmpty || !File.Exists(TempFileName))
			{
				SaveToTempFile();
			}

			File.SetAttributes(TempFileName, (isEditable) ? FileAttributes.Normal : FileAttributes.ReadOnly);
			LaunchProcess();
		}

		protected SynchronizationContext fContext;
		protected FileSystemWatcher watcher;

		readonly object remoteFileLock = new ();

#if DEBUG
		virtual
#endif
		public IDisposable OpenForEdit()
		{
			if (UseRemoteFile)
			{
				try
				{
					lock (remoteFileLock)
					{
						if (remoteFile != null)
						{
							remoteFile.Open();
						}
						else
						{
							fContext = SynchronizationContext.Current;
							var imageData = ZBlob.Empty;
							Exception exceptionThrown = null;
							fContext.Send(_ =>
							{
								try
								{
									imageData = SC_ImageData;
								}
								catch (Exception ex)
								{
									exceptionThrown = ex;
								}
							}
							, null);

							if (exceptionThrown != null)
							{
								throw exceptionThrown;
							}

							remoteFile = ObjectFactory.Get<IRemoteFile>(nameof(IRemoteFile), StorageFile.ReplaceIllegalCharsInFileNameWithDash(SC_FileNameWithExtension), (byte[])imageData, ReadOnly, false);
							if (remoteFile.Open())
							{
								remoteFile.FileChanged += new EventHandler(remoteFile_FileChanged);
							}
							else
							{
								remoteFile = null;
								return new DisposableAction(() => { });
							}
						}
						return remoteFile;
					}
				}
				catch (OperationCanceledException)
				{
					RemoteDesktopConnectionError?.Invoke(this, EventArgs.Empty);
					return new DisposableAction(() => { });
				}
			}

			DisposeWatcher();

			OpenInFileSystem(!ReadOnly);
			if (!TempFileName.IsEmpty)
			{
				fContext = SynchronizationContext.Current;
				watcher = new FileSystemWatcher(Path.GetDirectoryName(TempFileName), Path.GetFileName(TempFileName));
				watcher.EnableRaisingEvents = true;
				watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
				watcher.Changed += watcher_Changed;
				watcher.Renamed += watcher_Changed;
			}

			return new DisposableAction(DisposeWatcher);
		}

		public event EventHandler RemoteDesktopConnectionError;

		void DisposeWatcher()
		{
			if (watcher != null)
			{
				watcher.Changed -= watcher_Changed;
				watcher.Renamed -= watcher_Changed;
				watcher.Dispose();
			}
		}

		void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (FileWatcherChangedHelper.IsFileReallyChanged(tempFileName, e.FullPath, tempFileLastWriteTime, e.ChangeType))
			{
				fContext.Post(FileChangedAction, null);
			}
		}

		void remoteFile_FileChanged(object sender, EventArgs e)
		{
			fContext.Post(FileChangedAction, null); 
		}

		void FileChangedAction(object state)
		{
			HasChanges = true;
			IsSC_ImageDataOutOfSync = true;

			if (ParentMain != null)
			{
				ParentMain.ShouldSetImageDataForAll = true;
			}
		}

#if DEBUG
		public virtual void ViewFileExceptionExpose(Exception ex)
		{ }
#endif
		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Remote Desktop Services integration is explcity implemented in this context.")]
		protected virtual void LaunchProcess()
		{
#if DEBUG
			if (!Globals.IsTest || Globals.GetIsUnitTestingProductionFunctionality())
#endif
			{
				var startInfo = new ProcessStartInfo(TempFileName);
				startInfo.UseShellExecute = true;

				try
				{
					if (!TempFileName.IsEmpty && File.Exists(TempFileName))
					{
						tempFileLastWriteTime = File.GetLastWriteTime(TempFileName);
					}

					Process.Start(startInfo);
				}
				catch (Win32Exception)
				{
					if (LaunchProcessThrowWin32Exception != null)
					{
						LaunchProcessThrowWin32Exception(this, new FilenameEventArgs(TempFileName));
					}
				}
			}
		}

		public event FilenameEventHandler LaunchProcessThrowWin32Exception;

		/// <summary>
		/// This method copies the data in the temp file to the SC_ImageData blob so it can be saved into the db
		/// </summary>
		public bool SetImageData()
		{
			var isSuccessful = true;
			var isSettingEmptyValue = false;
			var tempOrRemoteFileName = string.Empty;

			if (remoteFile != null && !IsTempFileOpen)
			{
				lock (remoteFileLock)
				{
					if (remoteFile != null)
					{
						try
						{
							var newValue = RetriveRemoteFileData();
							if (newValue != null && !newValue.IsEmpty)
							{
								SC_ImageData = newValue;
							}
							else
							{
								tempOrRemoteFileName = remoteFile.FileName;
								isSettingEmptyValue = true;
							}
						}
						catch (OperationCanceledException)
						{
							isSuccessful = false;
						}
					}

					IsSC_ImageDataOutOfSync = false;
				}
			}
			else if (File.Exists(TempFileName) && !IsTempFileOpen)
			{
				try
				{
					ZBlob newValue = DocumentUtilities.GetFileAsBytes(TempFileName);
					if (newValue != null && !newValue.IsEmpty)
					{
						SC_ImageData = newValue;
					}
					else
					{
						tempOrRemoteFileName = TempFileName;
						isSettingEmptyValue = true;
					}

					IsSC_ImageDataOutOfSync = false;
				}
				catch (FileAccessException)
				{
					isSuccessful = false;
				}
			}

			if (isSettingEmptyValue)
			{
				var remoteVersion = ObjectFactory.Get<IRemoteChannel>().RemoteVersion;
				var supportedClientVersion = ObjectFactory.Get<TerminalService>().IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;

				if (remoteFile != null && supportedClientVersion > remoteVersion && !Globals.IsWinzor)
				{
					Globals.Message.Show(Res.GetString("D95876DF-0050-488C-B75C-9BCC84615F90", "Failed to save document {0} to database. Your RemoteDesktopServices is not up to date, please upgrade to 1.3.5 or higher to fix this issue.", SC_FileName)); // This action happens when user double click to open and edit a document from the EDocs tab. Therefore there must be a GUI attached.
				}
				else
				{
					if (IsTempFileOpen)
					{
						Globals.Message.Show(Res.GetString("1e7ad9f0-9dfb-4093-b339-78ec54225be6", "One or more document(s) is still open or being used by another process. Please check and save the document(s) again.")); // This action happens when user double click to open and edit a document from the EDocs tab. Therefore there must be a GUI attached.
					}
					else
					{
						Globals.Message.Show(Res.GetString("2f979c66-4b76-4734-ae92-8649893eba34", "Failed to save document {0} to database because it is empty. This could happen because of network or IO failure. Please check and try to modify and save again.", SC_FileName)); // This action happens when user double click to open and edit a document from the EDocs tab. Therefore there must be a GUI attached.
					}

					isSuccessful = false;
					if (remoteFile != null && remoteFile.GetDoesExistStatus())
					{
						ErrorReporter.ReportOnce("Saving_Empty_SC_ImageData",
							string.Format($@"Trying to save an empty storage data.
Is remote file?: {(remoteFile != null ? (NoResString)"Yes" : (NoResString)"No")}
RemoteApp Version on Client: {remoteVersion}
Remote File Length: {(remoteFile != null ? remoteFile.OriginalFileData.Length : 0)}
IsTempFileOpen: {(remoteFile.GetIsOpenStatus() ? (NoResString)"Yes" : (NoResString)"No")}
Doc Type: {SC_DocType}
File Name: {SC_FileName}
Data Type: {SC_DataType}
Temp File Name: {tempOrRemoteFileName}"));
						RestoreOriginalImageData();
					}
				}
			}

			return isSuccessful;
		}

		ZBlob RetriveRemoteFileData()
		{
			var newValue = ZBlob.Empty;
			for (var i = 0; i < RemoteFileMaxRetriveTimes; i++)
			{
#if DEBUG
				remoteFileRetreivedTimes++;
#endif
				newValue = remoteFile.FetchFileData();
				if (!newValue.IsEmpty)
				{
					break;
				}

				if (i + 1 < RemoteFileMaxRetriveTimes)
				{
					Thread.Sleep(50);
				}
			}
			return newValue;
		}
#if DEBUG
		internal int remoteFileRetreivedTimes;
		internal
#endif
		const int RemoteFileMaxRetriveTimes = 3;

		void RestoreOriginalImageData()
		{
			if (IsInDatabase)
			{
				SC_ImageData = (ZBlob)SC_ImageDataInfo.OriginalValue;
			}
		}

		internal bool IsSC_ImageDataOutOfSync { get; set; }

		public bool IsTempFileOpen
		{
			get
			{
				if (remoteFile != null)
				{
					try
					{
						return remoteFile.GetIsOpenStatus();
					}
					catch (OperationCanceledException)
					{
						return true;
					}
				}
				else
				{
					var fileOpen = false;
					if (File.Exists(TempFileName))
					{
						try
						{
							using (var stream = File.Open(TempFileName, FileMode.Open, FileAccess.Read, FileShare.None))
							{
							}
						}
						catch (IOException)
						{
							fileOpen = true;
						}
					}
					return fileOpen;
				}
			}
		}
#if DEBUG
		public
#else
		internal
#endif
		bool UseRemoteFile
		{
			get
			{
				try
				{
					using (var remoteFile = ObjectFactory.Get<IRemoteFile>())
					{
						return remoteFile.RemoteFilesSupported;
					}
				}
				catch (OperationCanceledException)
				{
					return false;
				}
				catch (ArgumentException)
				{
					return false;
				}
			}
		}
		IRemoteFile remoteFile;

		#endregion

		#region Shipamax Integration

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule

		List<string> supportedDataTypes;
		internal List<string> SupportedDataTypes
		{
			get
			{
				if (supportedDataTypes == null)
				{
					var dashService = ObjectFactory.Get<IDashParametersService>();
					supportedDataTypes = dashService.SupportedDataTypes;
				}
				return supportedDataTypes;
			}
		}

		static readonly List<string> NotSupportedDocManagerCodes = [Core.Constants.DocManagerCodes.InterchangeAttachments];

#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

		public EDocsShipamaxMessage ActiveShipamaxMessage
		{
			get
			{
				if (shipamaxMessage == null)
				{
					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
					query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
					shipamaxMessage = MasterFactory.LoadTop1<EDocsShipamaxMessage>(query);
				}

				return shipamaxMessage;
			}
		}

		EDocsShipamaxMessage shipamaxMessage;

		public void DeactivateCurrentShipamaxMessageIfNeeded()
		{
			if (IsInDatabase && ActiveShipamaxMessage != null)
			{
				ActiveShipamaxMessage.EM_IsActive = false;
				shipamaxMessage = null;
			}
		}

		public ZString ParseStatus => ActiveShipamaxMessage?.StatusName ?? ZString.Empty;

		public bool IsValidForShipamaxParsing()
		{
			return IsParsingSupported && !SC_IsDeleted;
		}

		public void CreateMessageForShipamaxParsing()
		{
			var newMessage = MasterFactory.New<EDocsShipamaxMessage>();
			newMessage.EM_LinkUniqueID = PK;
			newMessage.EM_LinkTable = AutoStorageDocs.Schema.TableName;
			newMessage.EM_ApplicationReference = SC_SM.ToString();

			if (IsParsingDeniedByDocumentOwner)
			{
				newMessage.EM_Status = EDIMessageStatusList.Codes.Withdrawn;
			}
			else if (!IsParsingEnabled)
			{
				newMessage.EM_Status = EDIMessageStatusList.Codes.Cancelled;
			}
		}

		public bool CanReviewParsedResult => ActiveShipamaxMessage != null && (ActiveShipamaxMessage.EM_Status == EDIMessageStatusList.Codes.ProcessedOK || ActiveShipamaxMessage.EM_Status == EDIMessageStatusList.Codes.PreProcessedOK);

		#endregion

		#region Saving

		public override void OnSaving()
		{
			if (!SC_ParentID.IsValid)
			{
				SC_ParentID = ZGuid.Empty;
			}

			if (!IsInDatabase || SC_DocTypeInfo.HasChanges || SC_SMInfo.HasChanges)
			{
				UpdateParentDocEvent();
			}

			UpdateDDIEventLogs();

			base.OnSaving();
		}

		void UpdateDDIEventLogs()
		{
			if ((SC_DocTypeInfo.HasChanges || SC_RDS_NKDocSourceInfo.HasChanges) && ParentMain != null)
			{
				var parent = ParentMain.DocumentOwner as EnterpriseBusinessObject;

				if (parent != null)
				{
					var oldReference = StorageDocsBase.CreateReference(PK, SC_DocTypeInfo.OriginalValue.ToString(), SC_RDS_NKDocSourceInfo.OriginalValue.ToString());
					var oldLogs = parent.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, oldReference));
					foreach (var log in oldLogs)
					{
						if (log.IsInDatabase)
						{
							log.Cancel();
						}
						else
						{
							log.Delete();
						}
					}

					var reference = CreateReference();
					parent.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.DocumentImported, reference);
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if ((!IsInDatabase || HasChanges) &&
				!SC_IsSystemGenerated && !IsOrphan() &&
				(IsParsingRequiredForNewOrDocChanges || isParsingEnabled != IsParsingEnabledOriginalValue) &&
				!NotSupportedDocManagerCodes.Contains(ParentMain?.SM_Type))
			{
				DeactivateCurrentShipamaxMessageIfNeeded();

				if (IsValidForShipamaxParsing())
				{
					CreateMessageForShipamaxParsing();
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				hasSC_ImageDataBeenUpdated = false;
			}
		}

		protected override void OnFactorySaving()
		{
			using (AddEventLogsForNewDocumentIfNeeded())
			{
				base.OnFactorySaving();
			}

			if (SC_IsDeleted && (!IsInDatabase || (SC_SM.IsEmpty && !(this is StorageDocsUnallocated))))
			{
				DeleteParentIfUnallocated();
				Delete();
			}
		}

		protected override bool IsOrphan()
		{
			var parentMain = ParentMain;
			return parentMain == null || parentMain.IsDeleted || parentMain.SM_DB != ((NumberedBusinessObjectFactory)Factory).DBNumber;
		}

		public override void Delete()
		{
			var parentMain = ParentMain;
			if (!IsOrphan() && parentMain.DocumentOwner is IStmALogParent parent)
			{
				CancelPreviousEventIfNeeded(parent, isForDelete: true);
			}

			DeactivateCurrentShipamaxMessageIfNeeded();

			base.Delete();
		}

		#endregion

		#region Event Logs

		bool enableAddEventLogsForNewDocument;
		public void EnableAddEventLogsForNewDocument() => enableAddEventLogsForNewDocument = true;

		internal bool ShouldAddEventLogsForNewDocument => enableAddEventLogsForNewDocument && !IsInDatabase && !SC_IsDeleted && ParentMain != null && !ParentMain.IsDeleted;

		IDisposable AddEventLogsForNewDocumentIfNeeded()
		{
			if (ShouldAddEventLogsForNewDocument)
			{
				ParentMain.AddLogsForNewDocument(ParentMain.DocumentOwner, this);
			}
			return new DisposableAction(() => enableAddEventLogsForNewDocument = false);
		}

		#endregion

		#region Save file

		public string GetFileNameOnlyWithoutExtension() => PathValidation.GetSafeFilename(SC_FileName, ' ');

		public string GetFileNameOnlyWithExtension() => PathValidation.GetSafeFilename(SC_FileNameWithExtension, ' ');

		protected virtual string GetTempFileNameWithPath()
		{
			var extension = MakeFilenameSafe.MakeSafe(SC_DataType.ToLower());
			return Temp.GetTempFileNameWithExtension(extension);
		}

		public string SaveToTempFile()
		{
			return SaveToTempFile(GetTempFileNameWithPath());
		}

		public string SaveToTempFile(string filename)
		{
			SaveToFilesystem(filename);

			tempFileName = filename;
			return tempFileName;
		}

		public void SaveToFilesystem(string filename)
		{
			try
			{
				if (!IsDeleted)
				{
					using (var stream = File.Create(filename))
					{
						SaveToStream(stream);
					}
				}
				else
				{
					AddRowError(Res.GetString("2B21F35F-02CF-4F3A-9EDD-1232E15BA2E0", "The file you are trying to save has been deleted or does not exist."));
					File.Delete(filename);
				}
			}
			catch (Exception)
			{
				File.Delete(filename);
				throw;
			}
		}

		public void SaveToStream(Stream streamToSaveTo)
		{
			using (var reader = GetSC_ImageDataReader())
			{
				reader.CopyTo(streamToSaveTo);
			}
		}

		#endregion

		#region Methods

		public bool IsAvailableForCurrentEnvContext(bool showForAllCompanies = false, bool showForAllBranches = false, bool showForAllDepartments = false)
		{
			var showToCurrentCompany = showForAllCompanies || (SC_GC_Company == ZGuid.Empty || SC_GC_Company == Env.Instance.CurrentCompanyPK);
			var showToCurrentBranch = showForAllBranches || (SC_GB_Branch == ZGuid.Empty || SC_GB_Branch == Env.Instance.CurrentBranchPK);
			var showToCurrentDepartment = showForAllDepartments || (SC_GE_Department == ZGuid.Empty || SC_GE_Department == Env.Instance.CurrentDepartmentPK);

			return showToCurrentCompany && showToCurrentBranch && showToCurrentDepartment && Env.Security.GetDocumentTypeViewCheckPoint(SC_DocType).IsAllowed;
		}

		public static string CreateReference(ZGuid identifier, string docType, string docSource)
		{
			var reference = string.Concat(docType, "|", identifier.ToString());
			if (SystemDataRegistry.Instance.DDIDocumentSource.Value && !string.IsNullOrWhiteSpace(docSource))
			{
				reference += "|NAM=" + docType;
				reference += "|SRC=" + docSource;
			}
			return reference;
		}

		public string CreateReference()
		{
			return StorageDocsBase.CreateReference(PK, SC_DocType, SC_RDS_NKDocSource);
		}

		public void AddDocType(RefDocType docType)
		{
			SC_DocType_List.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
			SC_DocType_List.Sort();
		}

		public void AddDocSource(RefDocSource docSource)
		{
			SC_DocSource_List.AddPair(docSource.RDS_Code, docSource.RDS_DescMultilingual);
			SC_DocSource_List.Sort();
		}

		protected virtual CodeDescriptionPairList GetDocumentTypeLookupList()
		{
			return new CodeDescriptionPairList(SC_DocType_List);
		}

		protected virtual CodeDescriptionPairList GetDocumentSourceLookupList()
		{
			return new CodeDescriptionPairList(SC_DocSource_List);
		}

		public void Restore()
		{
			SC_IsDeleted = false;
			RestoreShipamaxMessage();
		}

		void RestoreShipamaxMessage()
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " DESC";
			var lastShipamaxMessage = MasterFactory.LoadTop1<EDocsShipamaxMessage>(query);
			if (lastShipamaxMessage != null)
			{
				lastShipamaxMessage.EM_IsActive = true;
			}
		}

		public void DeleteQuietly()
		{
			SC_IsDeleted = true;
		}

#if DEBUG
		public void ResetIsReadByUserInThisSessionToFalse()
		{
			isReadByUserInThisSession = false;
		}

		public bool ForceThrowSqlStreamReaderRowNotFoundException;
		public bool ForceThrowOutOfMemoryException;
#endif

		public void NotifyReadByUser()
		{
			isReadByUserInThisSession = true;
			if (ParentMain != null)
			{
				ParentMain.NotifyDocumentReadByUser(this);
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Exception thrown just for Unit Test")]
		public void CopyPersistentValuesFrom(StorageDocsBase docToCopy)
		{
			ZGuid previousSC_SM;

			SuppressDocEventLog = docToCopy.SuppressDocEventLog;

			if (SC_SM.IsEmpty)
			{
				SC_SM = docToCopy.SC_SM;
				previousSC_SM = ZGuid.Empty;
			}
			else
			{
				previousSC_SM = SC_SM;
			}

			SC_IsSystemGenerated = docToCopy.SC_IsSystemGenerated; // value of some properties is dependent on this value

			var excludedColumns = new List<string>
			{
				StorageDocsSchema.Constants.SC_DocType,
				StorageDocsSchema.Constants.SC_Desc,
				StorageDocsSchema.Constants.SC_GB_Branch
			};

#if DEBUG
			if (docToCopy.ForceThrowSqlStreamReaderRowNotFoundException)
			{
				throw new SqlStreamReaderRowNotFoundException("Document has been allocated.");
			}
			if (docToCopy.ForceThrowOutOfMemoryException)
			{
				throw new Exception(string.Empty, new OutOfMemoryException("An error has occured because this program is running low on memory."));
			}
#endif

			base.CopyPersistentValuesFrom(docToCopy, new BusinessObjectCloneArgs(excludedColumns.ToArray()));

			if (!previousSC_SM.IsEmpty)
			{
				SC_SM = previousSC_SM;
			}

			this.SC_DocType = docToCopy.SC_DocType;
			this.SC_Desc = docToCopy.SC_Desc;
			this.SC_GB_Branch = docToCopy.SC_GB_Branch;
			this.SC_IsPublished = docToCopy.SC_IsPublished;
		}

		public bool SaveThisVersion => ((DocType != null && DocType.RT_SaveVersions) || SC_SaveVersions);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SC_Date = TimeFactory.CurrentUtcDateTime;
		}

		public void DeleteParentIfUnallocated()
		{
			if (ParentMain != null)
			{
				ParentMain.DeleteIfUnallocated(); // clean up parent
			}
		}

		public void UpdateDescriptionIfRequired()
		{
			if (ShouldUpdateDescAndPublishedFlagsWhileSettingDocType)
			{
				UpdateDescriptionField(GetDocumentTypeLookupList());
				UpdatePublishedAndSaveVersionFlags();
			}
		}

		protected internal virtual bool GetDescReadOnlyStatus()
		{
			return !DocTypesHelper.IsCustomisableDocType(SC_DocType);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "SCDesc assignment")]
		protected void UpdateDescriptionField(CodeDescriptionPairList docTypeList)
		{
			if (!SC_IsSystemGenerated)
			{
				if (SC_DocType.IsEmpty || !docTypeList.ContainsCode(SC_DocType))
				{
					if (!SC_Desc.IsEmpty)
					{
						SC_Desc = "";
					}
				}
				else
				{
					switch (SC_DocType)
					{
						case Core.Constants.RefDocTypes.MiscellaneousDocument:
							if (SC_Desc == docTypeList.GetMultilingualDescriptionFromCode(Core.Constants.RefDocTypes.MiscellaneousDocument).GetUnresolvedString())
							{
								SC_Desc = "";
							}
							break;
						case Core.Constants.RefDocTypes.Invoice:
							if (!SC_Desc.StartsWith(docTypeList.GetMultilingualDescriptionFromCode(Core.Constants.RefDocTypes.Invoice).GetUnresolvedString()))
							{
								SC_Desc = docTypeList.GetMultilingualDescriptionFromCode(Core.Constants.RefDocTypes.Invoice).GetUnresolvedString();
							}
							break;
						default:
							SC_Desc = docTypeList.GetMultilingualDescriptionFromCode(SC_DocType).GetUnresolvedString();
							break;
					}
				}
			}
		}

		protected virtual void UpdatePublishedAndSaveVersionFlags()
		{
			if (DocType != null)
			{
				if (!SC_IsPublished || !DocType.RT_IsPublishUpdatable)
				{
					SC_IsPublished = DocType.RT_IsPublished;
				}

				SC_SaveVersions = DocType.RT_SaveVersions;
			}
		}

		public void DeleteTempFile()
		{
			if (File.Exists(TempFileName))
			{
				try
				{
					File.Delete(TempFileName);

					tempFileName = ZString.Empty;
				}
				catch (Exception) // file in use, etc. don't bother trying to handle.
				{
				}
			}

			lock (remoteFileLock)
			{
				if (remoteFile != null)
				{
					remoteFile.Dispose();
					remoteFile = null;
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "This is just for better error reporting and not displayed to the user")]
		public override BusinessObjectFactory CreateNewFactory()
		{
			var factory = new DocumentFactoryProvider().GetFactory(MasterFactory.FactoryForEverythingExceptEDocs).GetFactory(ParentMain == null ? (ZInt)0 : ParentMain.SM_DB);
			factory.NameForDebugging = "StorageDocs Factory: " + (IsDeleted ? ZString.Empty : SC_Desc);

			return factory;
		}

		#endregion

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (imageDataStreamSource != null)
				{
					imageDataStreamSource.Dispose();
				}
				imageDataStream = null;

				if (watcher != null)
				{
					watcher.Dispose();
				}

				UnRegisterListChangedCalledRefreshBinding(fParentMain);
				DeleteTempFile();
				disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
