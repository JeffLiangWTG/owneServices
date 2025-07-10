using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	[CodeProperty(nameof(EffectiveCode)), DescriptionProperty(nameof(EffectiveDescription))]
	[UniversalCopyWithExtendedEntities]
	public class CusEntryInstruction : AutoCusEntryInstruction
		, Integration.Customs.DE.ICusEntryInstruction
		, ISequenceNumberHeader
		, IPreviousDocumentParentProvider
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.CusEntryInstruction.Schema
		{
			public const string CEI_LocalClearanceDate = "CEI_LocalClearanceDate";
			public const string CEI_EarlyClearanceFlag = "CEI_EarlyClearanceFlag";
			public const string CEI_SimplifiedGrantAuthorization = "CEI_SimplifiedGrantAuthorization";
			public const string CEI_AuthorisationNumber = "CEI_AuthorisationNumber";
			public const string CEI_CompletionDuration = "CEI_CompletionDuration";
			public const string CEI_CriteriaType = "CEI_CriteriaType";
			public const string CEI_InwardProcessingAdditionalInformation = "CEI_InwardProcessingAdditionalInformation";
			public const string CEI_InwardProcessingDescription = "CEI_InwardProcessingDescription";

			public const int CEI_InwardProcessingAdditionalInformationMaxLength = 512;
			public const int CEI_InwardProcessingDescriptionMaxLength = 512;
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override ZString HumanReadableNameCore => Res.GetString("a1d63ac2-f646-492d-9ed8-39efa9388cec", "Entry Instruction");

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ClearOutwardProcessingFieldsIfNeed();
			ClearInwardProcessingDetailsIfNeeded();
		}

		void ClearOutwardProcessingFieldsIfNeed()
		{
			if (!EnabledOutwardProcessing)
			{
				ReimportCountryCodes.RemoveAndDeleteAll();
				IdentificationMeanCodes.RemoveAndDeleteAll();
				Products.RemoveAndDeleteAll();
			}
		}

		public bool EnabledOutwardProcessing => JobDeclaration != null && JobDeclaration.IsExport && this.Style1stDigitIs1();

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.AdditionalInformation);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.InwardProcessingAdditionalInformation);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.InwardProcessingDescription);
				return fNoteTypes;
			}
		}

		public bool CEI_SubStyle_ReadOnly => CEI_Style == ImportDeclarationTypeList.Codes.LUZ || IsSimplifiedDeclaration;

		[ReadOnlyMember(nameof(CEI_SubStyle_ReadOnly))]
		[MaxLength(nameof(CEI_SubStyle_MaxLength))]
		public override ZString CEI_SubStyle
		{
			get => base.CEI_SubStyle;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_SubStyle))
				{
					var oldValue = base.CEI_SubStyle;
					base.CEI_SubStyle = value;
					if (!IsCopying && oldValue != value)
					{
						if (!IsValidationSuspended)
						{
							AddInfoValidation.ValidateZG_ReimportDate();
							Validation.ValidateReimportCountries();
							Validation.ValidateIdentificationMeans();
							Validation.ValidateProducts();
						}

						ReimportCountryCodes.MarkAsNeedingValidation();
						IdentificationMeanCodes.MarkAsNeedingValidation();
					}
				}
			}
		}

		int CEI_SubStyle_MaxLength => (JobDeclaration?.IsExport ?? false) ? 2 : 1;

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				if (CEI_JE != value)
				{
					base.CEI_JE = value;
					ReimportCountryCodes.MarkAsNeedingValidation();
					IdentificationMeanCodes.MarkAsNeedingValidation();
				}
			}
		}

		public ZString EffectiveCode => (JobDeclaration?.IsExport ?? false) ? (ZString)(CEI_SubStyle + "|" + CEI_Style) : CEI_Style;

		public ZString EffectiveDescription => (JobDeclaration?.IsExport ?? false) ? CEI_Description : (ZString)(CEI_SubStyle + " - " + CEI_Description);

		[MaxLength(nameof(CEI_Style_MaxLength))]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					var oldValue = base.CEI_Style;
					base.CEI_Style = value;
					if (!IsCopying && oldValue != value)
					{
						ClearLocalClearanceDateIfNeeded();
						UpdateCEISubStyleIfNeeded();
						UpdateCEI_ProcedureIfNeeded();
						UpdateEarlyClearanceFlagIfNeeded();
						ClearInwardProcessingDetailsIfNeeded();
						ClearAuthorisationNumberFieldsIfNeeded();
						UpdateGoodsLocationCGL_Qualifier();
						SetAdditionalInformation(AdditionalInformation);
						ClearDeclarationFieldsIfNeeded();
						ClearSubStyleIfNeeded();

						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							invoiceLine.ClearCountryOfOriginIfNeeded();
							invoiceLine.ClearDecisiveDateIfNeeded();
						}

						if (!IsValidationSuspended)
						{
							AddInfoValidation.ValidateZG_ReimportDate();
							Validation.ValidateReimportCountries();
							Validation.ValidateIdentificationMeans();
							Validation.ValidateProducts();
						}

						ReimportCountryCodes.MarkAsNeedingValidation();
						IdentificationMeanCodes.MarkAsNeedingValidation();
					}
				}
			}
		}

		int CEI_Style_MaxLength => (JobDeclaration?.IsExport ?? false) ? 6 : Schema.CEI_StyleMaxLength;

		public override ZString CEI_Description
		{
			get
			{
				var result = base.CEI_Description;
				if (result.IsEmpty)
				{
					if (JobDeclaration?.IsExport ?? false)
					{
						result = ((ZString)Lookups.VariantList.GetDescriptionFromCode(CEI_SubStyle)).Left(50);
					}
					else
					{
						result = ((ZString)Lookups.DeclarationTypeList.GetDescriptionFromCode(CEI_Style)).Left(50);
					}
				}
				return result;
			}
			set => base.CEI_Description = value;
		}

		[ResourceStringData("AA109235-2439-49FC-9AA3-25D630AE6169", Caption = "CPC")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CPCList))]
		public override ZString CEI_Procedure
		{
			get => base.CEI_Procedure;
			set => base.CEI_Procedure = value;
		}

		public override ZGuid CEI_OA_Warehouse
		{
			get => base.CEI_OA_Warehouse;
			set
			{
				var oldValue = CEI_OA_Warehouse;
				base.CEI_OA_Warehouse = value;
				if (oldValue != value)
				{
					UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessaryAndIsImport();
				}
			}
		}

		#region AddInfos

		#region Inward Processing

		void ClearInwardProcessingDetailsIfNeeded()
		{
			if (!EnabledInwardProcessing)
			{
				CEI_SimplifiedGrantAuthorization = ZString.Empty;
				CEI_CompletionDuration = ZInt.Zero;
				CEI_CriteriaType = ZString.Empty;
				CEI_InwardProcessingAdditionalInformation = ZString.Empty;
				CEI_InwardProcessingDescription = ZString.Empty;
				ClearMainAccountingAddress();
				CompletionCustomsOffices.RemoveAndDeleteAll();
				InwardProcessingPlaces.RemoveAndDeleteAll();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.ClearInwardProcessingDetailsIfNeeded();
				}
			}
		}

		void ClearAuthorisationNumberFieldsIfNeeded()
		{
			if (CEI_AuthorisationNumberReadOnly && !CEI_AuthorisationNumber.IsEmpty)
			{
				CEI_AuthorisationNumber = ZString.Empty;
			}
		}

		void UpdateGoodsLocationCGL_Qualifier()
		{
			if (JobDeclaration?.IsExport ?? false)
			{
				if (this.Style4thDigitIs1() || this.Style4thDigitIs9())
				{
					GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				}
				else if (this.Style4thDigitIs3() || this.Style4thDigitIs4())
				{
					GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				}
			}
		}

		public bool EnabledInwardProcessing => JobDeclaration != null && JobDeclaration.IsImport && CEI_Style == ImportDeclarationTypeList.Codes.EAV;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.SimplifiedGrantAuthorizationList))]
		[ResourceStringData("7F43E80C-E7CA-4400-9CC9-295163891254", Caption = "Simplified Grant Authorization")]
		public ZString CEI_SimplifiedGrantAuthorization
		{
			get => AddInfo.ZG_SimplifiedGrantAuthorization;
			set
			{
				var oldValue = CEI_SimplifiedGrantAuthorization;
				AddInfo.ZG_SimplifiedGrantAuthorization = value;
				if (!IsCopying && oldValue != CEI_SimplifiedGrantAuthorization)
				{
					if (CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.N)
					{
						CEI_AuthorisationNumber = ZString.Empty;
					}
					if (CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.J)
					{
						CEI_CompletionDuration = ZInt.Zero;
						CEI_CriteriaType = ZString.Empty;
						CEI_InwardProcessingDescription = ZString.Empty;
						ClearMainAccountingAddress();
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							invoiceLine.ZG_EconomicConditions = ZString.Empty;
							invoiceLine.ZG_IdentificationMeansType = ZString.Empty;
							invoiceLine.JI_ExtraInfoForClassification = ZString.Empty;
							invoiceLine.InwardProcessingProducts.RemoveAndDeleteAll();
						}
					}
					if (CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.N)
					{
						var inwardProcessingAuthorizationNumberList = Lookups.InwardProcessingAuthorizationNumberList;
						if (inwardProcessingAuthorizationNumberList.Count == 1)
						{
							CEI_AuthorisationNumber = inwardProcessingAuthorizationNumberList[0].Code;
						}
					}
				}
			}
		}

		public ZPropertyInfo CEI_SimplifiedGrantAuthorizationInfo => GetWrappedZPropertyInfo(Schema.CEI_SimplifiedGrantAuthorization, x => AddInfo.ZG_SimplifiedGrantAuthorizationInfo);

		[ReadOnlyMember(nameof(CEI_AuthorisationNumberReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.InwardProcessingAuthorizationNumberList))]
		[ResourceStringData("B3ADCB25-5700-4121-80B0-DE41148FB3EC", Caption = "Authorization Number", ShortCaption = "Auth. Number")]
		public ZString CEI_AuthorisationNumber
		{
			get => AddInfo.ZG_AuthorisationNumber;
			set => AddInfo.ZG_AuthorisationNumber = value;
		}

		public ZPropertyInfo CEI_AuthorisationNumberInfo => GetWrappedZPropertyInfo(Schema.CEI_AuthorisationNumber, x => AddInfo.ZG_AuthorisationNumberInfo);

		ZBool CEI_AuthorisationNumberReadOnly
		{
			get
			{
				if (EnabledInwardProcessing)
				{
					return CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.N;
				}
				else
				{
					return (JobDeclaration?.IsImport ?? false) && !authorisationNumberSimplified_EntryInstructionStyles.Contains(CEI_Style);
				}
			}
		}

		readonly ZString[] authorisationNumberSimplified_EntryInstructionStyles = new ZString[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.VAV };

		[ReadOnlyMember(nameof(CEI_CompletionDurationReadOnly))]
		[ResourceStringData("2C174077-FC42-4E3E-9FA8-FDE84EDA3E93", Caption = "Completion Duration (months)")]
		public ZInt CEI_CompletionDuration
		{
			get => AddInfo.ZG_CompletionDuration;
			set => AddInfo.ZG_CompletionDuration = value;
		}

		public ZPropertyInfo CEI_CompletionDurationInfo => GetWrappedZPropertyInfo(Schema.CEI_CompletionDuration, x => AddInfo.ZG_CompletionDurationInfo);

		ZBool CEI_CompletionDurationReadOnly => CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.J;

		[ReadOnlyMember(nameof(CEI_CriteriaTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CriteriaTypeList))]
		[ResourceStringData("1F13F4B2-2E4E-43F8-B8E5-895DB2BDF7C8", Caption = "Criteria Type")]
		public ZString CEI_CriteriaType
		{
			get => AddInfo.ZG_CriteriaType;
			set => AddInfo.ZG_CriteriaType = value;
		}

		public ZPropertyInfo CEI_CriteriaTypeInfo => GetWrappedZPropertyInfo(Schema.CEI_CriteriaType, x => AddInfo.ZG_CriteriaTypeInfo);

		ZBool CEI_CriteriaTypeReadOnly => CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.J;

		[MaxLength(Schema.CEI_InwardProcessingAdditionalInformationMaxLength)]
		[ResourceStringData("Enterprise.Customs.DE.Business.CusEntryInstruction|CEI_InwardProcessingAdditionalInformation", Caption = "Additional Information")]
		public ZString CEI_InwardProcessingAdditionalInformation
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.InwardProcessingAdditionalInformation.Description);
			set => Notes.SetNoteText(this, CEI_InwardProcessingAdditionalInformationInfo, PredefinedNoteTypes.Instance.InwardProcessingAdditionalInformation.Description, value);
		}

		public ZPropertyInfo CEI_InwardProcessingAdditionalInformationInfo => GetZPropertyInfo(Schema.CEI_InwardProcessingAdditionalInformation);

		[ReadOnlyMember(nameof(CEI_InwardProcessingDescriptionReadOnly))]
		[MaxLength(Schema.CEI_InwardProcessingDescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.DE.Business.CusEntryInstruction|CEI_InwardProcessingDescription", Caption = "Processing Description")]
		public ZString CEI_InwardProcessingDescription
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.InwardProcessingDescription.Description);
			set
			{
				var oldValue = CEI_InwardProcessingDescription;
				Notes.SetNoteText(this, CEI_InwardProcessingDescriptionInfo, PredefinedNoteTypes.Instance.InwardProcessingDescription.Description, value);
				if (!IsCopying && oldValue != CEI_InwardProcessingDescription)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateCEI_InwardProcessingDescription();
					}
				}
			}
		}

		public ZPropertyInfo CEI_InwardProcessingDescriptionInfo => GetZPropertyInfo(Schema.CEI_InwardProcessingDescription);

		ZBool CEI_InwardProcessingDescriptionReadOnly => CEI_SimplifiedGrantAuthorization != SimplifiedGrantAuthorizationList.Codes.J;

		[ChildEditable]
		public InwardProcessingPlaceCollection InwardProcessingPlaces
		{
			get
			{
				if (inwardProcessingPlaces == null)
				{
					inwardProcessingPlaces = new InwardProcessingPlaceCollection(this);
					inwardProcessingPlaces.Load();
					RegisterEditableChildObject(inwardProcessingPlaces);
				}
				return inwardProcessingPlaces;
			}
		}
		InwardProcessingPlaceCollection inwardProcessingPlaces;

		#region ISequenceNumberHeader

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(InwardProcessingPlaces);

		internal ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this, () => 0, () => byte.MaxValue));
		ShortSequenceNumberGenerator sequenceGenerator;

		#endregion

		[ChildEditable]
		public CompletionCustomsOfficeCollection CompletionCustomsOffices
		{
			get
			{
				if (completionCustomsOffices == null)
				{
					completionCustomsOffices = new CompletionCustomsOfficeCollection(this);
					completionCustomsOffices.Load();
					RegisterEditableChildObject(completionCustomsOffices);
				}

				return completionCustomsOffices;
			}
		}
		CompletionCustomsOfficeCollection completionCustomsOffices;

		#endregion

		[ResourceStringData("781CA636-F70A-4A51-AF60-38021D636A96", Caption = "Decisive Date")]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set => base.CEI_DateForDuty = value;
		}

		[ReadOnlyMember(nameof(IsLocalClearanceDateReadonly))]
		[ResourceStringData("4E20E84E-A9B9-4D9A-8876-3C0B68957399", Caption = "Local Clearance Date")]
		public ZDateTime CEI_LocalClearanceDate
		{
			get => AddInfo.ZG_LocalClearanceDate;
			set => AddInfo.ZG_LocalClearanceDate = value;
		}

		public ZPropertyInfo CEI_LocalClearanceDateInfo => GetWrappedZPropertyInfo(Schema.CEI_LocalClearanceDate, x => AddInfo.ZG_LocalClearanceDateInfo);

		public bool IsLocalClearanceDateReadonly => !CEI_Style.IsEmpty && !ImportDeclarationTypeList.IsLocalClearanceDateRelevant(CEI_Style);

		void ClearLocalClearanceDateIfNeeded()
		{
			if (IsLocalClearanceDateReadonly && !CEI_LocalClearanceDate.IsEmpty)
			{
				CEI_LocalClearanceDate = ZDateTime.Empty;
			}
		}

		[MaxLength(nameof(MaxLengthForAdditionalInformationField))]
		[ResourceStringData("Enterprise.Customs.DE.Business.CusEntryInstruction|AdditionalInformation", Caption = "Additional Information", ShortCaption = "Additional Info")]
		public override ZString AdditionalInformation
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.AdditionalInformation.Description);
			set => SetAdditionalInformation(value);
		}

		int MaxLengthForAdditionalInformationField
		{
			get
			{
				if (JobDeclaration?.IsImport ?? false)
				{
					switch (CEI_Style)
					{
						case ImportDeclarationTypeList.Codes.EAV:
						case ImportDeclarationTypeList.Codes.EZL:
							return 100;
						default:
							return 2000;
					}
				}
				return 350;
			}
		}

		void SetAdditionalInformation(ZString value)
		{
			Notes.SetNoteText(this, AdditionalInformationInfo, PredefinedNoteTypes.Instance.AdditionalInformation.Description, value.Left(AdditionalInformationInfo.MaxLength));
		}

		[ResourceStringData("02F219E5-AD71-484A-89E6-C07D19EB8BC1", Caption = "Exit Date")]
		public override ZDateTime ZG_ExitDate
		{
			get => base.ZG_ExitDate;
			set => base.ZG_ExitDate = value;
		}

		void UpdateCEISubStyleIfNeeded()
		{
			if (IsSimplifiedDeclaration)
			{
				CEI_SubStyle = ImportSubStyleList.Codes.C;
			}
			else if (CEI_SubStyle_ReadOnly)
			{
				CEI_SubStyle = ZString.Empty;
			}
		}

		void UpdateCEI_ProcedureIfNeeded()
		{
			if (JobDeclaration?.IsImport ?? false)
			{
				var fCPCList = Lookups.CPCList;
				if (fCPCList.Count == 1)
				{
					CEI_Procedure = fCPCList[0].Code;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EarlyClearanceFlags))]
		[ReadOnlyMember(nameof(CEI_EarlyClearanceFlag_ReadOnly))]
		[ResourceStringData("DBA5BE77-7424-419F-8095-519074648E55", Caption = "Early Clearance Flag")]
		public ZString CEI_EarlyClearanceFlag
		{
			get => AddInfo.ZG_EarlyClearanceFlag;
			set => AddInfo.ZG_EarlyClearanceFlag = value;
		}

		public ZPropertyInfo CEI_EarlyClearanceFlagInfo => GetWrappedZPropertyInfo(Schema.CEI_EarlyClearanceFlag, x => AddInfo.ZG_EarlyClearanceFlagInfo);
		public bool CEI_EarlyClearanceFlag_ReadOnly => !IsEarlyClearanceFlagApplicable;

		public ZBool IsEarlyClearanceFlagApplicable
		{
			get
			{
				switch (CEI_Style)
				{
					case ImportDeclarationTypeList.Codes.AZL:
					case ImportDeclarationTypeList.Codes.EZL:
					case ImportDeclarationTypeList.Codes.LUZ:
					case ImportDeclarationTypeList.Codes.VZL:
						return ZBool.True;
					default:
						return ZBool.False;
				}
			}
		}

		public ZBool IsInwardMovementApplicable
		{
			get
			{
				switch (CEI_Style)
				{
					case ImportDeclarationTypeList.Codes.AAV:
					case ImportDeclarationTypeList.Codes.AZL:
					case ImportDeclarationTypeList.Codes.EAV:
					case ImportDeclarationTypeList.Codes.EZL:
					case ImportDeclarationTypeList.Codes.LUZ:
					case ImportDeclarationTypeList.Codes.VAV:
					case ImportDeclarationTypeList.Codes.VZL:
						return ZBool.True;
					default:
						return ZBool.False;
				}
			}
		}

		public ZBool IsSimplifiedDeclaration
		{
			get
			{
				switch (CEI_Style)
				{
					case ImportDeclarationTypeList.Codes.AZ:
					case ImportDeclarationTypeList.Codes.AAV:
					case ImportDeclarationTypeList.Codes.AZL:
						return ZBool.True;
					default:
						return ZBool.False;
				}
			}
		}

		public void UpdateEarlyClearanceFlagIfNeeded()
		{
			var isEarlyClearanceFlagApplicable = IsEarlyClearanceFlagApplicable;
			var hasValue = !CEI_EarlyClearanceFlag.IsEmpty;
			if (!isEarlyClearanceFlagApplicable && hasValue)
			{
				CEI_EarlyClearanceFlag = ZString.Empty;
			}
			else if (isEarlyClearanceFlagApplicable && !hasValue)
			{
				CEI_EarlyClearanceFlag = EarlyClearanceFlagsList.Codes.N;
			}
			CEI_EarlyClearanceFlagInfo.RefreshBinding();
		}

		[ResourceStringData("DE.Business.Declaration.CusEntryInstruction|ZG_PartyConstellation", Caption = "Party Constellation")]
		public override ZString ZG_PartyConstellation
		{
			get => base.ZG_PartyConstellation;
			set => base.ZG_PartyConstellation = value;
		}

		#endregion

		public new INonPersistentCusDV1DetailPivotCollection<NonPersistentCusDV1DetailPivot> DV1DetailsPivots => (INonPersistentCusDV1DetailPivotCollection<NonPersistentCusDV1DetailPivot>)base.DV1DetailsPivots;

		protected override INonPersistentCusDV1DetailPivotCollection<EU.Business.Declaration.NonPersistentCusDV1DetailPivot> GetNewDV1DetailsPivotCollection()
		{
			return new NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>(this, e => new (e));
		}

		protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(this);

		protected new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

		public new AddInfoCusEntryInstructionValidation AddInfoValidation => AddInfo.Validation;

		public new AddInfoCusEntryInstructionLookups AddInfoLookups => AddInfo.Lookups;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		[ChildEditable]
		public ReimportCountryCodeCollection ReimportCountryCodes
		{
			get
			{
				if (reimportCountries == null)
				{
					reimportCountries = new ReimportCountryCodeCollection(this);
					reimportCountries.Load();
					RegisterEditableChildObject(reimportCountries);
				}

				return reimportCountries;
			}
		}
		ReimportCountryCodeCollection reimportCountries;

		[ChildEditable]
		public IdentificationMeansCodeCollection IdentificationMeanCodes
		{
			get
			{
				if (identificationMeans == null)
				{
					identificationMeans = new IdentificationMeansCodeCollection(this);
					identificationMeans.Load();
					RegisterEditableChildObject(identificationMeans);
				}

				return identificationMeans;
			}
		}
		IdentificationMeansCodeCollection identificationMeans;

		[ChildEditable]
		public ProductSupportingInfoCollection Products
		{
			get
			{
				if (products == null)
				{
					products = new ProductSupportingInfoCollection(this);
					products.Load();
					RegisterEditableChildObject(products);
				}

				return products;
			}
		}

		ProductSupportingInfoCollection products;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this, false);

		[UniversalCopyCollectionEntity(CusAuthorizationUsageSchema.Constants.TableName, CusAuthorizationUsageSchema.Constants.AGC_ParentID, CusAuthorizationUsageSchema.Constants.AGC_ParentTableCode)]
		public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages => (EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>)base.CusAuthorizationUsages;

		protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

		PreviousDocumentCollection IPreviousDocumentParentProvider.PreviousDocuments => PreviousDocuments;

		public PreviousDocumentMaster PreviousDocumentMaster
		{
			get
			{
				if (previousDocumentMaster == null)
				{
					previousDocumentMaster = new PreviousDocumentMaster(Factory, this);
					RegisterEditableChildObject(previousDocumentMaster);
				}
				return previousDocumentMaster;
			}
		}
		PreviousDocumentMaster previousDocumentMaster;

		#region JobDocAddress

		protected override DocAddressType[] GetSupportedAddressTypesCore()
		{
			return new DocAddressType[] { DocAddressType.MainAccountingAddress };
		}

		public JobDocAddress MainAccountingAddress
		{
			get
			{
				if (fMainAccountingAddress == null || fMainAccountingAddress.IsDeleted)
				{
					fMainAccountingAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.MainAccountingAddress);
				}
				return fMainAccountingAddress;
			}
		}
		JobDocAddress fMainAccountingAddress;

		protected override JobDocAddressValidation GetJobDocAddressValidationCore(JobDocAddress addressToValidate)
		{
			return new CusEntryInstructionJobDocAddressValidation(addressToValidate, this);
		}

		void ClearMainAccountingAddress()
		{
			MainAccountingAddress.OrganisationPK = ZGuid.Empty;
		}

		public bool IsMainAccountingAddressAvailable => CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J;

		#endregion

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.CompletionCustomsOffice, typeof(CompletionCustomsOffice) },
				{ CusCodeDataTypeList.Codes.IdentificationMeansCode, typeof(IdentificationMeansCode) },
				{ CusCodeDataTypeList.Codes.ReimportCountryCode, typeof(ReimportCountryCode) }
			};
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ ProductSupportingInfo.CusSupportingInfoType, typeof(ProductSupportingInfo) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument, typeof(EU.Business.RequestedDocument) }
			};
		}

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		protected override EU.Business.CusGoodsLocation GetCusGoodsLocation() => (CusGoodsLocation)base.GetCusGoodsLocation();

		void UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessaryAndIsImport()
		{
			if (JobDeclaration?.IsImport ?? false)
			{
				this.UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary();
			}
		}

		void ClearDeclarationFieldsIfNeeded()
		{
			var jobDeclaration = JobDeclaration;
			if (jobDeclaration != null && CEI_Style == ImportDeclarationTypeList.Codes.AVABR)
			{
				jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				jobDeclaration.JE_DeclarantType = ZString.Empty;
				jobDeclaration.JE_TransportMode = ZString.Empty;
				jobDeclaration.ZG_MethodOfPayment = ZString.Empty;
				jobDeclaration.ZG_BorderTransportMeans = ZString.Empty;
				jobDeclaration.JE_ContainerMode = ZString.Empty;
				jobDeclaration.ZG_IsHighValueOvrd = false;
				jobDeclaration.JE_VesselName = ZString.Empty;
				jobDeclaration.JE_RL_NKPortOfLoading = ZString.Empty;
				jobDeclaration.JE_RL_NKPortOfFirstArrival = ZString.Empty;
				jobDeclaration.JE_RL_NKPortOfArrival = ZString.Empty;
				jobDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
				jobDeclaration.JE_ExportDate = ZDate.Empty;
				jobDeclaration.JE_DateOfFirstArrival = ZDate.Empty;
				jobDeclaration.JE_DateOfArrival = ZDate.Empty;
				jobDeclaration.ZG_Box18TransportID = ZString.Empty;
				jobDeclaration.ZG_Box18TransportNationality = ZString.Empty;
				jobDeclaration.JE_TransportModeInland = ZString.Empty;
				jobDeclaration.JE_HouseBill = ZString.Empty;
				jobDeclaration.JE_RL_NKOrigin = ZString.Empty;
				jobDeclaration.JE_RL_NKFinalDestination = ZString.Empty;
				jobDeclaration.JE_DateAtOrigin = ZDate.Empty;
				jobDeclaration.JE_DateAtFinalDestination = ZDate.Empty;
				jobDeclaration.JE_GoodsDescription = ZString.Empty;
				jobDeclaration.JE_OwnerRef = ZString.Empty;
				jobDeclaration.JE_TotalNoOfPacks = ZInt.Zero;
				jobDeclaration.JE_TotalWeight = ZDecimal.Zero;
				jobDeclaration.JE_TotalWeightUnit = ZString.Empty;
				jobDeclaration.JE_TotalVolume = ZDecimal.Zero;
				jobDeclaration.JE_TotalVolumeUnit = ZString.Empty;
				jobDeclaration.JE_ShipmentIncoTerm = ZString.Empty;
				jobDeclaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				jobDeclaration.JE_GoodsOrigin = ZString.Empty;
				jobDeclaration.JE_GoodsDestination = ZString.Empty;
				jobDeclaration.JE_TotalNoOfPacksPackType = ZString.Empty;
				jobDeclaration.JE_UCR = ZString.Empty;
				jobDeclaration.JE_CustomsOffice = ZString.Empty;
				jobDeclaration.JE_OA_Representative = ZGuid.Empty;
				jobDeclaration.JE_OA_Representative_ZAddress.OrgPK = ZGuid.Empty;
				jobDeclaration.RepresentativeDocAddress.OrganisationPK = ZGuid.Empty;
				jobDeclaration.DefermentPartyDocAddress.OrganisationPK = ZGuid.Empty;
				jobDeclaration.JE_GoodsOrigin = ZString.Empty;
				jobDeclaration.JE_GoodsDestination = ZString.Empty;
			}
		}

		void ClearSubStyleIfNeeded()
		{
			if (CEI_Style == ImportDeclarationTypeList.Codes.AVABR && !CEI_SubStyle.IsEmpty)
			{
				CEI_SubStyle = ZString.Empty;
			}
		}
	}
}
