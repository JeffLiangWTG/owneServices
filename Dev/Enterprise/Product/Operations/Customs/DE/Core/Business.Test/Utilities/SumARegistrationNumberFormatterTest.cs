using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class SumARegistrationNumberFormatterTest : TestCaseWithFactory
	{
		public void TestFormatATB()
		{
			var formattedATB = SumARegistrationNumberFormatter.Format("ATB150000010320006000");
			AssertEquals("AT/B/15/000001/03/2000/6000", formattedATB);
			formattedATB = SumARegistrationNumberFormatter.Format("ATB150000010320006000EXTRA");
			AssertEquals("AT/B/15/000001/03/2000/6000EXTRA", formattedATB);
			formattedATB = SumARegistrationNumberFormatter.Format("ATB15SHORT");
			AssertEquals("AT/B/15/SHORT", formattedATB);
			formattedATB = SumARegistrationNumberFormatter.Format("ATB#15$0//000&*%$#010320?][006000");
			AssertEquals("AT/B/15/000001/03/2000/6000", formattedATB);
		}

		public void TestFormatMRN()
		{
			var formattedMRN = SumARegistrationNumberFormatter.Format("24DE5866D0000123U6");
			AssertEquals("MRN must left unformatted", "24DE5866D0000123U6", formattedMRN);
		}
	}
}
