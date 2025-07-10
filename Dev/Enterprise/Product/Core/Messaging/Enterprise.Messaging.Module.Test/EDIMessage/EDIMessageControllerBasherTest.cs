using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIMessageController))]
	sealed class EDIMessageControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Messaging.EDIMessage;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = EDIMessageTestFactory.New(Factory);
			bizO.EM_ReceiveTransmit = EDIMessage.Status.Received;
			Factory.Save();
			return bizO;
		}
	}
}
