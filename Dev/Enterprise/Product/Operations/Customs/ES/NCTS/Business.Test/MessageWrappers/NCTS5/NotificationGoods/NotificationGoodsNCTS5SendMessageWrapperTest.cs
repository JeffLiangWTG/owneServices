using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class NotificationGoodsNCTS5SendMessageWrapperTest : WrapperHelperTest<NotificationGoodsNCTS5SendMessageWrapper>
	{
		public void TestTransitOperation()
		{
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		public void TestNullHolderOfTheTransitProcedure()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.HolderOfTheTransitProcedure.ToString());
		}

		public void TestHolderOfTheTransitProcedure()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeader);
				var holderOfTheTransitProcedure = wrapper.HolderOfTheTransitProcedure;

				AssertNotNull("Expected filled HolderOfTheTransitProcedure", holderOfTheTransitProcedure);
				AssertSame("Cached HolderOfTheTransitProcedure", wrapper.HolderOfTheTransitProcedure, holderOfTheTransitProcedure);
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NotificationGoodsNCTS5SendMessageWrapper wrapper;

		NotificationGoodsNCTS5SendMessageWrapper GetWrapper(NctsHeader nctsHeader) => new NotificationGoodsNCTS5SendMessageWrapper(nctsHeader, Certificate);

		protected override NotificationGoodsNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
