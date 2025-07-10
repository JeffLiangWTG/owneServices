using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsMatchingConfigurationControl))]
	sealed class DpsMatchingConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new DpsMatchingConfigurationBusinessObject();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((DpsMatchingConfigurationControl)control).ReadOnly;

		public void TestSetControlOrBusinessEntityReadOnly()
		{
			using (var form = new ZForm())
			using (var dpsMatchingConfigurations = new DpsMatchingConfigurationControlForTest())
			{
				form.Controls.Add(dpsMatchingConfigurations);
				form.Show();

				dpsMatchingConfigurations.SetControlOrBusinessEntityReadOnly(true);

				AssertEquals(false, dpsMatchingConfigurations.StrictRadioButton.ReadOnly);
				AssertEquals(false, dpsMatchingConfigurations.BalancedRadioButton.ReadOnly);
				AssertEquals(false, dpsMatchingConfigurations.ComprehensiveRadioButton.ReadOnly);
			}
		}
	}
}
