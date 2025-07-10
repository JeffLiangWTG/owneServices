namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalProcedureWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalProcedureWrapper>
	{
		protected override AdditionalProcedureWrapper GetProvider()
		{
			var procedure = "F61";
			return AdditionalProcedureWrapper.New(procedure, string.Empty);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to FR as Customs Office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);
			var procedure = "F61";

			var wrapper = AdditionalProcedureWrapper.New(procedure, Core.Constants.CountryCodes.France);
			AssertEquals("CcQualifier should be empty as customs office starts with FR.", string.Empty, wrapper.CcQualifier);

			wrapper = AdditionalProcedureWrapper.New(procedure, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("CcQualifier should be equal to FR as customs office doesn't start with FR.", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestAdditionalProcedure()
		{
			AssertEquals("AdditionalProcedure should be equal to the value in parameter", "F61", Provider.AdditionalProcedure);
		}
	}
}
