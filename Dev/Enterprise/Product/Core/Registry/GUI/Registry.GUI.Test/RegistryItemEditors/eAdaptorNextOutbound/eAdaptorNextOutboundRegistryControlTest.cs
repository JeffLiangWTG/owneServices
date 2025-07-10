using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(eAdaptorNextOutboundRegistryControl))]
	sealed class eAdaptorNextOutboundRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return eAdaptorNextOutboundConfig.DefaultValue;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((eAdaptorNextOutboundRegistryControl)control).ReadOnly;
		}
	}
}
