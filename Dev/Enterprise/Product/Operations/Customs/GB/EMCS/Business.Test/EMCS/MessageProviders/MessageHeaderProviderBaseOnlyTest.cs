using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class MessageHeaderProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new MessageHeaderProviderForTest(null));
		}

		[TestDate(2019, 10, 29)]
		public void TestDateOfPreparation()
		{
			AssertEquals(new DateTime(2019, 10, 29, 0, 0, 0), messageHeaderProvider.PreparationDateAndTime.Date);
		}

		[TestDate(2019, 10, 29, 10, 37, 56)]
		public void TestTimeOfPreparation()
		{
			AssertStartsWith("Preparation time", "10:37:56", messageHeaderProvider.PreparationDateAndTime.TimeOfDay.ToString());
		}

		public void TestMessageRecipient()
		{
			AssertEquals("NDEA.GB", messageHeaderProvider.Recipient);
		}

		public void TestMessageSender_Consignor()
		{
			AssertEquals("NDEA.GB", messageHeaderProvider.Sender);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals(EDIMessage.MessageNumberPlaceHolder, messageHeaderProvider.MessageIdentifier);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals(EDIMessage.MessageNumberPlaceHolder, messageHeaderProvider.CorrelationIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			messageHeaderProvider = new MessageHeaderProviderForTest(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		MessageHeaderProviderForTest messageHeaderProvider;
	}

	class MessageHeaderProviderForTest : MessageHeaderProvider<HeaderProviderBase>, IEMCSMessageHeader
	{
		public MessageHeaderProviderForTest(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
		}
	}
}
