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
	sealed class SCWRECMessageBuilderTest : MessageBuilderTest<SCWRECMessageBuilder, LSCWRL>
	{
		[TestDate(2021, 05, 01, 10, 25, 00)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(scwrecBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestSCWRECMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(scwrecBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestMessageGroup_LVV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("LVV");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.MetaData.MessageGroup, Is.EqualTo(LSCWRLMetaDataMessageGroup.LVV));
		}

		[ExpectNoExceptions]
		public void TestPopulateLocalClearanceDate_NotSpecified()
		{
			headerMock.Setup(m => m.LocalClearanceDate).Returns((DateTime?)null);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.LocalClearanceDateSpecified, Is.EqualTo(false), "LocalClearanceDateSpecified");
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IImportParty)null);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(LSCWRLRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var representative = new Mock<IImportParty>();
			representative.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			headerMock.Setup(m => m.Representative).Returns(representative.Object);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(LSCWRLRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateContactPerson_Null()
		{
			headerMock.Setup(m => m.ContactPerson).Returns((IImportPartyContactPerson)null);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ContactPerson, Is.EqualTo(default(LSCWRLContactPerson)));
		}

		[ExpectNoExceptions]
		public void TestPopulateBorderTransportMeans_TruncatedInformation()
		{
			headerMock.Setup(m => m.BorderTransportMeansInformation).Returns(string.Empty.PadLeft(18, 'A'));
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.BorderTransportMeans.Information, Is.EqualTo(string.Empty.PadLeft(17, 'A')));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReferences_TypeIsEmpty()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns(string.Empty);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences, Is.EqualTo(default(LSCWRLPreviousAdministrativeReferences)));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeT1()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(LSCWRLPreviousAdministrativeReferencesType.T1));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference.ReferenceNumber, Is.EqualTo("REFNUM12345"));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_TypeMiscellaneous()
		{
			UpdateMockedHeaderToPopulateNone();
			var message = scwrecBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(LSCWRLSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(LSCWRLCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(LSCWRLInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousAdministrativeReference_NotPopulated_PreviousAdministrativeReferenceNumberEmpty()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(string.Empty);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.Type, Is.EqualTo(LSCWRLPreviousAdministrativeReferencesType.ATNEU));
			NUnit.Framework.Assert.That(message.PreviousAdministrativeReferences.PreviousAdministrativeReference, Is.EqualTo(default(LSCWRLPreviousAdministrativeReferencesPreviousAdministrativeReference)));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(LSCWRLSummaryDeclarationIdentificationIndicator.AWB));
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclarationIdentificationIndicator_REG()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.IdentificationIndicator, Is.EqualTo(LSCWRLSummaryDeclarationIdentificationIndicator.REG));
		}

		[ExpectNoExceptions]
		public void TestPopulateIdentificationByKey_ULD()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByKey.Kind, Is.EqualTo(LSCWRLSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD));
		}

		[ExpectNoExceptions]
		public void TestPopulateIdentificationByRegistration()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedRegistrationNumber, Is.EqualTo("ATA123456789123456789"));
			NUnit.Framework.Assert.That(message.SummaryDeclaration.GoodsItem[0].IdentificationByRegistration.ReferencedSequenceNumber, Is.EqualTo("1"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_LocalReferenceNumber_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo("WTG5678"));

				customsWarehouseMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.LRN, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_AccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.AccessViaATLASFlag).Returns(false);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_UsualProcessingFlag_N()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			customsWarehouseGoodsItemMock.Setup(m => m.UsualProcessingFlag).Returns(false);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].UsualProcessingFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_Complement_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo("Description text"), "Contains Complement");

				customsWarehouseGoodsItemMock.Setup(m => m.Complement).Returns(string.Empty);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].Complement, Is.EqualTo(default(string)));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.Not.EqualTo(default(LSCWRLCustomsWarehouseGoodsItemCommercialAmount)), "Contains CommercialAmount - should not be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(1.234m));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.MeasurementUnit, Is.EqualTo("KGM"));
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"));

				customsWarehouseGoodsItemMock.Setup(m => m.CommercialAmount).Returns((IAmount)null);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount, Is.EqualTo(default(LSCWRLCustomsWarehouseGoodsItemCommercialAmount)), "Doesn't contain CommercialAmount - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmountQualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo("A"), "Contains Qualifier");
				customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_CommercialAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemCommercialAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].CommercialAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmount_Qualifier_NotPopulated()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo("M"), "Contains Qualifier");
				customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Qualifier).Returns(string.Empty);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Qualifier, Is.EqualTo(default(string)), "No Qualifier");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmount_Round()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1111m);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.111m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_DebitAmount_Normalize()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			customsWarehouseGoodsItemDebitAmountMock.Setup(m => m.Quantity).Returns(123.1100m);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsWarehouse.GoodsItem[0].DebitAmount.Quantity, Is.EqualTo(123.11m));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_PreviousAdministrativeReference_TypeATZL()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var message = scwrecBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(LSCWRLSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.Not.EqualTo(default(LSCWRLCustomsWarehouse)), "Contains CustomsWarehouse - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.EqualTo(default(LSCWRLInwardProcessing)), "Doesn't contain InwardProcessing - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_PreviousAdministrativeReference_TypeATAV()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var message = scwrecBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SummaryDeclaration, Is.EqualTo(default(LSCWRLSummaryDeclaration)), "Doesn't contain SummaryDeclaration - should be [null]");
				NUnit.Framework.Assert.That(message.CustomsWarehouse, Is.EqualTo(default(LSCWRLCustomsWarehouse)), "Doesn't contain CustomsWarehouse - should be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing, Is.Not.EqualTo(default(LSCWRLInwardProcessing)), "Contains InwardProcessing - should not be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_CustomsAuthorisation_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation, Is.Not.EqualTo(default(LSCWRLInwardProcessingCustomsAuthorisation)), "Contains CustomsAuthorisation - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation.ProcessingOwner, Is.EqualTo("DE55555555"));

				inwardProcessingMock.Setup(m => m.ProcessingOwnerIdentifier).Returns(string.Empty);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.CustomsAuthorisation, Is.EqualTo(default(LSCWRLInwardProcessingCustomsAuthorisation)), "Doesn't contain CustomsAuthorisation - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_MonitoringCustomsOffice_NotPopulated()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			CombineAssertions(() =>
			{
				var message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.Not.EqualTo(default(LSCWRLInwardProcessingMonitoringCustomsOffice)), "Contains MonitoringCustomsOffice - should not be [null]");
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice.Identification.ReferenceNumber, Is.EqualTo("DE001234"));

				inwardProcessingMock.Setup(m => m.SimplifiedGrantAuthorisationFlag).Returns(false);
				message = CreateSCWRECBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.InwardProcessing.MonitoringCustomsOffice, Is.EqualTo(default(LSCWRLInwardProcessingMonitoringCustomsOffice)), "Doesn't contain MonitoringCustomsOffice - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_GoodsItem_AccessViaAtlasFlag_N()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();

			inwardProcessingGoodsItemMock.Setup(m => m.AccessViaAtlasFlag).Returns(false);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.InwardProcessing.GoodsItem[0].AccessViaAtlasFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestPopulateForeignTradeStatisticsTotalGrossMassMeasure_NotEmitWhen0()
		{
			headerMock.Setup(m => m.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(0m);

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(!message.Body.ForeignTradeStatistics.TotalGrossMassMeasureSpecified, Is.True, "<TotalGrossMassMeasure> not contained");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineOriginCountry_Empty()
		{
			goodsItemMock.Setup(m => m.OriginCountry).Returns(string.Empty);

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].OriginCountry, Is.EqualTo(string.Empty), "No Origin country when different to Preferential Origin Country and empty");
		}

		[ExpectNoExceptions]
		public void TestPopulateLineAdditionalProcedure()
		{
			goodsItemMock.Setup(m => m.AdditionalProcedure).Returns(new[] { GetLongString("C", 9) });

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].AdditionalProcedure[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineSupplementaryCodes()
		{
			goodsItemMock.Setup(m => m.SupplementaryCodes).Returns(new[] { GetLongString("C", 9) });
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].SupplementaryCodes[0].Code, Is.EqualTo(GetLongString("C", 9)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePackage_NoPackages()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);
			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package, Is.EqualTo(default(LSCWRLBodyGoodsItemPackage)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLinePackage_NotCountable()
		{
			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("NE");
			package.Setup(p => p.Quantity).Returns((int?)null);
			package.Setup(m => m.MarksNumbers).Returns(string.Empty);

			goodsItemMock.Setup(m => m.Package).Returns(package.Object);

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Package.Kind, Is.EqualTo("NE"));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineForeignTradeStatistics_GrossMassMeasure_Empty()
		{
			goodsItemMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(decimal.Zero);

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].ForeignTradeStatistics, Is.EqualTo(default(LSCWRLBodyGoodsItemForeignTradeStatistics)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLineDocument_WriteOff_Empty()
		{
			var documents = new Mock<IImportLineDocument>();
			documents.Setup(d => d.Division).Returns("4");
			documents.Setup(d => d.DocumentType).Returns("7HHF");
			documents.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents.Setup(d => d.AtHandFlag).Returns("J");
			documents.Setup(m => m.WriteOff).Returns((IAmount)null);

			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });

			var message = scwrecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.GoodsItem[0].Document[0].WriteOff, Is.EqualTo(default(LSCWRLBodyGoodsItemDocumentWriteOff)));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var goodsItemMock = Mock.Get(headerMock.Object.CustomsWarehouse.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scwrecBuilder.GenerateMessage();

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

			var message = scwrecBuilder.GenerateMessage();

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

			var message = scwrecBuilder.GenerateMessage();

			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		void SetupGoodsItem()
		{
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
			documents.Setup(d => d.WriteOff).Returns(GetAmount(1500, "NAR", "Z").Object);

			goodsItemMock = new Mock<ISCWRECLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("4000");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.5m);
			goodsItemMock.Setup(l => l.InwardMovementAmount).Returns(GetAmount(15000, "NAR", "X").Object);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.6m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { GetAmount(18219, "NAR", "X").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { assessmentSpecificRate.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { assessmentContentInformation.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { exciseDuty.Object });
			goodsItemMock.Setup(h => h.RequestedPreferentialTreatment).Returns("200");
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
			summaryDeclarationMock.Setup(x => x.GoodsItems)
				.Returns(new ISummaryDeclarationGoodsItem[] { goodsItems.Object });

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

			headerMock = new Mock<ISCWRECHeader>();
			headerMock.Setup(h => h.DeclarationKind).Returns("A");
			headerMock.Setup(h => h.DeclarationType).Returns("EZA");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.LocalClearanceDate).Returns(new DateTime(2021, 04, 30));
			headerMock.Setup(h => h.ForeignTradeImportEarlyClearanceFlag).Returns("J");
			headerMock.Setup(h => h.PrematureInputFlag).Returns(true);
			headerMock.Setup(h => h.GoodsItemQuantity).Returns(10);
			headerMock.Setup(h => h.CustomsGoodsStatus).Returns("IM");
			headerMock.Setup(h => h.LocalClearanceProcedure).Returns("DE001234567");
			headerMock.Setup(h => h.CurrentProcedure).Returns("DE001234567");
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
			headerMock.Setup(h => h.Declarant).Returns(declarant.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(h => h.Principal).Returns(principal.Object);
			headerMock.Setup(h => h.Consignee).Returns(consignee.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("1");
			headerMock.Setup(h => h.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(20000.5m);
			headerMock.Setup(h => h.Documents).Returns(new IImportDocument[] { documents1.Object, documents2.Object });
			headerMock.Setup(m => m.Lines).Returns(new ISCWRECLine[] { goodsItemMock.Object });

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

			scwrecBuilder = CreateSCWRECBuilder();
		}

		SCWRECMessageBuilder CreateSCWRECBuilder()
		{
			return new SCWRECMessageBuilder(messageHeaderMock.Object);
		}

		SCWRECMessageBuilder scwrecBuilder;
		Mock<ISCWRECHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ICustomsWarehouse> customsWarehouseMock;
		Mock<ICustomsWarehouseGoodsItem> customsWarehouseGoodsItemMock;
		Mock<IAmount> customsWarehouseGoodsItemCommercialAmountMock;
		Mock<IAmount> customsWarehouseGoodsItemDebitAmountMock;
		Mock<IInwardProcessing> inwardProcessingMock;
		Mock<IInwardProcessingGoodsItem> inwardProcessingGoodsItemMock;
		Mock<ISCWRECLine> goodsItemMock;

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
