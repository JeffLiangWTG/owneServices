using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class CFCDECMessageBuilderTest : MessageBuilderTest<CFCDECMessageBuilder, FCFCDF>
	{
		[ExpectNoExceptions]
		public void TestMessageGroup_ZBV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("ZBV");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.MetaData.MessageGroup, Is.EqualTo(FCFCDFMetaDataMessageGroup.ZBV));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(cfcdecBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestPopulateBorderTransportMeans_TruncatedInformation()
		{
			headerMock.Setup(m => m.BorderTransportMeansInformation).Returns(string.Empty.PadLeft(18, 'A'));

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.BorderTransportMeans.Information, Is.EqualTo(string.Empty.PadLeft(17, 'A')));
		}

		[ExpectNoExceptions]
		public void TestPopulateForeignTradeStatisticsTotalGrossMassMeasure_NotEmitWhen0()
		{
			headerMock.Setup(m => m.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(0m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(!message.Body.ForeignTradeStatistics.TotalGrossMassMeasureSpecified, Is.True, "<TotalGrossMassMeasure> not contained");
		}

		[ExpectNoExceptions]
		public void TestPopulatePaymentTransaction_Round()
		{
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.23333m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.PaymentTransaction.Amount, Is.EqualTo(12345678901.23m));
		}

		[ExpectNoExceptions]
		public void TestPopulatePaymentTransaction_Normalize()
		{
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.2000m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.PaymentTransaction.Amount, Is.EqualTo(12345678901.2m));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeT1()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(FCFCDFPreviousAdministrativeReferencesType.T1));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference.ReferenceNumber, Is.EqualTo("REFNUM12345"));
		}

		[ExpectNoExceptions]
		public void TestPreviousAdministrativeReference_NotPopulated_PreviousAdministrativeReferenceNumberEmpty()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(string.Empty);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(FCFCDFPreviousAdministrativeReferencesType.ATNEU));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(FCFCDFPreviousAdministrativeReferencesPreviousAdministrativeReference)));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_PreviousAdministrativeReferenceTypeATZL()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			var message = cfcdecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCDFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.Not.EqualTo(default(FCFCDFCustomsWarehouse)), "Contain CustomsWarehouse - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(FCFCDFInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var goodsItemMock = Mock.Get(headerMock.Object.CustomsWarehouse.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = cfcdecBuilder.GenerateMessage();

			var goodsItem = message.CustomsWarehouse.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_PreviousAdministrativeReferenceTypeATAV()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			var message = cfcdecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCDFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(FCFCDFCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.Not.EqualTo(default(FCFCDFInwardProcessing)), "Contains InwardProcessing - should not be [null]");

				NUnit.Framework.Assert.That(message.InwardProcessing.SequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItemQuantity, Is.EqualTo("20"));
				NUnit.Framework.Assert.That(message.InwardProcessing.SimplifiedGrantAuthorisationFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"));
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].SequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].ReferencedSequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].GoodsRelatedInformation, Is.EqualTo("Related information"));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var goodsItemMock = Mock.Get(headerMock.Object.InwardProcessing.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = cfcdecBuilder.GenerateMessage();

			var goodsItem = message.InwardProcessing.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulate_PreviousAdministrativeReferenceTypeMiscellaneous()
		{
			UpdateMockedHeaderToPopulateNone();

			var message = cfcdecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCDFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(FCFCDFCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(FCFCDFInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(FCFCDFSummaryDeclarationIdentificationIndicator.AWB));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_REG()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(FCFCDFSummaryDeclarationIdentificationIndicator.REG));
		}

		[ExpectNoExceptions]
		public void TestIdentificationByKey_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByKey.Kind, Is.EqualTo(FCFCDFSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD));
		}

		[ExpectNoExceptions]
		public void TestIdentificationByRegistration()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedSequenceNumber, Is.EqualTo("1"));
		}

		[ExpectNoExceptions]
		public void TestIdentificationByRegistration_MRN()
		{
			var goodsItemMock = Mock.Get(summaryDeclarationMock.Object.GoodsItems.Single());
			goodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("23DE586601055987B7");
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");

			var message = cfcdecBuilder.GenerateMessage();

			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseLocalReferenceNumber_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo("WTG5678"));

				customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.AccessViaATLASFlag).Returns(false);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemUsualProcessingFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.UsualProcessingFlag).Returns(false);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].UsualProcessingFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseGoodsItemComplement_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo("Description text"), "Contains Complement");

				customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(string.Empty);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.Not.EqualTo(default(FCFCDFCustomsWarehouseGoodsItemCommercialAmount)), "Contains CommercialAmount - should not be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(1.234m));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.MeasurementUnit, Is.EqualTo("KGM"));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"));

				customsWarehouseGoodsItemMock.Setup(m => m.CommercialAmount).Returns((IAmount)null);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.EqualTo(default(FCFCDFCustomsWarehouseGoodsItemCommercialAmount)), "Doesn't contain CommercialAmount - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"), "Contains Qualifier");

				customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1111m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseCommercialAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1100m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo("M"), "Contains Qualifier");

				customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1111m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouseDebitAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1100m);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingCustomsAuthorisation_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation, Is.Not.EqualTo(default(FCFCDFInwardProcessingCustomsAuthorisation)), "Contains CustomsAuthorisation - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"));

				inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(string.Empty);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation, Is.EqualTo(default(FCFCDFInwardProcessingCustomsAuthorisation)), "Doesn't contain CustomsAuthorisation - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingMonitoringCustomsOffice_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				var message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.Not.EqualTo(default(FCFCDFInwardProcessingMonitoringCustomsOffice)), "Contains MonitoringCustomsOffice - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"));

				inwardProcessingMock.Setup(m => m.SimplifiedGrantAuthorisationFlag).Returns(false);
				message = CreateCFCDECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.EqualTo(default(FCFCDFInwardProcessingMonitoringCustomsOffice)), "Doesn't contain MonitoringCustomsOffice - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestInwardProcessingGoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingGoodsItemMock.Setup(m => m.AccessViaAtlasFlag).Returns(false);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IImportParty)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(FCFCDFRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var importParty = new Mock<IImportParty>();
			importParty.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			headerMock.Setup(m => m.Representative).Returns(importParty.Object);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(FCFCDFRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateContactPerson_Null()
		{
			headerMock.Setup(m => m.ContactPerson).Returns((IImportPartyContactPerson)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.ContactPerson.Name, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(message.ContactPerson.MailAddress, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(message.ContactPerson.PhoneNumber, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(message.ContactPerson.Position, Is.EqualTo(default(string)));
		}

		[TestDate(2020, 10, 22, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			cfcdecBuilder = new CFCDECMessageBuilder(messageHeaderMock.Object);
			var actual = cfcdecBuilder.GetXMLMessage().AsString();
			NUnit.Framework.Assert.That(actual, Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestCFCDECMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestOriginCountry_EqualsPreferentialOriginCntryAndRequestedPreferentialTreatmentMoreThan199()
		{
			goodsItemMock.Setup(m => m.OriginCountry).Returns("CN");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(default(string)), "No Origin country when same as Preferential Origin Country - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestOriginCountry_Empty()
		{
			goodsItemMock.Setup(m => m.OriginCountry).Returns("");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(default(string)), "No Origin country when different to Preferential Origin Country and empty - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineOriginCountry_RequestedPreferentialTreatmentLess200()
		{
			preferentialTreatmentMock.Setup(m => m.RequestedPreferentialTreatment).Returns("199");
			goodsItemMock.Setup(m => m.OriginCountry).Returns("CN");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo("CN"));
		}

		public void TestPopulateLineOriginCountry_WhenPreferentialTreatmentIsNull()
		{
			goodsItemMock.Setup(m => m.PreferentialTreatment).Returns((ILinePreferentialTreatment)null);
			goodsItemMock.Setup(m => m.OriginCountry).Returns("CN");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			FCFCDF message = null;

			AssertNoExceptionThrown(() => message = cfcdecBuilder.GenerateMessage());
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalProcedure()
		{
			goodsItemMock.Setup(m => m.AdditionalProcedure).Returns(new[] { GetLongString("C", 9) });

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].AdditionalProcedure[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulateSupplementaryCodes()
		{
			goodsItemMock.Setup(m => m.SupplementaryCodes).Returns(new[] { GetLongString("C", 9) });

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].SupplementaryCodes[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePackage_NoPackages()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package, Is.EqualTo(default(FCFCDFBodyGoodsItemPackage)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueNetPrice_CurrencyIsEUR()
		{
			netPriceMock.Setup(m => m.CurrencyCode).Returns("EUR");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.NetPrice.Value, Is.EqualTo(123877.15m));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.NetPrice.CurrencyCode, Is.EqualTo("EUR"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueIndirectPayment_Empty()
		{
			customsValueMock.Setup(m => m.CustomsValueIndirectPayment).Returns((IImportCosts)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.IndirectPayment, Is.EqualTo(default(FCFCDFBodyGoodsItemCustomsValueIndirectPayment)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueIndirectPayment_CurrencyIsEUR()
		{
			indirectPaymentMock.Setup(m => m.CurrencyCode).Returns("EUR");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.IndirectPayment.Value, Is.EqualTo(2939.04m));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.IndirectPayment.CurrencyCode, Is.EqualTo("EUR"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueAirFreightCosts_Empty()
		{
			customsValueMock.Setup(m => m.CustomsValueAirFreightCosts).Returns((IAirFreightCosts)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AirFreightCosts, Is.EqualTo(default(FCFCDFBodyGoodsItemCustomsValueAirFreightCosts)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueAirFreightCosts_CurrencyIsEUR()
		{
			airFreightCostsMock.Setup(m => m.CurrencyCode).Returns("EUR");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AirFreightCosts.Value, Is.EqualTo(624.51m));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AirFreightCosts.CurrencyCode, Is.EqualTo("EUR"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueAdditionDeduction_CurrencyIsEUR()
		{
			additionDeductionMock.Setup(m => m.CurrencyCode).Returns("EUR");

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].Type, Is.EqualTo("015"));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].Value, Is.EqualTo(827.25m));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].CurrencyCode, Is.EqualTo("EUR"));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].Percentage, Is.EqualTo(0.15m));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineCustomsValueAdditionDeduction_NoPercentage()
		{
			additionDeductionMock.Setup(m => m.Percentage).Returns(decimal.Zero);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].CurrencyRateIATA, Is.EqualTo("J"));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].CurrencyRateAgreedFlag, Is.EqualTo("N"));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].CurrencyRate, Is.EqualTo(1m));
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].CurrencyRateDate, Is.EqualTo(new DateTime(2020, 10, 30)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreferentialTreatmentContingentNumber()
		{
			preferentialTreatmentMock.Setup(m => m.ContingentNumber).Returns(new[] { GetLongString("C", 9) });

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].PreferentialTreatment.Declaration.Contingent[0].ContingentNumber, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreferentialTreatmentQuantity_Empty()
		{
			preferentialTreatmentMock.Setup(m => m.Quantity).Returns((IAmount)null);

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].PreferentialTreatment.Declaration.PreferentialTreatmentQuantity, Is.EqualTo(default(FCFCDFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity)));
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalDutyReferences_Empty()
		{
			headerMock.Setup(m => m.AdditionalDutyReferences).Returns((IReadOnlyCollection<IImportAdditionalDutyReference>)Enumerable.Empty<IImportAdditionalDutyReference>());

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences.Length, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalDutyReferences_EmptyOwner()
		{
			var additionalDutyReferences = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences.Setup(duty => duty.ReferenceNumber).Returns("CFR00001");
			additionalDutyReferences.Setup(duty => duty.DutyInterestedParty).Returns((IImportParty)null);

			headerMock.Setup(h => h.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[] { additionalDutyReferences.Object });

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences[0].ReferenceNumber, Is.EqualTo("CFR00001"));
			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences[0].DutyInterestedParty, Is.EqualTo(default(FCFCDFBodyAdditionalDutyReferencesDutyInterestedParty)));
		}

		[ExpectNoExceptions]
		public void TestPopulateDocuments_WriteOff_Empty()
		{
			var importLineDocument = new Mock<IImportLineDocument>();
			importLineDocument.Setup(d => d.Division).Returns("4");
			importLineDocument.Setup(d => d.DocumentType).Returns("7HHF");
			importLineDocument.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			importLineDocument.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			importLineDocument.Setup(d => d.AtHandFlag).Returns("J");
			importLineDocument.Setup(m => m.WriteOff).Returns((IAmount)null);

			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { importLineDocument.Object });

			var message = cfcdecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Document[0].WriteOff, Is.EqualTo(default(FCFCDFBodyGoodsItemDocumentWriteOff)));
		}

		void SetupGoodsItem()
		{
			additionDeductionMock = new Mock<IAdditionDeduction>();
			additionDeductionMock.Setup(c => c.Type).Returns("015");
			additionDeductionMock.Setup(c => c.Value).Returns(827.25m);
			additionDeductionMock.Setup(c => c.CurrencyCode).Returns("RUB");
			additionDeductionMock.Setup(c => c.CurrencyRateIATA).Returns(true);
			additionDeductionMock.Setup(c => c.CurrencyRateAgreedFlag).Returns(false);
			additionDeductionMock.Setup(c => c.CurrencyRate).Returns(1m);
			additionDeductionMock.Setup(c => c.CurrencyRateDate).Returns(new DateTime(2020, 10, 30));
			additionDeductionMock.Setup(m => m.Percentage).Returns(0.15m);

			airFreightCostsMock = new Mock<IAirFreightCosts>();
			airFreightCostsMock.Setup(c => c.Value).Returns(624.51m);
			airFreightCostsMock.Setup(c => c.CurrencyCode).Returns("RUB");
			airFreightCostsMock.Setup(c => c.CurrencyRateIATA).Returns(true);
			airFreightCostsMock.Setup(c => c.CurrencyRateAgreedFlag).Returns(false);
			airFreightCostsMock.Setup(c => c.CurrencyRate).Returns(1m);
			airFreightCostsMock.Setup(m => m.CurrencyRateDate).Returns(new DateTime(2020, 10, 30));

			indirectPaymentMock = new Mock<IImportCosts>();
			indirectPaymentMock.Setup(c => c.Value).Returns(2939.04m);
			indirectPaymentMock.Setup(c => c.CurrencyCode).Returns("USD");
			indirectPaymentMock.Setup(c => c.CurrencyRateAgreedFlag).Returns(false);
			indirectPaymentMock.Setup(m => m.CurrencyRate).Returns(1m);

			netPriceMock = new Mock<IImportCosts>();
			netPriceMock.Setup(c => c.Value).Returns(123877.15m);
			netPriceMock.Setup(c => c.CurrencyCode).Returns("USD");
			netPriceMock.Setup(c => c.CurrencyRateAgreedFlag).Returns(false);
			netPriceMock.Setup(m => m.CurrencyRate).Returns(1m);

			preferentialTreatmentMock = new Mock<ILinePreferentialTreatment>();
			preferentialTreatmentMock.Setup(l => l.RequestedPreferentialTreatment).Returns("200");
			preferentialTreatmentMock.Setup(l => l.ContingentNumber).Returns(new[] { "1XYZ" });
			preferentialTreatmentMock.Setup(m => m.Quantity).Returns(GetAmount(18219, "NAR", "X").Object);

			customsValueMock = new Mock<IImportLineCustomsValue>();
			customsValueMock.Setup(c => c.CustomsValueDepartureAirport).Returns("DXB");
			customsValueMock.Setup(c => c.CustomsValueDestinationPlace).Returns("Hamburg");
			customsValueMock.Setup(c => c.CustomsValueAdditionDeductionDescription).Returns("Hinzurechnungen/Abzüge");
			customsValueMock.Setup(c => c.CustomsValueNetPrice).Returns(netPriceMock.Object);
			customsValueMock.Setup(c => c.CustomsValueIndirectPayment).Returns(indirectPaymentMock.Object);
			customsValueMock.Setup(c => c.CustomsValueAirFreightCosts).Returns(airFreightCostsMock.Object);
			customsValueMock.Setup(m => m.CustomsValueAdditionDeduction).Returns(new IAdditionDeduction[] { additionDeductionMock.Object });

			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("CT");
			package.Setup(p => p.Quantity).Returns(970);
			package.Setup(p => p.MarksNumbers).Returns("1-970");

			var importSpecificRate = new Mock<IImportSpecificRate>();
			importSpecificRate.Setup(r => r.Type).Returns("X");
			importSpecificRate.Setup(r => r.Value).Returns(10.02m);

			var contentInformation = new Mock<IContentInformation>();
			contentInformation.Setup(r => r.ContentType).Returns("X");
			contentInformation.Setup(r => r.DegreePercentage).Returns(0.01m);

			var exciseDuty = new Mock<IExciseDuty>();
			exciseDuty.Setup(d => d.Code).Returns("A123");
			exciseDuty.Setup(d => d.DegreePercentage).Returns(0.02m);
			exciseDuty.Setup(d => d.Value).Returns(1626.28m);
			exciseDuty.Setup(d => d.Amount).Returns(GetAmount(18219m, "NAR", "X").Object);

			var importSpecialCase = new Mock<IImportSpecialCase>();
			importSpecialCase.Setup(c => c.Group).Returns("A1");
			importSpecialCase.Setup(c => c.ApplicationType).Returns("A2");
			importSpecialCase.Setup(c => c.RateOrAmountOrFactor).Returns(10.25m);

			var importLineDocument = new Mock<IImportLineDocument>();
			importLineDocument.Setup(d => d.Division).Returns("4");
			importLineDocument.Setup(d => d.DocumentType).Returns("7HHF");
			importLineDocument.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			importLineDocument.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			importLineDocument.Setup(d => d.AtHandFlag).Returns("J");
			importLineDocument.Setup(m => m.WriteOff).Returns(GetAmount(1500m, "NAR", "Z").Object);

			goodsItemMock = new Mock<ICFCDECLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("4000");
			goodsItemMock.Setup(l => l.CessionManagementFlag).Returns("A1");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.InvoiceAmount).Returns(123877.15031m);
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.PreferentialOriginCountry).Returns("JP");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.TobaccoRevenueStampNumber).Returns("A1234");
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsQuantity).Returns(109513m);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.5m);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsAmount).Returns(GetAmount(18219, "NAR", "X").Object);
			goodsItemMock.Setup(l => l.CustomsValue).Returns(customsValueMock.Object);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.6m);
			goodsItemMock.Setup(l => l.AssessmentOutwardProcessingFee).Returns(37856.12m);
			goodsItemMock.Setup(l => l.AssessmentTaxCosts).Returns(1626.28m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { GetAmount(18219, "NAR", "X").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { importSpecificRate.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { contentInformation.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { exciseDuty.Object });
			goodsItemMock.Setup(l => l.PreferentialTreatment).Returns(preferentialTreatmentMock.Object);
			goodsItemMock.Setup(l => l.SpecialCases).Returns(new IImportSpecialCase[] { importSpecialCase.Object });
			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { importLineDocument.Object });
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();
			var dutyDefermentApprovals = new Mock<IDutyDefermentApproval>();
			dutyDefermentApprovals.Setup(dda => dda.Type).Returns("10");
			dutyDefermentApprovals.Setup(dda => dda.ApplicationType).Returns("E");
			dutyDefermentApprovals.Setup(dda => dda.AccountPrefix).Returns("F");
			dutyDefermentApprovals.Setup(dda => dda.AccountNumber).Returns("123456");
			dutyDefermentApprovals.Setup(dda => dda.AuthorisationNumber).Returns("12345BIN67890");
			dutyDefermentApprovals.Setup(dda => dda.Applicant).Returns("DE9000348");

			var dutyDefermentApproval = new Mock<IDutyDefermentApproval>();
			dutyDefermentApproval.Setup(dda => dda.Type).Returns("20");
			dutyDefermentApproval.Setup(dda => dda.ApplicationType).Returns("F");
			dutyDefermentApproval.Setup(dda => dda.AccountPrefix).Returns("F");
			dutyDefermentApproval.Setup(dda => dda.AccountNumber).Returns("123457");
			dutyDefermentApproval.Setup(dda => dda.AuthorisationNumber).Returns("92345BIN67890");
			dutyDefermentApproval.Setup(dda => dda.Applicant).Returns("DE9000349");

			var summaryDeclarationGoodsItem = new Mock<ISummaryDeclarationGoodsItem>();
			summaryDeclarationGoodsItem.Setup(x => x.Quantity).Returns(1);
			summaryDeclarationGoodsItem.Setup(x => x.IdentificationByKeyKind).Returns("AWB");
			summaryDeclarationGoodsItem.Setup(x => x.IdentificationByKeyNumber).Returns("1234567890");
			summaryDeclarationGoodsItem.Setup(x => x.IdentificationByKeyCustodianIdentifier).Returns("DE3333333");
			summaryDeclarationGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("ATA123456789123456789");
			summaryDeclarationGoodsItem.Setup(x => x.IdentificationByRegistrationReferencedSequenceNumber).Returns(1);

			summaryDeclarationMock = new Mock<ISummaryDeclaration>();
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			summaryDeclarationMock.Setup(x => x.GoodsItems).Returns(new ISummaryDeclarationGoodsItem[] { summaryDeclarationGoodsItem.Object });

			paymentTransactionMock = new Mock<IMoney>();
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.23m);
			paymentTransactionMock.Setup(m => m.CurrencyCode).Returns("INR");

			var declarantIdentification = new Mock<IPartyID>();
			declarantIdentification.Setup(id => id.EoriNumber).Returns("GR234567890");
			declarantIdentification.Setup(id => id.EoriBranchSuffix).Returns("0001");
			var declarant = new Mock<IImportParty>();
			declarant.Setup(v => v.Identification).Returns(declarantIdentification.Object);

			var representativeIdentification = new Mock<IPartyID>();
			representativeIdentification.Setup(id => id.EoriNumber).Returns("GR345678901");
			representativeIdentification.Setup(id => id.EoriBranchSuffix).Returns("0002");
			var representative = new Mock<IImportParty>();
			representative.Setup(v => v.Identification).Returns(representativeIdentification.Object);

			var principalIdentification = new Mock<IPartyID>();
			principalIdentification.Setup(id => id.EoriNumber).Returns("GR456789012");
			principalIdentification.Setup(id => id.EoriBranchSuffix).Returns("0003");
			var principal = new Mock<IImportParty>();
			principal.Setup(v => v.Identification).Returns(principalIdentification.Object);

			var consignorIdentification = new Mock<IPartyID>();
			consignorIdentification.Setup(id => id.EoriNumber).Returns("GR567890123");
			var consignor = new Mock<IImportParty>();
			consignor.Setup(v => v.Identification).Returns(consignorIdentification.Object);

			var consigneeIdentification = new Mock<IPartyID>();
			consigneeIdentification.Setup(id => id.EoriNumber).Returns("GR667890124");
			consigneeIdentification.Setup(id => id.EoriBranchSuffix).Returns("0005");
			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Identification).Returns(consigneeIdentification.Object);

			var contactPerson = new Mock<IImportPartyContactPerson>();
			contactPerson.Setup(v => v.MailAddress).Returns("bob.baumeister@samplefreight.com");
			contactPerson.Setup(v => v.PersonName).Returns("Bob Baumeister");
			contactPerson.Setup(v => v.PhoneNumber).Returns("06131-477447");
			contactPerson.Setup(v => v.Position).Returns("Sachbearbeiter");

			var vendeeIdentification = new Mock<IPartyID>();
			vendeeIdentification.Setup(id => id.EoriNumber).Returns("DE123456789");
			var vendee = new Mock<IImportParty>();
			vendee.Setup(v => v.Identification).Returns(vendeeIdentification.Object);

			var vendorAddress = new Mock<IImportPartyIdAddress>();
			vendorAddress.Setup(address => address.Address).Returns("Poststrasse 1");
			vendorAddress.Setup(address => address.City).Returns("Mainz");
			vendorAddress.Setup(address => address.Country).Returns("DE");
			vendorAddress.Setup(address => address.District).Returns("Finthen");
			vendorAddress.Setup(address => address.Name).Returns("Bob Baumeister");
			vendorAddress.Setup(address => address.Postcode).Returns("55126");

			var vendor = new Mock<IImportParty>();
			vendor.Setup(v => v.Address).Returns(vendorAddress.Object);

			var customsValue = new Mock<ICustomsValue>();
			customsValue.Setup(h => h.FormerDecisions).Returns("Former Decisions");
			customsValue.Setup(h => h.Vendee).Returns(vendee.Object);
			customsValue.Setup(h => h.Vendor).Returns(vendor.Object);
			customsValue.Setup(h => h.AffiliationType).Returns("X");
			customsValue.Setup(h => h.AffiliationDescription).Returns("AffiliationDescription");
			customsValue.Setup(h => h.RestrictionFlag).Returns(true);
			customsValue.Setup(h => h.ConditionFlag).Returns(false);
			customsValue.Setup(h => h.RestrictionOrConditionDescription).Returns("Restriction Description");
			customsValue.Setup(h => h.LicenseFeeFlag).Returns(true);
			customsValue.Setup(h => h.LicenseFeeDescription).Returns("LicenseDescription");
			customsValue.Setup(h => h.ResaleFlag).Returns(true);
			customsValue.Setup(h => h.ResaleDescription).Returns("ResaleDescription");

			var documentsIImportDocument1 = new Mock<IImportDocument>();
			documentsIImportDocument1.Setup(d => d.Type).Returns("N380");
			documentsIImportDocument1.Setup(d => d.ReferenceNumber).Returns("DOC1REFERENCE");
			documentsIImportDocument1.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 23));

			var documentsIImportDocument2 = new Mock<IImportDocument>();
			documentsIImportDocument2.Setup(d => d.Type).Returns("X123");
			documentsIImportDocument2.Setup(d => d.ReferenceNumber).Returns("DOC2REFERENCE");
			documentsIImportDocument2.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 24));

			var dutyInterestedPartyIdentification = new Mock<IPartyID>();
			dutyInterestedPartyIdentification.Setup(pi => pi.EoriNumber).Returns("CN00001");
			var dutyInterestedParty1 = new Mock<IImportParty>();
			dutyInterestedParty1.Setup(r => r.Identification).Returns(dutyInterestedPartyIdentification.Object);

			var dutyInterestedPartyAddress = new Mock<IImportPartyIdAddress>();
			dutyInterestedPartyAddress.Setup(ip => ip.Name).Returns("DutyName");
			dutyInterestedPartyAddress.Setup(ip => ip.Postcode).Returns("88888888");
			dutyInterestedPartyAddress.Setup(ip => ip.City).Returns("Nanjing");
			dutyInterestedPartyAddress.Setup(ip => ip.District).Returns("Xuanwu");
			dutyInterestedPartyAddress.Setup(ip => ip.Address).Returns("Zhongshan road");
			dutyInterestedPartyAddress.Setup(ip => ip.Country).Returns("CN");
			var dutyInterestedParty2 = new Mock<IImportParty>();
			dutyInterestedParty2.Setup(r => r.Address).Returns(dutyInterestedPartyAddress.Object);

			var additionalDutyReferences1 = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences1.Setup(duty => duty.ReferenceNumber).Returns("CFR00001");
			additionalDutyReferences1.Setup(duty => duty.DutyInterestedParty).Returns(dutyInterestedParty1.Object);

			var additionalDutyReferences2 = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences2.Setup(duty => duty.ReferenceNumber).Returns("CFR00002");
			additionalDutyReferences2.Setup(duty => duty.DutyInterestedParty).Returns(dutyInterestedParty2.Object);

			headerMock = new Mock<ICFCDECHeader>();
			headerMock.Setup(h => h.DeclarationKind).Returns("A");
			headerMock.Setup(h => h.DeclarationType).Returns("EZA");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.PrematureInputFlag).Returns(true);
			headerMock.Setup(h => h.GoodsItemQuantity).Returns(10);
			headerMock.Setup(h => h.CustomsGoodsStatus).Returns("IM");
			headerMock.Setup(h => h.DeclarantIsConsigneeFlag).Returns(true);
			headerMock.Setup(h => h.ProcedureAuthorisation).Returns("DE001234567");
			headerMock.Setup(h => h.InputTaxDeductionFlag).Returns(true);
			headerMock.Setup(h => h.GoodsLocation).Returns("locationOfGoods");
			headerMock.Setup(h => h.DepartureCountry).Returns("LV");
			headerMock.Setup(h => h.PaymentMethod).Returns("E");
			headerMock.Setup(h => h.CurrencyCode).Returns("USD");
			headerMock.Setup(h => h.AdditionalInformation).Returns("AdditionalInformation");
			headerMock.Setup(h => h.TaxOffice).Returns("DE001234");
			headerMock.Setup(h => h.RepresentativeRelationshipFlag).Returns("0");
			headerMock.Setup(h => h.DeclarationPlace).Returns("Hamburg");
			headerMock.Setup(h => h.DutyDefermentApprovals).Returns(new IDutyDefermentApproval[] { dutyDefermentApprovals.Object, dutyDefermentApproval.Object });
			headerMock.Setup(h => h.BorderTransportMeansMode).Returns("1");
			headerMock.Setup(h => h.BorderTransportMeansType).Returns("07");
			headerMock.Setup(h => h.BorderTransportMeansInformation).Returns("AA-BB123");
			headerMock.Setup(h => h.BorderTransportMeansNationality).Returns("FR");
			headerMock.Setup(h => h.ArrivalTransportMeansIdentity).Returns("XX-AA123");
			headerMock.Setup(h => h.PreviousAdministrativeReferenceType).Returns("ATNEU");
			headerMock.Setup(h => h.PreviousAdministrativeReferenceNumber).Returns("REFNUM12345");
			headerMock.Setup(h => h.SummaryDeclaration).Returns(summaryDeclarationMock.Object);
			headerMock.Setup(h => h.ContainerFlag).Returns("1");
			headerMock.Setup(h => h.ContainerIdentificationNumbers).Returns(new List<string> { "CONT1", "CONT2" });
			headerMock.Setup(h => h.DeliveryTermsCode).Returns("CFR");
			headerMock.Setup(h => h.DeliveryTermsDescription).Returns("Description");
			headerMock.Setup(h => h.DeliveryTermsPlace).Returns("Place");
			headerMock.Setup(h => h.DeliveryTermsKey).Returns("0");
			headerMock.Setup(h => h.PaymentTransaction).Returns(paymentTransactionMock.Object);
			headerMock.Setup(h => h.ForeignTradeStatisticsGoodsStatus).Returns("04");
			headerMock.Setup(h => h.ForeignTradeStatisticsTransactionType).Returns("X");
			headerMock.Setup(h => h.ForeignTradeStatisticsDestinationCountry).Returns("DE");
			headerMock.Setup(h => h.ForeignTradeStatisticsDestinationFederalState).Returns("06");
			headerMock.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("3");
			headerMock.Setup(h => h.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(123456789.1m);
			headerMock.Setup(h => h.EntryCustomsOfficeReferenceNumber).Returns("DE006665");
			headerMock.Setup(h => h.Declarant).Returns(declarant.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(h => h.Principal).Returns(principal.Object);
			headerMock.Setup(h => h.Consignor).Returns(consignor.Object);
			headerMock.Setup(h => h.Consignee).Returns(consignee.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(x => x.CustomsValue).Returns(customsValue.Object);
			headerMock.Setup(h => h.Documents).Returns(new IImportDocument[] { documentsIImportDocument1.Object, documentsIImportDocument2.Object });
			headerMock.Setup(h => h.Lines).Returns(new ICFCDECLine[] { goodsItemMock.Object });
			headerMock.Setup(h => h.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[] { additionalDutyReferences1.Object, additionalDutyReferences2.Object });
			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(i => i.EoriNumber).Returns("DE8999783");
			interchangeSender.Setup(i => i.EoriBranchSuffix).Returns("0000");

			messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(h => h.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(h => h.AuthorisationNumber).Returns("1234567890AUTH");
			messageHeaderMock.Setup(h => h.MessageGroup).Returns("ZBE");
			messageHeaderMock.Setup(h => h.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			cfcdecBuilder = CreateCFCDECBuilder();
		}

		CFCDECMessageBuilder CreateCFCDECBuilder()
		{
			return new CFCDECMessageBuilder(messageHeaderMock.Object);
		}

		CFCDECMessageBuilder cfcdecBuilder;
		Mock<ICFCDECHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ICustomsWarehouse> customsWarehouseMock;
		Mock<ICustomsWarehouseGoodsItem> customsWarehouseGoodsItemMock;
		Mock<IAmount> customsWarehouseGoodsItemCommercialAmountMock;
		Mock<IAmount> customsWarehouseGoodsItemDebitAmountMock;
		Mock<IInwardProcessing> inwardProcessingMock;
		Mock<IInwardProcessingGoodsItem> inwardProcessingGoodsItemMock;
		Mock<IMoney> paymentTransactionMock;
		Mock<ICFCDECLine> goodsItemMock;
		Mock<IImportLineCustomsValue> customsValueMock;
		Mock<ILinePreferentialTreatment> preferentialTreatmentMock;
		Mock<IImportCosts> netPriceMock;
		Mock<IImportCosts> indirectPaymentMock;
		Mock<IAirFreightCosts> airFreightCostsMock;
		Mock<IAdditionDeduction> additionDeductionMock;

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
