using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DG;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class G5CommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : IG5GenericMessageDataProvider
{
	public G5CommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected EnvelopeG5EntDg GetPopulatedEnvelope()
	{
		return new EnvelopeG5EntDg()
		{
			Sender = provider.SenderId,
			MessageId = TransactionId,
			PreparationDate = DateOfCET + TimeOfCET,
			Recipient = provider.IsTest
								? ExecEnvironmentDe.PreAeat
								: ExecEnvironmentDe.EsAeat
		};
	}

	protected HeaderDg GetPopulatedHeader(IG5CommonHeader header)
	{
		return header == null ? null : new HeaderDg
		{
			OriginCustomsOffice = header.OriginCustomsOffice,
			LocationOfGoodsAtOrigin = GetPopulatedLocationOfGoods(header.GoodsLocationOrigin),
			DestinationCustomsOffice = header.DestinationCustomsOffice,
			LocationOfGoodsAtDestination = GetPopulatedLocationOfGoods(header.GoodsLocationDestination),
			TsWarehouse = header.TSWarehouse,
			ArrivalTransportMeans = GetPopulatedTransportMeans(header.ArrivalTransportMeans),
			TransportDocument = GetPopulatedDocumentCommon<TransportDocumentDe>(header.TransportDocument),
			Lrn = header.LRN,
			Consignor = GetPopulatedActor<ActorDe>(header.Consignor),
			Consignee = GetPopulatedActor<ActorDe>(header.Consignee),
			Declarant = GetPopulatedActor<ActorDe>(header.Declarant),
			Representative = GetPopulatedRepresentative(header.Representative),
			SupportingDocument = header.SupportingDocuments.ConvertToCollection(GetPopulatedDocumentCommon<SupportingDocumentDe>),
			AdditionalInformation = header.AdditionalInfos.ConvertToCollection(GetPopulatedDocumentCommon<AdditionalInformationDe>),
			TotalGoodsItems = header.TotalLinesNum,
			TotalPackages = header.TotalPackagesNum,
			TotalGrossMass = header.TotalGrossWeightInKG
		};
	}

	LocationOfGoodsDe GetPopulatedLocationOfGoods(IG5LocationGoods goodsLocation)
	{
		return goodsLocation == null ? null : new LocationOfGoodsDe
		{
			NationalLocation = goodsLocation.NationalLocation,
			GenericLocation = GetPopulatedGenericLocation(goodsLocation.GenericLocation)
		};

		GenericLocationDe GetPopulatedGenericLocation(IGenericLocation genericLocation)
		{
			return genericLocation == null ? null : new GenericLocationDe
			{
				Type = genericLocation.Type,
				Qualifier = genericLocation.Qualifier,
				Address = GetPopulatedAddressCommon<GenericLocationDeAddress>(genericLocation.Address),
				Coded = GetGenericLocatioCoded(genericLocation.Coded)
			};
		}

		GenericLocationDeCoded GetGenericLocatioCoded(ICodedGenericLocation coded)
		{
			return coded == null ? null : new GenericLocationDeCoded
			{
				UnLocCode = coded.UNLOCOCode,
				AuthorisationNumber = coded.AuthorisationNumber,
				AdditionalIdentifier = coded.AdditionalId,
				CustomsOffice = GetPopulatedLocationCustomOffice(coded.CustomsOffice),
				EconomicOperator = GetPopulatedLocationEconomicOperator(coded.EconomicOperator),
				Gps = GetPopulatedGPS(coded.GPS)
			};
		}

		GenericLocationDeCodedCustomsOffice GetPopulatedLocationCustomOffice(ZString customOffice)
		{
			return customOffice.IsEmpty ? null : new GenericLocationDeCodedCustomsOffice
			{
				Reference = customOffice
			};
		}

		GenericLocationDeCodedEconomicOperator GetPopulatedLocationEconomicOperator(ZString economicOperator)
		{
			return economicOperator.IsEmpty ? null : new GenericLocationDeCodedEconomicOperator
			{
				IdentifierNumber = economicOperator
			};
		}

		GenericLocationDeCodedGps GetPopulatedGPS(ICommonGNSS gps)
		{
			return gps == null ? null : new GenericLocationDeCodedGps
			{
				Latitude = gps.Latitude,
				Longitude = gps.Longitude
			};
		}
	}

	TransportMeansDe GetPopulatedTransportMeans(ICommonArrivalTransportMeans transportMeans)
	{
		return transportMeans == null ? null : new TransportMeansDe
		{
			IdentificationNumber = transportMeans.Id,
			TypeOfIdentification = transportMeans.Type
		};
	}

	protected T GetPopulatedActor<T>(IG5PartyInfo partyInfo)
		where T : IG5Actor, new()
	{
		return partyInfo == null ? default(T) : new T()
		{
			Id = partyInfo.Id,
			Name = partyInfo.Name,
			Type = GetTypeOfPerson(),
			FullAddress = GetActorFullAddress(),
			Communication = GetActorCommunication()
		};

		TypeOfPersonDe GetTypeOfPerson()
		{
			switch (partyInfo.Type)
			{
				case "1":
					return TypeOfPersonDe.Item1;
				case "2":
					return TypeOfPersonDe.Item2;
				default:
					return TypeOfPersonDe.Item3;
			}
		}

		FullAddressDe GetActorFullAddress()
		{
			return new FullAddressDe
			{
				Street = partyInfo.Street,
				StreetAddtionalLine = partyInfo.StreetAddLine,
				Number = partyInfo.Number,
				PoBox = partyInfo.POBox,
				SubDivision = partyInfo.State,
				Country = partyInfo.Country,
				PostCode = partyInfo.PostCode,
				City = partyInfo.City,
			};
		}

		CommunicationDe GetActorCommunication()
		{
			return new CommunicationDe
			{
				Type = partyInfo.CommunicationType,
				Identifier = partyInfo.CommunicationId
			};
		}
	}

	protected RepresentativeDe GetPopulatedRepresentative(IG5RepresentativeInfo representative)
	{
		var actor = GetPopulatedActor<RepresentativeDe>(representative);
		if (actor != null)
		{
			actor.Status = representative.Status == "2" ? StatusDe.Item2 : StatusDe.Item3;
		}
		return actor;
	}

	protected GoodsItemDg GetPopulatedLine(IG5CommonLine line)
	{
		return new GoodsItemDg
		{
			GoodsItemNumber = line.LineNumber,
			PreviousDocument = GetPopulatedPreviousDocument(line.PreviousDocument),
			Packages = line.PackagesNum,
			Packaging = line.Packages.ConvertToCollection(GetPopulatedG5Package),
			GrossMass = line.GrossWeightInKG,
			TransportDocument = GetPopulatedDocumentCommon<TransportDocumentDe>(line.TransportDocument),
			Ucr = line.UCRCode,
			CommodityCode = line.CommodityCode,
			DescriptionOfGoods = line.GoodsDescription,
			CusCode = line.CusCode,
			TransportEquipment = line.TransportEquipments.ConvertToCollection(GetPopulatedTransportEquipment),
			PresentationDateAtOrigin = line.PresentationDateAtOrigin.ToCustomsFormatDateString(),
			SupportingDocument = line.SupportingDocuments.ConvertToCollection(GetPopulatedDocumentCommon<SupportingDocumentDe>),
			AdditionalInformation = line.AdditionalInfo.ConvertToCollection(GetPopulatedDocumentCommon<AdditionalInformationDe>)
		};
	}

	PreviousDocumentDe GetPopulatedPreviousDocument(IG5PreviousDocument doc)
	{
		return doc == null ? null : new PreviousDocumentDe
		{
			PreviousTsd = GetPopulatedPreviousTSD(doc.PreviousTSD),
			GenericPreviousDocument = GetPopulatedGenericPreviousDocument(doc.PreviousGeneric)
		};

		PreviousTsdDe GetPopulatedPreviousTSD(IG5PreviousTSD previousTSD)
		{
			return previousTSD == null ? null : new PreviousTsdDe
			{
				Mrn = previousTSD.MRN,
				TransportMeans = GetPopulatedTransportMeans(previousTSD.TransportMeans),
				TransportDocument = GetPopulatedDocumentCommon<TransportDocumentDe>(previousTSD.TransportDocument),
				GoodsItemIdentifier = previousTSD.GoodsItemId
			};
		}

		GenericPreviousDocumentDe GetPopulatedGenericPreviousDocument(ICommonDocumentGoodsItemId previousDoc)
		{
			var document = GetPopulatedDocumentCommon<GenericPreviousDocumentDe>(previousDoc);
			if (document != null)
			{
				document.GoodsItemIdentifier = previousDoc.GoodsItemId;
			}
			return document;
		}
	}

	PackagingDe GetPopulatedG5Package(IInternalPackageIdentificationCommon package)
	{
		return new PackagingDe
		{
			TypeOfPackages = package.ElementsType,
			NumberOfPackages = package.NumberOfElements.ToZInt(),
			ShippingMarks = package.Tag
		};
	}

	TransportEquipmentDe GetPopulatedTransportEquipment(IG5TransportEquipment equipment)
	{
		return new TransportEquipmentDe
		{
			ContainerIdentificationNumber = equipment.Id,
			ContainerPackedStatus = equipment.PackedStatus,
			SealIdentifer = equipment.SealIds.ConvertToStringCollection()
		};
	}
}
