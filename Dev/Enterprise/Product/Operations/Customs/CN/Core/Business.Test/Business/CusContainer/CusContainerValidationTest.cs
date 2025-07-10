using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestParent()
		{
			var parent = Factory.New<CusContainer>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public override void TestAirContainerIsMessageError()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			declaration.JE_ContainerMode = "";
			AssertEquals(false, declaration.ContainersAlwaysRequired);
			AssertEquals(true, declaration.ContainersRequired);
		}

		public new void TestCheckCO_RC()
		{
			base.TestCheckCO_RC();
			var cusContainer = Factory.New<CusContainer>();
			var refContainer = Factory.New<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("R1", Core.Constants.CountryCodes.UnitedStates);
			cusContainer.CO_RC = refContainer.PK;
			cusContainer.Validation.ValidateCO_RC();
			AssertHasMessageError(cusContainer.CO_RCInfo, "The selected Container Types doesn't have a code of China Customs. Please specify a China Customs Container Code in the 'Customs Container Codes' section of this Container Type.");
			refContainer.SetCountrySpecificContainerCode("R2", Core.Constants.CountryCodes.China);
			cusContainer.Validation.ValidateCO_RC();
			AssertNoMessageError(cusContainer.CO_RCInfo, "The selected Container Types doesn't have a code of China Customs. Please specify a China Customs Container Code in the 'Customs Container Codes' section of this Container Type.");
		}

		public void TestValidationModeProvider()
		{
			var declaration = GetJobDeclaration() as JobDeclaration;
			var container = declaration.CusContainers.AddNew();
			var validation = container.Validation;
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, validation.ValidationModeProvider);

			container = Factory.New<CusContainer>();
			validation = container.Validation;
			AssertNull(validation.ValidationModeProvider);
		}
	}
}
