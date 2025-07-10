using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2JobComInvoiceHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestB2JobComInvoiceHeaderSynchroniser()
		{
			var source = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var destination = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var synchroniser = new B2JobComInvoiceHeaderSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);

			source.JZ_InvoiceNumber = "INV1";
			AssertEquals("INV1", destination.JZ_InvoiceNumber);

			source.JZ_RN_NKDefaultOrigin = "US";
			AssertEquals("US", destination.JZ_RN_NKDefaultOrigin);

			source.JZ_RW_NKOriginState = "TN";
			AssertEquals("TN", destination.JZ_RW_NKOriginState);

			source.CA_RN_NKExport = "US";
			AssertEquals("US", destination.CA_RN_NKExport);

			source.CA_USStateOfExport = "NY";
			AssertEquals("NY", destination.CA_USStateOfExport);

			source.CA_TreatmentCode = "02";
			AssertEquals("02", destination.CA_TreatmentCode);

			var date = new ZDateTime(2020, 3, 6);
			source.JZ_ValuationDateOverride = date;
			AssertEquals(date, destination.JZ_ValuationDateOverride);

			source.JZ_RX_NKInvoice_Currency = "CAD";
			AssertEquals("CAD", destination.JZ_RX_NKInvoice_Currency);

			source.CA_TimeLimit = 10;
			AssertEquals(10, destination.CA_TimeLimit);

			source.CA_TimeLimitCode = "D";
			AssertEquals("D", destination.CA_TimeLimitCode);

			source.CA_TradeZone = "49";
			AssertEquals("49", destination.CA_TradeZone);

			destination.JZ_RN_NKDefaultOrigin = "CA";
			source.JZ_RN_NKDefaultOrigin = "CN";
			AssertEquals("If the targer and source are different, it will not be synchronized", "CA", destination.JZ_RN_NKDefaultOrigin);

			destination.JZ_RN_NKDefaultOrigin = "CN";
			source.JZ_RN_NKDefaultOrigin = "US";
			AssertEquals("If the targer and source are same, it will be synchronized", "US", destination.JZ_RN_NKDefaultOrigin);
		}
	}
}
