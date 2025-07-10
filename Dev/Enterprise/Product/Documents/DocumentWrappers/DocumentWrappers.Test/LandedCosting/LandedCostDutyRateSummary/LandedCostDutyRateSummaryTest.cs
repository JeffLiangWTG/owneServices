using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(LandedCostDutyRateSummary))]
	sealed class LandedCostDutyRateSummaryTest : DocumentWrapperTestCase
	{
		public void TestWithRealValues()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 300.00m;
			invoiceHeader.JZ_InvoiceCurrLandedCostExRate = 0.5m;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200.00m;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100.00m;

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lcLine1 = lcHeader.Histories.AddNew();
			lcLine1.LH_ParentID = invoiceLine1.PK;
			lcLine1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			LandedCostHistory lcLine2 = lcHeader.Histories.AddNew();
			lcLine2.LH_ParentID = invoiceLine2.PK;
			lcLine2.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			lcLine2.SetLineValue(300.00m, "TDT");

			LandedCostDutyRateSummary wrapper = LandedCostDutyRateSummary.New(5m, Factory);
			wrapper.AddHistoryLine(lcLine1);

			AssertEquals("wrapper.TotalInvoicePriceInOSCurrency", 200.00m, wrapper.TotalInvoicePriceInOSCurrency);
			AssertEquals("wrapper.TotalLineCostInLocalCurrency", 400.00m, wrapper.TotalLineCostInLocalCurrency);
			AssertEquals("wrapper.DutyPercent", 5.00m, wrapper.DutyPercent);
			AssertEquals("wrapper.DutyPercentString", "5.00%", wrapper.DutyPercentString);
			AssertEquals("wrapper.Factor", 2.00m, wrapper.Factor);

			wrapper.AddHistoryLine(lcLine2);
			AssertEquals("wrapper.TotalInvoicePriceInOSCurrency", 300.00m, wrapper.TotalInvoicePriceInOSCurrency);
			AssertEquals("wrapper.TotalLineCostInLocalCurrency", 900.00m, wrapper.TotalLineCostInLocalCurrency);
			AssertEquals("wrapper.DutyPercent", 5.00m, wrapper.DutyPercent);
			AssertEquals("wrapper.DutyPercentString", "5.00%", wrapper.DutyPercentString);
			AssertEquals("wrapper.Factor", 3.00m, wrapper.Factor);
		}

		public void TestEmpty()
		{
			LandedCostDutyRateSummary wrapper = LandedCostDutyRateSummary.New(ZDecimal.Zero, Factory);
			AssertEquals("wrapper.TotalInvoicePriceInOSCurrency", ZDecimal.Zero, wrapper.TotalInvoicePriceInOSCurrency);
			AssertEquals("wrapper.TotalLineCostInLocalCurrency", ZDecimal.Zero, wrapper.TotalLineCostInLocalCurrency);
			AssertEquals("wrapper.DutyPercent", ZDecimal.Zero, wrapper.DutyPercent);
			AssertEquals("wrapper.DutyPercentString", "Free", wrapper.DutyPercentString);
			AssertEquals("wrapper.Factor", ZDecimal.Zero, wrapper.Factor);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { LandedCostDutyRateSummary.New(0.00m, Factory) };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return LandedCostDutyRateSummary.New(0.00m, Factory);
		}
	}
}
