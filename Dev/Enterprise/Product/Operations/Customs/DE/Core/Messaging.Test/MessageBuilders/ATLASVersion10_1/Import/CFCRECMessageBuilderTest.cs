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
	sealed class CFCRECMessageBuilderTest : MessageBuilderTest<CFCRECMessageBuilder, FCFCRF>
	{
		[TestDate(2021, 05, 01, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(cfcrecBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestCFCRECMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(cfcrecBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestLocalClearanceDate()
		{
			headerMock.Setup(x => x.LocalClearanceDate).Returns((DateTime?)null);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.LocalClearanceDate, Is.EqualTo(DateTime.MinValue), "LocalClearanceDate is null");
		}

		[ExpectNoExceptions]
		public void TestLocalClearanceDateSpecified()
		{
			CombineAssertions(() =>
			{
				headerMock.Setup(x => x.DeclarationType).Returns("VZA");

				var message = cfcrecBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Header.LocalClearanceDateSpecified, Is.EqualTo(false), "DeclarationType isn't 'AZ'");

				headerMock.Setup(x => x.DeclarationType).Returns("AZ");
				headerMock.Setup(x => x.LocalClearanceDate).Returns((DateTime?)null);

				message = cfcrecBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Header.LocalClearanceDateSpecified, Is.EqualTo(false), "LocalClearanceDate is null");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarant_Null()
		{
			headerMock.Setup(m => m.Declarant).Returns((IImportParty)null);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant, Is.EqualTo(default(FCFCRFDeclarant)));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IImportParty)null);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(FCFCRFRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var representative = new Mock<IImportParty>();
			representative.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			headerMock.Setup(m => m.Representative).Returns(representative.Object);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(FCFCRFRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePrincipal_Null()
		{
			headerMock.Setup(m => m.Principal).Returns((IImportParty)null);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Principal, Is.EqualTo(default(FCFCRFPrincipal)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePrincipal_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(a => a.Name).Returns("WTG");
			address.Setup(a => a.Address).Returns("Mainzerstr. 20");
			address.Setup(a => a.City).Returns("Wiesbaden");
			address.Setup(a => a.Postcode).Returns("52211");
			address.Setup(a => a.District).Returns("Hessen");

			var principal = new Mock<IImportParty>();
			principal.Setup(m => m.Identification).Returns(partyWithNoEori.Object);
			principal.Setup(m => m.Address).Returns(address.Object);

			headerMock.Setup(duty => duty.Principal).Returns(principal.Object);
			var message = cfcrecBuilder.GenerateMessage();
			var principalParty = message.Principal;
			NUnit.Framework.Assert.That(principalParty.Name, Is.EqualTo("WTG"), "Principal Name");
			NUnit.Framework.Assert.That(principalParty.Address.Line, Is.EqualTo("Mainzerstr. 20"), "Principal Address");
			NUnit.Framework.Assert.That(principalParty.Address.City, Is.EqualTo("Wiesbaden"), "Principal City");
			NUnit.Framework.Assert.That(principalParty.Address.Postcode, Is.EqualTo("52211"), "Principal Postcode");
			NUnit.Framework.Assert.That(principalParty.Address.District, Is.EqualTo("Hessen"), "Principal District");
		}

		[ExpectNoExceptions]
		public void TestPopulateContactPerson_Null()
		{
			headerMock.Setup(m => m.ContactPerson).Returns((IImportPartyContactPerson)null);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ContactPerson, Is.EqualTo(default(FCFCRFContactPerson)));
		}

		[ExpectNoExceptions]
		public void TestPopulateBorderTransportMeans_TruncatedInformation()
		{
			headerMock.Setup(m => m.BorderTransportMeansInformation).Returns(string.Empty.PadLeft(18, 'A'));

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.BorderTransportMeans.Information, Is.EqualTo(string.Empty.PadLeft(17, 'A')));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeT1()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(FCFCRFPreviousAdministrativeReferencesType.T1));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference.ReferenceNumber, Is.EqualTo("REFNUM12345"));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReferences_NotPopulated_PreviousAdministrativeReferenceNumberEmpty()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
			headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(string.Empty);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(FCFCRFPreviousAdministrativeReferencesType.T1));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(FCFCRFPreviousAdministrativeReferencesPreviousAdministrativeReference)));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_Null()
		{
			headerMock.Setup(m => m.SummaryDeclaration).Returns((ISummaryDeclaration)null);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCRFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_IdentificationIndicator_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(FCFCRFSummaryDeclarationIdentificationIndicator.AWB), "ULD");
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_IdentificationIndicator_REG()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(FCFCRFSummaryDeclarationIdentificationIndicator.REG));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsItem_IdentificationByKey_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");

			var message = cfcrecBuilder.GenerateMessage();
			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			NUnit.Framework.Assert.That(goodsItem.IdentificationByKey.Kind, Is.EqualTo(FCFCRFSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD));
			NUnit.Framework.Assert.That(goodsItem.IdentificationByKey.Number, Is.EqualTo("1234567890"));
			NUnit.Framework.Assert.That(goodsItem.IdentificationByKey.Custodian.Identification.ReferenceNumber, Is.EqualTo("DE3333333"));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsItem_IdentificationByRegistration()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");

			var message = cfcrecBuilder.GenerateMessage();
			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
			NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedSequenceNumber, Is.EqualTo("1"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_PreviousAdministrativeReferenceTypeATZL()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var message = cfcrecBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCRFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.Not.EqualTo(default(FCFCRFCustomsWarehouse)), "Contains CustomsWarehouse - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(FCFCRFInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");

				var goodsItem = message.CustomsWarehouse.GoodsItem[0];
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo("WTG5678"));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.SequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItemQuantity, Is.EqualTo("10"));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.CustomsAuthorisation.WarehouseOwner, Is.EqualTo("DE44444444"));
				NUnit.Framework.Assert.That(goodsItem.SequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
				NUnit.Framework.Assert.That(goodsItem.ReferencedSequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(goodsItem.AccessViaAtlasFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(goodsItem.CommodityCode, Is.EqualTo("12345678"));
				NUnit.Framework.Assert.That(goodsItem.UsualProcessingFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(goodsItem.Complement, Is.EqualTo("Description text"));
				NUnit.Framework.Assert.That(goodsItem.CommercialAmount.Quantity, Is.EqualTo(1.234m));
				NUnit.Framework.Assert.That(goodsItem.CommercialAmount.MeasurementUnit, Is.EqualTo("KGM"));
				NUnit.Framework.Assert.That(goodsItem.CommercialAmount.Qualifier, Is.EqualTo("A"));
				NUnit.Framework.Assert.That(goodsItem.DebitAmount.Quantity, Is.EqualTo(5.678m));
				NUnit.Framework.Assert.That(goodsItem.DebitAmount.MeasurementUnit, Is.EqualTo("KGM"));
				NUnit.Framework.Assert.That(goodsItem.DebitAmount.Qualifier, Is.EqualTo("M"));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_LocalReferenceNumber_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo(default(string)), "Doesn't contain LocalReferenceNumber - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.AccessViaATLASFlag).Returns(false);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItemUsualProcessingFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.UsualProcessingFlag).Returns(false);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].UsualProcessingFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItemComplement_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(string.Empty);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(default(string)), "Doesn't contain Complement - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.CommercialAmount).Returns((IAmount)null);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.EqualTo(default(FCFCRFCustomsWarehouseGoodsItemCommercialAmount)), "Doesn't contain CommercialAmount - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateCFCRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"), "Contains Qualifier");
				customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateCFCRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateCFCRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo("M"), "Contains Qualifier");
				customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateCFCRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_PreviousAdministrativeReferenceTypeATAV()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var message = cfcrecBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(FCFCRFSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(FCFCRFCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.Not.EqualTo(default(FCFCRFInwardProcessing)), "Contains InwardProcessing - should not be [null]");

				NUnit.Framework.Assert.That(message.InwardProcessing.SequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItemQuantity, Is.EqualTo("20"));
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"));
				NUnit.Framework.Assert.That(message.InwardProcessing.SimplifiedGrantAuthorisationFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"));

				var goodsItem = message.InwardProcessing.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
				NUnit.Framework.Assert.That(goodsItem.ReferencedSequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(goodsItem.AccessViaAtlasFlag, Is.EqualTo("J"));
				NUnit.Framework.Assert.That(goodsItem.GoodsRelatedInformation, Is.EqualTo("Related information"));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_CustomsAuthorisation_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(string.Empty);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation, Is.EqualTo(default(FCFCRFInwardProcessingCustomsAuthorisation)), "Doesn't contain CustomsAuthorisation - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_MonitoringCustomsOffice_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			inwardProcessingMock.Setup(m => m.SimplifiedGrantAuthorisationFlag).Returns(false);
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.EqualTo(default(FCFCRFInwardProcessingMonitoringCustomsOffice)), "Doesn't contain MonitoringCustomsOffice - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_GoodsItemAccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingGoodsItemMock.Setup(m => m.AccessViaAtlasFlag).Returns(false);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalDutyReferences_NotPopulated()
		{
			headerMock.Setup(m => m.AdditionalDutyReferences).Returns((IReadOnlyCollection<IImportAdditionalDutyReference>)Enumerable.Empty<IImportAdditionalDutyReference>());
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences, Is.EqualTo(default(FCFCRFBodyAdditionalDutyReferences[])));
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalDutyReferences_NoDutyInterestedParty()
		{
			var additionalDutyReferences = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences.Setup(duty => duty.ReferenceNumber).Returns("CFR00001");
			additionalDutyReferences.Setup(duty => duty.DutyInterestedParty).Returns((IImportParty)null);

			headerMock.Setup(h => h.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[] { additionalDutyReferences.Object });
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences[0].ReferenceNumber, Is.EqualTo("CFR00001"), "ReferenceNumber");
			NUnit.Framework.Assert.That(message.Body.AdditionalDutyReferences[0].DutyInterestedParty, Is.EqualTo(default(FCFCRFBodyAdditionalDutyReferencesDutyInterestedParty)), "DutyInterestedParty - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalDutyReferences_DutyInterestedParty_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var dutyIntrestedParty = new Mock<IImportParty>();
			dutyIntrestedParty.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(a => a.Name).Returns("WTG");
			address.Setup(a => a.Address).Returns("Mainzerstr. 20");
			address.Setup(a => a.City).Returns("Wiesbaden");
			address.Setup(a => a.Postcode).Returns("52211");
			address.Setup(a => a.District).Returns("Hessen");
			dutyIntrestedParty.Setup(m => m.Address).Returns(address.Object);

			var additionalDutyReferences = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences.Setup(duty => duty.ReferenceNumber).Returns("CFR00001");
			additionalDutyReferences.Setup(duty => duty.DutyInterestedParty).Returns(dutyIntrestedParty.Object);

			headerMock.Setup(h => h.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[] { additionalDutyReferences.Object });
			var message = cfcrecBuilder.GenerateMessage();
			var referenceDutyInterestedParty = message.Body.AdditionalDutyReferences[0].DutyInterestedParty;
			NUnit.Framework.Assert.That(referenceDutyInterestedParty.Name, Is.EqualTo("WTG"), "DutyInterestedParty Name");
			NUnit.Framework.Assert.That(referenceDutyInterestedParty.Address.Line, Is.EqualTo("Mainzerstr. 20"), "DutyInterestedParty Address");
			NUnit.Framework.Assert.That(referenceDutyInterestedParty.Address.City, Is.EqualTo("Wiesbaden"), "DutyInterestedParty City");
			NUnit.Framework.Assert.That(referenceDutyInterestedParty.Address.Postcode, Is.EqualTo("52211"), "DutyInterestedParty Postcode");
			NUnit.Framework.Assert.That(referenceDutyInterestedParty.Address.District, Is.EqualTo("Hessen"), "DutyInterestedParty District");
		}

		[ExpectNoExceptions]
		public void TestPopulateForeignTradeStatistics_TotalGrossMassMeasure_NotEmitWhen0()
		{
			headerMock.Setup(m => m.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(0m);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.ForeignTradeStatistics.TotalGrossMassMeasureSpecified, Is.EqualTo(false), "<TotalGrossMassMeasure> not specified");
			NUnit.Framework.Assert.That(message.Body.ForeignTradeStatistics.TotalGrossMassMeasure, Is.EqualTo(0m), "<TotalGrossMassMeasure> not contained");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineOriginCountry_EqualsPreferentialOriginCntryAndRequestedPreferentialTreatmentMoreThan199()
		{
			goodsItemMock.Setup(m => m.OriginCountry).Returns("CN");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(default(string)), "No Origin country when same as Preferential Origin Country - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineOriginCountry_Empty()
		{
			goodsItemMock.Setup(m => m.OriginCountry).Returns("");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(default(string)), "No Origin country when different to Preferential Origin Country and empty - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineOriginCountry_RequestedPreferentialTreatmentLess200()
		{
			preferentialTreatmentMock.Setup(m => m.RequestedPreferentialTreatment).Returns("199");
			goodsItemMock.Setup(m => m.OriginCountry).Returns("CN");
			goodsItemMock.Setup(m => m.PreferentialOriginCountry).Returns("CN");
			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo("CN"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineAdditionalProcedure()
		{
			goodsItemMock.Setup(m => m.AdditionalProcedure).Returns(new[] { GetLongString("C", 9) });

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].AdditionalProcedure[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineSupplementaryCodes()
		{
			goodsItemMock.Setup(m => m.SupplementaryCodes).Returns(new[] { GetLongString("C", 9) });

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].SupplementaryCodes[0].Code, Is.EqualTo(GetLongString("C", 4)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePackage_NoPackages()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package, Is.EqualTo(default(FCFCRFBodyGoodsItemPackage)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePackage_NotCountable()
		{
			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("NE");
			package.Setup(p => p.Quantity).Returns((int?)null);
			package.Setup(m => m.MarksNumbers).Returns(string.Empty);

			goodsItemMock.Setup(m => m.Package).Returns(package.Object);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package.Kind, Is.EqualTo("NE"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineForeignTradeStatistics_NotPopulated()
		{
			goodsItemMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(decimal.Zero);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].ForeignTradeStatistics, Is.EqualTo(default(FCFCRFBodyGoodsItemForeignTradeStatistics)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePreferentialTreatment_ContingentNumber()
		{
			preferentialTreatmentMock.Setup(m => m.ContingentNumber).Returns(new[] { GetLongString("C", 9) });

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].PreferentialTreatment.Declaration.Contingent[0].ContingentNumber, Is.EqualTo(GetLongString("C", 4)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePreferentialTreatment_Quantity_Empty()
		{
			preferentialTreatmentMock.Setup(m => m.Quantity).Returns((IAmount)null);

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].PreferentialTreatment.Declaration.PreferentialTreatmentQuantity, Is.EqualTo(default(FCFCRFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineDocuments_WriteOff_Empty()
		{
			var documents = new Mock<IImportLineDocument>();
			documents.Setup(d => d.Division).Returns("4");
			documents.Setup(d => d.DocumentType).Returns("7HHF");
			documents.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents.Setup(d => d.AtHandFlag).Returns("J");
			documents.Setup(m => m.WriteOff).Returns((IAmount)null);

			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });

			var message = cfcrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Document[0].WriteOff, Is.EqualTo(default(FCFCRFBodyGoodsItemDocumentWriteOff)));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsItemsIdentificationByRegistration_AWB()
		{
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			var message = cfcrecBuilder.GenerateMessage();

			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration, Is.EqualTo(default(FCFCRFSummaryDeclarationGoodsItemIdentificationByRegistration)));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsItemsIdentificationByRegistration_REG_MRN()
		{
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("REG");
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns(deMRN);
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var identification = message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration;
				NUnit.Framework.Assert.That(identification.MRN, Is.EqualTo(deMRN));
				NUnit.Framework.Assert.That(identification.ReferencedRegistrationNumber, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationGoodsItemsIdentificationByRegistration_REG_RegistrationNumber()
		{
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("REG");
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var identification = message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration;
				NUnit.Framework.Assert.That(identification.MRN, Is.EqualTo(default(string)));
				NUnit.Framework.Assert.That(identification?.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouseGoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns(deMRN);
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var goodsItem = message.CustomsWarehouse.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo(deMRN));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouseGoodsItem_RegistrationNumber()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var goodsItem = message.CustomsWarehouse.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo(default(string)));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessingGoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			inwardProcessingGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns(deMRN);
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var goodsItem = message.InwardProcessing.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo(deMRN));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessingGoodsItem_RegistrationNumber()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var message = cfcrecBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				var goodsItem = message.InwardProcessing.GoodsItem[0];
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo(default(string)));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.EqualTo(registrationNumber));
			});
		}

		void SetupGoodsItem()
		{
			preferentialTreatmentMock = new Mock<ILinePreferentialTreatment>();
			preferentialTreatmentMock.Setup(l => l.RequestedPreferentialTreatment).Returns("200");
			preferentialTreatmentMock.Setup(l => l.ContingentNumber).Returns(new[] { "1XYZ" });
			preferentialTreatmentMock.Setup(m => m.Quantity).Returns(GetAmount(18219.1m, "NAR", "X").Object);

			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("CT");
			package.Setup(p => p.Quantity).Returns(970);
			package.Setup(p => p.MarksNumbers).Returns("1-970");

			var assessmentSpecificRate = new Mock<IImportSpecificRate>();
			assessmentSpecificRate.Setup(r => r.Type).Returns("X");
			assessmentSpecificRate.Setup(r => r.Value).Returns(10.021m);

			var assessmentContentInformation = new Mock<IContentInformation>();
			assessmentContentInformation.Setup(r => r.ContentType).Returns("X");
			assessmentContentInformation.Setup(r => r.DegreePercentage).Returns(0.011m);

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

			goodsItemMock = new Mock<ICFCRECLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("4000");
			goodsItemMock.Setup(l => l.CessionManagementFlag).Returns("A1");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.PreferentialOriginCountry).Returns("JP");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.TobaccoRevenueStampNumber).Returns("A1234");
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.5m);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.6m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { GetAmount(18219, "NAR", "X").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { assessmentSpecificRate.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { assessmentContentInformation.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { exciseDuty.Object });
			goodsItemMock.Setup(l => l.PreferentialTreatment).Returns(preferentialTreatmentMock.Object);
			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();

			summaryDeclarationGoodsItemMock = new Mock<ISummaryDeclarationGoodsItem>();
			summaryDeclarationGoodsItemMock.Setup(x => x.Quantity).Returns(1);
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByKeyKind).Returns("AWB");
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByKeyNumber).Returns("1234567890");
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByKeyCustodianIdentifier).Returns("DE3333333");
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns(registrationNumber);
			summaryDeclarationGoodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedSequenceNumber).Returns(1);

			summaryDeclarationMock = new Mock<ISummaryDeclaration>();
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			summaryDeclarationMock.Setup(x => x.GoodsItems).Returns(new ISummaryDeclarationGoodsItem[] { summaryDeclarationGoodsItemMock.Object });

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

			var identification4 = new Mock<IPartyID>();
			identification4.Setup(id => id.EoriNumber).Returns("GR667890124");
			identification4.Setup(id => id.EoriBranchSuffix).Returns("0005");
			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Identification).Returns(identification4.Object);

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

			var identification5 = new Mock<IPartyID>();
			identification5.Setup(pi => pi.EoriNumber).Returns("CN00001");
			var dutyInterestedParty1 = new Mock<IImportParty>();
			dutyInterestedParty1.Setup(r => r.Identification).Returns(identification5.Object);
			var additionalDutyReferences1 = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences1.Setup(duty => duty.ReferenceNumber).Returns("CFR00001");
			additionalDutyReferences1.Setup(duty => duty.DutyInterestedParty).Returns(dutyInterestedParty1.Object);

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(ip => ip.Name).Returns("DutyName");
			address.Setup(ip => ip.Postcode).Returns("88888888");
			address.Setup(ip => ip.City).Returns("Nanjing");
			address.Setup(ip => ip.District).Returns("Xuanwu");
			address.Setup(ip => ip.Address).Returns("Zhongshan road");
			address.Setup(ip => ip.Country).Returns("CN");
			var dutyInterestedParty2 = new Mock<IImportParty>();
			dutyInterestedParty2.Setup(r => r.Address).Returns(address.Object);
			var additionalDutyReferences2 = new Mock<IImportAdditionalDutyReference>();
			additionalDutyReferences2.Setup(duty => duty.ReferenceNumber).Returns("CFR00002");
			additionalDutyReferences2.Setup(duty => duty.DutyInterestedParty).Returns(dutyInterestedParty2.Object);

			headerMock = new Mock<ICFCRECHeader>();
			headerMock.Setup(h => h.DeclarationKind).Returns("C");
			headerMock.Setup(h => h.DeclarationType).Returns("AZ");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.LocalClearanceDate).Returns(new DateTime(2021, 05, 01));
			headerMock.Setup(h => h.PrematureInputFlag).Returns(true);
			headerMock.Setup(h => h.GoodsItemQuantity).Returns(10);
			headerMock.Setup(h => h.CustomsGoodsStatus).Returns("IM");
			headerMock.Setup(h => h.ProcedureAuthorisation).Returns("DE001234567");
			headerMock.Setup(h => h.LocalClearanceProcedure).Returns("DE001234567");
			headerMock.Setup(h => h.GoodsLocation).Returns("locationOfGoods");
			headerMock.Setup(h => h.DepartureCountry).Returns("LV");
			headerMock.Setup(h => h.CurrencyCode).Returns("USD");
			headerMock.Setup(h => h.AdditionalInformation).Returns("AdditionalInformation");
			headerMock.Setup(h => h.TaxOffice).Returns("DE001234");
			headerMock.Setup(h => h.RepresentativeRelationshipFlag).Returns("0");
			headerMock.Setup(h => h.DeclarationPlace).Returns("Hamburg");
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
			headerMock.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("3");
			headerMock.Setup(h => h.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(123456789.1m);
			headerMock.Setup(h => h.Declarant).Returns(declarant.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(h => h.Principal).Returns(principal.Object);
			headerMock.Setup(h => h.Consignee).Returns(consignee.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(h => h.Documents).Returns(new IImportDocument[] { documents1.Object, documents2.Object });
			headerMock.Setup(h => h.Lines).Returns(new ICFCRECLine[] { goodsItemMock.Object });
			headerMock.Setup(h => h.AdditionalDutyReferences)
				.Returns(new IImportAdditionalDutyReference[] { additionalDutyReferences1.Object, additionalDutyReferences2.Object });

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

			cfcrecBuilder = CreateCFCRECBuilder();
		}

		CFCRECMessageBuilder CreateCFCRECBuilder()
		{
			return new CFCRECMessageBuilder(messageHeaderMock.Object);
		}

		CFCRECMessageBuilder cfcrecBuilder;
		Mock<ICFCRECHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ISummaryDeclarationGoodsItem> summaryDeclarationGoodsItemMock;
		Mock<ICustomsWarehouse> customsWarehouseMock;
		Mock<ICustomsWarehouseGoodsItem> customsWarehouseGoodsItemMock;
		Mock<IAmount> customsWarehouseGoodsItemCommercialAmountMock;
		Mock<IAmount> customsWarehouseGoodsItemDebitAmountMock;
		Mock<IInwardProcessing> inwardProcessingMock;
		Mock<IInwardProcessingGoodsItem> inwardProcessingGoodsItemMock;
		Mock<ICFCRECLine> goodsItemMock;
		Mock<ILinePreferentialTreatment> preferentialTreatmentMock;

		const string registrationNumber = "ATA123456789123456789";
		const string deMRN = "24DE586600522085U1";

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
			customsWarehouseGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns(registrationNumber);
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
			inwardProcessingGoodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns(registrationNumber);
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
	}
}
