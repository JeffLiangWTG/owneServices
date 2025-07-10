using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestTimeZone]
	sealed class EXPAMDMessageBuilderTest : AESMessageBuilderTest<EXPAMDMessageBuilder, CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAE>
	{
		[ExpectNoExceptions]
		public void TestExportOperation()
		{
			var message = messageBuilder.GenerateMessage();
			var exportOperation = message.ExportOperation;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(exportOperation.LRN, Is.EqualTo("LRN00001"), "LRN");
				NUnit.Framework.Assert.That(exportOperation.MRN, Is.EqualTo("MRN00001"), "MRN");
				NUnit.Framework.Assert.That(exportOperation.amendmentSubmissionDateAndTime, Is.EqualTo(new DateTime(2021, 8, 12, 09, 53, 11)), "amendmentSubmissionDateAndTime");
				NUnit.Framework.Assert.That(exportOperation.totalAmountInvoiced, Is.EqualTo(2.22m), "totalAmountInvoiced");
				NUnit.Framework.Assert.That(exportOperation.totalAmountInvoicedSpecified, Is.EqualTo(true), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo("EUR"), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestExportOperation_Abnormal()
		{
			mockHeader.Setup(m => m.LocalReferenceNumber).Returns(GetLongString("L", LocalReferenceNumberMaxLength + 1));
			mockHeader.Setup(m => m.MRN).Returns(GetLongString("M", MRNMaxLength + 1));
			mockHeader.Setup(m => m.InvoiceAmount).Returns(0);
			mockHeader.Setup(m => m.Currency).Returns(GetLongString("C", CurrencyMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ExportOperation.LRN, Is.EqualTo(GetLongString("L", LocalReferenceNumberMaxLength)), "LRN");
				NUnit.Framework.Assert.That(message.ExportOperation.MRN, Is.EqualTo(GetLongString("M", MRNMaxLength)), "MRN");
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(true), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo(GetLongString("C", CurrencyMaxLength)), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestExportOperation_EmptyLRNNotMapped()
		{
			mockHeader.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ExportOperation.LRN, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestExportOperation_EmptyMRNNotMapped()
		{
			mockHeader.Setup(m => m.MRN).Returns(ZString.Empty);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ExportOperation.MRN, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestExportOperation_AmountAndCurrency()
		{
			mockHeader.Setup(m => m.InvoiceAmountAndCurrencySpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(false), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo(default(string)), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExport()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExport.referenceNumber, Is.EqualTo("DE003202"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExport_Abnormal()
		{
			mockHeader.Setup(m => m.ExportCustomsOffice).Returns(GetLongString("P", ExportCustomsOfficeMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExport.referenceNumber, Is.EqualTo(GetLongString("P", ExportCustomsOfficeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestDeclarant()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Declarant.identificationNumber, Is.EqualTo("DEEOR001"), "identificationNumber");
				NUnit.Framework.Assert.That(message.Declarant.subsidiaryNumber, Is.EqualTo("0001"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestRepresentative()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Representative.identificationNumber, Is.EqualTo("DEEOR002"), "identificationNumber");
				NUnit.Framework.Assert.That(message.Representative.subsidiaryNumber, Is.EqualTo("0002"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.inlandModeOfTransport, Is.EqualTo("1"), "inlandModeOfTransport");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.modeOfTransportAtTheBorder, Is.EqualTo("2"), "modeOfTransportAtTheBorder");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.grossMass, Is.EqualTo(3.33m), "grossMass");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.grossMassSpecified, Is.EqualTo(true), "grossMassSpecified");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.referenceNumberUCR, Is.EqualTo("CRN00001"), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_Abnormal()
		{
			mockHeader.Setup(m => m.InlandTransportMeansMode).Returns(GetLongString("I", InlandTransportMeanModeMaxLength1 + 1));
			mockHeader.Setup(m => m.BorderTransportMeansMode).Returns(GetLongString("B", BorderTransportMeansModeMaxLength1 + 1));
			mockHeader.Setup(m => m.TotalGrossMass).Returns(0m);
			mockHeader.Setup(m => m.CommercialReferenceNumber).Returns(GetLongString("C", ReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.inlandModeOfTransport, Is.EqualTo(GetLongString("I", InlandTransportMeanModeMaxLength1)), "inlandModeOfTransport");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.modeOfTransportAtTheBorder, Is.EqualTo(GetLongString("B", BorderTransportMeansModeMaxLength1)), "modeOfTransportAtTheBorder");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.grossMassSpecified, Is.EqualTo(false), "grossMassSpecified");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.referenceNumberUCR, Is.EqualTo(GetLongString("C", ReferenceNumberMaxLength)), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_EmptyModeOfTransportAtTheBorderNotMapped()
		{
			mockHeader.Setup(m => m.BorderTransportMeansMode).Returns(ZString.Empty);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.modeOfTransportAtTheBorder, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_EmptyReferenceNumberUCRNotMapped()
		{
			mockHeader.Setup(m => m.CommercialReferenceNumber).Returns(ZString.Empty);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.referenceNumberUCR, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_ContainerIndicator()
		{
			var message = CreateMessageBuilder().GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.containerIndicator.ToString(), Is.EqualTo("Item1"), "containerIndicator is Item1");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.containerIndicatorSpecified, Is.EqualTo(true), "containerIndicatorSpecified is true when IsContainerized is true");

				mockHeader.Setup(m => m.IsContainerized).Returns(false);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.containerIndicator.ToString(), Is.EqualTo("Item0"), "containerIndicator is Item0");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.containerIndicatorSpecified, Is.EqualTo(true), "containerIndicatorSpecified is true when IsContainerized is false");

				mockHeader.Setup(m => m.ContainerIndicatorSpecified).Returns(false);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.containerIndicatorSpecified, Is.EqualTo(false), "containerIndicatorSpecified is false");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_TransportEquipment()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.TransportEquipment;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.containerIdentificationNumber, Is.EqualTo("Reference1"), "element1 containerIdentificationNumber");
				NUnit.Framework.Assert.That(element1.numberOfSeals, Is.EqualTo("0"), "element1 numberOfSeals");
				NUnit.Framework.Assert.That(element1.Seal, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAEGoodsShipmentConsignmentTransportEquipmentSeal[])), "element1 Seal - should be [null]");
				NUnit.Framework.Assert.That(element1.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "element1 GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[0].declarationGoodsItemNumber, Is.EqualTo("1"), "element1 GoodsReference[0].declarationGoodsItemNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "element1 GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[1].declarationGoodsItemNumber, Is.EqualTo("3"), "element1 GoodsReference[1].declarationGoodsItemNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.containerIdentificationNumber, Is.EqualTo("Reference2"), "element2 containerIdentificationNumber");
				NUnit.Framework.Assert.That(element2.numberOfSeals, Is.EqualTo("0"), "element2 numberOfSeals");
				NUnit.Framework.Assert.That(element2.Seal, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAEGoodsShipmentConsignmentTransportEquipmentSeal[])), "element2 Seal - should be [null]");
				NUnit.Framework.Assert.That(element2.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "element2 GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[0].declarationGoodsItemNumber, Is.EqualTo("2"), "element2 GoodsReference[0].declarationGoodsItemNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "element2 GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[1].declarationGoodsItemNumber, Is.EqualTo("4"), "element2 GoodsReference[1].declarationGoodsItemNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_TransportEquipment_Abnormal()
		{
			var transportEquipments = new[]
			{
				MockTransportEquipment(GetLongString("C", ContainerIdentificationNumberMaxLength + 1)
				, null
				, new int[] { 1 }).Object,
			};
			mockHeader.Setup(m => m.TransportEquipments).Returns(transportEquipments);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.TransportEquipment[0].containerIdentificationNumber, Is.EqualTo(GetLongString("C", ContainerIdentificationNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_TransportEquipment_EmptyContainerIdentificationNumberNotMapped()
		{
			var transportEquipments = new[]
			{
				MockTransportEquipment(string.Empty
					, null
					, new int[] { 1 }).Object,
			};
			mockHeader.Setup(m => m.TransportEquipments).Returns(transportEquipments);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.TransportEquipment[0].containerIdentificationNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_DepartureTransportMeans()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.DepartureTransportMeans;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfIdentification, Is.EqualTo("T1"), "element1 typeOfIdentification");
				NUnit.Framework.Assert.That(element1.identificationNumber, Is.EqualTo("0001"), "element1 identificationNumber");
				NUnit.Framework.Assert.That(element1.nationality, Is.EqualTo("DE"), "element1 nationality");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfIdentification, Is.EqualTo("T2"), "element2 typeOfIdentification");
				NUnit.Framework.Assert.That(element2.identificationNumber, Is.EqualTo("0002"), "element2 identificationNumber");
				NUnit.Framework.Assert.That(element2.nationality, Is.EqualTo("AU"), "element2 nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_DepartureTransportMeanst_Abnormal()
		{
			var departureTransportMeans = new[]
			{
				MockDepartureTransportMeans(GetLongString("T", TransportMeansTypeMaxLength + 1)
				, GetLongString("I", TransportMeansIdentityMaxLength + 1)
				, GetLongString("N", TransportMeansNationalityMaxLength + 1)).Object,
			};
			mockHeader.Setup(m => m.DepartureTransportMeans).Returns(departureTransportMeans);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.Consignment.DepartureTransportMeans[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.typeOfIdentification, Is.EqualTo(GetLongString("T", TransportMeansTypeMaxLength)), "typeOfIdentification");
				NUnit.Framework.Assert.That(element.identificationNumber, Is.EqualTo(GetLongString("I", TransportMeansIdentityMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(element.nationality, Is.EqualTo(GetLongString("N", TransportMeansNationalityMaxLength)), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_ActiveBorderTransportMeans()
		{
			var message = messageBuilder.GenerateMessage();
			var activeBorderTransportMeans = message.GoodsShipment.Consignment.ActiveBorderTransportMeans;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(activeBorderTransportMeans.typeOfIdentification, Is.EqualTo("10"), "typeOfIdentification");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.identificationNumber, Is.EqualTo("1234567890"), "identificationNumber");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.nationality, Is.EqualTo("AU"), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_ActiveBorderTransportMeans_Abnormal()
		{
			mockHeader.Setup(m => m.BorderTransportMeansType).Returns(GetLongString("T", TransportMeansTypeMaxLength + 1));
			mockHeader.Setup(m => m.BorderTransportMeansIdentity).Returns(GetLongString("I", TransportMeansIdentityMaxLength + 1));
			mockHeader.Setup(m => m.BorderTransportMeansNationality).Returns(GetLongString("N", TransportMeansNationalityMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var activeBorderTransportMeans = message.GoodsShipment.Consignment.ActiveBorderTransportMeans;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(activeBorderTransportMeans.typeOfIdentification, Is.EqualTo(GetLongString("T", TransportMeansTypeMaxLength)), "typeOfIdentification");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.identificationNumber, Is.EqualTo(GetLongString("I", TransportMeansIdentityMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.nationality, Is.EqualTo(GetLongString("N", TransportMeansNationalityMaxLength)), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_ActiveBorderTransportMeans_Null()
		{
			mockHeader.Setup(m => m.ActiveBorderTransportMeansSpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ActiveBorderTransportMeans, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAEGoodsShipmentConsignmentActiveBorderTransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment_Consignment_GrossWeightSpecified_false()
		{
			mockLine1.Setup(m => m.CommoditySpecified).Returns(false);
			var mockLine2 = SetUpLine();
			mockLine2.Setup(m => m.CommoditySpecified).Returns(false);

			mockHeader.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.grossMassSpecified, Is.EqualTo(false), "gross mass not specified, when Commodity is not specified");
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(false), "total amount invoiced not specified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem()
		{
			var message = messageBuilder.GenerateMessage();
			var goodsItem = message.GoodsShipment.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(goodsItem.statisticalValue, Is.EqualTo(1.11m), "statisticalValue");
				NUnit.Framework.Assert.That(goodsItem.statisticalValueSpecified, Is.EqualTo(true), "statisticalValueSpecified");
				NUnit.Framework.Assert.That(goodsItem.referenceNumberUCR, Is.EqualTo("CRN0001"), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Abnormal()
		{
			mockLine1.Setup(m => m.StatisticalValueSpecified).Returns(false);
			mockLine1.Setup(m => m.CommercialReferenceNumber).Returns(GetLongString("C", ReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var goodsItem = message.GoodsShipment.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.statisticalValueSpecified, Is.EqualTo(false), "statisticalValueSpecified");
				NUnit.Framework.Assert.That(goodsItem.referenceNumberUCR, Is.EqualTo(GetLongString("C", ReferenceNumberMaxLength)), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_EmptyReferenceNumberUCRNotMapped()
		{
			mockLine1.Setup(m => m.CommercialReferenceNumber).Returns(ZString.Empty);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].referenceNumberUCR, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestItem_Commodity()
		{
			var message = messageBuilder.GenerateMessage();
			var goodsMeasure = message.GoodsShipment.GoodsItem[0].Commodity.GoodsMeasure;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsMeasure.grossMass, Is.EqualTo(2.22m), "grossMass");
				NUnit.Framework.Assert.That(goodsMeasure.netMass, Is.EqualTo(3.33m), "netMass");
				NUnit.Framework.Assert.That(goodsMeasure.supplementaryUnitsSpecified, Is.EqualTo(true), "supplementaryUnitsSpecified");
				NUnit.Framework.Assert.That(goodsMeasure.supplementaryUnits, Is.EqualTo(4.44m), "supplementaryUnits");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Commodity_Abnormal()
		{
			mockLine1.Setup(m => m.SupplementaryQuantity).Returns(0m);

			var message = messageBuilder.GenerateMessage();
			var goodsMeasure = message.GoodsShipment.GoodsItem[0].Commodity.GoodsMeasure;
			NUnit.Framework.Assert.That(goodsMeasure.supplementaryUnitsSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestItem_Commodity_Null()
		{
			mockLine1.Setup(m => m.CommoditySpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Commodity, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAEGoodsShipmentGoodsItemCommodity)));
		}

		[ExpectNoExceptions]
		public void TestItem_StatisticalValusOnlyMappedIfCommodityIsPresent()
		{
			CombineAssertions(() =>
			{
				mockLine1.Setup(m => m.CommoditySpecified).Returns(true);
				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].statisticalValueSpecified, Is.EqualTo(true), "When CommoditySpecified");

				mockLine1.Setup(m => m.CommoditySpecified).Returns(false);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].statisticalValueSpecified, Is.EqualTo(false), "When not CommoditySpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Packaging;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfPackages, Is.EqualTo("1A"), "element1 typeOfPackages");
				NUnit.Framework.Assert.That(element1.numberOfPackages, Is.EqualTo("2"), "element1 numberOfPackages");
				NUnit.Framework.Assert.That(element1.shippingMarks, Is.EqualTo("1234567890"), "element1 shippingMarks");
				NUnit.Framework.Assert.That(element1.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAEGoodsShipmentGoodsItemPackagingPackageReference)), "element1 PackageReference - should be [null]");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfPackages, Is.EqualTo("2C"), "element2 typeOfPackages");
				NUnit.Framework.Assert.That(element2.numberOfPackages, Is.EqualTo(default(string)), "element2 numberOfPackages - should be [null]");
				NUnit.Framework.Assert.That(element2.shippingMarks, Is.EqualTo("1234567891"), "element2 shippingMarks");
				NUnit.Framework.Assert.That(element2.PackageReference.declarationGoodsItemNumber, Is.EqualTo("2"), "element2 PackageReference");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_IsOnlyMappedIfCommodityIsPresent()
		{
			CombineAssertions(() =>
			{
				mockLine1.Setup(m => m.CommoditySpecified).Returns(true);
				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Packaging.Any(), Is.EqualTo(true), "When CommoditySpecified");

				mockLine1.Setup(m => m.CommoditySpecified).Returns(false);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Packaging.Any(), Is.EqualTo(false), "When not CommoditySpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_Abnormal()
		{
			var packages = new[]
			{
				MockPackage(2, GetLongString("K", PackageKindMaxLength2 + 1), GetLongString("N", PackageMarksNumbersMaxLength + 1), 0, true).Object,
			};
			mockLine1.Setup(m => m.Packages).Returns(packages);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].Packaging[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.typeOfPackages, Is.EqualTo(GetLongString("K", PackageKindMaxLength2)), "typeOfPackages");
				NUnit.Framework.Assert.That(element.shippingMarks, Is.EqualTo(GetLongString("N", PackageMarksNumbersMaxLength)), "shippingMarks");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_EmptyShippingMarksNotMapped()
		{
			var packages = new[]
			{
				MockPackage(2, GetLongString("K", PackageKindMaxLength2 + 1), string.Empty, 0, true).Object,
			};
			mockLine1.Setup(m => m.Packages).Returns(packages);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Packaging[0].shippingMarks, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestItem_Multiple()
		{
			var mockLine2 = new Mock<IEXPAMDLine>();
			mockLine2.Setup(l => l.LineNumber).Returns(2);
			mockLine2.Setup(l => l.StatisticalValueSpecified).Returns(true);
			mockLine2.Setup(l => l.StatisticalValue).Returns(1.11m);
			mockLine2.Setup(l => l.CommercialReferenceNumber).Returns("CRN0002");
			mockLine2.Setup(l => l.CommoditySpecified).Returns(true);
			mockLine2.Setup(l => l.GrossMass).Returns(2.22m);
			mockLine2.Setup(l => l.NetMass).Returns(3.33m);
			mockLine2.Setup(l => l.SupplementaryQuantity).Returns(4.44m);
			mockLine2.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackage>)GetPackages());
			mockHeader.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem.Length, Is.EqualTo(2));
		}

		protected override string GetCompleteMessageFileName() => "TestEXPAMDMessage";

		protected override string GetExpectedMessageVersion() => "E.1.7";

		protected override void SetUp()
		{
			base.SetUp();

			SetUpHeader();
			mockLine1 = SetUpLine();
			mockHeader.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			messageHeader.Setup(m => m.AESHeader).Returns(mockHeader.Object);
			messageBuilder = CreateMessageBuilder();
		}

		EXPAMDMessageBuilder CreateMessageBuilder()
		{
			return new EXPAMDMessageBuilder(messageHeader.Object);
		}

		Mock<IEXPAMDHeader> mockHeader;
		Mock<IEXPAMDLine> mockLine1;

		void SetUpHeader()
		{
			var submissionDateAndTimeMock = new Mock<IDateAndTime>();
			submissionDateAndTimeMock.Setup(dt => dt.DateAndTime).Returns(new DateTime(2021, 08, 12, 09, 53, 11, DateTimeKind.Utc));

			mockHeader = new Mock<IEXPAMDHeader>();
			mockHeader.Setup(h => h.LocalReferenceNumber).Returns("LRN00001");
			mockHeader.Setup(h => h.MRN).Returns("MRN00001");
			mockHeader.Setup(h => h.SubmissionDateAndTimeUtc).Returns(submissionDateAndTimeMock.Object);
			mockHeader.Setup(h => h.ContainerIndicatorSpecified).Returns(true);
			mockHeader.Setup(h => h.InvoiceAmount).Returns(2.22m);
			mockHeader.Setup(h => h.InvoiceAmountAndCurrencySpecified).Returns(true);
			mockHeader.Setup(h => h.Currency).Returns("EUR");
			mockHeader.Setup(h => h.ExportCustomsOffice).Returns("DE003202");
			mockHeader.Setup(h => h.Declarant).Returns(GetDeclarant());
			mockHeader.Setup(h => h.Representative).Returns(GetRepresentative());
			mockHeader.Setup(h => h.IsContainerized).Returns(true);
			mockHeader.Setup(h => h.InlandTransportMeansMode).Returns("1");
			mockHeader.Setup(h => h.BorderTransportMeansMode).Returns("2");
			mockHeader.Setup(h => h.TotalGrossMass).Returns(3.33m);
			mockHeader.Setup(h => h.CommercialReferenceNumber).Returns("CRN00001");
			mockHeader.Setup(h => h.TransportEquipments).Returns((IReadOnlyCollection<ITransportEquipment>)GetTransportEquipments());
			mockHeader.Setup(h => h.DepartureTransportMeans).Returns((IReadOnlyCollection<IDepartureTransportMeans>)GetDepartureTransportMeans());
			mockHeader.Setup(h => h.ActiveBorderTransportMeansSpecified).Returns(true);
			mockHeader.Setup(h => h.BorderTransportMeansType).Returns("10");
			mockHeader.Setup(h => h.BorderTransportMeansIdentity).Returns("1234567890");
			mockHeader.Setup(m => m.BorderTransportMeansNationality).Returns("AU");

			IAESParty GetDeclarant()
			{
				return MockParty("DEEOR001", "0001").Object;
			}

			IAESParty GetRepresentative()
			{
				return MockParty("DEEOR002", "0002").Object;
			}

			IEnumerable<ITransportEquipment> GetTransportEquipments()
			{
				return new[]
				{
					MockTransportEquipment("Reference1", null, new int[] { 1, 3 }).Object,
					MockTransportEquipment("Reference2", null, new int[] { 2, 4 }).Object,
				};
			}

			IEnumerable<IDepartureTransportMeans> GetDepartureTransportMeans()
			{
				return new[]
				{
					MockDepartureTransportMeans("T1", "0001", "DE").Object,
					MockDepartureTransportMeans("T2", "0002", "AU").Object,
				};
			}
		}

		Mock<IEXPAMDLine> SetUpLine()
		{
			var mock = new Mock<IEXPAMDLine>();
			mock.Setup(l => l.LineNumber).Returns(1);
			mock.Setup(l => l.StatisticalValueSpecified).Returns(true);
			mock.Setup(l => l.StatisticalValue).Returns(1.11m);
			mock.Setup(l => l.CommercialReferenceNumber).Returns("CRN0001");
			mock.Setup(l => l.CommoditySpecified).Returns(true);
			mock.Setup(l => l.GrossMass).Returns(2.22m);
			mock.Setup(l => l.NetMass).Returns(3.33m);
			mock.Setup(l => l.SupplementaryQuantity).Returns(4.44m);
			mock.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackage>)GetPackages());
			return mock;
		}

		IEnumerable<IPackage> GetPackages()
		{
			return new[]
			{
				MockPackage(2, "1A", "1234567890", 0, true).Object,
				MockPackage(0, "2C", "1234567891", 2, false).Object,
			};
		}
	}
}
