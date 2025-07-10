using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Test
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrlUserControl))]
	public class MyAccountHostingSiteLandingPageUrlUserControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (MyAccountHostingSiteLandingPageUrlUserControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new MyAccountHostingSiteLandingPageUrlCollection();
		}
	}
}
