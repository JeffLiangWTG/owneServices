using System.Windows.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.PBN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	[TestedType(typeof(PBNMessageSendingForm))]
	sealed class PBNMessageSendingFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var messageSendingObjectParent = new PBNMessageSendingObjectParent(header);
			return new PBNMessageSendingForm(messageSendingObjectParent);
		}
	}
}
