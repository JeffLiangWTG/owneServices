using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5TransitOperationWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5TransitOperationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => new NotificationUnloadingNCTS5TransitOperationWrapper(Factory.New<NctsHeader>()));
		}

		public void TestOtherThingsToReport()
		{
			nctsHeader.ArrivalMovementHeader.OtherThingsToReport = "I'm reporting some other things that might be important";
			AssertEquals("Expected filled OtherThingsToReport", "I'm reporting some other things that might be important", wrapper.OtherThingsToReport);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = new NotificationUnloadingNCTS5TransitOperationWrapper(nctsHeader);
		}

		NotificationUnloadingNCTS5TransitOperationWrapper wrapper;
		NctsHeader nctsHeader;

		protected override NotificationUnloadingNCTS5TransitOperationWrapper GetProvider() => wrapper;
	}
}
