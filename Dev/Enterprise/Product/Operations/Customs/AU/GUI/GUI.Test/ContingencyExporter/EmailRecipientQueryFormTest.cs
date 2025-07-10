using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(EmailRecipientQueryForm))]
	sealed class EmailRecipientQueryFormTest : ZFormBasherTest
	{
		public void TestSendButton()
		{
			EmailRecipientSelection emailSelector = new EmailRecipientSelection(Factory);
			using (EmailRecipientQueryForm form = new EmailRecipientQueryForm(emailSelector))
			{
				form.Show();
				form.SendButton.PerformClick();
				AssertEquals(true, form.BusinessEntity.HasErrors());
				AssertEquals(DialogResult.None, form.DialogResult);
				emailSelector.RecipientName = "JOHN";
				emailSelector.RecipientEmail = "zubin@example.com";
				form.SendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}

			EmailRecipientSelection newEmailSelector = new EmailRecipientSelection(Factory);
			AssertEquals("List in registry updated through Send Button", "zubin@example.com", newEmailSelector.Recipients.GetDescriptionFromCode("JOHN"));
		}

		protected override Form GetFormToBashCore() => new EmailRecipientQueryForm(new EmailRecipientSelection(Factory));
	}
}
