using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsWebServiceItemControl))]
	sealed class DpsWebServiceItemControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new DpsWebServiceItemCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((DpsWebServiceItemControl)control).ReadOnly;

		[RequiresSTA]
		public void TestSetControlOrBusinessEntityReadOnly()
		{
			using (var form = new ZForm())
			using (var dpsWebServiceItemControl = new DpsWebServiceItemControlForTest())
			{
				form.Controls.Add(dpsWebServiceItemControl);
				form.Show();
				AssertEquals("Precondition: ", true, dpsWebServiceItemControl.DpsWebServiceItemGrid.ReadOnly);

				dpsWebServiceItemControl.SetControlOrBusinessEntityReadOnly(false);
				AssertEquals(false, dpsWebServiceItemControl.DpsWebServiceItemGrid.ReadOnly);
			}
		}
	}
}
