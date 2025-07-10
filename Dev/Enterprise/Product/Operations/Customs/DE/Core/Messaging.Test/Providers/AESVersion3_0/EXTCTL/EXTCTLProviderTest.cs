using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTCTLProvider))]
	sealed class EXTCTLProviderTest : InboundDataProviderTestCase<IEXTCTL, EXTCTLProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTCTLProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			NUnit.Framework.Assert.That(provider.MessageIdentifier, Is.EqualTo("DE123456"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			NUnit.Framework.Assert.That(provider.ReferencedMessageIdentifier, Is.EqualTo("DE09876"));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			NUnit.Framework.Assert.That(provider.MovementReferenceNumber, Is.EqualTo("23DE12345678901238"));
		}

		[ExpectNoExceptions]
		public void TestTypeOfControls_Type()
		{
			var expected = new[] { "10", "20" };
			NUnit.Framework.Assert.That(provider.TypeOfControls.Select(t => t.Type), Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		public void TestTypeOfControls_Text()
		{
			var expected = new[] { "TEST1", "TEST2" };
			NUnit.Framework.Assert.That(provider.TypeOfControls.Select(t => t.Text), Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		public void TestTypeOfControls()
		{
			NUnit.Framework.Assert.That(provider.TypeOfControls.Count, Is.EqualTo(2));
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new DEXTLF
			{
				messageIdentification = "DE123456",
				correlationIdentifier = "DE09876",
				ExportOperation = new DEXTLFExportOperation { MRN = "23DE12345678901238" },
				TypeOfControls = new[]
				{
					new DEXTLFTypeOfControls { type = "10", text = "TEST1" },
					new DEXTLFTypeOfControls { type = "20", text = "TEST2" }
				}
			};

			provider = new EXTCTLProvider(message);
		}

		protected override EXTCTLProvider GetProvider() => provider;

		EXTCTLProvider provider;
		DEXTLF message;
	}
}
