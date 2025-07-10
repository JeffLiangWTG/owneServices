using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ABCAnalysisCategoryControl))]
	sealed class ABCAnalysisCategoryControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ABCAnalysisCategoryCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ABCAnalysisCategoryControl)control).ABCAnalysisCategoryGrid.ReadOnly;
		}
	}
}
