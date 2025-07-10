using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ValuationAdjustmentWrapperTest : DataProviderTestCase<ValuationAdjustmentWrapper>
	{
		public void TestValuationIndicators()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			line = invoice.InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);

			declaration.JE_ApplicationCode = "DI";
			declaration.JE_MessageType = "IMP";
			merger.DoMerge();

			AssertEquals("Default values: All header related indicators = No, lines = SameAsInvoiceHeader", "0000", GetProvider().ValuationIndicators);

			line.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.Yes;
			AssertEquals("Header RelatedIndicator = No, Line = Yes", "1000", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator = true;
			line.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("Header RelatedIndicator = Yes, Line = Same", "1000", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator = false;

			line.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
			AssertEquals("Header RelatedIndicator2 = No, Line = Yes", "0100", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator2 = true;
			line.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("Header RelatedIndicator2 = Yes, Line = Same", "0100", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator2 = false;

			line.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.Yes;
			AssertEquals("Header RelatedIndicator3 = No, Line = Yes", "0010", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator3 = true;
			line.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("Header RelatedIndicator3 = Yes, Line = Same", "0010", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator3 = false;

			line.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.Yes;
			AssertEquals("Header RelatedIndicator4 = No, Line = Yes", "0001", GetProvider().ValuationIndicators);
			invoice.RelatedIndicator4 = true;
			line.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("Header RelatedIndicator4 = Yes, Line = Same", "0001", GetProvider().ValuationIndicators);
		}

		protected override ValuationAdjustmentWrapper GetProvider()
		{
			return ValuationAdjustmentWrapper.New(line);
		}

		JobComInvoiceLine line;
	}
}
