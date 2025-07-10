using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	class AddInfoControlTest : BaseAddInfoControlTest
	{
		public void TestSendZeroDutyOverride_HiddenIsSetBeforeAddInfoLine()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				AddInfoControl testControl = new AddInfoControl();
				testControl.SetBindingMember("AddInfo+" + AUAddInfo.Schema.AddInfoLine);
				testForm.Controls.Add(testControl);

				testForm.Show();

				var testAddInfo = new AUAddInfo(invoiceLine);
				testAddInfo.ZA_SendZeroDutyOverride_Hidden = true;
				testControl.AddInfoForm_Closed(null, new AddInfoEventArgs(testAddInfo));
				AssertEquals("Invoice line addInfo should have the flag ticked", true, invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden);
				AssertEquals("Current AddInfo of Invoice line should have DTY=0 as user ticked SendZeroDutyOverride", true, invoiceLine.AddInfo.AddInfoLine.Contains("DTY="));
			}
		}

		public override void TestBindingWithFlattenedHierarchy()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				testControl = new AddInfoControl();
				testControl.SetBindingMember("AddInfo+" + AUAddInfo.Schema.AddInfoLine);
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.AddInfoLine);
				invoiceLine.AddInfo.AddInfoLine = "AMB=P";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.AddInfoLine, testControl.AddInfoTextBox.Text);
				invoiceLine.AddInfo.AddInfoLine = "AMB=XXX"; //invalid
				AssertEquals("The control should have notification", true, testControl.Extensions.Get<INotificationExtension>().Notifications.ToUniqueMessageListString().Length > 0);
				testControl.AddInfoTextBox.Text = "AMB=XXX";
				ChangeFocusToInvokeBinding();
				AssertEquals("The field should have notification", true, invoiceLine.AddInfo.ZA_AMBInfo.HasNotifications());
				AssertEquals("Add Info Biz Obj", invoiceLine.AddInfo, testControl.CurrentAddInfo);
			}
		}

		public override void TestBindingWithNormalProperty()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				testControl = new AddInfoControl();
				testControl.BindTo = "AddInfo+" + AUAddInfo.Schema.AddInfoLine;
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				testControl.AddInfoTextBox.Text = TestAddInfoString;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the value", TestAddInfoString, invoiceLine.AddInfo.AddInfoLine);
				invoiceLine.AddInfo.AddInfoLine = "AMB=P";
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", invoiceLine.AddInfo.AddInfoLine, testControl.AddInfoTextBox.Text);
				invoiceLine.AddInfo.AddInfoLine = "AMB=XXX"; //invalid
				AssertEquals("The control should have notification", true, testControl.Extensions.Get<INotificationExtension>().Notifications.ToUniqueMessageListString().Length > 0);
				testControl.AddInfoTextBox.Text = "AMB=XXX";
				ChangeFocusToInvokeBinding();
				AssertEquals("The field should have notification", true, invoiceLine.AddInfo.ZA_AMBInfo.HasNotifications());
				AssertEquals("Add Info Biz Obj", invoiceLine.AddInfo, testControl.CurrentAddInfo);
			}
		}

		protected override ZString SchemaColumnToBindTo => AUAddInfo.Schema.AddInfoLine;

		protected override ZPropertyInfo PropertyInfo => invoiceLine.AddInfo.AddInfoLineInfo;

		protected override BaseAddInfoControl ControlToTest => new AddInfoControl();

		const string TestAddInfoString = "AMB=C";
	}
}
