using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using UniversalReferenceConstants = Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument,
	ITariffFormatProvider,
	IMergedPreviousDocument,
	IPreviousDocumentReferenceNumberProvider,
	IPreviousDocumentUniversalTariffProvider,
	ISupportMultipleResourceStringData
{
	public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
	{
		public const string FormattedTariff = "FormattedTariff";
		public const int ProcedureMaxLength = 4;
		public const int CodeMaxLength = 3;
		public const int CodePhase5MaxLength = 4;
		public new const int SubTypeMaxLength = 1;
		public const int QuantityDecimalPlacesPhase4 = 3;
		public const int QuantityDecimalPlacesPhase5 = 6;
		public const int Reference2MaxLength = 35;
		public const int CSI_QuantityPrecisionPhase5 = 19;
	}

	#region Properties

	[MaxLength(Schema.ProcedureMaxLength)]
	[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentLookups.ProcedureList))]
	[ResourceStringData("ITNctsPreviousDocument|Procedure", Caption = "Procedure")]
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
					Validation.ValidateCSI_PackQty();
					Validation.ValidateCSI_Quantity3();
					Validation.ValidateCSI_LineNo();
					Validation.ValidateCSI_Tariff();
				}
			}
		}
	}

	public override ZInt CSI_LineNo
	{
		get => base.CSI_LineNo;
		set
		{
			var oldValue = CSI_LineNo;
			base.CSI_LineNo = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_LineNo)
			{
				Validation.ValidateCSI_Tariff();
			}
		}
	}

	public override ZString CSI_Tariff
	{
		get => base.CSI_Tariff;
		set
		{
			var oldValue = CSI_Tariff;
			base.CSI_Tariff = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_Tariff)
			{
				Validation.ValidateCSI_LineNo();
			}
		}
	}

	[MaxLength(nameof(CodeMaxLength))]
	[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentLookups.CodeList))]
	[ResourceStringData("ITNctsPreviousDocument|Document", Caption = "Document")]
	[ResourceStringData("ITNctsPreviousDocument|Document|Phase5", Caption = "Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[MaxLength(Schema.SubTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentLookups.SubTypeList))]
	[ResourceStringData("ITNctsPreviousDocument|Class", Caption = "Class")]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set => base.CSI_SubType = value;
	}

	[BusinessObjectTestExclude]
	[ResourceStringData("ITNctsPreviousDocument|Tariff", Caption = "Tariff")]
	public ZString FormattedTariff
	{
		get { return TariffFormatter.DisplayFormat(CSI_Tariff); }
		set { CSI_Tariff = TariffFormatter.Format(value); }
	}

	public ZPropertyInfo FormattedTariffInfo
	{
		get { return GetWrappedZPropertyInfo(Schema.FormattedTariff, x => CSI_TariffInfo); }
	}

	[ReadOnlyMember(nameof(UnitOfQuantityReadOnly))]
	[ResourceStringData("ITNctsPreviousDocument|CSI_UnitOfQuantity", ShortCaption = "Net Mass UQ", MediumCaption = "Net Mass Unit", Caption = "Net Mass UQ", FullDescription = "Net Mass Unit", MultipleKey = NctsHeader.Phase4CaptionKey)]
	[ResourceStringData("ITNctsPreviousDocument|CSI_UnitOfQuantity|Phase5", Caption = "UOM", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_Quantity", ShortCaption = "Net Mass", MediumCaption = "Net Mass", Caption = "Net Mass", MultipleKey = NctsHeader.Phase4CaptionKey)]
	[ResourceStringData("ITNctsPreviousDocument|CSI_Quantity|Phase5", Caption = "Quantity", MultipleKey = NctsHeader.Phase5CaptionKey)]
	[DecimalPlaces(nameof(QuantityDecimalPlaces))]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_UnitOfQuantity2", ShortCaption = "Supp. UQ", MediumCaption = "Supp. Unit", Caption = "Supplementary UQ", FullDescription = "Supplementary Unit")]
	public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_Quantity2", ShortCaption = "Supp. Qty", MediumCaption = "Supp. Qty", Caption = "Supp. Qty", FullDescription = "Supplementary Quantity")]
	public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_UnitOfQuantity3", ShortCaption = "Gross Mass UQ", MediumCaption = "Gross Mass Unit", Caption = "Gross Mass UQ", FullDescription = "Gross Mass Unit")]
	public override ZString CSI_UnitOfQuantity3 { get => base.CSI_UnitOfQuantity3; set => base.CSI_UnitOfQuantity3 = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_Quantity3", ShortCaption = "Gross Mass", MediumCaption = "Gross Mass", Caption = "Gross Mass")]
	public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_CustomsOffice", Caption = "Customs Office")]
	[MaxLength(10)]
	[List(nameof(Phase4Lookups) + "." + nameof(NctsPreviousDocumentLookups.CustomsOfficeList))]
	public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_ReferenceNumber", Caption = "Number + CIN")]
	[ResourceStringData("ITNctsPreviousDocument|CSI_ReferenceNumber|Phase5", Caption = "Reference Number", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_ReferenceNumber2", Caption = "MRN")]
	[ResourceStringData("ITNctsPreviousDocument|CSI_ReferenceNumber2|Phase5", Caption = "Complement of Information", MultipleKey = NctsHeader.Phase5CaptionKey)]
	[MaxLength(nameof(Reference2MaxLength))]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[ResourceStringData("ITNctsPreviousDocument|PackageQuantity", ShortCaption = "Pkg Qty", MediumCaption = "Pkg. Qty", Caption = "Package Qty", FullDescription = "Package Quantity")]
	[ResourceStringData("ITNctsPreviousDocument|PackageQuantity|Phase5", Caption = "Number of Packages", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_PackType", Caption = "Pkg Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_Status", Caption = "Series")]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	[ResourceStringData("ITNctsPreviousDocument|CSI_ItemNumber", Caption = "Goods Item Identifier", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldValue = CSI_ParentID;
			base.CSI_ParentID = value;
			if (!IsCopying && oldValue != CSI_ParentID)
			{
				SetDefaultUnitOfQuantitiesBasedOnParent();
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
				SetDefaultUnitOfQuantitiesBasedOnParent();
			}
		}
	}

	#endregion

	#region Read Only Members

	public bool ReferenceNumberReadOnly => PreviousDocumentHelper.IsReferenceReadOnly(this);
	public bool ReferenceNumber2ReadOnly => PreviousDocumentHelper.IsReference2ReadOnly(this);
	public bool DateOfIssueReadOnly => PreviousDocumentHelper.IsDateOfIssueReadOnly(this);
	public bool LineNoReadOnly => PreviousDocumentHelper.IsLineNoReadOnly(this);
	public bool StatusReadOnly => PreviousDocumentHelper.IsStatusReadOnly(this);
	public bool CustomsOfficeReadOnly => PreviousDocumentHelper.IsCustomsOfficeReadOnly(this);

	public ZBool IsTemporaryStorage => CSI_Procedure == PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
	public ZBool IsIntoWarehouse => CSI_Procedure == PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito;

	#endregion

	#region Implementation

	protected override Customs.Business.CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentPhase5Validation(this);

	protected override Customs.Business.CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);

	protected override Customs.Business.CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsPreviousDocumentPhase5Lookups(this);

	protected override Customs.Business.CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsPreviousDocumentLookups(this);

	public NctsPreviousDocumentLookups Phase4Lookups => Factory.GetCached(ref phase4LookupsCached, () => new NctsPreviousDocumentLookups(this));
	CachedProperty<NctsPreviousDocumentLookups> phase4LookupsCached;

	#endregion

	ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

	public bool IsTypeN337 => CSI_Code == UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

	TariffFormatter TariffFormatter
	{
		get { return PreviousDocumentHelper.GetTariffFormatter(); }
	}

	public ZBool IsSummaryDeclarationDocument => CustomsRulesProvider.IsSummaryDeclarationDocument(CSI_Procedure);
	public ZBool IsPreviousProcedureDocument => CustomsRulesProvider.IsPreviousProcedureDocument(CSI_Procedure);

	public PreviousDocumentReferenceNumberProvider ReferenceNumberProvider => referenceNumberProvider ?? (referenceNumberProvider = new PreviousDocumentReferenceNumberProvider(this));
	PreviousDocumentReferenceNumberProvider referenceNumberProvider;

	public PreviousDocumentSupplementaryQuantityHandler SupplementaryQuantityHandler => supplementaryQuantityHandler ?? (supplementaryQuantityHandler = new PreviousDocumentSupplementaryQuantityHandler(this, Factory));
	PreviousDocumentSupplementaryQuantityHandler supplementaryQuantityHandler;

	#region IPreviousDocumentUniversalTariffProvider

	ZString IPreviousDocumentUniversalTariffProvider.UniversalTariffType => Universal.Constants.TariffTypes.Import;

	ZString IPreviousDocumentUniversalTariffProvider.TariffCode => CSI_Tariff;

	ZString IPreviousDocumentUniversalTariffProvider.FormattedTariff => FormattedTariff;

	ZPropertyInfo IPreviousDocumentUniversalTariffProvider.Quantity2Info => CSI_Quantity2Info;

	ZString IPreviousDocumentUniversalTariffProvider.Procedure => CSI_Procedure;

	#endregion

	#region IMergedPreviousDocument Members

	ZString IMergedPreviousDocument.Type => CSI_SubType;

	ZString IMergedPreviousDocument.Category => CSI_Code;

	ZString IMergedPreviousDocument.Mrn => CSI_ReferenceNumber2;

	ZString IMergedPreviousDocument.Register => CSI_Procedure;

	ZString IMergedPreviousDocument.ReferenceNumber => ReferenceNumberProvider.ReferenceNumberWithoutCin;

	ZString IMergedPreviousDocument.ReferenceNumberCin => ReferenceNumberProvider.ReferenceNumberCin;

	ZDate IMergedPreviousDocument.Date => (ZDate)CSI_DateOfIssue;

	ZString IMergedPreviousDocument.Series => CSI_Status;

	ZString IMergedPreviousDocument.CustomsOffice => CSI_CustomsOffice;

	ZInt IMergedPreviousDocument.ItemNumber => CSI_LineNo;

	ZBool IMergedPreviousDocument.IsSummaryDeclarationDocument => CustomsRulesProvider.IsSummaryDeclarationDocument(CSI_Procedure);

	ZBool IMergedPreviousDocument.IsPreviousProcedureDocument => CustomsRulesProvider.IsPreviousProcedureDocument(CSI_Procedure);

	ZInt IMergedPreviousDocument.PackageQuantity => CSI_PackQty;

	ZDecimal IMergedPreviousDocument.GrossMass => new ZWeight(CSI_Quantity3, CSI_UnitOfQuantity3).InKilogramsSafe;

	ZDecimal IMergedPreviousDocument.NetMass => new ZWeight(CSI_Quantity, CSI_UnitOfQuantity).InKilogramsSafe;

	ZDecimal IMergedPreviousDocument.SupplementaryQuantity => CSI_Quantity2;

	ZString IMergedPreviousDocument.Tariff => CSI_Tariff;

	#endregion

	#region Implementation

	int CodeMaxLength => IsPhase5
		? Schema.CodePhase5MaxLength
		: Schema.CodeMaxLength;

	bool UnitOfQuantityReadOnly => !IsPhase5;

	int QuantityDecimalPlaces => IsPhase5
		? Schema.QuantityDecimalPlacesPhase5
		: Schema.QuantityDecimalPlacesPhase4;

	void SetDefaultUnitOfQuantitiesBasedOnParent()
	{
		if (Parent != null && !IsPhase5)
		{
			CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
			CSI_UnitOfQuantity3 = Core.Constants.Weight.Kilograms;
		}
	}

	int Reference2MaxLength => IsPhase5
		? Schema.Reference2MaxLength
		: Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2MaxLength;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => GetMultipleCaptionKeysToUse().ToArray();

	IEnumerable<string> GetMultipleCaptionKeysToUse()
	{
		yield return IsPhase5
			? EU.NCTS.Business.NctsHeader.Phase5CaptionKey
			: EU.NCTS.Business.NctsHeader.Phase4CaptionKey;
	}

	#endregion
}
