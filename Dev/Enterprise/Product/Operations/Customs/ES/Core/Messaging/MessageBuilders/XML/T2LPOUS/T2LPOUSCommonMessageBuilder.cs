using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class T2LPOUSCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : IT2LPOUSCommonDataProvider
{
	protected T2LPOUSCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	const string OperationType = "A";
	const string ReperesentativeStatus = "2";

	protected T GetPopulatedTypeCommon<T, TMessage, TPerson, TAddress, TContact>()
		where T : IT2LPOUSTypeCommon, new()
		where TMessage : IT2LPOUSMessageCommon, new()
		where TPerson : IT2LPOUSPersonReqPresCommon, new()
		where TAddress : IOrgAddressCommon, new()
		where TContact : IT2LPOUSContactPerson, new()
	{
		return new T()
		{
			Message = GetPopulatedMessage<TMessage>(),
			PersonReqPres = GetPopulatedPersonPresentingProofWithAddress<TPerson, TAddress, TContact>(provider.PersonReqPres),
			CustomOffice = provider.CustomsOffice,
		};
	}

	protected void GetPopulatedTypeCommonRequestAndReception<TAuthorisation, TRepresentativeData, TGoodsShipment, TPersonReqPres, TContactPersonInfo, TTransportEquipment, TAdditionalInfo, TCommonDoc, TGoodItems, TCommodityCode, TGoodsMeasure, TPackaging>(IT2LPOUSTypeCommonRequestAndReception typeCommon, IT2LPOUSRequestAndReceptionMessageDataProvider typeProvider)
		where TAuthorisation : IT2LPOUSCommonAuthorisation, new()
		where TRepresentativeData : IT2LPOUSRepresentativeData, new()
		where TGoodsShipment : IT2LPOUSCommonGoodsShipment, new()
		where TPersonReqPres : IT2LPOUSPersonReqPresCommon, new()
		where TContactPersonInfo : IT2LPOUSContactPerson, new()
		where TTransportEquipment : IT2LPOUSCommonTransportEquipment, new()
		where TAdditionalInfo : ICommonDocument, new()
		where TCommonDoc : ICommonDocument, new()
		where TGoodItems : IT2LPOUSGoodItem, new()
		where TCommodityCode : ICommonCommodityCode, new()
		where TGoodsMeasure : ICommonGoodsMeasure, new()
		where TPackaging : IT2LPOUSPackaging, new()
	{
		typeCommon.Authorisation = GetPopulatedAuthorisation(typeProvider.Authorisation);
		typeCommon.RepresentativeData = GetPopulatedRepresentative(typeProvider.Representative);
		typeCommon.GoodsShipment = GetPopulatedGoodsShipment(typeProvider.GoodsShipment);
		typeCommon.SendEmailU = typeProvider.SendEmailU;
		typeCommon.SendEmailExp = typeProvider.SendEmailExp;

		TAuthorisation GetPopulatedAuthorisation(IT2LPOUSAuthorisation authorisationProvider)
		{
			var authorisation = default(TAuthorisation);
			if (authorisationProvider != null)
			{
				authorisation = new TAuthorisation()
				{
					TypeOfAuthorisation = authorisationProvider.TypeOfAuthorisation,
					DecisionReferenceNumber = authorisationProvider.DecisionReferenceNumber,
					HolderOfTheAuthorisation = authorisationProvider.HolderOfTheAuthorisation,
				};
			}
			return authorisation;
		}

		TRepresentativeData GetPopulatedRepresentative(IT2LPOUSCommonPersonReqPres personReqPresProvider)
		{
			var representative = default(TRepresentativeData);
			if (personReqPresProvider != null)
			{
				representative = new TRepresentativeData()
				{
					RepresentativeData = GetPopulatedPersonPresentingProof<TPersonReqPres, TContactPersonInfo>(personReqPresProvider),
					Status = ReperesentativeStatus
				};
			}
			return representative;
		}

		TGoodsShipment GetPopulatedGoodsShipment(IT2LPOUSGoodsShipment shipmentProvider)
		{
			var shipment = default(TGoodsShipment);
			if (shipmentProvider != null)
			{
				var transportEquipmentCollection = new Collection<IT2LPOUSCommonTransportEquipment>();
				foreach (var transportEquipment in shipmentProvider.TransportEquipment.ConvertToCollection(GetPopulatedTransportEquipment<TTransportEquipment>) ?? Enumerable.Empty<TTransportEquipment>())
				{
					transportEquipmentCollection.Add(transportEquipment);
				}

				var additionalInfoCollection = new Collection<ICommonDocument>();
				foreach (var additionalInfo in shipmentProvider.AdditionalInformation.ConvertToCollection(GetPopulatedDocumentCommon<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
				{
					additionalInfoCollection.Add(additionalInfo);
				}

				var previousDocCollection = new Collection<ICommonDocument>();
				foreach (var previousDoc in shipmentProvider.PreviousDocument.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
				{
					previousDocCollection.Add(previousDoc);
				}

				var supportingDocCollection = new Collection<ICommonDocument>();
				foreach (var supportingDoc in shipmentProvider.SupportingDocument.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
				{
					supportingDocCollection.Add(supportingDoc);
				}

				var transportDocCollection = new Collection<ICommonDocument>();
				foreach (var transportDoc in shipmentProvider.TransportDocument.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
				{
					transportDocCollection.Add(transportDoc);
				}

				var additionalReferenceCollection = new Collection<ICommonDocument>();
				foreach (var additionalReference in shipmentProvider.AdditionalReference.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
				{
					additionalReferenceCollection.Add(additionalReference);
				}

				var goodItemsCollection = new Collection<IT2LPOUSGoodItem>();
				foreach (var goodItems in shipmentProvider.GoodItems.ConvertToCollection(GetPopulatedGoodItem<TGoodItems, TCommodityCode, TGoodsMeasure, TPackaging, TAdditionalInfo, TCommonDoc>) ?? Enumerable.Empty<TGoodItems>())
				{
					goodItemsCollection.Add(goodItems);
				}

				shipment = new TGoodsShipment()
				{
					TransportEquipment = transportEquipmentCollection,
					AdditionalInformation = additionalInfoCollection,
					PreviousDocument = previousDocCollection,
					SupportingDocument = supportingDocCollection,
					TransportDocument = transportDocCollection,
					AdditionalReference = additionalReferenceCollection,
					GoodItems = goodItemsCollection
				};
				GetPopulatedContainerIndication(shipmentProvider.ContainerIndication, shipment);
			}
			return shipment;
		}
	}

	protected T GetPopulatedMessage<T>()
		where T : IT2LPOUSMessageCommon, new()
	{
		var message = default(T);
		message = new T()
		{
			MessageIdentification = TransactionId,
			PreparationDateAndTime = CET.ToCustomsFormatDateStringyyyyMMddTHHmmss(),
			SendEmailL = provider.SendEmailL,
		};
		
		return message;
	}

	protected T GetPopulatedPersonPresentingProof<T, TContact>(IT2LPOUSCommonPersonReqPres personProvider)
		where T : IT2LPOUSPersonReqPresCommon, new()
		where TContact : IT2LPOUSContactPerson, new()
	{
		var person = GetPopulatedAddressInformationIdCommon<T>(personProvider);
		if (person != null)
		{
			person.ContactPerson = GetPopulatedContact(personProvider.ContactPerson);
		}
		return person;

		TContact GetPopulatedContact(IPartyContactProvider contactProvider)
		{
			var contact = default(TContact);
			if (contactProvider != null)
			{
				contact = new TContact()
				{
					Name = contactProvider.Name,
					Email = contactProvider.Email,
					PhoneNumber = contactProvider.PhoneNumber,
				};
			}
			return contact;
		}
	}

	protected T GetPopulatedPersonPresentingProofWithAddress<T, TAddress, TContact>(IT2LPOUSCommonPersonReqPresWithAddress personProvider)
		where T : IT2LPOUSPersonReqPresCommon, new()
		where TAddress : IOrgAddressCommon, new()
		where TContact : IT2LPOUSContactPerson, new()
	{
		var person = GetPopulatedPersonPresentingProof<T, TContact>(personProvider);
		if (person != null)
		{
			person.Address = GetPopulatedAddressCommon<TAddress>(personProvider.Address);
		}
		return person;
	}

	protected void GetPopulatedContainerIndication(IT2LPOUSCommonContainerIndicator containerIndicatorProvider, IT2LPOUSContainerIndicator containerIndicator)
	{
		if (containerIndicatorProvider != null)
		{
			containerIndicator.ContainerIndication = containerIndicatorProvider.IsContainerised;
		}
	}

	protected T GetPopulatedTransportEquipment<T>(IT2LPOUSTransportEquipment transportEquipmentProvider)
		where T : IT2LPOUSCommonTransportEquipment, new()
	{
		var transportEquipment = default(T);
		if (transportEquipmentProvider != null)
		{
			transportEquipment = new T()
			{
				ContainerIdentificationNumber = transportEquipmentProvider.ContainerIdentificationNumber,
				GoodsReference = transportEquipmentProvider.GoodsReference.ConvertToIntCollectionWithZero()
			};
		}
		return transportEquipment;
	}

	protected T GetPopulatedGoodsItem<T>(IT2LPOUSCommonGoodsItem goodsItemProvider)
		where T : IT2LPOUSGoodItemNumber, new()
	{
		var goodsItem = default(T);
		if (goodsItemProvider != null)
		{
			goodsItem = new T()
			{
				GoodsItemNumber = goodsItemProvider.GoodsItemNumber,
			};
		}
		return goodsItem;
	}

	protected T GetPopulatedGoodItem<T, TCommodityCode, TGoodsMeasure, TPackaging, TAdditionalInfo, TCommonDoc>(IT2LPOUSRequestAndReceptionGoodItem goodItemProvider)
		where T : IT2LPOUSGoodItem, new()
		where TCommodityCode : ICommonCommodityCode, new()
		where TGoodsMeasure : ICommonGoodsMeasure, new()
		where TPackaging : IT2LPOUSPackaging, new()
		where TAdditionalInfo : ICommonDocument, new()
		where TCommonDoc : ICommonDocument, new()
	{
		var goodItem = default(T);
		if (goodItemProvider != null)
		{
			var packagingCollection = new Collection<IT2LPOUSPackaging>();
			foreach (var packaging in goodItemProvider.Package.ConvertToCollection(GetPopulatedPackaging<TPackaging>) ?? Enumerable.Empty<TPackaging>())
			{
				packagingCollection.Add(packaging);
			}

			var additionalInfoCollection = new Collection<ICommonDocument>();
			foreach (var additionalInfo in goodItemProvider.AdditionalInformation.ConvertToCollection(GetPopulatedDocumentCommon<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfoCollection.Add(additionalInfo);
			}

			var previousDocCollection = new Collection<ICommonDocument>();
			foreach (var previousDoc in goodItemProvider.PreviousDocument.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
			{
				previousDocCollection.Add(previousDoc);
			}

			var supportingDocCollection = new Collection<ICommonDocument>();
			foreach (var supportingDoc in goodItemProvider.SupportingDocument.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
			{
				supportingDocCollection.Add(supportingDoc);
			}

			var additionalReferenceCollection = new Collection<ICommonDocument>();
			foreach (var additionalReference in goodItemProvider.AdditionalReference.ConvertToCollection(GetPopulatedDocumentCommon<TCommonDoc>) ?? Enumerable.Empty<TCommonDoc>())
			{
				additionalReferenceCollection.Add(additionalReference);
			}

			goodItem = new T()
			{
				GoodsItemNumber = goodItemProvider.GoodsItemNumber,
				CommodityCode = GetPopulatedCommodityCodeCommon<TCommodityCode>(goodItemProvider.CommodityCode),
				Description = goodItemProvider.Description,
				CusCode = goodItemProvider.CusCode,
				GoodsMeasure = GetPopulatedGoodsMeasureCommon<TGoodsMeasure>(goodItemProvider.GoodsMeasure),
				Package = packagingCollection,
				AdditionalInformation = additionalInfoCollection,
				PreviousDocument = previousDocCollection,
				SupportingDocument = supportingDocCollection,
				AdditionalReference = additionalReferenceCollection
			};
		}
		return goodItem;
	}

	protected void GetPopulatedPackageCommon(IT2LPOUSCommonPackaging packageProvider, IT2LPOUSPackagingCommon package)
	{
		if (packageProvider != null)
		{
			package.TypeOfPackages = packageProvider.TypeOfPackages;
			package.NumberOfPackages = packageProvider.NumberOfPackages;
			package.NumberOfPackagesValueSpecified = packageProvider.NumberOfPackagesValueSpecified;
		}
	}

	protected T GetPopulatedPackaging<T>(IT2LPOUSRequestAndReceptionPackaging packagingProvider) where T : IT2LPOUSPackaging, new()
	{
		var packaging = default(T);
		if (packagingProvider != null)
		{
			packaging = new T()
			{
				Marks = packagingProvider.Marks
			};
			GetPopulatedPackageCommon(packagingProvider, packaging);
		}
		return packaging;
	}

	protected void GetPopulatedProofInformationCommon<T>(IT2LPOUSCommonProofOperationInformationForT2LT2LF proofInformationProvider, IT2LPOUSProofOperationInformationForT2LT2LFCommon proofInformationCommon) where T : IT2LPOUSRequestedValididtyOfTheProof, new()
	{
		proofInformationCommon.OperationType = OperationType;
		proofInformationCommon.DeclarationType = proofInformationProvider.DeclarationType;
		proofInformationCommon.RequestedValididtyOfTheProof = GetPopulatedRequestedValidityOfTheProof(proofInformationProvider.RequestedValidityOfTheProof);
		proofInformationCommon.RequestType = proofInformationProvider.RequestType;
		proofInformationCommon.NationalOnlyRequest = proofInformationProvider.NationalOnlyRequest;

		T GetPopulatedRequestedValidityOfTheProof(IT2LPOUSRequestedValidityOfTheProof requestedValidityOfTheProofProvider)
		{
			var requestedValidityOfTheProof = default(T);
			if (requestedValidityOfTheProofProvider != null)
			{
				requestedValidityOfTheProof = new T()
				{
					NumberOfDays = (ushort)requestedValidityOfTheProofProvider.NumberOfDays,
					Justification = requestedValidityOfTheProofProvider.Justification,
				};
			}
			return requestedValidityOfTheProof;
		}
	}
}
