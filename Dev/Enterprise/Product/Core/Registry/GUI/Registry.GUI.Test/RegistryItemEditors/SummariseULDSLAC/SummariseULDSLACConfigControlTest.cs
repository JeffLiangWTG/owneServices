using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SummarizeULDSLACConfigControl))]
	sealed class SummariseULDSLACConfigControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SummarizeULDSLACConfigCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SummarizeULDSLACConfigControl)control).Grid.ReadOnly;
		}
	}
}
