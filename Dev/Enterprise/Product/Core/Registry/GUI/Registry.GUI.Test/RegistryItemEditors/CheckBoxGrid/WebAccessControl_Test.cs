using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebAccessControl))]
	sealed class WebAccessControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrgsRoleAccessCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WebAccessControl)control).CheckBoxGrid.ReadOnly;
		}
	}
}
