using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(EDIMessageWithDocumentsForm))]
	sealed class EDIMessageWithDocumentsFormTest : Enterprise.Messaging.GUI.Testing.EDIMessageFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = Factory.New<EDIMessageForTesting>();
			bizO.EM_ReceiveTransmit = EDIMessage.Status.Received;
			bizO.ClearHasChanges();
			return new EDIMessageWithDocumentsForm(bizO);
		}

		sealed class EDIMessageForTesting : K84Message
		{
			public EDIMessageForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool ShouldShowInterpretation => true;
		}
	}
}
