using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class G3CommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
		where TProvider : IG3CommonMessageDataProvider
	{
		public G3CommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected T GetPopulatedMessage<T>(IG3Message message)
			where T : IG3MessageTd, new()
		{
			return new T()
			{
				Sender = message.Sender,
				MessageId = TransactionId,
				PreparationDate = DateOfCET + TimeOfCET,
				Recipient = provider.IsTest
					? ExecEnvironmentTd.PreAeat
					: ExecEnvironmentTd.EsAeat
			};
		}

		protected DeclarantTd GetPopulatedG3Declarant(IG3Declarant declarant)
		{
			return declarant == null ? null : new DeclarantTd()
			{
				IdNumber = declarant.IdNumber,
				Name = declarant.Name,
				FullAddress = GetPopulatedG3FullAddress(declarant.FullAddress),
				Communication = GetPopulatedG3Communication(declarant.Communication),
			};
		}

		protected FullAddressTd GetPopulatedG3FullAddress(IG3FullAddress address)
		{
			return address == null ? null : new FullAddressTd()
			{
				Street = address.Street,
				StreetAddtionalLine	= address.StreetAddLine,
				Number = address.Number,
				PoBox = address.POBox,
				SubDivision = address.SubDivision,
				Country = address.Country,
				PostCode = address.PostCode,
				City = address.City,
			};
		}

		protected CommunicationTd GetPopulatedG3Communication(IG3Communication communication)
		{
			return communication == null ? null : new CommunicationTd()
			{
				CommType = communication.CommunicationType,
				CommIdentifier = communication.CommunicationId,
			};
		}

		protected RepresentativeTd GetPopulatedG3Representative(IG3Representative representative)
		{
			return representative == null ? null : new RepresentativeTd()
			{
				IdNumber = representative.IdNumber,
				Name = representative.Name,
				Status = representative.Status == "2" ? StatusTd.Item2 : StatusTd.Item3,
				Communication = GetPopulatedG3Communication(representative.Communication),
			};
		}

		protected PreviousDocumentTd GetPopulatedG3DocumentWithGoodsItemId(ICommonDocumentGoodsItemId document)
		{
			return document == null ? null : new PreviousDocumentTd()
			{
				PrevDocType = document.Name,
				PrevDocRefNum = document.Number,
				PrevDocGoodsItem = document.GoodsItemId,
			};
		}

		protected TransportEquipmentTd GetPopulatedG3TransportEquipment(ZString container)
		{
			return new TransportEquipmentTd()
			{
				Container = container,
			};
		}

		protected LocationOfGoodsTd GetPopulatedG3LocationOfGoods(IGenericLocation location)
		{
			return location == null ? null : new LocationOfGoodsTd()
			{
				LocType = location.Type,
				LocQualifier = location.Qualifier,
				LocCoded = (location.Coded == null) ? null : new LocationOfGoodsTdLocCoded()
				{
					LocUnLocCode = location.Coded.UNLOCOCode,
					LocCustomsOffice = new LocationOfGoodsTdLocCodedLocCustomsOffice()
					{
						Reference = location.Coded.CustomsOffice,
					},
					LocGps = new LocationOfGoodsTdLocCodedLocGps()
					{
						Latitude = location.Coded.GPS?.Latitude,
						Longitude = location.Coded.GPS?.Longitude,
					},
					LocEconomicOperator = new LocationOfGoodsTdLocCodedLocEconomicOperator()
					{
						IdNumber = location.Coded.EconomicOperator,
					},
					LocAuthNumber = location.Coded.AuthorisationNumber,
					LocAdditionalIdentifier = location.Coded.AdditionalId,
				},
				LocAddress = (location.Address == null) ? null : new LocationOfGoodsTdLocAddress()
				{
					LocStreetAndNumber = location.Address.Address,
					LocPostCode = location.Address.PostCode,
					LocCity = location.Address.City,
					LocCountry	= location.Address.Country,
				},
			};
		}
	}
}
