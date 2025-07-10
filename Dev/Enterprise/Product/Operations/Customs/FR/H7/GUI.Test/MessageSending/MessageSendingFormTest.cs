using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestMessageSendingGridColumnLayoutProvider()
		{
			var header = Factory.New<H7ManifestHeader>();
			var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);

			using (var form = new MessageSendingForm(testingParent))
			{
				var messageSendingGridColumnLayoutProviderPropertyInfo = form.GetType().GetProperty("MessageSendingGridColumnLayoutProvider", BindingFlags.Instance | BindingFlags.NonPublic);
				var messageSendingGridColumnLayoutProvider = messageSendingGridColumnLayoutProviderPropertyInfo.GetValue(form, null);

				AssertType<MessageSendingGridColumnLayout>(messageSendingGridColumnLayoutProvider);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<H7ManifestHeader>();
			var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);
			return new MessageSendingForm(testingParent);
		}
	}
}
