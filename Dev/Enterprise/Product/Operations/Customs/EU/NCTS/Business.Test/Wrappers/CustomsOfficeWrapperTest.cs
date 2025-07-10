using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Moq;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CustomsOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeWrapper>
	{
		public void TestArrivalTime_Formatted()
		{
			AssertEquals("202011120701", wrapper.ArrivalTime);
		}

		public void TestArrivalTime_Empty()
		{
			var customsOfficeMock = new Mock<ICustomsOffice>();
			customsOfficeMock.Setup(x => x.ArrivalTime).Returns(ZDateTime.Empty);
			customsOfficeMock.Setup(x => x.OfficeCode).Returns("1234");

			wrapper = new CustomsOfficeWrapper(customsOfficeMock.Object);
			AssertEquals(ZString.Empty, wrapper.ArrivalTime);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("1234", wrapper.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var customsOfficeMock = new Mock<ICustomsOffice>();
			customsOfficeMock.Setup(x => x.ArrivalTime).Returns(new ZDateTime(2020, 11, 12, 7, 1, 12));
			customsOfficeMock.Setup(x => x.OfficeCode).Returns("1234");

			wrapper = new CustomsOfficeWrapper(customsOfficeMock.Object);
		}

		CustomsOfficeWrapper wrapper;

		protected override CustomsOfficeWrapper GetProvider() => wrapper;
	}
}
