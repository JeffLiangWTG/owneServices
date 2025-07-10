using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class SCWDECMessageBuilder : MessageBuilder<LSCWDL>
	{
		public SCWDECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ISCWDECHeader)provider.Header;
		}
		readonly IImportMessageHeader provider;
		readonly ISCWDECHeader header;

		protected override LSCWDL GetMessageCore() => new LSCWDL
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = ContactPerson != null ? PopulateContactPerson() : null,
			BorderTransportMeans = PopulateBorderTransportMeans(),
			PreviousAdministrativeReferences = PopulatePreviousAdministrativeReferences(),
			SummaryDeclaration = SummaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = CustomsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = InwardProcessing != null ? PopulateInwardProcessing() : null,
			Body = PopulateBody()
		};

		LSCWDLMetaData PopulateMetaData() => new LSCWDLMetaData
		{
			Preparation = new LSCWDLMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<LSCWDLMetaDataMessageGroup>(),
			MessageType = LSCWDLMetaDataMessageType.LSCWDL,
			InterchangeSender = new LSCWDLMetaDataInterchangeSender
			{
				Identification = new LSCWDLMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new LSCWDLMetaDataInterchangeRecipient
			{
				Identification = new LSCWDLMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		LSCWDLHeader PopulateHeader() => new LSCWDLHeader
		{
			MessageVersion = "L.1.0",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new LSCWDLHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<LSCWDLHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<LSCWDLHeaderDeclarationType>()
			},
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			ForeignTradeImportEarlyClearanceFlag = header.ForeignTradeImportEarlyClearanceFlag.MapBoolToJN(),
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus.MapCodeToEnumWithDefault<LSCWDLHeaderCustomsGoodsStatus>(),
			DeclarantIsConsigneeFlag = header.DeclarantIsConsigneeFlag.MapBoolToJN(),
			CustomsAuthorisation = new LSCWDLHeaderCustomsAuthorisation
			{
				CurrentProcedure = header.ProcedureAuthorisation
			},
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<LSCWDLHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		LSCWDLDeclarant PopulateDeclarant() => new LSCWDLDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new LSCWDLDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			},
			Name = DeclarantHasEoriNumber ? null : Declarant.Address.Name,
			Address = DeclarantHasEoriNumber ? null : new LSCWDLDeclarantAddress
			{
				City = Declarant.Address.City,
				Country = Declarant.Address.Country,
				District = Declarant.Address.District,
				Line = Declarant.Address.Address,
				Postcode = Declarant.Address.Postcode
			}
		};

		LSCWDLRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new LSCWDLRepresentative
		{
			Identification = new LSCWDLRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		LSCWDLPrincipal PopulatePrincipal() => new LSCWDLPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new LSCWDLPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new LSCWDLPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		LSCWDLContactPerson PopulateContactPerson() => new LSCWDLContactPerson
		{
			MailAddress = ContactPerson.MailAddress,
			Name = ContactPerson.PersonName,
			PhoneNumber = ContactPerson.PhoneNumber,
			Position = ContactPerson.Position,
		};

		LSCWDLBorderTransportMeans PopulateBorderTransportMeans() => new LSCWDLBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength)
		};

		LSCWDLPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new LSCWDLPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<LSCWDLPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new LSCWDLPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		LSCWDLSummaryDeclaration PopulateSummaryDeclaration()
		{
			return new LSCWDLSummaryDeclaration
			{
				IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? LSCWDLSummaryDeclarationIdentificationIndicator.REG : LSCWDLSummaryDeclarationIdentificationIndicator.AWB,
				GoodsItem = SummaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsitem(x)).ToArray()
			};

			LSCWDLSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsitem(ISummaryDeclarationGoodsItem goodsItem) => new LSCWDLSummaryDeclarationGoodsItem
			{
				Quantity = goodsItem.Quantity.ToString(),
				IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
				IdentificationByRegistration = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWDLSummaryDeclarationGoodsItemIdentificationByRegistration>(
				SummaryDeclaration.IdentificationIndicator,
				goodsItem.IdentificationByRegistrationReferencedRegistrationNumber,
				(x) => x.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString())
			};

			LSCWDLSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
			{
				var summaryDeclarationIdentificationIndicatorIsAWB = SummaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				return new LSCWDLSummaryDeclarationGoodsItemIdentificationByKey
				{
					Kind = summaryDeclarationIdentificationIndicatorIsAWB ? LSCWDLSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : LSCWDLSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
					Number = goodsItem.IdentificationByKeyNumber,
					Custodian = new LSCWDLSummaryDeclarationGoodsItemIdentificationByKeyCustodian
					{
						Identification = new LSCWDLSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
					}
				};
			}
		}

		LSCWDLCustomsWarehouse PopulateCustomsWarehouse()
		{
			return new LSCWDLCustomsWarehouse
			{
				SequenceNumber = "1",
				GoodsItemQuantity = CustomsWarehouse.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = new LSCWDLCustomsWarehouseCustomsAuthorisation { WarehouseOwner = CustomsWarehouse.WarehouseOwnerIdentifier },
				LRN = CustomsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
				GoodsItem = CustomsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
			};

			LSCWDLCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter)
			{
				return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWDLCustomsWarehouseGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
				{
					x.SequenceNumber = counter.ToString();
					x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
					x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
					x.CommodityCode = goodsItem.CommodityCode;
					x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
					x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
					x.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
					x.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
				});
			}
		}

		LSCWDLInwardProcessing PopulateInwardProcessing()
		{
			return new LSCWDLInwardProcessing
			{
				SequenceNumber = "1",
				GoodsItemQuantity = InwardProcessing.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = !InwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
				SimplifiedGrantAuthorisationFlag = InwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
				MonitoringCustomsOffice = InwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
				GoodsItem = InwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
			};

			LSCWDLInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new LSCWDLInwardProcessingCustomsAuthorisation { ProcessingOwner = InwardProcessing.ProcessingOwnerIdentifier };

			LSCWDLInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new LSCWDLInwardProcessingMonitoringCustomsOffice
			{
				Identification = new LSCWDLInwardProcessingMonitoringCustomsOfficeIdentification
				{
					ReferenceNumber = InwardProcessing.MonitoringCustomsOfficeReferenceNumber
				}
			};

			LSCWDLInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter)
			{
				return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWDLInwardProcessingGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
				{
					x.SequenceNumber = counter.ToString();
					x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
					x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
					x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
				});
			}
		}

		LSCWDLBody PopulateBody() => new LSCWDLBody
		{
			CustomsValueFlag = header.CustomsValue != null ? "1" : "0",
			Consignee = header.Consignee != null ? PopulateConsignee() : null,
			Containers = PopulateContainers(),
			DeliveryTerms = PopulateDeliveryTerms(),
			PaymentTransaction = header.PaymentTransaction == null ? null : PopulatePaymentTransaction(),
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			CustomsValue = header.CustomsValue != null ? PopulateCustomsValue(header.CustomsValue) : null,
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		LSCWDLBodyConsignee PopulateConsignee()
		{
			var result = new LSCWDLBodyConsignee();
			var consigneeAddress = Consignee.Address;
			if (ConsigneeHasEoriNumber)
			{
				result.Identification = new LSCWDLBodyConsigneeIdentification
				{
					ReferenceNumber = Consignee.Identification.EoriNumber,
					SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
				};
			}
			else if (consigneeAddress != null)
			{
				result.Name = consigneeAddress.Name.ValueOrNullIfEmpty();
				result.Address = new LSCWDLBodyConsigneeAddress
				{
					City = consigneeAddress.City.ValueOrNullIfEmpty(),
					Country = consigneeAddress.Country.ValueOrNullIfEmpty(),
					District = consigneeAddress.District.ValueOrNullIfEmpty(),
					Line = consigneeAddress.Address.ValueOrNullIfEmpty(),
					Postcode = consigneeAddress.Postcode.ValueOrNullIfEmpty()
				};
			}
			return result;
		}

		LSCWDLBodyContainers PopulateContainers() => new LSCWDLBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerIdentificationNumbers.Select(c => new LSCWDLBodyContainersContainer { IdentificationNumber = c }).ToArray(),
		};

		LSCWDLBodyDeliveryTerms PopulateDeliveryTerms() => new LSCWDLBodyDeliveryTerms
		{
			Code = header.DeliveryTermsCode,
			Description = header.DeliveryTermsDescription,
			Place = header.DeliveryTermsPlace,
			Key = header.DeliveryTermsKey
		};

		LSCWDLBodyPaymentTransaction PopulatePaymentTransaction() => new LSCWDLBodyPaymentTransaction
		{
			Amount = header.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = header.PaymentTransaction.CurrencyCode
		};

		LSCWDLBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new LSCWDLBodyForeignTradeStatistics
		{
			GoodsStatus = header.ForeignTradeStatisticsGoodsStatus,
			DestinationCountry = header.ForeignTradeStatisticsDestinationCountry,
			DestinationFederalState = header.ForeignTradeStatisticsDestinationFederalState,
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
			EntryCustomsOffice = new LSCWDLBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = header.EntryCustomsOfficeReferenceNumber
			}
		};

		LSCWDLBodyCustomsValue PopulateCustomsValue(ICustomsValue customsValue)
		{
			var vendor = customsValue.Vendor;
			var vendee = customsValue.Vendee;
			var vendorHasEoriNumber = !vendor?.Identification.EoriNumberIsEmpty() ?? false;
			var vendeeHasEoriNumber = !vendee?.Identification.EoriNumberIsEmpty() ?? false;

			return new LSCWDLBodyCustomsValue
			{
				FormerDecisions = customsValue.FormerDecisions,
				Vendor = vendor != null ? PopulateVendor() : null,
				Vendee = vendee != null ? PopulateVendee() : null,
				Affiliation = PopulateAffiliation(),
				RestrictionOrCondition = PopulateRestrictionOrCondition(),
				LicenseFee = PopulateLicenseFee(),
				Resale = PopulateResale()
			};

			LSCWDLBodyCustomsValueVendor PopulateVendor() => new LSCWDLBodyCustomsValueVendor
			{
				Identification = !vendorHasEoriNumber ? null : new LSCWDLBodyCustomsValueVendorIdentification
				{
					ReferenceNumber = vendor.Identification.EoriNumber
				},
				Name = vendorHasEoriNumber ? null : vendor.Address.Name,
				Address = vendorHasEoriNumber ? null : new LSCWDLBodyCustomsValueVendorAddress
				{
					City = vendor.Address.City,
					Country = vendor.Address.Country,
					District = vendor.Address.District,
					Line = vendor.Address.Address,
					Postcode = vendor.Address.Postcode
				}
			};

			LSCWDLBodyCustomsValueVendee PopulateVendee() => new LSCWDLBodyCustomsValueVendee
			{
				Identification = !vendeeHasEoriNumber ? null : new LSCWDLBodyCustomsValueVendeeIdentification
				{
					ReferenceNumber = vendee.Identification.EoriNumber
				},
				Name = vendeeHasEoriNumber ? null : vendee.Address.Name,
				Address = vendeeHasEoriNumber ? null : new LSCWDLBodyCustomsValueVendeeAddress
				{
					City = vendee.Address.City,
					Country = vendee.Address.Country,
					District = vendee.Address.District,
					Line = vendee.Address.Address,
					Postcode = vendee.Address.Postcode
				}
			};

			LSCWDLBodyCustomsValueAffiliation PopulateAffiliation() => new LSCWDLBodyCustomsValueAffiliation
			{
				Type = customsValue.AffiliationType,
				Description = customsValue.AffiliationDescription
			};

			LSCWDLBodyCustomsValueRestrictionOrCondition PopulateRestrictionOrCondition() => new LSCWDLBodyCustomsValueRestrictionOrCondition
			{
				RestrictionFlag = customsValue.RestrictionFlag.MapBoolToJN(),
				ConditionFlag = customsValue.ConditionFlag.MapBoolToJN(),
				Description = customsValue.RestrictionOrConditionDescription
			};

			LSCWDLBodyCustomsValueLicenseFee PopulateLicenseFee() => new LSCWDLBodyCustomsValueLicenseFee
			{
				LicenseFeeFlag = customsValue.LicenseFeeFlag.MapBoolToJN(),
				Description = customsValue.LicenseFeeDescription
			};

			LSCWDLBodyCustomsValueResale PopulateResale() => new LSCWDLBodyCustomsValueResale
			{
				ResaleFlag = customsValue.ResaleFlag.MapBoolToJN(),
				Description = customsValue.ResaleDescription
			};
		}

		LSCWDLBodyDocument PopulateDocument(IImportDocument d) => new LSCWDLBodyDocument
		{
			Division = LSCWDLBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault(),
		};

		LSCWDLBodyGoodsItem PopulateLine(ISCWDECLine line)
		{
			return new LSCWDLBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new LSCWDLBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				GoodsDescription = line.GoodsDescription,
				ArticleNumber = line.ArticleNumber,
				InvoiceAmount = line.InvoiceAmount.Round(2).Normalize(),
				InvoiceAmountSpecified = !line.InvoiceAmount.IsZero(),
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = line.OriginCountry.ValueOrNullIfEmpty(),
				SupplementaryInformation = line.SupplementaryInformation,
				CommodityCode = new LSCWDLBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				ForeignTradeStatistics = new LSCWDLBodyGoodsItemForeignTradeStatistics
				{
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
				},
				InwardMovement = new LSCWDLBodyGoodsItemInwardMovement
				{
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLBodyGoodsItemInwardMovementAmount>(line.InwardMovementAmount)
				},
				CustomsValue = line.CustomsValue != null ? PopulateLineCustomsValue(line) : null,
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = new LSCWDLBodyGoodsItemPreferentialTreatment
				{
					RequestedPreferentialTreatment = line.RequestedPreferentialTreatment
				},
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
		}

		LSCWDLBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure)
		{
			return new LSCWDLBodyGoodsItemAdditionalProcedure
			{
				Code = additionalProcedure
			};
		}

		LSCWDLBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode)
		{
			return new LSCWDLBodyGoodsItemSupplementaryCodes
			{
				Code = supplementaryCode
			};
		}

		LSCWDLBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			return new LSCWDLBodyGoodsItemPackage
			{
				Kind = package.Kind,
				Quantity = package?.Quantity.ToString(),
				MarksNumbers = package.MarksNumbers
			};
		}

		LSCWDLBodyGoodsItemCustomsValueNetPrice PopulateNetPrice(IImportCosts netPrice)
		{
			var isEUR = netPrice.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWDLBodyGoodsItemCustomsValueNetPrice
			{
				Value = netPrice.Value.Round(2).Normalize(),
				CurrencyCode = netPrice.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? netPrice.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = netPrice.CurrencyRate,
				CurrencyRateSpecified = !netPrice.CurrencyRate.IsZero() && !isEUR,
			};
		}

		LSCWDLBodyGoodsItemCustomsValueIndirectPayment PopulateIndirectPayment(IImportCosts indirectPayment)
		{
			var isEUR = indirectPayment.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWDLBodyGoodsItemCustomsValueIndirectPayment
			{
				Value = indirectPayment.Value.Round(2).Normalize(),
				CurrencyCode = indirectPayment.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? indirectPayment.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = indirectPayment.CurrencyRate,
				CurrencyRateSpecified = !indirectPayment.CurrencyRate.IsZero() && !isEUR,
			};
		}

		LSCWDLBodyGoodsItemCustomsValueAirFreightCosts PopulateAirFreightCosts(IAirFreightCosts airFreightCosts)
		{
			var isEUR = airFreightCosts.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWDLBodyGoodsItemCustomsValueAirFreightCosts
			{
				Value = airFreightCosts.Value.Round(2).Normalize(),
				CurrencyCode = airFreightCosts.CurrencyCode,
				CurrencyRateIATA = !isEUR ? airFreightCosts.CurrencyRateIATA.MapBoolToJN() : null,
				CurrencyRateAgreedFlag = !isEUR ? airFreightCosts.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = airFreightCosts.CurrencyRate,
				CurrencyRateSpecified = !airFreightCosts.CurrencyRate.IsZero() && !isEUR,
				CurrencyRateDate = airFreightCosts.CurrencyRateDate.GetValueOrDefault(),
				CurrencyRateDateSpecified = airFreightCosts.CurrencyRateDate.HasValue && !isEUR,
			};
		}

		LSCWDLBodyGoodsItemCustomsValueAdditionDeduction PopulateAdditionDeduction(IAdditionDeduction additionDeduction)
		{
			var isEUR = additionDeduction.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWDLBodyGoodsItemCustomsValueAdditionDeduction
			{
				Type = additionDeduction.Type,
				Value = additionDeduction.Value,
				CurrencyCode = additionDeduction.CurrencyCode,
				CurrencyRateIATA = !isEUR ? additionDeduction.CurrencyRateIATA.MapBoolToJN() : null,
				CurrencyRateAgreedFlag = !isEUR ? additionDeduction.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = additionDeduction.CurrencyRate,
				CurrencyRateSpecified = !additionDeduction.CurrencyRate.IsZero() && !isEUR,
				CurrencyRateDate = additionDeduction.CurrencyRateDate.GetValueOrDefault(),
				CurrencyRateDateSpecified = additionDeduction.CurrencyRateDate.HasValue && !isEUR,
				Percentage = additionDeduction.Percentage,
				PercentageSpecified = !additionDeduction.Percentage.IsZero(),
			};
		}

		LSCWDLBodyGoodsItemCustomsValue PopulateLineCustomsValue(ISCWDECLine line)
		{
			return new LSCWDLBodyGoodsItemCustomsValue
			{
				DepartureAirport = line.CustomsValue.CustomsValueDepartureAirport,
				DestinationPlace = line.CustomsValue.CustomsValueDestinationPlace,
				AdditionDeductionDescription = line.CustomsValue.CustomsValueAdditionDeductionDescription,
				NetPrice = PopulateNetPrice(line.CustomsValue.CustomsValueNetPrice),
				IndirectPayment = line.CustomsValue.CustomsValueIndirectPayment != null ? PopulateIndirectPayment(line.CustomsValue.CustomsValueIndirectPayment) : null,
				AirFreightCosts = line.CustomsValue.CustomsValueAirFreightCosts != null ? PopulateAirFreightCosts(line.CustomsValue.CustomsValueAirFreightCosts) : null,
				AdditionDeduction = line.CustomsValue.CustomsValueAdditionDeduction.Select(ad => PopulateAdditionDeduction(ad)).ToArray(),
			};
		}

		LSCWDLBodyGoodsItemAssessment PopulateAssessment(ISCWDECLine line)
		{
			return new LSCWDLBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				Amount = line.AssessmentAmount.Select(a => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLBodyGoodsItemAssessmentAmount>(a)).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};

			LSCWDLBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
			{
				return new LSCWDLBodyGoodsItemAssessmentSpecificRate
				{
					Type = rate.Type,
					Value = rate.Value.Round(2).Normalize()
				};
			}

			LSCWDLBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
			{
				return new LSCWDLBodyGoodsItemAssessmentContentInformation
				{
					Type = contentInformation.ContentType,
					DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
				};
			}
		}

		LSCWDLBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new LSCWDLBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		LSCWDLBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new LSCWDLBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWDLBodyGoodsItemDocumentWriteOff>(document.WriteOff)
			};
		}

		ISummaryDeclaration SummaryDeclaration => header.SummaryDeclaration;

		ICustomsWarehouse CustomsWarehouse => header.CustomsWarehouse;

		IInwardProcessing InwardProcessing => header.InwardProcessing;

		IImportPartyContactPerson ContactPerson => header.ContactPerson;

		IImportParty Declarant => header.Declarant;

		IImportParty Representative => header.Representative;

		IImportParty Principal => header.Principal;

		IImportParty Consignee => header.Consignee;

		bool DeclarantHasEoriNumber => CachedValueHelper.GetValue(ref declarantHasEoriNumberCached, () => !Declarant?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> declarantHasEoriNumberCached;

		bool PrincipalHasEoriNumber => CachedValueHelper.GetValue(ref principalHasEoriNumberCached, () => !Principal?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> principalHasEoriNumberCached;

		bool ConsigneeHasEoriNumber => CachedValueHelper.GetValue(ref consigneeHasEoriNumberCached, () => !Consignee?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> consigneeHasEoriNumberCached;

		bool RepresentativeHasEoriNumber => CachedValueHelper.GetValue(ref representativeHasEoriNumberCached, () => !Representative?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> representativeHasEoriNumberCached;

		string PreviousAdministrativeReferenceType => CachedValueHelper.GetValue(ref previousAdministrativeReferenceTypeCached, () => header.PreviousAdministrativeReferenceType);
		CachedValue<string> previousAdministrativeReferenceTypeCached;

		bool SummaryDeclarationIdentificationIndicatorIsREG => CachedValueHelper.GetValue(ref summaryDeclarationIdentificationIndicatorIsREGCached, () => SummaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG);
		CachedValue<bool> summaryDeclarationIdentificationIndicatorIsREGCached;
	}
}
