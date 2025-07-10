using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(RequestDateForm))]
	class RequestDateFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new RequestDateForm(formTitle, Res.GetString("test3", "Test Text"), ZDateTime.BrettsBirthday, NoResourceStringData.GetData("Date"));

		#endregion

		public void TestInitializeDefaultDate()
		{
			var expectedMessageLabelText = "Message Label Text";
			using (var requestDateForm = new RequestDateForm(formTitle, expectedMessageLabelText, ZDateTime.BrettsBirthday, dateCaption))
			{
				var messageLabel = (ZLabel)requestDateForm.Controls.Find("MessageLabel", true).Single();
				var dateEdit = (ZDateEdit)(requestDateForm.Controls.Find("DateEdit", true).Single());

				requestDateForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the text we pass on the constructor", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("DateEdit has the default date we pass on the constructor", ZDateTime.BrettsBirthday, dateEdit.DateTimeValue);
					AssertEquals("Title window has the text we pass on the constructor", formTitle, requestDateForm.FormCaption);
					AssertEquals("DateEdit CaptionResourceString has the text we pass on the constructor", dateCaption, dateEdit.CaptionResourceString);
				});
			}
		}

		public void TestValidateDateEdit()
		{
			using (var requestDateForm = new RequestDateForm(formTitle, "Message Label Text", ZDateTime.BrettsBirthday, dateCaption))
			{
				var dateEdit = (ZDateEdit)(requestDateForm.Controls.Find("DateEdit", true).Single());
				var okButton = (ZButton)(requestDateForm.Controls.Find("OKButton", true).Single());

				requestDateForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Date is Valid so OkButton is Enable", true, okButton.Enabled);

					dateEdit.DateTimeValue = ZDateTime.Invalid;
					AssertEquals("Date is not Valid so DateEdit has the Red ForeColor", Color.Red, dateEdit.DateTextBox.ForeColor);
					AssertEquals("Date is not Valid so OkButton is Enable", false, okButton.Enabled);
				});
			}
		}

		readonly ResourceString formTitle = ResString.GetMultilingualString("Test1", "Request Date Form");
		readonly ResourceStringData dateCaption = NoResourceStringData.GetData("Date");
	}
}
