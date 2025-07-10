using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusAddInfo<CommunityHandlingCode>))]
	class CusAddInfoOfTypeCommunityHandlingCodeTest : CusAddInfoTest<CusAddInfo<CommunityHandlingCode>>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return chc;
		}

		protected override IEnumerable<CusAddInfo<CommunityHandlingCode>> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var mawb = factory.New<CusMAWB>();
			yield return mawb.CommunityHandlingCodes.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
			chc = mawb.CommunityHandlingCodes.AddNew();
		}

		CusAddInfo<CommunityHandlingCode> chc;
		CusMAWB mawb;
	}

	[TestedType(typeof(CommunityHandlingCode))]
	public class CommunityHandlingCodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var mawb = Factory.New<CusMAWB>();
			var chc = mawb.CommunityHandlingCodes.AddNew();
			return chc.Data;
		}

		public void TestAwb()
		{
			var mawb = Factory.New<CusMAWB>();
			var chc = mawb.CommunityHandlingCodes.AddNew().Data;
			AssertEquals(mawb, chc.Awb);

			var hawb = mawb.ChildBills.AddNew();
			var chc2 = hawb.CommunityHandlingCodes.AddNew().Data;
			AssertEquals(hawb, chc2.Awb);
		}

		public void TestSplits()
		{
			var basic = Factory.New<CusMAWB>();
			var chc = basic.CommunityHandlingCodes.AddNew().Data;
			AssertEquals(true, chc.C4_SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			var split1 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			var split2 = basic.Splits.AddNew();
			split2.SplitReference = "02";
			chc = basic.CommunityHandlingCodes.AddNew().Data;
			AssertEquals(false, chc.C4_SplitReferenceToWhichThisPertainsInfo.ReadOnly);
		}
	}
}
