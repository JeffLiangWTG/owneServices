using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class JobWork : CusSupportingInfoWithSerialNo
{
	public JobWork(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusSupportingInfo.Schema
	{
		public new const int CSI_ReferenceNumberMaxLength = 7;
		public new const int CSI_ReferenceNumber2MaxLength = 2;
		public new const int CSI_CustomsOfficeMaxLength = 6;
		public new const int CSI_UnitOfQuantityMaxLength = 3;
	}

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_ReferenceNumber", Caption = "BE Number", MediumCaption = "BE No.", ShortCaption = "BE No.")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_DateOfIssue", Caption = "BE Date", MediumCaption = "BE Dt", ShortCaption = "BE Dt")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[MaxLength(Schema.CSI_CustomsOfficeMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_CustomsOffice", Caption = "BE Customs House", MediumCaption = "Customs House", ShortCaption = "Cus. House")]
	public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_ReferenceNumber2", Caption = "BE Invoice Serial No", MediumCaption = "Invoice Sr. No.", ShortCaption = "Inv. Sr. No.")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_ItemNumber", Caption = "BE Item Sr. No.", MediumCaption = "Item Sr.", ShortCaption = "Item Sr.")]
	public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_Quantity", Caption = "BE Quantity Utilized", MediumCaption = "Qty. Utilized", ShortCaption = "Qty.")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobWork|CSI_UnitOfQuantity", Caption = "BE UOM", MediumCaption = "UOM", ShortCaption = "UOM")]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.JobWork;
	}

	public new JobWorkValidation Validation => (JobWorkValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new JobWorkValidation(this);

	public new JobWorkLookups Lookups => (JobWorkLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new JobWorkLookups(this);

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
}
