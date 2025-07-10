using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(HyperlinkAlertBusinessObject))]
	sealed class HyperlinkAlertBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHyperlinkAlertBusinessObject()
		{
			var messages = GetNewBusinessObject() as HyperlinkAlertBusinessObject;
			using (var form = new HyperlinkAlertForm(messages))
			{
				form.Show();

				AssertEquals(messages.MessageLabel, form.HyperlinkFormMessageLabel.Text);

				form.HyperlinkFormViewDetailLinkLabel.OnLinkClicked_Exposed(null);
				AssertEquals(messages.LongMessageText, form.HyperlinkFormDetailMessageTextBox.Text);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var messageLabel = "This record is in use by other record and cannot be deleted.";
			var detailMessage = "This record is in use by other records in the system and cannot be deleted.";

			var messages = new HyperlinkAlertBusinessObject(new ZString(messageLabel), new ZString(detailMessage));
			return messages;
		}
	}
}
