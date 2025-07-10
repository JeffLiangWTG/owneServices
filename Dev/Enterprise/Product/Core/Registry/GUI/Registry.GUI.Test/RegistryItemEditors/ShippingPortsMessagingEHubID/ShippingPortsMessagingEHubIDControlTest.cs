using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ShippingPortsMessagingEHubIDControl))]
	public class ShippingPortsMessagingEHubIDControlTest : RegistryZUserControlTestCase
	{
		public void TestShippingPortsMessagingEHubIDGrid()
		{
			using (var form = new ZForm(GetNewBusinessEntity()))
			using (var control = new ShippingPortsMessagingEHubIDControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Port", control.ShippingPortsMessagingEHubIDGrid.GetColumnCaption("Port"));
				AssertEquals("Module", control.ShippingPortsMessagingEHubIDGrid.GetColumnCaption("Module"));
				AssertEquals("Recipient ID", control.ShippingPortsMessagingEHubIDGrid.GetColumnCaption("RecipientID"));
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return ShippingPortsMessagingEHubIDCollection.NewWithDefaultValues();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ShippingPortsMessagingEHubIDControl)control).ShippingPortsMessagingEHubIDGrid.ReadOnly;
		}
	}
}
