using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusOutturnOutturnReportLineInformationAbstractTest : TestCaseWithFactory
	{
		public void TestDamageIndicator()
		{
			Outturn.C5_DamageIndicator = true;
			AssertEquals("DamageIndicator", true, HeaderInfo.DamageIndicator);
		}

		public void TestPillageIndicator()
		{
			Outturn.C5_PillageIndicator = true;
			AssertEquals("PillageIndicator", true, HeaderInfo.PillageIndicator);
		}

		public void TestNumberOfPackages()
		{
			Outturn.C5_PackagesOutturned = 100;
			AssertEquals("NumberOfPackages", 100, HeaderInfo.NumberOfPackages);
		}

		public void TestOutturnResultType()
		{
			Outturn.C5_OutturnResultType = "ABC";
			AssertEquals("OutturnResultType", "ABC", HeaderInfo.OutturnResultType);
		}

		protected abstract CusOutturnOutturnReportLineInformation GetHeaderInfo();

		CusOutturn outturn;
		protected CusOutturn Outturn => outturn ?? (outturn = Factory.New<CusOutturn>());

		CusOutturnOutturnReportLineInformation HeaderInfo => GetHeaderInfo();
	}
}
