using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	abstract class NonPersistentErtsReleaseTests : NonPersistentC1ReleaseTests
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var awb = GetAwb();
			awb.NumberOfPiecesReceived = 9999;
			return new NonPersistentErtsRelease(awb);
		}

		public void TestStatus2Granted()
		{
			var awb = GetAwb();
			var releaseHelper = new NonPersistentC1Release(awb);
			SetStatus2GrantedForTest(true);
			AssertEquals("Granted", releaseHelper.Status2Granted);
			AssertNoWarningContaining(releaseHelper.Status2GrantedInfo, "status 2");
			SetStatus2GrantedForTest(false);
			releaseHelper = new NonPersistentC1Release(awb);
			AssertEquals("Revoked", releaseHelper.Status2Granted);
			AssertHasWarningContaining(releaseHelper.Status2GrantedInfo, "status 2");
		}

		protected abstract void SetStatus2GrantedForTest(bool isGranted);
	}

	namespace RRA
	{
		[TestedType(typeof(NonPersistentErtsRelease))]
		class Basic : NonPersistentErtsReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				basic = Factory.New<CusMAWB>();
				basic.Profile = "CUKAIR98LHRXXX";
				return basic;
			}

			protected override void SetStatus2GrantedForTest(bool p)
			{
				basic.Status2Granted = p;
			}

			CusMAWB basic;
		}

		[TestedType(typeof(NonPersistentErtsRelease))]
		class House : NonPersistentErtsReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var mawb = Factory.New<CusMAWB>();
				house = mawb.ChildBills.AddNew();
				house.Profile = "CUKAIR98LHRXXX";
				return house;
			}

			protected override void SetStatus2GrantedForTest(bool p)
			{
				house.Status2Granted = p;
			}
			CusHAWB house;
		}

		[TestedType(typeof(NonPersistentErtsRelease))]
		class SplitHouse : NonPersistentErtsReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.Profile = "CUKAIR98LHRXXX";
				split = (BusinessObjects.SplitHouse)hawb.Splits.AddNew();
				split.SplitReference = "01";
				return split;
			}

			protected override void SetStatus2GrantedForTest(bool p)
			{
				split.HAWB.Status2Granted = p;
			}
			BusinessObjects.SplitHouse split;
		}

		[TestedType(typeof(NonPersistentErtsRelease))]
		class SplitBasic : NonPersistentErtsReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var basic = Factory.New<CusMAWB>();
				basic.Profile = "CUKAIR98LHRXXX";
				split = (BusinessObjects.SplitBasic)basic.Splits.AddNew();
				split.SplitReference = "01";
				return split;
			}

			protected override void SetStatus2GrantedForTest(bool p)
			{
				split.Basic.Status2Granted = p;
			}
			BusinessObjects.SplitBasic split;
		}
	}
}
