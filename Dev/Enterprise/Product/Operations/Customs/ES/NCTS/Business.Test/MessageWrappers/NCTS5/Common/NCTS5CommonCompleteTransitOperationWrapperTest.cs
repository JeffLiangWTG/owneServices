using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCompleteTransitOperationWrapperTest : WrapperHelperTest<NCTS5CommonCompleteTransitOperationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if messageType is empty", typeof(ArgumentException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "messageType"), () => new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, ZString.Empty));
		}

		public void TestAdditionalDeclarationType()
		{
			CombineAssertions(() =>
			{
				wrapper = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
				AssertEquals("Expected filled AdditionalDeclarationType always with D when messageType is DPM", "D", wrapper.AdditionalDeclarationType);

				wrapper = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure);
				AssertEquals("Expected filled AdditionalDeclarationType always with A when messageType is DPT", "A", wrapper.AdditionalDeclarationType);

				wrapper = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
				AssertEquals("Expected filled AdditionalDeclarationType always with D when messageType is DPD", "D", wrapper.AdditionalDeclarationType);
			});
		}

		public void TestReducedDatasetIndicator()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_ReducedDatasetIndicator = false;
				AssertEquals("Expected false ReducedDatasetIndicator when flag not ticked", false, wrapper.ReducedDatasetIndicator);

				departureMovement.BM_ReducedDatasetIndicator = true;
				AssertEquals("Expected true ReducedDatasetIndicator when flag ticked", true, wrapper.ReducedDatasetIndicator);
			});
		}

		public void TestSpecificCircumstanceIndicator()
		{
			departureMovement.BM_SpecificCircumstance = "A20";
			AssertEquals("Expected filled SpecificCircumstanceIndicator", "A20", wrapper.SpecificCircumstanceIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonCompleteTransitOperationWrapper wrapper;

		protected override NCTS5CommonCompleteTransitOperationWrapper GetProvider() => wrapper;
	}
}
