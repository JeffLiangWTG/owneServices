using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(OrganisationalUnitPickerForm))]
	class OrganisationalUnitPickerFormTest : ZFormBasherTest
	{
		public void TestOrganisationalUnitPickerForm()
		{
			using (var form = new OrganisationalUnitPickerForm())
			{
				AssertNotNull(form.Controls.Cast<Control>().Single(control => control is OUPickerControl));
				AssertEquals(DialogResult.OK, form.AcceptButton.DialogResult);
				AssertEquals(DialogResult.Cancel, form.CancelButton.DialogResult);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore() => new OrganisationalUnitPickerForm();

		#endregion
	}
}
