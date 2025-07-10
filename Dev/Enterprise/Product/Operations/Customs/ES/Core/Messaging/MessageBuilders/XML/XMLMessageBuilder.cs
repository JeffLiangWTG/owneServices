using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class XMLMessageBuilder<TProvider, TObject> : MessageBuilder<TProvider>
	where TProvider : IESEDIMessageCollectionProvider
{
	protected XMLMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	public readonly int OrgHeaderNameMaxLength = 70;
	public readonly int OrgHeaderAddressMaxLength = 35;

	public override ZString UnsignedMessageText
	{
		get
		{
			if (unsignedMessageText.IsEmpty)
			{
				var settings = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = true
				};

				var xmlObject = GenerateXMLMessage();
				var xmlCleaner = new XmlObjectCleaner();
				xmlCleaner.RemoveEmptyXmlElements(xmlObject);
				unsignedMessageText = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeWithoutNamespaces(xmlObject, settings);
				if (t2lMessages.Contains(MessageType) || MessageType == DeclarationMessageTypeList.Codes.InboxPendingList)
				{
					unsignedMessageText = unsignedMessageText.Replace("<q1:", "<").Replace("</q1:", "</").Replace("xmlns:q1", "xmlns");
				}

				unsignedMessageText = RemoveExtraDataFromUnsignedMessageText(unsignedMessageText);
#if NET
				unsignedMessageText = unsignedMessageText.Replace(" xmlns=\"\"", "");
#endif
				return unsignedMessageText;
			}
			else
			{
				return base.UnsignedMessageText;
			}
		}
	}

	readonly List<string> t2lMessages = new List<string>()
	{
		DeclarationMessageTypeList.Codes.T2lAnnex,
		DeclarationMessageTypeList.Codes.T2lExpedition,
		DeclarationMessageTypeList.Codes.T2lExpeditionAmendment,
		DeclarationMessageTypeList.Codes.T2lReception,
		DeclarationMessageTypeList.Codes.T2lReceptionAmendment,
		DeclarationMessageTypeList.Codes.T2lClearance
	};

	protected virtual ZString RemoveExtraDataFromUnsignedMessageText(ZString unsignedMessageText) => unsignedMessageText;

	protected override ZString SignMessageText(ZString messageText)
	{
		var message = SOAPEnvelope(messageText);
		if (signedMessageTypes.Contains(MessageType))
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(message);
			message = XMLSignature.Sign(xmlDoc, Certificate);
			XDocument finalDoc = XDocument.Parse(message);
			using (var sw = new MemoryStream())
			using (var strw = new StreamWriter(sw, System.Text.UTF8Encoding.UTF8))
			{
				finalDoc.Save(strw);
				message = System.Text.UTF8Encoding.UTF8.GetString(sw.ToArray());
			}
		}
		return RemoveUTF8BOM(message);
	}

	public static string RemoveUTF8BOM(string message)
	{
		var stringBuilder = new StringBuilder();
		foreach (char c in message)
		{
			if (c != '\uFEFF')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	ZString SOAPEnvelope(ZString innerText)
	{
		return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
{innerText}
  </soapenv:Body>
</soapenv:Envelope>";
	}

	readonly List<string> signedMessageTypes = new List<string>()
	{
		DeclarationMessageTypeList.Codes.EntrySummaryDeclaration,
		DeclarationMessageTypeList.Codes.EnsAmendment,
		DeclarationMessageTypeList.Codes.EnsDeviationRequest,
		DeclarationMessageTypeList.Codes.T2lAnnex,
		DeclarationMessageTypeList.Codes.T2lExpedition,
		DeclarationMessageTypeList.Codes.T2lExpeditionAmendment,
		DeclarationMessageTypeList.Codes.T2lReception,
		DeclarationMessageTypeList.Codes.T2lReceptionAmendment,
		DeclarationMessageTypeList.Codes.T2lClearance
	};
	protected abstract TObject GenerateXMLMessage();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "TimeZone Id code")]
	const string TimeZoneSpain = "Central European Standard Time";

	protected ZDateTime CET
	{
		get
		{
			if (!cet.HasValue)
			{
				cet = TimeZoneInfo.ConvertTime(ZDateTime.UtcNow.ToDateTime(), TimeZoneInfo.FindSystemTimeZoneById(TimeZoneSpain));
			}

			return cet.Value;
		}
	}
	ZDateTime? cet;

	protected string TimeOfCET => CET.ToCustomsFormatString(CustomsDateTimeExtension.TimeFormatSpain);
	protected string DateOfCET => CET.ToCustomsFormatDateString();

	IRandomGenerator RandomGenerator => randomGenerator ??= new RandomGenerator();
	IRandomGenerator randomGenerator;

	protected string TransactionId => "ES" + CET.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatTransaction) + RandomGenerator.Generate(10000);

	protected string TransactionIdLength14 => CET.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatTransaction14);

	protected X509Certificate2 Certificate => GetCertificate();
	X509Certificate2 GetCertificate()
	{
		byte[] certifacteBytes = provider.CertificateBytes;
		var certificatePass = provider.DecryptedCertificatePassphrase;
		var certificate2 = new X509Certificate2(certifacteBytes, certificatePass);
		return certificate2;
	}

	#region Common Builder Population Methods

	protected T GetPopulatedCustomsOffice<T>(ZString office)
		where T : ICommonCustomOffice, new()
	{
		var customsOffice = default(T);
		if (provider != null)
		{
			customsOffice = new T()
			{
				ReferenceNumber = office,
			};
		}
		return customsOffice;
	}

	protected T GetPopulatedPackageNumbers<T>(IPackageCommonNumbers provider) where T : ICommonInternalPackageQuantities, new()
	{
		var package = GetPopulatedPackage<T>(provider);
		if (package != null)
		{
			package.PackagesNumber = provider.PackagesQty;
			package.PiecesNumber = provider.PiecesQty;
		}
		return package;
	}

	protected T GetPopulatedPackage<T>(IPackageCommon packageProvider) where T : ICommonInternalPackage, new()
	{
		var package = default(T);
		if (packageProvider != null)
		{
			package = new T()
			{
				PackageType = packageProvider.PackageType,
				Marks = packageProvider.Marks,
			};
		}
		return package;
	}

	protected T GetPopulatedCommonPackage<T>(ICommonPackageWithSequenceAndPackNum packageProvider)
		where T : IPackageWithSequenceAndPackNumCommon, new()
	{
		var package = GetPopulatedPackage<T>(packageProvider);
		if (package != null)
		{
			package.SequenceNumber = packageProvider.SequenceNumber;
			package.NumberOfPackages = packageProvider.NumberOfPackages;
		}
		return package;
	}

	protected T GetPopulatedCommonVehicle<T>(IVehicleCommon vehicle) where T : ICommonVehiclePack, new()
	{
		return new T()
		{
			Chassis = vehicle.Chassis,
			Brand = vehicle.Brand,
			Model = vehicle.Model
		};
	}

	protected T GetPopulatedAddressInformationCommon<T>(IPartyProvider provider, bool truncate = false) where T : IOrgAddressInfoCommon, new()
	{
		var declaration = GetPopulatedAddressInformationDeclarantCommon<T>(provider, truncate);
		if (declaration != null)
		{
			declaration.Address = truncate ? provider.Address.Left(OrgHeaderAddressMaxLength) : provider.Address;
			declaration.PostCode = provider.PostCode;
			declaration.City = provider.City;
			declaration.Country = provider.Country;
		}
		return declaration;
	}

	protected T GetPopulatedAddressCommon<T>(IPartyAddressProvider addressProvider)
		where T : IOrgAddressCommon, new()
	{
		var address = default(T);
		if (addressProvider != null)
		{
			address = new T()
			{
				StreetAndNumber = addressProvider.Address,
				Country = addressProvider.Country,
				PostCode = addressProvider.PostCode,
				City = addressProvider.City,
			};
		}
		return address;
	}

	protected T GetPopulatedAddressInformationDeclarantCommon<T>(IPartyNameProvider provider, bool truncate = false) where T : IOrgAddressInfoDeclarantCommon, new()
	{
		var declaration = default(T);
		if (provider != null)
		{
			declaration = new T()
			{
				Id = provider.Id,
				Name = truncate ? provider.Name.Left(OrgHeaderNameMaxLength) : provider.Name,
			};
		}
		return declaration;
	}

	protected T GetPopulatedAddressInformationIdCommon<T>(IPartyIdProvider provider) where T : IOrgAddressInfoIdCommon, new()
	{
		var declaration = default(T);
		if (provider != null)
		{
			declaration = new T()
			{
				Id = provider.Id,
			};
		}
		return declaration;
	}

	protected T GetPopulatedPartyIdProviderWithContactPerson<T, TContactPerson>(IPartyIdProviderWithContactPerson providerDeclarant)
		where T : IOrgAddressInfoIdCommonWithContactPerson, new()
		where TContactPerson : ICommonContactPerson, new()
	{
		var declarant = GetPopulatedAddressInformationIdCommon<T>(providerDeclarant);
		if (declarant != null)
		{
			declarant.ContactPerson = GetPopulatedContactPerson<TContactPerson>(providerDeclarant.ContactPerson);
		}
		return declarant;
	}

	protected T GetPopulatedCommonRepresentativeWithContactPerson<T, TContactPerson>(ICommonRepresentativeWithContactPerson providerRepresentative)
		where T : IOrgAddressInfoIdCommonWithStatusAndContactPerson, new()
		where TContactPerson : ICommonContactPerson, new()
	{
		var representative = GetPopulatedAddressInformationIdCommon<T>(providerRepresentative);
		if (representative != null)
		{
			representative.Status = providerRepresentative.Status;
			representative.ContactPerson = GetPopulatedContactPerson<TContactPerson>(providerRepresentative.ContactPerson);
		}
		return representative;
	}

	protected T GetPopulatedContactPerson<T>(IPartyContactProvider contactInfo)
		where T : ICommonContactPerson, new()
	{
		var contactPerson = default(T);
		if (contactInfo != null)
		{
			contactPerson = new T()
			{
				Name = contactInfo.Name,
				EmailAddress = contactInfo.Email,
				PhoneNumber = contactInfo.PhoneNumber
			};
		}
		return contactPerson;
	}

	protected T GetPopulatedDocumentCommon<T>(IDocumentsCommon doc) where T : ICommonDocument, new()
	{
		var declarationDoc = default(T);
		if (doc != null)
		{
			declarationDoc = new T
			{
				Name = doc.Name,
				Number = doc.Number
			};
		}
		return declarationDoc;
	}

	protected T GetPopulatedTransportMediumInfoCommon<T>(ITransportMediumInfoCommon transport) where T : ICommonTransportMediumInfo, new()
	{
		var transportMedium = default(T);
		if (transport != null)
		{
			transportMedium = new T
			{
				TransportMode = transport.TransportMode,
				TransportId = transport.TransportId,
				TransportNationality = transport.TransportNationality,
			};
		}
		return transportMedium;
	}

	protected T GetPopulatedCommodityCodeCommon<T>(ICommodityCodeCommon commodityProvider) where T : ICommonCommodityCode, new()
	{
		var commodityCode = default(T);
		if (commodityProvider != null)
		{
			commodityCode = new T
			{
				HarmonizedSystemSubHeadingCode = commodityProvider.TariffCode,
				CombinedNomenclatureCode = commodityProvider.TariffCodeCombined
			};
		}
		return commodityCode;
	}

	protected T GetPopulatedCommonAdditionalCode<T>(ICommonAdditionalCode providerAdditionalProcedure)
		where T : IAdditionalCodeCommon, new()
	{
		var additionalProcedure = default(T);
		if (providerAdditionalProcedure != null)
		{
			additionalProcedure = new T()
			{
				SequenceNumber = providerAdditionalProcedure.SequenceNumber,
				Code = providerAdditionalProcedure.Code,
			};
		}
		return additionalProcedure;
	}

	protected T GetPopulatedGoodsMeasureCommon<T>(IGoodsMeasureCommon goodsMeasureProvider) where T : ICommonGoodsMeasure, new()
	{
		var goodsMeasure = default(T);
		if (goodsMeasureProvider != null)
		{
			goodsMeasure = new T
			{
				GrossWeight = goodsMeasureProvider.GrossWeight,
				NetWeight = goodsMeasureProvider.NetWeight
			};
		}
		return goodsMeasure;
	}

	protected T GetPopulatedDeclarationAuthorisation<T>(ICommonAuthorisation providerAuthorisation)
		where T : IAuthorisationCommon, new()
	{
		var authorisation = default(T);
		if (providerAuthorisation != null)
		{
			authorisation = new T()
			{
				SequenceNumber = providerAuthorisation.SequenceNumber,
				Type = providerAuthorisation.Type,
				ReferenceNumber = providerAuthorisation.ReferenceNumber,
				Holder = providerAuthorisation.Holder,
			};
		}
		return authorisation;
	}

	protected T GetPopulatedDeliveryTerms<T>(ICommonDeliveryTerms providerDeliveryTerms)
	where T : IDeliverytermsDeclarationCommon, new()
	{
		var deliveryTerms = default(T);
		if (providerDeliveryTerms != null)
		{
			deliveryTerms = new T()
			{
				Incoterm = providerDeliveryTerms.Incoterm,
				UNLCode = providerDeliveryTerms.UNLCode,
				IncotermLocation = providerDeliveryTerms.IncotermLocation,
				DeliveryCountry = providerDeliveryTerms.DeliveryCountry,
				DeliveryText = providerDeliveryTerms.DeliveryText,
			};
		}
		return deliveryTerms;
	}

	protected T GetPopulatedWarehouse<T>(IWarehouseCommon providerWarehouse)
		where T : IWarehouseDeclarationCommon, new()
	{
		var warehouse = default(T);
		if (providerWarehouse != null)
		{
			warehouse = new T()
			{
				Type = providerWarehouse.Type,
				Identifier = providerWarehouse.Identifier,
			};
		}
		return warehouse;
	}

	protected T GetPopulatedAdditionalSupplyActor<T>(ICommonAdditionalSupplyChainActorSeqNum supplyChainActorProvider)
		where T : IAdditionalSupplyChainActorWithSeqNumCommon, new()
	{
		var supplyChainActor = default(T);
		if (supplyChainActorProvider != null)
		{
			supplyChainActor = new T()
			{
				SequenceNumber = supplyChainActorProvider.SequenceNumber,
				Role = supplyChainActorProvider.Role,
				Id = supplyChainActorProvider.Id,
			};
		}
		return supplyChainActor;
	}

	protected T GetPopulatedCommonDocumentSequenceNumber<T>(ICommonDocumentSequenceNumber commonDocumentProvider)
		where T : IDocumentSequenceNumberCommon, new()
	{
		var commonDocument = GetPopulatedDocumentCommon<T>(commonDocumentProvider);
		if (commonDocument != null)
		{
			commonDocument.SequenceNumber = commonDocumentProvider.SequenceNumber;
		}
		return commonDocument;
	}

	protected T GetPopulatedCommonLocationOfGoods<T, TCustomsOffice, TGNSS, TEconomicOperator, TAddress, TPostCodeAddress>(ICommonLocationOfGoods providerLocationOfGoods)
	where T : ILocationOfGoodsCommon, new()
	where TCustomsOffice : ICommonCustomOffice, new()
	where TGNSS : IGNSSCommon, new()
	where TEconomicOperator : IEconomicOperatorCommon, new()
	where TAddress : IOrgAddressCommon, new()
	where TPostCodeAddress : IPostcodeAddressCommon, new()
	{
		var locationOfGoods = default(T);
		if (providerLocationOfGoods != null)
		{
			locationOfGoods = new T()
			{
				LocationType = providerLocationOfGoods.LocationType,
				LocationQualifier = providerLocationOfGoods.LocationQualifier,
				LocationId = providerLocationOfGoods.LocationId,
				LocationAdditionalId = providerLocationOfGoods.LocationAdditionalId,
				LocationUNloCode = providerLocationOfGoods.LocationUNloCode,
				LocationCustomOffice = GetPopulatedCustomsOffice<TCustomsOffice>(providerLocationOfGoods.LocationCustomOffice),
				LocationGNSS = GetPopulatedGNSS(),
				LocationEconomicOperator = GetPopulatedLocationEconomicOperator(),
				LocationAddress = GetPopulatedAddressCommon<TAddress>(providerLocationOfGoods.LocationAddress),
				LocationPostcodeAddress = GetPopulatedPostCodeAddress(),
			};
		}
		return locationOfGoods;

		TEconomicOperator GetPopulatedLocationEconomicOperator()
		{
			var locationEconomicOperator = default(TEconomicOperator);
			var providerLocationEconomicOperatorId = providerLocationOfGoods.LocationEconomicOperatorId;
			if (!providerLocationEconomicOperatorId.IsEmpty)
			{
				locationEconomicOperator = new TEconomicOperator()
				{
					Id = providerLocationEconomicOperatorId
				};
			}
			return locationEconomicOperator;
		}

		TGNSS GetPopulatedGNSS()
		{
			var gnss = default(TGNSS);
			var providerGNSS = providerLocationOfGoods.LocationGNSS;
			if (providerGNSS != null)
			{
				gnss = new TGNSS()
				{
					Latitude = providerGNSS.Latitude,
					Longitude = providerGNSS.Longitude,
				};
			}
			return gnss;
		}

		TPostCodeAddress GetPopulatedPostCodeAddress()
		{
			var postcodeAddress = default(TPostCodeAddress);
			var providerPostcodeAddress = providerLocationOfGoods.LocationPostcodeAddress;
			if (providerPostcodeAddress != null)
			{
				postcodeAddress = new TPostCodeAddress()
				{
					HouseNumber = providerPostcodeAddress.HouseNumber,
					PostCode = providerPostcodeAddress.PostCode,
					Country = providerPostcodeAddress.Country
				};
			}
			return postcodeAddress;
		}
	}

	protected T GetPopulatedCommonTransportEquipment<T, TGoodsReference>(ICommonTransportEquipment providerTransportEquipment)
	where T : ITransportEquipmentCommon, new()
	where TGoodsReference : IGoodsReferenceCommon, new()
	{
		var transportEquipment = default(T);
		if (providerTransportEquipment != null)
		{
			var goodsReferenceCollection = new Collection<IGoodsReferenceCommon>();
			foreach (var goodsReference in providerTransportEquipment.GoodsReference.ConvertToCollection(GetPopulatedGoodsReference) ?? Enumerable.Empty<TGoodsReference>())
			{
				goodsReferenceCollection.Add(goodsReference);
			}

			transportEquipment = new T()
			{
				SequenceNumber = providerTransportEquipment.SequenceNumber,
				ContainerNumber = providerTransportEquipment.ContainerNumber,
				GoodsReference = goodsReferenceCollection,
			};
		}
		return transportEquipment;

		TGoodsReference GetPopulatedGoodsReference(ICommonGoodsReference providerGoodsReference)
		{
			var goodsReference = default(TGoodsReference);
			if (providerGoodsReference != null)
			{
				goodsReference = new TGoodsReference
				{
					SequenceNumber = providerGoodsReference.SequenceNumber,
					GoodsItemNumber = providerGoodsReference.GoodsItemNumber,
				};
			}
			return goodsReference;
		}
	}

	protected T GetPopulatedCommonGoodsMeasureWithSupUnitsAndSpecified<T>(ICommonGoodsMeasureWithSupUnitsAndSpecified providerGoodsMeasure)
		where T : IGoodsMeasureCommonWithSpecifiedWithSupUnits, new()
	{
		var goodMeasure = GetPopulatedCommonGoodsMeasure<T>(providerGoodsMeasure);
		if (goodMeasure != null)
		{
			goodMeasure.SupplementaryUnits = providerGoodsMeasure.SupplementaryUnits;
			goodMeasure.SupplementaryUnitsSpecified = providerGoodsMeasure.SupplementaryUnitsSpecified;
		}
		return goodMeasure;
	}

	protected T GetPopulatedCommonGoodsMeasure<T>(ICommonGoodsMeasureWithSpecified providerGoodMeasure)
		where T : IGoodsMeasureCommonWithSpecified, new()
	{
		var goodMeasure = default(T);
		if (providerGoodMeasure != null && (providerGoodMeasure.GrossWeightSpecified || providerGoodMeasure.NetWeightSpecified))
		{
			goodMeasure = new T()
			{
				GrossWeight = providerGoodMeasure.GrossWeight,
				GrossWeightSpecified = providerGoodMeasure.GrossWeightSpecified,
				NetWeight = providerGoodMeasure.NetWeight,
				NetWeightSpecified = providerGoodMeasure.NetWeightSpecified,
			};
		}
		return goodMeasure;
	}

	#endregion
}
