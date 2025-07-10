using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(LandedCostDutyRateSummaryCollection))]
	sealed class LandedCostDutyRateSummaryCollectionTest : DocBaseWrapperCollectionTest<LandedCostDutyRateSummaryCollection>
	{
		public void TestConstructorAndLoadAndSummarise()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 300.00m;
			invoiceHeader.JZ_InvoiceCurrLandedCostExRate = 0.5m;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100.00m;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100.00m;
			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 100.00m;

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lcLine1 = lcHeader.Histories.AddNew();
			lcLine1.LH_ParentID = invoiceLine1.PK;
			lcLine1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lcLine1.SetLineValue(40.00m, "TDT");
			lcLine1.LH_DutyPercent = 4.00m;

			LandedCostHistory lcLine2 = lcHeader.Histories.AddNew();
			lcLine2.LH_ParentID = invoiceLine2.PK;
			lcLine2.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lcLine2.SetLineValue(60.00m, "TDT");
			lcLine2.LH_DutyPercent = 4.00m;

			LandedCostHistory lcLine3 = lcHeader.Histories.AddNew();
			lcLine3.LH_ParentID = invoiceLine3.PK;
			lcLine3.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lcLine3.SetLineValue(100.00m, "TDT");
			lcLine3.LH_DutyPercent = 6.45m;

			DocLandedCostHeader docLCHeader = DocLandedCostHeader.New(lcHeader, Factory);

			LandedCostDutyRateSummaryCollection dutyRates = new LandedCostDutyRateSummaryCollection(docLCHeader.Histories, Factory);

			AssertEquals(2, dutyRates.Count);

			LandedCostDutyRateSummary wrapper = dutyRates[0];
			AssertEquals("wrapper.TotalInvoicePriceInOSCurrency", 200.00m, wrapper.TotalInvoicePriceInOSCurrency);
			AssertEquals("wrapper.TotalLineCostInLocalCurrency", 500.00m, wrapper.TotalLineCostInLocalCurrency);
			AssertEquals("wrapper.DutyPercent", 4.00m, wrapper.DutyPercent);
			AssertEquals("wrapper.DutyPercentString", "4.00%", wrapper.DutyPercentString);
			AssertEquals("wrapper.Factor", 2.50m, wrapper.Factor);

			wrapper = dutyRates[1];
			AssertEquals("wrapper.TotalInvoicePriceInOSCurrency", 100.00m, wrapper.TotalInvoicePriceInOSCurrency);
			AssertEquals("wrapper.TotalLineCostInLocalCurrency", 300.00m, wrapper.TotalLineCostInLocalCurrency);
			AssertEquals("wrapper.DutyPercent", 6.45m, wrapper.DutyPercent);
			AssertEquals("wrapper.DutyPercentString", "6.45%", wrapper.DutyPercentString);
			AssertEquals("wrapper.Factor", 3.00m, wrapper.Factor);
		}

		protected override LandedCostDutyRateSummaryCollection GetNewDocumentWrapperCollection()
		{
			return new LandedCostDutyRateSummaryCollection(null, Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return new ZDecimal(0.00m);
		}
	}
}
