using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(AuthorizationModeAndSettingsControl))]
	public class AuthorizationModeAndSettingsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AuthorizationModeAndSettings();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AuthorizationModeAndSettingsControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new AuthorizationModeAndSettingsControl();
		}
	}
}
