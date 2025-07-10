using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class SWProduction : CusSupportingInfoWithSerialNo
{
	public SWProduction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public new const int CSI_UnitOfQuantityMaxLength = 3;
		public new const int CSI_ReferenceNumberMaxLength = 17;
	}

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_ReferenceNumber", Caption = "Batch ID", MediumCaption = "Batch ID", ShortCaption = "ID")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_Quantity", Caption = "Batch Quantity", MediumCaption = "Batch Qty.", ShortCaption = "Qty.")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
	[List(nameof(Lookups) + "." + nameof(SWProductionLookups.UnitOfQuantityList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_UnitOfQuantity", Caption = "Unit Of Quantity", MediumCaption = "Unit Qty.", ShortCaption = "Qty.")]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_DateOfIssue", Caption = "Manufacturing Date", MediumCaption = "Mfg. Dt.", ShortCaption = "M. Dt.")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_DateOfExpiry", Caption = "Expiry Date", MediumCaption = "Exp. Dt.", ShortCaption = "E. Dt.")]
	public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWProduction|CSI_EffectiveDate;", Caption = "Best Before Date", MediumCaption = "Best Before", ShortCaption = "B.B. Dt.")]
	public override ZDateTimeOffset CSI_EffectiveDate { get => base.CSI_EffectiveDate; set => base.CSI_EffectiveDate = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.SingleWindowProduction;
	}

	public new SWProductionLookups Lookups => (SWProductionLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new SWProductionLookups(this);

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public new SWProductionValidation Validation => (SWProductionValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SWProductionValidation(this);
}
