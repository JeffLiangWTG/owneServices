using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	public class ServiceTaskHelperTest : TestCaseWithFactory
	{
		public void TestIfZero()
		{
			AssertEquals(50, ServiceTaskHelper.IfZero(0, 50));
			AssertEquals(20, ServiceTaskHelper.IfZero(20, 50));
		}

		public void TestGetFallbackEntryNumbersInStatus()
		{
			var cusEntryNum1 = CreateCusEntryNum("PPW", "FBK", "FR");
			var cusEntryNum2 = CreateCusEntryNum("PPS", "FBK", "FR");
			var cusEntryNum3 = CreateCusEntryNum("PPS", "IMP", "FR");
			var cusEntryNum4 = CreateCusEntryNum("PPS", "FBK", "DE");
			Factory.Save();

			var qureyResult = ServiceTaskHelper.GetFallbackEntryNumbersInStatus(Factory, "PPW", GlbBranch.CurrentBranch.Country.Code);
			AssertCollectionContains(cusEntryNum1, qureyResult);
			AssertCollectionNotContains(cusEntryNum2, qureyResult);
			AssertCollectionNotContains(cusEntryNum3, qureyResult);
			AssertCollectionNotContains(cusEntryNum4, qureyResult);

			qureyResult = ServiceTaskHelper.GetFallbackEntryNumbersInStatus(Factory, "PPS", GlbBranch.CurrentBranch.Country.Code);
			AssertCollectionNotContains(cusEntryNum1, qureyResult);
			AssertCollectionContains(cusEntryNum2, qureyResult);
			AssertCollectionNotContains(cusEntryNum3, qureyResult);
			AssertCollectionNotContains(cusEntryNum4, qureyResult);
		}

		CusEntryNumber CreateCusEntryNum(ZString entryStatus, ZString fallbackEntryType, ZString countryCode)
		{
			var cusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusEntryNum.CE_EntryStatus = entryStatus;
			cusEntryNum.CE_EntryType = fallbackEntryType;
			cusEntryNum.CE_RN_NKCountryCode = countryCode;
			cusEntryNum.CE_ParentTable = "JobDeclaration";
			return cusEntryNum;
		}
	}
}
