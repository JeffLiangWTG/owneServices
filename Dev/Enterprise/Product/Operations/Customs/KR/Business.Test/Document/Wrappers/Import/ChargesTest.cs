using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Charges))]
	sealed class ChargesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapSerializable()
		{
			var fileReader = new TestFileReader(typeof(ChargesTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine.xml");
			var snapshot = Factory.New<CusReconSnapshot>();
			snapshot.CRS_SnapshotXml = messageText;
			using var textReader = snapshot.GetCRS_SnapshotXmlReader();
			var importEntryOrEntryLine = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(textReader);

			var charges = new Charges(importEntryOrEntryLine.PaidAmounts, Factory);
			AssertEquals(100m, charges.DutyAmount);
			AssertEquals(200m, charges.LiquorTaxAmount);
			AssertEquals(400m, charges.SpecialConsumptionTaxAmount);
			AssertEquals(800m, charges.TransportTaxAmount);
			AssertEquals(1600m, charges.EducationTaxAmount);
			AssertEquals(3200m, charges.AgricultureTaxAmount);
			AssertEquals(6400m, charges.VATAmount);
			AssertEquals(64000m, charges.ValueForVAT);
			AssertEquals(12800m, charges.ValueExemptForVAT);
			AssertEquals(25600m, charges.TotalPenalty);
			AssertEquals(51200m, charges.LateDeclarationPenalty);
			AssertEquals(102400m, charges.MissedDeclarationPenalty);
			AssertEquals(204800m, charges.LatePaymentPenalty);
			AssertEquals(409600m, charges.NonDutyTaxPayment);
			AssertEquals(806300m, charges.TotalPaid);
		}

		public void TestSerialize()
		{
			var chargesSerializable = new ChargesSerializable()
			{
				DutyAmount = 10m,
				LiquorTaxAmount = 20m,
				SpecialConsumptionTaxAmount = 40m,
				TransportTaxAmount = 80m,
				EducationTaxAmount = 160m,
				AgricultureTaxAmount = 320m,
				VATAmount = 640m,
				ValueForVAT = 6400m,
				ValueExemptForVAT = 2000m,
				TotalPenalty = 2560m,
				LateDeclarationPenalty = 5120m,
				MissedDeclarationPenalty = 10240m,
				LatePaymentPenalty = 20480m,
				NonDutyTaxPayment = 40960m,
				TotalPaid = 80630m,
			};

			using var stream = KRXmlObjectSerializer.Serialize(chargesSerializable);
			var readerSource = new TextReaderSource(stream);
			var serializedXmlReader = readerSource.GetReader();

			var fileReader = new TestFileReader(typeof(ChargesTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "Charges.xml");
			AssertXMLEquals(messageText, serializedXmlReader.ReadToEnd());

			serializedXmlReader = readerSource.GetReader();
			var deserialized = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ChargesSerializable>(serializedXmlReader);
			AssertEquals("DutyAmount", 10m, deserialized.DutyAmount);
			AssertEquals("LiquorTaxAmount", 20m, deserialized.LiquorTaxAmount);
			AssertEquals("SpecialConsumptionTaxAmount", 40m, deserialized.SpecialConsumptionTaxAmount);
			AssertEquals("TransportTaxAmount", 80m, deserialized.TransportTaxAmount);
			AssertEquals("EducationTaxAmount", 160m, deserialized.EducationTaxAmount);
			AssertEquals("AgricultureTaxAmount", 320m, deserialized.AgricultureTaxAmount);
			AssertEquals("VATAmount", 640m, deserialized.VATAmount);
			AssertEquals("ValueForVAT", 6400m, deserialized.ValueForVAT);
			AssertEquals("ValueExemptForVAT", 2000m, deserialized.ValueExemptForVAT);
			AssertEquals("TotalPenalty", 2560m, deserialized.TotalPenalty);
			AssertEquals("LateDeclarationPenalty", 5120m, deserialized.LateDeclarationPenalty);
			AssertEquals("MissedDeclarationPenalty", 10240m, deserialized.MissedDeclarationPenalty);
			AssertEquals("LatePaymentPenalty", 20480m, deserialized.LatePaymentPenalty);
			AssertEquals("NonDutyTaxPayment", 40960m, deserialized.NonDutyTaxPayment);
			AssertEquals("TotalPaid", 80630m, deserialized.TotalPaid);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var serializable = new ChargesSerializable();
			return new Charges(serializable, Factory);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
