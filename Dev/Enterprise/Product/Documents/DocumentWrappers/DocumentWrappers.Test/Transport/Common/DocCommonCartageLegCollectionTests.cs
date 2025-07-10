using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonCartageLegCollection))]
	sealed class DocCommonCartageLegCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCommonCartageLegCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			return DocCommonCartageLeg.New(leg, Factory);
		}

		protected override DocCommonCartageLegCollection GetCollectionToTest()
		{
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			return new DocCommonCartageLegCollection(move);
		}

		public void TestSortOnPlannedPickupTime()
		{
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage1.JJ_ConsignmentID = "TCARTAGE1";

			CommonBookedCtgMove move1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonWorkSheet workSheet1 = Factory.New<CommonWorkSheet>();
			leg1.JU_EY_RunSheet = workSheet1.PK;
			workSheet1.EY_DriversName = "Driver1";

			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			CommonWorkSheet workSheet2 = Factory.New<CommonWorkSheet>();
			leg2.JU_EY_RunSheet = workSheet2.PK;
			workSheet2.EY_DriversName = "Driver2";

			CommonCartageLeg leg3 = move1.CartageLegs.AddNew();
			CommonWorkSheet workSheet3 = Factory.New<CommonWorkSheet>();
			leg3.JU_EY_RunSheet = workSheet3.PK;
			workSheet1.EY_DriversName = "Driver3";

			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_ConsignmentID = "TCARTAGE2";

			CommonBookedCtgMove move2 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg leg4 = move2.CartageLegs.AddNew();
			CommonCartageLeg leg5 = move2.CartageLegs.AddNew();

			CommonWorkSheet workSheet4 = Factory.New<CommonWorkSheet>();
			leg4.JU_EY_RunSheet = workSheet4.PK;
			workSheet4.EY_DriversName = "Driver4";

			CommonWorkSheet workSheet5 = Factory.New<CommonWorkSheet>();
			leg5.JU_EY_RunSheet = workSheet5.PK;
			workSheet5.EY_DriversName = "Driver5";

			DocCommonCartageLeg leg1Wrapper = DocCommonCartageLeg.New(leg1, Factory);
			DocCommonCartageLeg leg2Wrapper = DocCommonCartageLeg.New(leg2, Factory);
			DocCommonCartageLeg leg3Wrapper = DocCommonCartageLeg.New(leg3, Factory);
			DocCommonCartageLeg leg4Wrapper = DocCommonCartageLeg.New(leg4, Factory);
			DocCommonCartageLeg leg5Wrapper = DocCommonCartageLeg.New(leg5, Factory);
			DocCommonCartageLegCollection coll = new DocCommonCartageLegCollection(Factory);
			coll.Add(leg1Wrapper);
			coll.Add(leg2Wrapper);
			coll.Add(leg3Wrapper);
			coll.Add(leg4Wrapper);
			coll.Add(leg5Wrapper);

			leg1.JU_PlannedPickupTime = ZDateTime.Today;
			leg2.JU_PlannedPickupTime = ZDateTime.Today.AddDays(4);
			leg3.JU_PlannedPickupTime = ZDateTime.Today.AddDays(3);
			leg4.JU_PlannedPickupTime = ZDateTime.Today.AddDays(2);
			leg5.JU_PlannedPickupTime = ZDateTime.Today.AddDays(1);

			coll.SortOnPlannedPickupTime();
			AssertEquals(leg1Wrapper, coll[0]);
			AssertEquals(leg5Wrapper, coll[1]);
			AssertEquals(leg4Wrapper, coll[2]);
			AssertEquals(leg3Wrapper, coll[3]);
			AssertEquals(leg2Wrapper, coll[4]);
		}
	}
}
