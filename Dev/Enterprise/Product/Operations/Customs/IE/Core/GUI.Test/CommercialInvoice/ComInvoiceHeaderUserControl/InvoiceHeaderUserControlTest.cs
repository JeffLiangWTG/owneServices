using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestInvoiceChargesGrid()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>() as Business.Declaration.JobComInvoiceHeader;
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;

			using (var form = new CommercialInvoiceForm(invoiceHeader))
			{
				form.Show();

				var invoiceChargesGrid = form.FindSingle<ZGrid>("InvoiceChargesGrid");

				invoiceHeader.JZ_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Column correct heading", "Statistical Value Applicable", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsStatisticalValueApplicable).CaptionResourceString.Caption);
				AssertEquals("Column correct heading", "Included in Inv. Amt", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).CaptionResourceString.Caption);
				AssertEquals("Column correct heading", "Distribute By", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_DistributeBy).CaptionResourceString.Caption);
				AssertEquals("Column correct heading", "Fixed Rate", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable).CaptionResourceString.Caption);
				AssertEquals("Column correct heading", "Exchange Rate", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate).CaptionResourceString.Caption);
				AssertEquals("Column correct heading", "Curr.", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).Caption);
				AssertEquals("Column correct heading", "Add to FOB?", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption);
				AssertEquals("Column correct heading", "Incl. in  Inv. Lines?", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).Caption);
				AssertSequencesEqual(ExportOrderedColumns, invoiceChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));

				invoiceHeader.JZ_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Column correct heading", "Dutiable", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption);
				AssertEquals("Column correct heading", "VAT Apply", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable).Caption);
				AssertSequencesEqual(ImportOrderedColumns, invoiceChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));

				invoiceHeader.JZ_MessageType = "ABC";
				AssertEquals("Column correct heading", "Curr", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).Caption);
				AssertEquals("Column correct heading", "Included in Lines", invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).Caption);
				AssertSequencesEqual(OtherOrderedColumns, invoiceChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public IReadOnlyList<string> ExportOrderedColumns => new[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy,
				JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
		};

		public IReadOnlyList<string> ImportOrderedColumns => new[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceCharge.Schema.J7_IsGSTApplicable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy,
				JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
		};

		public IReadOnlyList<string> OtherOrderedColumns => new[]
		{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.ChargeCodeDescription,
				InvoiceCharge.Schema.J7_IsGSTApplicable,
				InvoiceCharge.Schema.J7_PrepaidCollect,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
		};

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
