using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AQISProducerCodeControlTest : BaseAddInfoControlTest
	{
		public override void TestBindingWithFlattenedHierarchy()
		{
			using (ZChildForm testForm = new ZChildForm(declaration))
			{
				testControl = new AQISProducerCodeControl();
				testControl.SetBindingMember("FilteredInvoiceLines.AddInfo.ZA_AQISProducerCodes_Hidden");
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden);
				invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "NPT";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden, testControl.AddInfoTextBox.Text);
			}
		}

		public override void TestBindingWithNormalProperty()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				testControl = new AQISProducerCodeControl();
				testControl.SetBindingMember("AddInfo+ZA_AQISProducerCodes_Hidden");
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden);
				invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "NPT";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden, testControl.AddInfoTextBox.Text);
			}
		}

		protected override ZString SchemaColumnToBindTo => "ZA_AQISProducerCodes_Hidden";

		protected override ZPropertyInfo PropertyInfo => invoiceLine.AddInfo.ZA_AQISProducerCodes_HiddenInfo;

		protected override BaseAddInfoControl ControlToTest => new AQISProducerCodeControl();

		const string TestAddInfoString = "RPT,NPT";
	}
}
