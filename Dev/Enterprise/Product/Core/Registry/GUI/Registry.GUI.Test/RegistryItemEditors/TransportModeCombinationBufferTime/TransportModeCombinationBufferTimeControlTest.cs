using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTimeRegistryControl))]
	sealed class TransportModeCombinationBufferTimeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TransportModeCombinationBufferTimeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TransportModeCombinationBufferTimeRegistryControl)control).TransportModeCombinationBufferTimeGrid.ReadOnly;
		}
	}
}
