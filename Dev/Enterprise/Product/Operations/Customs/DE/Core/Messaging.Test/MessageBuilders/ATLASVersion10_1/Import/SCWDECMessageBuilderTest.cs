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
	sealed class SCWDECMessageBuilderTest : MessageBuilderTest<SCWDECMessageBuilder, LSCWDL>
	{
		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestMessageGroup_LVE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("LVE");
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(LSCWDLMetaDataMessageGroup.LVE));
		}

		[ExpectNoExceptions]
		public void TestDeclarant_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Address).Returns("Poststrasse 1");
			address.Setup(m => m.City).Returns("Mainz");
			address.Setup(m => m.Country).Returns("DE");
			address.Setup(m => m.District).Returns("Finthen");
			address.Setup(m => m.Name).Returns("Bob Baumeister");
			address.Setup(m => m.Postcode).Returns("55126");
			var declarant = new Mock<IImportParty>();
			declarant.Setup(v => v.Address).Returns(address.Object);

			headerMock.Setup(m => m.Declarant).Returns(declarant.Object);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant.Identification, Is.EqualTo(default(LSCWDLDeclarantIdentification)));
			NUnit.Framework.Assert.That(message.Declarant.Address, Is.Not.EqualTo(default(LSCWDLDeclarantAddress)));
			NUnit.Framework.Assert.That(message.Declarant.Name, Is.EqualTo("Bob Baumeister"));

			var addressExpected = message.Declarant.Address;
			NUnit.Framework.Assert.That(addressExpected, Is.Not.EqualTo(default(LSCWDLDeclarantAddress)));
			NUnit.Framework.Assert.That(addressExpected.Line, Is.EqualTo("Poststrasse 1"));
			NUnit.Framework.Assert.That(addressExpected.City, Is.EqualTo("Mainz"));
			NUnit.Framework.Assert.That(addressExpected.Country, Is.EqualTo("DE"));
			NUnit.Framework.Assert.That(addressExpected.District, Is.EqualTo("Finthen"));
			NUnit.Framework.Assert.That(addressExpected.Postcode, Is.EqualTo("55126"));
		}

		[ExpectNoExceptions]
		public void TestRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IImportParty)null);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(LSCWDLRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestRepresentative_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var representative = new Mock<IImportParty>();
			representative.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			headerMock.Setup(m => m.Representative).Returns(representative.Object);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(LSCWDLRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPrincipal_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Address).Returns("Poststrasse 1");
			address.Setup(m => m.City).Returns("Mainz");
			address.Setup(m => m.Country).Returns("DE");
			address.Setup(m => m.District).Returns("Finthen");
			address.Setup(m => m.Name).Returns("Bob Baumeister");
			address.Setup(m => m.Postcode).Returns("55126");
			var principal = new Mock<IImportParty>();
			principal.Setup(v => v.Address).Returns(address.Object);

			headerMock.Setup(m => m.Principal).Returns(principal.Object);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Principal.Identification, Is.EqualTo(default(LSCWDLPrincipalIdentification)));
			NUnit.Framework.Assert.That(message.Principal.Address, Is.Not.EqualTo(default(LSCWDLPrincipalAddress)));
			NUnit.Framework.Assert.That(message.Principal.Name, Is.EqualTo("Bob Baumeister"));

			var addressExpected = message.Principal.Address;
			NUnit.Framework.Assert.That(addressExpected, Is.Not.EqualTo(default(LSCWDLPrincipalAddress)));
			NUnit.Framework.Assert.That(addressExpected.Line, Is.EqualTo("Poststrasse 1"));
			NUnit.Framework.Assert.That(addressExpected.City, Is.EqualTo("Mainz"));
			NUnit.Framework.Assert.That(addressExpected.Country, Is.EqualTo("DE"));
			NUnit.Framework.Assert.That(addressExpected.District, Is.EqualTo("Finthen"));
			NUnit.Framework.Assert.That(addressExpected.Postcode, Is.EqualTo("55126"));
		}

		[ExpectNoExceptions]
		public void TestContactPerson_Null()
		{
			headerMock.Setup(m => m.ContactPerson).Returns((IImportPartyContactPerson)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().ContactPerson, Is.EqualTo(default(LSCWDLContactPerson)));
		}

		[ExpectNoExceptions]
		public void TestPreviousAdministrativeReferences_TypeT1()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(LSCWDLPreviousAdministrativeReferencesType.T1));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference.ReferenceNumber, Is.EqualTo("REFNUM12345"));
		}

		[ExpectNoExceptions]
		public void TestPreviousAdministrativeReferences_NumberIsEmpty()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(string.Empty);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(LSCWDLPreviousAdministrativeReferencesType.ATNEU));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(LSCWDLPreviousAdministrativeReferencesPreviousAdministrativeReference)));
		}

		[ExpectNoExceptions]
		public void TestSummaryDeclaration_Null()
		{
			headerMock.Setup(m => m.SummaryDeclaration).Returns((ISummaryDeclaration)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().SummaryDeclaration, Is.EqualTo(default(LSCWDLSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestSummaryDeclaration_IdentificationByKey_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByKey.Kind, Is.EqualTo(LSCWDLSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD));
		}

		[ExpectNoExceptions]
		public void TestSummaryDeclaration_IdentificationByRegistration()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedSequenceNumber, Is.EqualTo("1"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.Not.EqualTo(default(LSCWDLCustomsWarehouse)), "Contains CustomsWarehouse - should not be [null]");

			NUnit.Framework.Assert.That(message.CustomsWarehouse.SequenceNumber, Is.EqualTo("1"));
			NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo("WTG5678"));
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItemQuantity, Is.EqualTo("10"));
			NUnit.Framework.Assert.That(message.CustomsWarehouse.CustomsAuthorisation.WarehouseOwner, Is.EqualTo("DE44444444"));
			var goodItem = message.CustomsWarehouse.GoodsItem[0];
			NUnit.Framework.Assert.That(goodItem, Is.Not.EqualTo(default(LSCWDLCustomsWarehouseGoodsItem)));
			NUnit.Framework.Assert.That(goodItem.SequenceNumber, Is.EqualTo("1"));
			NUnit.Framework.Assert.That(goodItem.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
			NUnit.Framework.Assert.That(goodItem.AccessViaAtlasFlag, Is.EqualTo("J"));
			NUnit.Framework.Assert.That(goodItem.CommodityCode, Is.EqualTo("12345678"));
			NUnit.Framework.Assert.That(goodItem.UsualProcessingFlag, Is.EqualTo("J"));
			NUnit.Framework.Assert.That(goodItem.Complement, Is.EqualTo("Description text"));
			NUnit.Framework.Assert.That(goodItem.CommercialAmount.Quantity, Is.EqualTo(1.234m));
			NUnit.Framework.Assert.That(goodItem.CommercialAmount.MeasurementUnit, Is.EqualTo("KGM"));
			NUnit.Framework.Assert.That(goodItem.CommercialAmount.Qualifier, Is.EqualTo("A"));
			NUnit.Framework.Assert.That(goodItem.DebitAmount.Quantity, Is.EqualTo(5.678m));
			NUnit.Framework.Assert.That(goodItem.DebitAmount.MeasurementUnit, Is.EqualTo("KGM"));
			NUnit.Framework.Assert.That(goodItem.DebitAmount.Qualifier, Is.EqualTo("M"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_LocalReferenceNumber_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.LRN, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_AccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.AccessViaATLASFlag).Returns(false);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_UsualProcessingFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.UsualProcessingFlag).Returns(false);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].UsualProcessingFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_Complement_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(string.Empty);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_CommercialAmount_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(m => m.CommercialAmount).Returns((IAmount)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.EqualTo(default(LSCWDLCustomsWarehouseGoodsItemCommercialAmount)));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_CommercialAmount_Qualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_CommercialAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_CommercialAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_DebitAmount_Qualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_DebitAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestCustomsWarehouse_GoodsItem_DebitAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestInwardProcessing()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing, Is.Not.EqualTo(default(LSCWDLInwardProcessing)), "Contains InwardProcessing - should not be [null]");

			NUnit.Framework.Assert.That(message.InwardProcessing.SequenceNumber, Is.EqualTo("1"));
			NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItemQuantity, Is.EqualTo("20"));
			NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"));
			NUnit.Framework.Assert.That(message.InwardProcessing.SimplifiedGrantAuthorisationFlag, Is.EqualTo("J"));
			NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"));
			var goodItem = message.InwardProcessing.GoodsItem[0];
			NUnit.Framework.Assert.That(goodItem.SequenceNumber, Is.EqualTo("1"));
			NUnit.Framework.Assert.That(goodItem.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
			NUnit.Framework.Assert.That(goodItem.ReferencedSequenceNumber, Is.EqualTo("1"));
			NUnit.Framework.Assert.That(goodItem.AccessViaAtlasFlag, Is.EqualTo("J"));
			NUnit.Framework.Assert.That(goodItem.GoodsRelatedInformation, Is.EqualTo("Related information"));
		}

		[ExpectNoExceptions]
		public void TestInwardProcessing_CustomsAuthorisation_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(string.Empty);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().InwardProcessing.CustomsAuthorisation, Is.EqualTo(default(LSCWDLInwardProcessingCustomsAuthorisation)), "Doesn't contain CustomsAuthorisation - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestInwardProcessing_MonitoringCustomsOffice_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingMock.Setup(m => m.SimplifiedGrantAuthorisationFlag).Returns(false);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing.SimplifiedGrantAuthorisationFlag, Is.EqualTo("N"));
			NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.EqualTo(default(LSCWDLInwardProcessingMonitoringCustomsOffice)), "Doesn't contain MonitoringCustomsOffice - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestInwardProcessing_GoodsItem_AccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingGoodsItemMock.Setup(m => m.AccessViaAtlasFlag).Returns(false);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestBody_Consignee_Null()
		{
			headerMock.Setup(m => m.Consignee).Returns((IImportParty)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.Consignee, Is.EqualTo(default(LSCWDLBodyConsignee)));
		}

		[ExpectNoExceptions]
		public void TestBody_Consignee_Address()
		{
			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Address).Returns("Consignee Address");
			address.Setup(m => m.City).Returns("City1");
			address.Setup(m => m.Country).Returns("DE");
			address.Setup(m => m.District).Returns("District1");
			address.Setup(m => m.Name).Returns("Name1");
			address.Setup(m => m.Postcode).Returns("11111");

			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Address).Returns(address.Object);

			headerMock.Setup(m => m.Consignee).Returns(consignee.Object);
			var message = scwdecMessageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.Consignee.Name, Is.EqualTo("Name1"));
			var addressExpected = message.Body.Consignee.Address;
			NUnit.Framework.Assert.That(addressExpected, Is.Not.EqualTo(default(LSCWDLBodyConsigneeAddress)));
			NUnit.Framework.Assert.That(addressExpected.Line, Is.EqualTo("Consignee Address"));
			NUnit.Framework.Assert.That(addressExpected.City, Is.EqualTo("City1"));
			NUnit.Framework.Assert.That(addressExpected.Country, Is.EqualTo("DE"));
			NUnit.Framework.Assert.That(addressExpected.District, Is.EqualTo("District1"));
			NUnit.Framework.Assert.That(addressExpected.Postcode, Is.EqualTo("11111"));
		}

		[ExpectNoExceptions]
		public void TestBody_Consignee_Address_Null()
		{
			headerMock.Setup(m => m.Consignee).Returns(new Mock<IImportParty>().Object);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.Consignee.Address, Is.EqualTo(default(LSCWDLBodyConsigneeAddress)));
		}

		[ExpectNoExceptions]
		public void TesttBody_PaymentTransaction_Round()
		{
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.23333m);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.PaymentTransaction.Amount, Is.EqualTo(12345678901.23m));
		}

		[ExpectNoExceptions]
		public void TestBody_PaymentTransaction_Normalize()
		{
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.2000m);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.PaymentTransaction.Amount, Is.EqualTo(12345678901.2m));
		}

		[ExpectNoExceptions]
		public void TestBody_ForeignTradeStatistics_TotalGrossMassMeasure_NotEmitWhen0()
		{
			headerMock.Setup(m => m.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(0m);
			NUnit.Framework.Assert.That(!scwdecMessageBuilder.GenerateMessage().Body.ForeignTradeStatistics.TotalGrossMassMeasureSpecified, Is.True, "<TotalGrossMassMeasure> not contained");
		}

		[ExpectNoExceptions]
		public void TestLine_AdditionalProcedure()
		{
			goodsItemMock.Setup(m => m.AdditionalProcedure).Returns(new[] { GetLongString("C", 9) });
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].AdditionalProcedure[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestLine_SupplementaryCodes()
		{
			goodsItemMock.Setup(m => m.SupplementaryCodes).Returns(new[] { GetLongString("C", 9) });
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].SupplementaryCodes[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestLine_Package_NoPackages()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].Package, Is.EqualTo(default(LSCWDLBodyGoodsItemPackage)));
		}

		[ExpectNoExceptions]
		public void TestLine_CustomsValue_IndirectPayment_CurrencyIsEUR()
		{
			indirectPaymentMock.Setup(m => m.CurrencyCode).Returns("EUR");
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.IndirectPayment.CurrencyCode, Is.EqualTo("EUR"));
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.IndirectPayment.CurrencyRateAgreedFlag, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLine_CustomsValue_AirFreightCosts_Empty()
		{
			customsValueMock.Setup(m => m.CustomsValueAirFreightCosts).Returns((IAirFreightCosts)null);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.AirFreightCosts, Is.EqualTo(default(LSCWDLBodyGoodsItemCustomsValueAirFreightCosts)));
		}

		[ExpectNoExceptions]
		public void TestLine_CustomsValue_AirFreightCosts_CurrencyIsEUR()
		{
			airFreightCostsMock.Setup(m => m.CurrencyCode).Returns("EUR");
			var airFreightCosts = scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.AirFreightCosts;
			NUnit.Framework.Assert.That(airFreightCosts.CurrencyCode, Is.EqualTo("EUR"));
			NUnit.Framework.Assert.That(airFreightCosts.CurrencyRateIATA, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(airFreightCosts.CurrencyRateAgreedFlag, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(airFreightCosts.CurrencyRateSpecified, Is.EqualTo(false));
			NUnit.Framework.Assert.That(airFreightCosts.CurrencyRateDateSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestLine_CustomsValue_AdditionDeduction_CurrencyIsEUR()
		{
			additionDeductionMock.Setup(m => m.CurrencyCode).Returns("EUR");
			var additionDeduction = scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.AdditionDeduction[0];
			NUnit.Framework.Assert.That(additionDeduction.CurrencyCode, Is.EqualTo("EUR"));
			NUnit.Framework.Assert.That(additionDeduction.CurrencyRateIATA, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(additionDeduction.CurrencyRateAgreedFlag, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(additionDeduction.CurrencyRateSpecified, Is.EqualTo(false));
			NUnit.Framework.Assert.That(additionDeduction.CurrencyRateDateSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestLine_CustomsValue_AdditionDeduction_NoPercentage()
		{
			additionDeductionMock.Setup(m => m.Percentage).Returns(decimal.Zero);
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].CustomsValue.AdditionDeduction[0].PercentageSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestLine_Document_WriteOff_Empty()
		{
			var documents = new Mock<IImportLineDocument>();
			documents.Setup(d => d.Division).Returns("4");
			documents.Setup(d => d.DocumentType).Returns("7HHF");
			documents.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents.Setup(d => d.AtHandFlag).Returns("J");
			documents.Setup(m => m.WriteOff).Returns((IAmount)null);

			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });

			var document = scwdecMessageBuilder.GenerateMessage().Body.GoodsItem[0].Document[0];
			NUnit.Framework.Assert.That(document, Is.Not.EqualTo(default(LSCWDLBodyGoodsItemDocument)));
			NUnit.Framework.Assert.That(document.WriteOff, Is.EqualTo(default(LSCWDLBodyGoodsItemDocumentWriteOff)));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var goodsItemMock = Mock.Get(headerMock.Object.CustomsWarehouse.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scwdecMessageBuilder.GenerateMessage();

			var goodsItem = message.CustomsWarehouse.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var goodsItemMock = Mock.Get(headerMock.Object.InwardProcessing.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scwdecMessageBuilder.GenerateMessage();

			var goodsItem = message.InwardProcessing.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_GoodsItem_MRN()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			var goodsItemMock = Mock.Get(headerMock.Object.SummaryDeclaration.GoodsItems.Single());
			goodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scwdecMessageBuilder.GenerateMessage();

			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[TestDate(2020, 10, 22, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(scwdecMessageBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestSCWDECMessage.txt")));
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

			var assessmentSpecificRate = new Mock<IImportSpecificRate>();
			assessmentSpecificRate.Setup(r => r.Type).Returns("X");
			assessmentSpecificRate.Setup(r => r.Value).Returns(10.02m);

			var assessmentContentInformation = new Mock<IContentInformation>();
			assessmentContentInformation.Setup(r => r.ContentType).Returns("X");
			assessmentContentInformation.Setup(r => r.DegreePercentage).Returns(0.01m);

			var exciseDuty = new Mock<IExciseDuty>();
			exciseDuty.Setup(d => d.Code).Returns("A123");
			exciseDuty.Setup(d => d.DegreePercentage).Returns(0.02m);
			exciseDuty.Setup(d => d.Value).Returns(1626.28m);
			exciseDuty.Setup(d => d.Amount).Returns(GetAmount(18219, "NAR", "X").Object);

			var documents = new Mock<IImportLineDocument>();
			documents.Setup(d => d.Division).Returns("4");
			documents.Setup(d => d.DocumentType).Returns("7HHF");
			documents.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents.Setup(d => d.AtHandFlag).Returns("J");
			documents.Setup(m => m.WriteOff).Returns(GetAmount(1500, "NAR", "Z").Object);

			goodsItemMock = new Mock<ISCWDECLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("4000");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.ArticleNumber).Returns("Num001");
			goodsItemMock.Setup(l => l.InvoiceAmount).Returns(123877.15031m);
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsQuantity).Returns(109513m);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.5m);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsAmount).Returns(GetAmount(18219, "NAR", "X").Object);
			goodsItemMock.Setup(l => l.CustomsValue).Returns(customsValueMock.Object);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.6m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { GetAmount(18219, "NAR", "X").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { assessmentSpecificRate.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { assessmentContentInformation.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { exciseDuty.Object });
			goodsItemMock.Setup(l => l.InwardMovementAmount).Returns(GetAmount(1400.12045m, "NAR", "I").Object);
			goodsItemMock.Setup(l => l.RequestedPreferentialTreatment).Returns("200");
			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();
			var goodsItems = new Mock<ISummaryDeclarationGoodsItem>();
			goodsItems.Setup(x => x.Quantity).Returns(1);
			goodsItems.Setup(x => x.IdentificationByKeyKind).Returns("AWB");
			goodsItems.Setup(x => x.IdentificationByKeyNumber).Returns("1234567890");
			goodsItems.Setup(x => x.IdentificationByKeyCustodianIdentifier).Returns("DE3333333");
			goodsItems.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("ATA123456789123456789");
			goodsItems.Setup(x => x.IdentificationByRegistrationReferencedSequenceNumber).Returns(1);

			summaryDeclarationMock = new Mock<ISummaryDeclaration>();
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			summaryDeclarationMock.Setup(x => x.GoodsItems).Returns(new ISummaryDeclarationGoodsItem[] { goodsItems.Object });

			paymentTransactionMock = new Mock<IMoney>();
			paymentTransactionMock.Setup(m => m.Value).Returns(12345678901.23m);
			paymentTransactionMock.Setup(m => m.CurrencyCode).Returns("INR");

			var identification1 = new Mock<IPartyID>();
			identification1.Setup(id => id.EoriNumber).Returns("GR234567890");
			identification1.Setup(id => id.EoriBranchSuffix).Returns("0001");
			var declarant = new Mock<IImportParty>();
			declarant.Setup(v => v.Identification).Returns(identification1.Object);

			var identification2 = new Mock<IPartyID>();
			identification2.Setup(id => id.EoriNumber).Returns("GR345678901");
			identification2.Setup(id => id.EoriBranchSuffix).Returns("0002");
			var representative = new Mock<IImportParty>();
			representative.Setup(v => v.Identification).Returns(identification2.Object);

			var identification3 = new Mock<IPartyID>();
			identification3.Setup(id => id.EoriNumber).Returns("GR456789012");
			identification3.Setup(id => id.EoriBranchSuffix).Returns("0003");
			var principal = new Mock<IImportParty>();
			principal.Setup(v => v.Identification).Returns(identification3.Object);

			var identification5 = new Mock<IPartyID>();
			identification5.Setup(id => id.EoriNumber).Returns("GR667890124");
			identification5.Setup(id => id.EoriBranchSuffix).Returns("0005");
			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Identification).Returns(identification5.Object);

			var contactPerson = new Mock<IImportPartyContactPerson>();
			contactPerson.Setup(v => v.MailAddress).Returns("bob.baumeister@samplefreight.com");
			contactPerson.Setup(v => v.PersonName).Returns("Bob Baumeister");
			contactPerson.Setup(v => v.PhoneNumber).Returns("06131-477447");
			contactPerson.Setup(v => v.Position).Returns("Sachbearbeiter");

			var identification6 = new Mock<IPartyID>();
			identification6.Setup(id => id.EoriNumber).Returns("DE123456789");
			var vendee = new Mock<IImportParty>();
			vendee.Setup(v => v.Identification).Returns(identification6.Object);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Address).Returns("Poststrasse 1");
			address.Setup(m => m.City).Returns("Mainz");
			address.Setup(m => m.Country).Returns("DE");
			address.Setup(m => m.District).Returns("Finthen");
			address.Setup(m => m.Name).Returns("Bob Baumeister");
			address.Setup(m => m.Postcode).Returns("55126");
			var vendor = new Mock<IImportParty>();
			vendor.Setup(v => v.Address).Returns(address.Object);

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

			var documents1 = new Mock<IImportDocument>();
			documents1.Setup(d => d.Type).Returns("N380");
			documents1.Setup(d => d.ReferenceNumber).Returns("DOC1REFERENCE");
			documents1.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 23));

			var documents2 = new Mock<IImportDocument>();
			documents2.Setup(d => d.Type).Returns("X123");
			documents2.Setup(d => d.ReferenceNumber).Returns("DOC2REFERENCE");
			documents2.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 24));

			headerMock = new Mock<ISCWDECHeader>();
			headerMock.Setup(h => h.DeclarationKind).Returns("A");
			headerMock.Setup(h => h.DeclarationType).Returns("EZA");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.ForeignTradeImportEarlyClearanceFlag).Returns(true);
			headerMock.Setup(h => h.PrematureInputFlag).Returns(true);
			headerMock.Setup(h => h.GoodsItemQuantity).Returns(10);
			headerMock.Setup(h => h.CustomsGoodsStatus).Returns("IM");
			headerMock.Setup(h => h.DeclarantIsConsigneeFlag).Returns(true);
			headerMock.Setup(h => h.ProcedureAuthorisation).Returns("DE001234567");
			headerMock.Setup(h => h.GoodsLocation).Returns("locationOfGoods");
			headerMock.Setup(h => h.DepartureCountry).Returns("LV");
			headerMock.Setup(h => h.CurrencyCode).Returns("USD");
			headerMock.Setup(h => h.AdditionalInformation).Returns("AdditionalInformation");
			headerMock.Setup(h => h.RepresentativeRelationshipFlag).Returns("0");
			headerMock.Setup(h => h.DeclarationPlace).Returns("Hamburg");
			headerMock.Setup(h => h.BorderTransportMeansMode).Returns("1");
			headerMock.Setup(h => h.BorderTransportMeansType).Returns("07");
			headerMock.Setup(h => h.BorderTransportMeansInformation).Returns("AA-BB123");
			headerMock.Setup(h => h.BorderTransportMeansNationality).Returns("FR");
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
			headerMock.Setup(h => h.ForeignTradeStatisticsDestinationCountry).Returns("DE");
			headerMock.Setup(h => h.ForeignTradeStatisticsDestinationFederalState).Returns("06");
			headerMock.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("3");
			headerMock.Setup(h => h.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(123456789.1m);
			headerMock.Setup(h => h.EntryCustomsOfficeReferenceNumber).Returns("DE006665");
			headerMock.Setup(h => h.Declarant).Returns(declarant.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(h => h.Principal).Returns(principal.Object);
			headerMock.Setup(h => h.Consignee).Returns(consignee.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(x => x.CustomsValue).Returns(customsValue.Object);
			headerMock.Setup(h => h.Documents).Returns(new IImportDocument[] { documents1.Object, documents2.Object });
			headerMock.Setup(m => m.Lines).Returns(new ISCWDECLine[] { goodsItemMock.Object });

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(i => i.EoriNumber).Returns("DE8999783");
			interchangeSender.Setup(i => i.EoriBranchSuffix).Returns("0000");

			messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(h => h.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(h => h.AuthorisationNumber).Returns("1234567890AUTH");
			messageHeaderMock.Setup(h => h.MessageGroup).Returns("LAE");
			messageHeaderMock.Setup(h => h.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			scwdecMessageBuilder = new SCWDECMessageBuilder(messageHeaderMock.Object);
		}
		SCWDECMessageBuilder scwdecMessageBuilder;
		Mock<ISCWDECHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ICustomsWarehouse> customsWarehouseMock;
		Mock<ICustomsWarehouseGoodsItem> customsWarehouseGoodsItemMock;
		Mock<IAmount> customsWarehouseGoodsItemCommercialAmountMock;
		Mock<IAmount> customsWarehouseGoodsItemDebitAmountMock;
		Mock<IInwardProcessing> inwardProcessingMock;
		Mock<IInwardProcessingGoodsItem> inwardProcessingGoodsItemMock;
		Mock<IMoney> paymentTransactionMock;
		Mock<ISCWDECLine> goodsItemMock;
		Mock<IImportLineCustomsValue> customsValueMock;
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

			headerMock.Setup(m => m.InwardProcessing).Returns(inwardProcessingMock.Object);
		}
	}
}
