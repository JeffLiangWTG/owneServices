using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class CUSWATMessageBuilderTest : MessageBuilderTest<CUSWATMessageBuilder, CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWK>
	{
		[TestDate(2021, 05, 01, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(messageBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestCUSWATMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}
		[ExpectNoExceptions]
		public void TestPopulateWarehouseOwner_NoDeclarant()
		{
			headerMock.Setup(m => m.WarehouseOwner).Returns((IPartyID)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.WarehouseOwner, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWKWarehouseOwner)));
		}

		[ExpectNoExceptions]
		public void TestPopulateWarehouseOwner_DeclarantNoEoriNumber()
		{
			var warehouseOwner = new Mock<IPartyID>();
			warehouseOwner.Setup(v => v.EoriNumber).Returns(string.Empty);
			warehouseOwner.Setup(v => v.EoriBranchSuffix).Returns(string.Empty);

			headerMock.Setup(h => h.WarehouseOwner).Returns(warehouseOwner.Object);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.WarehouseOwner, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWKWarehouseOwner)));
		}

		[ExpectNoExceptions]
		public void TestDepartureCustomsWarehouseSupervisingCustomsOffice_NotPopulated()
		{
			CombineAssertions(() =>
			{
				headerMock.Setup(x => x.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber).Returns(string.Empty);
				var message = messageBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.DepartureCustomsWarehouseSupervisingCustomsOffice, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWKDepartureCustomsWarehouseSupervisingCustomsOffice)), "empty string - should be [null]");

				headerMock.Setup(x => x.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber).Returns((string)null);
				message = messageBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.DepartureCustomsWarehouseSupervisingCustomsOffice, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWKDepartureCustomsWarehouseSupervisingCustomsOffice)), "null string - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateLine_MultipleGoodsItems()
		{
			headerMock.Setup(m => m.Lines).Returns(new[] { goodsItemMock.Object, goodsItemMock.Object });

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem.Length, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestDecisiveDate_Null()
		{
			goodsItemMock.Setup(l => l.DecisiveDate).Returns((DateTime?)null);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body.GoodsItem[0].DecisiveDate, Is.EqualTo(DateTime.MinValue));
		}

		[ExpectNoExceptions]
		public void TestPopulatePackage_NoQuantity()
		{
			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("CT");
			package.Setup(m => m.MarksNumbers).Returns(string.Empty);

			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package.Kind, Is.EqualTo("CT"));
		}

		[ExpectNoExceptions]
		public void TestPopulatePackage_NotSpecified()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWKBodyGoodsItemPackage)));
		}

		[ExpectNoExceptions]
		public void TestPopulateAssessmentCustomsValue_NotSpecified()
		{
			goodsItemMock.Setup(m => m.AssessmentCustomsValue).Returns(decimal.Zero);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Assessment.CustomsValueSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardMovementDepartureCustomsWarehouseReferenceNumber_MRN()
		{
			goodsItemMock.Setup(m => m.InwardMovementDepartureCustomsWarehouseReferenceNumber).Returns("23DE12345678901234");

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Body.GoodsItem[0].InwardMovementDepartureCustomsWarehouse.MRN, Is.EqualTo("23DE12345678901234"), "MRN");
				NUnit.Framework.Assert.That(message.Body.GoodsItem[0].InwardMovementDepartureCustomsWarehouse.ReferenceNumber, Is.EqualTo(default(string)), "ReferenceNumber - should be [null]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();

			var warehouseOwner = new Mock<IPartyID>();
			warehouseOwner.Setup(v => v.EoriNumber).Returns("AA123");
			warehouseOwner.Setup(v => v.EoriBranchSuffix).Returns("8172");

			var contactPerson = new Mock<IImportPartyContactPerson>();
			contactPerson.Setup(v => v.MailAddress).Returns("bob.baumeister@samplefreight.com");
			contactPerson.Setup(v => v.PersonName).Returns("Bob Baumeister");
			contactPerson.Setup(v => v.PhoneNumber).Returns("06131-477447");
			contactPerson.Setup(v => v.Position).Returns("Sachbearbeiter");

			var documents1 = new Mock<IImportDocument>();
			documents1.Setup(d => d.Type).Returns("N380");
			documents1.Setup(d => d.ReferenceNumber).Returns("DOC1REFERENCE");
			documents1.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 23));
			var documents2 = new Mock<IImportDocument>();
			documents2.Setup(d => d.Type).Returns("X123");
			documents2.Setup(d => d.ReferenceNumber).Returns("DOC2REFERENCE");
			documents2.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 24));

			headerMock = new Mock<ICUSWATHeader>();
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.CustomsWarehouseDepartureLocalReferenceNumber).Returns("CWDLRN123");
			headerMock.Setup(h => h.CustomsWarehouseDeparture).Returns("DE0000AB1234");
			headerMock.Setup(h => h.CurrentProcedure).Returns("DE0000CD5678");
			headerMock.Setup(h => h.DeclarationPlace).Returns("Hamburg");
			headerMock.Setup(h => h.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber).Returns("DE001234");
			headerMock.Setup(h => h.WarehouseOwner).Returns(warehouseOwner.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(h => h.Documents).Returns(new[] { documents1.Object, documents2.Object });
			headerMock.Setup(m => m.Lines).Returns(new[] { goodsItemMock.Object });

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(i => i.EoriNumber).Returns("DE8999783");
			interchangeSender.Setup(i => i.EoriBranchSuffix).Returns("0000");

			var messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(h => h.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(h => h.AuthorisationNumber).Returns("1234567890AUTH");
			messageHeaderMock.Setup(h => h.MessageGroup).Returns(ImportMessageSubTypeList.Codes.WarehouseStockTransfer);
			messageHeaderMock.Setup(h => h.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			messageBuilder = new CUSWATMessageBuilder(messageHeaderMock.Object);
		}
		CUSWATMessageBuilder messageBuilder;
		Mock<ICUSWATHeader> headerMock;
		Mock<ICUSWATLine> goodsItemMock;

		void SetupGoodsItem()
		{
			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("CT");
			package.Setup(p => p.Quantity).Returns(970);
			package.Setup(p => p.MarksNumbers).Returns("1-970");

			var assessmentSpecificRate1 = new Mock<IImportSpecificRate>();
			assessmentSpecificRate1.Setup(r => r.Type).Returns("X");
			assessmentSpecificRate1.Setup(r => r.Value).Returns(10.02m);
			var assessmentSpecificRate2 = new Mock<IImportSpecificRate>();
			assessmentSpecificRate2.Setup(r => r.Type).Returns("Y");
			assessmentSpecificRate2.Setup(r => r.Value).Returns(20.40m);

			var assessmentContentInformation1 = new Mock<IContentInformation>();
			assessmentContentInformation1.Setup(r => r.ContentType).Returns("XY");
			assessmentContentInformation1.Setup(r => r.DegreePercentage).Returns(0.01m);
			var assessmentContentInformation2 = new Mock<IContentInformation>();
			assessmentContentInformation2.Setup(r => r.ContentType).Returns("AA");
			assessmentContentInformation2.Setup(r => r.DegreePercentage).Returns(4.20m);

			var exciseDuty1 = new Mock<IExciseDuty>();
			exciseDuty1.Setup(d => d.Code).Returns("E789");
			exciseDuty1.Setup(d => d.DegreePercentage).Returns(0.02m);
			exciseDuty1.Setup(d => d.Value).Returns(1626.28m);
			exciseDuty1.Setup(d => d.Amount).Returns(GetAmount(21.32m, "NAR", "X").Object);
			var exciseDuty2 = new Mock<IExciseDuty>();
			exciseDuty2.Setup(d => d.Code).Returns("F789");
			exciseDuty2.Setup(d => d.DegreePercentage).Returns(decimal.Zero);
			exciseDuty2.Setup(d => d.Value).Returns(decimal.Zero);
			exciseDuty2.Setup(d => d.Amount).Returns(GetAmount(76.30m, "ABC", "Z").Object);

			var documents1 = new Mock<IImportLineDocument>();
			documents1.Setup(d => d.Division).Returns("4");
			documents1.Setup(d => d.DocumentType).Returns("7HHF");
			documents1.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents1.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents1.Setup(d => d.AtHandFlag).Returns("J");
			documents1.Setup(d => d.WriteOff).Returns(GetAmount(1500, "NAR", "Z").Object);

			var documents2 = new Mock<IImportLineDocument>();
			documents2.Setup(d => d.Division).Returns("6");
			documents2.Setup(d => d.DocumentType).Returns("9AAB");
			documents2.Setup(d => d.ReferenceNumber).Returns("AABU6271657530");
			documents2.Setup(d => d.IssuingDate).Returns((DateTime?)null);
			documents2.Setup(m => m.AtHandFlag).Returns("N");

			goodsItemMock = new Mock<ICUSWATLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("7171");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.ArticleNumber).Returns("ANZ1203");
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.DepartureCountry).Returns("JP");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.DecisiveDate).Returns(new DateTime(2020, 01, 15));
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12", "B34" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A123", "B456" });
			goodsItemMock.Setup(l => l.ContainerFlag).Returns(true);
			goodsItemMock.Setup(l => l.ContainerIdentificationNumbers).Returns(new[] { "CON1", "CON2" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.InwardMovementAmount).Returns(GetAmount(2134.24m, "KGM", "A").Object);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.60m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new[] {
					GetAmount(18219, "NAR", "X").Object,
					GetAmount(21.20m, "KGM", "Y").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new[] {
							assessmentSpecificRate1.Object,
							assessmentSpecificRate2.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new[] {
							assessmentContentInformation1.Object,
							assessmentContentInformation2.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new[] {
							exciseDuty1.Object,
							exciseDuty2.Object });
			goodsItemMock.Setup(l => l.RequestedPreferentialTreatment).Returns("200");
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseReferenceNumber).Returns("REF123");
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseSequenceNumber).Returns(12);
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag).Returns(true);
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseUsualProcessingFlag).Returns(false);
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseAdditionalInformation).Returns("ADDITIONAL INFO");
			goodsItemMock.Setup(l => l.InwardMovementDepartureCustomsWarehouseDebitAmount).Returns(GetAmount(2120.655m, "KGM", "Z").Object);
			goodsItemMock.Setup(l => l.Documents).Returns(new[] {
							documents1.Object,
							documents2.Object });
		}
	}
}
