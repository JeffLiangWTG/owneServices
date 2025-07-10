using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.Testing.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CcsukUtilities))]
	class CcsukUtilitiesTest : TestCaseWithFactory
	{
		public void TestFindDuplicateSplits()
		{
			var mawb = Factory.New<CusMAWB>();
			var split1 = mawb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2Isr = mawb.Splits.AddNew();
			split2Isr.SplitReference = "02";
			split2Isr.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			var split2Yes = mawb.Splits.AddNew();
			split2Yes.SplitReference = "02";
			split2Yes.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var split3 = mawb.Splits.AddNew();
			split3.SplitReference = "03";
			var split4Ass = mawb.Splits.AddNew();
			split4Ass.SplitReference = "04";
			split4Ass.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			var split4Com = mawb.Splits.AddNew();
			split4Com.SplitReference = "04";
			split4Com.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			split4Com.CG_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			var split4Yes = mawb.Splits.AddNew();
			split4Yes.SplitReference = "04";
			split4Yes.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;

			var result = mawb.FindDuplicateSplits();
			AssertEquals(2, result.Length);
			AssertEquals(split2Isr, result[0]);
			AssertEquals(split4Ass, result[1]);

			split4Ass.CG_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			split4Com.CG_CustomsStatus = ZString.Empty;

			result = mawb.FindDuplicateSplits();
			AssertEquals(2, result.Length);
			AssertEquals(split2Isr, result[0]);
			AssertEquals(split4Yes, result[1]);

			var emptyMawb = Factory.New<CusMAWB>();
			result = emptyMawb.FindDuplicateSplits();
			AssertEquals(0, result.Length);
		}

		public void TestHasDuplicateSplits()
		{
			var mawb = Factory.New<CusMAWB>();
			var split1 = mawb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2Isr = mawb.Splits.AddNew();
			split2Isr.SplitReference = "02";
			split2Isr.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			AssertEquals(false, mawb.HasDuplicateSplits());

			var split2Yes = mawb.Splits.AddNew();
			split2Yes.SplitReference = "02";
			split2Yes.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			AssertEquals(true, mawb.HasDuplicateSplits());
		}

		public void TestDeleteSplits()
		{
			var mawb = Factory.New<CusMAWB>();
			var split1 = mawb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2 = mawb.Splits.AddNew();
			split2.SplitReference = "02";
			var split3 = mawb.Splits.AddNew();
			split3.SplitReference = "03";
			var split4 = mawb.Splits.AddNew();
			split4.SplitReference = "04";

			mawb.DeleteSplits([split2, split3]);

			AssertEquals(2, mawb.Splits.Count);
			AssertEquals(split1, mawb.Splits[0]);
			AssertEquals(split4, mawb.Splits[1]);
			AssertEquals(true, split2.IsDeleted);
			AssertEquals(true, split3.IsDeleted);
		}
	}
}
