using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingForm))]
	sealed class GuaranteeAccessCodesSendingFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormHeading()
		{
			using (var form = GetForm() as ZForm)
			{
				AssertEquals("FormHeading", "Guarantee Access Codes", form.FormHeading);
				form.Show();
				AssertEquals("Text", "Guarantee Access Codes", form.Text);
			}
		}

		public void TestGuaranteeReferenceNumberTextBox()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				var field = form.FindSingle<ZTextBox>("GuaranteeReferenceNumberTextBox");
				CombineAssertions(() =>
				{
					AssertNotNull(field);
					AssertEquals("GRN.Caption", "GRN", field.CaptionResourceString.Caption);
				});
			}
		}

		[RequiresSTA]
		public void TestOfficeOfGuaranteeCodeFindBox()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				var field = form.FindSingle<ZCodeFindBox>("OfficeOfGuaranteeCodeFindBox");
				CombineAssertions(() =>
				{
					AssertNotNull(field);
					AssertEquals("OfficeOfGuarantee.Caption", "Office of Guarantee", field.CaptionResourceString.Caption);
				});
			}
		}

		public void TestCurrentCodeTextBox()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				var field = form.FindSingle<ZTextBox>("CurrentCodeTextBox");
				CombineAssertions(() =>
				{
					AssertNotNull(field);
					AssertEquals("CurrentCode.Caption", "Current Code", field.CaptionResourceString.Caption);
				});
			}
		}

		public void TestNewAccessCodeTextBox()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				var field = form.FindSingle<ZTextBox>("NewAccessCodeTextBox");
				CombineAssertions(() =>
				{
					AssertNotNull(field);
					AssertEquals("NewAccessCode.Caption", "New Access Code", field.CaptionResourceString.Caption);
				});
			}
		}

		[RequiresSTA]
		public void TestMasterCodeTextBox()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				var field = form.FindSingle<ZTextBox>("MasterCodeTextBox");
				CombineAssertions(() =>
				{
					AssertNotNull(field);
					AssertEquals("MasterCode.Caption", "Master Code", field.CaptionResourceString.Caption);
				});
			}
		}

		public void TestButtonsCaptions()
		{
			using (var form = new GuaranteeAccessCodesSendingForm(sendingObjectParent))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
					AssertNotNull(sendButton);
					AssertEquals("SendButton.Caption", "Confirm and Send", sendButton.CaptionResourceString.Caption);

					var cancelButton2 = form.FindSingleOrDefault<ZButton>(c => c.Name == "CancelButton2");
					AssertNotNull(cancelButton2);
					AssertEquals("CancelButton2.Caption", "Cancel", cancelButton2.CaptionResourceString.Caption);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return GetForm();
		}

		GuaranteeAccessCodesSendingForm GetForm()
		{
			return new GuaranteeAccessCodesSendingForm(sendingObjectParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendingObjectParent = new GuaranteeAccessCodesSendingObjectParent(cusGuaranteeHeader);
		}

		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingObjectParent sendingObjectParent;
	}
}
