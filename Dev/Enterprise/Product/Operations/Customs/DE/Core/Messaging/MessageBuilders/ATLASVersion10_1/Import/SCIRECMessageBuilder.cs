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
	public sealed class SCIRECMessageBuilder : MessageBuilder<VSCIRJ>
	{
		public SCIRECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ISCIRECHeader)provider.Header;
		}

		readonly IImportMessageHeader provider;
		readonly ISCIRECHeader header;
		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;
		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;
		IInwardProcessing inwardProcessing => header.InwardProcessing;

		protected override VSCIRJ GetMessageCore() => new VSCIRJ
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = PopulateContactPerson(),
			BorderTransportMeans = PopulateBorderTransportMeans(),
			ArrivalTransportMeans = PopulateArrivalTransportMeans(),
			PreviousAdministrativeReferences = PopulatePreviousAdministrativeReferences(),
			SummaryDeclaration = summaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = customsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = inwardProcessing != null ? PopulateInwardProcessing() : null,
			Body = PopulateBody()
		};

		VSCIRJMetaData PopulateMetaData() => new VSCIRJMetaData
		{
			Preparation = new VSCIRJMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<VSCIRJMetaDataMessageGroup>(),
			MessageType = VSCIRJMetaDataMessageType.VSCIRJ,
			InterchangeSender = new VSCIRJMetaDataInterchangeSender
			{
				Identification = new VSCIRJMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new VSCIRJMetaDataInterchangeRecipient
			{
				Identification = new VSCIRJMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		VSCIRJHeader PopulateHeader() => new VSCIRJHeader
		{
			MessageVersion = "J.1.1",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new VSCIRJHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<VSCIRJHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<VSCIRJHeaderDeclarationType>()
			},
			LRN	 = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			LocalClearanceDate = header.LocalClearanceDate.GetValueOrDefault(),
			LocalClearanceDateSpecified = header.DeclarationType == "AAV" && header.LocalClearanceDate.HasValue,
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus.MapCodeToEnumWithDefault<VSCIRJHeaderCustomsGoodsStatus>(),
			CustomsAuthorisation = new VSCIRJHeaderCustomsAuthorisation
			{
				LocalClearanceProcedure = header.LocalClearanceProcedure,
				CurrentProcedure = header.ProcedureAuthorisation,
			},
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<VSCIRJHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		VSCIRJDeclarant PopulateDeclarant() => new VSCIRJDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new VSCIRJDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			},
		};

		VSCIRJRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new VSCIRJRepresentative
		{
			Identification = new VSCIRJRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		VSCIRJPrincipal PopulatePrincipal() => new VSCIRJPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new VSCIRJPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new VSCIRJPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		VSCIRJContactPerson PopulateContactPerson() => new VSCIRJContactPerson
		{
			MailAddress = ContactPerson?.MailAddress,
			Name = ContactPerson?.PersonName,
			PhoneNumber = ContactPerson?.PhoneNumber,
			Position = ContactPerson?.Position,
		};

		VSCIRJBorderTransportMeans PopulateBorderTransportMeans() => new VSCIRJBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength),
			Nationality = header.BorderTransportMeansNationality
		};

		VSCIRJArrivalTransportMeans PopulateArrivalTransportMeans() => new VSCIRJArrivalTransportMeans
		{
			Identity = header.ArrivalTransportMeansIdentity
		};

		VSCIRJPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new VSCIRJPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<VSCIRJPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new VSCIRJPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		VSCIRJSummaryDeclaration PopulateSummaryDeclaration() => new VSCIRJSummaryDeclaration
		{
			IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? VSCIRJSummaryDeclarationIdentificationIndicator.REG : VSCIRJSummaryDeclarationIdentificationIndicator.AWB,
			GoodsItem = summaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsitem(x)).ToArray()
		};

		VSCIRJSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsitem(ISummaryDeclarationGoodsItem goodsItem) => new VSCIRJSummaryDeclarationGoodsItem
		{
			Quantity = goodsItem.Quantity.ToString(),
			IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
			IdentificationByRegistration = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<VSCIRJSummaryDeclarationGoodsItemIdentificationByRegistration>(
				summaryDeclaration.IdentificationIndicator,
				goodsItem.IdentificationByRegistrationReferencedRegistrationNumber,
				(x) => x.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString())
		};

		VSCIRJSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
		{
			var summaryDeclarationIdentificationIndicatorIsAWB = summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			return new VSCIRJSummaryDeclarationGoodsItemIdentificationByKey
			{
				Kind = summaryDeclarationIdentificationIndicatorIsAWB ? VSCIRJSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : VSCIRJSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
				Number = goodsItem.IdentificationByKeyNumber,
				Custodian = new VSCIRJSummaryDeclarationGoodsItemIdentificationByKeyCustodian
				{
					Identification = new VSCIRJSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
				}
			};
		}

		VSCIRJCustomsWarehouse PopulateCustomsWarehouse() => new VSCIRJCustomsWarehouse
		{
			SequenceNumber = "1",
			GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = new VSCIRJCustomsWarehouseCustomsAuthorisation { WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier },
			LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
			GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
		};

		VSCIRJCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<VSCIRJCustomsWarehouseGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
				{
					x.SequenceNumber = counter.ToString();
					x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
					x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
					x.CommodityCode = goodsItem.CommodityCode;
					x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
					x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
					x.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
					x.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
				});
		}

		VSCIRJInwardProcessing PopulateInwardProcessing() => new VSCIRJInwardProcessing
		{
			SequenceNumber = "1",
			GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
			SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
			MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
			GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
		};

		VSCIRJInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new VSCIRJInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier };

		VSCIRJInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new VSCIRJInwardProcessingMonitoringCustomsOffice
		{
			Identification = new VSCIRJInwardProcessingMonitoringCustomsOfficeIdentification
			{
				ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
			}
		};

		VSCIRJInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<VSCIRJInwardProcessingGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
				x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
			});
		}

		VSCIRJBody PopulateBody() => new VSCIRJBody
		{
			Consignee = Consignee != null ? PopulateConsignee() : null,
			Containers = PopulateContainers(),
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		VSCIRJBodyConsignee PopulateConsignee() => new VSCIRJBodyConsignee
		{
			Identification = !ConsigneeHasEoriNumber ? null : new VSCIRJBodyConsigneeIdentification
			{
				ReferenceNumber = Consignee.Identification.EoriNumber,
				SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
			},
			Name = ConsigneeHasEoriNumber ? null : Consignee.Address.Name,
			Address = ConsigneeHasEoriNumber ? null : new VSCIRJBodyConsigneeAddress
			{
				City = Consignee.Address.City,
				Country = Consignee.Address.Country,
				District = Consignee.Address.District,
				Line = Consignee.Address.Address,
				Postcode = Consignee.Address.Postcode
			}
		};

		VSCIRJBodyContainers PopulateContainers() => new VSCIRJBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerFlag == "J" ? header.ContainerIdentificationNumbers.Select(c => new VSCIRJBodyContainersContainer { IdentificationNumber = c }).ToArray() : null,
		};

		VSCIRJBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new VSCIRJBodyForeignTradeStatistics
		{
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
		};

		VSCIRJBodyDocument PopulateDocument(IImportDocument d) => new VSCIRJBodyDocument
		{
			Division = VSCIRJBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault(),
		};

		VSCIRJBodyGoodsItem PopulateLine(ISCIRECLine line)
		{
			var result = new VSCIRJBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new VSCIRJBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				GoodsDescription = line.GoodsDescription,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = line.OriginCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				CommodityCode = new VSCIRJBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				InwardMovement = new VSCIRJBodyGoodsItemInwardMovement
				{
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJBodyGoodsItemInwardMovementAmount>(line.InwardMovementAmount)
				},
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = !line.RequestedPreferentialTreatment.IsNullOrEmpty() ? PopulatePreferentialTreatment(line) : null,
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
			if (!line.ForeignTradeStatisticsGrossMassMeasure.IsZero())
			{
				result.ForeignTradeStatistics = new VSCIRJBodyGoodsItemForeignTradeStatistics
				{
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure,
				};
			}
			return result;
		}

		VSCIRJBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure) => new VSCIRJBodyGoodsItemAdditionalProcedure { Code = additionalProcedure };

		VSCIRJBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode) => new VSCIRJBodyGoodsItemSupplementaryCodes { Code = supplementaryCode };

		VSCIRJBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			return new VSCIRJBodyGoodsItemPackage
			{
				Kind = package.Kind,
				Quantity = package?.Quantity.ToString(),
				MarksNumbers = package.MarksNumbers
			};
		}

		VSCIRJBodyGoodsItemAssessment PopulateAssessment(ISCIRECLine line)
		{
			return new VSCIRJBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				Amount = line.AssessmentAmount.Select(a => a.Quantity > 0 ? CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJBodyGoodsItemAssessmentAmount>(a) : null).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};
		}

		VSCIRJBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
		{
			return new VSCIRJBodyGoodsItemAssessmentSpecificRate
			{
				Type = rate.Type,
				Value = rate.Value.Round(2).Normalize()
			};
		}

		VSCIRJBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
		{
			return new VSCIRJBodyGoodsItemAssessmentContentInformation
			{
				Type = contentInformation.ContentType,
				DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
			};
		}

		VSCIRJBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new VSCIRJBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		VSCIRJBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(ISCIRECLine line) => new VSCIRJBodyGoodsItemPreferentialTreatment { RequestedPreferentialTreatment = line.RequestedPreferentialTreatment, };

		VSCIRJBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new VSCIRJBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<VSCIRJBodyGoodsItemDocumentWriteOff>(document.WriteOff)
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
