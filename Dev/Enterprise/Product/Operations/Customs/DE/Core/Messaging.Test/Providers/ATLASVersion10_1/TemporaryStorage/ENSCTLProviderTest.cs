using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(ENSCTLProvider))]
	sealed class ENSCTLProviderTest : InboundDataProviderTestCase<ENSCTLProvider, IENSCTL>
	{
		[ExpectNoExceptions]
		public void TestMessageIdentifier() => NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("ENSCTL123456"));

		[ExpectNoExceptions]
		public void TestMessageRecipientIdentificationNumber() => NUnit.Framework.Assert.That(dataProvider.MessageRecipientIdentificationNumber, Is.EqualTo("DE123456"));

		[ExpectNoExceptions]
		public void TestMessageRecipientSubsidiaryNumber() => NUnit.Framework.Assert.That(dataProvider.MessageRecipientSubsidiaryNumber, Is.EqualTo("0000"));

		[ExpectNoExceptions]
		public void TestMRN() => NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"));

		[ExpectNoExceptions]
		public void TestTransportDocumentNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.TransportDocumentNumber, Is.EqualTo(default(string)), "No transportDocument - should be [null]");

				message.transportDocument = new DEIACATransportDocument { documentNumber = "654321" };
				NUnit.Framework.Assert.That(dataProvider.TransportDocumentNumber, Is.EqualTo("654321"), "TransportDocumentNumber populated");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEIACA()
			{
				messageIdentification = "ENSCTL123456",
				MessageRecipient = new DEIACAMessageRecipient()
				{
					identificationNumber = "DE123456",
					subsidiaryNumber = "0000"
				},
				MRN = "23DE586601055987B7"
			};
			dataProvider = new ENSCTLProvider(message);
		}
		DEIACA message;
		IENSCTL dataProvider;

		protected override IENSCTL GetProvider() => (ENSCTLProvider)dataProvider;
	}
}
