using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Messaging
{
	[TestedType(typeof(EHubMessageDecodeForm))]
	public class EhubMessageDecodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			return new EHubMessageDecodeForm(message);
		}
	}
}
