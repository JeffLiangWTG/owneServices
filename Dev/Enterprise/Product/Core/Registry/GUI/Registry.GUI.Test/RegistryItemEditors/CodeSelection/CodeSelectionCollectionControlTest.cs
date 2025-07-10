using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeSelectionCollectionControl))]
	sealed class CodeSelectionCollectionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeSelectionCollectionControl)control).codeSelectionGrid.ReadOnly;
		}
	}
}
