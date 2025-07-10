using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocalTransportLegWrapperCollection))]
	sealed class LocalTransportLegWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<LocalTransportLegWrapperCollection>
	{
		protected override LocalTransportLegWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new LocalTransportLegWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var leg = Factory.New<CommonCartageLeg>();
			return new LocalTransportLegWrapper(leg, Factory);
		}

		public void TestSort()
		{
			var moveA = Factory.New<CommonBookedCtgMove>();
			var moveB = Factory.New<CommonBookedCtgMove>();
			moveA.EW_DisplayOrder = 2;
			moveB.EW_DisplayOrder = 1;

			var legA = moveA.CartageLegs.AddNew();
			var legB = moveB.CartageLegs.AddNew();
			var legC = moveA.CartageLegs.AddNew();
			var legD = moveB.CartageLegs.AddNew();

			legA.JU_DisplayOrder = 1;
			legB.JU_DisplayOrder = 3;
			legC.JU_DisplayOrder = 2;
			legD.JU_DisplayOrder = 1;

			legA.JU_RunSheetSequence = 3;
			legB.JU_RunSheetSequence = 2;
			legC.JU_RunSheetSequence = 1;
			legD.JU_RunSheetSequence = 4;

			legA.JU_PlannedPickupTime = ZDateTime.Now.AddHours(2);
			legB.JU_PlannedPickupTime = ZDateTime.Now.AddHours(3);
			legC.JU_PickupTimeIn = ZDateTime.Now.AddHours(1);
			legD.JU_PickupTimeIn = ZDateTime.Now.AddHours(4);

			AssertLegOrder(LocalTransportLegWrapperCollection.SortBy.DisplayOrder, legD, legB, legA, legC);
			AssertLegOrder(LocalTransportLegWrapperCollection.SortBy.Sequence, legC, legB, legA, legD);
			AssertLegOrder(LocalTransportLegWrapperCollection.SortBy.PlannedPickup, legC, legA, legB, legD);
		}

		void AssertLegOrder(LocalTransportLegWrapperCollection.SortBy sortBy, params CommonCartageLeg[] legs)
		{
			var wrappers = new LocalTransportLegWrapperCollection(Factory);
			wrappers.Add(new LocalTransportLegWrapper(legs[3], Factory));
			wrappers.Add(new LocalTransportLegWrapper(legs[0], Factory));
			wrappers.Add(new LocalTransportLegWrapper(legs[2], Factory));
			wrappers.Add(new LocalTransportLegWrapper(legs[1], Factory));

			wrappers.Sort(sortBy);
			AssertEquals(legs[0], wrappers[0].LegBO);
			AssertEquals(legs[1], wrappers[1].LegBO);
			AssertEquals(legs[2], wrappers[2].LegBO);
			AssertEquals(legs[3], wrappers[3].LegBO);
		}
	}
}
