using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5SendMessageWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestTransitOperation()
		{
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled empty in DestinationCustomsOfficeCodeForArrival", ZString.Empty, wrapper.CustomsOfficeOfDestinationActual);
				nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "FR008889";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected filled DestinationCustomsOfficeCodeForArrival", "FR008889", wrapper.CustomsOfficeOfDestinationActual);
			});
		}

		public void TestNullTraderAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.TraderAtDestination.ToString());
		}

		public void TestTraderAtDestination()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;

			wrapper = GetWrapper(nctsHeader);
			var representative = wrapper.TraderAtDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Destination Trader", wrapper.TraderAtDestination);
				AssertSame("Cached Destination Trader", wrapper.TraderAtDestination, representative);
			});
		}

		public void TestNullRepresentativeAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.RepresentativeAtDestination.ToString());
		}

		public void TestRepresentativeAtDestination()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = orgHeader.PK;

			wrapper = GetWrapper(nctsHeader);
			var representative = wrapper.RepresentativeAtDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Representative", wrapper.RepresentativeAtDestination);
				AssertSame("Cached Representative", wrapper.RepresentativeAtDestination, representative);
			});
		}

		public void TestUnloadingRemarks()
		{
			var unloadingRemarks = wrapper.UnloadingRemarks;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled UnloadingRemarks", unloadingRemarks);
				AssertSame("Cached UnloadingRemarks", wrapper.UnloadingRemarks, unloadingRemarks);
			});
		}

		public void TestConsignment()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Consignment when BM_NoChangesToReport is true", wrapper.Consignment);

				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
				wrapper = GetWrapper(nctsHeader);
				var consignment = wrapper.Consignment;
				AssertNotNull("Expected filled Consignment when BM_NoChangesToReport is false", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NotificationUnloadingNCTS5SendMessageWrapper wrapper;

		NotificationUnloadingNCTS5SendMessageWrapper GetWrapper(NctsHeader header) => new NotificationUnloadingNCTS5SendMessageWrapper(header, Certificate);

		protected override NotificationUnloadingNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
