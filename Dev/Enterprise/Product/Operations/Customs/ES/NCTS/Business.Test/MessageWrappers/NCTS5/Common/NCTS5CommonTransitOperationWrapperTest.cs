using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonTransitOperationWrapperTest : WrapperHelperTest<NCTS5CommonTransitOperationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => new NCTS5CommonTransitOperationWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => new NCTS5CommonTransitOperationWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestDeclarationType()
		{
			departureMovement.BM_InBondEntryType = "TIR";
			AssertEquals("Expected filled DeclarationType", "TIR", wrapper.DeclarationType);
		}

		public void TestTIRCarnetNumber()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_InBondEntryType = "T";
				departureMovement.TirCarnetNumber = "CarnetNumber";
				AssertEquals("Expected empty TIRCarnetNumber when declarationType is not TIR", ZString.Empty, wrapper.TIRCarnetNumber);

				departureMovement.BM_InBondEntryType = "TIR";
				departureMovement.TirCarnetNumber = "CarnetNumber";
				AssertEquals("Expected filled TIRCarnetNumber when declarationType is TIR", "CarnetNumber", wrapper.TIRCarnetNumber);
			});
		}

		public void TestSecurity()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_TypeOfSecurity = "NON";
				AssertEquals("Expected filled Security with 0 when NON", "0", wrapper.Security);

				departureMovement.BM_TypeOfSecurity = "EXI";
				AssertEquals("Expected filled Security with 2 when EXI", "2", wrapper.Security);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = new NCTS5CommonTransitOperationWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonTransitOperationWrapper wrapper;

		protected override NCTS5CommonTransitOperationWrapper GetProvider() => wrapper;
	}
}
