using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3PresV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class G3PresentGoodMessageBuilder : G3CommonMessageBuilder<IG3PresentGoodMessageDataProvider, G3PresV1Ent>
	{
		public G3PresentGoodMessageBuilder(IG3PresentGoodMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G3PresV1Ent GenerateXMLMessage()
		{
			return new G3PresV1Ent()
			{
				Message = GetPopulatedMessage<MessageTd>(provider.Message),
				Header = GetPopulatedHeader(provider.Header),
			};
		}

		protected HeaderTd GetPopulatedHeader(IG3PresentGoodHeader header)
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
				MasterConsignment = header.MasterConsignment.ConvertToCollection(GetPopulatedMasterConsignment),
			};
		}

		protected MasterConsignmentTd GetPopulatedMasterConsignment(IG3PresentGoodMasterConsignment consignment)
		{
			return consignment == null ? null : new MasterConsignmentTd()
			{
				PreviousDocument = consignment.PreviousDocument.ConvertToCollection(GetPopulatedG3DocumentWithGoodsItemId),
				TransportDocument = (consignment.TransportDocument == null) ? null : new TransportDocumentTd()
				{
					TransDocType = consignment.TransportDocument.Name,
					TransDocRefNum = consignment.TransportDocument.Number,
				},
				Receptacle = consignment.Receptacle,
				LocationOfGoods = GetPopulatedG3LocationOfGoods(consignment.LocationOfGoods),
				TransportEquipment = consignment.TransportEquipmentContainers.ConvertToCollection(GetPopulatedG3TransportEquipment),
				HouseConsignment = consignment.HouseConsignment.ConvertToCollection(GetPopulatedG3HouseConsignment)
			};
		}

		protected HouseConsignmentTd GetPopulatedG3HouseConsignment(IG3HouseConsignment consignment)
		{
			return consignment == null ? null : new HouseConsignmentTd()
			{
				PreviousDocument = consignment.PreviousDocument.ConvertToCollection(GetPopulatedG3DocumentWithGoodsItemId),
				TransportDocument = (consignment.TransportDocument == null) ? null : new TransportDocumentTd()
				{
					TransDocType = consignment.TransportDocument.Name,
					TransDocRefNum = consignment.TransportDocument.Number,
				},
			};
		}
	}
}
