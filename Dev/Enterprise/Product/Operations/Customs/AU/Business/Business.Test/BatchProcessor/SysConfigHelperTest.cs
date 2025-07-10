using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SysConfigHelperTest : TestCaseWithFactory
	{
		public void TestAUCCustomsCCFCurrentEmailAddress()
		{
			var refSysConfigLoader = new Universal.RefSysConfig.Loader(Factory);
			var currentCCFemailAddressLoadedFromSysConfig = refSysConfigLoader.Load(TestCurrentCCFAddress, ZDateTime.Today)?.ZRC_StringValue ?? ZString.Empty;
			AssertEquals("Test email address created", TestCurrentCCFAddressValue, currentCCFemailAddressLoadedFromSysConfig);
		}

		public void TestAUCCustomsCCFPreviousEmailAddress()
		{
			var refSysConfigLoader = new Universal.RefSysConfig.Loader(Factory);
			var previousCCFemailAddressLoadedFromSysConfig = refSysConfigLoader.Load(TestPreviousCCFAddress, ZDateTime.Today)?.ZRC_StringValue ?? ZString.Empty;
			AssertEquals("Test email address created for previous address", TestPreviousCCFAddressValue, previousCCFemailAddressLoadedFromSysConfig);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType(TestCurrentCCFAddress, "Current AU Customs CCF Email Address", "Current AU Customs CCF Email Address");
			helper.CreateRefSysConfig(TestCurrentCCFAddress, TestCurrentCCFAddressValue, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(2));
			helper.CreateRefSysConfigType(TestPreviousCCFAddress, "Current AU Customs CCF Email Address", "Current AU Customs CCF Email Address");
			helper.CreateRefSysConfig(TestPreviousCCFAddress, TestPreviousCCFAddressValue, ZDateTime.Today.AddYears(-3), ZDateTime.Today.AddDays(-2));
			Factory.Save();
		}

		const string TestCurrentCCFAddress = "TestCur";
		const string TestCurrentCCFAddressValue = "AUCustoms@government.au";
		const string TestPreviousCCFAddress = "TestPrev";
		const string TestPreviousCCFAddressValue = "AUCustoms@bettergovernment.au";
	}
}
