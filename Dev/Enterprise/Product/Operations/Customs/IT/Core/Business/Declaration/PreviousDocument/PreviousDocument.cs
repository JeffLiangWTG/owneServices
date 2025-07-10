using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument,
	ITariffFormatProvider,
	IPreviousDocumentReferenceNumberProvider,
	IPreviousDocumentUniversalTariffProvider,
	ISupportMultipleResourceStringData
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
	{
		public const string FormattedTariff = "FormattedTariff";
		public const int ProcedureMaxLength = 4;
		public const int CodeMaxLength = 3;
		public new const int SubTypeMaxLength = 1;
		public const int LegacyReferenceNumberMaxLength = 8;
		public const int QuantityDecimalPlaces = 5;
		public const int Ucc6ExportCodeMaxLength = 4;
		public const int Ucc6ExportReferenceNumberMaxLength = 70;
		public const int Ucc6ExportPackTypeMaxLength = 2;
		public const int Ucc6ExportQuantityDecimalPlaces = 6;
	}

	#region Properties

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override ZString HumanReadableNameCore => Res.GetString("EAA6DF1F-C8CD-4CA5-ADDE-DA0594361FFE", "Previous Document");

	[MaxLength(Schema.ProcedureMaxLength)]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.ProcedureList))]
	[ResourceStringData("IT.PreviousDocument|CSI_Procedure", Caption = "Procedure")]
	public override ZString CSI_Procedure
	{
		get => base.CSI_Procedure;
		set
		{
			var oldValue = CSI_Procedure;
			base.CSI_Procedure = value;
			if (!IsCopying && oldValue != CSI_Procedure)
			{
				PreviousDocumentHelper.SetDefaultAndEmptyField(this);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Tariff();
					Validation.ValidateCSI_Quantity();
					Validation.ValidateCSI_PackQty();
					Validation.ValidateCSI_Quantity3();
				}
			}
		}
	}

	[MaxLength(nameof(CodeMaxLength))]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
	[ResourceStringData("IT.PreviousDocument|CSI_Code", Caption = "Document")]
	[ResourceStringData("420A453E-62CB-4B90-83F8-8BD57EE99CEE", Caption = "Type", FullDescription = "[12 01 002 000] Type", MultipleKey = Ucc6ExportEntryInstructionResourceStringKey)]
	public override ZString CSI_Code => base.CSI_Code;

	[MaxLength(Schema.SubTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.SubTypeList))]
	[ResourceStringData("IT.PreviousDocument|CSI_SubType", Caption = "Class")]
	public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

	[BusinessObjectTestExclude]
	[ResourceStringData("IT.PreviousDocument|Tariff", Caption = "Tariff")]
	public ZString FormattedTariff
	{
		get { return TariffFormatter.DisplayFormat(CSI_Tariff); }
		set
		{
			var oldValue = FormattedTariff;
			CSI_Tariff = TariffFormatter.Format(value);
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_Tariff)
			{
				Validation.ValidateCSI_Quantity2();
			}
		}
	}

	public ZPropertyInfo FormattedTariffInfo
	{
		get { return GetWrappedZPropertyInfo(Schema.FormattedTariff, x => CSI_TariffInfo); }
	}

	[ReadOnlyMember(nameof(NetMassReadOnly))]
	[ResourceStringData("IT.PreviousDocument|CSI_UnitOfQuantity", ShortCaption = "Net Mass UQ", MediumCaption = "Net Mass Unit", Caption = "Net Mass UQ", FullDescription = "Net Mass Unit")]
	[ResourceStringData("IT.PreviousDocument|CSI_UnitOfQuantity|Ucc6Export", ShortCaption = "UOM", MediumCaption = "Unit Of Measure", Caption = "UOM", FullDescription = "Quantity Unit of Measure", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("IT.PreviousDocument|CSI_UnitOfQuantity|EntryInstruction", ShortCaption = "Mass UQ", MediumCaption = "Mass Unit", Caption = "Mass UQ", FullDescription = "Mass Unit", MultipleKey = EntryInstructionResourceStringKey)]
	[ResourceStringData("IT.PreviousDocument|CSI_UnitOfQuantity|InvoiceLineImport", ShortCaption = "Mass Unit", MediumCaption = "Mass Unit", Caption = "Mass Unit", MultipleKey = InvoiceLineImportResourceStringKey)]
	public override ZString CSI_UnitOfQuantity
	{
		get => base.CSI_UnitOfQuantity;
		set
		{
			var oldValue = CSI_UnitOfQuantity;
			base.CSI_UnitOfQuantity = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_UnitOfQuantity)
			{
				Validation.ValidateCSI_Quantity();
			}
		}
	}

	[ReadOnlyMember(nameof(NetMassReadOnly))]
	[ResourceStringData("IT.PreviousDocument|CSI_Quantity", ShortCaption = "Net Mass", MediumCaption = "Net Mass", Caption = "Net Mass")]
	[ResourceStringData("IT.PreviousDocument|CSI_Quantity|EntryInstruction", ShortCaption = "Mass", MediumCaption = "Mass", Caption = "Mass", MultipleKey = EntryInstructionResourceStringKey)]
	[ResourceStringData("IT.PreviousDocument|CSI_Quantity|Ucc6Export", ShortCaption = "Qty", MediumCaption = "Quantity", Caption = "Quantity", FullDescription = "Previous Document Quantity", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("IT.PreviousDocument|CSI_Quantity|InvoiceLineImport", ShortCaption = "Mass", MediumCaption = "Mass", Caption = "Mass", MultipleKey = InvoiceLineImportResourceStringKey)]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	public ZWeight EffectiveNetMass => new ZWeight(CSI_Quantity, CSI_UnitOfQuantity);

	[ResourceStringData("IT.PreviousDocument|CSI_UnitOfQuantity2", ShortCaption = "Supp. UQ", MediumCaption = "Supp. Unit", Caption = "Supplementary UQ", FullDescription = "Supplementary Unit")]
	public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

	[ResourceStringData("IT.PreviousDocument|CSI_Quantity2", ShortCaption = "Supp. Qty", MediumCaption = "Supp. Qty", Caption = "Supp. Qty", FullDescription = "Supplementary Quantity")]
	public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

	[ResourceStringData("IT.PreviousDocument|CSI_CustomsOffice", Caption = "Customs Office")]
	[ReadOnlyMember(nameof(CustomsOfficeReadOnly))]
	public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

	[MaxLength(nameof(ReferenceNumberMaxLength))]
	[ReadOnlyMember(nameof(ReferenceNumberReadOnly))]
	[ResourceStringData("IT.PreviousDocument|CSI_ReferenceNumber", Caption = "Number + CIN")]
	[ResourceStringData("IT.PreviousDocument|CSI_ReferenceNumber|UCC", Caption = "Number", MultipleKey = JobDeclaration.CaptionKeyUCC)]
	[ResourceStringData("994255F3-061E-4037-8DD4-FB7A9B77175E", Caption = "Reference Number", FullDescription = "[12 01 001 000] Reference Number", MultipleKey = Ucc6ExportEntryInstructionResourceStringKey)]
	[ResourceStringData("IT.PreviousDocument|CSI_ReferenceNumber|InvoiceLineImport", ShortCaption = "Number", MediumCaption = "Number", Caption = "Number", MultipleKey = InvoiceLineImportResourceStringKey)]
	public override ZString CSI_ReferenceNumber => base.CSI_ReferenceNumber;

	[MaxLength(18)]
	[ResourceStringData("IT.PreviousDocument|CSI_ReferenceNumber2", Caption = "MRN")]
	[ReadOnlyMember(nameof(ReferenceNumber2ReadOnly))]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[ResourceStringData("IT.PreviousDocument|CSI_DateOfIssue", Caption = "Date Of Issue")]
	[ReadOnlyMember(nameof(DateOfIssueReadOnly))]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[ResourceStringData("IT.PreviousDocument|CSI_Status", Caption = "Series")]
	[MaxLength(2)]
	[ReadOnlyMember(nameof(StatusReadOnly))]
	public override ZString CSI_Status
	{
		get => base.CSI_Status;
		set
		{
			var oldValue = CSI_Status;
			base.CSI_Status = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_Status)
			{
				Validation.ValidateCSI_ReferenceNumber();
			}
		}
	}

	[ReadOnlyMember(nameof(LineNoReadOnly))]
	public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

	[ReadOnlyMember(nameof(GrossMassReadOnly))]
	[ResourceStringData("IT.PreviousDocument|GrossMass", ShortCaption = "Gross Mass", MediumCaption = "Gross Mass", Caption = "Gross Mass")]
	public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

	[ReadOnlyMember(nameof(GrossMassReadOnly))]
	[ResourceStringData("IT.PreviousDocument|UnitOfGrossMass", ShortCaption = "Gross Mass UQ", MediumCaption = "Gross Mass Unit", Caption = "Gross Mass UQ", FullDescription = "Gross Mass Unit")]
	public override ZString CSI_UnitOfQuantity3
	{
		get => base.CSI_UnitOfQuantity3;
		set
		{
			var oldValue = CSI_UnitOfQuantity3;
			base.CSI_UnitOfQuantity3 = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_UnitOfQuantity3)
			{
				Validation.ValidateCSI_Quantity3();
			}
		}
	}

	public ZWeight EffectiveGrossMass => new ZWeight(CSI_Quantity3, CSI_UnitOfQuantity3);

	[ReadOnlyMember(nameof(PackageQuantityReadOnly))]
	[ResourceStringData("IT.PreviousDocument|PackageQuantity", ShortCaption = "Pkg Qty", MediumCaption = "Pkg. Qty", Caption = "Package Qty", FullDescription = "Package Quantity")]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[MaxLength(nameof(PackTypeMaxLength))]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackageTypeList))]
	[ReadOnlyMember(nameof(PackageQuantityReadOnly))]
	[ResourceStringData("IT.PreviousDocument|PackageType", ShortCaption = "Pkg Type", Caption = "Package Type")]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	#endregion

	#region Read Only Members

	public bool ReferenceNumberReadOnly => PreviousDocumentHelper.IsReferenceReadOnly(this);
	public bool ReferenceNumber2ReadOnly => PreviousDocumentHelper.IsReference2ReadOnly(this);
	public bool DateOfIssueReadOnly => PreviousDocumentHelper.IsDateOfIssueReadOnly(this);
	public bool LineNoReadOnly => PreviousDocumentHelper.IsLineNoReadOnly(this);
	public bool StatusReadOnly => PreviousDocumentHelper.IsStatusReadOnly(this);
	public bool CustomsOfficeReadOnly => PreviousDocumentHelper.IsCustomsOfficeReadOnly(this);
	public bool NetMassReadOnly => PreviousDocumentHelper.IsNetMassReadOnly(this);
	public bool GrossMassReadOnly => PreviousDocumentHelper.IsGrossMassReadOnly(this);
	public bool PackageQuantityReadOnly => PreviousDocumentHelper.IsPackageQuantityReadOnly(this);

	public ZBool IsSummaryDeclarationDocument => CustomsRulesProvider.IsSummaryDeclarationDocument(CSI_Procedure);

	public ZBool IsPreviousProcedureDocument => CustomsRulesProvider.IsPreviousProcedureDocument(CSI_Procedure);

	#endregion

	#region MaxLength

	int ReferenceNumberMaxLength
	{
		get
		{
			if (IsImport)
			{
				return Schema.ReferenceNumberMaxLength;
			}
			if (IsUcc6Export)
			{
				return Schema.Ucc6ExportReferenceNumberMaxLength;
			}
			return Schema.LegacyReferenceNumberMaxLength;
		}
	}

	int CodeMaxLength
	{
		get
		{
			if (IsUcc6Export)
			{
				return Schema.Ucc6ExportCodeMaxLength;
			}
			return Schema.CodeMaxLength;
		}
	}

	int PackTypeMaxLength
	{
		get
		{
			if (IsUcc6Export)
			{
				return Schema.Ucc6ExportPackTypeMaxLength;
			}
			return Schema.CSI_PackTypeMaxLength;
		}
	}

	#endregion

	ZBool IsImport => Declaration?.IsImport ?? false;

	ZBool IsUcc6Export => Declaration?.IsUCC6AndIsExport ?? false;

	ZBool IsExport => Declaration?.IsExport ?? false;

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldValue = CSI_ParentID;
			base.CSI_ParentID = value;
			if (!IsCopying && oldValue != CSI_ParentID)
			{
				SetDefaultQuantityValuesIfApplicable();
			}
		}
	}

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldValue = CSI_ParentTableCode;
			base.CSI_ParentTableCode = value;
			if (!IsCopying && oldValue != CSI_ParentTableCode)
			{
				SetDefaultQuantityValuesIfApplicable();
			}
		}
	}

	#region Implementation

	public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;
	public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

	protected override CusSupportingInfoValidation GetNewValidation()
	{
		if (ParentIsJobComInvoiceLine && IsExport)
		{
			return new InvoiceLineExportPreviousDocumentValidation(this);
		}

		if (ParentIsEntryInstruction && IsUcc6Export)
		{
			return new Ucc6ExportEntryInstructionPreviousDocumentValidation(this);
		}

		if (IsImport)
		{
			return new ImportPreviousDocumentValidation(this);
		}

		return new PreviousDocumentValidation(this);
	}

	protected override CusSupportingInfoLookups GetNewLookups()
	{
		return new PreviousDocumentLookups(this);
	}

	#endregion

	ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

	TariffFormatter TariffFormatter
	{
		get { return PreviousDocumentHelper.GetTariffFormatter(); }
	}

	public PreviousDocumentReferenceNumberProvider ReferenceNumberProvider => referenceNumberProvider ?? (referenceNumberProvider = new PreviousDocumentReferenceNumberProvider(this));
	PreviousDocumentReferenceNumberProvider referenceNumberProvider;

	public PreviousDocumentSupplementaryQuantityHandler SupplementaryQuantityHandler => supplementaryQuantityHandler ?? (supplementaryQuantityHandler = new PreviousDocumentSupplementaryQuantityHandler(this, Factory));
	PreviousDocumentSupplementaryQuantityHandler supplementaryQuantityHandler;

	void SetDefaultQuantityValuesIfApplicable()
	{
		if (Declaration is null || IsUcc6Export)
		{
			return;
		}

		if (CSI_UnitOfQuantity.IsEmpty)
		{
			CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
		}

		if (CSI_UnitOfQuantity3.IsEmpty)
		{
			CSI_UnitOfQuantity3 = Core.Constants.Weight.Kilograms;
		}
	}

	#region IPreviousDocumentUniversalTariffProvider

	ZString IPreviousDocumentUniversalTariffProvider.UniversalTariffType
	{
		get
		{
			if (IsImport)
			{
				return Constants.TariffTypes.Import;
			}
			else if (Declaration?.IsExport ?? ZBool.False)
			{
				return Constants.TariffTypes.Export;
			}
			return ZString.Empty;
		}
	}

	ZString IPreviousDocumentUniversalTariffProvider.TariffCode => CSI_Tariff;

	ZString IPreviousDocumentUniversalTariffProvider.FormattedTariff => FormattedTariff;

	ZPropertyInfo IPreviousDocumentUniversalTariffProvider.Quantity2Info => CSI_Quantity2Info;

	ZString IPreviousDocumentUniversalTariffProvider.Procedure => CSI_Procedure;

	#endregion

	#region ISupportMultipleResourceStringData

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse
	{
		get
		{
			if (Parent is JobComInvoiceLine && IsImport)
			{
				return GetMultipleKeysToUseForInvoiceLineParent().ToArray();
			}
			if (Parent is CusEntryInstruction)
			{
				return GetMultipleKeysToUseForEntryInstructionParent().ToArray();
			}
			return Declaration?.MultipleKeysToUse ?? Array.Empty<string>();
		}
	}

	IEnumerable<string> GetMultipleKeysToUseForEntryInstructionParent()
	{
		yield return EntryInstructionResourceStringKey;
		if (IsUcc6Export)
		{
			yield return Ucc6ExportEntryInstructionResourceStringKey;
		}
	}

	IEnumerable<string> GetMultipleKeysToUseForInvoiceLineParent()
	{
		yield return InvoiceLineImportResourceStringKey;
	}

	bool ParentIsEntryInstruction => CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

	public const string InvoiceLineImportResourceStringKey = "86D1148D-E8C6-4B77-88BF-31D1490D9CF5";
	public const string EntryInstructionResourceStringKey = "46A87B44-8FBC-4167-9E0E-81B2853DA6C0";
	public const string Ucc6ExportEntryInstructionResourceStringKey = "3D7FE7D5-B2DE-424E-9B54-95CE3D6AFFDD";

	#endregion
}
