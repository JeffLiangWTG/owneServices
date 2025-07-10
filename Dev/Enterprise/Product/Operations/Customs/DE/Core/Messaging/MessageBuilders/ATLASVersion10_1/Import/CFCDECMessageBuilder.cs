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
	public sealed class CFCDECMessageBuilder : MessageBuilder<FCFCDF>
	{
		public CFCDECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ICFCDECHeader)provider.Header;
		}

		readonly IImportMessageHeader provider;
		readonly ICFCDECHeader header;
		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;
		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;
		IInwardProcessing inwardProcessing => header.InwardProcessing;

		protected override FCFCDF GetMessageCore() => new FCFCDF
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = PopulateContactPerson(),
			DutyDefermentApproval = header.DutyDefermentApprovals.Select(dda => PopulateDutyDefermentApproval(dda)).ToArray(),
			BorderTransportMeans = PopulateBorderTransportMeans(),
			ArrivalTransportMeans = PopulateArrivalTransportMeans(),
			PreviousAdministrativeReferences = PopulatePreviousAdministrativeReferences(),
			SummaryDeclaration = summaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = customsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = inwardProcessing != null ? PopulateInwardProcessing() : null,
			Body = PopulateBody()
		};

		FCFCDFMetaData PopulateMetaData() => new FCFCDFMetaData
		{
			Preparation = new FCFCDFMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<FCFCDFMetaDataMessageGroup>(),
			MessageType = FCFCDFMetaDataMessageType.FCFCDF,
			InterchangeSender = new FCFCDFMetaDataInterchangeSender
			{
				Identification = new FCFCDFMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new FCFCDFMetaDataInterchangeRecipient
			{
				Identification = new FCFCDFMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		FCFCDFHeader PopulateHeader() => new FCFCDFHeader
		{
			MessageVersion = "F.1.2",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new FCFCDFHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<FCFCDFHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<FCFCDFHeaderDeclarationType>()
			},
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus,
			DeclarantIsConsigneeFlag = header.DeclarantIsConsigneeFlag.MapBoolToJN(),
			CustomsAuthorisation = new FCFCDFHeaderCustomsAuthorisation
			{
				EndUse = header.ProcedureAuthorisation
			},
			InputTaxDeductionFlag = header.InputTaxDeductionFlag.MapBoolToJN(),
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			PaymentMethod = header.PaymentMethod.MapCodeToEnumWithDefault<FCFCDFHeaderPaymentMethod>(),
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<FCFCDFHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			TaxOffice = header.TaxOffice,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		FCFCDFDeclarant PopulateDeclarant() => new FCFCDFDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new FCFCDFDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			},
			Name = DeclarantHasEoriNumber ? null : Declarant.Address.Name,
			Address = DeclarantHasEoriNumber ? null : new FCFCDFDeclarantAddress
			{
				City = Declarant.Address.City,
				Country = Declarant.Address.Country,
				District = Declarant.Address.District,
				Line = Declarant.Address.Address,
				Postcode = Declarant.Address.Postcode
			}
		};

		FCFCDFRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new FCFCDFRepresentative
		{
			Identification = new FCFCDFRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		FCFCDFPrincipal PopulatePrincipal() => new FCFCDFPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new FCFCDFPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new FCFCDFPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		FCFCDFContactPerson PopulateContactPerson() => new FCFCDFContactPerson
		{
			MailAddress = ContactPerson?.MailAddress,
			Name = ContactPerson?.PersonName,
			PhoneNumber = ContactPerson?.PhoneNumber,
			Position = ContactPerson?.Position,
		};

		FCFCDFDutyDefermentApproval PopulateDutyDefermentApproval(IDutyDefermentApproval dda) => new FCFCDFDutyDefermentApproval
		{
			Type = dda.Type,
			ApplicationType = dda.ApplicationType,
			AccountPrefix = dda.AccountPrefix,
			AccountNumber = dda.AccountNumber,
			AuthorisationNumber = dda.AuthorisationNumber,
			DutyDefermentApplicant = new FCFCDFDutyDefermentApprovalDutyDefermentApplicant
			{
				Identification = new FCFCDFDutyDefermentApprovalDutyDefermentApplicantIdentification
				{
					ReferenceNumber = dda.Applicant
				}
			}
		};

		FCFCDFBorderTransportMeans PopulateBorderTransportMeans() => new FCFCDFBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength),
			Nationality = header.BorderTransportMeansNationality
		};

		FCFCDFArrivalTransportMeans PopulateArrivalTransportMeans() => new FCFCDFArrivalTransportMeans
		{
			Identity = header.ArrivalTransportMeansIdentity
		};

		FCFCDFPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new FCFCDFPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<FCFCDFPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new FCFCDFPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		FCFCDFSummaryDeclaration PopulateSummaryDeclaration()
		{
			return new FCFCDFSummaryDeclaration
			{
				IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? FCFCDFSummaryDeclarationIdentificationIndicator.REG : FCFCDFSummaryDeclarationIdentificationIndicator.AWB,
				GoodsItem = summaryDeclaration.GoodsItems.Select(PopulateSummaryDeclarationGoodsItem).ToArray()
			};

			FCFCDFSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsItem(ISummaryDeclarationGoodsItem goodsItem) => new ()
			{
				Quantity = goodsItem.Quantity.ToString(),
				IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
				IdentificationByRegistration = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCDFSummaryDeclarationGoodsItemIdentificationByRegistration>(summaryDeclaration.IdentificationIndicator, goodsItem.IdentificationByRegistrationReferencedRegistrationNumber, im => im.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString()),
			};

			FCFCDFSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
			{
				var summaryDeclarationIdentificationIndicatorIsAWB = summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				return new FCFCDFSummaryDeclarationGoodsItemIdentificationByKey
				{
					Kind = summaryDeclarationIdentificationIndicatorIsAWB ? FCFCDFSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : FCFCDFSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
					Number = goodsItem.IdentificationByKeyNumber,
					Custodian = new FCFCDFSummaryDeclarationGoodsItemIdentificationByKeyCustodian
					{
						Identification = new FCFCDFSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
					}
				};
			}
		}

		FCFCDFCustomsWarehouse PopulateCustomsWarehouse()
		{
			return new FCFCDFCustomsWarehouse
			{
				SequenceNumber = "1",
				GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = new FCFCDFCustomsWarehouseCustomsAuthorisation { WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier },
				LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
				GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
			};

			FCFCDFCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter) =>
				CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCDFCustomsWarehouseGoodsItem>(
					goodsItem.ReferencedRegistrationNumber,
					gi =>
					{
						gi.SequenceNumber = counter.ToString();
						gi.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
						gi.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
						gi.CommodityCode = goodsItem.CommodityCode;
						gi.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
						gi.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
						gi.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
						gi.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
					});
		}

		FCFCDFInwardProcessing PopulateInwardProcessing()
		{
			return new FCFCDFInwardProcessing
			{
				SequenceNumber = "1",
				GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
				SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
				MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
				GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
			};

			FCFCDFInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new FCFCDFInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier };

			FCFCDFInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new FCFCDFInwardProcessingMonitoringCustomsOffice
			{
				Identification = new FCFCDFInwardProcessingMonitoringCustomsOfficeIdentification
				{
					ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
				}
			};

			FCFCDFInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter) =>
				CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCDFInwardProcessingGoodsItem>(
					goodsItem.ReferencedRegistrationNumber,
					gi =>
					{
						gi.SequenceNumber = counter.ToString();
						gi.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
						gi.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
						gi.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
					});
		}

		FCFCDFBody PopulateBody() => new FCFCDFBody
		{
			CustomsValueFlag = header.CustomsValue != null ? "1" : "0",
			Consignor = Consignor != null ? PopulateConsignor() : null,
			Consignee = Consignee != null ? PopulateConsignee() : null,
			AdditionalDutyReferences = PopulateAdditionalDutyReferences(),
			Containers = PopulateContainers(),
			DeliveryTerms = PopulateDeliveryTerms(),
			PaymentTransaction = header.PaymentTransaction == null ? null : PopulatePaymentTransaction(),
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			CustomsValue = header.CustomsValue != null ? PopulateCustomsValue(header.CustomsValue) : null,
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		FCFCDFBodyConsignor PopulateConsignor() => new FCFCDFBodyConsignor
		{
			Identification = !ConsignorHasEoriNumber ? null : new FCFCDFBodyConsignorIdentification
			{
				ReferenceNumber = Consignor.Identification.EoriNumber
			},
			Name = ConsignorHasEoriNumber ? null : Consignor.Address.Name,
			Address = ConsignorHasEoriNumber ? null : new FCFCDFBodyConsignorAddress
			{
				City = Consignor.Address.City,
				Country = Consignor.Address.Country,
				District = Consignor.Address.District,
				Line = Consignor.Address.Address,
				Postcode = Consignor.Address.Postcode
			}
		};

		FCFCDFBodyConsignee PopulateConsignee() => new FCFCDFBodyConsignee
		{
			Identification = !ConsigneeHasEoriNumber ? null : new FCFCDFBodyConsigneeIdentification
			{
				ReferenceNumber = Consignee.Identification.EoriNumber,
				SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
			},
			Name = ConsigneeHasEoriNumber ? null : Consignee.Address.Name,
			Address = ConsigneeHasEoriNumber ? null : new FCFCDFBodyConsigneeAddress
			{
				City = Consignee.Address.City,
				Country = Consignee.Address.Country,
				District = Consignee.Address.District,
				Line = Consignee.Address.Address,
				Postcode = Consignee.Address.Postcode
			}
		};

		FCFCDFBodyAdditionalDutyReferences[] PopulateAdditionalDutyReferences()
		{
			return header.AdditionalDutyReferences.Select(duty => new FCFCDFBodyAdditionalDutyReferences()
			{
				ReferenceNumber = duty.ReferenceNumber,
				DutyInterestedParty = duty.DutyInterestedParty != null ? PopuateDutyInterestedParty(duty.DutyInterestedParty) : null
			}).ToArray();
		}

		FCFCDFBodyAdditionalDutyReferencesDutyInterestedParty PopuateDutyInterestedParty(IImportParty dutyInterestedParty)
		{
			var dutyInterestedPartyHasEoriNumber = !dutyInterestedParty.Identification.EoriNumberIsEmpty();
			return new FCFCDFBodyAdditionalDutyReferencesDutyInterestedParty()
			{
				Identification = !dutyInterestedPartyHasEoriNumber ? null : new FCFCDFBodyAdditionalDutyReferencesDutyInterestedPartyIdentification()
				{
					ReferenceNumber = dutyInterestedParty.Identification.EoriNumber
				},
				Name = dutyInterestedPartyHasEoriNumber ? null : dutyInterestedParty.Address.Name,
				Address = dutyInterestedPartyHasEoriNumber ? null : new FCFCDFBodyAdditionalDutyReferencesDutyInterestedPartyAddress
				{
					City = dutyInterestedParty.Address.City,
					Country = dutyInterestedParty.Address.Country,
					District = dutyInterestedParty.Address.District,
					Line = dutyInterestedParty.Address.Address,
					Postcode = dutyInterestedParty.Address.Postcode
				}
			};
		}

		FCFCDFBodyContainers PopulateContainers() => new FCFCDFBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerIdentificationNumbers.Select(c => new FCFCDFBodyContainersContainer { IdentificationNumber = c }).ToArray(),
		};

		FCFCDFBodyDeliveryTerms PopulateDeliveryTerms() => new FCFCDFBodyDeliveryTerms
		{
			Code = header.DeliveryTermsCode,
			Description = header.DeliveryTermsDescription,
			Place = header.DeliveryTermsPlace,
			Key = header.DeliveryTermsKey
		};

		FCFCDFBodyPaymentTransaction PopulatePaymentTransaction() => new FCFCDFBodyPaymentTransaction
		{
			Amount = header.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = header.PaymentTransaction.CurrencyCode
		};

		FCFCDFBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new FCFCDFBodyForeignTradeStatistics
		{
			GoodsStatus = header.ForeignTradeStatisticsGoodsStatus,
			TransactionType = header.ForeignTradeStatisticsTransactionType,
			DestinationCountry = header.ForeignTradeStatisticsDestinationCountry,
			DestinationFederalState = header.ForeignTradeStatisticsDestinationFederalState,
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
			EntryCustomsOffice = new FCFCDFBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = header.EntryCustomsOfficeReferenceNumber
			}
		};

		FCFCDFBodyCustomsValue PopulateCustomsValue(ICustomsValue customsValue)
		{
			var vendor = customsValue.Vendor;
			var vendee = customsValue.Vendee;
			var vendorHasEoriNumber = !vendor?.Identification.EoriNumberIsEmpty() ?? false;
			var vendeeHasEoriNumber = !vendee?.Identification.EoriNumberIsEmpty() ?? false;

			return new FCFCDFBodyCustomsValue
			{
				FormerDecisions = customsValue.FormerDecisions,
				Vendor = vendor != null ? PopulateVendor() : null,
				Vendee = vendee != null ? PopulateVendee() : null,
				Affiliation = PopulateAffiliation(),
				RestrictionOrCondition = PopulateRestrictionOrCondition(),
				LicenseFee = PopulateLicenseFee(),
				Resale = PopulateResale()
			};

			FCFCDFBodyCustomsValueVendor PopulateVendor() => new FCFCDFBodyCustomsValueVendor
			{
				Identification = !vendorHasEoriNumber ? null : new FCFCDFBodyCustomsValueVendorIdentification
				{
					ReferenceNumber = vendor.Identification.EoriNumber
				},
				Name = vendorHasEoriNumber ? null : vendor.Address.Name,
				Address = vendorHasEoriNumber ? null : new FCFCDFBodyCustomsValueVendorAddress
				{
					City = vendor.Address.City,
					Country = vendor.Address.Country,
					District = vendor.Address.District,
					Line = vendor.Address.Address,
					Postcode = vendor.Address.Postcode
				}
			};

			FCFCDFBodyCustomsValueVendee PopulateVendee() => new FCFCDFBodyCustomsValueVendee
			{
				Identification = !vendeeHasEoriNumber ? null : new FCFCDFBodyCustomsValueVendeeIdentification
				{
					ReferenceNumber = vendee.Identification.EoriNumber
				},
				Name = vendeeHasEoriNumber ? null : vendee.Address.Name,
				Address = vendeeHasEoriNumber ? null : new FCFCDFBodyCustomsValueVendeeAddress
				{
					City = vendee.Address.City,
					Country = vendee.Address.Country,
					District = vendee.Address.District,
					Line = vendee.Address.Address,
					Postcode = vendee.Address.Postcode
				}
			};

			FCFCDFBodyCustomsValueAffiliation PopulateAffiliation() => new FCFCDFBodyCustomsValueAffiliation
			{
				Type = customsValue.AffiliationType,
				Description = customsValue.AffiliationDescription
			};

			FCFCDFBodyCustomsValueRestrictionOrCondition PopulateRestrictionOrCondition() => new FCFCDFBodyCustomsValueRestrictionOrCondition
			{
				RestrictionFlag = customsValue.RestrictionFlag.MapBoolToJN(),
				ConditionFlag = customsValue.ConditionFlag.MapBoolToJN(),
				Description = customsValue.RestrictionOrConditionDescription
			};

			FCFCDFBodyCustomsValueLicenseFee PopulateLicenseFee() => new FCFCDFBodyCustomsValueLicenseFee
			{
				LicenseFeeFlag = customsValue.LicenseFeeFlag.MapBoolToJN(),
				Description = customsValue.LicenseFeeDescription
			};

			FCFCDFBodyCustomsValueResale PopulateResale() => new FCFCDFBodyCustomsValueResale
			{
				ResaleFlag = customsValue.ResaleFlag.MapBoolToJN(),
				Description = customsValue.ResaleDescription
			};
		}

		FCFCDFBodyDocument PopulateDocument(IImportDocument d) => new FCFCDFBodyDocument
		{
			Division = FCFCDFBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault(),
		};

		FCFCDFBodyGoodsItem PopulateLine(ICFCDECLine line)
		{
			var originCountry = line.OriginCountry;
			var preferentialOriginCountry = line.PreferentialOriginCountry;
			return new FCFCDFBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new FCFCDFBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				CessionManagementFlag = line.CessionManagementFlag,
				GoodsDescription = line.GoodsDescription,
				InvoiceAmount = line.InvoiceAmount.Round(2).Normalize(),
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = originCountry != preferentialOriginCountry || int.TryParse(line.PreferentialTreatment?.RequestedPreferentialTreatment, out var requestedPreferentialTreatment) && requestedPreferentialTreatment < 200 ? originCountry.ValueOrNullIfEmpty() : null,
				PreferentialOriginCountry = preferentialOriginCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				TobaccoRevenueStampNumber = line.TobaccoRevenueStampNumber,
				CommodityCode = new FCFCDFBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				ForeignTradeStatistics = new FCFCDFBodyGoodsItemForeignTradeStatistics
				{
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
				},
				CustomsValue = line.CustomsValue != null ? PopulateLineCustomsValue(line) : null,
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = line.PreferentialTreatment != null ? PopulatePreferentialTreatment(line) : null,
				SpecialCase = line.SpecialCases.Select(c => PopulateSpecialCases(c)).ToArray(),
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
		}

		FCFCDFBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure)
		{
			return new FCFCDFBodyGoodsItemAdditionalProcedure
			{
				Code = additionalProcedure
			};
		}

		FCFCDFBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode)
		{
			return new FCFCDFBodyGoodsItemSupplementaryCodes
			{
				Code = supplementaryCode
			};
		}

		FCFCDFBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			return new FCFCDFBodyGoodsItemPackage
			{
				Kind = package.Kind,
				Quantity = package?.Quantity.ToString(),
				MarksNumbers = package.MarksNumbers
			};
		}

		FCFCDFBodyGoodsItemCustomsValueNetPrice PopulateNetPrice(IImportCosts netPrice)
		{
			var isEUR = netPrice.CurrencyCode == CurrencyCodes.Germany;
			return new FCFCDFBodyGoodsItemCustomsValueNetPrice
			{
				Value = netPrice.Value.Round(2).Normalize(),
				CurrencyCode = netPrice.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? netPrice.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = netPrice.CurrencyRate,
				CurrencyRateSpecified = !netPrice.CurrencyRate.IsZero() && !isEUR,
			};
		}

		FCFCDFBodyGoodsItemCustomsValueIndirectPayment PopulateIndirectPayment(IImportCosts indirectPayment)
		{
			var isEUR = indirectPayment.CurrencyCode == CurrencyCodes.Germany;
			return new FCFCDFBodyGoodsItemCustomsValueIndirectPayment
			{
				Value = indirectPayment.Value.Round(2).Normalize(),
				CurrencyCode = indirectPayment.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? indirectPayment.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = indirectPayment.CurrencyRate,
				CurrencyRateSpecified = !indirectPayment.CurrencyRate.IsZero() && !isEUR,
			};
		}

		FCFCDFBodyGoodsItemCustomsValueAirFreightCosts PopulateAirFreightCosts(IAirFreightCosts airFreightCosts)
		{
			var isEUR = airFreightCosts.CurrencyCode == CurrencyCodes.Germany;
			return new FCFCDFBodyGoodsItemCustomsValueAirFreightCosts
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

		FCFCDFBodyGoodsItemCustomsValueAdditionDeduction PopulateAdditionDeduction(IAdditionDeduction additionDeduction)
		{
			var isEUR = additionDeduction.CurrencyCode == CurrencyCodes.Germany;
			return new FCFCDFBodyGoodsItemCustomsValueAdditionDeduction
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

		FCFCDFBodyGoodsItemCustomsValue PopulateLineCustomsValue(ICFCDECLine line)
		{
			return new FCFCDFBodyGoodsItemCustomsValue
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

		FCFCDFBodyGoodsItemAssessment PopulateAssessment(ICFCDECLine line)
		{
			return new FCFCDFBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				OutwardProcessingFee = line.AssessmentOutwardProcessingFee,
				OutwardProcessingFeeSpecified = !line.AssessmentOutwardProcessingFee.IsZero(),
				TaxCosts = line.AssessmentTaxCosts,
				TaxCostsSpecified = !line.AssessmentTaxCosts.IsZero(),
				Amount = line.AssessmentAmount.Select(a => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFBodyGoodsItemAssessmentAmount>(a)).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};

			FCFCDFBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
			{
				return new FCFCDFBodyGoodsItemAssessmentSpecificRate
				{
					Type = rate.Type,
					Value = rate.Value.Round(2).Normalize()
				};
			}

			FCFCDFBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
			{
				return new FCFCDFBodyGoodsItemAssessmentContentInformation
				{
					Type = contentInformation.ContentType,
					DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
				};
			}
		}

		FCFCDFBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new FCFCDFBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		FCFCDFBodyGoodsItemPreferentialTreatmentDeclarationContingent PopulateContingentNumber(string contingentNumber)
		{
			return new FCFCDFBodyGoodsItemPreferentialTreatmentDeclarationContingent
			{
				ContingentNumber = contingentNumber
			};
		}

		FCFCDFBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(ICFCDECLine line)
		{
			return new FCFCDFBodyGoodsItemPreferentialTreatment
			{
				RequestedPreferentialTreatment = line.PreferentialTreatment.RequestedPreferentialTreatment,
				Declaration = new FCFCDFBodyGoodsItemPreferentialTreatmentDeclaration
				{
					Contingent = line.PreferentialTreatment.ContingentNumber.Select(cn => PopulateContingentNumber(cn)).ToArray(),
					PreferentialTreatmentQuantity = line.PreferentialTreatment.Quantity != null ? PopulatePreferentialTreatmentQuantity(line.PreferentialTreatment.Quantity) : null,
				}
			};
		}

		FCFCDFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity PopulatePreferentialTreatmentQuantity(IAmount amount)
		{
			return new FCFCDFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity
			{
				Quantity = amount.Quantity.RoundAndNormalize(0).ToString(),
				MeasurementUnit = amount.MeasurementUnit,
				Qualifier = amount.Qualifier
			};
		}

		FCFCDFBodyGoodsItemSpecialCase PopulateSpecialCases(IImportSpecialCase specialCase)
		{
			return new FCFCDFBodyGoodsItemSpecialCase
			{
				Group = specialCase.Group,
				ApplicationType = specialCase.ApplicationType,
				RateOrAmountOrFactor = specialCase.RateOrAmountOrFactor,
				RateOrAmountOrFactorSpecified = !specialCase.RateOrAmountOrFactor.IsZero(),
			};
		}

		FCFCDFBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new FCFCDFBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<FCFCDFBodyGoodsItemDocumentWriteOff>(document.WriteOff)
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
