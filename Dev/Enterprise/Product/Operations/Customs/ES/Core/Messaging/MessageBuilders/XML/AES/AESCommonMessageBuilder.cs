using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class AESCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : IAESCommonDataProvider
{
	protected AESCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	const string MessageRecipient = "NECA.ES";

	protected const int MaxDecimals2 = 2;

	protected abstract ZString GetMessageType();

	protected T GetPopulatedTransactionId<T>()
		where T : IAESTransactionId, new()
	{
		return new T()
		{
			TransactionId = TransactionId,
		};
	}

	protected T GetPopulatedMessage<T>()
		where T : IAESMessage, new()
	{
		return new T()
		{
			Sender = provider.Message.Sender,
			Recipient = MessageRecipient,
			PreparationDateAndTime = GetDateTimeFromZDateTime(CET),
			MessageIdentification = provider.Message.MessageIdentification,
			MessageType = GetMessageType(),
			IsFinalPeriod = provider.IsFinalPeriod,
			PhaseIDSpecified = provider.PhaseIDSpecified,
		};
	}

	DateTime GetDateTimeFromZDateTime(ZDateTime providerDate) => new DateTime(providerDate.Year, providerDate.Month, providerDate.Day, providerDate.Hour, providerDate.Minute, providerDate.Second);

	protected T GetPopulatedCommonConsignment<T>(IAESCommonConsignment providerConsignment)
		where T : IAESConsignmentCommon, new()
	{
		var consignment = default(T);
		if (providerConsignment != null)
		{
			consignment = new T()
			{
				IsContainerisedFlag = providerConsignment.IsContainerised,
				InlandModeOfTransport = providerConsignment.InlandModeOfTransport
			};
		}
		return consignment;
	}

	protected T GetPopulatedDeclarationExportOperation<T>(IDeclarationAESExportOperation providerExportOperation)
		where T : IAESExportOperationCommon, new()
	{
		var exportOperation = default(T);
		if (providerExportOperation != null)
		{
			exportOperation = new T()
			{
				LRN = providerExportOperation.LRN,
				DeclarationType = providerExportOperation.DeclarationType,
				DeclarationSubType = providerExportOperation.DeclarationSubType,
				RecapitulationDate = GetDateTimeFromZDateTime(providerExportOperation.RecapitulationDate),
				RecapitulationDateSpecified = providerExportOperation.RecapitulationDateSpecified,
				SecurityFlag = providerExportOperation.SecurityFlag,
				SpecificCircumstance = providerExportOperation.SpecificCircumstance,
				TotalAmount = providerExportOperation.TotalAmount.Round(MaxDecimals2),
				Currency = providerExportOperation.Currency,
			};
		}
		return exportOperation;
	}

	protected T GetPopulatedExporter<T, TAddress>(IDeclarationAESExporter providerExporter)
		where T : IAESExporter, new()
		where TAddress : IOrgAddressCommon, new()
	{
		var exporter = GetPopulatedAddressInformationIdCommon<T>(providerExporter);
		if (exporter != null)
		{
			exporter.Address = GetPopulatedAddressCommon<TAddress>(providerExporter.Address);
		}
		return exporter;
	}

	protected T GetPopulatedTransportEquipment<T, TSeal, TGoodsReference>(IAESCommonTransportEquipment providerTransportEquipment)
		where T : IAESTransportEquipment, new()
		where TSeal : IAESSeal, new()
		where TGoodsReference : IGoodsReferenceCommon, new()
	{
		var transportEquipment = GetPopulatedCommonTransportEquipment<T, TGoodsReference>(providerTransportEquipment);
		if (transportEquipment != null)
		{
			var sealsCollection = new Collection<IAESSeal>();
			foreach (var seal in providerTransportEquipment.Seals.ConvertToCollection(GetPopulatedSeal) ?? Enumerable.Empty<TSeal>())
			{
				sealsCollection.Add(seal);
			}

			transportEquipment.NumberOfSeals = providerTransportEquipment.NumberOfSeals;
			transportEquipment.Seals = sealsCollection;
		}
		return transportEquipment;

		TSeal GetPopulatedSeal(ISealCommon providerSeal)
		{
			var seal = default(TSeal);
			if (providerSeal != null)
			{
				seal = new TSeal
				{
					SequenceNumber = providerSeal.SequenceNumber,
					SealNumber = providerSeal.SealNumber,
				};
			}
			return seal;
		}
	}

	protected T GetPopulatedLocationOfGoods<T, TCustomsOffice, TGNSS, TEconomicOperator, TAddress, TPostCodeAddress, TContactPerson>(IAESCommonLocationOfGoods providerLocationOfGoods)
		where T : IAESLocationOfGoods, new()
		where TCustomsOffice : ICommonCustomOffice, new()
		where TGNSS : IGNSSCommon, new()
		where TEconomicOperator : IEconomicOperatorCommon, new()
		where TAddress : IOrgAddressCommon, new()
		where TPostCodeAddress : IPostcodeAddressCommon, new()
		where TContactPerson : ICommonContactPerson, new()
	{
		var locationOfGoods = GetPopulatedCommonLocationOfGoods<T, TCustomsOffice, TGNSS, TEconomicOperator, TAddress, TPostCodeAddress>(providerLocationOfGoods);
		if (locationOfGoods != null)
		{
			locationOfGoods.LocationContactPerson = GetPopulatedContactPerson<TContactPerson>(providerLocationOfGoods.LocationContactPerson);
		}
		return locationOfGoods;
	}

	protected T GetPopulatedDepartureTransportMeans<T>(ICommonDepartureTransportMeans providerDepartureTransportMeans)
		where T : IAESDepartureTransportMeans, new()
	{
		var departureTransportMeans = GetPopulatedTransportMediumInfoCommon<T>(providerDepartureTransportMeans);
		if (departureTransportMeans != null)
		{
			departureTransportMeans.SequenceNumber = providerDepartureTransportMeans.SequenceNumber;
		}
		return departureTransportMeans;
	}

	protected T GetPopulatedDeclarationGoodsShipment<T, TChainActor, TDelivery, TWarehouse, TSupDoc, TAddRef, TAddInfo, TConsignment, TConsignee, TTranspEqu, TLocation, TLine, TCommodity>(IDeclarationAESGoodsShipment providerGoodsShipment,
																																						Func<IDeclarationAESConsignment, Func<IDeclarationAESConsignee, TConsignee>, Func<IAESCommonTransportEquipment, TTranspEqu>, Func<IAESCommonLocationOfGoods, TLocation>, TConsignment> getConsignment,
																																						Func<IDeclarationAESConsignee, TConsignee> getConsignee,
																																						Func<IAESCommonTransportEquipment, TTranspEqu> getTransportEquipment,
																																						Func<IAESCommonLocationOfGoods, TLocation> getLocationOfGoods,
																																						Func<IDeclarationAESLine, Func<IDeclarationAESConsignee, TConsignee>, Func<IDeclarationAESCommodity, TCommodity>, TLine> getLine,
																																						Func<IDeclarationAESCommodity, TCommodity> getCommodity)
		where T : IAESGoodsShipmentDeclaration, new()
		where TChainActor : IAdditionalSupplyChainActorWithSeqNumCommon, new()
		where TDelivery : IDeliverytermsDeclarationCommon, new()
		where TWarehouse : IWarehouseDeclarationCommon, new()
		where TSupDoc : IAESSupportingDocument, new()
		where TAddRef : IDocumentSequenceNumberCommon, new()
		where TAddInfo : IDocumentSequenceNumberCommon, new()
		where TConsignment : IAESConsigmentDeclaration, new()
		where TConsignee : IAESConsignee, new()
		where TTranspEqu : IAESTransportEquipment, new()
		where TLocation : IAESLocationOfGoods, new()
		where TLine : IAESLineDeclaration, new()
		where TCommodity : IAESCommodityDeclaration, new()
	{
		var goodsShipment = default(T);
		if (providerGoodsShipment != null)
		{
			var supplyChainActorsCollection = new Collection<IAdditionalSupplyChainActorWithSeqNumCommon>();
			foreach (var supplyChainActor in providerGoodsShipment.AdditionalSupplyActors.ConvertToCollection(GetPopulatedAdditionalSupplyActor<TChainActor>) ?? Enumerable.Empty<TChainActor>())
			{
				supplyChainActorsCollection.Add(supplyChainActor);
			}

			var supportingDocumentsCollection = new Collection<IAESSupportingDocument>();
			foreach (var supportingDocument in providerGoodsShipment.SupportingDocuments.ConvertToCollection(GetPopulatedSupportingDocument) ?? Enumerable.Empty<TSupDoc>())
			{
				supportingDocumentsCollection.Add(supportingDocument);
			}

			var additionalReferencesCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var additionalReference in providerGoodsShipment.AdditionalReferences.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TAddRef>) ?? Enumerable.Empty<TAddRef>())
			{
				additionalReferencesCollection.Add(additionalReference);
			}

			var additionalInfosCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var additionalInfo in providerGoodsShipment.AdditionalInfos.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TAddInfo>) ?? Enumerable.Empty<TAddInfo>())
			{
				additionalInfosCollection.Add(additionalInfo);
			}

			var linesCollection = new Collection<IAESLineDeclaration>();
			foreach (var providerLine in providerGoodsShipment.Lines ?? Enumerable.Empty<IDeclarationAESLine>())
			{
				var lineToAdd = getLine(providerLine, getConsignee, getCommodity);
				linesCollection.Add(lineToAdd);
			}

			goodsShipment = new T()
			{
				NatureOfTransaction = providerGoodsShipment.NatureOfTransaction,
				CountryOfExport = providerGoodsShipment.CountryOfExport,
				CountryOfDestination = providerGoodsShipment.CountryOfDestination,
				AdditionalSupplyActors = supplyChainActorsCollection,
				DeliveryTerms = GetPopulatedDeliveryTerms<TDelivery>(providerGoodsShipment.DeliveryTerms),
				Warehouse = GetPopulatedWarehouse<TWarehouse>(providerGoodsShipment.Warehouse),
				SupportingDocuments = supportingDocumentsCollection,
				AdditionalReferences = additionalReferencesCollection,
				AdditionalInfos = additionalInfosCollection,
				Consignment = getConsignment(providerGoodsShipment.Consignment, getConsignee, getTransportEquipment, getLocationOfGoods),
				Lines = linesCollection,
			};
		}
		return goodsShipment;

		TSupDoc GetPopulatedSupportingDocument(IDeclarationAESSupportingDocumentHeader documentProvider)
		{
			var document = GetPopulatedCommonLineNumberDocument<TSupDoc>(documentProvider);
			if (document != null)
			{
				GetPopulatedCommonSupportingDocumentExtraFields(documentProvider.CommonSupportingDocumentExtraFields, document);
			}
			return document;
		}
	}

	protected T GetPopulatedDeclarationConsignment<T, TCarrier, TConsignor, TConsignee, TTranspEqu, TLocation, TDepTranspMeans, TCountryOfRouting, TBorderTransp, TTranspDoc, TTranspCharges>(IDeclarationAESConsignment providerConsignment,
																																										Func<IDeclarationAESConsignee, TConsignee> getConsignee,
																																										Func<IAESCommonTransportEquipment, TTranspEqu> getTransportEquipment,
																																										Func<IAESCommonLocationOfGoods, TLocation> getLocationOfGoods)
		where T : IAESConsigmentDeclaration, new()
		where TCarrier : IOrgAddressInfoIdCommon, new()
		where TConsignor : IOrgAddressInfoIdCommon, new()
		where TConsignee : IAESConsignee, new()
		where TTranspEqu : IAESTransportEquipment, new()
		where TLocation : IAESLocationOfGoods, new()
		where TDepTranspMeans : IAESDepartureTransportMeans, new()
		where TCountryOfRouting : IAESCountryOfRoutingOfConsignmentDeclaration, new()
		where TBorderTransp : ICommonTransportMediumInfo, new()
		where TTranspDoc : IDocumentSequenceNumberCommon, new()
		where TTranspCharges : IAESTransportCharges, new()
	{
		var consignment = GetPopulatedCommonConsignment<T>(providerConsignment);
		if (consignment != null)
		{
			var transportEquipmentCollection = new Collection<IAESTransportEquipment>();
			foreach (var transportEquipment in providerConsignment.TransportEquipment.ConvertToCollection(getTransportEquipment) ?? Enumerable.Empty<TTranspEqu>())
			{
				transportEquipmentCollection.Add(transportEquipment);
			}

			var departureTransportMeansCollection = new Collection<IAESDepartureTransportMeans>();
			foreach (var departureTransportMeans in providerConsignment.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<TDepTranspMeans>) ?? Enumerable.Empty<TDepTranspMeans>())
			{
				departureTransportMeansCollection.Add(departureTransportMeans);
			}

			var countryOfRoutingOfConsignmentsCollection = new Collection<IAESCountryOfRoutingOfConsignmentDeclaration>();
			foreach (var countryOfRoutingOfConsignments in providerConsignment.CountryOfRoutingOfConsignments.ConvertToCollection(GetPopulatedCountryOfRouting) ?? Enumerable.Empty<TCountryOfRouting>())
			{
				countryOfRoutingOfConsignmentsCollection.Add(countryOfRoutingOfConsignments);
			}

			var transportDocumentsCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var transportDocument in providerConsignment.TransportDocuments.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TTranspDoc>) ?? Enumerable.Empty<TTranspDoc>())
			{
				transportDocumentsCollection.Add(transportDocument);
			}

			consignment.ModeOfTransportAtBorder = providerConsignment.ModeOfTransportAtBorder;
			consignment.GrossMass = providerConsignment.GrossMass;
			consignment.ReferenceNumberUCR = providerConsignment.ReferenceNumberUCR;
			consignment.Carrier = GetPopulatedAddressInformationIdCommon<TCarrier>(providerConsignment.Carrier);
			consignment.Consignor = GetPopulatedAddressInformationIdCommon<TConsignor>(providerConsignment.Consignor);
			consignment.Consignee = getConsignee(providerConsignment.Consignee);
			consignment.TransportEquipment = transportEquipmentCollection;
			consignment.LocationOfGoods = getLocationOfGoods(providerConsignment.LocationOfGoods);
			consignment.DepartureTransportMeans = departureTransportMeansCollection;
			consignment.CountryOfRoutingOfConsignments = countryOfRoutingOfConsignmentsCollection;
			consignment.ActiveBorderTransportMeans = GetPopulatedTransportMediumInfoCommon<TBorderTransp>(providerConsignment.ActiveBorderTransportMeans);
			consignment.TransportDocuments = transportDocumentsCollection;
			consignment.TransportCharges = GetPopulatedTransportCharges<TTranspCharges>(providerConsignment.TransportChargesMoP);
		}
		return consignment;

		TCountryOfRouting GetPopulatedCountryOfRouting(ICommonCountryOfRoutingOfConsignment providerCountryOfRouting)
		{
			var countryOfRouting = default(TCountryOfRouting);
			if (providerCountryOfRouting != null)
			{
				countryOfRouting = new TCountryOfRouting()
				{
					SequenceNumber = providerCountryOfRouting.SequenceNumber,
					CountryOfRouting = providerCountryOfRouting.CountryOfRouting,
				};
			}
			return countryOfRouting;
		}
	}

	protected T GetPopulatedTransportCharges<T>(ZString transportChargesMoP)
	where T : IAESTransportCharges, new()
	{
		var transportCharges = default(T);
		if (!transportChargesMoP.IsEmpty)
		{
			transportCharges = new T()
			{
				MethodOfPayment = transportChargesMoP
			};
		}
		return transportCharges;
	}

	protected T GetPopulatedConsignee<T, TAddress>(IDeclarationAESConsignee providerConsignee)
		where T : IAESConsignee, new()
		where TAddress : IOrgAddressCommon, new()
	{
		var consignee = GetPopulatedAddressInformationDeclarantCommon<T>(providerConsignee);
		if (consignee != null)
		{
			consignee.Address = GetPopulatedAddressCommon<TAddress>(providerConsignee.Address);
		}
		return consignee;
	}

	protected T GetPopulatedDeclarationLine<T, TAuth, TProcedure, TAddProcedure, TConsignor, TConsignee, TChainActor, TOrigin, TCommodity, TPackage, TPrevDoc, TSupDoc, TTranspDoc, TAddRef, TAddInfo>(IDeclarationAESLine providerLine,
																																													Func<IDeclarationAESConsignee, TConsignee> getConsignee,
																																													Func<IDeclarationAESCommodity, TCommodity> getCommodity)
		where T : IAESLineDeclaration, new()
		where TAuth : IAuthorisationCommon, new()
		where TProcedure : IAESProcedureDeclaration, new()
		where TAddProcedure : IAdditionalCodeCommon, new()
		where TConsignor : IOrgAddressInfoIdCommon, new()
		where TConsignee : IAESConsignee, new()
		where TChainActor : IAdditionalSupplyChainActorWithSeqNumCommon, new()
		where TOrigin : IAESOrigin, new()
		where TCommodity : IAESCommodityDeclaration, new()
		where TPackage : IPackageWithSequenceAndPackNumCommon, new()
		where TPrevDoc : IAESDocumentCommon, new()
		where TSupDoc : IAESSupportingDocumentDeclarationLine, new()
		where TTranspDoc : IDocumentSequenceNumberCommon, new()
		where TAddRef : IDocumentSequenceNumberCommon, new()
		where TAddInfo : IDocumentSequenceNumberCommon, new()
	{
		var line = default(T);
		if (providerLine != null)
		{
			var authorisationsCollection = new Collection<IAuthorisationCommon>();
			foreach (var auth in providerLine.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<TAuth>) ?? Enumerable.Empty<TAuth>())
			{
				authorisationsCollection.Add(auth);
			}

			var supplyChainActorsCollection = new Collection<IAdditionalSupplyChainActorWithSeqNumCommon>();
			foreach (var supplyChainActor in providerLine.AdditionalSupplyActors.ConvertToCollection(GetPopulatedAdditionalSupplyActor<TChainActor>) ?? Enumerable.Empty<TChainActor>())
			{
				supplyChainActorsCollection.Add(supplyChainActor);
			}

			var internalPackagesCollection = new Collection<IPackageWithSequenceAndPackNumCommon>();
			foreach (var package in providerLine.InternalPackages.ConvertToCollection(GetPopulatedCommonPackage<TPackage>) ?? Enumerable.Empty<TPackage>())
			{
				internalPackagesCollection.Add(package);
			}

			var previousDocumentsCollection = new Collection<IAESDocumentCommon>();
			foreach (var previousDocument in providerLine.PreviousDocuments.ConvertToCollection(GetPopulatedCommonDocument<TPrevDoc>) ?? Enumerable.Empty<TPrevDoc>())
			{
				previousDocumentsCollection.Add(previousDocument);
			}

			var supportingDocumentsCollection = new Collection<IAESSupportingDocumentDeclarationLine>();
			foreach (var supportingDocument in providerLine.SupportingDocuments.ConvertToCollection(GetPopulatedSupportingDocument) ?? Enumerable.Empty<TSupDoc>())
			{
				supportingDocumentsCollection.Add(supportingDocument);
			}

			var transportDocumentsCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var transportDocument in providerLine.TransportDocuments.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TTranspDoc>) ?? Enumerable.Empty<TTranspDoc>())
			{
				transportDocumentsCollection.Add(transportDocument);
			}

			var additionalReferencesCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var additionalReference in providerLine.AdditionalReferences.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TAddRef>) ?? Enumerable.Empty<TAddRef>())
			{
				additionalReferencesCollection.Add(additionalReference);
			}

			var additionalInfosCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var additionalInfo in providerLine.AdditionalInfos.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TAddInfo>) ?? Enumerable.Empty<TAddInfo>())
			{
				additionalInfosCollection.Add(additionalInfo);
			}

			line = new T()
			{
				SequenceNumber = providerLine.SequenceNumber,
				StatisticalValue = providerLine.StatisticalValue.Round(MaxDecimals2),
				UCRReferenceNumber = providerLine.UCRReferenceNumber,
				Authorisations = authorisationsCollection,
				Procedure = GetPopulatedProcedure(),
				Consignor = GetPopulatedAddressInformationIdCommon<TConsignor>(providerLine.Consignor),
				Consignee = getConsignee(providerLine.Consignee),
				AdditionalSupplyActors = supplyChainActorsCollection,
				Origin = GetPopulatedOrigin<TOrigin>(providerLine.Origin),
				Commodity = getCommodity(providerLine.Commodity),
				InternalPackages = internalPackagesCollection,
				PreviousDocuments = previousDocumentsCollection,
				SupportingDocuments = supportingDocumentsCollection,
				TransportDocuments = transportDocumentsCollection,
				AdditionalReferences = additionalReferencesCollection,
				AdditionalInfos = additionalInfosCollection,
			};
		}
		return line;

		TProcedure GetPopulatedProcedure()
		{
			var providerProcedure = providerLine.Procedure;
			var procedure = default(TProcedure);
			if (providerProcedure != null)
			{
				var additionalProceduresCollection = new Collection<IAdditionalCodeCommon>();
				foreach (var addProcedure in providerProcedure.AdditionalProcedures.ConvertToCollection(GetPopulatedCommonAdditionalCode<TAddProcedure>) ?? Enumerable.Empty<TAddProcedure>())
				{
					additionalProceduresCollection.Add(addProcedure);
				}

				procedure = new TProcedure()
				{
					RequestedCPC = providerProcedure.RequestedCPC,
					PreviousCPC = providerProcedure.PreviousCPC,
					AdditionalProcedures = additionalProceduresCollection,
				};
			}
			return procedure;
		}

		TSupDoc GetPopulatedSupportingDocument(IDeclarationAESSupportingDocumentLine documentProvider)
		{
			var document = GetPopulatedCommonDocument<TSupDoc>(documentProvider);
			if (document != null)
			{
				GetPopulatedCommonSupportingDocumentExtraFields(documentProvider.CommonSupportingDocumentExtraFields, document);
			}
			return document;
		}
	}

	protected void GetPopulatedCommonSupportingDocumentExtraFields(IAESCommonSupportingDocumentExtraFields commonDocumentProvider, IAESSupportingDocumentExtraFields document)
	{
		if (commonDocumentProvider != null)
		{
			document.IssuingAuthorityName = commonDocumentProvider.IssuingAuthorityName;
			document.DocumentDate = commonDocumentProvider.DocumentDate;
			document.DocumentDateSpecified = commonDocumentProvider.DocumentDateSpecified;
		}
	}

	protected T GetPopulatedCommonLineNumberDocument<T>(IAESCommonLineNumberDocument commonDocumentProvider)
		where T : IAESDocumentLineNumberCommon, new()
	{
		var commonDocument = GetPopulatedCommonDocumentSequenceNumber<T>(commonDocumentProvider);
		if (commonDocument != null)
		{
			commonDocument.LineNumber = commonDocumentProvider.LineNumber;
		}
		return commonDocument;
	}

	protected T GetPopulatedCommonDocument<T>(IAESCommonDocument commonDocumentProvider)
		where T : IAESDocumentCommon, new()
	{
		var commonDocument = GetPopulatedCommonLineNumberDocument<T>(commonDocumentProvider);
		if (commonDocument != null)
		{
			commonDocument.Measurement = commonDocumentProvider.Measurement;
			commonDocument.Quantity = commonDocumentProvider.Quantity.Round(MaxDecimals2);
			commonDocument.QuantitySpecified = commonDocumentProvider.QuantitySpecified;
		}
		return commonDocument;
	}

	protected T GetPopulatedOrigin<T>(IAESCommonOrigin providerOrigin)
		where T : IAESOrigin, new()
	{
		var origin = default(T);
		if (providerOrigin != null)
		{
			origin = new T()
			{
				CountryOfOrigin = providerOrigin.CountryOfOrigin,
				StateOfOrigin = providerOrigin.StateOfOrigin,
			};
		}
		return origin;
	}

	protected T GetPopulatedDeclarationCommodity<T, TCommodityCode, TAddTariff, TAddNational, TDangerourGoods, TGoodsMeasure>(IDeclarationAESCommodity providerCommodity)
		where T : IAESCommodityDeclaration, new()
		where TCommodityCode : IAESCommodityCodeDeclaration, new()
		where TAddTariff : IAdditionalCodeCommon, new()
		where TAddNational : IAdditionalCodeCommon, new()
		where TDangerourGoods : IAESDangerousGoods, new()
		where TGoodsMeasure : IGoodsMeasureCommonWithSpecifiedWithSupUnits, new()
	{
		var commodity = default(T);
		if (providerCommodity != null)
		{
			var dangerousGoodsCollection = new Collection<IAESDangerousGoods>();
			foreach (var dangerousGoods in providerCommodity.DangerousGoods.ConvertToCollection(GetPopulatedDangerousGoods) ?? Enumerable.Empty<TDangerourGoods>())
			{
				dangerousGoodsCollection.Add(dangerousGoods);
			}

			commodity = new T()
			{
				GoodsDescription = providerCommodity.GoodsDescription,
				CusCode = providerCommodity.CusCode,
				CommodityCode = GetPopulatedCommodityCode(),
				DangerousGoods = dangerousGoodsCollection,
				GoodsMeasure = GetPopulatedCommonGoodsMeasureWithSupUnitsAndSpecified<TGoodsMeasure>(providerCommodity.GoodsMeasure),
			};
		}
		return commodity;

		TCommodityCode GetPopulatedCommodityCode()
		{
			var providerCommodityCode = providerCommodity.CommodityCode;
			var commodityCode = default(TCommodityCode);
			if (providerCommodityCode != null)
			{
				var tariffAdditionalCodesCollection = new Collection<IAdditionalCodeCommon>();
				foreach (var tariffAddCode in providerCommodityCode.TariffAdditionalCodes.ConvertToCollection(GetPopulatedCommonAdditionalCode<TAddTariff>) ?? Enumerable.Empty<TAddTariff>())
				{
					tariffAdditionalCodesCollection.Add(tariffAddCode);
				}

				var nationalAdditionalCodesCollection = new Collection<IAdditionalCodeCommon>();
				foreach (var nationalAddCode in providerCommodityCode.NationalAdditionalCodes.ConvertToCollection(GetPopulatedCommonAdditionalCode<TAddNational>) ?? Enumerable.Empty<TAddNational>())
				{
					nationalAdditionalCodesCollection.Add(nationalAddCode);
				}

				commodityCode = new TCommodityCode()
				{
					TariffCode = providerCommodityCode.TariffCode,
					TariffCodeCombined = providerCommodityCode.TariffCodeCombined,
					TariffAdditionalCodes = tariffAdditionalCodesCollection,
					NationalAdditionalCodes = nationalAdditionalCodesCollection,
				};
			}
			return commodityCode;
		}

		TDangerourGoods GetPopulatedDangerousGoods(ICommonDangerousGoods providerDangerousGoods)
		{
			var dangerousGoods = default(TDangerourGoods);
			if (providerDangerousGoods != null)
			{
				dangerousGoods = new TDangerourGoods()
				{
					SequenceNumber = providerDangerousGoods.SequenceNumber,
					UNDangerousCode = providerDangerousGoods.UNDangerousCode,
				};
			}
			return dangerousGoods;
		}
	}
}
