using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMappingControl))]
	public class StaffColumnToGroupDescriptionScimMappingControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new StaffColumnToGroupDescriptionScimMappingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((StaffColumnToGroupDescriptionScimMappingControl)control).ReadOnly;
		}

		public void TestGridReadonly()
		{
			using var form = new Form();
			using var control = GetNewControl() as StaffColumnToGroupDescriptionScimMappingControl;
			form.Controls.Add(control);
			form.Show();

			AssertEquals(false, control.ReadOnly);
			AssertEquals(false, control.Grid.ReadOnly);

			control.ReadOnly = true;
			AssertEquals(true, control.ReadOnly);
			AssertEquals(true, control.Grid.ReadOnly);
		}
	}
}
