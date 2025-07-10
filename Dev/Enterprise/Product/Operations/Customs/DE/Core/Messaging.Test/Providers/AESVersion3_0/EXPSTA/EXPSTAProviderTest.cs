using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPSTAProvider))]
	sealed class EXPSTAProviderTest : InboundDataProviderTestCase<IEXPSTA, EXPSTAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPSTAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestExportStatus()
		{
			message.ExportOperation.exportStatus = DEXPSEExportOperationExportStatus.Item000;
			NUnit.Framework.Assert.That(dataProvider.ExportStatus, Is.EqualTo("000"));
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
			message.correlationIdentifier = "CUSCAN58750000000518175240519105043";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExport()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CustomsOfficeOfExport, Is.EqualTo(default(string)), "CustomsOfficeOfExport missing - should be [null]");

				message.CustomsOfficeOfExport = new DEXPSECustomsOfficeOfExport
				{
					referenceNumber = "DE005866"
				};
				NUnit.Framework.Assert.That(dataProvider.CustomsOfficeOfExport, Is.EqualTo("DE005866"), "CustomsOfficeOfExport filled");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPSE()
			{
				ExportOperation = new DEXPSEExportOperation()
			};
			dataProvider = new EXPSTAProvider(message);
		}
		DEXPSE message;
		IEXPSTA dataProvider;

		protected override EXPSTAProvider GetProvider() => (EXPSTAProvider)dataProvider;
	}
}
