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
	public sealed class SCIPEDMessageBuilder : MonthlyClosingMessageBuilder<VSCIPK>
	{
		public SCIPEDMessageBuilder(IImportMessageHeader messageHeaderProvider)
		{
			this.messageHeaderProvider = Argument.NotNull(messageHeaderProvider, nameof(messageHeaderProvider));
			headerProvider = (ISCIPEDHeader)messageHeaderProvider.Header;
		}
		readonly IImportMessageHeader messageHeaderProvider;
		readonly ISCIPEDHeader headerProvider;

		protected override VSCIPK GetMessageCore() => new VSCIPK
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = DeclarantHasEoriNumber ? PopulateDeclarant() : null,
			Representative = RepresentativeHasEoriNumber ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = PopulateContactPerson(),
			Body = headerProvider.Bodies.Select(x => PopulateBody(x)).ToArray()
		};

		VSCIPKMetaData PopulateMetaData() => new VSCIPKMetaData
		{
			Preparation = new VSCIPKMetaDataPreparation
			{
				Date = messageHeaderProvider.PreparationDateAndTimeCET.Date,
				Time = messageHeaderProvider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = messageHeaderProvider.MessageGroup.MapCodeToEnumWithDefault<VSCIPKMetaDataMessageGroup>(),
			MessageType = VSCIPKMetaDataMessageType.VSCIPK,
			InterchangeSender = new VSCIPKMetaDataInterchangeSender
			{
				Identification = new VSCIPKMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = messageHeaderProvider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new VSCIPKMetaDataInterchangeRecipient
			{
				Identification = new VSCIPKMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = messageHeaderProvider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		VSCIPKHeader PopulateHeader()
		{
			var startAccountingPeriodDate = headerProvider.StartAccountingPeriodDate;
			var endAccountingPeriodDate = headerProvider.EndAccountingPeriodDate;
			var localClearanceProcedure = headerProvider.LocalClearanceProcedure;
			var procedureAuthorization = headerProvider.ProcedureAuthorization;
			return MessageBuilderExtensions.CreateCommonIdentificationByRegistration<VSCIPKHeader>(headerProvider.ReferenceNumber,
				a =>
				{
					a.MessageVersion = "K.1.0";
					a.MessageRole = headerProvider.MessageRole;
					a.MessageCreationDate = messageHeaderProvider.PreparationDateAndTimeCET.Date;
					a.Declaration = new VSCIPKHeaderDeclaration
					{
						Kind = headerProvider.DeclarationKind.MapCodeToEnumWithDefault<VSCIPKHeaderDeclarationKind>()
					};
					a.LRN = headerProvider.LocalReferenceNumber.LeftOrNull(LRNMaxLength);
					a.StartAccountingPeriodDate = startAccountingPeriodDate.GetValueOrDefault();
					a.StartAccountingPeriodDateSpecified = startAccountingPeriodDate.HasValue;
					a.EndAccountingPeriodDate = endAccountingPeriodDate.GetValueOrDefault();
					a.EndAccountingPeriodDateSpecified = endAccountingPeriodDate.HasValue;
					a.DeclarantIsConsigneeFlag = headerProvider.DeclarantIsConsigneeFlag.MapBoolToJN();
					a.CustomsAuthorisation = localClearanceProcedure.IsEmpty() && procedureAuthorization.IsEmpty() ? null : new VSCIPKHeaderCustomsAuthorisation
					{
						LocalClearanceProcedure = localClearanceProcedure,
						CurrentProcedure = procedureAuthorization
					};
					a.CurrencyCode = headerProvider.CurrencyCode.MapCodeToEnumWithDefault<VSCIPKHeaderCurrencyCode>();
					a.CurrencyCodeSpecified = true;
					a.RepresentativeRelationshipFlag = headerProvider.RepresentativeRelationshipFlag;
					a.DeclarationPlace = headerProvider.DeclarationPlace;
					a.AuthorisationNumber = messageHeaderProvider.AuthorisationNumber.LeftOrNull(AuthorisationNumberMaxLength);
				});
		}

		VSCIPKDeclarant PopulateDeclarant() => new VSCIPKDeclarant
		{
			Identification = new VSCIPKDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			}
		};

		VSCIPKRepresentative PopulateRepresentative() => new VSCIPKRepresentative
		{
			Identification = new VSCIPKRepresentativeIdentification
			{
				ReferenceNumber = Representative.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Representative.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			}
		};

		VSCIPKPrincipal PopulatePrincipal() => new VSCIPKPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new VSCIPKPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new VSCIPKPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		VSCIPKContactPerson PopulateContactPerson() => new VSCIPKContactPerson
		{
			MailAddress = ContactPerson?.MailAddress ?? string.Empty,
			Name = ContactPerson?.PersonName ?? string.Empty,
			PhoneNumber = ContactPerson?.PhoneNumber ?? string.Empty,
			Position = ContactPerson?.Position ?? string.Empty,
		};

		VSCIPKBody PopulateBody(ISCIPEDBody body)
		{
			var consignor = body.Consignor;
			var consignee = body.Consignee;
			var consignorHasEoriNumber = !consignor?.Identification.EoriNumberIsEmpty() ?? false;
			var consigneeHasEoriNumber = !consignee?.Identification.EoriNumberIsEmpty() ?? false;
			return MessageBuilderExtensions.CreateCommonIdentificationByRegistration<VSCIPKBody>(body.ReferenceNumber,
				a =>
				{
					a.CustomsValueFlag = body.CustomsValue != null ? "1" : "0";
					a.Consignor = consignor != null ? PopulateConsignor() : null;
					a.Consignee = consignee != null ? PopulateConsignee() : null;
					a.DeliveryTerms = PopulateDeliveryTerms(body);
					a.PaymentTransaction = body.PaymentTransaction != null ? PopulatePaymentTransaction(body) : null;
					a.ForeignTradeStatistics = !string.IsNullOrWhiteSpace(body.ForeignTradeStatisticsEntryCustomsOffice) ? PopulateForeignTradeStatistics(body.ForeignTradeStatisticsEntryCustomsOffice) : null;
					a.CustomsValue = body.CustomsValue != null ? PopulateCustomsValue(body.CustomsValue) : null;
					a.Document = body.Documents.Select(x => PopulateDocument(x)).ToArray();
					a.GoodsItem = body.Lines.Select(x => PopulateLine(x)).ToArray();
				});

			VSCIPKBodyConsignor PopulateConsignor() => new VSCIPKBodyConsignor
			{
				Identification = !consignorHasEoriNumber ? null : new VSCIPKBodyConsignorIdentification
				{
					ReferenceNumber = consignor.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = consignorHasEoriNumber ? null : consignor.Address.Name,
				Address = consignorHasEoriNumber ? null : new VSCIPKBodyConsignorAddress
				{
					City = consignor.Address.City,
					Country = consignor.Address.Country,
					District = consignor.Address.District,
					Line = consignor.Address.Address,
					Postcode = consignor.Address.Postcode
				}
			};

			VSCIPKBodyConsignee PopulateConsignee() => new VSCIPKBodyConsignee
			{
				Identification = !consigneeHasEoriNumber ? null : new VSCIPKBodyConsigneeIdentification
				{
					ReferenceNumber = consignee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = consignee.Identification.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				},
				Name = consigneeHasEoriNumber ? null : consignee.Address.Name,
				Address = consigneeHasEoriNumber ? null : new VSCIPKBodyConsigneeAddress
				{
					City = consignee.Address.City,
					Country = consignee.Address.Country,
					District = consignee.Address.District,
					Line = consignee.Address.Address,
					Postcode = consignee.Address.Postcode
				}
			};
		}

		VSCIPKBodyDeliveryTerms PopulateDeliveryTerms(ISCIPEDBody body) => new VSCIPKBodyDeliveryTerms
		{
			Code = body.DeliveryTermsCode,
			Description = body.DeliveryTermsDescription,
			Place = body.DeliveryTermsPlace,
			Key = body.DeliveryTermsKey
		};

		VSCIPKBodyPaymentTransaction PopulatePaymentTransaction(ISCIPEDBody body) => new VSCIPKBodyPaymentTransaction
		{
			Amount = body.PaymentTransaction.Value.Round(2).Normalize(),
			AmountSpecified = true,
			CurrencyCode = body.PaymentTransaction.CurrencyCode
		};

		VSCIPKBodyForeignTradeStatistics PopulateForeignTradeStatistics(string foreignTradeStatisticsEntryCustomsOffice) => new VSCIPKBodyForeignTradeStatistics
		{
			EntryCustomsOffice = new VSCIPKBodyForeignTradeStatisticsEntryCustomsOffice
			{
				ReferenceNumber = foreignTradeStatisticsEntryCustomsOffice
			}
		};

		VSCIPKBodyCustomsValue PopulateCustomsValue(ICustomsValue customsValue)
		{
			var vendor = customsValue.Vendor;
			var vendee = customsValue.Vendee;
			var vendorHasEoriNumber = !vendor?.Identification.EoriNumberIsEmpty() ?? false;
			var vendeeHasEoriNumber = !vendee?.Identification.EoriNumberIsEmpty() ?? false;

			return new VSCIPKBodyCustomsValue
			{
				FormerDecisions = customsValue.FormerDecisions,
				Vendor = vendor != null ? PopulateVendor() : null,
				Vendee = vendee != null ? PopulateVendee() : null,
				Affiliation = PopulateAffiliation(),
				RestrictionOrCondition = PopulateRestrictionOrCondition(),
				LicenseFee = PopulateLicenseFee(),
				Resale = PopulateResale()
			};

			VSCIPKBodyCustomsValueVendor PopulateVendor() => new VSCIPKBodyCustomsValueVendor
			{
				Identification = !vendorHasEoriNumber ? null : new VSCIPKBodyCustomsValueVendorIdentification
				{
					ReferenceNumber = vendor.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendorHasEoriNumber ? null : vendor.Address.Name,
				Address = vendorHasEoriNumber ? null : new VSCIPKBodyCustomsValueVendorAddress
				{
					City = vendor.Address.City,
					Country = vendor.Address.Country,
					District = vendor.Address.District,
					Line = vendor.Address.Address,
					Postcode = vendor.Address.Postcode
				}
			};

			VSCIPKBodyCustomsValueVendee PopulateVendee() => new VSCIPKBodyCustomsValueVendee
			{
				Identification = !vendeeHasEoriNumber ? null : new VSCIPKBodyCustomsValueVendeeIdentification
				{
					ReferenceNumber = vendee.Identification.EoriNumber.LeftOrNull(EoriCodeMaxLength)
				},
				Name = vendeeHasEoriNumber ? null : vendee.Address.Name,
				Address = vendeeHasEoriNumber ? null : new VSCIPKBodyCustomsValueVendeeAddress
				{
					City = vendee.Address.City,
					Country = vendee.Address.Country,
					District = vendee.Address.District,
					Line = vendee.Address.Address,
					Postcode = vendee.Address.Postcode
				}
			};

			VSCIPKBodyCustomsValueAffiliation PopulateAffiliation() => new VSCIPKBodyCustomsValueAffiliation
			{
				Type = customsValue.AffiliationType,
				Description = customsValue.AffiliationDescription
			};

			VSCIPKBodyCustomsValueRestrictionOrCondition PopulateRestrictionOrCondition() => new VSCIPKBodyCustomsValueRestrictionOrCondition
			{
				RestrictionFlag = customsValue.RestrictionFlag.MapBoolToJN(),
				ConditionFlag = customsValue.ConditionFlag.MapBoolToJN(),
				Description = customsValue.RestrictionOrConditionDescription
			};

			VSCIPKBodyCustomsValueLicenseFee PopulateLicenseFee() => new VSCIPKBodyCustomsValueLicenseFee
			{
				LicenseFeeFlag = customsValue.LicenseFeeFlag.MapBoolToJN(),
				Description = customsValue.LicenseFeeDescription
			};

			VSCIPKBodyCustomsValueResale PopulateResale() => new VSCIPKBodyCustomsValueResale
			{
				ResaleFlag = customsValue.ResaleFlag.MapBoolToJN(),
				Description = customsValue.ResaleDescription
			};
		}

		VSCIPKBodyDocument PopulateDocument(IImportDocument document) => new VSCIPKBodyDocument
		{
			Division = VSCIPKBodyDocumentDivision.Item4,
			Type = document.Type,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
		};

		VSCIPKBodyGoodsItem PopulateLine(ISCIPEDLine line)
		{
			var lineNumber = line.SequenceNumber;
			AddToLineNumbersInMessage(lineNumber);
			return new VSCIPKBodyGoodsItem
			{
				SequenceNumber = lineNumber.ToString(),
				ReferredSequenceNumber = line.ReferencedSequenceNumber?.ToString(),
				MatterCode = line.MatterCode,
				ArticleNumber = line.ArticleNumber,
				InvoiceAmount = line.InvoiceAmount?.RoundAndNormalize(2) ?? default,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				NetMassMeasureSpecified = line.NetMassMeasureSpecified,
				OriginCountry = line.OriginCountry,
				DepartureCountry = line.DepartureCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CompleteDeclarationFlag = "J",
				CommodityCode = new VSCIPKBodyGoodsItemCommodityCode { CommodityCode = line.CommodityCode },
				AdditionalProcedure = line.AdditionalProcedure.Select(x => PopulateAdditionalProcedure(x)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(x => PopulateSupplementaryCode(x)).ToArray(),
				ForeignTradeStatistics = new VSCIPKBodyGoodsItemForeignTradeStatistics
				{
					GoodsStatus = line.ForeignTradeStatisticsGoodsStatus,
					TransactionType = line.ForeignTradeStatisticsTransactionType,
					DestinationCountry = line.ForeignTradeStatisticsDestinationCountry,
					DestinationFederalState = line.ForeignTradeStatisticsDestinationFederalState,
					InlandTransportMode = line.ForeignTradeStatisticsInlandTransportMode,
					Quantity = line.ForeignTradeStatisticsQuantity.ToString(),
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
					GrossMassMeasureSpecified = !line.ForeignTradeStatisticsGrossMassMeasure.IsZero(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIPKBodyGoodsItemForeignTradeStatisticsAmount>(line.ForeignTradeStatisticsAmount)
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

		VSCIPKBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(string requestedPreferentialTreatment) => new VSCIPKBodyGoodsItemPreferentialTreatment
		{
			RequestedPreferentialTreatment = requestedPreferentialTreatment
		};

		VSCIPKBodyGoodsItemInwardMovement PopulateInwardMovement(IAmount inwardMovementAmount) => new VSCIPKBodyGoodsItemInwardMovement
		{
			Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIPKBodyGoodsItemInwardMovementAmount>(inwardMovementAmount)
		};

		VSCIPKBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure) => new VSCIPKBodyGoodsItemAdditionalProcedure
		{
			Code = additionalProcedure
		};

		VSCIPKBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode) => new VSCIPKBodyGoodsItemSupplementaryCodes
		{
			Code = supplementaryCode
		};

		VSCIPKBodyGoodsItemCustomsValue PopulateLineCustomsValue(IImportLineCustomsValue customsValue) => new VSCIPKBodyGoodsItemCustomsValue
		{
			DepartureAirport = customsValue.CustomsValueDepartureAirport,
			DestinationPlace = customsValue.CustomsValueDestinationPlace,
			AdditionDeductionDescription = customsValue.CustomsValueAdditionDeductionDescription,
			NetPrice = PopulateNetPrice(customsValue.CustomsValueNetPrice),
			IndirectPayment = customsValue.CustomsValueIndirectPayment != null ? PopulateIndirectPayment(customsValue.CustomsValueIndirectPayment) : null,
			AirFreightCosts = customsValue.CustomsValueAirFreightCosts != null ? PopulateAirFreightCosts(customsValue.CustomsValueAirFreightCosts) : null,
			AdditionDeduction = customsValue.CustomsValueAdditionDeduction.Select(x => PopulateAdditionDeduction(x)).ToArray(),
		};

		VSCIPKBodyGoodsItemCustomsValueNetPrice PopulateNetPrice(IImportCosts netPrice)
		{
			var isEUR = netPrice.CurrencyCode == CurrencyCodes.Germany;
			return new VSCIPKBodyGoodsItemCustomsValueNetPrice
			{
				Value = netPrice.Value.Round(2).Normalize(),
				CurrencyCode = netPrice.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? netPrice.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = netPrice.CurrencyRate,
				CurrencyRateSpecified = !netPrice.CurrencyRate.IsZero() && !isEUR,
			};
		}

		VSCIPKBodyGoodsItemCustomsValueIndirectPayment PopulateIndirectPayment(IImportCosts indirectPayment)
		{
			var isEUR = indirectPayment.CurrencyCode == CurrencyCodes.Germany;
			return new VSCIPKBodyGoodsItemCustomsValueIndirectPayment
			{
				Value = indirectPayment.Value.Round(2).Normalize(),
				CurrencyCode = indirectPayment.CurrencyCode,
				CurrencyRateAgreedFlag = !isEUR ? indirectPayment.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = indirectPayment.CurrencyRate,
				CurrencyRateSpecified = !indirectPayment.CurrencyRate.IsZero() && !isEUR,
			};
		}

		VSCIPKBodyGoodsItemCustomsValueAirFreightCosts PopulateAirFreightCosts(IAirFreightCosts airFreightCosts)
		{
			var isEUR = airFreightCosts.CurrencyCode == CurrencyCodes.Germany;
			return new VSCIPKBodyGoodsItemCustomsValueAirFreightCosts
			{
				Value = airFreightCosts.Value.Round(2).Normalize(),
				CurrencyCode = airFreightCosts.CurrencyCode,
				CurrencyRateIATA = !isEUR ? airFreightCosts.CurrencyRateIATA.MapBoolToJN() : null,
				CurrencyRateAgreedFlag = !isEUR ? airFreightCosts.CurrencyRateAgreedFlag.MapBoolToJN() : null,
				CurrencyRate = airFreightCosts.CurrencyRate,
				CurrencyRateSpecified = !airFreightCosts.CurrencyRate.IsZero() && !isEUR,
				CurrencyRateDate = airFreightCosts.CurrencyRateDate.GetValueOrDefault(),
				CurrencyRateDateSpecified = airFreightCosts.CurrencyRateDate.HasValue && !isEUR
			};
		}

		VSCIPKBodyGoodsItemCustomsValueAdditionDeduction PopulateAdditionDeduction(IAdditionDeduction additionDeduction)
		{
			var isEUR = additionDeduction.CurrencyCode == CurrencyCodes.Germany;
			return new VSCIPKBodyGoodsItemCustomsValueAdditionDeduction
			{
				Type = additionDeduction.Type,
				Value = additionDeduction.Value,
				ValueSpecified = !additionDeduction.Value.IsZero(),
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

		VSCIPKBodyGoodsItemAssessment PopulateAssessment(ISCIPEDLine line) => new VSCIPKBodyGoodsItemAssessment
		{
			CustomsValue = line.AssessmentCustomsValue,
			CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
			Amount = line.AssessmentAmount.Select(x => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIPKBodyGoodsItemAssessmentAmount>(x)).ToArray(),
			SpecificRate = line.AssessmentSpecificRate.Select(x => PopulateAssessmentSpecificRate(x)).ToArray(),
			ContentInformation = line.AssessmentContentInformation.Select(x => PopulateAssessmentContentInformation(x)).ToArray()
		};

		VSCIPKBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate) => new VSCIPKBodyGoodsItemAssessmentSpecificRate
		{
			Type = rate.Type,
			Value = rate.Value.Round(2).Normalize()
		};

		VSCIPKBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation) => new VSCIPKBodyGoodsItemAssessmentContentInformation
		{
			Type = contentInformation.ContentType,
			DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
		};

		VSCIPKBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty) => new VSCIPKBodyGoodsItemExciseDuty
		{
			Code = exciseDuty.Code,
			DegreePercentage = exciseDuty.DegreePercentage,
			DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
			Value = exciseDuty.Value,
			ValueSpecified = !exciseDuty.Value.IsZero(),
			Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIPKBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
		};

		VSCIPKBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document) => new VSCIPKBodyGoodsItemDocument
		{
			Division = document.Division,
			Type = document.DocumentType,
			ReferenceNumber = document.ReferenceNumber,
			IssuingDate = document.IssuingDate.GetValueOrDefault(),
			IssuingDateSpecified = document.IssuingDate.HasValue,
			AtHandFlag = document.AtHandFlag,
			WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIPKBodyGoodsItemDocumentWriteOff>(document.WriteOff)
		};

		VSCIPKBodyGoodsItemBorderTransportMeans PopulateBorderTransportMeans(ISCIPEDLine line) => new VSCIPKBodyGoodsItemBorderTransportMeans
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

		bool RepresentativeHasEoriNumber => CachedValueHelper.GetValue(ref representativeHasEoriNumberCached, () => !Representative?.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> representativeHasEoriNumberCached;

		bool PrincipalHasEoriNumber => CachedValueHelper.GetValue(ref principalHasEoriNumberCached, () => !Principal?.Identification.EoriNumberIsEmpty() ?? false);
		CachedValue<bool> principalHasEoriNumberCached;
	}
}
