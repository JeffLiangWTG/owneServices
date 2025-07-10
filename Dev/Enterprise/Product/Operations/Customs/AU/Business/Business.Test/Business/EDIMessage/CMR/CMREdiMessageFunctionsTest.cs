using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMREdiMessageFunctionsTest : TestCaseWithFactory
	{
		public void TestDoMessagesContainAnyCARSTs()
		{
			CusContainer container = Factory.New<CusContainer>();
			Assert("No Carsts or messages yet", !messageFunctions.DoMessagesContainAnyCARSTs(container.Messages));
			EDIMessage message = container.Messages.AddNew();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			Assert("CARST should be found", messageFunctions.DoMessagesContainAnyCARSTs(container.Messages));
			message.EM_MessageType = "";
			Assert("No Carsts", !messageFunctions.DoMessagesContainAnyCARSTs(container.Messages));
		}

		CMREdiMessageFunctions messageFunctions;

		protected override void SetUp()
		{
			base.SetUp();
			messageFunctions = new CMREdiMessageFunctions();
		}
	}
}
