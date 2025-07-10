using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.GUI.Testing
{
	[TestedType(typeof(ShippingAgentControl))]
	class ShippingAgentControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new ShippingAgentObject();
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var zAddressControl = (ZAddressControl)((ShippingAgentControl)control).Controls.Find("ShippingAgentZAddressControl", true).FirstOrDefault();
			return zAddressControl.ReadOnly && !zAddressControl.Enabled;
		}
	}
}
