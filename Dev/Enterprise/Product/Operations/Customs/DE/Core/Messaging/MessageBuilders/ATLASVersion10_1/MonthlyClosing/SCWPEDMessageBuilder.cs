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
	public sealed class SCWPEDMessageBuilder : MonthlyClosingMessageBuilder<LSCWPM>
	{
		public SCWPEDMessageBuilder(IImportMessageHeader messageHeaderProvider)
		{
			this.messageHeaderProvider = Argument.NotNull(messageHeaderProvider, nameof(messageHeaderProvider));
			headerProvider = (ISCWPEDHeader)messageHeaderProvider.Header;
		}
		readonly IImportMessageHeader messageHeaderProvider;
		readonly ISCWPEDHeader headerProvider;

		protected override LSCWPM GetMessageCore() => new LSCWPM
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = DeclarantHasEoriNumber ? PopulateDeclarant() : null,
			Representative = RepresentativeHasEoriNumber ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = PopulateContactPerson(),
			Body = headerProvider.Bodies.Select(x => PopulateBody(x)).ToArray()
		};

		LSCWPMMetaData PopulateMetaData() => new LSCWPMMetaData
		{
			Preparation = new LSCWPMMetaDataPreparation
			{
				Date = messageHeaderProvider.PreparationDateAndTimeCET.Date,
				Time = messageHeaderProvider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = messageHeaderProvider.MessageGroup.MapCodeToEnumWithDefault<LSCWPMMetaDataMessageGroup>(),
			MessageType = LSCWPMMetaDataMessageType.LSCWPM,
			InterchangeSender = new LSCWPMMetaDataInterchangeSender
			{
				Identification = new LSCWPMMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = messageHeaderProvider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new LSCWPMMetaDataInterchangeRecipient
			{
				Identification = new LSCWPMMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		LSCWPMHeader PopulateHeader()
		{
			var startAccountingPeriodDate = headerProvider.StartAccountingPeriodDate;
			var endAccountingPeriodDate = headerProvider.EndAccountingPeriodDate;
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWPMHeader>(headerProvider.ReferenceNumber, x =>
			{
				x.MessageVersion = "M.1.0";
				x.MessageRole = headerProvider.MessageRole;
				x.MessageCreationDate = messageHeaderProvider.PreparationDateAndTimeCET.Date;
				x.Declaration = new LSCWPMHeaderDeclaration
				{
					Kind = headerProvider.DeclarationKind.MapCodeToEnumWithDefault<LSCWPMHeaderDeclarationKind>()
				};
				x.LRN = headerProvider.LocalReferenceNumber.LeftOrNull(LRNMaxLength);
				x.StartAccountingPeriodDate = startAccountingPeriodDate.GetValueOrDefault();
				x.StartAccountingPeriodDateSpecified = startAccountingPeriodDate.HasValue;
				x.EndAccountingPeriodDate = endAccountingPeriodDate.GetValueOrDefault();
				x.EndAccountingPeriodDateSpecified = endAccountingPeriodDate.HasValue;
				x.DeclarantIsConsigneeFlag = headerProvider.DeclarantIsConsigneeFlag.MapBoolToJN();
				x.CustomsAuthorisation = !headerProvider.LocalClearanceProcedure.IsEmpty() || !headerProvider.ProcedureAuthorization.IsEmpty() ? new LSCWPMHeaderCustomsAuthorisation
				{
					LocalClearanceProcedure = headerProvider.LocalClearanceProcedure,
					CurrentProcedure = headerProvider.ProcedureAuthorization
				} : null;
				x.CurrencyCode = headerProvider.CurrencyCode.MapCodeToEnumWithDefault<LSCWPMHeaderCurrencyCode>();
				x.CurrencyCodeSpecified = true;
				x.RepresentativeRelationshipFlag = headerProvider.RepresentativeRelationshipFlag;
				x.DeclarationPlace = headerProvider.DeclarationPlace;
				x.AuthorisationNumber = messageHeaderProvider.AuthorisationNumber.LeftOrNull(AuthorisationNumberMaxLength);
			});
		}

		LSCWPMDeclarant PopulateDeclarant() => new LSCWPMDeclarant
		{
			Identification = new LSCWPMDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			}
		};

		LSCWPMRepresentative PopulateRepresentative() => new LSCWPMRepresentative
		{
			Identification = new LSCWPMRepresentativeIdentification
			{
				ReferenceNumber = Representative.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Representative.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			}
		};

		LSCWPMPrincipal PopulatePrincipal() => new LSCWPMPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new LSCWPMPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new LSCWPMPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		LSCWPMContactPerson PopulateContactPerson() => new LSCWPMContactPerson
		{
			MailAddress = ContactPerson?.MailAddress ?? string.Empty,
			Name = ContactPerson?.PersonName ?? string.Empty,
			PhoneNumber = ContactPerson?.PhoneNumber ?? string.Empty,
			Position = ContactPerson?.Position ?? string.Empty,
		};

		LSCWPMBody PopulateBody(ISCWPEDBody body)
		{
			var consignee = body.Consignee;
			var consigneeHasEoriNumber = !consignee?.Identification.EoriNumberIsEmpty() ?? false;
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWPMBody>(body.ReferenceNumber, x =>
			{
				x.CustomsValueFlag = body.CustomsValue != null ? "1" : "0";
				x.Consignee = consignee != null ? PopulateConsignee() : null;
				x.DeliveryTerms = PopulateDeliveryTerms(body);
				x.PaymentTransaction = body.PaymentTransaction != null ? PopulatePaymentTransaction(body) : null;
				x.ForeignTradeStatistics = !string.IsNullOrWhiteSpace(body.ForeignTradeStatisticsEntryCustomsOffice) ? PopulateForeignTradeStatistics(body.ForeignTradeStatisticsEntryCustomsOffice) : null;
				x.CustomsValue = body.CustomsValue != null ? PopulateCustomsValue(body.CustomsValue) : null;
				x.Document = body.Documents.Select(x => PopulateDocument(x)).ToArray();
				x.GoodsItem = body.Lines.Select(x => PopulateLine(x)).ToArray();
			});

			LSCWPMBodyConsignee PopulateConsignee() => new LSCWPMBodyConsignee
			{
				Identification = !consigneeHasEoriNumber ? null : new LSCWPMBodyConsigneeIdentification
				{
					ReferenceNumber = consignee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = consignee.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				},
				Name = consigneeHasEoriNumber ? null : consignee.Address.Name,
				Address = consigneeHasEoriNumber ? null : new LSCWPMBodyConsigneeAddress
				{
					City = consignee.Address.City,
					Country = consignee.Address.Country,
					District = consignee.Address.District,
					Line = consignee.Address.Address,
					Postcode = consignee.Address.Postcode
				}
			};
		}

		LSCWPMBodyDeliveryTerms PopulateDeliveryTerms(ISCWPEDBody body) => new LSCWPMBodyDeliveryTerms
		{
			Code = body.DeliveryTermsCode,
			Description = body.DeliveryTermsDescription,
			Place = body.DeliveryTermsPlace,
			Key = body.DeliveryTermsKey
		};

		LSCWPMBodyPaymentTransaction PopulatePaymentTransaction(ISCWPEDBody body) => new LSCWPMBodyPaymentTransaction
		{
			Amount = body.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = body.PaymentTransaction.CurrencyCode
		};

		LSCWPMBodyForeignTradeStatistics PopulateForeignTradeStatistics(string foreignTradeStatisticsEntryCustomsOffice) => new LSCWPMBodyForeignTradeStatistics
		{
			EntryCustomsOffice = new LSCWPMBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = foreignTradeStatisticsEntryCustomsOffice
			}
		};

		LSCWPMBodyCustomsValue PopulateCustomsValue(ICustomsValue customsValue)
		{
			var vendor = customsValue.Vendor;
			var vendee = customsValue.Vendee;
			var vendorHasEoriNumber = !vendor?.Identification.EoriNumberIsEmpty() ?? false;
			var vendeeHasEoriNumber = !vendee?.Identification.EoriNumberIsEmpty() ?? false;

			return new LSCWPMBodyCustomsValue
			{
				FormerDecisions = customsValue.FormerDecisions,
				Vendor = vendor != null ? PopulateVendor() : null,
				Vendee = vendee != null ? PopulateVendee() : null,
				Affiliation = PopulateAffiliation(),
				RestrictionOrCondition = PopulateRestrictionOrCondition(),
				LicenseFee = PopulateLicenseFee(),
				Resale = PopulateResale()
			};

			LSCWPMBodyCustomsValueVendor PopulateVendor() => new LSCWPMBodyCustomsValueVendor
			{
				Identification = !vendorHasEoriNumber ? null : new LSCWPMBodyCustomsValueVendorIdentification
				{
					ReferenceNumber = vendor.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendorHasEoriNumber ? null : vendor.Address.Name,
				Address = vendorHasEoriNumber ? null : new LSCWPMBodyCustomsValueVendorAddress
				{
					City = vendor.Address.City,
					Country = vendor.Address.Country,
					District = vendor.Address.District,
					Line = vendor.Address.Address,
					Postcode = vendor.Address.Postcode
				}
			};

			LSCWPMBodyCustomsValueVendee PopulateVendee() => new LSCWPMBodyCustomsValueVendee
			{
				Identification = !vendeeHasEoriNumber ? null : new LSCWPMBodyCustomsValueVendeeIdentification
				{
					ReferenceNumber = vendee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendeeHasEoriNumber ? null : vendee.Address.Name,
				Address = vendeeHasEoriNumber ? null : new LSCWPMBodyCustomsValueVendeeAddress
				{
					City = vendee.Address.City,
					Country = vendee.Address.Country,
					District = vendee.Address.District,
					Line = vendee.Address.Address,
					Postcode = vendee.Address.Postcode
				}
			};

			LSCWPMBodyCustomsValueAffiliation PopulateAffiliation() => new LSCWPMBodyCustomsValueAffiliation
			{
				Type = customsValue.AffiliationType,
				Description = customsValue.AffiliationDescription
			};

			LSCWPMBodyCustomsValueRestrictionOrCondition PopulateRestrictionOrCondition() => new LSCWPMBodyCustomsValueRestrictionOrCondition
			{
				RestrictionFlag = customsValue.RestrictionFlag.MapBoolToJN(),
				ConditionFlag = customsValue.ConditionFlag.MapBoolToJN(),
				Description = customsValue.RestrictionOrConditionDescription
			};

			LSCWPMBodyCustomsValueLicenseFee PopulateLicenseFee() => new LSCWPMBodyCustomsValueLicenseFee
			{
				LicenseFeeFlag = customsValue.LicenseFeeFlag.MapBoolToJN(),
				Description = customsValue.LicenseFeeDescription
			};

			LSCWPMBodyCustomsValueResale PopulateResale() => new LSCWPMBodyCustomsValueResale
			{
				ResaleFlag = customsValue.ResaleFlag.MapBoolToJN(),
				Description = customsValue.ResaleDescription
			};
		}

		LSCWPMBodyDocument PopulateDocument(IImportDocument document) => new LSCWPMBodyDocument
		{
			Division = LSCWPMBodyDocumentDivision.Item4,
			Type = document.Type,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
		};

		LSCWPMBodyGoodsItem PopulateLine(ISCWPEDLine line)
		{
			var originCountry = line.OriginCountry;
			var lineNumber = line.SequenceNumber;
			AddToLineNumbersInMessage(lineNumber);
			return new LSCWPMBodyGoodsItem
			{
				SequenceNumber = lineNumber.ToString(),
				ReferredSequenceNumber = line.ReferencedSequenceNumber?.ToString(),
				MatterCode = line.MatterCode,
				ArticleNumber = line.ArticleNumber,
				InvoiceAmount = line.InvoiceAmount?.RoundAndNormalize(2) ?? default,
				InvoiceAmountSpecified = true,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				NetMassMeasureSpecified = line.NetMassMeasureSpecified,
				OriginCountry = originCountry,
				DepartureCountry = line.DepartureCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CompleteDeclarationFlag = "J",
				CommodityCode = new LSCWPMBodyGoodsItemCommodityCode { CommodityCode = line.CommodityCode },
				AdditionalProcedure = line.AdditionalProcedure.Select(x => PopulateAdditionalProcedure(x)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(x => PopulateSupplementaryCode(x)).ToArray(),
				ForeignTradeImportEarlyClearanceFlag = line.ForeignTradeImportEarlyClearanceFlag,
				ForeignTradeStatistics = new LSCWPMBodyGoodsItemForeignTradeStatistics
				{
					GoodsStatus = line.ForeignTradeStatisticsGoodsStatus,
					DestinationCountry = line.ForeignTradeStatisticsDestinationCountry,
					DestinationFederalState = line.ForeignTradeStatisticsDestinationFederalState,
					InlandTransportMode = line.ForeignTradeStatisticsInlandTransportMode,
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWPMBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
				},
				CustomsValue = line.CustomsValue != null ? PopulateLineCustomsValue(line.CustomsValue) : null,
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(x => PopulateExciseDuty(x)).ToArray(),
				PreferentialTreatment = !string.IsNullOrEmpty(line.RequestedPreferentialTreatment) ? PopulatePreferentialTreatment(line.RequestedPreferentialTreatment) : null,
				Document = line.Documents.Select(x => PopulateLineDocuments(x)).ToArray(),
				BorderTransportMeans = PopulateBorderTransportMeans(line),
				InwardMovement = line.InwardMovementAmount != null ? PopulateInwardMovement(line.InwardMovementAmount) : null
			};
		}

		LSCWPMBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(string requestedPreferentialTreatment) => new LSCWPMBodyGoodsItemPreferentialTreatment
		{
			RequestedPreferentialTreatment = requestedPreferentialTreatment
		};

		LSCWPMBodyGoodsItemInwardMovement PopulateInwardMovement(IAmount inwardMovementAmount) => new LSCWPMBodyGoodsItemInwardMovement
		{
			Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWPMBodyGoodsItemInwardMovementAmount>(inwardMovementAmount)
		};

		LSCWPMBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure) => new LSCWPMBodyGoodsItemAdditionalProcedure
		{
			Code = additionalProcedure
		};

		LSCWPMBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode) => new LSCWPMBodyGoodsItemSupplementaryCodes
		{
			Code = supplementaryCode
		};

		LSCWPMBodyGoodsItemCustomsValue PopulateLineCustomsValue(IImportLineCustomsValue customsValue) => new LSCWPMBodyGoodsItemCustomsValue
		{
			DepartureAirport = customsValue.CustomsValueDepartureAirport,
			DestinationPlace = customsValue.CustomsValueDestinationPlace,
			AdditionDeductionDescription = customsValue.CustomsValueAdditionDeductionDescription,
			NetPrice = PopulateNetPrice(customsValue.CustomsValueNetPrice),
			IndirectPayment = customsValue.CustomsValueIndirectPayment != null ? PopulateIndirectPayment(customsValue.CustomsValueIndirectPayment) : null,
			AirFreightCosts = customsValue.CustomsValueAirFreightCosts != null ? PopulateAirFreightCosts(customsValue.CustomsValueAirFreightCosts) : null,
			AdditionDeduction = customsValue.CustomsValueAdditionDeduction.Select(x => PopulateAdditionDeduction(x)).ToArray(),
		};

		LSCWPMBodyGoodsItemCustomsValueNetPrice PopulateNetPrice(IImportCosts netPrice)
		{
			var isEUR = netPrice.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWPMBodyGoodsItemCustomsValueNetPrice
			{
				Value = netPrice.Value.Round(2).Normalize(),
				CurrencyCode = netPrice.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? netPrice.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = netPrice.CurrencyRate,
				CurrencyRateSpecified = !netPrice.CurrencyRate.IsZero() && !isEUR,
			};
		}

		LSCWPMBodyGoodsItemCustomsValueIndirectPayment PopulateIndirectPayment(IImportCosts indirectPayment)
		{
			var isEUR = indirectPayment.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWPMBodyGoodsItemCustomsValueIndirectPayment
			{
				Value = indirectPayment.Value.Round(2).Normalize(),
				CurrencyCode = indirectPayment.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? indirectPayment.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = indirectPayment.CurrencyRate,
				CurrencyRateSpecified = !indirectPayment.CurrencyRate.IsZero() && !isEUR,
			};
		}

		LSCWPMBodyGoodsItemCustomsValueAirFreightCosts PopulateAirFreightCosts(IAirFreightCosts airFreightCosts)
		{
			var isEUR = airFreightCosts.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWPMBodyGoodsItemCustomsValueAirFreightCosts
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

		LSCWPMBodyGoodsItemCustomsValueAdditionDeduction PopulateAdditionDeduction(IAdditionDeduction additionDeduction)
		{
			var isEUR = additionDeduction.CurrencyCode == CurrencyCodes.Germany;
			return new LSCWPMBodyGoodsItemCustomsValueAdditionDeduction
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

		LSCWPMBodyGoodsItemAssessment PopulateAssessment(ISCWPEDLine line) => new LSCWPMBodyGoodsItemAssessment
		{
			CustomsValue = line.AssessmentCustomsValue,
			CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
			Amount = line.AssessmentAmount.Select(x => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWPMBodyGoodsItemAssessmentAmount>(x)).ToArray(),
			SpecificRate = line.AssessmentSpecificRate.Select(x => PopulateAssessmentSpecificRate(x)).ToArray(),
			ContentInformation = line.AssessmentContentInformation.Select(x => PopulateAssessmentContentInformation(x)).ToArray()
		};

		LSCWPMBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate) => new LSCWPMBodyGoodsItemAssessmentSpecificRate
		{
			Type = rate.Type,
			Value = rate.Value.Round(2).Normalize()
		};

		LSCWPMBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation) => new LSCWPMBodyGoodsItemAssessmentContentInformation
		{
			Type = contentInformation.ContentType,
			DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
		};

		LSCWPMBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty) => new LSCWPMBodyGoodsItemExciseDuty
		{
			Code = exciseDuty.Code,
			DegreePercentage = exciseDuty.DegreePercentage,
			DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
			Value = exciseDuty.Value,
			ValueSpecified = !exciseDuty.Value.IsZero(),
			Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWPMBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
		};

		LSCWPMBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document) => new LSCWPMBodyGoodsItemDocument
		{
			Division = document.Division,
			Type = document.DocumentType,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
			IssuingDateSpecified = document.IssuingDate.HasValue,
			AtHandFlag = document.AtHandFlag,
			WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWPMBodyGoodsItemDocumentWriteOff>(document.WriteOff)
		};

		LSCWPMBodyGoodsItemBorderTransportMeans PopulateBorderTransportMeans(ISCWPEDLine line) => new LSCWPMBodyGoodsItemBorderTransportMeans
		{
			Mode = line.BorderTransportMeansMode,
			Type = line.BorderTransportMeansType,
			Information = line.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength)
		};

		IImportPartyContactPerson ContactPerson => headerProvider.ContactPerson;
		IImportParty Declarant => headerProvider.Declarant;
		IImportParty Principal => headerProvider.Principal;
		IPartyID Representative => headerProvider.Representative;

		bool DeclarantHasEoriNumber => CachedValueHelper.GetValue(ref declarantHasEoriNumberCached, () => !Declarant?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> declarantHasEoriNumberCached;

		bool RepresentativeHasEoriNumber => CachedValueHelper.GetValue(ref representativeHasEoriNumberCached, () => !Representative?.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> representativeHasEoriNumberCached;

		bool PrincipalHasEoriNumber => CachedValueHelper.GetValue(ref principalHasEoriNumberCached, () => !Principal?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> principalHasEoriNumberCached;
	}
}
