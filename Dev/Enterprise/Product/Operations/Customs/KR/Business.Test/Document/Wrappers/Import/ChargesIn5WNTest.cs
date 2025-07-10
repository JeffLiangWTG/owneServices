using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ChargesIn5WN))]
	sealed class ChargesIn5WNTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapSerializable()
		{
			var fileReader = new TestFileReader(typeof(ChargesIn5WNTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine.xml");
			var snapshot = Factory.New<CusReconSnapshot>();
			snapshot.CRS_SnapshotXml = messageText;
			using var textReader = snapshot.GetCRS_SnapshotXmlReader();
			var importEntryOrEntryLine = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(textReader);

			var chargesIn5WN = new ChargesIn5WN(importEntryOrEntryLine.RefundAmounts[0], Factory);
			AssertEquals((short)1, chargesIn5WN.VersionNumber);
			AssertEquals(1m, chargesIn5WN.RefundAmounts.DutyAmount);
			AssertEquals(2m, chargesIn5WN.RefundAmounts.LiquorTaxAmount);
			AssertEquals(4m, chargesIn5WN.RefundAmounts.SpecialConsumptionTaxAmount);
			AssertEquals(8m, chargesIn5WN.RefundAmounts.TransportTaxAmount);
			AssertEquals(16m, chargesIn5WN.RefundAmounts.EducationTaxAmount);
			AssertEquals(32m, chargesIn5WN.RefundAmounts.AgricultureTaxAmount);
			AssertEquals(64m, chargesIn5WN.RefundAmounts.VATAmount);
			AssertEquals(640m, chargesIn5WN.RefundAmounts.ValueForVAT);
			AssertEquals(1000m, chargesIn5WN.RefundAmounts.ValueExemptForVAT);
			AssertEquals(256m, chargesIn5WN.RefundAmounts.TotalPenalty);
			AssertEquals(512m, chargesIn5WN.RefundAmounts.LateDeclarationPenalty);
			AssertEquals(1024m, chargesIn5WN.RefundAmounts.MissedDeclarationPenalty);
			AssertEquals(2048m, chargesIn5WN.RefundAmounts.LatePaymentPenalty);
			AssertEquals(4096m, chargesIn5WN.RefundAmounts.NonDutyTaxPayment);
			AssertEquals(8063m, chargesIn5WN.RefundAmounts.TotalPaid);

			chargesIn5WN = new ChargesIn5WN(importEntryOrEntryLine.RefundAmounts[1], Factory);
			AssertEquals((short)2, chargesIn5WN.VersionNumber);
			AssertEquals(10m, chargesIn5WN.RefundAmounts.DutyAmount);
			AssertEquals(20m, chargesIn5WN.RefundAmounts.LiquorTaxAmount);
			AssertEquals(40m, chargesIn5WN.RefundAmounts.SpecialConsumptionTaxAmount);
			AssertEquals(80m, chargesIn5WN.RefundAmounts.TransportTaxAmount);
			AssertEquals(160m, chargesIn5WN.RefundAmounts.EducationTaxAmount);
			AssertEquals(320m, chargesIn5WN.RefundAmounts.AgricultureTaxAmount);
			AssertEquals(640m, chargesIn5WN.RefundAmounts.VATAmount);
			AssertEquals(6400m, chargesIn5WN.RefundAmounts.ValueForVAT);
			AssertEquals(2000m, chargesIn5WN.RefundAmounts.ValueExemptForVAT);
			AssertEquals(2560m, chargesIn5WN.RefundAmounts.TotalPenalty);
			AssertEquals(5120m, chargesIn5WN.RefundAmounts.LateDeclarationPenalty);
			AssertEquals(10240m, chargesIn5WN.RefundAmounts.MissedDeclarationPenalty);
			AssertEquals(20480m, chargesIn5WN.RefundAmounts.LatePaymentPenalty);
			AssertEquals(40960m, chargesIn5WN.RefundAmounts.NonDutyTaxPayment);
			AssertEquals(80630m, chargesIn5WN.RefundAmounts.TotalPaid);
		}

		public void TestSerialize()
		{
			var charges = new ChargesSerializable()
			{
				DutyAmount = 1m,
				LiquorTaxAmount = 2m,
				SpecialConsumptionTaxAmount = 4m,
				TransportTaxAmount = 8m,
				EducationTaxAmount = 16m,
				AgricultureTaxAmount = 32m,
				VATAmount = 64m,
				ValueForVAT = 640m,
				ValueExemptForVAT = 1000m,
				TotalPenalty = 256m,
				LateDeclarationPenalty = 512m,
				MissedDeclarationPenalty = 1024m,
				LatePaymentPenalty = 2048m,
				NonDutyTaxPayment = 4096m,
				TotalPaid = 8063m,
			};

			var chargesIn5WNSerializable = new ChargesIn5WNSerializable();
			chargesIn5WNSerializable.VersionNumber = 1;
			chargesIn5WNSerializable.VersionDescription = "2020-10-14 (1)";
			chargesIn5WNSerializable.RefundAmounts = charges;

			using var stream = KRXmlObjectSerializer.Serialize(chargesIn5WNSerializable);
			var readerSource = new TextReaderSource(stream);
			var serializedXmlReader = readerSource.GetReader();

			var fileReader = new TestFileReader(typeof(ChargesIn5WNTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ChargesIn5WN.xml");
			AssertXMLEquals(messageText, serializedXmlReader.ReadToEnd());

			serializedXmlReader = readerSource.GetReader();
			var deserialized = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ChargesIn5WNSerializable>(serializedXmlReader);
			AssertEquals("VersionNumber", (short)1, deserialized.VersionNumber);
			AssertEquals("Description", "2020-10-14 (1)", deserialized.VersionDescription);
			AssertEquals("DutyAmount", 1m, deserialized.RefundAmounts.DutyAmount);
			AssertEquals("LiquorTaxAmount", 2m, deserialized.RefundAmounts.LiquorTaxAmount);
			AssertEquals("SpecialConsumptionTaxAmount", 4m, deserialized.RefundAmounts.SpecialConsumptionTaxAmount);
			AssertEquals("TransportTaxAmount", 8m, deserialized.RefundAmounts.TransportTaxAmount);
			AssertEquals("EducationTaxAmount", 16m, deserialized.RefundAmounts.EducationTaxAmount);
			AssertEquals("AgricultureTaxAmount", 32m, deserialized.RefundAmounts.AgricultureTaxAmount);
			AssertEquals("VATAmount", 64m, deserialized.RefundAmounts.VATAmount);
			AssertEquals("ValueForVAT", 640m, deserialized.RefundAmounts.ValueForVAT);
			AssertEquals("ValueExemptForVAT", 1000m, deserialized.RefundAmounts.ValueExemptForVAT);
			AssertEquals("TotalPenalty", 256m, deserialized.RefundAmounts.TotalPenalty);
			AssertEquals("LateDeclarationPenalty", 512m, deserialized.RefundAmounts.LateDeclarationPenalty);
			AssertEquals("MissedDeclarationPenalty", 1024m, deserialized.RefundAmounts.MissedDeclarationPenalty);
			AssertEquals("LatePaymentPenalty", 2048m, deserialized.RefundAmounts.LatePaymentPenalty);
			AssertEquals("NonDutyTaxPayment", 4096m, deserialized.RefundAmounts.NonDutyTaxPayment);
			AssertEquals("TotalPaid", 8063m, deserialized.RefundAmounts.TotalPaid);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var serializable = new ChargesIn5WNSerializable();
			return new ChargesIn5WN(serializable, Factory);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
