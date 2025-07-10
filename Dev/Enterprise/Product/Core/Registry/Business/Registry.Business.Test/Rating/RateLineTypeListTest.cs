namespace Enterprise.Registry.Business.Testing
{
	sealed class RateLineTypeListTest : NUnit.Framework.TestCase
	{
		public void TestGetLineType()
		{
			AssertEquals("Invoice Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerInvoice));
			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerEntry));

			AssertEquals("Invoice Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerEntry));
			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerInvoice));

			AssertEquals("Invoice Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerShipment));
			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerShipment));

			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerHTSCodePerShipment));
			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerHTSCodePerInvoice));
			AssertEquals("Tariff Line", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.TotalHTSCountPerDeclaration));
		}

		public void TestGetLineTypePlural()
		{
			AssertEquals("Invoice Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerInvoice, true));
			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerEntry, true));

			AssertEquals("Invoice Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerEntry, true));
			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerInvoice, true));

			AssertEquals("Invoice Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerInvoiceLinePerShipment, true));
			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerTariffLinePerShipment, true));

			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerHTSCodePerShipment, true));
			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.PerHTSCodePerInvoice, true));
			AssertEquals("Tariff Lines", RateLineTypeList.GetTypeOfLine(RateLineTypeList.Codes.TotalHTSCountPerDeclaration, true));
		}

		public void TestGetDescriptiveHeaderText()
		{
			AssertEquals("for each entry", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerInvoiceLinePerEntry));
			AssertEquals("for each invoice", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerInvoiceLinePerInvoice));
			AssertEquals("for shipment", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerInvoiceLinePerShipment));

			AssertEquals("for each entry", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerTariffLinePerEntry));
			AssertEquals("for each invoice", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerTariffLinePerInvoice));
			AssertEquals("for shipment", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerTariffLinePerShipment));

			AssertEquals("for shipment", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerHTSCodePerShipment));
			AssertEquals("for each invoice", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.PerHTSCodePerInvoice));

			AssertEquals("for declaration", RateLineTypeList.GetDescriptiveHeaderText(RateLineTypeList.Codes.TotalHTSCountPerDeclaration));
		}
	}
}
