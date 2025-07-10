using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NotificationGoodsNCTS5ConsignmentWrapperTest : WrapperHelperTest<NotificationGoodsNCTS5ConsignmentWrapper>
	{
		public void TestHouseConsignment()
		{
			var houseConsignment = wrapper.HouseConsignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected null for provisional Period  HouseConsignment", houseConsignment);
				AssertSame("Cached HouseConsignment", wrapper.HouseConsignment, houseConsignment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = GetWrapper(nctsHeader);
		}

		NotificationGoodsNCTS5ConsignmentWrapper wrapper;

		NotificationGoodsNCTS5ConsignmentWrapper GetWrapper(NctsHeader nctsHeader) => new NotificationGoodsNCTS5ConsignmentWrapper(nctsHeader);

		protected override NotificationGoodsNCTS5ConsignmentWrapper GetProvider() => wrapper;
	}
}
