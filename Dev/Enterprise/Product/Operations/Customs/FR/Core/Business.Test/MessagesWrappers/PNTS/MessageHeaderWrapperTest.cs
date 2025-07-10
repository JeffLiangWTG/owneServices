using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Registry;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class MessageHeaderWrapperTest : Customs.Business.Testing.DataProviderTestCase<MessageHeaderWrapper>
	{
		public void TestSender()
		{
			AssertEquals("Sender should equal to PNTSConstants.MessageHeaderWrapperConstants.Sender", "45244585100028", Provider.Sender);
		}

		public void TestRecipient()
		{
			AssertEquals("Recipient should equal to PNTSConstants.MessageHeaderWrapperConstants.Recipient", "TSD", Provider.Recipient);
		}

		[TestDate(2023, 01, 18)]
		public void TestMessageTimestamp()
		{
			AssertEquals("MessageTimestamp should equal DateTime.UtcNow", new ZDateTime(2023, 01, 18), Provider.MessageTimestamp);
		}

		public void TestMessageId()
		{
			AssertEquals("MessageId should equal EDIMessage.MessageNumberPlaceHolder", EDIMessage.MessageNumberPlaceHolder, Provider.MessageId);
		}

		public void TestRefToMessageId()
		{
			AssertEquals("RefToMessageId should equal MessageBuilderBase<object>.CorrelationIdPlaceholder", MessageBuilderBase<object>.CorrelationidPlaceholder, Provider.RefToMessageId);
		}

		public void TestCorrelationId()
		{
			AssertEquals("CorrelationId should equal null", null, Provider.CorrelationId);
		}

		public void TestLanguageCode()
		{
			AssertEquals("LanguageCode should equal FR", Core.Constants.CountryCodes.France, Provider.LanguageCode);
		}

		protected override MessageHeaderWrapper GetProvider()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			return new MessageHeaderWrapper();
		}
	}
}
