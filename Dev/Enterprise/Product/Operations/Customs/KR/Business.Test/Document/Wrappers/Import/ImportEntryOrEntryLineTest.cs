using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportEntryOrEntryLine))]
	sealed class ImportEntryOrEntryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapSerializable()
		{
			var fileReader = new TestFileReader(typeof(ImportEntryOrEntryLineTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine.xml");
			var snapshot = Factory.New<CusReconSnapshot>();
			snapshot.CRS_SnapshotXml = messageText;
			using var textReader = snapshot.GetCRS_SnapshotXmlReader();
			var serializable = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(textReader);

			var importEntryOrEntryLine = new ImportEntryOrEntryLine(serializable, Factory);
			AssertEquals(new DateTime(2024, 1, 23), importEntryOrEntryLine.PaidDate);
			var charges = importEntryOrEntryLine.PaidAmounts;
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

			var chargesIn5WNs = importEntryOrEntryLine.RefundAmounts.ToArray();
			var chargesIn5WN = chargesIn5WNs[0];
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

			chargesIn5WN = chargesIn5WNs[1];
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

		[TestDate(2024, 1, 23)]
		public void TestSerialize()
		{
			var charges = new ChargesSerializable()
			{
				DutyAmount = 100m,
				LiquorTaxAmount = 200m,
				SpecialConsumptionTaxAmount = 400m,
				TransportTaxAmount = 800m,
				EducationTaxAmount = 1600m,
				AgricultureTaxAmount = 3200m,
				VATAmount = 6400m,
				ValueForVAT = 64000m,
				ValueExemptForVAT = 12800m,
				TotalPenalty = 25600m,
				LateDeclarationPenalty = 51200m,
				MissedDeclarationPenalty = 102400m,
				LatePaymentPenalty = 204800m,
				NonDutyTaxPayment = 409600m,
				TotalPaid = 806300m,
			};

			var chargesFor5WN1 = new ChargesSerializable()
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

			var chargesFor5WN2 = new ChargesSerializable()
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

			var chargesIn5WN1 = new ChargesIn5WNSerializable();
			chargesIn5WN1.VersionNumber = 1;
			chargesIn5WN1.VersionDescription = "2024-01-01 (2)";
			chargesIn5WN1.RefundAmounts = chargesFor5WN1;

			var chargesIn5WN2 = new ChargesIn5WNSerializable();
			chargesIn5WN2.VersionNumber = 2;
			chargesIn5WN2.VersionDescription = "2024-01-20 (3)";
			chargesIn5WN2.RefundAmounts = chargesFor5WN2;

			var importEntryOrEntryLineSerializable = new ImportEntryOrEntryLineSerializable();
			importEntryOrEntryLineSerializable.PaidDate = new DateTime(2024, 1, 23);
			importEntryOrEntryLineSerializable.PaidAmounts = charges;
			importEntryOrEntryLineSerializable.RefundAmounts = [chargesIn5WN1, chargesIn5WN2];

			using var stream = KRXmlObjectSerializer.Serialize(importEntryOrEntryLineSerializable);
			var readerSource = new TextReaderSource(stream);
			var serializedXmlReader = readerSource.GetReader();

			var fileReader = new TestFileReader(typeof(ImportEntryOrEntryLineTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine.xml");
			AssertXMLEquals(messageText, serializedXmlReader.ReadToEnd());

			serializedXmlReader = readerSource.GetReader();
			var deserialized = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(serializedXmlReader);
			AssertEquals("PaidDate", new DateTime(2024, 1, 23), deserialized.PaidDate);
			AssertEquals("DutyAmount", 100m, deserialized.PaidAmounts.DutyAmount);
			AssertEquals("LiquorTaxAmount", 200m, deserialized.PaidAmounts.LiquorTaxAmount);
			AssertEquals("SpecialConsumptionTaxAmount", 400m, deserialized.PaidAmounts.SpecialConsumptionTaxAmount);
			AssertEquals("TransportTaxAmount", 800m, deserialized.PaidAmounts.TransportTaxAmount);
			AssertEquals("EducationTaxAmount", 1600m, deserialized.PaidAmounts.EducationTaxAmount);
			AssertEquals("AgricultureTaxAmount", 3200m, deserialized.PaidAmounts.AgricultureTaxAmount);
			AssertEquals("VATAmount", 6400m, deserialized.PaidAmounts.VATAmount);
			AssertEquals("ValueForVAT", 64000m, deserialized.PaidAmounts.ValueForVAT);
			AssertEquals("ValueExemptForVAT", 12800m, deserialized.PaidAmounts.ValueExemptForVAT);
			AssertEquals("TotalPenalty", 25600m, deserialized.PaidAmounts.TotalPenalty);
			AssertEquals("LateDeclarationPenalty", 51200m, deserialized.PaidAmounts.LateDeclarationPenalty);
			AssertEquals("MissedDeclarationPenalty", 102400m, deserialized.PaidAmounts.MissedDeclarationPenalty);
			AssertEquals("LatePaymentPenalty", 204800m, deserialized.PaidAmounts.LatePaymentPenalty);
			AssertEquals("NonDutyTaxPayment", 409600m, deserialized.PaidAmounts.NonDutyTaxPayment);
			AssertEquals("TotalPaid", 806300m, deserialized.PaidAmounts.TotalPaid);

			AssertEquals("VersionNumber", (short)1, deserialized.RefundAmounts[0].VersionNumber);
			AssertEquals("Description", "2024-01-01 (2)", deserialized.RefundAmounts[0].VersionDescription);
			AssertEquals("DutyAmount", 1m, deserialized.RefundAmounts[0].RefundAmounts.DutyAmount);
			AssertEquals("LiquorTaxAmount", 2m, deserialized.RefundAmounts[0].RefundAmounts.LiquorTaxAmount);
			AssertEquals("SpecialConsumptionTaxAmount", 4m, deserialized.RefundAmounts[0].RefundAmounts.SpecialConsumptionTaxAmount);
			AssertEquals("TransportTaxAmount", 8m, deserialized.RefundAmounts[0].RefundAmounts.TransportTaxAmount);
			AssertEquals("EducationTaxAmount", 16m, deserialized.RefundAmounts[0].RefundAmounts.EducationTaxAmount);
			AssertEquals("AgricultureTaxAmount", 32m, deserialized.RefundAmounts[0].RefundAmounts.AgricultureTaxAmount);
			AssertEquals("VATAmount", 64m, deserialized.RefundAmounts[0].RefundAmounts.VATAmount);
			AssertEquals("ValueForVAT", 640m, deserialized.RefundAmounts[0].RefundAmounts.ValueForVAT);
			AssertEquals("ValueExemptForVAT", 1000m, deserialized.RefundAmounts[0].RefundAmounts.ValueExemptForVAT);
			AssertEquals("TotalPenalty", 256m, deserialized.RefundAmounts[0].RefundAmounts.TotalPenalty);
			AssertEquals("LateDeclarationPenalty", 512m, deserialized.RefundAmounts[0].RefundAmounts.LateDeclarationPenalty);
			AssertEquals("MissedDeclarationPenalty", 1024m, deserialized.RefundAmounts[0].RefundAmounts.MissedDeclarationPenalty);
			AssertEquals("LatePaymentPenalty", 2048m, deserialized.RefundAmounts[0].RefundAmounts.LatePaymentPenalty);
			AssertEquals("NonDutyTaxPayment", 4096m, deserialized.RefundAmounts[0].RefundAmounts.NonDutyTaxPayment);
			AssertEquals("TotalPaid", 8063m, deserialized.RefundAmounts[0].RefundAmounts.TotalPaid);

			AssertEquals("VersionNumber", (short)2, deserialized.RefundAmounts[1].VersionNumber);
			AssertEquals("Description", "2024-01-20 (3)", deserialized.RefundAmounts[1].VersionDescription);
			AssertEquals("DutyAmount", 10m, deserialized.RefundAmounts[1].RefundAmounts.DutyAmount);
			AssertEquals("LiquorTaxAmount", 20m, deserialized.RefundAmounts[1].RefundAmounts.LiquorTaxAmount);
			AssertEquals("SpecialConsumptionTaxAmount", 40m, deserialized.RefundAmounts[1].RefundAmounts.SpecialConsumptionTaxAmount);
			AssertEquals("TransportTaxAmount", 80m, deserialized.RefundAmounts[1].RefundAmounts.TransportTaxAmount);
			AssertEquals("EducationTaxAmount", 160m, deserialized.RefundAmounts[1].RefundAmounts.EducationTaxAmount);
			AssertEquals("AgricultureTaxAmount", 320m, deserialized.RefundAmounts[1].RefundAmounts.AgricultureTaxAmount);
			AssertEquals("VATAmount", 640m, deserialized.RefundAmounts[1].RefundAmounts.VATAmount);
			AssertEquals("ValueForVAT", 6400m, deserialized.RefundAmounts[1].RefundAmounts.ValueForVAT);
			AssertEquals("ValueExemptForVAT", 2000m, deserialized.RefundAmounts[1].RefundAmounts.ValueExemptForVAT);
			AssertEquals("TotalPenalty", 2560m, deserialized.RefundAmounts[1].RefundAmounts.TotalPenalty);
			AssertEquals("LateDeclarationPenalty", 5120m, deserialized.RefundAmounts[1].RefundAmounts.LateDeclarationPenalty);
			AssertEquals("MissedDeclarationPenalty", 10240m, deserialized.RefundAmounts[1].RefundAmounts.MissedDeclarationPenalty);
			AssertEquals("LatePaymentPenalty", 20480m, deserialized.RefundAmounts[1].RefundAmounts.LatePaymentPenalty);
			AssertEquals("NonDutyTaxPayment", 40960m, deserialized.RefundAmounts[1].RefundAmounts.NonDutyTaxPayment);
			AssertEquals("TotalPaid", 80630m, deserialized.RefundAmounts[1].RefundAmounts.TotalPaid);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var serializable = new ImportEntryOrEntryLineSerializable();
			return new ImportEntryOrEntryLine(serializable, Factory);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
