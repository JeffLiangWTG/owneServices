using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	[TestedType(typeof(DocCommonCartage))]
	sealed class DocCartageContainerTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCommonCartage.New(Cartage, Factory) };
		}

		#region Cartage Cover/Summary Sheet Fields

		public void TestCartageContainerFieldCartageParent()
		{
			Cartage.ContainerBookedMoves.DeleteAll();
			Cartage.JJ_GoodsDescription = "TEST";
			CommonContainer container = Cartage.ContainerBookedMoves.AddNew().Container;

			AssertEquals("Incorrect CartageParent found.", "TEST", CartageWrapper.Containers[0].Cartage.GoodsDescription);
		}

		public void TestCartageContainerFieldContainerType()
		{
			Cartage.ContainerBookedMoves.DeleteAll();
			CommonContainer container = Cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerMode = "TYP";

			AssertEquals("Incorrect Container Type found.", "TYP", CartageWrapper.Containers[0].ContainerType);
		}

		public void TestCartageContainerFieldArrivalSlotDetails()
		{
			Cartage.ContainerBookedMoves.DeleteAll();
			CommonContainer container = Cartage.ContainerBookedMoves.AddNew().Container;
			AssertEquals("Incorrect Container Slot Details found for arrival with no date or reference.", " - ", CartageWrapper.Containers[0].SlotArrivalDetails);

			container.JC_ArrivalSlotReference = "ARR-REF";
			AssertEquals("Incorrect Container Slot Details found for arrival with no date.", "ARR-REF", CartageWrapper.Containers[0].SlotArrivalDetails);

			ZDateTime arrivalTime = ZDateTime.Now.AddDays(1);
			container.JC_ArrivalSlotDateTime = arrivalTime;

			ZString slotArrivalDetails = CartageWrapper.Containers[0].SlotArrivalDetails;
			ZString slotDepartureDetails = CartageWrapper.Containers[0].SlotDepartureDetails;

			AssertEquals("Incorrect Container Slot Details found for arrival.", "ARR-REF / " + arrivalTime.ToShortDateString() + " " + arrivalTime.ToShortTimeString(), slotArrivalDetails);
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			ZString slotDetails = (CartageWrapper.Containers[0].SlotAsArrivalOrDeparture == "A") ? slotArrivalDetails : ZString.Empty;
			AssertEquals("Incorrect Container Slot Details found for non-arrival.", "", slotDetails);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			slotDetails = (CartageWrapper.Containers[0].SlotAsArrivalOrDeparture == "A") ? slotArrivalDetails : ZString.Empty;
			AssertEquals("Incorrect Container Slot Details found for arrival.", slotArrivalDetails, slotDetails);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			slotDetails = CartageWrapper.SlotAsArrivalOrDeparture;
			if (CartageWrapper.SlotAsArrivalOrDeparture == "D")
			{
				slotDetails = slotDepartureDetails;
			}
			else if (CartageWrapper.SlotAsArrivalOrDeparture == "A")
			{
				slotDetails = slotArrivalDetails;
			}
			AssertEquals("Incorrect Container Slot Details found for non-arrival/non-departure.", " - ", slotDetails);
		}
		public void TestCartageContainerFieldDepartureSlotDetails()
		{
			Cartage.ContainerBookedMoves.DeleteAll();
			CommonContainer container = Cartage.ContainerBookedMoves.AddNew().Container;
			AssertEquals("Incorrect Container Slot Details found for departure with no date or reference.", " - ", CartageWrapper.Containers[0].SlotDepartureDetails);

			container.JC_DepartureSlotReference = "DEP-REF";
			AssertEquals("Incorrect Container Slot Details found for departure with no date.", "DEP-REF", CartageWrapper.Containers[0].SlotDepartureDetails);

			ZDateTime departureTime = ZDateTime.Now.AddDays(2);
			container.JC_DepartureSlotDateTime = departureTime;

			ZString slotArrivalDetails = CartageWrapper.Containers[0].SlotArrivalDetails;
			ZString slotDepartureDetails = CartageWrapper.Containers[0].SlotDepartureDetails;
			AssertEquals("Incorrect Container Slot Details found for departure.", "DEP-REF / " + departureTime.ToShortDateString() + " " + departureTime.ToShortTimeString(), slotDepartureDetails);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			ZString slotDetails = (CartageWrapper.SlotAsArrivalOrDeparture == "D") ? slotDepartureDetails : ZString.Empty;
			AssertEquals("Incorrect Container Slot Details found for non-arrival.", slotDepartureDetails, slotDetails);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			slotDetails = (CartageWrapper.SlotAsArrivalOrDeparture == "D") ? slotDepartureDetails : ZString.Empty;
			AssertEquals("Incorrect Container Slot Details found for non-departure.", "", slotDetails);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			slotDetails = CartageWrapper.SlotAsArrivalOrDeparture;
			if (CartageWrapper.SlotAsArrivalOrDeparture == "D")
			{
				slotDetails = slotDepartureDetails;
			}
			else if (CartageWrapper.SlotAsArrivalOrDeparture == "A")
			{
				slotDetails = slotArrivalDetails;
			}
			AssertEquals("Incorrect Container Slot Details found for non-arrival/non-departure.", " - ", slotDetails);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Cartage = Factory.New<CommonCartage>();
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			base.SetUp();
		}

		CommonCartage Cartage;
		DocCommonCartage CartageWrapper;

		#endregion
	}
}
