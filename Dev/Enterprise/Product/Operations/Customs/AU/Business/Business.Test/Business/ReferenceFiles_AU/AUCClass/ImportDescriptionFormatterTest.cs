using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ImportDescriptionFormatterTest : TestCaseWithFactory
	{
		public void TestDescriptioveTariff()
		{
			var formatter = new ImportDescriptionFormatter(Factory, DescriptiveTariffNum1);
			AssertEquals("Should not have changed this description", DescriptiveTariffNumDescription1.ToUpper(), formatter.Description);
		}

		public void TestReplacementWordOther()
		{
			var formatter = new ImportDescriptionFormatter(Factory, DoNotReplaceOtherTariffNum1);
			AssertEquals("Should not have changed this description", DoNotReplaceOtherTariffNumDescription1.ToUpper(), formatter.Description);
		}

		public void TestOtherIfPartOfLongDescription()
		{
			var formatter = new ImportDescriptionFormatter(Factory, ReplaceOtherTariffNum1);
			Assert("Should have changed this description to a better one", ReplaceOtherTariffNumDescription1 != formatter.Description);
			Assert("Length Should be greater than 10", 10 < formatter.Description.Length);
		}

		public void TestExpectedReplacementDescription()
		{
			var formatter = new ImportDescriptionFormatter(Factory, ReplaceOtherTariffNum2);
			AssertEquals("Should change description", ReplaceOtherTariffNumDescription2.ToUpper(), formatter.Description);
		}

		const string DescriptiveTariffNum1 = "1704.10.00 42";
		const string DescriptiveTariffNumDescription1 = "Chewing gum, whether or not sugar-coated";

		const string DoNotReplaceOtherTariffNum1 = "0902.20.00 09";
		const string DoNotReplaceOtherTariffNumDescription1 = "Other green tea (not fermented)";

		const string ReplaceOtherTariffNum1 = "0208.90.00 28";
		const string ReplaceOtherTariffNumDescription1 = "Other";

		const string ReplaceOtherTariffNum2 = "0106.90.00 69";
		const string ReplaceOtherTariffNumDescription2 = "Other live animals excl mammals,reptiles,birds";
	}
}
