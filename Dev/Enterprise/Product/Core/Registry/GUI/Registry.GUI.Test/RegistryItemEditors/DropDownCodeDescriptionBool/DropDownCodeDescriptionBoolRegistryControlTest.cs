using System.Windows.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBoolRegistryControl))]
	sealed class DropDownCodeDescriptionBoolRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestReadOnlySetsReadOnlyOnGrid()
		{
			using (var form = new TestForm())
			using (var control = new DropDownCodeDescriptionBoolRegistryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.ReadOnly = false;
				AssertEquals(false, control.ReadOnly);
				AssertEquals(false, control.DropDownCodeDescriptionBoolGridExposed.ReadOnly);
				control.ReadOnly = true;
				AssertEquals(true, control.ReadOnly);
				AssertEquals(true, control.DropDownCodeDescriptionBoolGridExposed.ReadOnly);
				control.ReadOnly = false;
				AssertEquals(false, control.ReadOnly);
				AssertEquals(false, control.DropDownCodeDescriptionBoolGridExposed.ReadOnly);
			}
		}

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((DropDownCodeDescriptionBoolRegistryControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new DropDownCodeDescriptionBoolRegistryControl();
		}
	}
}
