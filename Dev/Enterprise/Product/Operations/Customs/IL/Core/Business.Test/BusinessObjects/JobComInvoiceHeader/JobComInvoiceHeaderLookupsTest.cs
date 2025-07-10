using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestCountryList()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertType<RefCountryCollection>(invoiceHeader.Lookups.CountryList);
		}

		public void TestInvoiceTypeList()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("325, 326, 380, I01, I02, I03, I04", invoiceHeader.Lookups.InvoiceTypeList.CodesAsString);
		}

		public void TestPreferenceDocumentTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateTradeGroup(Core.Constants.CountryCodes.Israel, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("CN", invoiceHeader.Lookups.PreferenceDocumentTypeList.CodesAsString);
		}

		public void TestPaymentTermsList()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("1, 2, 3", invoiceHeader.Lookups.PaymentTermsList.CodesAsString);
		}
	}
}
