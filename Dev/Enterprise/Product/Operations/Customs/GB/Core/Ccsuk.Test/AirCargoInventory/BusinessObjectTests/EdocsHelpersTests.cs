using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class EdocsHelpersTests : TestCaseWithFactory
	{
		public void TestCusMawbDocManagerInfo()
		{
			var mawb = Factory.New<CusMAWB>();
			var manager = ((IDocManagerSupport)mawb).DocManagerInfo;
			AssertType(typeof(CusMawbDocManagerInfo), manager);
			var hawb = mawb.ChildBills.AddNew();
			var mawbMessage = mawb.Messages.AddNew();
			var hawbMessage = hawb.Messages.AddNew();
			var testManager = new CusMawbDocManagerInfoForTest(mawb);
			AssertCollectionContains(mawbMessage, testManager.GetRelatedObjects_Exposed());
			AssertCollectionNotContains(hawbMessage, testManager.GetRelatedObjects_Exposed());
		}

		public void TestCusHawbDocManagerInfo()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var manager = ((IDocManagerSupport)hawb).DocManagerInfo;
			AssertType(typeof(CusHawbDocManagerInfo), manager);
			var mawbMessage = mawb.Messages.AddNew();
			var hawbMessage = hawb.Messages.AddNew();
			var testManager = new CusHawbDocManagerInfoForTest(hawb);
			AssertCollectionNotContains(mawbMessage, testManager.GetRelatedObjects_Exposed());
			AssertCollectionContains(hawbMessage, testManager.GetRelatedObjects_Exposed());
		}

		class CusMawbDocManagerInfoForTest : CusMawbDocManagerInfo
		{
			public CusMawbDocManagerInfoForTest(CusMAWB mawb)
				: base(mawb)
			{ }

			public BusinessObject[] GetRelatedObjects_Exposed()
			{
				return base.GetRelatedObjects();
			}
		}

		class CusHawbDocManagerInfoForTest : CusHawbDocManagerInfo
		{
			public CusHawbDocManagerInfoForTest(CusHAWB hawb)
				: base(hawb)
			{ }

			public BusinessObject[] GetRelatedObjects_Exposed()
			{
				return base.GetRelatedObjects();
			}
		}

		[TestedType(typeof(CusMawbDocManagerInfo))]
		class CusMawbDocManagerInfoTest : DocManagerInfoTestCase
		{
			public override BusinessObject GetEmptyParentBusinessObject()
			{
				return Factory.New<CusMAWB>();
			}

			public override BusinessObject GetPopulatedParentBusinessObject()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.Messages.AddNew();
				mawb.Messages.AddNew();
				return mawb;
			}
		}

		[TestedType(typeof(CusHawbDocManagerInfo))]
		class CusHawbDocManagerInfoTest : DocManagerInfoTestCase
		{
			public override BusinessObject GetEmptyParentBusinessObject()
			{
				return Factory.New<CusHAWB>();
			}

			public override BusinessObject GetPopulatedParentBusinessObject()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.Messages.AddNew();
				mawb.Messages.AddNew();
				return hawb;
			}
		}

		[TestedType(typeof(SplitConsignmentDocManagerInfo))]
		class SplitConsignmentDocManagerInfoTest : DocManagerInfoTestCase
		{
			public override BusinessObject GetEmptyParentBusinessObject()
			{
				return Factory.New<SplitBasic>();
			}

			public override BusinessObject GetPopulatedParentBusinessObject()
			{
				var basic = Factory.New<CusMAWB>();
				basic.Messages.AddNew();
				var split = basic.Splits.AddNew();
				split.SplitReference = "01";
				return split;
			}
		}
	}
}
