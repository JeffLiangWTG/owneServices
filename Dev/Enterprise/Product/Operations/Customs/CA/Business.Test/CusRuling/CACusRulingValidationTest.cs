using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACusRulingValidationTest : TestCaseWithFactory
	{
		public void TestCheckZZX_RulingNumber()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			cusRuling.ZZX_RulingNumber = "OIC-123";
			Factory.Save();
			var cusRuling2 = Factory.NewWithValidTestData<CACusRuling>();
			cusRuling2.ZZX_RulingNumber = "OIC-123";
			AssertHasErrorContaining(cusRuling2.ZZX_RulingNumberInfo, "Remission Number already exists.");
			cusRuling2.ZZX_RulingNumber = "OIC-234";
			AssertNoErrorContaining(cusRuling2.ZZX_RulingNumberInfo, "Remission Number already exists.");
		}
	}
}
