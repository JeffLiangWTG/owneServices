using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryItemControl))]
	public class TransportAndChargeCodeMappingRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			TransportAndChargeCodeMappingRegistryItemControl control = (TransportAndChargeCodeMappingRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObjectCollection();
		}
	}
}
