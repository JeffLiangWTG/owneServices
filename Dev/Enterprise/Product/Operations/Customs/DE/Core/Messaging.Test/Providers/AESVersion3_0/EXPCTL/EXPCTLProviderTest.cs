using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPCTLProvider))]
	sealed class EXPCTLProviderTest : InboundDataProviderTestCase<IEXPCTL, EXPCTLProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPCTLProvider(null));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "outgoing";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("outgoing"));
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
		public void TestLines()
		{
			NUnit.Framework.Assert.That(dataProvider.Lines.Count, Is.EqualTo(2));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPLD()
			{
				ExportOperation = new DEXPLDExportOperation(),
				TypeOfControls = new DEXPLDTypeOfControls[]
				{
					new DEXPLDTypeOfControls(),
					new DEXPLDTypeOfControls()
				}
			};
			dataProvider = new EXPCTLProvider(message);
		}
		DEXPLD message;
		IEXPCTL dataProvider;

		protected override EXPCTLProvider GetProvider() => (EXPCTLProvider)dataProvider;
	}
}
