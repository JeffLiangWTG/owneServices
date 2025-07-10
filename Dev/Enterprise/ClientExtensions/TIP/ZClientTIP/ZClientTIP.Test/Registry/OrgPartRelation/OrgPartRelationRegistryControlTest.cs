using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP
{
	[TestedType(typeof(OrgPartRelationRegistryControl))]
	class OrgPartRelationRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrgPartRelationRegistryBusinessObjectCollection(Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OrgPartRelationRegistryControl)control).zGrid1.ReadOnly;
		}
	}
}
