using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaImportItemDetail : CusSupportingInfoWithSerialNo, IHugeSequenceNumberLine
{
	public DfiaImportItemDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 10;
		public const int IssuerTypeMaxLength = 1;
		public const int QuantityDecimalPlaces = 3;
	}

	public new DfiaExportItemDetail Parent => (DfiaExportItemDetail)base.Parent;

	[MaxLength(Schema.ReferenceNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaImportItemDetail|CSI_ReferenceNumber", Caption = "License Import Item Serial Number", MediumCaption = "Lic. IMP. Item. Sr. No.", ShortCaption = "Lic. Sr. No.")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[DecimalPlaces(Schema.QuantityDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaImportItemDetail|CSI_Quantity", Caption = "License Import Quantity", MediumCaption = "Lic. IMP. Qty.", ShortCaption = "Qty.")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaImportItemDetail|CSI_UnitOfQuantity", Caption = "License Import Quantity Unit", MediumCaption = "UOM", ShortCaption = "UOM")]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[MaxLength(Schema.IssuerTypeMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaImportItemDetail|CSI_IssuerType", Caption = "Item Type", MediumCaption = "Type", ShortCaption = "Type")]
	public override ZString CSI_IssuerType { get => base.CSI_IssuerType; set => base.CSI_IssuerType = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization;
		CSI_Code = Constants.CusSupportingInfo.DutyFreeImportAuthorization;
		CSI_SubType = Constants.CusSupportingInfo.Export;
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new DfiaImportItemDetailLookup(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new DfiaImportItemDetailValidation(this);
}
