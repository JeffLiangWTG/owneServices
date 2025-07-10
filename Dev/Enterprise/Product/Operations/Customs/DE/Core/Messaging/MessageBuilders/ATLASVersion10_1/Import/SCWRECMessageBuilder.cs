using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class SCWRECMessageBuilder : MessageBuilder<LSCWRL>
	{
		public SCWRECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ISCWRECHeader)provider.Header;
		}

		readonly IImportMessageHeader provider;
		readonly ISCWRECHeader header;
		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;
		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;
		IInwardProcessing inwardProcessing => header.InwardProcessing;

		protected override LSCWRL GetMessageCore() => new LSCWRL
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

		LSCWRLMetaData PopulateMetaData() => new LSCWRLMetaData
		{
			Preparation = new LSCWRLMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<LSCWRLMetaDataMessageGroup>(),
			MessageType = LSCWRLMetaDataMessageType.LSCWRL,
			InterchangeSender = new LSCWRLMetaDataInterchangeSender
			{
				Identification = new LSCWRLMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new LSCWRLMetaDataInterchangeRecipient
			{
				Identification = new LSCWRLMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		LSCWRLHeader PopulateHeader() => new LSCWRLHeader
		{
			MessageVersion = "L.1.0",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new LSCWRLHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<LSCWRLHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<LSCWRLHeaderDeclarationType>()
			},
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			LocalClearanceDate = header.LocalClearanceDate.GetValueOrDefault(),
			LocalClearanceDateSpecified = header.LocalClearanceDate.HasValue,
			ForeignTradeImportEarlyClearanceFlag = header.ForeignTradeImportEarlyClearanceFlag,
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus.MapCodeToEnumWithDefault<LSCWRLHeaderCustomsGoodsStatus>(),
			CustomsAuthorisation = PopulateHeaderCustomsAuthorisation(),
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<LSCWRLHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		LSCWRLHeaderCustomsAuthorisation PopulateHeaderCustomsAuthorisation()
		{
			return new LSCWRLHeaderCustomsAuthorisation
			{
				LocalClearanceProcedure = header.LocalClearanceProcedure,
				CurrentProcedure = header.CurrentProcedure
			};
		}

		LSCWRLDeclarant PopulateDeclarant() => new LSCWRLDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new LSCWRLDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			}
		};

		LSCWRLRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new LSCWRLRepresentative
		{
			Identification = new LSCWRLRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		LSCWRLPrincipal PopulatePrincipal() => new LSCWRLPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new LSCWRLPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new LSCWRLPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		LSCWRLContactPerson PopulateContactPerson() => new LSCWRLContactPerson
		{
			MailAddress = ContactPerson.MailAddress,
			Name = ContactPerson.PersonName,
			PhoneNumber = ContactPerson.PhoneNumber,
			Position = ContactPerson.Position
		};

		LSCWRLBorderTransportMeans PopulateBorderTransportMeans() => new LSCWRLBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength)
		};

		LSCWRLPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new LSCWRLPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<LSCWRLPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new LSCWRLPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		LSCWRLSummaryDeclaration PopulateSummaryDeclaration() => new LSCWRLSummaryDeclaration
		{
			IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? LSCWRLSummaryDeclarationIdentificationIndicator.REG : LSCWRLSummaryDeclarationIdentificationIndicator.AWB,
			GoodsItem = summaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsitem(x)).ToArray(),
		};

		LSCWRLSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsitem(ISummaryDeclarationGoodsItem goodsItem) => new LSCWRLSummaryDeclarationGoodsItem
		{
			Quantity = goodsItem.Quantity.ToString(),
			IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
			IdentificationByRegistration = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWRLSummaryDeclarationGoodsItemIdentificationByRegistration>(
				summaryDeclaration.IdentificationIndicator,
				goodsItem.IdentificationByRegistrationReferencedRegistrationNumber,
				(x) => x.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString())
		};

		LSCWRLSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
		{
			var summaryDeclarationIdentificationIndicatorIsAWB = summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			return new LSCWRLSummaryDeclarationGoodsItemIdentificationByKey
			{
				Kind = summaryDeclarationIdentificationIndicatorIsAWB ? LSCWRLSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : LSCWRLSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
				Number = goodsItem.IdentificationByKeyNumber,
				Custodian = new LSCWRLSummaryDeclarationGoodsItemIdentificationByKeyCustodian
				{
					Identification = new LSCWRLSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
				}
			};
		}

		LSCWRLCustomsWarehouse PopulateCustomsWarehouse() => new LSCWRLCustomsWarehouse
		{
			SequenceNumber = "1",
			GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = new LSCWRLCustomsWarehouseCustomsAuthorisation { WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier },
			LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
			GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
		};

		LSCWRLCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWRLCustomsWarehouseGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
				x.CommodityCode = goodsItem.CommodityCode;
				x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
				x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
				x.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
				x.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
			});
		}

		LSCWRLInwardProcessing PopulateInwardProcessing() => new LSCWRLInwardProcessing
		{
			SequenceNumber = "1",
			GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
			SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
			MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
			GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
		};

		LSCWRLInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new LSCWRLInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier };

		LSCWRLInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new LSCWRLInwardProcessingMonitoringCustomsOffice
		{
			Identification = new LSCWRLInwardProcessingMonitoringCustomsOfficeIdentification
			{
				ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
			}
		};

		LSCWRLInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LSCWRLInwardProcessingGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
				x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
			});
		}

		LSCWRLBody PopulateBody() => new LSCWRLBody
		{
			Consignee = Consignee != null ? PopulateConsignee() : null,
			Containers = PopulateContainers(),
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		LSCWRLBodyConsignee PopulateConsignee() => new LSCWRLBodyConsignee
		{
			Identification = !ConsigneeHasEoriNumber ? null : new LSCWRLBodyConsigneeIdentification
			{
				ReferenceNumber = Consignee.Identification.EoriNumber,
				SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
			},
			Name = ConsigneeHasEoriNumber ? null : Consignee.Address.Name,
			Address = ConsigneeHasEoriNumber ? null : new LSCWRLBodyConsigneeAddress
			{
				City = Consignee.Address.City,
				Country = Consignee.Address.Country,
				District = Consignee.Address.District,
				Line = Consignee.Address.Address,
				Postcode = Consignee.Address.Postcode
			}
		};

		LSCWRLBodyContainers PopulateContainers() => new LSCWRLBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerIdentificationNumbers.Select(c => new LSCWRLBodyContainersContainer { IdentificationNumber = c }).ToArray(),
		};

		LSCWRLBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new LSCWRLBodyForeignTradeStatistics
		{
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
		};

		LSCWRLBodyDocument PopulateDocument(IImportDocument d) => new LSCWRLBodyDocument
		{
			Division = LSCWRLBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault(),
		};

		LSCWRLBodyGoodsItem PopulateLine(ISCWRECLine line)
		{
			var result = new LSCWRLBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new LSCWRLBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				GoodsDescription = line.GoodsDescription,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = line.OriginCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CommodityCode = new LSCWRLBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				InwardMovement = new LSCWRLBodyGoodsItemInwardMovement
				{
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLBodyGoodsItemInwardMovementAmount>(line.InwardMovementAmount),
				},
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = new LSCWRLBodyGoodsItemPreferentialTreatment
				{
					RequestedPreferentialTreatment = line.RequestedPreferentialTreatment
				},
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
			if (!line.ForeignTradeStatisticsGrossMassMeasure.IsZero())
			{
				result.ForeignTradeStatistics = new LSCWRLBodyGoodsItemForeignTradeStatistics
				{
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
				};
			}
			return result;
		}

		LSCWRLBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure)
		{
			return new LSCWRLBodyGoodsItemAdditionalProcedure
			{
				Code = additionalProcedure
			};
		}

		LSCWRLBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode)
		{
			return new LSCWRLBodyGoodsItemSupplementaryCodes
			{
				Code = supplementaryCode
			};
		}

		LSCWRLBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			var result = new LSCWRLBodyGoodsItemPackage { Kind = package.Kind };
			if (package.Quantity.HasValue)
			{
				result.Quantity = package.Quantity.ToString();
				result.MarksNumbers = package.MarksNumbers;
			}
			return result;
		}

		LSCWRLBodyGoodsItemAssessment PopulateAssessment(ISCWRECLine line)
		{
			return new LSCWRLBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				Amount = line.AssessmentAmount.Select(a => CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLBodyGoodsItemAssessmentAmount>(a)).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};
		}

		LSCWRLBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
		{
			return new LSCWRLBodyGoodsItemAssessmentSpecificRate
			{
				Type = rate.Type,
				Value = rate.Value.Round(2).Normalize()
			};
		}

		LSCWRLBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
		{
			return new LSCWRLBodyGoodsItemAssessmentContentInformation
			{
				Type = contentInformation.ContentType,
				DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
			};
		}

		LSCWRLBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new LSCWRLBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		LSCWRLBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new LSCWRLBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LSCWRLBodyGoodsItemDocumentWriteOff>(document.WriteOff)
			};
		}

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

		bool SummaryDeclarationIdentificationIndicatorIsREG => CachedValueHelper.GetValue(ref summaryDeclarationIdentificationIndicatorIsREGCached, () => summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG);
		CachedValue<bool> summaryDeclarationIdentificationIndicatorIsREGCached;
	}
}
