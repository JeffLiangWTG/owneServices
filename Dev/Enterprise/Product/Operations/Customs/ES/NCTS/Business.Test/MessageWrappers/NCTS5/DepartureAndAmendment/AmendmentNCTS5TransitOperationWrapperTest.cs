using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class AmendmentNCTS5TransitOperationWrapperTest : WrapperHelperTest<AmendmentNCTS5TransitOperationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if messageType is empty", typeof(ArgumentException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "messageType"), () => new AmendmentNCTS5TransitOperationWrapper(nctsHeader, ZString.Empty));
		}

		public void TestCommonTransitOperation()
		{
			var commonTransitOperation = wrapper.CommonTransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled CommonTransitOperation", commonTransitOperation);
				AssertSame("Cached CommonTransitOperation", wrapper.CommonTransitOperation, commonTransitOperation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = new AmendmentNCTS5TransitOperationWrapper(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure);
		}

		NctsHeader nctsHeader;
		AmendmentNCTS5TransitOperationWrapper wrapper;

		protected override AmendmentNCTS5TransitOperationWrapper GetProvider() => wrapper;
	}
}
