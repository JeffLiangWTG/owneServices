using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class G3RevokeGoodMessageBuilder : G3CommonMessageBuilder<IG3RevokeGoodMessageDataProvider, G3RevokeV1Ent>
	{
		public G3RevokeGoodMessageBuilder(IG3RevokeGoodMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G3RevokeV1Ent GenerateXMLMessage()
		{
			return new G3RevokeV1Ent()
			{
				Message = GetPopulatedMessage<MessageTd>(provider.Message),
				Header = GetPopulatedHeader(provider.Header),
			};
		}

		protected HeaderTd GetPopulatedHeader(IG3RevokeGoodHeader header)
		{
			return header == null ? null : new HeaderTd()
			{
				Lrn = header.LRN,
				CustomsOffice = header.CustomsOffice,
				PersonPresentingGoods = header.PersonPresentingGoods,
				Declarant = GetPopulatedG3Declarant(header.Declarant),
				Representative = GetPopulatedG3Representative(header.Representative),
				DeclarationDate = header.DeclarationDate.ToCustomsFormatDateString(),
				PresentationDate = header.PresentationDate.ToCustomsFormatDateString(),
				MasterConsignment = header.MasterConsignment.ConvertToCollection(GetMasterConsignment),
			};
		}

		protected MasterConsignmentTd GetMasterConsignment(IG3RevokeMasterConsignment consignment)
		{
			return consignment == null ? null : new MasterConsignmentTd()
			{
				PreviousDocument = consignment.PreviousDocument.ConvertToCollection(GetPopulatedG3DocumentWithGoodsItemId),
				TransportDocument = (consignment.TransportDocument == null) ? null : new TransportDocumentTd()
				{
					TransDocType = consignment.TransportDocument.Name,
					TransDocRefNum = consignment.TransportDocument.Number
				},
				Receptacle = consignment.Receptacle,
				LocationOfGoods = GetPopulatedG3LocationOfGoods(consignment.LocationOfGoods),
				TransportEquipment = consignment.TransportEquipmentContainers.ConvertToCollection(GetPopulatedG3TransportEquipment),
				HouseConsignment = consignment.HouseConsignment.ConvertToCollection(GetPopulatedG3HouseConsignment)
			};
		}

		protected HouseConsignmentTd GetPopulatedG3HouseConsignment(IG3RevokeHouseConsignment consignment)
		{
			return consignment == null ? null : new HouseConsignmentTd()
			{
				PreviousDocument = consignment.PreviousDocument.ConvertToCollection(GetPopulatedG3DocumentWithGoodsItemId),
				TransportDocument = (consignment.TransportDocument == null) ? null : new TransportDocumentTd()
				{
					TransDocType = consignment.TransportDocument.Name,
					TransDocRefNum = consignment.TransportDocument.Number,
				},
				AdditionalInformation = (consignment.AdditionalInformation == null) ? null : new AdditionalInformationTd()
				{
					Code = consignment.AdditionalInformation.Number,
					Text = consignment.AdditionalInformation.Name,
				},
			};
		}
	}
}
