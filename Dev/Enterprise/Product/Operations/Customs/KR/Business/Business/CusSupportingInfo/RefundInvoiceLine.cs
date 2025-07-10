using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class RefundInvoiceLine : CusSupportingInfo
	{
		public RefundInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int LineNoMaxLength = 2;
			public const int AdditionalDescriptionMaxLength = 200;
			public const int DescriptionMaxLength = 200;
		}

		[ResourceStringData("186A640E-2207-4EB9-B16D-C38B0920A7DD", Caption = "IMP Invoice Line No.")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("C2CCBF42-6334-4E8D-9D10-556EE51743E4", Caption = "IMP Invoice Line No.")]
		public ZString FormattedCSI_LineNo => CSI_LineNo.ToString("00");

		[ReadOnly(true)]
		[MaxLength(Schema.AdditionalDescriptionMaxLength)]
		[ResourceStringData("9F47E1E7-FE35-42DA-8761-039311D3159E", Caption = "Description")]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		[ReadOnly(true)]
		[MaxLength(Schema.DescriptionMaxLength)]
		[ResourceStringData("4DC9E032-E82C-4F9E-96AC-4A06507861A5", Caption = "Goods Description")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		[ResourceStringData("3F18ED6E-7BE3-4A17-932E-B776816B41EF", Caption = "Refund Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ReadOnly(true)]
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		[ResourceStringData("2075758C-0EFF-4B38-93F3-9DE5A74CE6EA", Caption = "Invoice Quantity")]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[ReadOnly(true)]
		[DecimalPlaces(Constants.DecimalPlacesConstants.UnitPrice)]
		[ResourceStringData("65E8F05D-7372-4D80-95DB-E0AFDF8E3789", Caption = "Unit Price")]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		protected override CusSupportingInfoValidation GetNewValidation() => new RefundInvoiceLineValidation(this);

		public new CusReconEntryLine Parent => base.Parent as CusReconEntryLine;
	}
}
