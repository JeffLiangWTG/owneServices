using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(RefundEnquiryForm))]
	internal class RefundEnquiryFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (RefundEnquiryForm f = new RefundEnquiryForm(new ClientRefundWrapper(Factory)))
			{
				AssertEquals("Enter Refund Enquiry Details", f.FormHeading);
			}
		}

		public void TestSaveButtonClick()
		{
			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			using (var f = new RefundEnquiryForm(new ClientRefundWrapper(Factory)
			{ RefundOwner = declaration }))
			{
				f.Show();
				f.zButtonSave.PerformClick();
				AssertNotEquals(DialogResult.OK, f.DialogResult);
				Assert("EnquiryDetailsValid should be invalid", !f.RefundWrapper.EnquiryDetailsValid);
				AssertNotNull(f.RefundWrapper.RefundOwner);
				AssertNull(f.RefundWrapper.RefundOwner.Refund);
				f.RefundWrapper.Contact = "Contact";
				f.RefundWrapper.PhoneNumber = "234";
				f.RefundWrapper.EnquiryDetails = "EnquiryDetails";
				f.RefundWrapper.EnquiryRaisedBy = new RaisedByPairList()[0].Code;
				f.zButtonSave.PerformClick();
				AssertEquals(DialogResult.OK, f.DialogResult);
				AssertNotNull("RefundWrapper.RefundOwner.Refund", f.RefundWrapper.RefundOwner.Refund);
				Assert("PostRefund", f.RefundWrapper.RefundOwner.Refund.PostRefund);
			}
		}

		public void TestCancelButtonClick()
		{
			using (RefundEnquiryForm f = new RefundEnquiryForm(new ClientRefundWrapper(Factory)))
			{
				f.Show();
				f.zButtonClose.PerformClick();
				AssertEquals(DialogResult.Cancel, f.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new RefundEnquiryForm(new ClientRefundWrapper(Factory));
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
