using CargoWise.Customs.FR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.NCTS.Messaging;

namespace Enterprise.Customs.FR.NCTS.Testing.Messaging.MessageBuilders
{
	sealed class TP5MessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilder()
		{
			CombineAssertions(() =>
			{
				AssertMessageBuilderType<TP5007MessageBuilder>(TP5MessageTypeList.Codes.CC007C);
				AssertMessageBuilderType<TP5013MessageBuilder>(TP5MessageTypeList.Codes.CC013C);
				AssertMessageBuilderType<TP5014MessageBuilder>(TP5MessageTypeList.Codes.CC014C);
				AssertMessageBuilderType<TP5015MessageBuilder>(TP5MessageTypeList.Codes.CC015C);
				AssertMessageBuilderType<TP5034MessageBuilder>(TP5MessageTypeList.Codes.CC034C);
				AssertMessageBuilderType<TP5044MessageBuilder>(TP5MessageTypeList.Codes.CC044C);
				AssertMessageBuilderType<TP5141MessageBuilder>(TP5MessageTypeList.Codes.CC141C);
				AssertMessageBuilderType<TP5170MessageBuilder>(TP5MessageTypeList.Codes.CC170C);
			});
		}

		void AssertMessageBuilderType<T>(ZString messageType)
		{
			var nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			var tp5MessageBuilderManager = new TP5MessageBuilderManager();
			var objectToSend = new TP5MessageSendingObject(nctsHeader);
			objectToSend.MessageType = messageType;
			var builder = tp5MessageBuilderManager.NewMessageBuilder(objectToSend);
			AssertType<T>(builder);
		}

		public void TestBuilderType()
		{
			var tp5MessageBuilderManager = new TP5MessageBuilderManager();
			AssertEquals(FR.Business.MessageTypeList.Codes.TP5, tp5MessageBuilderManager.BuilderType);
		}
	}
}
