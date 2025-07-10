using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(EFTPaymentForm))]
	sealed class EFTPaymentFormTest : ZFormBasherTest
	{
		public void TestScheduledPaymentDate()
		{
			using (var form = new EFTPaymentForm(PayInfoCollection))
			{
				form.Show();
				var control = form.scheduledPaymentDate;
				AssertType<ZDateEdit>("Type", control);
				AssertEquals("BindTo", nameof(EFTPaymentInformation.ScheduledPaymentDate), control.BindTo);
				AssertEquals("Not visible by default", false, control.Visible);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var form = new EFTPaymentForm(PayInfoCollection))
				{
					form.Show();
					AssertEquals("Visible when EnableQueuedEntryPayments is True", true, form.scheduledPaymentDate.Visible);
				}
			}
		}

		public void TestScheduledPaymentDateReadonly_PaymentScheduled()
		{
			TestDec.JE_MessageStatus = CustomsEntryStatus.ScheduledPayment.Code;
			PayInfoCollection[0].CustomsChargeAmountPayableNow = 100m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var form = new EFTPaymentForm(PayInfoCollection))
			{
				form.Show();
				var control = form.scheduledPaymentDate;
				AssertEquals("Scheduled Payment Date control is read only", true, control.ReadOnly);
			}
		}

		public void TestOKButtonDisabled_PaymentScheduled()
		{
			TestDec.JE_MessageStatus = CustomsEntryStatus.ScheduledPayment.Code;
			PayInfoCollection[0].CustomsChargeAmountPayableNow = 100m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var form = new EFTPaymentForm(PayInfoCollection))
			{
				form.Show();
				var control = form.oKBoundButton;
				AssertEquals("Send button control is disabled", false, control.Enabled);
			}
		}

		public void TestOKButtonWithSomethingToPay()
		{
			PayInfoCollection[0].CustomsChargeAmountPayableNow = 100m;
			AssertEquals("HasSomething to pay", true, PayInfoCollection.HasAmountsToPay);
			using (EFTPaymentForm form1 = new EFTPaymentForm(PayInfoCollection))
			{
				form1.OKBoundButton_Click(form1.oKBoundButton, EventArgs.Empty);
				AssertEquals("Is OK to send", false, form1.IsOKToSend);
				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Last message", "Customs advised total amount due is 0, which is different to what you indicated here.", lastMessage);
			}

			TestDec.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader = TestDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "12345678";
			var message = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message.EM_MessageText = Business.Testing.TestMessages.IMDRMessageText;
			message.EM_ReceiveTransmit = "RCV";
			payInfoCollection = new EFTPaymentInformationCollection(TestDec);
			PayInfoCollection[0].CustomsChargeAmountPayableNow = 100m;
			using (var form = new EFTPaymentForm(PayInfoCollection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.OKBoundButton_Click(form.oKBoundButton, EventArgs.Empty);
				AssertEquals("Is OK to send", true, form.IsOKToSend);
			}
		}

		public void TestOKButtonWithNothingToPay()
		{
			AssertEquals("HasSomething to pay", false, PayInfoCollection.HasAmountsToPay);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (EFTPaymentForm form = new EFTPaymentForm(PayInfoCollection))
			{
				form.OKBoundButton_Click(form.oKBoundButton, EventArgs.Empty);
				AssertEquals("Is OK to send", false, form.IsOKToSend);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Last message", "There are no amounts to pay and no payment message will be sent.", lastMessage);
			}
		}

		public void TestCancelButton()
		{
			using (EFTPaymentForm form = new EFTPaymentForm(PayInfoCollection))
			{
				form.OKBoundButton_Click(form.oKBoundButton, EventArgs.Empty);
				AssertEquals("Is OK to send", false, form.IsOKToSend);
			}
		}

		protected override Form GetFormToBashCore() => new EFTPaymentForm(PayInfoCollection);

		EFTPaymentInformationCollection payInfoCollection;
		EFTPaymentInformationCollection PayInfoCollection => payInfoCollection ?? (payInfoCollection = new EFTPaymentInformationCollection(TestDec));

		JobDeclaration testDec;
		JobDeclaration TestDec
		{
			get
			{
				if (testDec == null)
				{
					testDec = JobDeclaration.New(Factory);
					testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
					var entryHeader = testDec.CustomsEntryHeaders.AddNew();
					entryHeader.EntryNumber = "111AAA222";
				}

				return testDec;
			}
		}
	}
}
