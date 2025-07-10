using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyModuleTextFilterHasNoComparisonOperator : ModuleTextFilter
	{
		public DummyModuleTextFilterHasNoComparisonOperator()
			: base("DummyModuleTextFilterHasNoComparisonOperator", DummyBizoSchema.Z0_Description)
		{
		}

		public DummyModuleTextFilterHasNoComparisonOperator(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
