using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(EDIInterchangeModificationForm))]
	sealed class EDIInterchangeModificationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			interchange.ClearHasChanges();
			return new EDIInterchangeModificationForm(interchange);
		}
	}
}
