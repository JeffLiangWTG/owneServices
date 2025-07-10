using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ComplXAESMessageBuilder : AESCommonMessageBuilder<IComplXAESMessageDataProvider, Cc515Xv1Ent>
	{
		public ComplXAESMessageBuilder(IComplXAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		const string ExportOperationAdditionalType = "X";

		protected override ZString GetMessageType() => "CC515X";

		protected override Cc515Xv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc515Xv1Ent>();
			if (declaration != null)
			{
				declaration.Cc515X = GetPopulatedCC515X();
			}

			return declaration;
		}

		Cc515XType GetPopulatedCC515X()
		{
			var cC515Xv514 = GetPopulatedMessage<Cc515XType>();
			if (cC515Xv514 != null)
			{
				cC515Xv514.ExportOperation = GetPopulatedExportOperation();
				cC515Xv514.Declarant = GetPopulatedAddressInformationIdCommon<DeclarantType80>(provider.Declarant);
				cC515Xv514.Representative = GetPopulatedAddressInformationIdCommon<RepresentativeType80>(provider.Representative);
				cC515Xv514.GoodsShipment = GetPopulatedGoodsShipment();
			}

			return cC515Xv514;
		}

		ExportOperationType80 GetPopulatedExportOperation()
		{
			var exportOperation = provider.ExportOperation;
			return exportOperation == null ? null : new ExportOperationType80
			{
				Mrn = exportOperation.MRN,
				AdditionalDeclarationType = ExportOperationAdditionalType,
				TotalAmountInvoiced = exportOperation.TotalAmount.Round(MaxDecimals2),
				InvoiceCurrency = exportOperation.Currency
			};
		}

		GoodsShipmentType80 GetPopulatedGoodsShipment()
		{
			var goodsShipment = provider.GoodsShipment;
			return goodsShipment == null ? null : new GoodsShipmentType80
			{
				NatureOfTransaction = goodsShipment.NatureOfTransaction,
				DeliveryTerms = GetPopulatedDeliveryTerms<DeliveryTermsType80>(goodsShipment.DeliveryTerms),
				Warehouse = GetPopulatedWarehouse<WarehouseType80>(goodsShipment.Warehouse),
				Consignment = GetPopulatedConsignment(goodsShipment.Consignment),
				GoodsItem = goodsShipment.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		ConsignmentType80 GetPopulatedConsignment(IComplXAESConsignment consignment)
		{
			return consignment == null ? null : new ConsignmentType80
			{
				InlandModeOfTransport = consignment.InlandModeOfTransport,
				ModeOfTransportAtTheBorder = consignment.ModeOfTransportAtBorder,
				ActiveBorderTransportMeans = GetPopulatedTransportMediumInfoCommon<ActiveBorderTransportMeansType80>(consignment.ActiveBorderTransportMeans),
				TransportCharges = GetPopulatedTransportCharges<TransportChargesType80>(consignment.TransportChargesMoP)
			};
		}

		GoodsItemType80 GetPopulatedLine(IComplXAESLine line)
		{
			return line == null ? null : new GoodsItemType80
			{
				DeclarationGoodsItemNumber = line.SequenceNumber,
				StatisticalValue = line.StatisticalValue.Round(MaxDecimals2),
				Origin = GetPopulatedOrigin<OriginType80>(line.Origin),
				Commodity = GetPopulatedCommodity(line.Commodity),
				PreviousDocument = line.PreviousDocuments.ConvertToCollection(GetPopulatedCommonDocument<PreviousDocumentType80>),
				SupportingDocument = line.SupportingDocuments.ConvertToCollection(GetPopulatedSupportingDocument)
			};
		}

		CommodityType80 GetPopulatedCommodity(IComplXAESCommodity commodity)
		{
			return commodity == null ? null : new CommodityType80
			{
				GoodsMeasure = GetPopulatedCommonGoodsMeasure<GoodsMeasureType80>(commodity.GoodsMeasure)
			};
		}

		SupportingDocumentType80 GetPopulatedSupportingDocument(IComplXAESSupportingDocument documentProvider)
		{
			var document = GetPopulatedCommonLineNumberDocument<SupportingDocumentType80>(documentProvider);
			if (document != null)
			{
				GetPopulatedCommonSupportingDocumentExtraFields(documentProvider.CommonSupportingDocumentExtraFields, document);
			}
			return document;
		}
	}
}
