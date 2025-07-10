using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AQISPermitNumberControlTest : BaseAddInfoControlTest
	{
		public override void TestBindingWithFlattenedHierarchy()
		{
			using (ZChildForm testForm = new ZChildForm(declaration))
			{
				testControl = new AQISPermitNumberControl();
				testControl.SetBindingMember("FilteredInvoiceLines.AddInfo.ZA_AQISPermitIds_Hidden");
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden);
				invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "NPT";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden, testControl.AddInfoTextBox.Text);
			}
		}

		public override void TestBindingWithNormalProperty()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				testControl = new AQISEntityIdControl();
				testControl.SetBindingMember("AddInfo+ZA_AQISPermitIds_Hidden");
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden);
				invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "NPT";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden, testControl.AddInfoTextBox.Text);
			}
		}

		protected override ZString SchemaColumnToBindTo => "ZA_AQISPermitIds_Hidden";

		protected override ZPropertyInfo PropertyInfo => invoiceLine.AddInfo.ZA_AQISPermitIds_HiddenInfo;

		protected override BaseAddInfoControl ControlToTest => new AQISPermitNumberControl();

		const string TestAddInfoString = "RPT,NPT";
	}
}
