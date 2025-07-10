using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	class IApportionmentMethodOverrideHelperTest_ConsolCostDefaultApportionmentMethod : IApportionmentMethodOverrideHelperTest<ConsolCostDefaultApportionmentMethod>
	{
		protected override ConsolCostDefaultApportionmentMethod CreateBusinessObject(DummyConsolCostApportionmentMethodSetter setter)
		{
			return new ConsolCostDefaultApportionmentMethod
			{
				Module = setter.Module,
				ConsolType = setter.ConsolType,
				Apportionment = setter.ApportionmentMethod,
				ContainerMode = setter.ContainerMode,
				TransportMode = setter.TransportMode,
				Direction = setter.Direction,
			};
		}
	}
}
