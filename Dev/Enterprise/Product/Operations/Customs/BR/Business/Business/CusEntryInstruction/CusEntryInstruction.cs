using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ECC = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	[CodeProperty(CusEntryInstruction.Schema.CEI_Description)]
	public partial class CusEntryInstruction : AutoBRCusEntryInstruction, Integration.Customs.BR.ICusEntryInstruction
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
		}

		#region Schema

		public new class Schema : AutoBRCusEntryInstruction.Schema
		{
			public const int UCRNumberMaxLength = 35;
			public const int BillNumberMaxLength = 32;
			public const string UCRNumber = "UCRNumber";
			public const string BillNumber = "BillNumber";
			public const string IsUCROverridden = "IsUCROverridden";
			public const string AdditionalInformation = "AdditionalInformation";
			public const string AdditionalInformationManual = "AdditionalInformationManual";
			public const string AdditionalInformationOptionDescription = "AdditionalInformationOptionDescription";
			public const string IsAFRMMRateOverridden = "IsAFRMMRateOverridden";
			public const string Justification = "Justification";

			public const int ImportAdditionalInformationMaxLength = 7800;
			public const int ExportAdditionalInformationMaxLength = 2000;
			public const int ImportLicenseAdditionalInformationMaxLength = 3900;
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|CEI_Style", Caption = "Declaration Type")]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set => base.CEI_Style = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.SpecialCustomsClearanceList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_SpecialCustomsClearance", ShortCaption = "Special Clearance", Caption = "Special Clearance", FullDescription = "Indicates some special feature of the dispatch. If not, it must be left empty")]
		public override ZString CEI_SpecialCustomsClearance
		{
			get => base.CEI_SpecialCustomsClearance;
			set
			{
				var oldValue = CEI_SpecialCustomsClearance;
				base.CEI_SpecialCustomsClearance = value;
				if (!IsCopying && oldValue != CEI_SpecialCustomsClearance)
				{
					OnSpecialCustomsClearanceChanged();
					UpdateCEI_DetailWithoutLegalDoc();

					if (JobDeclaration != null)
					{
						foreach (var invoiceLine in JobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>())
						{
							invoiceLine.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		void OnSpecialCustomsClearanceChanged()
		{
			if (CEI_SpecialCustomsClearance == SpecialCustomsClearanceList.Codes._2002)
			{
				CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			}
			else if (CEI_SpecialCustomsClearance.IsEmpty)
			{
				CEI_LegalDocument = ZString.Empty;
				CEI_DetailWithoutLegalDoc = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.LegalDocumentList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_LegalDocument", ShortCaption = "Legal Document", Caption = "Legal Document", FullDescription = "The type of tax document that supports the export of the goods must be indicated. If part of the goods is subject to NF-e, part is subject to Manual Invoice and part is not subject to any type of invoice, 03 entries must be prepared/created, one for each situation.")]
		public override ZString CEI_LegalDocument
		{
			get => base.CEI_LegalDocument;
			set
			{
				var oldValue = CEI_LegalDocument;
				base.CEI_LegalDocument = value;
				if (!IsCopying && oldValue != CEI_LegalDocument)
				{
					if (CEI_DetailWithoutLegalDoc_ReadOnly)
					{
						CEI_DetailWithoutLegalDoc = ZString.Empty;
					}
					else
					{
						UpdateCEI_DetailWithoutLegalDoc();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(CEI_DetailWithoutLegalDoc_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.DetailWithoutLegalDocList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_DetailWithoutLegalDoc", Caption = "Details of the operation without Invoice", FullDescription = "In the case of Legal Document = SNF - No Invoice, choose one option in this list")]
		public override ZString CEI_DetailWithoutLegalDoc
		{
			get => base.CEI_DetailWithoutLegalDoc;
			set => base.CEI_DetailWithoutLegalDoc = value;
		}

		public bool CEI_DetailWithoutLegalDoc_ReadOnly => CEI_LegalDocument == LegalDocumentList.Codes.ElectronicLogisticInvoice;

		void UpdateCEI_DetailWithoutLegalDoc()
		{
			if (CEI_SpecialCustomsClearance == SpecialCustomsClearanceList.Codes._2002 && !CEI_DetailWithoutLegalDoc_ReadOnly && CEI_DetailWithoutLegalDoc.IsEmpty)
			{
				CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3004;
			}
		}

		#region AFRMM

		public bool IsAFRMMApplicable => JobDeclaration?.IsAFRMMApplicable ?? false;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.AFRMMMethodOfCalculationList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_AFRMMMethodOfCalculation", Caption = "Type", FullDescription = "The AFRMM Method of Calculation.")]
		public override ZString CEI_AFRMMMethodOfCalculation { get => base.CEI_AFRMMMethodOfCalculation; set => base.CEI_AFRMMMethodOfCalculation = value; }

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(CEI_AFRMMRateOverride_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_AFRMMRateOverride", Caption = "AFRMM Rate", FullDescription = "The AFRMM Rate in percentage.")]
		public override ZDecimal CEI_AFRMMRateOverride
		{
			get => IsAFRMMRateOverridden ? base.CEI_AFRMMRateOverride : BRRefCusTaxOrFee.GetAfrmmTaxRate(Factory, CEI_AFRMMMethodOfCalculation, DateOfValuation);
			set => base.CEI_AFRMMRateOverride = value;
		}

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(CEI_AFRMMRateOverride_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_UtilizationFeeOverride", Caption = "Merchant System Utilization Fee", FullDescription = "The Merchant System Utilization Fee in BRL.")]
		public override ZDecimal CEI_UtilizationFeeOverride
		{
			get => IsAFRMMRateOverridden ? base.CEI_UtilizationFeeOverride : BRRefCusTaxOrFee.GetAfrmmTaxRate(Factory, ECC.Customs.Universal.RefCusTaxOrFee.Codes.MerchantSystemUtilizationFee, DateOfValuation);
			set => base.CEI_UtilizationFeeOverride = value;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_SystemUtilizationFeeOverrideCurrency", Caption = "Merchant System Utilization Fee", FullDescription = "The Merchant System Utilization Fee in BRL.")]
		public ZString CEI_SystemUtilizationFeeOverrideCurrency => ECC.CurrencyCodes.Brazil;

		[BusinessObjectTestExclude()]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|IsAFRMMRateOverridden", Caption = "Override")]
		public ZBool IsAFRMMRateOverridden
		{
			get
			{
				if (!isAFRMMRateOverridden.HasValue)
				{
					isAFRMMRateOverridden = !base.CEI_AFRMMRateOverride.IsEmpty || !base.CEI_UtilizationFeeOverride.IsEmpty;
				}
				return isAFRMMRateOverridden.Value;
			}
			set
			{
				var oldValue = IsAFRMMRateOverridden;
				isAFRMMRateOverridden = value;

				if (!IsCopying && oldValue != IsAFRMMRateOverridden)
				{
					if (IsAFRMMRateOverridden)
					{
						CEI_AFRMMRateOverrideInfo.RefreshBinding();
						CEI_UtilizationFeeOverrideInfo.RefreshBinding();
					}
					else
					{
						CEI_AFRMMRateOverride = ZDecimal.Zero;
						CEI_UtilizationFeeOverride = ZDecimal.Zero;
					}
				}
				IsAFRMMRateOverriddenInfo.RefreshBinding();
			}
		}
		ZBool? isAFRMMRateOverridden;

		public ZPropertyInfo IsAFRMMRateOverriddenInfo => GetZPropertyInfo(Schema.IsAFRMMRateOverridden);

		public bool CEI_AFRMMRateOverride_ReadOnly => !IsAFRMMApplicable || !IsAFRMMRateOverridden;

		#endregion

		#region UCR Entry Number

		CusEntryNumber UCRCusEntryNumber
		{
			get
			{
				if (fUCRCusEntryNumber == null || fUCRCusEntryNumber.IsDeleted)
				{
					fUCRCusEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Brazil);
					RegisterEditableChildObject(fUCRCusEntryNumber);
				}
				return fUCRCusEntryNumber;
			}
		}
		CusEntryNumber fUCRCusEntryNumber;

		public void DeleteUCRNumber()
		{
			CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Brazil)?.Delete();
			fUCRCusEntryNumber = null;
		}

		[MaxLength(Schema.UCRNumberMaxLength)]
		[ReadOnlyMember(nameof(UCRNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|UCRNumber", ShortCaption = "UCR Number", Caption = "Unique Consignment Reference Number", FullDescription = "Choose UCR Override to define your own UCR or empty to receive after send Entry to Customs.")]
		public ZString UCRNumber
		{
			get { return UCRCusEntryNumber.CE_EntryNum; }
			set
			{
				if (UCRNumber != value)
				{
					UCRCusEntryNumber.CE_EntryNum = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateUCRNumber();
				}
				UCRNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UCRNumberInfo
		{
			get { return GetZPropertyInfo(Schema.UCRNumber); }
		}

		bool UCRNumber_ReadOnly => !IsUCROverridden || (!EntryHeader?.EntryNumber.IsEmpty ?? false);

		[ReadOnlyMember(nameof(IsUCROverridden_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|IsUCROverridden", ShortCaption = "UCR Override", Caption = "UCR Override", FullDescription = "Choose UCR Override to define your own UCR or empty to receive after send Entry to Customs.")]
		public ZBool IsUCROverridden
		{
			get
			{
				return !UCRCusEntryNumber.CE_EntryIsSystemGenerated;
			}
			set
			{
				if (UCRCusEntryNumber.CE_EntryIsSystemGenerated != !value)
				{
					UCRCusEntryNumber.CE_EntryIsSystemGenerated = !value;
					UCRNumber = ZString.Empty;
				}
				IsUCROverriddenInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsUCROverriddenInfo
		{
			get { return GetZPropertyInfo(Schema.IsUCROverridden); }
		}

		bool IsUCROverridden_ReadOnly => EntryHeader != null && !(EntryHeader.EntryNumber.IsEmpty && EntryHeader.DeclarationUCR.IsEmpty);

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_IsConsortedExport", Caption = "Is Consorted Export?")]
		public override ZBool CEI_IsConsortedExport
		{
			get => base.CEI_IsConsortedExport;
			set => base.CEI_IsConsortedExport = value;
		}

		#endregion

		#region Bill Number Entry Number

		CusEntryNumber BillNumberCusEntryNumber
		{
			get
			{
				if (fBillNumberCusEntryNumber == null || fBillNumberCusEntryNumber.IsDeleted)
				{
					fBillNumberCusEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil);
					RegisterEditableChildObject(fBillNumberCusEntryNumber);
				}
				return fBillNumberCusEntryNumber;
			}
		}
		CusEntryNumber fBillNumberCusEntryNumber;

		public void DeleteBillNumber()
		{
			CusEntryNumber.Load(this, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil)?.Delete();
			fBillNumberCusEntryNumber = null;
		}

		[MaxLength(Schema.BillNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BillNumber", Caption = "e-Bill Number")]
		public ZString BillNumber
		{
			get { return BillNumberCusEntryNumber.CE_EntryNum; }
			set
			{
				if (BillNumber != value)
				{
					CheckMaximumLength(BillNumberInfo, value);
					BillNumberCusEntryNumber.CE_EntryNum = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBillNumber();
				}
				BillNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BillNumberInfo => GetZPropertyInfo(Schema.BillNumber);

		[MaxLength(7)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BillTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BillType", Caption = "e-Bill Type")]
		public ZString BillType
		{
			get
			{
				if (JobDeclaration?.IsBillNumberOnEntryInstructionApplicable ?? false)
				{
					var billType = JobDeclaration.IsTransportByWater ? BillTypeList.Codes.HBL : BillTypeList.Codes.UCR;
					return Lookups.BillTypeList.GetDescriptionFromCode(billType);
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region Justification Contact Detail Address

		public JobDocAddress JustificationContactDetailAddress
		{
			get
			{
				if (fJustificationContactDetailAddress == null || fJustificationContactDetailAddress.IsDeleted)
				{
					fJustificationContactDetailAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.JustificationContactDetailAddress);
				}
				return fJustificationContactDetailAddress;
			}
		}

		JobDocAddress fJustificationContactDetailAddress;

		public bool IsJustificationContactDetailAddressAvailable => JobDeclaration?.IsExport ?? false;

		#endregion

		#region IDocAddresses Members

		protected override JobDocAddressValidation GetJobDocAddressValidationCore(JobDocAddress addressToValidate)
		{
			return new CusEntryInstructionJobDocAddressValidation(addressToValidate, this);
		}

		protected override DocAddressType[] GetSupportedAddressTypesCore()
		{
			return new DocAddressType[]
			{
				DocAddressType.JustificationContactDetailAddress
			};
		}

		#endregion

		#region StmNote

		public bool IsSystemGeneratedInformationApplicable => !CEI_AdditionalInformationOption.IsEmpty && CEI_AdditionalInformationOption != AdditionalInformationOptions.Codes.OnlyFreeText;

		public bool IsFreeTextInformationApplicable => !CEI_AdditionalInformationOption.IsEmpty && CEI_AdditionalInformationOption != AdditionalInformationOptions.Codes.OnlySystemGenerated;

		[MaxLength(nameof(MaxLengthForAdditionalInformationField))]
		[ReadOnlyMember(nameof(AdditionalInformation_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_AdditionalInformation", Caption = "System Generated", FullDescription = "The text automatically generated by the system to compose the Additional Information.")]
		public ZString AdditionalInformation
		{
			get => AdditionalInformationNote.Text;
			set
			{
				AdditionalInformationNote.SetNoteText(this, AdditionalInformationInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_AdditionalInformationOption();
				}
			}
		}

		bool AdditionalInformation_ReadOnly => IsImportExcludingLicense;

		HiddenTextNote AdditionalInformationNote => additionalInformationNote ??= new HiddenTextNote(this, PredefinedNoteTypes.Instance.AdditionalInformation.Description);
		HiddenTextNote additionalInformationNote;

		public ZPropertyInfo AdditionalInformationInfo => GetZPropertyInfo(Schema.AdditionalInformation);

		int MaxLengthForAdditionalInformationField
		{
			get
			{
				if (IsImportExcludingLicense)
				{
					return AutoStmNote.Schema.ST_NoteTextMaxLength;
				}
				else if (IsImportLicense)
				{
					return Schema.ImportLicenseAdditionalInformationMaxLength;
				}
				else
				{
					return Schema.ExportAdditionalInformationMaxLength;
				}
			}
		}

		[ReadOnlyMember(nameof(AdditionalInformationManual_ReadOnly))]
		[MaxLength(nameof(AutoStmNote.Schema.ST_NoteTextMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_AdditionalInformationManual", Caption = "Free Text", FullDescription = "A text that will be used to compose the Additional Information.")]
		public ZString AdditionalInformationManual
		{
			get => AdditionalInformationManualNote.Text;
			set
			{
				AdditionalInformationManualNote.SetNoteText(this, AdditionalInformationManualInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_AdditionalInformationOption();
				}
			}
		}

		HiddenTextNote AdditionalInformationManualNote => additionalInformationManualNote ??= new HiddenTextNote(this, PredefinedNoteTypes.Instance.SecondaryAdditionalInformation.Description);
		HiddenTextNote additionalInformationManualNote;

		public ZPropertyInfo AdditionalInformationManualInfo => GetZPropertyInfo(Schema.AdditionalInformationManual);

		bool AdditionalInformationManual_ReadOnly => !IsFreeTextInformationApplicable;

		public ZString AdditionalInformationConcatenated
		{
			get
			{
				var additionalInformations = Array.Empty<ZString>();
				switch (CEI_AdditionalInformationOption)
				{
					case AdditionalInformationOptions.Codes.OnlyFreeText:
						additionalInformations = new[] { AdditionalInformationManual };
						break;
					case AdditionalInformationOptions.Codes.OnlySystemGenerated:
						additionalInformations = new[] { AdditionalInformation };
						break;
					case AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText:
						additionalInformations = new[] { AdditionalInformation, AdditionalInformationManual };
						break;
					case AdditionalInformationOptions.Codes.FreeTextAndSystemGenerated:
						additionalInformations = new[] { AdditionalInformationManual, AdditionalInformation };
						break;
				}
				return ZString.Join(System.Environment.NewLine, additionalInformations).Truncate(7800);
			}
		}

		[MaxLength(1000)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|BR_Justification", Caption = "Justification", FullDescription = "Justification for waiving the invoice")]
		public ZString Justification
		{
			get => JustificationNote.Text;
			set => JustificationNote.SetNoteText(this, JustificationInfo, value);
		}

		HiddenTextNote JustificationNote => justificationNote ?? (justificationNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.Justification.Description));
		HiddenTextNote justificationNote;

		public ZPropertyInfo JustificationInfo => GetZPropertyInfo(Schema.Justification);

		public ZBool IsJustificationVisible => CEI_LegalDocument == LegalDocumentList.Codes.NoInvoice && DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification(CEI_DetailWithoutLegalDoc);

		[MaxLength(50)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.AdditionalInformationOptions))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|AdditionalInformationOptionDescription", Caption = "Additional Information Option")]
		public ZString AdditionalInformationOptionDescription
		{
			get => Lookups.AdditionalInformationOptions.GetDescriptionFromCode(CEI_AdditionalInformationOption);
			set
			{
				var oldValue = CEI_AdditionalInformationOption;
				CEI_AdditionalInformationOption = Lookups.AdditionalInformationOptions.GetCodeFromDescription(value);
				if (!IsCopying && oldValue != CEI_AdditionalInformationOption)
				{
					if (!IsFreeTextInformationApplicable)
					{
						AdditionalInformationManual = ZString.Empty;
					}
					if (CEI_AdditionalInformationOption.IsEmpty)
					{
						AdditionalInformation = ZString.Empty;
					}
				}
				AdditionalInformationOptionDescriptionInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AdditionalInformationOptionDescriptionInfo => GetWrappedZPropertyInfo(Schema.AdditionalInformationOptionDescription, x => CEI_AdditionalInformationOptionInfo);

		#endregion

		#region MercosulForeignExportDeclaration

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public MercosulForeignDeclarationCollection MercosulForeignDeclarations
		{
			get
			{
				if (fMercosulForeignDeclarations == null)
				{
					fMercosulForeignDeclarations = new MercosulForeignDeclarationCollection(this);
					fMercosulForeignDeclarations.Load();
					RegisterEditableChildObject(fMercosulForeignDeclarations);
				}
				return fMercosulForeignDeclarations;
			}
		}

		MercosulForeignDeclarationCollection fMercosulForeignDeclarations;

		#endregion

		#region ParentEntryInstructionGenPivot

		public RelatedEntryInstructionGenPivot ParentEntryInstructionGenPivot => ParentEntryInstructionGenPivotCollection.Cast<RelatedEntryInstructionGenPivot>().FirstOrDefault();

		[ChildEditable(true)]
		public RelatedEntryInstructionGenPivotCollection ParentEntryInstructionGenPivotCollection
		{
			get
			{
				if (fParentEntryInstructionGenPivotCollection == null)
				{
					fParentEntryInstructionGenPivotCollection = new RelatedEntryInstructionGenPivotCollection(this);
					fParentEntryInstructionGenPivotCollection.Load();
					RegisterEditableChildObject(fParentEntryInstructionGenPivotCollection);
				}
				return fParentEntryInstructionGenPivotCollection;
			}
		}

		RelatedEntryInstructionGenPivotCollection fParentEntryInstructionGenPivotCollection;

		public CusEntryInstruction ParentEntryInstruction => ParentEntryInstructionGenPivot?.ParentEntryInstruction;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstruction|HasParentEntryInstruction", Caption = "Is Split?")]
		public ZBool HasParentEntryInstruction => ParentEntryInstruction != null;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryInstructiont|ParentEntryInstructionDescription", Caption = "Parent Entry Instruction")]
		public ZString ParentEntryInstructionDescription => ParentEntryInstruction?.CEI_Description ?? ZString.Empty;

		public void LinkToParentInstruction(CusEntryInstruction parentEntryInstruction)
		{
			ParentEntryInstructionGenPivotCollection.AddPivotFor(parentEntryInstruction);
		}

		public void RemoveParentInstruction()
		{
			ParentEntryInstructionGenPivot.Delete();
		}

		#endregion

		public bool IsExport => JobDeclaration?.IsExport ?? false;

		public bool IsImport => JobDeclaration?.IsImport ?? false;

		public bool IsImportSiscomex => JobDeclaration?.IsImportSiscomex ?? false;

		public bool IsImportLicense => JobDeclaration?.IsImportLicense ?? false;

		public bool IsImportOnly => JobDeclaration?.IsImportOnly ?? false;

		public bool IsImportExcludingLicense => JobDeclaration?.IsImportExcludingLicense ?? false;

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new IEnumerable<JobComInvoiceLine> InvoiceLines => base.InvoiceLines.Cast<JobComInvoiceLine>();

		public IEnumerable<JobComInvoiceHeader> Invoices => Factory.GetCached(ref fInvoices,
			() => InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.InvoiceHeader != null).Select(x => x.InvoiceHeader).Distinct());

		CachedProperty<IEnumerable<JobComInvoiceHeader>> fInvoices;

		protected override bool SupportsCloneCore() => true;

		public override bool CanDelete => base.CanDelete && LinkedImportDeclaration == null;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (LinkedImportDeclaration != null)
				{
					return ResString.GetMultilingualString("5644808C-3B13-42D8-BC5F-60B9EDF5C955", "The Entry Instruction cannot be deleted. There is an Import Declaration reference it.");
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		public JobDeclaration LinkedImportDeclaration
		{
			get
			{
				if (!IsImportLicense || !IsInDatabase)
				{
					fLinkedImportDeclaration = null;
				}
				else if (fLinkedImportDeclaration == null)
				{
					fLinkedImportDeclaration = new CachedProperty<JobDeclaration>(Factory, () =>
					{
						var subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
						subQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot);
						subQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, PK);
						var query = new ZDBOnlyQuery(typeof(JobDeclaration));
						query.AddSubQuery(JobDeclarationSchema.PK, subQuery, JoinCondition.And);
						return Factory.LoadTop1<JobDeclaration>(query);
					});
				}
				return fLinkedImportDeclaration?.Value;
			}
		}

		CachedProperty<JobDeclaration> fLinkedImportDeclaration;

		public ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

		public override void Delete()
		{
			JobDeclaration?.CustomsEntryInstructions.Cast<CusEntryInstruction>()
				.SelectMany(x => x.ParentEntryInstructionGenPivotCollection.Cast<RelatedEntryInstructionGenPivot>())
				.Where(x => x.XX_Relation1ID == PK || x.XX_Relation2ID == PK).DeleteAll();

			this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			this.DeleteHiddenNotes();
			base.Delete();
		}

		protected override void OnFactorySaving()
		{
			if (!IsJustificationContactDetailAddressAvailable)
			{
				DocAddresses.FindByDocAddressType(DocAddressType.JustificationContactDetailAddress)?.Delete();
			}

			if (!IsImportExcludingLicense)
			{
				MercosulForeignDeclarations.RemoveAndDeleteAll();
			}

			if (!IsImportLicense)
			{
				ParentEntryInstructionGenPivotCollection.RemoveAndDeleteAll();
			}

			if (fUCRCusEntryNumber?.CE_EntryNum.IsEmpty ?? false)
			{
				fUCRCusEntryNumber.Delete();
				fUCRCusEntryNumber = null;
			}

			if (fBillNumberCusEntryNumber?.CE_EntryNum.IsEmpty ?? false)
			{
				fBillNumberCusEntryNumber.Delete();
				fBillNumberCusEntryNumber = null;
			}

			base.OnFactorySaving();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsJustificationVisible)
			{
				JustificationNote.Delete();
				justificationNote = null;
			}
		}

		public const int MaximumInvoiceLinesAllowedForImportLicense = 80;

		public const int MaximumCharactersForWarningMessageForImportLicense = 46;
	}
}
