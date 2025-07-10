using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	abstract class NonPersistentC1ReleaseTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var awb = GetAwb();
			awb.NumberOfPiecesReceived = 9999;
			return new NonPersistentC1Release(awb);
		}

		protected abstract ICcsukCusAwb GetAwb();

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestAllNewProperties()
		{
			var awb = GetAwb();
			awb.NumberOfPiecesExpected = 10;
			awb.NumberOfPiecesReceived = 9;
			var c1ReleaseHelper = new NonPersistentC1Release(awb);
			AssertEquals(9, c1ReleaseHelper.PiecesRemainingToRelease);
			awb.ReleaseThisNumberOfPieces(2, NumberOfPiecesReleasedHelper.AgentC1Event);
			awb.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.AgentC1Event);
			AssertEquals(4, c1ReleaseHelper.PiecesRemainingToRelease);
			c1ReleaseHelper.NumberOfPieces = 2;
			AssertEquals(2, c1ReleaseHelper.NumberOfPieces);
			c1ReleaseHelper.NumberOfPieces = 4;
			AssertEquals(4, c1ReleaseHelper.NumberOfPieces);
			c1ReleaseHelper.NumberOfPieces = 5;
			AssertEquals("Setting NOP to something over the available/unreleased count resets to the available/unreleased count", 4, c1ReleaseHelper.NumberOfPieces);
			awb.ReleaseThisNumberOfPieces(4, NumberOfPiecesReleasedHelper.AgentC1Event); // all now released
			c1ReleaseHelper.NumberOfPieces = 1;
			AssertEquals("Reset to available/unreleased count, which is 0", 0, c1ReleaseHelper.NumberOfPieces);
			AssertEquals("Reset to available/unreleased count, which is 0", 0, c1ReleaseHelper.PiecesRemainingToRelease);
			AssertStartsWith("Log records releases", string.Format("2 piece(s) released for {0} by E at 1986-03-12", awb.Profile), c1ReleaseHelper.LogOfReleases);
			AssertContains("Log records releases", string.Format("\r\n3 piece(s) released for {0} by E at 1986-03-12", awb.Profile), c1ReleaseHelper.LogOfReleases);
			AssertContains("Log records releases", string.Format("\r\n4 piece(s) released for {0} by E at 1986-03-12", awb.Profile), c1ReleaseHelper.LogOfReleases);
			AssertEquals(false, c1ReleaseHelper.NumberOfPiecesInfo.ReadOnly);
			c1ReleaseHelper.NumberOfPiecesReadOnly = true;
			AssertEquals(true, c1ReleaseHelper.NumberOfPiecesInfo.ReadOnly);
			c1ReleaseHelper.NumberOfPiecesReadOnly = false;
			AssertEquals(false, c1ReleaseHelper.NumberOfPiecesInfo.ReadOnly);
		}

		public void TestValidation()
		{
			var awb = GetAwb();
			awb.NumberOfPiecesExpected = 10;
			awb.NumberOfPiecesReceived = 10;
			var c1ReleaseHelper = new NonPersistentC1Release(awb);
			c1ReleaseHelper.NumberOfPieces = 0;
			AssertHasErrorContaining(c1ReleaseHelper.NumberOfPiecesInfo, "select at least one piece");
			c1ReleaseHelper.NumberOfPieces = 1;
			AssertNoErrorContaining(c1ReleaseHelper.NumberOfPiecesInfo, "select at least one piece");
		}
	}

	namespace ERTS
	{
		class NonPersistentErtsReleaseOrchestratorTest : TestCaseWithFactory
		{
			public void TestDefaultvailablePieces()
			{
				var basic = Factory.New<CusMAWB>();
				basic.Profile = "CUKAIR98LHRABC";
				var ertsReleaseHelper = new NonPersistentErtsReleaseOrchestrator(basic);
				AssertEquals(0, ertsReleaseHelper.ErtsReleaseHelper.NumberOfPieces);
				var ot1 = basic.OutTurns.AddNew();
				ot1.C5_PackagesOutturned = 69;
				var ot2 = basic.OutTurns.AddNew();
				ot2.C5_PackagesOutturned = 70;
				basic.NumberOfPiecesReceived = 140; // TODO once the NPR field becomes readonly even for sheds - replace this with an explicit or implicit SAVE()
				ertsReleaseHelper = new NonPersistentErtsReleaseOrchestrator(basic);
				AssertEquals(0, ertsReleaseHelper.ErtsReleaseHelper.NumberOfPieces);
				ot1.IsBeingReleasedNow = true;
				ertsReleaseHelper = new NonPersistentErtsReleaseOrchestrator(basic);
				AssertEquals(69, ertsReleaseHelper.ErtsReleaseHelper.NumberOfPieces);
				ot2.IsBeingReleasedNow = true;
				ertsReleaseHelper = new NonPersistentErtsReleaseOrchestrator(basic);
				AssertEquals(139, ertsReleaseHelper.ErtsReleaseHelper.NumberOfPieces);
			}
		}
	}

	namespace C1
	{
		[TestedType(typeof(NonPersistentC1Release))]
		class Basic : NonPersistentC1ReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var m = Factory.New<CusMAWB>();
				m.Profile = "CUKFFW98000XXX";
				return m;
			}
		}

		[TestedType(typeof(NonPersistentC1Release))]
		class House : NonPersistentC1ReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var mawb = Factory.New<CusMAWB>();
				var h = mawb.ChildBills.AddNew();
				h.Profile = "CUKFFW98000XXX";
				return h;
			}
		}

		[TestedType(typeof(NonPersistentC1Release))]
		class SplitHouse : NonPersistentC1ReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.Profile = "CUKFFW98000XXX";
				var split = hawb.Splits.AddNew();
				split.SplitReference = "01";
				return split;
			}
		}

		[TestedType(typeof(NonPersistentC1Release))]
		class SplitBasic : NonPersistentC1ReleaseTests
		{
			protected override ICcsukCusAwb GetAwb()
			{
				var basic = Factory.New<CusMAWB>();
				basic.Profile = "CUKFFW98000XXX";
				var split = basic.Splits.AddNew();
				split.SplitReference = "01";
				return split;
			}
		}
	}
}
