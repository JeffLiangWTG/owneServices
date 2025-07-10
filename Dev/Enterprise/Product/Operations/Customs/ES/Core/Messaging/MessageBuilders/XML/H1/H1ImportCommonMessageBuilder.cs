using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class H1ImportCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : IH1ImportCommonDataProvider
{
	protected H1ImportCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	readonly string isContainerised = "1";
	readonly string isNotContainerised = "0";

	protected MessageTypeD GetPopulatedMessage()
	{
		return new MessageTypeD
		{
			MessageIdentification = TransactionId,
			PreparationDateAndTime = CET.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss),
		};
	}

	protected T GetPopulatedCommonImportOperation<T>(IH1CommonImportOperation importOperationProvider)
		where T : IH1ImportOperationCommon, new()
	{
		var importOperation = default(T);
		if (importOperationProvider != null)
		{
			importOperation = new T()
			{
				Lrn = importOperationProvider.LRN,
				DeclarationType = importOperationProvider.DeclarationType,
			};
		}
		return importOperation;
	}

	protected MCciOperationType04 GetPopulatedImportOperation(IH1ImportOperation importOperationProvider)
	{
		var importOperation = GetPopulatedCommonImportOperation<MCciOperationType04>(importOperationProvider);
		if (importOperation != null)
		{
			importOperation.AdditionalDeclarationType = importOperationProvider.AdditionalDeclarationType;
			importOperation.LanguageCode = importOperationProvider.LanguageCode;
		}
		return importOperation;
	}

	protected T GetPopulatedPartyWithAddress<T, TAddress>(IH1PartyProviderWithAddress partyProvider)
		where T : IH1OrgAddressInfo, new()
		where TAddress : IOrgAddressCommon, new()
	{
		var partyWithAddress = GetPopulatedAddressInformationDeclarantCommon<T>(partyProvider);
		if (partyWithAddress != null)
		{
			partyWithAddress.Address = GetPopulatedAddressCommon<TAddress>(partyProvider.Address);
		}
		return partyWithAddress;
	}

	protected MDeferredPaymentType GetPopulatedDeferredPayment(IH1DeferredPayment deferredPaymentProvider)
	{
		return deferredPaymentProvider == null ? null : new MDeferredPaymentType
		{
			SequenceNumber = deferredPaymentProvider.SequenceNumber,
			DeferredPayment = deferredPaymentProvider.Payment,
			CcQualifier = deferredPaymentProvider.CcQualifier,
		};
	}

	protected MCountryOfDispatchType GetPopulatedCountryOfDispatch(ZString country)
	{
		return country.IsEmpty ? null : new MCountryOfDispatchType
		{
			CountryOfDispatch = country,
		};
	}

	protected MDestinationType GetPopulatedDestination(IH1Destination destinationProvider)
	{
		return destinationProvider == null ? null : new MDestinationType
		{
			CountryOfDestination = destinationProvider.Country,
			RegionOfDestination = destinationProvider.Region,
			CcQualifier = destinationProvider.CcQualifier,
		};
	}

	protected T GetPopulatedH1CommonDocument<T>(IH1CommonDocument docProvider)
		where T : IH1DocumentSequenceNumberCommon, new()
	{
		var doc = default(T);
		if (provider != null)
		{
			doc = new T()
			{
				Name = docProvider.Name,
				Number = docProvider.Number,
				SequenceNumber = docProvider.SequenceNumber,
				CcQualifier = docProvider.CcQualifier,
			};
		}
		return doc;
	}

	protected T GetPopulatedH1CommonPreviousDocument<T>(IH1CommonPreviousDocument docProvider)
		where T : IH1PreviousDocumentCommon, new()
	{
		var doc = GetPopulatedH1CommonDocument<T>(docProvider);
		if (doc != null)
		{
			doc.TypeOfPackages = docProvider.TypeOfPackages;
			doc.NumberOfPackages = docProvider.NumberOfPackages;
			doc.MeasurementUnitAndQualifier = docProvider.MeasurementUnitAndQualifier;
			doc.CcQualifierForMeasurementUnitAndQualifier = docProvider.CcQualifierForMeasurementUnitAndQualifier;
			doc.Quantity = docProvider.Quantity;
			doc.GoodsItemIdentifier = docProvider.GoodsItemId;
		}
		return doc;
	}

	protected T GetPopulatedH1CommonSupportingDocument<T>(IH1CommonSupportingDocument docProvider)
		where T : IH1SupportingDocumentCommon, new()
	{
		var doc = GetPopulatedH1CommonDocument<T>(docProvider);
		if (doc != null)
		{
			doc.IssuingAuthorityName = docProvider.IssuingAuthorityName;
			doc.LineNumber = docProvider.LineNumber;
			doc.DocumentDate = docProvider.DocumentDate.ToCustomsFormatString(CustomsDateTimeExtension.DateFormatWithDash);
		}
		return doc;
	}

	protected T GetPopulatedH1CommonLineSupportingDocument<T>(IH1CommonLineSupportingDocument docProvider)
		where T : IH1LineSupportingDocumentCommon, new()
	{
		var doc = GetPopulatedH1CommonSupportingDocument<T>(docProvider);
		if (doc != null)
		{
			doc.MeasurementUnitAndQualifier = docProvider.MeasurementUnitAndQualifier;
			doc.CcQualifierForMeasurementUnitAndQualifier = docProvider.CcQualifierForMeasurementUnitAndQualifier;
			doc.Quantity = docProvider.Quantity;
			doc.Currency = docProvider.Currency;
			doc.Amount = docProvider.Amount;
		}
		return doc;
	}

	protected MAdditionalFiscalReferenceType GetPopulatedH1AdditionalFiscalReference(IH1AdditionalFiscalReference fiscalRefProvider)
	{
		return fiscalRefProvider == null ? null : new MAdditionalFiscalReferenceType
		{
			SequenceNumber = fiscalRefProvider.SequenceNumber,
			Role = fiscalRefProvider.Role,
			VatIdentificationNumber = fiscalRefProvider.VAT,
		};
	}

	protected MArrivalTransportMeansType GetPopulatedArrivalTransportMeans(ICommonArrivalTransportMeans arrivalTransportMeansProvider)
	{
		return arrivalTransportMeansProvider == null ? null : new MArrivalTransportMeansType
		{
			TypeOfIdentification = arrivalTransportMeansProvider.Type,
			IdentificationNumber = arrivalTransportMeansProvider.Id,
		};
	}

	protected T GetPopulatedCommonProcedure<T>(ICommonH1Procedure procedureProvider)
		where T : IH1ProcedureCommon, new()
	{
		var procedure = default(T);
		if (procedureProvider != null)
		{
			procedure = new T()
			{
				RequestedProcedure = procedureProvider.RequestedCPC,
				PreviousProcedure = procedureProvider.PreviousCPC,
			};
		}
		return procedure;
	}

	protected MProcedureType02 GetPopulatedProcedure(IH1Procedure procedureProvider)
	{
		var procedure = GetPopulatedCommonProcedure<MProcedureType02>(procedureProvider);
		if (procedure != null)
		{
			procedure.AdditionalProcedure = procedureProvider.AdditionalProcedures.ConvertToCollection(GetPopulatedH1AdditionalCode<MAdditionalProcedureType>);
		}
		return procedure;
	}

	protected T GetPopulatedH1AdditionalCode<T>(IH1AdditionalCode additionalCodeProvider)
		where T : IH1AdditionalCodeCommon, new()
	{
		var doc = default(T);
		if (provider != null)
		{
			doc = new T()
			{
				SequenceNumber = additionalCodeProvider.SequenceNumber,
				Code = additionalCodeProvider.Code,
				CcQualifier = additionalCodeProvider.CcQualifier,
			};
		}
		return doc;
	}

	protected T GetPopulatedCommonOrigin<T>(ZString country)
		where T : IH1OriginCommon, new()
	{
		var origin = default(T);
		if (!country.IsEmpty)
		{
			origin = new T()
			{
				CountryOfOrigin = country,
			};
		}
		return origin;
	}

	protected MOriginType GetPopulatedOrigin(IH1Origin originProvider)
	{
		if (originProvider == null)
		{
			return null;
		}
		var origin = GetPopulatedCommonOrigin<MOriginType>(originProvider.Country);
		if (origin != null)
		{
			origin.CountryOfPreferentialOrigin = originProvider.PreferentialCountry;
		}
		return origin;
	}

	protected T GetPopulatedCommonCommodity<T>(ICommonH1Commodity commodityProvider)
	where T : IH1CommodityCommon, new()
	{
		var commodity = default(T);
		if (commodityProvider != null)
		{
			commodity = new T()
			{
				GoodsDescription = commodityProvider.GoodsDescription,
			};
		}
		return commodity;
	}

	protected T GetPopulatedCompleteAndSimplifiedCommodity<T>(IH1CompleteAndSimplifiedCommonImportCommodity commodityProvider)
		where T : IH1Commodity, new()
	{
		var commodity = GetPopulatedCommonCommodity<T>(commodityProvider);
		if (commodity != null)
		{
			commodity.CusCode = commodityProvider.CusCode;
			commodity.QuotaOrderNumber = commodityProvider.QuotaOrderNumber;
			commodity.CommodityCode = GetPopulatedCommodityCode(commodityProvider.CommodityCode);
			commodity.GoodsMeasure = GetPopulatedCommonGoodsMeasureWithSupUnitsAndSpecified<MGoodsMeasureType01>(commodityProvider.GoodsMeasure);
			commodity.InvoiceLine = new MInvoiceLineType
			{
				ItemAmountInvoiced = commodityProvider.InvoiceLineAmountInvoiced,
			};
		}
		return commodity;
	}

	protected T GetPopulatedCommonCommodityCode<T>(ICommonH1CommodityCode commodityCodeProvider)
		where T : IH1CommodityCodeCommon, new()
	{
		var commodityCode = GetPopulatedCommodityCodeCommon<T>(commodityCodeProvider);
		if (commodityCode != null)
		{
			commodityCode.TaricCode = commodityCodeProvider.TaricCode;
		}
		return commodityCode;
	}

	protected MCommodityCodeType03 GetPopulatedCommodityCode(IH1CommodityCode commodityCodeProvider)
	{
		var commodityCode = GetPopulatedCommonCommodityCode<MCommodityCodeType03>(commodityCodeProvider);
		if (commodityCode != null)
		{
			commodityCode.TaricAdditionalCode = commodityCodeProvider.TariffAdditionalCodes.ConvertToCollection(GetPopulatedCommonAdditionalCode<MTaricAdditionalCodeType>);
			commodityCode.NationalAdditionalCode = commodityCodeProvider.NationalAdditionalCodes.ConvertToCollection(GetPopulatedH1AdditionalCode<MNationalAdditionalCodeType>);
		}
		return commodityCode;
	}

	protected MCalculationOfTaxesType03 GetPopulatedCalculationOfTaxes(IH1CalculationOfTaxes calculationOfTaxesProvider)
	{
		return calculationOfTaxesProvider == null ? null : new MCalculationOfTaxesType03
		{
			Preference = calculationOfTaxesProvider.Preference,
			DutiesAndTaxes = calculationOfTaxesProvider.DutiesAndTaxes.ConvertToCollection(GetPopulatedDutiesAndTaxes),
		};
	}

	protected MDutiesAndTaxesType03 GetPopulatedDutiesAndTaxes(IH1DutiesAndTaxes dutiesAndTaxesProvider)
	{
		return dutiesAndTaxesProvider == null ? null : new MDutiesAndTaxesType03
		{
			SequenceNumber = dutiesAndTaxesProvider.SequenceNumber,
			TaxType = dutiesAndTaxesProvider.Type,
			CcQualifier = dutiesAndTaxesProvider.CcQualifier,
			MethodOfPayment = dutiesAndTaxesProvider.MethodOfPayment,
			TaxBase = dutiesAndTaxesProvider.TaxBases.ConvertToCollection(GetPopulatedTaxBase),
		};
	}

	protected MTaxBaseType01 GetPopulatedTaxBase(IH1TaxBase taxBaseProvider)
	{
		return taxBaseProvider == null ? null : new MTaxBaseType01
		{
			SequenceNumber = taxBaseProvider.SequenceNumber,
			TaxRate = taxBaseProvider.Rate,
			MeasurementUnitAndQualifier = taxBaseProvider.MeasurementUnitAndQualifier,
			CcQualifierForMeasurementUnitAndQualifier = taxBaseProvider.CcQualifierForMeasurementUnitAndQualifier,
			Quantity = taxBaseProvider.Quantity,
			Amount = taxBaseProvider.Amount,
			TaxAmount = taxBaseProvider.TaxAmount,
		};
	}

	protected MCustomsValuationType GetPopulatedCustomsValuation(IH1CustomsValuation customsValuationProvider)
	{
		return customsValuationProvider == null ? null : new MCustomsValuationType
		{
			ValuationMethod = customsValuationProvider.ValuationMethod,
			AdditionsAndDeductions = customsValuationProvider.AdditionsAndDeductions.ConvertToCollection(GetPopulatedAdditionsAndDeductions),
		};
	}

	protected MAdditionsAndDeductionsType GetPopulatedAdditionsAndDeductions(IH1AdditionsAndDeductions additionsAndDeductionsProvider)
	{
		return additionsAndDeductionsProvider == null ? null : new MAdditionsAndDeductionsType
		{
			SequenceNumber = additionsAndDeductionsProvider.SequenceNumber,
			Code = additionsAndDeductionsProvider.Code,
			Amount = additionsAndDeductionsProvider.Amount,
		};
	}

	protected T GetPopulatedCommonGoodsShipment<T, TAdditionalSupplyActors, TPreviousDocument, TSupportingDocument, TAdditionalRef, TAdditionalInfo>(ICommonImportH1GoodsShipment goodsShipmentProvider)
		where T : IH1GoodsShipmentCommon, new()
		where TAdditionalSupplyActors : IAdditionalSupplyChainActorWithSeqNumCommon, new()
		where TPreviousDocument : IH1DocumentSequenceNumberCommon, new()
		where TSupportingDocument : IH1SupportingDocumentCommon, new()
		where TAdditionalRef : IH1DocumentSequenceNumberCommon, new()
		where TAdditionalInfo : IH1DocumentSequenceNumberCommon, new()
	{
		var goodsShipment = default(T);
		if (goodsShipmentProvider != null)
		{
			var additionalSupplyChainActorsCollection = new Collection<IAdditionalSupplyChainActorWithSeqNumCommon>();
			foreach (var additionalSupplyChainActor in goodsShipmentProvider.AdditionalSupplyChainActors.ConvertToCollection(GetPopulatedAdditionalSupplyActor<TAdditionalSupplyActors>) ?? Enumerable.Empty<TAdditionalSupplyActors>())
			{
				additionalSupplyChainActorsCollection.Add(additionalSupplyChainActor);
			}
			var previousDocumentsCollection = new Collection<IH1DocumentSequenceNumberCommon>();
			foreach (var previousDocument in goodsShipmentProvider.PreviousDocuments.ConvertToCollection(GetPopulatedH1CommonDocument<TPreviousDocument>) ?? Enumerable.Empty<TPreviousDocument>())
			{
				previousDocumentsCollection.Add(previousDocument);
			}
			var supportingDocumentsCollection = new Collection<IH1SupportingDocumentCommon>();
			foreach (var supportingDocument in goodsShipmentProvider.SupportingDocuments.ConvertToCollection(GetPopulatedH1CommonSupportingDocument<TSupportingDocument>) ?? Enumerable.Empty<TSupportingDocument>())
			{
				supportingDocumentsCollection.Add(supportingDocument);
			}
			var additionalReferencesCollection = new Collection<IH1DocumentSequenceNumberCommon>();
			foreach (var additionalReference in goodsShipmentProvider.AdditionalReferences.ConvertToCollection(GetPopulatedH1CommonDocument<TAdditionalRef>) ?? Enumerable.Empty<TAdditionalRef>())
			{
				additionalReferencesCollection.Add(additionalReference);
			}
			var additionalInfosCollection = new Collection<IH1DocumentSequenceNumberCommon>();
			foreach (var additionalInfo in goodsShipmentProvider.AdditionalInfos.ConvertToCollection(GetPopulatedH1CommonDocument<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfosCollection.Add(additionalInfo);
			}

			goodsShipment = new T()
			{
				InvoiceCurrency = goodsShipmentProvider.InvoiceCurrency,
				ExchangeRate = goodsShipmentProvider.ExchangeRate,
				AdditionalSupplyActors = additionalSupplyChainActorsCollection,
				Exporter = GetPopulatedPartyWithAddress<MExporterType, MAddressType01>(goodsShipmentProvider.Exporter),
				CountryOfDispatch = GetPopulatedCountryOfDispatch(goodsShipmentProvider.CountryOfDispatch),
				PreviousDocuments = previousDocumentsCollection,
				SupportingDocuments = supportingDocumentsCollection,
				AdditionalReferences = additionalReferencesCollection,
				AdditionalInfos = additionalInfosCollection,
				AdditionalFiscalReference = goodsShipmentProvider.AdditionalFiscalReferences.ConvertToCollection(GetPopulatedH1AdditionalFiscalReference),
			};
		}
		return goodsShipment;
	}

	protected T GetPopulatedCommonConsignment<T, TTransportEquipment, TLocationOfGoods, TTransportDocument>(ICommonImportH1Consigment consignmentProvider)
		where T : IH1ConsignmentCommon, new()
		where TTransportEquipment : ITransportEquipmentCommon, new()
		where TLocationOfGoods : ILocationOfGoodsCommon, new()
		where TTransportDocument : IDocumentSequenceNumberCommon, new()
	{
		var consigment = default(T);
		if (consignmentProvider != null)
		{
			var transportEquipmentsCollection = new Collection<ITransportEquipmentCommon>();
			foreach (var transportEquipment in consignmentProvider.TransportEquipments.ConvertToCollection(GetPopulatedCommonTransportEquipment<TTransportEquipment, MGoodsReferenceType>) ?? Enumerable.Empty<TTransportEquipment>())
			{
				transportEquipmentsCollection.Add(transportEquipment);
			}
			var transportDocumentsCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var transportDocument in consignmentProvider.TransportDocuments.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TTransportDocument>) ?? Enumerable.Empty<TTransportDocument>())
			{
				transportDocumentsCollection.Add(transportDocument);
			}

			consigment = new T()
			{
				IsContainerised = consignmentProvider.IsContainerised ? isContainerised : isNotContainerised,
				GrossMass = consignmentProvider.GrossMass,
				ReferenceNumberUCR = consignmentProvider.ReferenceNumberUCR,
				TransportEquipments = transportEquipmentsCollection,
				LocationOfGoods = GetPopulatedCommonLocationOfGoods<TLocationOfGoods, MCustomsOfficeType, MGpsType, MEconomicOperatorType, MAddressType01, MAddressType02>(consignmentProvider.LocationOfGoods),
				TransportDocuments = transportDocumentsCollection,
			};
		}
		return consigment;
	}

	protected T GetPopulatedCommonGoodsShipmentItem<T>(ICommonH1GoodsShipmentItem goodsShipmentItemProvider)
		where T : IH1GoodsShipmentItemCommon, new()
	{
		var goodsShipmentItem = default(T);
		if (goodsShipmentItemProvider != null)
		{
			goodsShipmentItem = new T()
			{
				DeclarationGoodsItemNumber = goodsShipmentItemProvider.DeclarationGoodsItemNumber,
			};
		}
		return goodsShipmentItem;
	}

	protected T GetPopulatedCompleteAndSimplifiedGoodsShipmentItem<T, TAuthorisation, TAdditionalSupplyActors, TPackage, TSupportingDocument, TTransportDocument, TAdditionalRef, TAdditionalInfo>(ICompleteAndSimplifiedCommonImportH1GoodsShipmentItem goodsShipmentItemProvider)
		where T : IH1GoodsShipmentItem, new()
		where TAuthorisation : IAuthorisationCommon, new()
		where TAdditionalSupplyActors : IAdditionalSupplyChainActorWithSeqNumCommon, new()
		where TPackage : IPackageWithSequenceAndPackNumCommon, new()
		where TSupportingDocument : IH1LineSupportingDocumentCommon, new()
		where TTransportDocument : IDocumentSequenceNumberCommon, new()
		where TAdditionalRef : IH1DocumentSequenceNumberCommon, new()
		where TAdditionalInfo : IH1DocumentSequenceNumberCommon, new()
	{
		var goodsShipmentItem = GetPopulatedCommonGoodsShipmentItem<T>(goodsShipmentItemProvider);
		if (goodsShipmentItem != null)
		{
			var authorisationsCollection = new Collection<IAuthorisationCommon>();
			foreach (var authorisations in goodsShipmentItemProvider.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<TAuthorisation>) ?? Enumerable.Empty<TAuthorisation>())
			{
				authorisationsCollection.Add(authorisations);
			}
			var additionalSupplyChainActorsCollection = new Collection<IAdditionalSupplyChainActorWithSeqNumCommon>();
			foreach (var additionalSupplyChainActor in goodsShipmentItemProvider.AdditionalSupplyChainActors.ConvertToCollection(GetPopulatedAdditionalSupplyActor<TAdditionalSupplyActors>) ?? Enumerable.Empty<TAdditionalSupplyActors>())
			{
				additionalSupplyChainActorsCollection.Add(additionalSupplyChainActor);
			}
			var packagesCollection = new Collection<IPackageWithSequenceAndPackNumCommon>();
			foreach (var package in goodsShipmentItemProvider.Packages.ConvertToCollection(GetPopulatedCommonPackage<TPackage>) ?? Enumerable.Empty<TPackage>())
			{
				packagesCollection.Add(package);
			}
			var previousDocumentsCollection = new Collection<IH1PreviousDocumentCommon>();
			foreach (var previousDocument in goodsShipmentItemProvider.PreviousDocuments.ConvertToCollection(GetPopulatedH1CommonPreviousDocument<MPreviousDocumentType03>) ?? Enumerable.Empty<MPreviousDocumentType03>())
			{
				previousDocumentsCollection.Add(previousDocument);
			}
			var supportingDocumentsCollection = new Collection<IH1LineSupportingDocumentCommon>();
			foreach (var supportingDocument in goodsShipmentItemProvider.SupportingDocuments.ConvertToCollection(GetPopulatedH1CommonLineSupportingDocument<TSupportingDocument>) ?? Enumerable.Empty<TSupportingDocument>())
			{
				supportingDocumentsCollection.Add(supportingDocument);
			}
			var transportDocumentsCollection = new Collection<IDocumentSequenceNumberCommon>();
			foreach (var transportDocument in goodsShipmentItemProvider.TransportDocuments.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TTransportDocument>) ?? Enumerable.Empty<TTransportDocument>())
			{
				transportDocumentsCollection.Add(transportDocument);
			}
			var additionalReferencesCollection = new Collection<IH1DocumentSequenceNumberCommon>();
			foreach (var additionalReference in goodsShipmentItemProvider.AdditionalReferences.ConvertToCollection(GetPopulatedH1CommonDocument<TAdditionalRef>) ?? Enumerable.Empty<TAdditionalRef>())
			{
				additionalReferencesCollection.Add(additionalReference);
			}
			var additionalInfosCollection = new Collection<IH1DocumentSequenceNumberCommon>();
			foreach (var additionalInfo in goodsShipmentItemProvider.AdditionalInfos.ConvertToCollection(GetPopulatedH1CommonDocument<TAdditionalInfo>) ?? Enumerable.Empty<TAdditionalInfo>())
			{
				additionalInfosCollection.Add(additionalInfo);
			}

			goodsShipmentItem.ReferenceNumberUCR = goodsShipmentItemProvider.ReferenceNumberUCR;
			goodsShipmentItem.Authorisations = authorisationsCollection;
			goodsShipmentItem.Procedure = GetPopulatedProcedure(goodsShipmentItemProvider.Procedure);
			goodsShipmentItem.AdditionalSupplyActors = additionalSupplyChainActorsCollection;
			goodsShipmentItem.Exporter = GetPopulatedPartyWithAddress<MExporterType, MAddressType01>(goodsShipmentItemProvider.Exporter);
			goodsShipmentItem.Origin = GetPopulatedOrigin(goodsShipmentItemProvider.Origin);
			goodsShipmentItem.CountryOfDispatch = GetPopulatedCountryOfDispatch(goodsShipmentItemProvider.CountryOfDispatch);
			goodsShipmentItem.Packages = packagesCollection;
			goodsShipmentItem.PreviousDocuments = previousDocumentsCollection;
			goodsShipmentItem.SupportingDocuments = supportingDocumentsCollection;
			goodsShipmentItem.TransportDocuments = transportDocumentsCollection;
			goodsShipmentItem.AdditionalReferences = additionalReferencesCollection;
			goodsShipmentItem.AdditionalInfos = additionalInfosCollection;
			goodsShipmentItem.AdditionalFiscalReference = goodsShipmentItemProvider.AdditionalFiscalReferences.ConvertToCollection(GetPopulatedH1AdditionalFiscalReference);
		}
		return goodsShipmentItem;
	}
}
