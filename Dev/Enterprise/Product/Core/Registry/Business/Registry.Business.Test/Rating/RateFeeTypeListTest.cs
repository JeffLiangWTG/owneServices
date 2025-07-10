namespace Enterprise.Registry.Business.Testing
{
	sealed class RateFeeTypeListTest : NUnit.Framework.TestCase
	{
		public void TestGetHeaderType()
		{
			AssertEquals("Entries", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerEntry, true));
			AssertEquals("Entry", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerEntry, false));

			AssertEquals("Entry Pages Per Entry", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerEntryPage, true));
			AssertEquals("Entry Page Per Entry", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerEntryPage, false));

			AssertEquals("Invoices", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerInvoice, true));
			AssertEquals("Invoice", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerInvoice, false));

			AssertEquals("Suppliers", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerSupplier, true));
			AssertEquals("Supplier", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerSupplier, false));

			AssertEquals("Shipments", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerShipment, true));
			AssertEquals("Shipment", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerShipment, false));

			AssertEquals("Sub Header", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerSubHeader, true));
			AssertEquals("Sub Header", RateFeeTypeList.GetHeaderType(RateFeeTypeList.Codes.PerSubHeader, false));
		}
	}
}
