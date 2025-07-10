using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(TriggerPointsConfigurationControl))]
	public class TriggerPointsConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TriggerPointsConfigurationControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new TriggerPointsConfiguration();
		}
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}

		public void TestField()
		{
			using (var control = new TriggerPointsConfigurationControl())
			{
				control.Show();
				var userControl = (ZArchitecture.GUI.ZCheckBox)control.Controls.Find("zEnableAutomatedValidationCheckBox", true).First();
				AssertEquals("Enable automated validation", userControl.CaptionResourceString.Caption);
			}
		}
	}
}
