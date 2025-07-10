using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(AutomatedModificationControl))]
	class AutomatedModificationControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AutomatedModificationControl)control).ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutomatedModification();
		}
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}

		public void TestField()
		{
			using (var control = new AutomatedModificationControl())
			{
				control.Show();
				var userControl = (ZArchitecture.GUI.ZCheckBox)control.Controls.Find("zEnableAutomatedModificationCheckBox", true).First();
				AssertEquals("Enable automated Modification", userControl.CaptionResourceString.Caption);
			}
		}
	}
}
