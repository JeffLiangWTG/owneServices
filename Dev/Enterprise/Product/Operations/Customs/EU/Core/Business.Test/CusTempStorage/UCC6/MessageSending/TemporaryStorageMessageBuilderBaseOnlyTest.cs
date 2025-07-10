using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessageBuilderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestPreviewMessage()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var messageSendingObject = new TemporaryStorageMessageSendingObject(header);
			messageSendingObject.PreviewMessage += (args) => { args.MessageText = $"Edited {args.MessageText}"; };
			var function = new InvalidationRequestTSDMessageFunction(messageSendingObject);
			var messageBuilder = new TemporaryStorageMessageBuilderForTest(messageSendingObject, function);
			var result = messageBuilder.PopulateMessages();
			AssertEquals("Edited Message Text", result.GetBuilderResults().Single().Message.EM_MessageText);
		}
	}

	class TemporaryStorageMessageBuilderForTest : TemporaryStorageMessageBuilder
	{
		public TemporaryStorageMessageBuilderForTest(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction) : base(messageSendingObject, messageFunction)
		{
		}

		protected override ZString GetApplicationCode() => "TST";

		protected override IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult) => null;

		protected override ZString GetMessageText(ErrorCollector errorCollector) => "Message Text";
	}
}
