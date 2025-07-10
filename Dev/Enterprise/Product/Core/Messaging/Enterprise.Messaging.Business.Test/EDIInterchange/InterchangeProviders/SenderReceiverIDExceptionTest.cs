using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.InterchangeProviders.Testing
{
	public abstract class SenderReceiverIDExceptionTest : TestCaseWithFactory
	{
		public void TestThrowApplicationWhenDestinationIsEmptyForOutboundInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var interchange = Factory.New<EDIInterchange>();

			ApplicationException exception = null;
			var provider = GetInterchangeProvider(messages);
			try
			{
				provider.SetInterchangeValuesForTransmit(interchange, messages, "", "", "ABC");
			}
			catch (ApplicationException e)
			{
				exception = e;
			}

			AssertNotNull("The destination mail box for outbound interchanges should be known when coding", exception);
		}

		public void TestThrowMessageProcessingExceptionWhenSenderIDIsNotKnown()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var interchange = Factory.New<EDIInterchange>();

			MessageProcessingException exception = null;
			var provider = GetInterchangeProvider(messages);
			try
			{
				provider.SetInterchangeValuesForTransmit(interchange, messages, "", "ABC", "");
			}
			catch (MessageProcessingException e)
			{
				exception = e;
			}

			AssertNotNull("a message processing exception should have been thrown", exception);
			ZString exceptionMessage = exception.Message;
			Assert(exceptionMessage.Contains("There is no Customs interchange sender id set up,"));
			Assert(exceptionMessage.Contains("for Company - EDI, Branch - BNE"));

			if (string.IsNullOrEmpty(provider.InstructionHowToSetInterchangeSenderID))
			{
				Assert(!exceptionMessage.Contains("Please follow the instruction here to set it up."));
			}
			else
			{
				Assert(exceptionMessage.Contains("Please follow the instruction here to set it up."));
				Assert(exceptionMessage.Contains(provider.InstructionHowToSetInterchangeSenderID));
			}
		}

		protected abstract InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages);
	}
}
