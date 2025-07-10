using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(RefundProcessingForm))]
	internal class RefundProcessingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				AssertEquals("Refund Processing", form.FormHeading);
			}
		}

		public void TestOkButtonClick()
		{
			ClientRefund refund = Factory.New<ClientRefund>();
			using (RefundProcessingForm form = new RefundProcessingForm(refund))
			{
				form.Show();
				Application.DoEvents();
				form.OKButton.PerformClick();
				Assert("ProcessRefund", !refund.ProcessRefund);
				Assert("HasErrors", refund.HasErrors);
				AssertNotEquals(DialogResult.OK, form.DialogResult);
			}

			ClientRefund.T10_ControlNumber = "123123";
			ClientRefund.T10_DateCreated = ZDateTime.Now;
			ClientRefund.T10_GS_NKAtFaultUser = ClientRefund.AtFaultList[ClientRefund.AtFaultList.Count - 1].Code;
			ClientRefund.T10_GS_NKCreatedUser = GlbStaff.CurrentUser.GS_Code;
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				form.Show();
				Application.DoEvents();
				form.OKButton.PerformClick();
				Assert("ProcessRefund", ClientRefund.ProcessRefund);
				Assert("HasErrors", !ClientRefund.HasErrors);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButtonClick()
		{
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				form.Show();
				Application.DoEvents();
				form.cancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestApprovedRejectedRadioButtons()
		{
			ClientRefund.T10_IsRefundRejected = true;
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.RefundApprovedRadioButton.Checked);
				Assert(form.RefundRejectedRadioButton.Checked);
			}

			ClientRefund.T10_IsRefundRejected = false;
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.RefundApprovedRadioButton.Checked);
				Assert(!form.RefundRejectedRadioButton.Checked);
			}
		}

		public void TestStoreRestoreRefund()
		{
			ClientRefund expected = Factory.New<ClientRefund>();
			using (RefundProcessingForm form = new RefundProcessingForm(ClientRefund))
			{
				form.Show();
				Application.DoEvents();
				CloneRefund(ClientRefund, expected);
				ClientRefund.T10_IsRefundRejected = true;
				ClientRefund.T10_RefundAmount = 123456.321m;
				form.cancelButton.PerformClick();
				AssertRefund(expected, ClientRefund);
			}
		}

		void CloneRefund(ClientRefund from, ClientRefund to)
		{
			to.T10_IsRefundRejected = from.T10_IsRefundRejected;
			to.T10_IsRefundApproved = from.T10_IsRefundApproved;
			to.T10_RefundRejectedDetails = from.T10_RefundRejectedDetails;
			to.T10_GS_NKAtFaultUser = from.T10_GS_NKAtFaultUser;
			to.T10_RefundAmount = from.T10_RefundAmount;
			to.T10_RefundProcessingFee = from.T10_RefundProcessingFee;
			to.T10_RefundReason = from.T10_RefundReason;
			to.T10_WriteOffAmount = from.T10_WriteOffAmount;
			to.T10_AdditionalCharges = from.T10_AdditionalCharges;
			to.Remarks = from.Remarks;
		}

		void AssertRefund(ClientRefund expected, ClientRefund original)
		{
			AssertEquals("T10_IsRefundRejected", expected.T10_IsRefundRejected, original.T10_IsRefundRejected);
			AssertEquals("T10_IsRefundApproved", expected.T10_IsRefundApproved, original.T10_IsRefundApproved);
			AssertEquals("T10_RefundRejectedDetails", expected.T10_RefundRejectedDetails, original.T10_RefundRejectedDetails);
			AssertEquals("T10_GS_NKAtFaultUser", expected.T10_GS_NKAtFaultUser, original.T10_GS_NKAtFaultUser);
			AssertEquals("T10_RefundAmount", expected.T10_RefundAmount, original.T10_RefundAmount);
			AssertEquals("T10_RefundProcessingFee", expected.T10_RefundProcessingFee, original.T10_RefundProcessingFee);
			AssertEquals("T10_RefundReason", expected.T10_RefundReason, original.T10_RefundReason);
			AssertEquals("T10_WriteOffAmount", expected.T10_WriteOffAmount, original.T10_WriteOffAmount);
			AssertEquals("T10_AdditionalCharges", expected.T10_AdditionalCharges, original.T10_AdditionalCharges);
			AssertEquals("Remarks", expected.Remarks, original.Remarks);
		}

		protected override Form GetFormToBashCore()
		{
			ClientRefund refund = Factory.NewWithValidTestData<ClientRefund>();
			refund.T10_RefundRejectedDetails = "T10_RefundRejectedDetails";
			Factory.Save();
			return new RefundProcessingForm(refund);
		}

		ClientRefund ClientRefund
		{
			get
			{
				return refund ?? (refund = ClientRefundTest.TestHelper.PopulatedRefund(Factory));
			}
		}

		ClientRefund refund;
	}
}
