using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2JobComInvoiceLineSynchroniserTest : TestCaseWithFactory
	{
		public void TestB2JobComInvoiceLineSynchroniser()
		{
			var source = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var destination = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var synchroniser = new B2JobComInvoiceLineSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);

			source.CA_OriginalLineNo = "LINE1";
			AssertEquals(source.CA_OriginalLineNo, destination.CA_OriginalLineNo);

			source.JI_Description = "test";
			AssertEquals("test", destination.JI_Description);

			source.CA_CustomsValue = 100m;
			AssertEquals(100m, destination.CA_CustomsValue);

			source.CA_AuthorityNumber = "X";
			AssertEquals("X", destination.CA_AuthorityNumber);

			source.JI_Tariff = "99038801";
			AssertEquals("99038801", destination.JI_Tariff);

			source.CA_99TariffCode = "0000";
			AssertEquals("0000", destination.CA_99TariffCode);

			source.CA_ValueForDutyCode = "13";
			AssertEquals("13", destination.CA_ValueForDutyCode);

			source.CA_CVforCurrConv = 10m;
			AssertEquals(source.CA_CVforCurrConv, destination.CA_CVforCurrConv);

			source.JI_CustomsUnitQty = "BG";
			AssertEquals("BG", destination.JI_CustomsUnitQty);

			source.JI_CustomsQuantity = 1m;
			AssertEquals(1m, destination.JI_CustomsQuantity);

			source.JI_CustomsSecondUnitQty = "BL";
			AssertEquals("BL", destination.JI_CustomsSecondUnitQty);

			source.JI_CustomsSecondQuantity = 6m;
			AssertEquals(6m, destination.JI_CustomsSecondQuantity);

			source.JI_CustomsThirdUnitQty = "BK";
			AssertEquals("BK", destination.JI_CustomsThirdUnitQty);

			source.JI_CustomsThirdQuantity = 7m;
			AssertEquals(7m, destination.JI_CustomsThirdQuantity);

			destination.JI_Description = "test1";
			source.JI_Description = "test2";
			AssertEquals("If the targer and source are different, it will not be synchronized", "test1", destination.JI_Description);

			destination.JI_Description = "test2";
			source.JI_Description = "test3";
			AssertEquals("If the targer and source are same, it will be synchronized", "test3", destination.JI_Description);
		}
	}
}
