using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPFUPProvider))]
	sealed class EXPFUPProviderTest : InboundDataProviderTestCase<IEXPFUP, EXPFUPProvider>
	{
		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation.LRN = "123456";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("123456"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation.MRN = "123456";
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("123456"));
		}

		[ExpectNoExceptions]
		public void TestRequestDate()
		{
			message.ExportOperation.requestOnNonExitedExportDate = new DateTime(2020, 1, 2);
			NUnit.Framework.Assert.That(dataProvider.RequestDate, Is.EqualTo(new DateTime(2020, 1, 2)));
		}

		[ExpectNoExceptions]
		public void TestLatestPresentationDate()
		{
			message.ExportOperation.limitForPresentationDate = new DateTime(2020, 1, 2);
			NUnit.Framework.Assert.That(dataProvider.LatestPresentationDate, Is.EqualTo(new DateTime(2020, 1, 2)));
		}

		[ExpectNoExceptions]
		public void TestLatestResponseDate()
		{
			message.ExportOperation.limitForResponseDate = new DateTime(2020, 1, 2);
			NUnit.Framework.Assert.That(dataProvider.LatestResponseDate, Is.EqualTo(new DateTime(2020, 1, 2)));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "EXPFUP";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("EXPFUP"));
		}

		[ExpectNoExceptions]
		public void TestFollowUpType()
		{
			NUnit.Framework.Assert.That(dataProvider.FollowUpType, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "123456";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("123456"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPFE()
			{
				ExportOperation = new DEXPFEExportOperation()
			};
			dataProvider = new EXPFUPProvider(message);
		}
		DEXPFE message;
		IEXPFUP dataProvider;

		protected override EXPFUPProvider GetProvider() => (EXPFUPProvider)dataProvider;
	}
}
