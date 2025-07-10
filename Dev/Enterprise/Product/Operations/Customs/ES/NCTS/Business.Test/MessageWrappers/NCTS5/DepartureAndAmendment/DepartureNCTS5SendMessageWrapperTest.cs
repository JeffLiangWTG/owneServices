using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class DepartureNCTS5SendMessageWrapperTest : WrapperHelperTest<DepartureNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if messageType is empty", typeof(ArgumentException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "messageType"), () => new DepartureNCTS5SendMessageWrapper(nctsHeader, Certificate, ZString.Empty));
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

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = new DepartureNCTS5SendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.Ncts5Departure);
		}

		NctsHeader nctsHeader;
		DepartureNCTS5SendMessageWrapper wrapper;

		protected override DepartureNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
