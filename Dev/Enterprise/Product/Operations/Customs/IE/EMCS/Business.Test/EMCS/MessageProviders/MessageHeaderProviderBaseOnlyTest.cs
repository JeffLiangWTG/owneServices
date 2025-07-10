using System;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
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
			AssertEquals(new DateTime(2019, 10, 29, 0, 0, 0), messageHeaderProvider.PreparationDateTime.Date);
		}

		[TestDate(2019, 10, 29, 14, 37, 21, 418)]
		public void TestPreparationTimeAsString()
		{
			AssertEquals("14:37:21.418", messageHeaderProvider.TimeOfPreparation);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("NDEA.IE", messageHeaderProvider.Recipient);
		}

		public void TestMessageSender_Consignor()
		{
			AssertEquals("NDEA.IE", messageHeaderProvider.Sender);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals(EDIMessage.MessageNumberPlaceHolder, messageHeaderProvider.MessageIdentifier);
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
