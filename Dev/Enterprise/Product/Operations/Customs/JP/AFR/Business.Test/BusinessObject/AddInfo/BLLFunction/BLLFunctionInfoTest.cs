using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLFunctionInfo))]
	class BLLFunctionInfoTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<BLLFunctionInfo>
	{
		public void TestBillNumbers()
		{
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			AssertEquals(ZString.Empty, bllFunctionInfo.JP_LinkedBills);

			bllFunctionInfo.LinkedBills = new ZString[] { "123", "456", "789" };
			AssertEquals($"123{SplitChar}456{SplitChar}789", bllFunctionInfo.JP_LinkedBills);

			bllFunctionInfo.JP_LinkedBills = $"1{SplitChar}2{SplitChar}3";
			AssertEquals(3, bllFunctionInfo.LinkedBills.Count);
			AssertEquals("1", bllFunctionInfo.LinkedBills[0]);
			AssertEquals("2", bllFunctionInfo.LinkedBills[1]);
			AssertEquals("3", bllFunctionInfo.LinkedBills[2]);

			bllFunctionInfo.JP_LinkedBills = $"1{SplitChar}2{SplitChar}3{SplitChar}";
			AssertEquals(4, bllFunctionInfo.LinkedBills.Count);
			AssertEquals("1", bllFunctionInfo.LinkedBills[0]);
			AssertEquals("2", bllFunctionInfo.LinkedBills[1]);
			AssertEquals("3", bllFunctionInfo.LinkedBills[2]);
			AssertEquals("", bllFunctionInfo.LinkedBills[3]);
		}

		protected override IEnumerable<BLLFunctionInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var bllFunctionInfo = factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill.PK;
			bllFunctionInfo.B7_ParentTableCode = bill.TablePrefix;
			yield return bllFunctionInfo;
		}

		public const char SplitChar = '$';
	}
}
