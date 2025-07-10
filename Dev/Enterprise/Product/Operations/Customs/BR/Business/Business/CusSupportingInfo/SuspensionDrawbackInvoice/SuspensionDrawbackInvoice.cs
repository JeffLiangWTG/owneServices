using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackInvoice : CusSupportingInfo
	{
		public new class Schema : CusSupportingInfo.Schema
		{
		}

		public SuspensionDrawbackInvoice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(44)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice|CSI_ReferenceNumber", Caption = "Invoice Number or NF-e Access Key")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice|CSI_Quantity", Caption = "Quantity")]
		[DecimalPlaces(5)]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawInvoiceBackCusSupporting|CSI_Value", Caption = "Trading currency value")]
		[DecimalPlaces(2)]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawInvoiceBackCusSupporting|CSI_DateOfIssue", Caption = "Date")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.SuspensionDrawbackInvoice;
		}

		public new SuspensionDrawbackInvoiceValidation Validation => (SuspensionDrawbackInvoiceValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SuspensionDrawbackInvoiceValidation(this);
		}

		public new SuspensionDrawback Parent => base.Parent as SuspensionDrawback;
	}
}
