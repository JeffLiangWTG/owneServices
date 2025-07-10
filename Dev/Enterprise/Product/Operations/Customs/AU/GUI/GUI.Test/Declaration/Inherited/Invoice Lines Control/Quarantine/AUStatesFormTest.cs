using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AUStatesForm))]
	sealed class AUStatesFormTest : ZFormBasherTest
	{
		public void TestLoadedInFormConstructor()
		{
			using (AUStatesForm form = (AUStatesForm)GetFormToBashCore())
			{
				AssertEquals("Should be reloaded in the constructor", 1, invoiceLine.AUStateCodeCollection.Count);
			}
		}

		public void TestOKButtonClicked()
		{
			using (AUStatesForm form = (AUStatesForm)GetFormToBashCore())
			{
				AssertEquals("With one AUState Code", "ACT", invoiceLine.JI_AUState);
				form.Closed += Form_Closed;
				form.Show();
				AUStateCode added = invoiceLine.AUStateCodeCollection.AddNew();
				added.Code = "ACT";
				form.OKBtn.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("There are errors that need to be fixed", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				added.Code = "FO";
				form.OKBtn.PerformClick();
				Assert("Should be closed now", formClosed);
				AssertEquals("Multiply AUState Codes", "MULT", invoiceLine.JI_AUState);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AUState_Hidden = "ACT";
			return new AUStatesForm(invoiceLine);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		void Form_Closed(object sender, EventArgs e)
		{
			formClosed = true;
		}
		JobComInvoiceLine invoiceLine;
		bool formClosed;
	}
}
