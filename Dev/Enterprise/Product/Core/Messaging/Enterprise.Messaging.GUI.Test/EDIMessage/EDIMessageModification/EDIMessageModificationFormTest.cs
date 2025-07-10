using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(EDIMessageModificationForm))]
	sealed class EDIMessageModificationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.ClearHasChanges();
			return new EDIMessageModificationForm(message);
		}
	}
}
