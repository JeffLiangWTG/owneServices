using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryLine : AutoJPCusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusEntryLine.Schema
		{
			public const string BasicPrice = "BasicPrice";
			public const string BasicPriceCurrencyCode = "BasicPriceCurrencyCode";
		}

		[ResourceStringData("7C512826-9734-4BC9-82C8-35F35D66B98B", Caption = "Customs Value")]
		public override ZDecimal CL_CustomsValue { get => base.CL_CustomsValue; set => base.CL_CustomsValue = value; }

		[ResourceStringData("JP.Business.CusEntryLine|DutyReductionAmount", Caption = "Duty Reduction Amount", ShortCaption = "Duty Reduction Amt.")]
		public ZDecimal DutyReductionAmount => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_DutyReductionAmount);

		[ResourceStringData("JP.Business.CusEntryLine|BasicPrice", Caption = "Basic Price", ShortCaption = "BPR")]
		public ZDecimal BasicPrice => FOB.Amount;

		[ResourceStringData("JP.Business.CusEntryLine|BasicPrice", Caption = "Basic Price Currency", ShortCaption = "BPR Curr.")]
		public ZString BasicPriceCurrencyCode => FOB.Currency?.Code ?? ZString.Empty;

		protected override ZString DescriptionInternal
		{
			get
			{
				var result = ZString.Empty;
				if (Header.MergedLines.Count == 1)
				{
					result = Header.EntryInstruction?.CEI_GoodsDescription ?? ZString.Empty;
				}
				if (result.IsEmpty)
				{
					result = InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => !x.JI_Description.IsEmpty)?.JI_Description ?? ZString.Empty;
				}
				return result.IsEmpty ? base.DescriptionInternal : result;
			}
		}

		[DecimalPlaces(2)]
		[DecimalPrecision(9)]
		[ResourceStringData("JP.Business.CusEntryLine|CustomsQuantity1", Caption = "Customs Quantity 1", MediumCaption = "Customs Qty 1", ShortCaption = "Qty 1")]
		public ZDecimal CustomsQuantity1
		{
			get
			{
				if (Declaration.IsExport)
				{
					var invoiceLines = InvoiceLines.Cast<JobComInvoiceLine>();
					return invoiceLines.Select(x => x.JI_CustomsUnitQty).Distinct().Count() == 1 ? invoiceLines.Sum(x => x.JI_CustomsQuantity) : 0m;
				}
				else
				{
					return RandomLine.JI_CustomsQuantity;
				}
			}
		}

		[MaxLength(4)]
		[ResourceStringData("JP.Business.CusEntryLine|CustomsQuantityQty", Caption = "Customs Quantity Unit 1", MediumCaption = "Customs Unit 1", ShortCaption = "Unit 1")]
		public ZString CustomsQuantityUnit1
		{
			get
			{
				if (Declaration.IsExport)
				{
					var invoiceLines = InvoiceLines.Cast<JobComInvoiceLine>();
					return invoiceLines.Select(x => x.JI_CustomsUnitQty).Distinct().Count() == 1 ? RandomLine.JI_CustomsUnitQty : ZString.Empty;
				}
				else
				{
					return RandomLine.JI_CustomsUnitQty;
				}
			}
		}

		[DecimalPlaces(2)]
		[DecimalPrecision(9)]
		[ResourceStringData("JP.Business.CusEntryLine|CustomsQuantity2", Caption = "Customs Quantity 2", MediumCaption = "Customs Qty 2", ShortCaption = "Qty 2")]
		public ZDecimal CustomsQuantity2
		{
			get
			{
				if (Declaration.IsExport)
				{
					var invoiceLines = InvoiceLines.Cast<JobComInvoiceLine>();
					return invoiceLines.Select(x => x.JI_CustomsSecondUnitQty).Distinct().Count() == 1 ? invoiceLines.Sum(x => x.JI_CustomsSecondQuantity) : 0m;
				}
				else
				{
					return RandomLine.JI_CustomsSecondQuantity;
				}
			}
		}

		[MaxLength(4)]
		[ResourceStringData("JP.Business.CusEntryLine|CustomsQuantityQty2", Caption = "Customs Quantity Unit 2", MediumCaption = "Customs Unit 2", ShortCaption = "Unit 2")]
		public ZString CustomsQuantityUnit2
		{
			get
			{
				if (Declaration.IsExport)
				{
					var invoiceLines = InvoiceLines.Cast<JobComInvoiceLine>();
					return invoiceLines.Select(x => x.JI_CustomsSecondUnitQty).Distinct().Count() == 1 ? RandomLine.JI_CustomsSecondUnitQty : ZString.Empty;
				}
				else
				{
					return RandomLine.JI_CustomsSecondUnitQty;
				}
			}
		}

		[ResourceStringData("CF00F4BE-13A7-416D-A684-EF6FA6E027CD", Caption = "Confirmed Customs Value ", ShortCaption = "Confirmed CV")]
		public override ZDecimal CL_ConfirmedCustomsValue { get => base.CL_ConfirmedCustomsValue; set => base.CL_ConfirmedCustomsValue = value; }

		[ResourceStringData("09F2BCE7-B31E-479F-862F-B914CB857D76", Caption = "Customs Value (Merged)", ShortCaption = "CV (Merged)")]
		public override ZDecimal CL_MergedCustomsValue { get => base.CL_MergedCustomsValue; set => base.CL_MergedCustomsValue = value; }

		[ResourceStringData("73B5FA3A-2B83-4EEE-8823-F6819609D7B5", Caption = "Parent Line Number ", ShortCaption = "Parent LNO")]
		public override ZString CL_ParentLineNumber { get => base.CL_ParentLineNumber; set => base.CL_ParentLineNumber = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.PriceCheckTypeList))]
		[ResourceStringData("AE2E2511-EBEE-41A7-909B-EC0502246561", Caption = "Price Check")]
		public override ZString CL_PriceCheck { get => base.CL_PriceCheck; set => base.CL_PriceCheck = value; }

		[ResourceStringData("52BE4482-ECB8-461C-BF3E-1AEF52441C25", Caption = "Price Check Description")]
		public ZString CL_PriceCheckDescription => Lookups.PriceCheckTypeList.GetDescriptionFromCode(CL_PriceCheck);
	}
}
