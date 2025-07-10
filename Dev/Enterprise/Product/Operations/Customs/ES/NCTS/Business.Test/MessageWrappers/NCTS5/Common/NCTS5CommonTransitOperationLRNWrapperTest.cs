using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonTransitOperationLRNWrapperTest : WrapperHelperTest<NCTS5CommonTransitOperationLRNWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => new NCTS5CommonTransitOperationLRNWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => new NCTS5CommonTransitOperationLRNWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestLRN()
		{
			departureMovement.BM_PaperlessInbondNum = "TestLRN";
			AssertEquals("Expected filled LRN with only customer ref code when no Principal declared", "TestLRN", wrapper.LRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = new NCTS5CommonTransitOperationLRNWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonTransitOperationLRNWrapper wrapper;

		protected override NCTS5CommonTransitOperationLRNWrapper GetProvider() => wrapper;
	}
}
