using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class CUSCONMessageBuilderTest : MessageBuilderTest<CUSCONMessageBuilder, GCCONJ>
	{
		[ExpectNoExceptions]
		public void TestPopulateMessageGroup_ZVV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("ZVV");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(GCCONJMetaDataMessageGroup.ZVV));
		}

		[ExpectNoExceptions]
		public void TestPopulateMessageGroup_LVE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("LVE");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(GCCONJMetaDataMessageGroup.LVE));
		}

		[ExpectNoExceptions]
		public void TestPopulateMessageGroup_LVV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("LVV");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(GCCONJMetaDataMessageGroup.LVV));
		}

		[ExpectNoExceptions]
		public void TestPopulateMessageGroup_AVE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("AVE");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(GCCONJMetaDataMessageGroup.AVE));
		}

		public void TestPopulateMessageGroup_AVV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("AVV");
			AssertXMLContainsUnformatted("<MessageGroup>AVV</MessageGroup>", messageBuilder.GetXMLMessage().AsString());
		}

		[ExpectNoExceptions]
		public void TestPopulateInterchangeSender()
		{
			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(x => x.EoriNumber).Returns("DE99999999");
			interchangeSender.Setup(x => x.EoriBranchSuffix).Returns("0001");

			messageHeaderMock.Setup(m => m.InterchangeSender).Returns(interchangeSender.Object);

			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.MetaData.InterchangeSender.Identification.ReferenceNumber, Is.EqualTo("DE99999999"));
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.InterchangeSender.Identification.SubsidiaryNumber, Is.EqualTo("0001"));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInterchangeRecipient()
		{
			messageHeaderMock.Setup(m => m.InterchangeRecipientID).Returns("DE999999");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.InterchangeRecipient.Identification.ReferenceNumber, Is.EqualTo("DE999999"));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader()
		{
			messageHeaderMock.Setup(m => m.AuthorisationNumber).Returns(GetLongString("A", 26));

			headerMock.Setup(m => m.TemporaryReferenceNumber).Returns(GetLongString("T", 22));
			headerMock.Setup(m => m.LocalReferenceNumber).Returns(GetLongString("L", 36));
			headerMock.Setup(m => m.GoodsLocation).Returns(GetLongString("G", 36));

			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Header.AuthorisationNumber, Is.EqualTo(GetLongString("A", 26)));
				NUnit.Framework.Assert.That(message.Header.TemporaryReferenceNumber, Is.EqualTo(GetLongString("T", 22)));
				NUnit.Framework.Assert.That(message.Header.LRN, Is.EqualTo(GetLongString("L", 22)));
				NUnit.Framework.Assert.That(message.Header.GoodsLocation, Is.EqualTo(GetLongString("G", 36)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_TemporaryReferenceNumber()
		{
			headerMock.Setup(m => m.TemporaryReferenceNumber).Returns((string)null);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.TemporaryReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestPopulatePresentationConfirmer()
		{
			var presentationConfirmer = new Mock<IPartyID>();
			presentationConfirmer.Setup(x => x.EoriNumber).Returns("DE33333333");
			presentationConfirmer.Setup(x => x.EoriBranchSuffix).Returns("0000");

			headerMock.Setup(m => m.PresentationConfirmer).Returns(presentationConfirmer.Object);

			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.PresentationConfirmer.Identification.ReferenceNumber, Is.EqualTo("DE33333333"));
				NUnit.Framework.Assert.That(message.PresentationConfirmer.Identification.SubsidiaryNumber, Is.EqualTo("0000"));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulatePresentationConfirmer_Null()
		{
			headerMock.Setup(m => m.PresentationConfirmer).Returns((IPartyID)null);

			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().PresentationConfirmer, Is.EqualTo(default(GCCONJPresentationConfirmer)));
		}

		[ExpectNoExceptions]
		public void TestPopulateContactPerson()
		{
			var contactPerson = new Mock<IImportPartyContactPerson>();
			contactPerson.Setup(x => x.Position).Returns("Sacharbeiter");
			contactPerson.Setup(x => x.PersonName).Returns("Bob Baumeister");
			contactPerson.Setup(x => x.PhoneNumber).Returns("06131-123456");
			contactPerson.Setup(x => x.MailAddress).Returns("bob.baumeister@samplefreight.de");

			headerMock.Setup(m => m.ContactPerson).Returns(contactPerson.Object);

			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContactPerson.Position, Is.EqualTo("Sacharbeiter"));
				NUnit.Framework.Assert.That(message.ContactPerson.Name, Is.EqualTo("Bob Baumeister"));
				NUnit.Framework.Assert.That(message.ContactPerson.PhoneNumber, Is.EqualTo("06131-123456"));
				NUnit.Framework.Assert.That(message.ContactPerson.MailAddress, Is.EqualTo("bob.baumeister@samplefreight.de"));
			});
		}

		[ExpectNoExceptions]
		public void TestArrivalTransportMeans_NotPopulated()
		{
			CombineAssertions(() =>
			{
				headerMock.Setup(m => m.ArrivalTransportMeansIdentity).Returns((string)null);
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().ArrivalTransportMeans, Is.EqualTo(default(GCCONJArrivalTransportMeans)), "Null string - should be [null]");

				headerMock.Setup(m => m.ArrivalTransportMeansIdentity).Returns(string.Empty);
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().ArrivalTransportMeans, Is.EqualTo(default(GCCONJArrivalTransportMeans)), "Empty string - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousAdministrativeReferences_NotPopulated()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences.Type, Is.Not.EqualTo(default(GCCONJPreviousAdministrativeReferencesType)), "Contains PreviousAdministrativeReferences - should not be [null]");

				headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns(ZString.Empty);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences, Is.EqualTo(default(GCCONJPreviousAdministrativeReferences)), "Doesn't contain PreviousAdministrativeReferences - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeT1()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(GCCONJPreviousAdministrativeReferencesPreviousAdministrativeReference)), "Doesn't contain PreviousAdministrativeReference - should be [null]");

				headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.Not.EqualTo(default(GCCONJPreviousAdministrativeReferencesPreviousAdministrativeReference)), "Contains PreviousAdministrativeReference - should not be [null]");
			});
		}

		public void TestPreviousAdministrativeReference_NotPopulated_PreviousAdministrativeReferenceNumberEmpty()
		{
			CombineAssertions(() =>
			{
				headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
				AssertNotNullOrEmpty("Precondition", headerMock.Object.PreviousAdministrativeReferenceNumber);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.Not.EqualTo(default(GCCONJPreviousAdministrativeReferencesPreviousAdministrativeReference)), "Contains PreviousAdministrativeReference - should not be [null]");

				headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(ZString.Empty);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(GCCONJPreviousAdministrativeReferencesPreviousAdministrativeReference)), "Doesn't contain PreviousAdministrativeReference - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_PreviousAdministrativeReferenceTypeATNEU()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.Not.EqualTo(default(GCCONJSummaryDeclaration)), "Contains SummaryDeclaration - should not be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(GCCONJCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(GCCONJInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsitem_IdentificationByRegistration()
		{
			CombineAssertions(() =>
			{
				summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
				summaryGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("ATA123456789123456789");

				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"), "ReferencedRegistrationNumber should be mappped");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.MRN, Is.EqualTo(default(string)), "MRN should be Null - should be [null]");

				summaryGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("00DE000000000012E0");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo(default(string)), "ReferencedRegistrationNumber should be Null - should be [null]");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.MRN, Is.EqualTo("00DE000000000012E0"), "MRN should be mapped");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouseGoodsitem()
		{
			CombineAssertions(() =>
			{
				UpdateMockedHeaderToPopulateCustomsWarehouse();
				customsWarehouseGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456790");

				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456790"), "ReferencedRegistrationNumber should be mappped");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].MRN, Is.EqualTo(default(string)), "MRN should be Null - should be [null]");

				customsWarehouseGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("00DE000000000012E1");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo(default(string)), "ReferencedRegistrationNumber should be Null - should be [null]");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].MRN, Is.EqualTo("00DE000000000012E1"), "MRN should be mapped");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessingGoodsitem()
		{
			CombineAssertions(() =>
			{
				UpdateMockedHeaderToPopulateInwardProcessing();
				inwardProcessingGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456791");

				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456791"), "ReferencedRegistrationNumber should be mappped");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.GoodsItem[0].MRN, Is.EqualTo(default(string)), "MRN should be Null - should be [null]");

				inwardProcessingGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("00DE000000000012E2");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo(default(string)), "ReferencedRegistrationNumber should be Null - should be [null]");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.GoodsItem[0].MRN, Is.EqualTo("00DE000000000012E2"), "MRN should be mapped");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_PreviousAdministrativeReferenceTypeATZL()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(GCCONJSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.Not.EqualTo(default(GCCONJCustomsWarehouse)), "Contains CustomsWarehouse - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(GCCONJInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");

				NUnit.Framework.Assert.That(message.CustomsWarehouse.SequenceNumber, Is.EqualTo("1"), "Contains SequenceNumber");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItemQuantity, Is.EqualTo("10"), "Contains GoodsItemQuantity");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.CustomsAuthorisation.WarehouseOwner, Is.EqualTo("DE44444444"), "Contains WarehouseOwner");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo("WTG5678"), "Contains LRN");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].SequenceNumber, Is.EqualTo("1"), "Contains GoodsItem SequenceNumber");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"), "Contains ReferenceRegistrationNumber");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].ReferencedSequenceNumber, Is.EqualTo("1"), "Contains ReferenceSequenceNumber");
				var goodsItem = message.CustomsWarehouse.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.ReferencedSequenceNumber, Is.EqualTo("1"), "Contains ReferenceSequenceNumber");
				NUnit.Framework.Assert.That(goodsItem.AccessViaAtlasFlag, Is.EqualTo("J"), "Contains AccessViaAtlasFlag");
				NUnit.Framework.Assert.That(goodsItem.CommodityCode, Is.EqualTo("12345678"), "Contains CommodityCode");
				NUnit.Framework.Assert.That(goodsItem.UsualProcessingFlag, Is.EqualTo("J"), "Contains UsualProcessingFlag");
				NUnit.Framework.Assert.That(goodsItem.Complement, Is.EqualTo("Description text"), "Contains Complement");
				NUnit.Framework.Assert.That(goodsItem.CommercialAmount.Quantity, Is.EqualTo(1.234m), "Contains CommercialAmount Quantity");
				NUnit.Framework.Assert.That(goodsItem.CommercialAmount.MeasurementUnit, Is.EqualTo("KGM"), "Contains CommercialAmount MeasurementUnit");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"), "Contains CommercialAmount Qualifier");
				NUnit.Framework.Assert.That(goodsItem.DebitAmount.Quantity, Is.EqualTo(5.678m), "Contains DebitAmount Quantity");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.MeasurementUnit, Is.EqualTo("KGM"), "Contains DebitAmount MeasurementUnit");
				NUnit.Framework.Assert.That(goodsItem.DebitAmount.Qualifier, Is.EqualTo("M"), "Contains DebitAmount Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_PreviousAdministrativeReferenceTypeATAV()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(GCCONJSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(GCCONJCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.Not.EqualTo(default(GCCONJInwardProcessing)), "Contains InwardProcessing - should not be [null]");

				NUnit.Framework.Assert.That(message.InwardProcessing.SequenceNumber, Is.EqualTo("1"), "Contains SequenceNumber");
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItemQuantity, Is.EqualTo("20"), "Contains GoodsItemQuantity");
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"), "Contains ProcessingOwner");
				NUnit.Framework.Assert.That(message.InwardProcessing.SimplifiedGrantAuthorisationFlag, Is.EqualTo("J"), "Contains SimplifiedGrantAuthorisationFlag");
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"), "Contains MonitoringCustomsOffice Identification ReferenceNumber");
				var goodsItem = message.InwardProcessing.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.SequenceNumber, Is.EqualTo("1"), "Contains GoodsItem SequenceNumber");
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"), "Contains GoodsItem ReferenceRegistrationNumer");
				NUnit.Framework.Assert.That(goodsItem.ReferencedSequenceNumber, Is.EqualTo("1"), "Contains GoodsItem ReferencedSequenceNumber");
				NUnit.Framework.Assert.That(goodsItem.AccessViaAtlasFlag, Is.EqualTo("J"), "Contains GoodsItem AccesViaAtlasFlag");
				NUnit.Framework.Assert.That(goodsItem.GoodsRelatedInformation, Is.EqualTo("Related information"), "Contians GoodsItem GoodsRelatedInformation");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulate_PreviousAdministrativeReferenceTypeMiscellaneous()
		{
			UpdateMockedHeaderToPopulateNone();
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(GCCONJSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(GCCONJCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(GCCONJInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_ULD()
		{
			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(GCCONJSummaryDeclarationIdentificationIndicator.AWB), "AWB");

				summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
				NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(GCCONJSummaryDeclarationIdentificationIndicator.AWB), "ULD");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_REG()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.IdentificationIndicator, Is.EqualTo(GCCONJSummaryDeclarationIdentificationIndicator.AWB), "AWB");

				summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.IdentificationIndicator, Is.EqualTo(GCCONJSummaryDeclarationIdentificationIndicator.REG), "REG");
			});
		}

		[ExpectNoExceptions]
		public void TestIdentificationByKey_IdentificationByRegistration()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByKey.Kind, Is.EqualTo(GCCONJSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB), "AWB");

				summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().SummaryDeclaration.GoodsItem[0].IdentificationByKey.Kind, Is.EqualTo(GCCONJSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD), "ULD");
			});

			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			summaryGoodsItem.Setup(m => m.IdentificationByRegistrationReferencedRegistrationNumber).Returns(GetLongString("P", 36));
			var message = CreateMessageBuilder().GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(GCCONJSummaryDeclarationIdentificationIndicator.REG), "REG");
				NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo(GetLongString("P", 36)));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseLocalReferenceNumber_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(GetLongString("R", 36));
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.LRN, Is.EqualTo(GetLongString("R", 36)));

				customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.LRN, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.AccessViaATLASFlag).Returns(false);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemUsualProcessingFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.UsualProcessingFlag).Returns(false);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].UsualProcessingFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemComplement_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(GetLongString("C", 101));
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(GetLongString("C", 101)));

				customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(string.Empty);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.Not.EqualTo(default(GCCONJCustomsWarehouseGoodsItemCommercialAmount)), "Contains CommercialAmount - should not be [null]");

				customsWarehouseGoodsItemMock.Setup(m => m.CommercialAmount).Returns((IAmount)null);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.EqualTo(default(GCCONJCustomsWarehouseGoodsItemCommercialAmount)), "Doesn't contain CommercialAmount - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"), "Contains Qualifier");
				customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.MeasurementUnit).Returns("MMMM");
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.11m), "Quantity");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.MeasurementUnit, Is.EqualTo("MMM"), "MeasurementUnit");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo("M"), "Contains Qualifier");
				customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.MeasurementUnit).Returns("MMMM");
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.11m), "Quantity");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.MeasurementUnit, Is.EqualTo("MMM"), "MeasurementUnit");
			});
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingCustomsAuthorisation_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.CustomsAuthorisation, Is.Not.EqualTo(default(GCCONJInwardProcessingCustomsAuthorisation)), "Contains CustomsAuthorisation - should not be [null]");

				inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(string.Empty);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.CustomsAuthorisation, Is.EqualTo(default(GCCONJInwardProcessingCustomsAuthorisation)), "Doesn't contain CustomsAuthorisation - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingMonitoringCustomsOffice_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.MonitoringCustomsOffice, Is.Not.EqualTo(default(GCCONJInwardProcessingMonitoringCustomsOffice)), "Contains MonitoringCustomsOffice - should not be [null]");

				inwardProcessingMock.Setup(m => m.SimplifiedGrantAuthorisationFlag).Returns(false);
				NUnit.Framework.Assert.That(CreateMessageBuilder().GenerateMessage().InwardProcessing.MonitoringCustomsOffice, Is.EqualTo(default(GCCONJInwardProcessingMonitoringCustomsOffice)), "Doesn't contain MonitoringCustomsOffice - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingGoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingMock.Setup(m => m.MonitoringCustomsOfficeReferenceNumber).Returns(GetLongString("M", 9));
			inwardProcessingGoodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(GetLongString("R", 36));
			inwardProcessingGoodsItemMock.Setup(m => m.GoodsRelatedInformation).Returns(GetLongString("G", 351));
			inwardProcessingGoodsItemMock.Setup(m => m.AccessViaAtlasFlag).Returns(false);
			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo(GetLongString("M", 9)));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo(GetLongString("R", 36)));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].GoodsRelatedInformation, Is.EqualTo(GetLongString("G", 351)));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
			});
		}

		[ExpectNoExceptions]
		public void TestLongFieldsCutOff()
		{
			contactPersonMock.Setup(m => m.PersonName).Returns(GetLongString("N", ContactNameMaxLength + 1));
			contactPersonMock.Setup(m => m.Position).Returns(GetLongString("A", ContactPositionMaxLength + 1));
			contactPersonMock.Setup(m => m.PhoneNumber).Returns(GetLongString("N", ContactPhoneNumberMaxLength + 1));
			contactPersonMock.Setup(m => m.MailAddress).Returns(GetLongString("E", ContactEmailAdressMaxLength + 1));
			summaryGoodsItem.Setup(m => m.IdentificationByKeyCustodianIdentifier).Returns(GetLongString("E", EoriCodeMaxLength + 1));
			var message = messageBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContactPerson.Name, Is.EqualTo(GetLongString("N", ContactNameMaxLength)));
				NUnit.Framework.Assert.That(message.ContactPerson.Position, Is.EqualTo(GetLongString("A", ContactPositionMaxLength)));
				NUnit.Framework.Assert.That(message.ContactPerson.PhoneNumber, Is.EqualTo(GetLongString("N", ContactPhoneNumberMaxLength)));
				NUnit.Framework.Assert.That(message.ContactPerson.MailAddress, Is.EqualTo(GetLongString("E", ContactEmailAdressMaxLength)));
				NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByKey.Custodian.Identification.ReferenceNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)));
			});
		}

		[ExpectNoExceptions]
		public void TestLongFieldsCutOffCustomsWarehouse()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseMock.Setup(m => m.WarehouseOwnerIdentifier).Returns(GetLongString("W", WarehouseOwnerMaxLength + 1));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().CustomsWarehouse.CustomsAuthorisation.WarehouseOwner, Is.EqualTo(GetLongString("W", WarehouseOwnerMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestLongFieldsCutOffInwardProcessing()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(GetLongString("P", ProcessingOwnerMaxLength + 1));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo(GetLongString("P", ProcessingOwnerMaxLength)));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var presentationConfirmer = new Mock<IPartyID>();
			presentationConfirmer.Setup(x => x.EoriNumber).Returns("DE2222222");
			presentationConfirmer.Setup(x => x.EoriBranchSuffix).Returns("0002");

			contactPersonMock = new Mock<IImportPartyContactPerson>();
			contactPersonMock.Setup(x => x.Position).Returns("Niederlassungsleiter");
			contactPersonMock.Setup(x => x.PersonName).Returns("Max Mustermann");
			contactPersonMock.Setup(x => x.PhoneNumber).Returns("069-123 456 0");
			contactPersonMock.Setup(x => x.MailAddress).Returns("max.mustermann@samplefreight.de");

			summaryGoodsItem = new Mock<ISummaryDeclarationGoodsItem>();
			summaryGoodsItem.Setup(x => x.Quantity).Returns(1);
			summaryGoodsItem.Setup(x => x.IdentificationByKeyKind).Returns("AWB");
			summaryGoodsItem.Setup(x => x.IdentificationByKeyNumber).Returns("1234567890");
			summaryGoodsItem.Setup(x => x.IdentificationByKeyCustodianIdentifier).Returns("DE3333333");
			summaryGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("ATA123456789123456789");
			summaryGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedSequenceNumber).Returns(1);

			summaryDeclarationMock = new Mock<ISummaryDeclaration>();
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			summaryDeclarationMock.Setup(x => x.GoodsItems).Returns(new ISummaryDeclarationGoodsItem[] { summaryGoodsItem.Object });

			headerMock = new Mock<ICUSCONHeader>();
			headerMock.Setup(x => x.TemporaryReferenceNumber).Returns("ATA001203191020203302");
			headerMock.Setup(x => x.GoodsLocation).Returns("Wiesbaden - Biebrich");
			headerMock.Setup(x => x.LocalReferenceNumber).Returns("WTG1234");
			headerMock.Setup(x => x.PresentationConfirmer).Returns(presentationConfirmer.Object);
			headerMock.Setup(x => x.ContactPerson).Returns(contactPersonMock.Object);
			headerMock.Setup(x => x.ArrivalTransportMeansIdentity).Returns("Luftverkehr / Air");
			headerMock.Setup(x => x.PreviousAdministrativeReferenceType).Returns("ATNEU");
			headerMock.Setup(x => x.PreviousAdministrativeReferenceNumber).Returns("REFNUM12345");
			headerMock.Setup(x => x.SummaryDeclaration).Returns(summaryDeclarationMock.Object);

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(x => x.EoriNumber).Returns("DE1111111");
			interchangeSender.Setup(x => x.EoriBranchSuffix).Returns("0000");

			messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(x => x.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(x => x.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(x => x.MessageGroup).Returns("ZBV");
			messageHeaderMock.Setup(x => x.AuthorisationNumber).Returns("AUT123456789");
			messageHeaderMock.Setup(x => x.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			messageBuilder = CreateMessageBuilder();
		}

		CUSCONMessageBuilder CreateMessageBuilder()
		{
			return new CUSCONMessageBuilder(messageHeaderMock.Object);
		}

		CUSCONMessageBuilder messageBuilder;
		Mock<ICUSCONHeader> headerMock;
		Mock<IImportPartyContactPerson> contactPersonMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ICustomsWarehouse> customsWarehouseMock;
		Mock<ISummaryDeclarationGoodsItem> summaryGoodsItem;
		Mock<ICustomsWarehouseGoodsItem> customsWarehouseGoodsItemMock;
		Mock<IAmount> customsWarehouseGoodsItemCommercialAmountMock;
		Mock<IAmount> customsWarehouseGoodsItemDebitAmountMock;
		Mock<IInwardProcessing> inwardProcessingMock;
		Mock<IInwardProcessingGoodsItem> inwardProcessingGoodsItemMock;

		void UpdateMockedHeaderToPopulateCustomsWarehouse()
		{
			customsWarehouseGoodsItemCommercialAmountMock = new Mock<IAmount>();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(x => x.MeasurementUnit).Returns("KGM");
			customsWarehouseGoodsItemCommercialAmountMock.Setup(x => x.Quantity).Returns(1.234m);
			customsWarehouseGoodsItemCommercialAmountMock.Setup(x => x.Qualifier).Returns("A");

			customsWarehouseGoodsItemDebitAmountMock = new Mock<IAmount>();
			customsWarehouseGoodsItemDebitAmountMock.Setup(x => x.MeasurementUnit).Returns("KGM");
			customsWarehouseGoodsItemDebitAmountMock.Setup(x => x.Quantity).Returns(5.678m);
			customsWarehouseGoodsItemDebitAmountMock.Setup(x => x.Qualifier).Returns("M");

			customsWarehouseGoodsItemMock = new Mock<ICustomsWarehouseGoodsItem>();
			customsWarehouseGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456789");
			customsWarehouseGoodsItemMock.Setup(x => x.ReferencedSequenceNumber).Returns(1);
			customsWarehouseGoodsItemMock.Setup(x => x.AccessViaATLASFlag).Returns(true);
			customsWarehouseGoodsItemMock.Setup(x => x.CommodityCode).Returns("12345678");
			customsWarehouseGoodsItemMock.Setup(x => x.UsualProcessingFlag).Returns(true);
			customsWarehouseGoodsItemMock.Setup(x => x.Complement).Returns("Description text");
			customsWarehouseGoodsItemMock.Setup(x => x.CommercialAmount).Returns(customsWarehouseGoodsItemCommercialAmountMock.Object);
			customsWarehouseGoodsItemMock.Setup(x => x.DebitAmount).Returns(customsWarehouseGoodsItemDebitAmountMock.Object);

			customsWarehouseMock = new Mock<ICustomsWarehouse>();
			customsWarehouseMock.Setup(x => x.GoodsItemQuantity).Returns(10);
			customsWarehouseMock.Setup(x => x.WarehouseOwnerIdentifier).Returns("DE44444444");
			customsWarehouseMock.Setup(x => x.LocalReferenceNumber).Returns("WTG5678");
			customsWarehouseMock.Setup(x => x.GoodsItems).Returns(new ICustomsWarehouseGoodsItem[] { customsWarehouseGoodsItemMock.Object });

			headerMock.Setup(m => m.SummaryDeclaration).Returns((ISummaryDeclaration)null);
			headerMock.Setup(m => m.CustomsWarehouse).Returns(customsWarehouseMock.Object);
		}

		void UpdateMockedHeaderToPopulateInwardProcessing()
		{
			inwardProcessingGoodsItemMock = new Mock<IInwardProcessingGoodsItem>();
			inwardProcessingGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456789");
			inwardProcessingGoodsItemMock.Setup(x => x.ReferencedSequenceNumber).Returns(1);
			inwardProcessingGoodsItemMock.Setup(x => x.AccessViaAtlasFlag).Returns(true);
			inwardProcessingGoodsItemMock.Setup(x => x.GoodsRelatedInformation).Returns("Related information");

			inwardProcessingMock = new Mock<IInwardProcessing>();
			inwardProcessingMock.Setup(x => x.GoodsItemQuantity).Returns(20);
			inwardProcessingMock.Setup(x => x.ProcessingOwnerIdentifier).Returns("DE55555555");
			inwardProcessingMock.Setup(x => x.SimplifiedGrantAuthorisationFlag).Returns(true);
			inwardProcessingMock.Setup(x => x.MonitoringCustomsOfficeReferenceNumber).Returns("DE001234");
			inwardProcessingMock.Setup(x => x.GoodsItems).Returns(new IInwardProcessingGoodsItem[] { inwardProcessingGoodsItemMock.Object });

			headerMock.Setup(m => m.SummaryDeclaration).Returns((ISummaryDeclaration)null);
			headerMock.Setup(m => m.InwardProcessing).Returns(inwardProcessingMock.Object);
		}

		void UpdateMockedHeaderToPopulateNone()
		{
			headerMock.Setup(m => m.SummaryDeclaration).Returns((ISummaryDeclaration)null);
		}
	}
}
