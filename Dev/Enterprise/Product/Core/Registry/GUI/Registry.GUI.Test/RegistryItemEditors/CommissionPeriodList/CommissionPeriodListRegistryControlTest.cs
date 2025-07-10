using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CommissionPeriodListRegistryControl))]
	sealed class CommissionPeriodListRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestControlReadOnly()
		{
			using (var dummyForm = new Form())
			using (var control = new CommissionPeriodListRegistryControl())
			{
				dummyForm.Controls.Add(control);

				AssertEquals(false, control.Grid.ReadOnly);
				control.ReadOnly = true;
				AssertEquals(true, control.Grid.ReadOnly);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CommissionPeriodCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((CommissionPeriodListRegistryControl)control).Grid.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CommissionPeriodListRegistryControl();
		}
	}
}
