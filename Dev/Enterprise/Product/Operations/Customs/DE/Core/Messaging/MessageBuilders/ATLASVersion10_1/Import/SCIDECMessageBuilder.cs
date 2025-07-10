using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using static CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class SCIDECMessageBuilder : MessageBuilder<VSCIDC>
	{
		public SCIDECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ISCIDECHeader)provider.Header;
		}

		readonly IImportMessageHeader provider;
		readonly ISCIDECHeader header;
		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;
		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;
		IInwardProcessing inwardProcessing => header.InwardProcessing;

		protected override VSCIDC GetMessageCore() => new VSCIDC
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = ContactPerson != null ? PopulateContactPerson() : null,
			BorderTransportMeans = PopulateBorderTransportMeans(),
			PreviousAdministrativeReferences = !PreviousAdministrativeReferenceType.IsEmpty() ? PopulatePreviousAdministrativeReferences() : null,
			SummaryDeclaration = summaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = customsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = inwardProcessing != null ? PopulateInwardProcessing() : null,
			Body = PopulateBody()
		};

		VSCIDCMetaData PopulateMetaData() => new VSCIDCMetaData
		{
			Preparation = new VSCIDCMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<VSCIDCMetaDataMessageGroup>(),
			MessageType = VSCIDCMetaDataMessageType.VSCIDC,
			InterchangeSender = new VSCIDCMetaDataInterchangeSender
			{
				Identification = new VSCIDCMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new VSCIDCMetaDataInterchangeRecipient
			{
				Identification = new VSCIDCMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		VSCIDCHeader PopulateHeader() => new VSCIDCHeader
		{
			MessageVersion = "C.1.0",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new VSCIDCHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<VSCIDCHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<VSCIDCHeaderDeclarationType>()
			},
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus.MapCodeToEnumWithDefault<VSCIDCHeaderCustomsGoodsStatus>(),
			DeclarantIsConsigneeFlag = header.DeclarantIsConsigneeFlag.MapBoolToJN(),
			SimplifiedRequestAuthorisationFlag = header.SimplifiedRequestAuthorisationFlag,
			CustomsAuthorisation = !header.ProcedureAuthorisation.IsEmpty() ? PopulateHeaderCustomsAuthorisation() : null,
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<VSCIDCHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag.MapCodeToEnumWithDefault<VSCIDCHeaderRepresentativeRelationshipFlag>(),
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		VSCIDCHeaderCustomsAuthorisation PopulateHeaderCustomsAuthorisation() => new VSCIDCHeaderCustomsAuthorisation { CurrentProcedure = header.ProcedureAuthorisation };

		VSCIDCDeclarant PopulateDeclarant() => new VSCIDCDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new VSCIDCDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			},
			Name = DeclarantHasEoriNumber ? null : Declarant.Address.Name,
			Address = DeclarantHasEoriNumber ? null : new VSCIDCDeclarantAddress
			{
				City = Declarant.Address.City,
				Country = Declarant.Address.Country,
				District = Declarant.Address.District,
				Line = Declarant.Address.Address,
				Postcode = Declarant.Address.Postcode
			}
		};

		VSCIDCRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new VSCIDCRepresentative
		{
			Identification = new VSCIDCRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		VSCIDCPrincipal PopulatePrincipal() => new VSCIDCPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new VSCIDCPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new VSCIDCPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		VSCIDCContactPerson PopulateContactPerson() => new VSCIDCContactPerson
		{
			MailAddress = ContactPerson.MailAddress,
			Name = ContactPerson.PersonName,
			PhoneNumber = ContactPerson.PhoneNumber,
			Position = ContactPerson.Position
		};

		VSCIDCBorderTransportMeans PopulateBorderTransportMeans() => new VSCIDCBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength),
			Nationality = header.BorderTransportMeansNationality
		};

		VSCIDCPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new VSCIDCPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<VSCIDCPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new VSCIDCPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		VSCIDCSummaryDeclaration PopulateSummaryDeclaration() => new VSCIDCSummaryDeclaration
		{
			IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? VSCIDCSummaryDeclarationIdentificationIndicator.REG : VSCIDCSummaryDeclarationIdentificationIndicator.AWB,
			GoodsItem = summaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsitem(x)).ToArray(),
		};

		VSCIDCSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsitem(ISummaryDeclarationGoodsItem goodsItem) => new VSCIDCSummaryDeclarationGoodsItem
		{
			Quantity = goodsItem.Quantity.ToString(),
			IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
			IdentificationByRegistration = CreateCommonIdentificationByRegistration<VSCIDCSummaryDeclarationGoodsItemIdentificationByRegistration>(summaryDeclaration.IdentificationIndicator, goodsItem.IdentificationByRegistrationReferencedRegistrationNumber, im => im.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString()),
		};

		VSCIDCSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
		{
			var summaryDeclarationIdentificationIndicatorIsAWB = summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			return new VSCIDCSummaryDeclarationGoodsItemIdentificationByKey
			{
				Kind = summaryDeclarationIdentificationIndicatorIsAWB ? VSCIDCSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : VSCIDCSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
				Number = goodsItem.IdentificationByKeyNumber,
				Custodian = new VSCIDCSummaryDeclarationGoodsItemIdentificationByKeyCustodian
				{
					Identification = new VSCIDCSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
				}
			};
		}

		VSCIDCCustomsWarehouse PopulateCustomsWarehouse() => new VSCIDCCustomsWarehouse
		{
			SequenceNumber = "1",
			GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = new VSCIDCCustomsWarehouseCustomsAuthorisation { WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier },
			LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
			GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
		};

		VSCIDCCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter)
		{
			return CreateCommonIdentificationByRegistration<VSCIDCCustomsWarehouseGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
				x.CommodityCode = goodsItem.CommodityCode;
				x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
				x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
				x.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
				x.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
			});
		}

		VSCIDCInwardProcessing PopulateInwardProcessing() => new VSCIDCInwardProcessing
		{
			SequenceNumber = "1",
			GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
			SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
			MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
			GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
		};

		VSCIDCInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new VSCIDCInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier };

		VSCIDCInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new VSCIDCInwardProcessingMonitoringCustomsOffice
		{
			Identification = new VSCIDCInwardProcessingMonitoringCustomsOfficeIdentification
			{
				ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
			}
		};

		VSCIDCInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter)
		{
			return CreateCommonIdentificationByRegistration<VSCIDCInwardProcessingGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
				x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
			});
		}

		VSCIDCBody PopulateBody() => new VSCIDCBody
		{
			Consignor = Consignor != null ? PopulateConsignor() : null,
			Consignee = Consignee != null ? PopulateConsignee() : null,
			Containers = PopulateContainers(),
			DeliveryTerms = PopulateDeliveryTerms(),
			PaymentTransaction = header.PaymentTransaction == null ? null : PopulatePaymentTransaction(),
			CustomsAuthorisationInwardProcessing = !header.InwardProcessingCriteriaType.IsEmpty() ? PopulateCustomsAuthorisationInwardProcessing() : null,
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		VSCIDCBodyConsignor PopulateConsignor() => new VSCIDCBodyConsignor
		{
			Identification = !ConsignorHasEoriNumber ? null : new VSCIDCBodyConsignorIdentification
			{
				ReferenceNumber = Consignor.Identification.EoriNumber
			},
			Name = ConsignorHasEoriNumber ? null : Consignor.Address.Name,
			Address = ConsignorHasEoriNumber ? null : new VSCIDCBodyConsignorAddress
			{
				City = Consignor.Address.City,
				Country = Consignor.Address.Country,
				District = Consignor.Address.District,
				Line = Consignor.Address.Address,
				Postcode = Consignor.Address.Postcode
			}
		};

		VSCIDCBodyConsignee PopulateConsignee() => new VSCIDCBodyConsignee
		{
			Identification = !ConsigneeHasEoriNumber ? null : new VSCIDCBodyConsigneeIdentification
			{
				ReferenceNumber = Consignee.Identification.EoriNumber,
				SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
			},
			Name = ConsigneeHasEoriNumber ? null : Consignee.Address.Name,
			Address = ConsigneeHasEoriNumber ? null : new VSCIDCBodyConsigneeAddress
			{
				City = Consignee.Address.City,
				Country = Consignee.Address.Country,
				District = Consignee.Address.District,
				Line = Consignee.Address.Address,
				Postcode = Consignee.Address.Postcode
			}
		};

		VSCIDCBodyContainers PopulateContainers() => new VSCIDCBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerIdentificationNumbers.Select(c => new VSCIDCBodyContainersContainer { IdentificationNumber = c }).ToArray(),
		};

		VSCIDCBodyDeliveryTerms PopulateDeliveryTerms() => new VSCIDCBodyDeliveryTerms
		{
			Code = header.DeliveryTermsCode,
			Description = header.DeliveryTermsDescription,
			Place = header.DeliveryTermsPlace,
			Key = header.DeliveryTermsKey
		};

		VSCIDCBodyPaymentTransaction PopulatePaymentTransaction() => new VSCIDCBodyPaymentTransaction
		{
			Amount = header.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = header.PaymentTransaction.CurrencyCode
		};

		VSCIDCBodyCustomsAuthorisationInwardProcessing PopulateCustomsAuthorisationInwardProcessing()
		{
			var mainAccounting = header.MainAccounting;
			var firstInwardProcessingPlace = header.FirstInwardProcessingPlace;
			return new VSCIDCBodyCustomsAuthorisationInwardProcessing
			{
				CompletionLimitDate = header.InwardProcessingCompletionLimitDate.ToString(),
				CriteriaType = header.InwardProcessingCriteriaType,
				AdditionalInformation = header.InwardProcessingAdditionalInformation,
				IntendedActivityDetail = new VSCIDCBodyCustomsAuthorisationInwardProcessingIntendedActivityDetail
				{
					Description = header.IntendedActivityDetailDescription,
				},
				MainAccounting = mainAccounting == null ? null : new VSCIDCBodyCustomsAuthorisationInwardProcessingMainAccounting
				{
					Line = mainAccounting.Address.LeftOrNull(StreetAndNumberMaxLength),
					Country = mainAccounting.Country,
					Postcode = mainAccounting.Postcode.LeftOrNull(AddressPostcodeMaxLength),
					City = mainAccounting.City.LeftOrNull(AddressCityMaxLength),
					District = mainAccounting.District.LeftOrNull(AddressDistrictMaxLength),
				},
				FirstInwardProcessingPlace = firstInwardProcessingPlace == null ? null : new VSCIDCBodyCustomsAuthorisationInwardProcessingFirstInwardProcessingPlace
				{
					Line = firstInwardProcessingPlace.Address.LeftOrNull(StreetAndNumberMaxLength),
					Country = firstInwardProcessingPlace.Country.MapCodeToEnumWithDefault<VSCIDCBodyCustomsAuthorisationInwardProcessingFirstInwardProcessingPlaceCountry>(),
					Postcode = firstInwardProcessingPlace.Postcode.LeftOrNull(AddressPostcodeMaxLength),
					City = firstInwardProcessingPlace.City.LeftOrNull(AddressCityMaxLength),
					District = firstInwardProcessingPlace.District.LeftOrNull(AddressDistrictMaxLength),
				},
				AdditionalInwardProcessingPlace = header.AdditionalInwardProcessingPlace.Select(PopulateAdditionalInwardProcessingPlace).ToArray(),
				CompletionCustomsOffice = header.CompletionCustomsOfficeReferenceNumbers.Select(PopulateCompletionCustomsOffice).ToArray(),
			};
		}

		VSCIDCBodyCustomsAuthorisationInwardProcessingAdditionalInwardProcessingPlace PopulateAdditionalInwardProcessingPlace(IImportPartyIdAddress address)
		{
			return new VSCIDCBodyCustomsAuthorisationInwardProcessingAdditionalInwardProcessingPlace
			{
				Line = address.Address.LeftOrNull(StreetAndNumberMaxLength),
				Country = address.Country.MapCodeToEnumWithDefault<VSCIDCBodyCustomsAuthorisationInwardProcessingAdditionalInwardProcessingPlaceCountry>(),
				Postcode = address.Postcode.LeftOrNull(AddressPostcodeMaxLength),
				City = address.City.LeftOrNull(AddressCityMaxLength),
				District = address.District.LeftOrNull(AddressDistrictMaxLength),
			};
		}

		VSCIDCBodyCustomsAuthorisationInwardProcessingCompletionCustomsOffice PopulateCompletionCustomsOffice(string reference)
		{
			return new VSCIDCBodyCustomsAuthorisationInwardProcessingCompletionCustomsOffice
			{
				Identification = new VSCIDCBodyCustomsAuthorisationInwardProcessingCompletionCustomsOfficeIdentification
				{
					ReferenceNumber = reference
				}
			};
		}

		VSCIDCBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new VSCIDCBodyForeignTradeStatistics
		{
			GoodsStatus = header.ForeignTradeStatisticsGoodsStatus,
			TransactionType = header.ForeignTradeStatisticsTransactionType,
			DestinationCountry = header.ForeignTradeStatisticsDestinationCountry,
			DestinationFederalState = header.ForeignTradeStatisticsDestinationFederalState,
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
			EntryCustomsOffice = new VSCIDCBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = header.EntryCustomsOfficeReferenceNumber
			}
		};

		VSCIDCBodyDocument PopulateDocument(IImportDocument d) => new VSCIDCBodyDocument
		{
			Division = VSCIDCBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault()
		};

		VSCIDCBodyGoodsItem PopulateLine(ISCIDECLine line)
		{
			return new VSCIDCBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new VSCIDCBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				GoodsDescription = line.GoodsDescription,
				ArticleNumber = line.ArticleNumber,
				InvoiceAmount = line.InvoiceAmount.Round(2).Normalize(),
				InvoiceAmountSpecified = true,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = line.OriginCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CommodityCode = new VSCIDCBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				ForeignTradeStatistics = new VSCIDCBodyGoodsItemForeignTradeStatistics
				{
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
				},
				InwardMovement = new VSCIDCBodyGoodsItemInwardMovement
				{
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCBodyGoodsItemInwardMovementAmount>(line.InwardMovementAmount),
				},
				CustomsAuthorisationInwardProcessing = new VSCIDCBodyGoodsItemCustomsAuthorisationInwardProcessing
				{
					EconomicConditions = line.EconomicConditions,
					Product = line.Products.Select(x => PopulateLineProduct(x)).ToArray(),
					IdentificationMeans = new VSCIDCBodyGoodsItemCustomsAuthorisationInwardProcessingIdentificationMeans
					{
						Type = line.IdentificationMeans.Type,
						Description = line.IdentificationMeans.Description
					}
				},
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = new VSCIDCBodyGoodsItemPreferentialTreatment
				{
					RequestedPreferentialTreatment = line.RequestedPreferentialTreatment
				},
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
		}

		VSCIDCBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure)
		{
			return new VSCIDCBodyGoodsItemAdditionalProcedure
			{
				Code = additionalProcedure
			};
		}

		VSCIDCBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode)
		{
			return new VSCIDCBodyGoodsItemSupplementaryCodes
			{
				Code = supplementaryCode
			};
		}

		VSCIDCBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			return new VSCIDCBodyGoodsItemPackage
			{
				Kind = package.Kind,
				Quantity = package?.Quantity.ToString(),
				MarksNumbers = package.MarksNumbers
			};
		}

		VSCIDCBodyGoodsItemCustomsAuthorisationInwardProcessingProduct PopulateLineProduct(ISCIDECLineProduct product)
		{
			return new VSCIDCBodyGoodsItemCustomsAuthorisationInwardProcessingProduct
			{
				GoodsDescription = product.GoodsDescription,
				YieldType = product.YieldType,
				YieldRate = product.YieldRate,
				CombinedNomenclature = new VSCIDCBodyGoodsItemCustomsAuthorisationInwardProcessingProductCombinedNomenclature
				{
					Code = product.CombinedNomenclatureCode
				},
			};
		}

		VSCIDCBodyGoodsItemAssessment PopulateAssessment(ISCIDECLine line)
		{
			return new VSCIDCBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				Amount = line.AssessmentAmount.Select(a => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCBodyGoodsItemAssessmentAmount>(a)).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};
		}

		VSCIDCBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
		{
			return new VSCIDCBodyGoodsItemAssessmentSpecificRate
			{
				Type = rate.Type,
				Value = rate.Value.Round(2).Normalize()
			};
		}

		VSCIDCBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
		{
			return new VSCIDCBodyGoodsItemAssessmentContentInformation
			{
				Type = contentInformation.ContentType,
				DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
			};
		}

		VSCIDCBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new VSCIDCBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		VSCIDCBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new VSCIDCBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIDCBodyGoodsItemDocumentWriteOff>(document.WriteOff)
			};
		}

		IImportPartyContactPerson ContactPerson => header.ContactPerson;
		IImportParty Declarant => header.Declarant;
		IImportParty Representative => header.Representative;
		IImportParty Principal => header.Principal;
		IImportParty Consignor => header.Consignor;
		IImportParty Consignee => header.Consignee;

		bool DeclarantHasEoriNumber => CachedValueHelper.GetValue(ref declarantHasEoriNumberCached, () => !Declarant?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> declarantHasEoriNumberCached;

		bool PrincipalHasEoriNumber => CachedValueHelper.GetValue(ref principalHasEoriNumberCached, () => !Principal?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> principalHasEoriNumberCached;

		bool ConsignorHasEoriNumber => CachedValueHelper.GetValue(ref consignorHasEoriNumberCached, () => !Consignor?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> consignorHasEoriNumberCached;

		bool ConsigneeHasEoriNumber => CachedValueHelper.GetValue(ref consigneeHasEoriNumberCached, () => !Consignee?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> consigneeHasEoriNumberCached;

		bool RepresentativeHasEoriNumber => CachedValueHelper.GetValue(ref representativeHasEoriNumberCached, () => !Representative?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> representativeHasEoriNumberCached;

		string PreviousAdministrativeReferenceType => CachedValueHelper.GetValue(ref previousAdministrativeReferenceTypeCached, () => header.PreviousAdministrativeReferenceType);
		CachedValue<string> previousAdministrativeReferenceTypeCached;

		bool SummaryDeclarationIdentificationIndicatorIsREG => CachedValueHelper.GetValue(ref summaryDeclarationIdentificationIndicatorIsREGCached, () => summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG);
		CachedValue<bool> summaryDeclarationIdentificationIndicatorIsREGCached;
	}
}
