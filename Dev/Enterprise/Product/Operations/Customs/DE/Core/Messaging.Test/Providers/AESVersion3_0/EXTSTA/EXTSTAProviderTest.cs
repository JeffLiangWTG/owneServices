using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTSTAProvider))]
	sealed class EXTSTAProviderTest : InboundDataProviderTestCase<IEXTSTA, EXTSTAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTSTAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestExitStatus()
		{
			message.ExportOperation.exitStatus = DEXTSEExportOperationExitStatus.Item000;
			NUnit.Framework.Assert.That(dataProvider.ExitStatus, Is.EqualTo("000"));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation.MRN = "00DE000000000000E0";
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("00DE000000000000E0"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation.LRN = "local reference number";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "0000000001";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0000000001"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "Test5678";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("Test5678"));
		}

		protected override EXTSTAProvider GetProvider() => (EXTSTAProvider)dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXTSE()
			{
				ExportOperation = new DEXTSEExportOperation()
			};
			dataProvider = new EXTSTAProvider(message);
		}
		DEXTSE message;
		IEXTSTA dataProvider;
	}
}
