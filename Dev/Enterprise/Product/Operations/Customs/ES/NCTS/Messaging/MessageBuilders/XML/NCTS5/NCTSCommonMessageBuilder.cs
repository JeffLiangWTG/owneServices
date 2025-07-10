using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

public abstract class NCTSCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : INCTSCommonDataProvider
{
	protected NCTSCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	const string MessageRecipient = "NTA.ES";

	const int MaxDecimals2 = 2;
	const int MaxDecimals3 = 3;

	protected abstract ZString GetMessageType();

	protected T GetPopulatedTransactionId<T>()
		where T : INCTSTransactionId, new()
	{
		return new T()
		{
			TransactionId = TransactionId,
		};
	}

	protected T GetPopulatedMessage<T>()
		where T : INCTSServiceSegmentCommon, new()
	{
		return new T()
		{
			MessageSender = provider.MessageSender,
			MessageRecipient = MessageRecipient,
			PreparationDateAndTime = GetDateTimeFromZDateTime(CET),
			MessageIdentification = provider.MessageIdentification,
			MessageType = GetMessageType(),
			IsFinalPeriod = provider.IsFinalPeriod,
			PhaseIDSpecified = provider.PhaseIDSpecified,
		};
	}

	protected DateTime GetDateTimeFromZDateTime(ZDateTime providerDate) => new DateTime(providerDate.Year, providerDate.Month, providerDate.Day, providerDate.Hour, providerDate.Minute, providerDate.Second);

	protected T GetPopulatedCommonTransitOperationMRN<T>(INCTSCommonTransitOperationMRN transitOperationProvider)
		where T : INCTSTransitOperationMRN, new()
	{
		var transitOperation = default(T);
		if (transitOperationProvider != null)
		{
			transitOperation = new T()
			{
				MRN = transitOperationProvider.MRN,
			};
		}
		return transitOperation;
	}

	protected T GetPopulatedCommonTransitOperationLRN<T>(INCTSCommonTransitOperationLRN transitOperationProvider)
		where T : INCTSTransitOperationLRN, new()
	{
		var transitOperation = default(T);
		if (transitOperationProvider != null)
		{
			transitOperation = new T()
			{
				LRN = transitOperationProvider.LRN,
			};
		}
		return transitOperation;
	}

	protected void GetPopulatedCommonTransitOperation(INCTSCommonTransitOperation commonTransitOperationProvider, INCTSTransitOperationCommon transitOperation)
	{
		if (commonTransitOperationProvider != null)
		{
			transitOperation.DeclarationType = commonTransitOperationProvider.DeclarationType;
			transitOperation.TIRCarnetNumber = commonTransitOperationProvider.TIRCarnetNumber;
			transitOperation.Security = commonTransitOperationProvider.Security;
		}
	}

	protected void GetPopulatedCommonCompleteTransitOperation(INCTSCommonCompleteTransitOperation commonTransitOperationProvider, INCTSTransitOperationCommonComplete transitOperation)
	{
		GetPopulatedCommonTransitOperation(commonTransitOperationProvider, transitOperation);
		if (commonTransitOperationProvider != null)
		{
			transitOperation.AdditionalDeclarationType = commonTransitOperationProvider.AdditionalDeclarationType;
			transitOperation.ReducedDatasetIndicator = commonTransitOperationProvider.ReducedDatasetIndicator;
			transitOperation.SpecificCircumstanceIndicator = commonTransitOperationProvider.SpecificCircumstanceIndicator;
			transitOperation.BindingItinerary = ZBool.False;
		}
	}

	protected T GetPopulatedAuthorisation<T>(INCTSCommonAuthorisation providerAuthorisation)
		where T : INCTSAuthorisation, new()
	{
		var authorisation = default(T);
		if (providerAuthorisation != null)
		{
			authorisation = new T()
			{
				SequenceNumber = providerAuthorisation.SequenceNumber,
				Type = providerAuthorisation.Type,
				ReferenceNumber = providerAuthorisation.ReferenceNumber
			};
		}
		return authorisation;
	}

	protected T GetPopulatedCustomOffice<T>(ZString customOfficeProvider)
		where T : INCTSCustomsOffice, new()
	{
		var office = default(T);
		if (!customOfficeProvider.IsEmpty)
		{
			office = new T()
			{
				ReferenceNumber = customOfficeProvider,
			};
		}
		return office;
	}

	protected T GetPopulatedCommonCustomOffice<T>(INCTSCommonCustomsOffice customOfficeProvider)
		where T : INCTSCustomsOfficeDeclared, new()
	{
		var office = GetPopulatedCustomOffice<T>(customOfficeProvider.ReferenceNumber);
		if (office != null && customOfficeProvider != null)
		{
			office.SequenceNumber = customOfficeProvider.SequenceNumber;
		}
		return office;
	}

	protected T GetPopulatedCommonHolderOfTheTransitProcedure<T>(INCTSCommonHolderOfTheTransitProcedure holderOfTheTransitProcedureProvider)
		where T : INCTSHolderOfTheTransitProcedureCommon, new()
	{
		var holderOfTheTransitProcedure = GetPopulatedAddressInformationIdCommon<T>(holderOfTheTransitProcedureProvider);
		if (holderOfTheTransitProcedureProvider != null)
		{
			holderOfTheTransitProcedure.TIRHolderIdentificationNumber = holderOfTheTransitProcedureProvider.TIRHolderIdentificationNumber;
		}
		return holderOfTheTransitProcedure;
	}

	protected T GetPopulatedHolderOfTheTransitProcedureWithAddress<T, TAddress>(INCTSCommonHolderOfTheTransitProcedureWithAddress holderOfTheTransitProcedureProvider)
		where T : INCTSHolderOfTheTransitProcedureWithAddress, new()
		where TAddress : INCTSAddressInfo, new()
	{
		var holderOfTheTransitProcedure = GetPopulatedCommonHolderOfTheTransitProcedure<T>(holderOfTheTransitProcedureProvider);
		if (holderOfTheTransitProcedureProvider != null)
		{
			holderOfTheTransitProcedure.Address = GetPopulatedAddress<TAddress>(holderOfTheTransitProcedureProvider.Address);
		}
		return holderOfTheTransitProcedure;
	}

	protected T GetPopulatedHolderOfTheTransitProcedure<T, TContactPerson, TAddress>(INCTSCompleteHolderOfTheTransitProcedure holderOfTheTransitProcedureProvider)
		where T : INCTSHolderOfTheTransitProcedure, new()
		where TContactPerson : ICommonContactPerson, new()
		where TAddress : INCTSAddressInfo, new()
	{
		var holderOfTheTransitProcedure = GetPopulatedHolderOfTheTransitProcedureWithAddress<T, TAddress>(holderOfTheTransitProcedureProvider);
		if (holderOfTheTransitProcedureProvider != null)
		{
			holderOfTheTransitProcedure.ContactPerson = GetPopulatedContactPerson<TContactPerson>(holderOfTheTransitProcedureProvider.ContactPerson);
		}
		return holderOfTheTransitProcedure;
	}

	protected T GetPopulatedRepresentative<T, TContactPerson>(ICommonRepresentativeWithContactPerson providerRepresentative)
		where T : INCTSRepresentative, new()
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

	protected T GetPopulatedCommonAddress<T>(INCTSCommonAddress addressInfo)
		where T : INCTSAddressCommonInfo, new()
	{
		var address = default(T);
		if (addressInfo != null)
		{
			address = new T()
			{
				StreetAndNumber = addressInfo.StreetAndNumber,
				PostCode = addressInfo.PostCode,
				City = addressInfo.City,
			};
		}
		return address;
	}

	protected T GetPopulatedAddress<T>(INCTSCommonAddressInfo addressInfo)
		where T : INCTSAddressInfo, new()
	{
		var address = GetPopulatedCommonAddress<T>(addressInfo);
		if (addressInfo != null)
		{
			address.Country = addressInfo.Country;
		}
		return address;
	}

	protected T GetPopulatedGuarantee<T, TReference>(INCTSCommonGuarantee providerGuarantee)
		where T : INCTSGuarantee, new()
		where TReference : INCTSGuaranteeReference, new()
	{
		var guarantee = default(T);
		if (providerGuarantee != null)
		{
			var referenceCollection = new Collection<INCTSGuaranteeReference>();
			foreach (var reference in providerGuarantee.GuaranteeReference.ConvertToCollection(GetPopulatedGuaranteeReference) ?? Enumerable.Empty<TReference>())
			{
				referenceCollection.Add(reference);
			}
			guarantee = new T()
			{
				SequenceNumber = providerGuarantee.SequenceNumber,
				GuaranteeType = providerGuarantee.GuaranteeType,
				GuaranteeReference = referenceCollection
			};
		}
		return guarantee;

		TReference GetPopulatedGuaranteeReference(INCTSCommonGuaranteeReference providerReference)
		{
			var reference = default(TReference);
			if (providerReference != null)
			{
				reference = new TReference()
				{
					SequenceNumber = providerReference.SequenceNumber,
					GRN = providerReference.GRN,
					AccessCode = providerReference.AccessCode,
					AmountToBeCovered = providerReference.AmountToBeCovered.Round(MaxDecimals2)
				};
			}
			return reference;
		}
	}

	protected T GetPopulatedCommonConsignment<T, TTranspEqu, TDepartureTransport>(INCTSCommonConsignment providerConsignment,
															Func<INCTSCommonTransportEquipment, TTranspEqu> getTransportEquipment)
		where T : INCTSConsignmentCommon, new()
		where TTranspEqu : INCTSTransportEquipment, new()
		where TDepartureTransport : INCTSDepartureTransportMeans, new()
	{
		var consignment = default(T);
		if (providerConsignment != null)
		{
			var transportEquipmentCollection = new Collection<INCTSTransportEquipment>();
			foreach (var transportEquipment in providerConsignment.TransportEquipment.ConvertToCollection(getTransportEquipment) ?? Enumerable.Empty<TTranspEqu>())
			{
				transportEquipmentCollection.Add(transportEquipment);
			}
			var departureTransportCollection = new Collection<INCTSDepartureTransportMeans>();
			foreach (var departureTransport in providerConsignment.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<TDepartureTransport>) ?? Enumerable.Empty<TDepartureTransport>())
			{
				departureTransportCollection.Add(departureTransport);
			}
			consignment = new T()
			{
				TransportEquipment = transportEquipmentCollection,
				DepartureTransportMeans = departureTransportCollection,
			};
		}
		return consignment;
	}

	protected T GetPopulatedCommonConsignmentDeparture<T, TTranspEqu, TDepartureTransport>(INCTSCommonDepartureConsignment providerConsignment,
															Func<INCTSCommonTransportEquipment, TTranspEqu> getTransportEquipment)
		where T : INCTSConsignmentDepartureCommon, new()
		where TTranspEqu : INCTSTransportEquipment, new()
		where TDepartureTransport : INCTSDepartureTransportMeans, new()
	{
		var consignment = GetPopulatedCommonConsignment<T, TTranspEqu, TDepartureTransport>(providerConsignment, getTransportEquipment);
		if (providerConsignment != null)
		{
			var transportEquipmentCollection = new Collection<INCTSTransportEquipment>();
			foreach (var transportEquipment in providerConsignment.TransportEquipment.ConvertToCollection(getTransportEquipment) ?? Enumerable.Empty<TTranspEqu>())
			{
				transportEquipmentCollection.Add(transportEquipment);
			}
			var departureTransportCollection = new Collection<INCTSDepartureTransportMeans>();
			foreach (var departureTransport in providerConsignment.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<TDepartureTransport>) ?? Enumerable.Empty<TDepartureTransport>())
			{
				departureTransportCollection.Add(departureTransport);
			}
			consignment.ContainerIndicator = providerConsignment.ContainerIndicator;
			consignment.InlandModeOfTransport = providerConsignment.InlandModeOfTransport;
			consignment.ModeOfTransportAtTheBorder = providerConsignment.ModeOfTransportAtTheBorder;
		}
		return consignment;
	}

	protected T GetPopulatedCommonConsignmentDepartureAndNotif<T, TTranspEqu, TLocation, TDepartureTransport, TActiveBorderTransport, TPlace>(INCTSCommonDepartureAndNotifConsignment providerConsignment,
															Func<INCTSCommonTransportEquipment, TTranspEqu> getTransportEquipment)
		where T : INCTSConsignmentDepartureAndNotifCommon, new()
		where TTranspEqu : INCTSTransportEquipment, new()
		where TLocation : INCTSLocationOfGoodsCommon, new()
		where TDepartureTransport : INCTSDepartureTransportMeans, new()
		where TActiveBorderTransport : INCTSActiveBorderTransportMeansWithOffice, new()
		where TPlace : INCTSPlaceCommon, new()
	{
		var consignment = GetPopulatedCommonConsignmentDeparture<T, TTranspEqu, TDepartureTransport>(providerConsignment, getTransportEquipment);
		if (providerConsignment != null)
		{
			var activeBorderTransportCollection = new Collection<INCTSActiveBorderTransportMeans>();
			foreach (var activeBorderTransport in providerConsignment.ActiveBorderTransportMeans.ConvertToCollection(GetPopulatedActiveBorderTransportMeansWithOffice<TActiveBorderTransport>) ?? Enumerable.Empty<TActiveBorderTransport>())
			{
				activeBorderTransportCollection.Add(activeBorderTransport);
			}
			consignment.LocationOfGoods = GetPopulatedLocationOfGoods<TLocation>(providerConsignment.LocationOfGoods);
			consignment.ActiveBorderTransportMeans = activeBorderTransportCollection;
			consignment.PlaceOfLoading = GetPopulatedCommonPlace<TPlace>(providerConsignment.PlaceOfLoading);
		}
		return consignment;
	}

	protected T GetPopulatedConsignmentDepartureAndAmendment<T, TTranspEqu, TLocation, TDepartureTransport, TActiveBorderTransport, TPlaceLoading,
															TCarrier, TConsignor, TConsignee, TContactPerson, TAddress, TActor, TCountry,
															TPlaceUnloading, TTransportCharge, THouse, TSupportingDoc, TTransportDoc, TAdditionalRef,
															TAdditionalInfo>(INCTSCommonConsignmentDepartureAndAmendment providerConsignment,
															Func<INCTSCommonTransportEquipment, TTranspEqu> getTransportEquipment,
															Func<INCTSCommonHouseConsignmentDepartureAndAmendment, THouse> getHouseConsignment)
		where T : INCTSConsignmentDepartureAndAmendment, new()
		where TTranspEqu : INCTSTransportEquipment, new()
		where TLocation : INCTSLocationOfGoodsCommon, new()
		where TDepartureTransport : INCTSDepartureTransportMeans, new()
		where TActiveBorderTransport : INCTSActiveBorderTransportMeansWithOffice, new()
		where TPlaceLoading : INCTSPlaceCommon, new()
		where TCarrier : INCTSCarrier, new()
		where TConsignor : INCTSConsignor, new()
		where TConsignee : INCTSOrgAddressInfoWithAddress, new()
		where TContactPerson : ICommonContactPerson, new()
		where TAddress : INCTSAddressInfo, new()
		where TActor : INCTSAdditionalSupplyChainActor, new()
		where TCountry : INCTSCountryOfRoutingOfConsignment, new()
		where TPlaceUnloading : INCTSPlaceCommon, new()
		where TTransportCharge : INCTSTransportCharges, new()
		where THouse : INCTSHouseConsignmentDepartureAndAmendment, new()
		where TSupportingDoc : INCTSDocumentCommonWithItem, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		var consignment = GetPopulatedCommonConsignmentDepartureAndNotif<T, TTranspEqu, TLocation, TDepartureTransport, TActiveBorderTransport, TPlaceLoading>(providerConsignment,
										getTransportEquipment);
		if (consignment != null)
		{
			var actorCollection = new Collection<INCTSAdditionalSupplyChainActor>();
			foreach (var actor in providerConsignment.AdditionalSupplyChainActor.ConvertToCollection(GetPopulatedAdditionalSupplyChainActor<TActor>) ?? Enumerable.Empty<TActor>())
			{
				actorCollection.Add(actor);
			}
			var houseConsignmentCollection = new Collection<INCTSHouseConsignmentDepartureAndAmendment>();
			foreach (var house in providerConsignment.HouseConsignment.ConvertToCollection(getHouseConsignment) ?? Enumerable.Empty<THouse>())
			{
				houseConsignmentCollection.Add(house);
			}
			GetPopulatedCommonConsignmentData<TCountry, TConsignee, TAddress, TTransportCharge, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalInfo>(providerConsignment.CommonConsignmentData, consignment);
			consignment.Carrier = GetPopulatedCarrier<TCarrier, TContactPerson>(providerConsignment.Carrier);
			consignment.Consignor = GetPopulatedConsignor<TConsignor, TContactPerson, TAddress>(providerConsignment.Consignor);
			consignment.AdditionalSupplyChainActor = actorCollection;
			consignment.PlaceOfUnloading = GetPopulatedCommonPlace<TPlaceUnloading>(providerConsignment.PlaceOfUnloading);
			consignment.TransportCharges = GetPopulatedTransportCharges<TTransportCharge>(providerConsignment.CommonConsignmentData.MethodOfPayment);
			consignment.HouseConsignment = houseConsignmentCollection;
		}
		return consignment;
	}

	protected void GetPopulatedCommonConsignmentData<TCountry, TConsignee, TAddress, TTransportCharge, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalInfo>(INCTSCommonConsignmentDepartureAndAmendmentAndTNN commonConsignmentDataProvider, INCTSConsignmentDepartureAndAmendmentAndTNN commonConsignment)
		where TCountry : INCTSCountryOfRoutingOfConsignment, new()
		where TConsignee : INCTSOrgAddressInfoWithAddress, new()
		where TAddress : INCTSAddressInfo, new()
		where TTransportCharge : INCTSTransportCharges, new()
		where TSupportingDoc : INCTSDocumentCommonWithItem, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		if (commonConsignmentDataProvider != null)
		{
			var countryCollection = new Collection<INCTSCountryOfRoutingOfConsignment>();
			foreach (var country in commonConsignmentDataProvider.CountryOfRoutingOfConsignment.ConvertToCollection(GetPopulatedCountryOfRouting<TCountry>) ?? Enumerable.Empty<TCountry>())
			{
				countryCollection.Add(country);
			}
			var supportingDocCollection = new Collection<INCTSDocumentCommonWithItem>();
			foreach (var supportingDoc in commonConsignmentDataProvider.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithItem<TSupportingDoc>) ?? Enumerable.Empty<TSupportingDoc>())
			{
				supportingDocCollection.Add(supportingDoc);
			}
			var transportDocCollection = new Collection<INCTSDocumentCommon>();
			foreach (var transportDoc in commonConsignmentDataProvider.TransportDocument.ConvertToCollection(GetPopulatedCommonDocument<TTransportDoc>) ?? Enumerable.Empty<TTransportDoc>())
			{
				transportDocCollection.Add(transportDoc);
			}
			var additionalRefCollection = new Collection<INCTSDocumentCommon>();
			foreach (var additionalRef in commonConsignmentDataProvider.AdditionalReference.ConvertToCollection(GetPopulatedCommonDocument<TAdditionalRef>) ?? Enumerable.Empty<TAdditionalRef>())
			{
				additionalRefCollection.Add(additionalRef);
			}
			var additionalInfoCollection = new Collection<INCTSAdditionalInformation>();
			foreach (var additionalInfo in commonConsignmentDataProvider.AdditionalInformation.ConvertToCollection(GetPopulatedAdditionalInfo<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfoCollection.Add(additionalInfo);
			}
			GetPopulatedCommonGrossMass(commonConsignmentDataProvider.GrossMass, commonConsignment);
			commonConsignment.CountryOfDispatch = commonConsignmentDataProvider.CountryOfDispatch;
			commonConsignment.CountryOfDestination = commonConsignmentDataProvider.CountryOfDestination;
			commonConsignment.ReferenceNumberUCR = commonConsignmentDataProvider.ReferenceNumberUCR;
			commonConsignment.Consignee = GetPopulatedNCTSPartyNameProviderWithAddress<TConsignee, TAddress>(commonConsignmentDataProvider.Consignee);
			commonConsignment.TransportCharges = GetPopulatedTransportCharges<TTransportCharge>(commonConsignmentDataProvider.MethodOfPayment);
			commonConsignment.CountryOfRoutingOfConsignment = countryCollection;
			commonConsignment.SupportingDocument = supportingDocCollection;
			commonConsignment.TransportDocument = transportDocCollection;
			commonConsignment.AdditionalReference = additionalRefCollection;
			commonConsignment.AdditionalInformation = additionalInfoCollection;
		}
	}

	protected T GetPopulatedTransportEquipment<T, TSeal, TGoodsReference>(INCTSCommonTransportEquipment providerTransportEquipment)
		where T : INCTSTransportEquipment, new()
		where TSeal : INCTSSeal, new()
		where TGoodsReference : INCTSGoodsReference, new()
	{
		var transportEquipment = default(T);
		if (providerTransportEquipment != null)
		{
			var sealsCollection = new Collection<INCTSSeal>();
			foreach (var seal in providerTransportEquipment.Seals.ConvertToCollection(GetPopulatedSeal) ?? Enumerable.Empty<TSeal>())
			{
				sealsCollection.Add(seal);
			}
			var goodsReferenceCollection = new Collection<INCTSGoodsReference>();
			foreach (var goodsReference in providerTransportEquipment.GoodsReference.ConvertToCollection(GetPopulatedGoodsReference) ?? Enumerable.Empty<TGoodsReference>())
			{
				goodsReferenceCollection.Add(goodsReference);
			}
			transportEquipment = new T()
			{
				SequenceNumber = providerTransportEquipment.SequenceNumber,
				ContainerIdentificationNumber = providerTransportEquipment.ContainerIdentificationNumber,
				NumberOfSeals = providerTransportEquipment.NumberOfSeals,
				Seals = sealsCollection,
				GoodsReference = goodsReferenceCollection
			};
		}
		return transportEquipment;

		TSeal GetPopulatedSeal(ISealCommon providerSeal)
		{
			var seal = default(TSeal);
			if (providerSeal != null)
			{
				seal = new TSeal()
				{
					SequenceNumber = providerSeal.SequenceNumber,
					Identifier = providerSeal.SealNumber
				};
			}
			return seal;
		}

		TGoodsReference GetPopulatedGoodsReference(INCTSCommonGoodsReference providerGoodsReference)
		{
			var goodsReference = default(TGoodsReference);
			if (providerGoodsReference != null)
			{
				goodsReference = new TGoodsReference
				{
					SequenceNumber = providerGoodsReference.SequenceNumber,
					DeclarationGoodsItemNumber = providerGoodsReference.DeclarationGoodsItemNumber
				};
			}
			return goodsReference;
		}
	}

	protected T GetPopulatedLocationOfGoods<T>(INCTSCommonLocationOfGoods providerLocationOfGoods)
		where T : INCTSLocationOfGoodsCommon, new()
	{
		var locationOfGoods = default(T);
		if (providerLocationOfGoods != null)
		{
			locationOfGoods = new T()
			{
				TypeOfLocation = providerLocationOfGoods.TypeOfLocation,
				QualifierOfIdentification = providerLocationOfGoods.QualifierOfIdentification,
				AuthorisationNumber = providerLocationOfGoods.AuthorisationNumber
			};
		}
		return locationOfGoods;
	}

	protected T GetPopulatedDepartureTransportMeans<T>(ICommonDepartureTransportMeans providerDeparture)
		where T : INCTSDepartureTransportMeans, new()
	{
		var departure = GetPopulatedTransportMediumInfoCommon<T>(providerDeparture);
		if (providerDeparture != null)
		{
			departure.SequenceNumber = providerDeparture.SequenceNumber;
		}
		return departure;
	}

	protected T GetPopulatedActiveBorderTransportMeans<T>(INCTSCommonActiveBorderTransportMeans providerActiveBorder)
		where T : INCTSActiveBorderTransportMeans, new()
	{
		var activeBorder = GetPopulatedTransportMediumInfoCommon<T>(providerActiveBorder);
		if (providerActiveBorder != null)
		{
			activeBorder.SequenceNumber = providerActiveBorder.SequenceNumber;
			activeBorder.ConveyanceReferenceNumber = providerActiveBorder.ConveyanceReferenceNumber;
		}
		return activeBorder;
	}

	protected T GetPopulatedActiveBorderTransportMeansWithOffice<T>(INCTSCommonActiveBorderTransportMeansWithOffice providerActiveBorder)
		where T : INCTSActiveBorderTransportMeansWithOffice, new()
	{
		var activeBorder = GetPopulatedActiveBorderTransportMeans<T>(providerActiveBorder);
		if (providerActiveBorder != null)
		{
			activeBorder.CustomsOfficeAtBorderReferenceNumber = providerActiveBorder.CustomsOfficeAtBorderReferenceNumber;
		}
		return activeBorder;
	}

	protected T GetPopulatedCommonPlace<T>(INCTSCommonPlace providerPlaceOfLoading)
		where T : INCTSPlaceCommon, new()
	{
		var placeOfLoading = default(T);
		if (providerPlaceOfLoading != null)
		{
			placeOfLoading = new T()
			{
				UNLocode = providerPlaceOfLoading.UNLocode,
				Country = providerPlaceOfLoading.Country,
				Location = providerPlaceOfLoading.Location
			};
		}
		return placeOfLoading;
	}

	protected void GetPopulatedCommonGrossMass(ZDecimal grossMassProvider, INCTSConsignmentGrossMass grossMass)
	{
		if (!grossMassProvider.IsEmpty)
		{
			grossMass.GrossMass = grossMassProvider;
		}
	}

	protected T GetPopulatedCarrier<T, TContactPerson>(INCTSCommonCarrier carrierProvider)
		where T : INCTSCarrier, new()
		where TContactPerson : ICommonContactPerson, new()
	{
		var carrier = GetPopulatedAddressInformationIdCommon<T>(carrierProvider);
		if (carrierProvider != null)
		{
			carrier.ContactPerson = GetPopulatedContactPerson<TContactPerson>(carrierProvider.ContactPerson);
		}
		return carrier;
	}

	protected T GetPopulatedConsignor<T, TContactPerson, TAddress>(INCTSCommonConsignor consignorProvider)
		where T : INCTSConsignor, new()
		where TContactPerson : ICommonContactPerson, new()
		where TAddress : INCTSAddressInfo, new()
	{
		var consignor = GetPopulatedAddressInformationDeclarantCommon<T>(consignorProvider);
		if (consignorProvider != null)
		{
			consignor.ContactPerson = GetPopulatedContactPerson<TContactPerson>(consignorProvider.ContactPerson);
			consignor.Address = GetPopulatedAddress<TAddress>(consignorProvider.Address);
		}
		return consignor;
	}

	protected T GetPopulatedNCTSPartyNameProviderWithAddress<T, TAddress>(INCTSPartyNameProviderWithAddress consigneeProvider)
		where T : INCTSOrgAddressInfoWithAddress, new()
		where TAddress : INCTSAddressInfo, new()
	{
		var consignee = GetPopulatedAddressInformationDeclarantCommon<T>(consigneeProvider);
		if (consigneeProvider != null)
		{
			consignee.Address = GetPopulatedAddress<TAddress>(consigneeProvider.Address);
		}
		return consignee;
	}

	protected T GetPopulatedAdditionalSupplyChainActor<T>(ICommonAdditionalSupplyChainActorSeqNum providerActor)
		where T : INCTSAdditionalSupplyChainActor, new()
	{
		var actor = default(T);
		if (providerActor != null)
		{
			actor = new T()
			{
				SequenceNumber = providerActor.SequenceNumber,
				Role = providerActor.Role,
				IdentificationNumber = providerActor.Id
			};
		}
		return actor;
	}

	protected T GetPopulatedCountryOfRouting<T>(ICommonCountryOfRoutingOfConsignment providerCountry)
		where T : INCTSCountryOfRoutingOfConsignment, new()
	{
		var country = default(T);
		if (providerCountry != null)
		{
			country = new T()
			{
				SequenceNumber = providerCountry.SequenceNumber,
				Country = providerCountry.CountryOfRouting
			};
		}
		return country;
	}

	protected T GetPopulatedTransportCharges<T>(ZString providerCharge)
		where T : INCTSTransportCharges, new()
	{
		var transportCharge = default(T);
		if (!providerCharge.IsEmpty)
		{
			transportCharge = new T()
			{
				MethodOfPayment = providerCharge
			};
		}
		return transportCharge;
	}

	protected T GetPopulatedHouseConsignmentSeqNumCommon<T>(INCTSCommonHouseConsignmentSeqNum providerHouse)
		where T : INCTSHouseConsignmentSeqNumCommon, new()
	{
		var house = default(T);
		if (providerHouse != null)
		{
			house = new T()
			{
				SequenceNumber = providerHouse.SequenceNumber
			};
		}
		return house;
	}

	protected T GetPopulatedHouseConsignmentCommon<T, TTransportDoc, TAdditionalRef>(INCTSCommonHouseConsignment providerHouse)
		where T : INCTSHouseConsignmentCommon, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
	{
		var house = GetPopulatedHouseConsignmentSeqNumCommon<T>(providerHouse);
		if (providerHouse != null)
		{
			var transportDocCollection = new Collection<INCTSDocumentCommon>();
			foreach (var transportDoc in providerHouse.TransportDocument.ConvertToCollection(GetPopulatedCommonDocument<TTransportDoc>) ?? Enumerable.Empty<TTransportDoc>())
			{
				transportDocCollection.Add(transportDoc);
			}
			var additionalRefCollection = new Collection<INCTSDocumentCommon>();
			foreach (var additionalRef in providerHouse.AdditionalReference.ConvertToCollection(GetPopulatedCommonDocument<TAdditionalRef>) ?? Enumerable.Empty<TAdditionalRef>())
			{
				additionalRefCollection.Add(additionalRef);
			}
			house.GrossMass = providerHouse.GrossMass;
			house.TransportDocument = transportDocCollection;
			house.AdditionalReference = additionalRefCollection;
		}
		return house;
	}

	protected T GetPopulatedHouseConsignmentDepartureAndAmendmentAndTNN<T, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalInfo>(INCTSCommonHouseConsignmentDepartureAndAmendmentAndTNN providerHouse)
		where T : INCTSHouseConsignmentDepartureAndAmendmentAndTNN, new()
		where TSupportingDoc : INCTSDocumentCommonWithItem, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		var house = GetPopulatedHouseConsignmentCommon<T, TTransportDoc, TAdditionalRef>(providerHouse);
		if (providerHouse != null)
		{
			var supportingDocCollection = new Collection<INCTSDocumentCommonWithItem>();
			foreach (var supportingDoc in providerHouse.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithItem<TSupportingDoc>) ?? Enumerable.Empty<TSupportingDoc>())
			{
				supportingDocCollection.Add(supportingDoc);
			}
			var additionalInfoCollection = new Collection<INCTSAdditionalInformation>();
			foreach (var additionalInfo in providerHouse.AdditionalInformation.ConvertToCollection(GetPopulatedAdditionalInfo<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfoCollection.Add(additionalInfo);
			}
			house.ReferenceNumberUCR = providerHouse.ReferenceNumberUCR;
			house.SupportingDocument = supportingDocCollection;
			house.AdditionalInformation = additionalInfoCollection;
		}
		return house;
	}

	protected T GetPopulatedHouseConsignmentDepartureAndAmendment<T, TActor, TItem, TConsignee, TConsigneeHouse, TConsignor, TContactPerson, TAddressHouse, TAddress, TCommodity, TCommodityCode, TDangerous, TMeasure, TPackage, TDepartureTransport, TPreviousDoc, TPreviousDocHouse, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalRefHouse, TAdditionalInfo>(INCTSCommonHouseConsignmentDepartureAndAmendment providerHouse)
		where T : INCTSHouseConsignmentDepartureAndAmendment, new()
		where TActor : INCTSAdditionalSupplyChainActor, new()
		where TItem : INCTSConsignmentItem, new()
		where TConsignee : INCTSOrgAddressInfoWithAddress, new()
		where TConsigneeHouse : INCTSOrgAddressInfoWithAddress, new()
		where TConsignor : INCTSConsignor, new()
		where TContactPerson : ICommonContactPerson, new()
		where TAddress : INCTSAddressInfo, new()
		where TAddressHouse : INCTSAddressInfo, new()
		where TCommodity : INCTSCommodity, new()
		where TCommodityCode : INCTSCommodityCode, new()
		where TDangerous : INCTSDangerousGoods, new()
		where TMeasure : INCTSGoodsMeasure, new()
		where TPackage : INCTSPackaging, new()
		where TDepartureTransport : INCTSDepartureTransportMeans, new()
		where TPreviousDoc : INCTSPreviousDocument, new()
		where TPreviousDocHouse : INCTSDocumentCommonWithInfo, new()
		where TSupportingDoc : INCTSDocumentCommonWithItem, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalRefHouse : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		var house = GetPopulatedHouseConsignmentDepartureAndAmendmentAndTNN<T, TSupportingDoc, TTransportDoc, TAdditionalRefHouse, TAdditionalInfo>(providerHouse);
		if (providerHouse != null)
		{
			var actorCollection = new Collection<INCTSAdditionalSupplyChainActor>();
			foreach (var actor in providerHouse.AdditionalSupplyChainActor.ConvertToCollection(GetPopulatedAdditionalSupplyChainActor<TActor>) ?? Enumerable.Empty<TActor>())
			{
				actorCollection.Add(actor);
			}
			var itemCollection = new Collection<INCTSConsignmentItem>();
			foreach (var item in providerHouse.ConsignmentItem.ConvertToCollection(GetPopulatedConsigmentItem<TItem, TConsignee, TAddress, TActor, TCommodity, TCommodityCode, TDangerous, TMeasure, TPackage, TPreviousDoc, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalInfo>) ?? Enumerable.Empty<TItem>())
			{
				itemCollection.Add(item);
			}
			var departureTransportCollection = new Collection<INCTSDepartureTransportMeans>();
			foreach (var departureTransport in providerHouse.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<TDepartureTransport>) ?? Enumerable.Empty<TDepartureTransport>())
			{
				departureTransportCollection.Add(departureTransport);
			}
			var previousDocCollection = new Collection<INCTSDocumentCommonWithInfo>();
			foreach (var previousDoc in providerHouse.PreviousDocument.ConvertToCollection(GetPopulatedCommonDocumentWithInfo<TPreviousDocHouse>) ?? Enumerable.Empty<TPreviousDocHouse>())
			{
				previousDocCollection.Add(previousDoc);
			}
			house.AdditionalSupplyChainActor = actorCollection;
			house.ConsignmentItem = itemCollection;
			house.CountryOfDispatch = providerHouse.CountryOfDispatch;
			house.CountryOfDestination = providerHouse.CountryOfDestination;
			house.Consignee = GetPopulatedNCTSPartyNameProviderWithAddress<TConsigneeHouse, TAddressHouse>(providerHouse.Consignee);
			house.Consignor = GetPopulatedConsignor<TConsignor, TContactPerson, TAddressHouse>(providerHouse.Consignor);
			house.DepartureTransportMeans = departureTransportCollection;
			house.PreviousDocument = previousDocCollection;
		}
		return house;
	}

	protected T GetPopulatedCommonConsigmentItem<T, TPackage, TTransportDoc, TAdditionalRef>(INCTSCommonConsignmentItem providerItem)
		where T : INCTSConsignmentItemCommon, new()
		where TPackage : INCTSPackaging, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
	{
		var item = default(T);
		if (providerItem != null)
		{
			var packagingCollection = new Collection<INCTSPackaging>();
			foreach (var package in providerItem.Packaging.ConvertToCollection(GetPopulatedPackaging) ?? Enumerable.Empty<TPackage>())
			{
				packagingCollection.Add(package);
			}
			var transportDocCollection = new Collection<INCTSDocumentCommon>();
			foreach (var transportDoc in providerItem.TransportDocument.ConvertToCollection(GetPopulatedCommonDocument<TTransportDoc>) ?? Enumerable.Empty<TTransportDoc>())
			{
				transportDocCollection.Add(transportDoc);
			}
			var additionalRefCollection = new Collection<INCTSDocumentCommon>();
			foreach (var additionalRef in providerItem.AdditionalReference.ConvertToCollection(GetPopulatedCommonDocument<TAdditionalRef>) ?? Enumerable.Empty<TAdditionalRef>())
			{
				additionalRefCollection.Add(additionalRef);
			}
			item = new T()
			{
				GoodsItemNumber = providerItem.GoodsItemNumber,
				DeclarationGoodsItemNumber = providerItem.DeclarationGoodsItemNumber,
				Packaging = packagingCollection,
				TransportDocument = transportDocCollection,
				AdditionalReference = additionalRefCollection
			};
		}
		return item;

		TPackage GetPopulatedPackaging(INCTSCommonPackaging providerPackage)
		{
			var package = default(TPackage);
			if (providerPackage != null)
			{
				package = new TPackage()
				{
					SequenceNumber = providerPackage.SequenceNumber,
					PackageType = providerPackage.PackageType,
					Marks = providerPackage.Marks,
					NumberOfPackages = providerPackage.NumberOfPackages
				};
			}
			return package;
		}
	}

	protected T GetPopulatedConsigmentItemDepartureAndAmendmentAndTNN<T, TPackage, TTransportDoc, TAdditionalRef, TAdditionalInfo>(INCTSCommonConsignmentItemDepartureAndAmendmentAndTNN providerItem)
		where T : INCTSConsignmentItemDepartureAndAmendmentAndTNN, new()
		where TPackage : INCTSPackaging, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		var item = GetPopulatedCommonConsigmentItem<T, TPackage, TTransportDoc, TAdditionalRef>(providerItem);
		if (providerItem != null)
		{
			var additionalInfoCollection = new Collection<INCTSAdditionalInformation>();
			foreach (var additionalInfo in providerItem.AdditionalInformation.ConvertToCollection(GetPopulatedAdditionalInfo<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfoCollection.Add(additionalInfo);
			}

			item.DeclarationType = providerItem.DeclarationType;
			item.CountryOfDestination = providerItem.CountryOfDestination;
			item.ReferenceNumberUCR = providerItem.ReferenceNumberUCR;
			item.AdditionalInformation = additionalInfoCollection;
		}
		return item;
	}

	T GetPopulatedAdditionalInfo<T>(ICommonDocumentSequenceNumber providerAdditionalInfo)
		where T : INCTSAdditionalInformation, new()
	{
		var additionalInfo = default(T);
		if (providerAdditionalInfo != null)
		{
			additionalInfo = new T()
			{
				SequenceNumber = providerAdditionalInfo.SequenceNumber,
				Code = providerAdditionalInfo.Name,
				Text = providerAdditionalInfo.Number
			};
		}
		return additionalInfo;
	}

	protected T GetPopulatedConsigmentItem<T, TConsignee, TAddress, TActor, TCommodity, TCommodityCode, TDangerous, TMeasure, TPackage, TPreviousDoc, TSupportingDoc, TTransportDoc, TAdditionalRef, TAdditionalInfo>(INCTSConsignmentItemDepartureAndAmendment providerItem)
		where T : INCTSConsignmentItem, new()
		where TConsignee : INCTSOrgAddressInfoWithAddress, new()
		where TAddress : INCTSAddressInfo, new()
		where TActor : INCTSAdditionalSupplyChainActor, new()
		where TCommodity : INCTSCommodity, new()
		where TCommodityCode : INCTSCommodityCode, new()
		where TDangerous : INCTSDangerousGoods, new()
		where TMeasure : INCTSGoodsMeasure, new()
		where TPackage : INCTSPackaging, new()
		where TPreviousDoc : INCTSPreviousDocument, new()
		where TSupportingDoc : INCTSDocumentCommon, new()
		where TTransportDoc : INCTSDocumentCommon, new()
		where TAdditionalRef : INCTSDocumentCommon, new()
		where TAdditionalInfo : INCTSAdditionalInformation, new()
	{
		var item = GetPopulatedConsigmentItemDepartureAndAmendmentAndTNN<T, TPackage, TTransportDoc, TAdditionalRef, TAdditionalInfo>(providerItem);
		if (providerItem != null)
		{
			var actorCollection = new Collection<INCTSAdditionalSupplyChainActor>();
			foreach (var actor in providerItem.AdditionalSupplyChainActor.ConvertToCollection(GetPopulatedAdditionalSupplyChainActor<TActor>) ?? Enumerable.Empty<TActor>())
			{
				actorCollection.Add(actor);
			}
			var previousDocCollection = new Collection<INCTSPreviousDocument>();
			foreach (var previousDoc in providerItem.PreviousDocument.ConvertToCollection(GetPopulatedPreviousDoc) ?? Enumerable.Empty<TPreviousDoc>())
			{
				previousDocCollection.Add(previousDoc);
			}
			var supportingDocCollection = new Collection<INCTSDocumentCommon>();
			foreach (var supportingDoc in providerItem.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocument<TSupportingDoc>) ?? Enumerable.Empty<TSupportingDoc>())
			{
				supportingDocCollection.Add(supportingDoc);
			}

			item.CountryOfDispatch = providerItem.CountryOfDispatch;
			item.Consignee = GetPopulatedNCTSPartyNameProviderWithAddress<TConsignee, TAddress>(providerItem.Consignee);
			item.AdditionalSupplyChainActor = actorCollection;
			item.Commodity = GetPopulatedCommodity(providerItem.Commodity);
			item.PreviousDocument = previousDocCollection;
			item.SupportingDocument = supportingDocCollection;
		}
		return item;

		TCommodity GetPopulatedCommodity(INCTSCommodityDepartureAndAmendment providerCommodity)
		{
			var commodity = GetPopulatedCommonCommodityWithCusCode<TCommodity, TCommodityCode>(providerCommodity);
			if (providerCommodity != null)
			{
				var dangerousGoodsCollection = new Collection<INCTSDangerousGoods>();
				foreach (var dangerousGood in providerCommodity.DangerousGoods.ConvertToCollection(GetPopulatedDangerousGoods<TDangerous>) ?? Enumerable.Empty<TDangerous>())
				{
					dangerousGoodsCollection.Add(dangerousGood);
				}

				commodity.DangerousGoods = dangerousGoodsCollection;
				commodity.GoodsMeasure = GetPopulatedGoodsMeasure(providerCommodity.GoodsMeasure);
			}
			return commodity;
		}

		TMeasure GetPopulatedGoodsMeasure(INCTSGoodsMeasureDepartureAndAmendment providerGoodsMeasure)
		{
			var goodsMeasure = GetPopulatedCommonGoodsMeasure<TMeasure>(providerGoodsMeasure);
			if (providerGoodsMeasure != null)
			{
				goodsMeasure.SupplementaryUnits = providerGoodsMeasure.SupplementaryUnits;
				goodsMeasure.SupplementaryUnitsSpecified = providerGoodsMeasure.SupplementaryUnitsSpecified;
			}
			return goodsMeasure;
		}

		TPreviousDoc GetPopulatedPreviousDoc(INCTSCommonPreviousDocument providerPreviousDoc)
		{
			var previousDoc = GetPopulatedCommonDocumentWithInfo<TPreviousDoc>(providerPreviousDoc);
			if (providerPreviousDoc != null)
			{
				previousDoc.GoodsItemNumber = providerPreviousDoc.GoodsItemNumber;
				previousDoc.MeasurementUnitAndQualifier = providerPreviousDoc.MeasurementUnitAndQualifier;
				previousDoc.Quantity = providerPreviousDoc.Quantity.Round(MaxDecimals3);
				previousDoc.QuantitySpecified = providerPreviousDoc.QuantitySpecified;
			}
			return previousDoc;
		}
	}

	protected T GetPopulatedDangerousGoods<T>(ICommonDangerousGoods providerDangerousGoods)
		where T : INCTSDangerousGoods, new()
	{
		var dangerousGoods = default(T);
		if (providerDangerousGoods != null)
		{
			dangerousGoods = new T()
			{
				SequenceNumber = providerDangerousGoods.SequenceNumber,
				UNNumber = providerDangerousGoods.UNDangerousCode
			};
		}
		return dangerousGoods;
	}

	protected T GetPopulatedCommonCommodity<T, TCommodityCode>(INCTSCommonCommodity providerCommodity)
		where T : INCTSCommodityCommon, new()
		where TCommodityCode : INCTSCommodityCode, new()
	{
		var commodity = default(T);
		if (providerCommodity != null)
		{
			commodity = new T()
			{
				DescriptionOfGoods = providerCommodity.DescriptionOfGoods,
				CommodityCode = GetPopulatedCommodityCode(providerCommodity.CommodityCode)
			};
		}

		TCommodityCode GetPopulatedCommodityCode(INCTSCommonCommodityCode providerCommodityCode)
		{
			var commodityCode = default(TCommodityCode);
			if (providerCommodityCode != null)
			{
				commodityCode = new TCommodityCode()
				{
					HarmonizedSystemSubHeadingCode = providerCommodityCode.HarmonizedSystemSubHeadingCode,
					CombinedNomenclatureCode = providerCommodityCode.CombinedNomenclatureCode
				};
			}
			return commodityCode;
		}
		return commodity;
	}

	protected T GetPopulatedCommonCommodityWithCusCode<T, TCommodityCode>(INCTSCommonCommodityWithCusCode providerCommodity)
		where T : INCTSCommodityCommonWithCusCode, new()
		where TCommodityCode : INCTSCommodityCode, new()
	{
		var commodity = GetPopulatedCommonCommodity<T, TCommodityCode>(providerCommodity);
		if (providerCommodity != null)
		{
			commodity.CusCode = providerCommodity.CusCode;
		}
		return commodity;
	}

	protected T GetPopulatedCommonGoodsMeasure<T>(INCTSCommonGoodsMeasure providerGoodsMeasure)
		where T : INCTSGoodsMeasureCommon, new()
	{
		var goodsMeasure = default(T);
		if (providerGoodsMeasure != null)
		{
			goodsMeasure = new T()
			{
				GrossMass = providerGoodsMeasure.GrossMass,
				GrossMassSpecified = providerGoodsMeasure.GrossMassSpecified,
				NetMass = providerGoodsMeasure.NetMass,
				NetMassSpecified = providerGoodsMeasure.NetMassSpecified,
			};
		}
		return goodsMeasure;
	}

	protected T GetPopulatedCommonDocument<T>(ICommonDocumentSequenceNumber providerDocument)
			where T : INCTSDocumentCommon, new()
	{
		var document = default(T);
		if (providerDocument != null)
		{
			document = new T()
			{
				SequenceNumber = providerDocument.SequenceNumber,
				Name = providerDocument.Name,
				Number = providerDocument.Number
			};
		}
		return document;
	}

	protected T GetPopulatedCommonDocumentWithInfo<T>(INCTSCommonDocumentWithInfo providerDocument)
		where T : INCTSDocumentCommonWithInfo, new()
	{
		var document = GetPopulatedCommonDocument<T>(providerDocument);
		if (providerDocument != null)
		{
			document.ComplementaryInformation = providerDocument.ComplementaryInformation;
		}
		return document;
	}

	protected T GetPopulatedCommonDocumentWithItem<T>(INCTSCommonDocumentWithItem providerDocument)
		where T : INCTSDocumentCommonWithItem, new()
	{
		var document = GetPopulatedCommonDocumentWithInfo<T>(providerDocument);
		if (providerDocument != null)
		{
			document.GoodsItemNumber = providerDocument.GoodsItemNumber;
		}
		return document;
	}

	protected T GetPopulatedAnnexesCommon<T>(IAnnexDocCommon providerDocument)
			where T : INCTSAnnexesCommon, new()
	{
		var document = default(T);
		if (providerDocument != null)
		{
			document = new T()
			{
				Description = providerDocument.Description,
				ReferenceNumber = providerDocument.ReferenceNumber,
				Image = providerDocument.Image,
				Extension = providerDocument.Extension
			};
		}

		return document;
	}
}
