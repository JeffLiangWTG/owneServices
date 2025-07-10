using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SageAccountCodeMappingRegistryItemControl))]
	public class SageAccountCodeMappingRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			SageAccountCodeMappingRegistryItemControl control = (SageAccountCodeMappingRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new SageAccountCodeMappingRegistryBusinessObjectCollection();
		}
	}
}
