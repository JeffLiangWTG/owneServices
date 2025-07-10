using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;
using MessageBuilderExtensions = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CFCPEDMessageBuilder : MonthlyClosingMessageBuilder<ECFCPF>
	{
		public CFCPEDMessageBuilder(IImportMessageHeader messageHeaderProvider)
		{
			this.messageHeaderProvider = Argument.NotNull(messageHeaderProvider, nameof(messageHeaderProvider));
			headerProvider = (ICFCPEDHeader)messageHeaderProvider.Header;
		}
		readonly IImportMessageHeader messageHeaderProvider;
		readonly ICFCPEDHeader headerProvider;

		protected override ECFCPF GetMessageCore() => new ECFCPF
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = PopulateContactPerson(),
			Body = headerProvider.Bodies.Select(x => PopulateBody(x)).ToArray()
		};

		ECFCPFMetaData PopulateMetaData() => new ECFCPFMetaData
		{
			Preparation = new ECFCPFMetaDataPreparation
			{
				Date = messageHeaderProvider.PreparationDateAndTimeCET.Date,
				Time = messageHeaderProvider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = messageHeaderProvider.MessageGroup.MapCodeToEnumWithDefault<ECFCPFMetaDataMessageGroup>(),
			MessageType = ECFCPFMetaDataMessageType.ECFCPF,
			InterchangeSender = new ECFCPFMetaDataInterchangeSender
			{
				Identification = new ECFCPFMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = messageHeaderProvider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new ECFCPFMetaDataInterchangeRecipient
			{
				Identification = new ECFCPFMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		ECFCPFHeader PopulateHeader()
		{
			var startAccountingPeriodDate = headerProvider.StartAccountingPeriodDate;
			var endAccountingPeriodDate = headerProvider.EndAccountingPeriodDate;

			return MessageBuilderExtensions.CreateCommonIdentificationByRegistration<ECFCPFHeader>(headerProvider.ReferenceNumber,
				header =>
				{
					header.MessageVersion = "F.1.2";
					header.MessageRole = headerProvider.MessageRole;
					header.MessageCreationDate = messageHeaderProvider.PreparationDateAndTimeCET.Date;
					header.Declaration = new ECFCPFHeaderDeclaration
					{
						Kind = headerProvider.DeclarationKind.MapCodeToEnumWithDefault<ECFCPFHeaderDeclarationKind>()
					};
					header.LRN = headerProvider.LocalReferenceNumber.LeftOrNull(LRNMaxLength);
					header.StartAccountingPeriodDate = startAccountingPeriodDate.GetValueOrDefault();
					header.StartAccountingPeriodDateSpecified = startAccountingPeriodDate.HasValue;
					header.EndAccountingPeriodDate = endAccountingPeriodDate.GetValueOrDefault();
					header.EndAccountingPeriodDateSpecified = endAccountingPeriodDate.HasValue;
					header.DeclarantIsConsigneeFlag = headerProvider.DeclarantIsConsigneeFlag.MapBoolToJN();
					header.CustomsAuthorisation = new ECFCPFHeaderCustomsAuthorisation
					{
						LocalClearanceProcedure = headerProvider.LocalClearanceProcedure,
						EndUse = headerProvider.ProcedureAuthorization
					};
					header.InputTaxDeductionFlag = headerProvider.InputTaxDeductionFlag.MapBoolToJN();
					header.CurrencyCode = headerProvider.CurrencyCode.MapCodeToEnumWithDefault<ECFCPFHeaderCurrencyCode>();
					header.CurrencyCodeSpecified = true;
					header.TaxOffice = headerProvider.TaxOffice;
					header.RepresentativeRelationshipFlag = headerProvider.RepresentativeRelationshipFlag;
					header.MandateReference = headerProvider.MandateReference;
					header.DeclarationPlace = headerProvider.DeclarationPlace;
					header.AuthorisationNumber = messageHeaderProvider.AuthorisationNumber.LeftOrNull(AuthorisationNumberMaxLength);
				});
		}

		ECFCPFDeclarant PopulateDeclarant() => new ECFCPFDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new ECFCPFDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			},
			Name = DeclarantHasEoriNumber ? null : Declarant.Address.Name,
			Address = DeclarantHasEoriNumber ? null : new ECFCPFDeclarantAddress
			{
				City = Declarant.Address.City,
				Country = Declarant.Address.Country,
				District = Declarant.Address.District,
				Line = Declarant.Address.Address,
				Postcode = Declarant.Address.Postcode
			}
		};

		ECFCPFRepresentative PopulateRepresentative() => new ECFCPFRepresentative
		{
			Identification = new ECFCPFRepresentativeIdentification
			{
				ReferenceNumber = Representative.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Representative.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			}
		};

		ECFCPFPrincipal PopulatePrincipal() => new ECFCPFPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new ECFCPFPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new ECFCPFPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		ECFCPFContactPerson PopulateContactPerson() => new ECFCPFContactPerson
		{
			MailAddress = ContactPerson?.MailAddress ?? string.Empty,
			Name = ContactPerson?.PersonName ?? string.Empty,
			PhoneNumber = ContactPerson?.PhoneNumber ?? string.Empty,
			Position = ContactPerson?.Position ?? string.Empty,
		};

		ECFCPFBody PopulateBody(ICFCPEDBody body)
		{
			var consignor = body.Consignor;
			var consignee = body.Consignee;
			var consignorHasEoriNumber = !consignor?.Identification.EoriNumberIsEmpty() ?? false;
			var consigneeHasEoriNumber = !consignee?.Identification.EoriNumberIsEmpty() ?? false;

			return MessageBuilderExtensions.CreateCommonIdentificationByRegistration<ECFCPFBody>(body.ReferenceNumber,
				b =>
				{
					b.CustomsValueFlag = body.CustomsValue != null ? "1" : "0";
					b.Consignor = consignor != null ? PopulateConsignor() : null;
					b.Consignee = consignee != null ? PopulateConsignee() : null;
					b.AdditionalDutyReferences = body.AdditionalDutyReferences.Select(x => PopulateAdditionalDutyReference(x)).ToArray();
					b.DeliveryTerms = PopulateDeliveryTerms(body);
					b.PaymentTransaction = body.PaymentTransaction != null ? PopulatePaymentTransaction(body) : null;
					b.ForeignTradeStatistics = !string.IsNullOrWhiteSpace(body.ForeignTradeStatisticsEntryCustomsOffice) ? PopulateForeignTradeStatistics(body.ForeignTradeStatisticsEntryCustomsOffice) : null;
					b.CustomsValue = body.CustomsValue != null ? PopulateCustomsValue(body.CustomsValue) : null;
					b.Document = body.Documents.Select(x => PopulateDocument(x)).ToArray();
					b.GoodsItem = body.Lines.Select(x => PopulateLine(x)).ToArray();
				});

			ECFCPFBodyConsignor PopulateConsignor() => new ECFCPFBodyConsignor
			{
				Identification = !consignorHasEoriNumber ? null : new ECFCPFBodyConsignorIdentification
				{
					ReferenceNumber = consignor.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = consignorHasEoriNumber ? null : consignor.Address.Name,
				Address = consignorHasEoriNumber ? null : new ECFCPFBodyConsignorAddress
				{
					City = consignor.Address.City,
					Country = consignor.Address.Country,
					District = consignor.Address.District,
					Line = consignor.Address.Address,
					Postcode = consignor.Address.Postcode
				}
			};

			ECFCPFBodyConsignee PopulateConsignee() => new ECFCPFBodyConsignee
			{
				Identification = !consigneeHasEoriNumber ? null : new ECFCPFBodyConsigneeIdentification
				{
					ReferenceNumber = consignee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = consignee.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				},
				Name = consigneeHasEoriNumber ? null : consignee.Address.Name,
				Address = consigneeHasEoriNumber ? null : new ECFCPFBodyConsigneeAddress
				{
					City = consignee.Address.City,
					Country = consignee.Address.Country,
					District = consignee.Address.District,
					Line = consignee.Address.Address,
					Postcode = consignee.Address.Postcode
				}
			};
		}

		ECFCPFBodyAdditionalDutyReferences PopulateAdditionalDutyReference(IImportAdditionalDutyReference additionalDutyReference) => new ECFCPFBodyAdditionalDutyReferences
		{
			ReferenceNumber = additionalDutyReference.ReferenceNumber,
			DutyInterestedParty = additionalDutyReference.DutyInterestedParty != null ? PopulateDutyInterestedParty(additionalDutyReference.DutyInterestedParty) : null
		};

		ECFCPFBodyAdditionalDutyReferencesDutyInterestedParty PopulateDutyInterestedParty(IImportParty dutyInterestedParty)
		{
			var dutyInterestedPartyHasEoriNumber = !dutyInterestedParty.Identification.EoriNumberIsEmpty();
			return new ECFCPFBodyAdditionalDutyReferencesDutyInterestedParty()
			{
				Identification = !dutyInterestedPartyHasEoriNumber ? null : new ECFCPFBodyAdditionalDutyReferencesDutyInterestedPartyIdentification()
				{
					ReferenceNumber = dutyInterestedParty.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = dutyInterestedPartyHasEoriNumber ? null : dutyInterestedParty.Address.Name,
				Address = dutyInterestedPartyHasEoriNumber ? null : new ECFCPFBodyAdditionalDutyReferencesDutyInterestedPartyAddress
				{
					City = dutyInterestedParty.Address.City,
					Country = dutyInterestedParty.Address.Country,
					District = dutyInterestedParty.Address.District,
					Line = dutyInterestedParty.Address.Address,
					Postcode = dutyInterestedParty.Address.Postcode
				}
			};
		}

		ECFCPFBodyDeliveryTerms PopulateDeliveryTerms(ICFCPEDBody body) => new ECFCPFBodyDeliveryTerms
		{
			Code = body.DeliveryTermsCode,
			Description = body.DeliveryTermsDescription,
			Place = body.DeliveryTermsPlace,
			Key = body.DeliveryTermsKey
		};

		ECFCPFBodyPaymentTransaction PopulatePaymentTransaction(ICFCPEDBody body) => new ECFCPFBodyPaymentTransaction
		{
			Amount = body.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = body.PaymentTransaction.CurrencyCode
		};

		ECFCPFBodyForeignTradeStatistics PopulateForeignTradeStatistics(string foreignTradeStatisticsEntryCustomsOffice) => new ECFCPFBodyForeignTradeStatistics
		{
			EntryCustomsOffice = new ECFCPFBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = foreignTradeStatisticsEntryCustomsOffice
			}
		};

		ECFCPFBodyCustomsValue PopulateCustomsValue(ICustomsValue customsValue)
		{
			var vendor = customsValue.Vendor;
			var vendee = customsValue.Vendee;
			var vendorHasEoriNumber = !vendor?.Identification.EoriNumberIsEmpty() ?? false;
			var vendeeHasEoriNumber = !vendee?.Identification.EoriNumberIsEmpty() ?? false;

			return new ECFCPFBodyCustomsValue
			{
				FormerDecisions = customsValue.FormerDecisions,
				Vendor = vendor != null ? PopulateVendor() : null,
				Vendee = vendee != null ? PopulateVendee() : null,
				Affiliation = PopulateAffiliation(),
				RestrictionOrCondition = PopulateRestrictionOrCondition(),
				LicenseFee = PopulateLicenseFee(),
				Resale = PopulateResale()
			};

			ECFCPFBodyCustomsValueVendor PopulateVendor() => new ECFCPFBodyCustomsValueVendor
			{
				Identification = !vendorHasEoriNumber ? null : new ECFCPFBodyCustomsValueVendorIdentification
				{
					ReferenceNumber = vendor.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendorHasEoriNumber ? null : vendor.Address.Name,
				Address = vendorHasEoriNumber ? null : new ECFCPFBodyCustomsValueVendorAddress
				{
					City = vendor.Address.City,
					Country = vendor.Address.Country,
					District = vendor.Address.District,
					Line = vendor.Address.Address,
					Postcode = vendor.Address.Postcode
				}
			};

			ECFCPFBodyCustomsValueVendee PopulateVendee() => new ECFCPFBodyCustomsValueVendee
			{
				Identification = !vendeeHasEoriNumber ? null : new ECFCPFBodyCustomsValueVendeeIdentification
				{
					ReferenceNumber = vendee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendeeHasEoriNumber ? null : vendee.Address.Name,
				Address = vendeeHasEoriNumber ? null : new ECFCPFBodyCustomsValueVendeeAddress
				{
					City = vendee.Address.City,
					Country = vendee.Address.Country,
					District = vendee.Address.District,
					Line = vendee.Address.Address,
					Postcode = vendee.Address.Postcode
				}
			};

			ECFCPFBodyCustomsValueAffiliation PopulateAffiliation() => new ECFCPFBodyCustomsValueAffiliation
			{
				Type = customsValue.AffiliationType,
				Description = customsValue.AffiliationDescription
			};

			ECFCPFBodyCustomsValueRestrictionOrCondition PopulateRestrictionOrCondition() => new ECFCPFBodyCustomsValueRestrictionOrCondition
			{
				RestrictionFlag = customsValue.RestrictionFlag.MapBoolToJN(),
				ConditionFlag = customsValue.ConditionFlag.MapBoolToJN(),
				Description = customsValue.RestrictionOrConditionDescription
			};

			ECFCPFBodyCustomsValueLicenseFee PopulateLicenseFee() => new ECFCPFBodyCustomsValueLicenseFee
			{
				LicenseFeeFlag = customsValue.LicenseFeeFlag.MapBoolToJN(),
				Description = customsValue.LicenseFeeDescription
			};

			ECFCPFBodyCustomsValueResale PopulateResale() => new ECFCPFBodyCustomsValueResale
			{
				ResaleFlag = customsValue.ResaleFlag.MapBoolToJN(),
				Description = customsValue.ResaleDescription
			};
		}

		ECFCPFBodyDocument PopulateDocument(IImportDocument document) => new ECFCPFBodyDocument
		{
			Division = ECFCPFBodyDocumentDivision.Item4,
			Type = document.Type,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
		};

		ECFCPFBodyGoodsItem PopulateLine(ICFCPEDLine line)
		{
			var originCountry = line.OriginCountry;
			var preferentialOriginCountry = line.PreferentialOriginCountry;
			var lineNumber = line.SequenceNumber;
			AddToLineNumbersInMessage(lineNumber);
			return new ECFCPFBodyGoodsItem
			{
				SequenceNumber = lineNumber.ToString(),
				ReferredSequenceNumber = line.ReferencedSequenceNumber?.ToString(),
				CessionManagementFlag = line.CessionManagementFlag,
				MatterCode = line.MatterCode,
				ArticleNumber = line.ArticleNumber,
				InvoiceAmount = line.InvoiceAmount?.RoundAndNormalize(2) ?? default,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				NetMassMeasureSpecified = line.NetMassMeasureSpecified,
				OriginCountry = originCountry != preferentialOriginCountry
					|| int.TryParse(line.PreferentialTreatment?.RequestedPreferentialTreatment, out var requestedPreferentialTreatment) && requestedPreferentialTreatment < 200 ? originCountry.ValueOrNullIfEmpty() : null,
				PreferentialOriginCountry = preferentialOriginCountry,
				DepartureCountry = line.DepartureCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CompleteDeclarationFlag = "J",
				TobaccoRevenueStampNumber = line.TobaccoRevenueStampNumber,
				CommodityCode = new ECFCPFBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(x => PopulateAdditionalProcedure(x)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(x => PopulateSupplementaryCode(x)).ToArray(),
				ForeignTradeStatistics = new ECFCPFBodyGoodsItemForeignTradeStatistics
				{
					GoodsStatus = line.ForeignTradeStatisticsGoodsStatus,
					TransactionType = line.ForeignTradeStatisticsTransactionType,
					DestinationCountry = line.ForeignTradeStatisticsDestinationCountry,
					DestinationFederalState = line.ForeignTradeStatisticsDestinationFederalState,
					InlandTransportMode = line.ForeignTradeStatisticsInlandTransportMode,
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<ECFCPFBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
				},
				CustomsValue = line.CustomsValue != null ? PopulateLineCustomsValue(line.CustomsValue) : null,
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(x => PopulateExciseDuty(x)).ToArray(),
				PreferentialTreatment = line.PreferentialTreatment != null ? PopulatePreferentialTreatment(line.PreferentialTreatment) : null,
				SpecialCase = line.SpecialCase.Select(x => PopulateSpecialCases(x)).ToArray(),
				Document = line.Documents.Select(x => PopulateLineDocuments(x)).ToArray(),
				BorderTransportMeans = PopulateBorderTransportMeans(line)
			};
		}

		ECFCPFBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure) => new ECFCPFBodyGoodsItemAdditionalProcedure
		{
			Code = additionalProcedure
		};

		ECFCPFBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode) => new ECFCPFBodyGoodsItemSupplementaryCodes
		{
			Code = supplementaryCode
		};

		ECFCPFBodyGoodsItemCustomsValue PopulateLineCustomsValue(IImportLineCustomsValue customsValue) => new ECFCPFBodyGoodsItemCustomsValue
		{
			DepartureAirport = customsValue.CustomsValueDepartureAirport,
			DestinationPlace = customsValue.CustomsValueDestinationPlace,
			AdditionDeductionDescription = customsValue.CustomsValueAdditionDeductionDescription,
			NetPrice = PopulateNetPrice(customsValue.CustomsValueNetPrice),
			IndirectPayment = customsValue.CustomsValueIndirectPayment != null ? PopulateIndirectPayment(customsValue.CustomsValueIndirectPayment) : null,
			AirFreightCosts = customsValue.CustomsValueAirFreightCosts != null ? PopulateAirFreightCosts(customsValue.CustomsValueAirFreightCosts) : null,
			AdditionDeduction = customsValue.CustomsValueAdditionDeduction.Select(x => PopulateAdditionDeduction(x)).ToArray(),
		};

		ECFCPFBodyGoodsItemCustomsValueNetPrice PopulateNetPrice(IImportCosts netPrice)
		{
			var isEUR = netPrice.CurrencyCode == CurrencyCodes.Germany;
			return new ECFCPFBodyGoodsItemCustomsValueNetPrice
			{
				Value = netPrice.Value.Round(2).Normalize(),
				CurrencyCode = netPrice.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? netPrice.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = netPrice.CurrencyRate,
				CurrencyRateSpecified = !netPrice.CurrencyRate.IsZero() && !isEUR,
			};
		}

		ECFCPFBodyGoodsItemCustomsValueIndirectPayment PopulateIndirectPayment(IImportCosts indirectPayment)
		{
			var isEUR = indirectPayment.CurrencyCode == CurrencyCodes.Germany;
			return new ECFCPFBodyGoodsItemCustomsValueIndirectPayment
			{
				Value = indirectPayment.Value.Round(2).Normalize(),
				CurrencyCode = indirectPayment.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? indirectPayment.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = indirectPayment.CurrencyRate,
				CurrencyRateSpecified = !indirectPayment.CurrencyRate.IsZero() && !isEUR,
			};
		}

		ECFCPFBodyGoodsItemCustomsValueAirFreightCosts PopulateAirFreightCosts(IAirFreightCosts airFreightCosts)
		{
			var isEUR = airFreightCosts.CurrencyCode == CurrencyCodes.Germany;
			return new ECFCPFBodyGoodsItemCustomsValueAirFreightCosts
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

		ECFCPFBodyGoodsItemCustomsValueAdditionDeduction PopulateAdditionDeduction(IAdditionDeduction additionDeduction)
		{
			var isEUR = additionDeduction.CurrencyCode == CurrencyCodes.Germany;
			return new ECFCPFBodyGoodsItemCustomsValueAdditionDeduction
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

		ECFCPFBodyGoodsItemAssessment PopulateAssessment(ICFCPEDLine line) => new ECFCPFBodyGoodsItemAssessment
		{
			CustomsValue = line.AssessmentCustomsValue,
			CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
			OutwardProcessingFee = line.AssessmentOutwardProcessingFee.RoundAndNormalize(2),
			OutwardProcessingFeeSpecified = line.AssessmentOutwardProcessingFee != default,
			TaxCosts = line.AssessmentTaxCosts.RoundAndNormalize(2),
			TaxCostsSpecified = line.AssessmentTaxCosts != default,
			Amount = line.AssessmentAmount.Select(x => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<ECFCPFBodyGoodsItemAssessmentAmount>(x)).ToArray(),
			SpecificRate = line.AssessmentSpecificRate.Select(x => PopulateAssessmentSpecificRate(x)).ToArray(),
			ContentInformation = line.AssessmentContentInformation.Select(x => PopulateAssessmentContentInformation(x)).ToArray()
		};

		ECFCPFBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate) => new ECFCPFBodyGoodsItemAssessmentSpecificRate
		{
			Type = rate.Type,
			Value = rate.Value.Round(2).Normalize()
		};

		ECFCPFBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation) => new ECFCPFBodyGoodsItemAssessmentContentInformation
		{
			Type = contentInformation.ContentType,
			DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
		};

		ECFCPFBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty) => new ECFCPFBodyGoodsItemExciseDuty
		{
			Code = exciseDuty.Code,
			DegreePercentage = exciseDuty.DegreePercentage,
			DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
			Value = exciseDuty.Value,
			ValueSpecified = !exciseDuty.Value.IsZero(),
			Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<ECFCPFBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
		};

		ECFCPFBodyGoodsItemPreferentialTreatmentDeclarationContingent PopulateContingentNumber(string contingentNumber) => new ECFCPFBodyGoodsItemPreferentialTreatmentDeclarationContingent
		{
			ContingentNumber = contingentNumber
		};

		ECFCPFBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(ILinePreferentialTreatment preferentialTreatment) => new ECFCPFBodyGoodsItemPreferentialTreatment
		{
			RequestedPreferentialTreatment = preferentialTreatment.RequestedPreferentialTreatment,
			Declaration = new ECFCPFBodyGoodsItemPreferentialTreatmentDeclaration
			{
				Contingent = preferentialTreatment.ContingentNumber.Select(x => PopulateContingentNumber(x)).ToArray(),
				PreferentialTreatmentQuantity = preferentialTreatment.Quantity != null ? PopulatePreferentialTreatmentQuantity(preferentialTreatment.Quantity) : null,
			}
		};

		ECFCPFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity PopulatePreferentialTreatmentQuantity(IAmount amount)
			=> new ECFCPFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity
			{
				Quantity = amount.Quantity.RoundAndNormalize(0).ToString(),
				MeasurementUnit = amount.MeasurementUnit,
				Qualifier = amount.Qualifier
			};

		ECFCPFBodyGoodsItemSpecialCase PopulateSpecialCases(IImportSpecialCase specialCase) => new ECFCPFBodyGoodsItemSpecialCase
		{
			Group = specialCase.Group,
			ApplicationType = specialCase.ApplicationType,
			RateOrAmountOrFactor = specialCase.RateOrAmountOrFactor,
			RateOrAmountOrFactorSpecified = !specialCase.RateOrAmountOrFactor.IsZero(),
		};

		ECFCPFBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document) => new ECFCPFBodyGoodsItemDocument
		{
			Division = document.Division,
			Type = document.DocumentType,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
			IssuingDateSpecified = document.IssuingDate.HasValue,
			AtHandFlag = document.AtHandFlag,
			WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<ECFCPFBodyGoodsItemDocumentWriteOff>(document.WriteOff)
		};

		ECFCPFBodyGoodsItemBorderTransportMeans PopulateBorderTransportMeans(ICFCPEDLine line) => new ECFCPFBodyGoodsItemBorderTransportMeans
		{
			Mode = line.BorderTransportMeansMode,
			Type = line.BorderTransportMeansType,
			Information = line.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength),
			Nationality = line.BorderTransportMeansNationality
		};

		IImportPartyContactPerson ContactPerson => headerProvider.ContactPerson;
		IImportParty Declarant => headerProvider.Declarant;
		IImportParty Principal => headerProvider.Principal;
		IPartyID Representative => headerProvider.Representative;

		bool DeclarantHasEoriNumber => CachedValueHelper.GetValue(ref declarantHasEoriNumberCached, () => !Declarant?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> declarantHasEoriNumberCached;

		bool PrincipalHasEoriNumber => CachedValueHelper.GetValue(ref principalHasEoriNumberCached, () => !Principal?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> principalHasEoriNumberCached;
	}
}
