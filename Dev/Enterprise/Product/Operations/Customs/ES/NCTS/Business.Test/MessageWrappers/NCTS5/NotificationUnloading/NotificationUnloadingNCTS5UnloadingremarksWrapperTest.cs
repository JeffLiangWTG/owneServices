using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5UnloadingRemarksWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5UnloadingRemarksWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "moveHeader"), () => GetWrapper(null));
			});
		}

		public void TestConform()
		{
			CombineAssertions(() =>
			{
				arrivalMovementHeader.BM_NoChangesToReport = true;
				AssertEquals("Expected filled Conform true", true, wrapper.Conform);
				arrivalMovementHeader.BM_NoChangesToReport = false;
				AssertEquals("Expected filled Conform false", false, wrapper.Conform);
			});
		}

		public void TestUnloadingDate()
		{
			arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
			AssertEquals("Expected filled UnloadingDate with the datetime in BM_UnloadingDate", new ZDateTime(2022, 02, 15, 16, 43, 27), wrapper.UnloadingDate);
		}

		public void TestStateOfSeals()
		{
			CombineAssertions(() =>
			{
				arrivalMovementHeader.BM_StateOfSealsBoolean = true;
				AssertEquals("Expected filled StateOfSeals true", true, wrapper.StateOfSeals);
				arrivalMovementHeader.BM_StateOfSealsBoolean = false;
				AssertEquals("Expected filled StateOfSeals false", false, wrapper.StateOfSeals);
			});
		}

		public void TestStateOfSealsSpecified()
		{
			CombineAssertions(() =>
			{
				arrivalMovementHeader.BM_StateOfSealsBoolean = true;
				AssertEquals("Expected StateOfSealsSpecified false when no containers declared (in header or in incidents)", false, wrapper.StateOfSealsSpecified);

				var container1 = nctsHeader.ArrivalHeaderContainers.AddNew();
				container1.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				container1.Seals.AddNew();
				var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
				container2.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var container3 = nctsHeader.ArrivalHeaderContainers.AddNew();
				container3.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				container3.Seals.AddNew();

				AssertEquals("Expected StateOfSealsSpecified true when there is at least one container with status not NEW and sealqty > 0", true, wrapper.StateOfSealsSpecified);

				container1.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Expected StateOfSealsSpecified false when there is at least one container sealqty > 0 but status is NEW", false, wrapper.StateOfSealsSpecified);

				var incident1 = nctsHeader.EnRouteIncidents.AddNew();
				var incident1Container1 = incident1.IncidentContainers.AddNew();
				incident1Container1.Seals.AddNew();
				var incident1Container2 = incident1.IncidentContainers.AddNew();
				var incident2 = nctsHeader.EnRouteIncidents.AddNew();
				var incident2Container1 = incident2.IncidentContainers.AddNew();
				AssertEquals("Expected StateOfSealsSpecified true when there is at least one incident with container with sealqty > 0", true, wrapper.StateOfSealsSpecified);
				incident1Container1.Seals.RemoveAll();
				AssertEquals("Expected StateOfSealsSpecified false when there are no incidents with containers with sealqty > 0", false, wrapper.StateOfSealsSpecified);
			});
		}

		public void TestUnloadingRemark()
		{
			arrivalMovementHeader.BM_UnloadingRemarks = "unloading remarks";
			AssertEquals("Expected filled UnloadingRemark", "unloading remarks", wrapper.UnloadingRemark);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

			wrapper = GetWrapper(arrivalMovementHeader);
		}

		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovementHeader;
		NotificationUnloadingNCTS5UnloadingRemarksWrapper wrapper;

		NotificationUnloadingNCTS5UnloadingRemarksWrapper GetWrapper(NctsArrivalMovementHeader moveHeader) => new NotificationUnloadingNCTS5UnloadingRemarksWrapper(moveHeader);

		protected override NotificationUnloadingNCTS5UnloadingRemarksWrapper GetProvider() => wrapper;
	}
}
