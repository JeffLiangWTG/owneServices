using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPREJProvider))]
	sealed class EXPREJProviderTest : InboundDataProviderTestCase<IEXPREJ, EXPREJProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPREJProvider(null));
		}

		[ExpectNoExceptions]
		public void TestExportStatus()
		{
			message.ExportOperation = new DEXPJEExportOperation
			{
				businessRejectionType = DEXPJEExportOperationBusinessRejectionType.Item513
			};
			NUnit.Framework.Assert.That(dataProvider.ExportStatus, Is.EqualTo("513"));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation = new DEXPJEExportOperation
			{
				MRN = "00DE000000000000E0"
			};
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("00DE000000000000E0"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation = new DEXPJEExportOperation
			{
				LRN = "local reference number"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			message.ExportOperation = new DEXPJEExportOperation();
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "CUSCAN58750000000518175240519105043";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "0000000001";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0000000001"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPJE();
			dataProvider = new EXPREJProvider(message);
		}
		DEXPJE message;
		IEXPREJ dataProvider;

		protected override EXPREJProvider GetProvider() => (EXPREJProvider)dataProvider;
	}
}
