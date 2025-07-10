namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class CusContainerValidationTest<TJobDeclaration> : Customs.Business.Testing.CusContainerValidationTest<TJobDeclaration>
		where TJobDeclaration : JobDeclaration
	{
		public void TestParent()
		{
			CusContainer parent = Factory.New<CusContainer>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public override void TestAirContainerIsMessageError()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = "AIR";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Assert(!container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.CannotHaveContainersOnAirJob));
		}

		public void TestContainerWeightUQError()
		{
			var declaration = GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			container.CO_WeightUQ = "ZZ";
			AssertHasMessageErrorContaining(container.CO_WeightUQInfo, "not in the list");  // this can be just a substring of the expected error message
			AssertNoMessageErrorContaining(container.CO_WeightUQInfo, "have not entered");
			container.CO_WeightUQ = "";
			AssertNoMessageErrorContaining(container.CO_WeightUQInfo, "not in the list");
			AssertHasMessageErrorContaining(container.CO_WeightUQInfo, "have not entered");
			container.CO_WeightUQ = container.Lookups.WeightUnits[0].Code;
			AssertNoMessageErrorContaining(container.CO_WeightUQInfo, "not in the list");
			AssertNoMessageErrorContaining(container.CO_WeightUQInfo, "have not entered");
		}
	}
}
