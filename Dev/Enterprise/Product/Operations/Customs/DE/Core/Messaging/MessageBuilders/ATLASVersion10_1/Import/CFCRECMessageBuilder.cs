using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;
using MessageBuilderExtensions = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CFCRECMessageBuilder : MessageBuilder<FCFCRF>
	{
		public CFCRECMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ICFCRECHeader)provider.Header;
		}
		readonly IImportMessageHeader provider;
		readonly ICFCRECHeader header;

		protected override FCFCRF GetMessageCore() => new FCFCRF
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			Declarant = Declarant != null ? PopulateDeclarant() : null,
			Representative = Representative != null ? PopulateRepresentative() : null,
			Principal = Principal != null ? PopulatePrincipal() : null,
			ContactPerson = ContactPerson != null ?  PopulateContactPerson() : null,
			BorderTransportMeans = PopulateBorderTransportMeans(),
			ArrivalTransportMeans = PopulateArrivalTransportMeans(),
			PreviousAdministrativeReferences = PopulatePreviousAdministrativeReferences(),
			SummaryDeclaration = summaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = customsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = inwardProcessing != null ? PopulateInwardProcessing() : null,
			Body = PopulateBody()
		};

		FCFCRFMetaData PopulateMetaData() => new FCFCRFMetaData
		{
			Preparation = new FCFCRFMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<FCFCRFMetaDataMessageGroup>(),
			MessageType = FCFCRFMetaDataMessageType.FCFCRF,
			InterchangeSender = new FCFCRFMetaDataInterchangeSender
			{
				Identification = new FCFCRFMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new FCFCRFMetaDataInterchangeRecipient
			{
				Identification = new FCFCRFMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		FCFCRFHeader PopulateHeader() => new FCFCRFHeader
		{
			MessageVersion = "F.1.2",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			Declaration = new FCFCRFHeaderDeclaration
			{
				Kind = header.DeclarationKind.MapCodeToEnumWithDefault<FCFCRFHeaderDeclarationKind>(),
				Type = header.DeclarationType.MapCodeToEnumWithDefault<FCFCRFHeaderDeclarationType>()
			},
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			LocalClearanceDate = header.LocalClearanceDate.GetValueOrDefault(),
			LocalClearanceDateSpecified = header.LocalClearanceDate.HasValue && header.DeclarationType == "AZ",
			PrematureInputFlag = header.PrematureInputFlag.MapBoolToJN(),
			GoodsItemQuantity = header.GoodsItemQuantity.ToString(),
			CustomsGoodsStatus = header.CustomsGoodsStatus,
			CustomsAuthorisation = new FCFCRFHeaderCustomsAuthorisation
			{
				LocalClearanceProcedure = header.LocalClearanceProcedure,
				EndUse = header.ProcedureAuthorisation
			},
			GoodsLocation = header.GoodsLocation,
			DepartureCountry = header.DepartureCountry,
			CurrencyCode = header.CurrencyCode.MapCodeToEnumWithDefault<FCFCRFHeaderCurrencyCode>(),
			AdditionalInformation = header.AdditionalInformation,
			TaxOffice = header.TaxOffice,
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber
		};

		FCFCRFDeclarant PopulateDeclarant() => new FCFCRFDeclarant
		{
			Identification = !DeclarantHasEoriNumber ? null : new FCFCRFDeclarantIdentification
			{
				ReferenceNumber = Declarant.Identification.EoriNumber,
				SubsidiaryNumber = Declarant.Identification.EoriBranchSuffix
			},
			Name = DeclarantHasEoriNumber ? null : Declarant.Address.Name,
			Address = DeclarantHasEoriNumber ? null : new FCFCRFDeclarantAddress
			{
				City = Declarant.Address.City,
				Country = Declarant.Address.Country,
				District = Declarant.Address.District,
				Line = Declarant.Address.Address,
				Postcode = Declarant.Address.Postcode
			}
		};

		FCFCRFRepresentative PopulateRepresentative() => !RepresentativeHasEoriNumber ? null : new FCFCRFRepresentative
		{
			Identification = new FCFCRFRepresentativeIdentification
			{
				ReferenceNumber = Representative.Identification.EoriNumber,
				SubsidiaryNumber = Representative.Identification.EoriBranchSuffix
			}
		};

		FCFCRFPrincipal PopulatePrincipal() => new FCFCRFPrincipal
		{
			Identification = !PrincipalHasEoriNumber ? null : new FCFCRFPrincipalIdentification
			{
				ReferenceNumber = Principal.Identification.EoriNumber,
				SubsidiaryNumber = Principal.Identification.EoriBranchSuffix
			},
			Name = PrincipalHasEoriNumber ? null : Principal.Address.Name,
			Address = PrincipalHasEoriNumber ? null : new FCFCRFPrincipalAddress
			{
				City = Principal.Address.City,
				Country = Principal.Address.Country,
				District = Principal.Address.District,
				Line = Principal.Address.Address,
				Postcode = Principal.Address.Postcode
			}
		};

		FCFCRFContactPerson PopulateContactPerson() => new FCFCRFContactPerson
		{
			MailAddress = ContactPerson?.MailAddress,
			Name = ContactPerson?.PersonName,
			PhoneNumber = ContactPerson?.PhoneNumber,
			Position = ContactPerson?.Position,
		};

		FCFCRFBorderTransportMeans PopulateBorderTransportMeans() => new FCFCRFBorderTransportMeans
		{
			Mode = header.BorderTransportMeansMode,
			Type = header.BorderTransportMeansType,
			Information = header.BorderTransportMeansInformation.LeftOrNull(BorderTransportMeansInformationMaxLength),
			Nationality = header.BorderTransportMeansNationality
		};

		FCFCRFArrivalTransportMeans PopulateArrivalTransportMeans() => new FCFCRFArrivalTransportMeans
		{
			Identity = header.ArrivalTransportMeansIdentity
		};

		FCFCRFPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new FCFCRFPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.MapCodeToEnumWithDefaultAndOptionalItemPrefix<FCFCRFPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodes()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new FCFCRFPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		FCFCRFSummaryDeclaration PopulateSummaryDeclaration()
		{
			return new FCFCRFSummaryDeclaration
			{
				IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? FCFCRFSummaryDeclarationIdentificationIndicator.REG : FCFCRFSummaryDeclarationIdentificationIndicator.AWB,
				GoodsItem = summaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsItem(x)).ToArray()
			};

			FCFCRFSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsItem(ISummaryDeclarationGoodsItem goodsItem) => new FCFCRFSummaryDeclarationGoodsItem
			{
				Quantity = goodsItem.Quantity.ToString(),
				IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
				IdentificationByRegistration = MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCRFSummaryDeclarationGoodsItemIdentificationByRegistration>(
					summaryDeclaration.IdentificationIndicator,
					goodsItem.IdentificationByRegistrationReferencedRegistrationNumber,
					x =>
					{
						x.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString();
					})
			};

			FCFCRFSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem)
			{
				var summaryDeclarationIdentificationIndicatorIsAWB = summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				return new FCFCRFSummaryDeclarationGoodsItemIdentificationByKey
				{
					Kind = summaryDeclarationIdentificationIndicatorIsAWB ? FCFCRFSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : FCFCRFSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
					Number = goodsItem.IdentificationByKeyNumber,
					Custodian = new FCFCRFSummaryDeclarationGoodsItemIdentificationByKeyCustodian
					{
						Identification = new FCFCRFSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier }
					}
				};
			}
		}

		FCFCRFCustomsWarehouse PopulateCustomsWarehouse()
		{
			return new FCFCRFCustomsWarehouse
			{
				SequenceNumber = "1",
				GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = new FCFCRFCustomsWarehouseCustomsAuthorisation { WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier },
				LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
				GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray()
			};

			FCFCRFCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter) =>
				MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCRFCustomsWarehouseGoodsItem>(
					goodsItem.ReferencedRegistrationNumber,
					x =>
					{
						x.SequenceNumber = counter.ToString();
						x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
						x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
						x.CommodityCode = goodsItem.CommodityCode;
						x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
						x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
						x.CommercialAmount = MessageBuilderExtensions.CreateCommonAmount<FCFCRFCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
						x.DebitAmount = MessageBuilderExtensions.CreateCommonAmount<FCFCRFCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
					});
		}

		FCFCRFInwardProcessing PopulateInwardProcessing()
		{
			return new FCFCRFInwardProcessing
			{
				SequenceNumber = "1",
				GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
				CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
				SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
				MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
				GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray()
			};

			FCFCRFInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new FCFCRFInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier };

			FCFCRFInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new FCFCRFInwardProcessingMonitoringCustomsOffice
			{
				Identification = new FCFCRFInwardProcessingMonitoringCustomsOfficeIdentification
				{
					ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
				}
			};

			FCFCRFInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter) =>
				MessageBuilderExtensions.CreateCommonIdentificationByRegistration<FCFCRFInwardProcessingGoodsItem>(
					goodsItem.ReferencedRegistrationNumber,
					x =>
					{
						x.SequenceNumber = counter.ToString();
						x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
						x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
						x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
					});
		}

		FCFCRFBody PopulateBody() => new FCFCRFBody
		{
			Consignee = Consignee != null ? PopulateConsignee() : null,
			AdditionalDutyReferences = !header.AdditionalDutyReferences.IsNullOrEmpty() ? PopulateAdditionalDutyReferences() : null,
			Containers = PopulateContainers(),
			ForeignTradeStatistics = PopulateForeignTradeStatistics(),
			Document = header.Documents.Select(d => PopulateDocument(d)).ToArray(),
			GoodsItem = header.Lines.Select(line => PopulateLine(line)).ToArray()
		};

		FCFCRFBodyConsignee PopulateConsignee() => new FCFCRFBodyConsignee
		{
			Identification = !ConsigneeHasEoriNumber ? null : new FCFCRFBodyConsigneeIdentification
			{
				ReferenceNumber = Consignee.Identification.EoriNumber,
				SubsidiaryNumber = Consignee.Identification.EoriBranchSuffix
			},
			Name = ConsigneeHasEoriNumber ? null : Consignee.Address.Name,
			Address = ConsigneeHasEoriNumber ? null : new FCFCRFBodyConsigneeAddress
			{
				City = Consignee.Address.City,
				Country = Consignee.Address.Country,
				District = Consignee.Address.District,
				Line = Consignee.Address.Address,
				Postcode = Consignee.Address.Postcode
			}
		};

		FCFCRFBodyAdditionalDutyReferences[] PopulateAdditionalDutyReferences()
		{
			return header.AdditionalDutyReferences.Select(duty => new FCFCRFBodyAdditionalDutyReferences()
			{
				ReferenceNumber = duty.ReferenceNumber,
				DutyInterestedParty = duty.DutyInterestedParty != null ? PopulateDutyInterestedParty(duty.DutyInterestedParty) : null
			}).ToArray();
		}

		FCFCRFBodyAdditionalDutyReferencesDutyInterestedParty PopulateDutyInterestedParty(IImportParty dutyInterestedParty)
		{
			var dutyInterestedPartyHasEoriNumber = !dutyInterestedParty.Identification.EoriNumberIsEmpty();
			return new FCFCRFBodyAdditionalDutyReferencesDutyInterestedParty()
			{
				Identification = !dutyInterestedPartyHasEoriNumber ? null : new FCFCRFBodyAdditionalDutyReferencesDutyInterestedPartyIdentification()
				{
					ReferenceNumber = dutyInterestedParty.Identification.EoriNumber
				},
				Name = dutyInterestedPartyHasEoriNumber ? null : dutyInterestedParty.Address.Name,
				Address = dutyInterestedPartyHasEoriNumber ? null : new FCFCRFBodyAdditionalDutyReferencesDutyInterestedPartyAddress
				{
					City = dutyInterestedParty.Address.City,
					Country = dutyInterestedParty.Address.Country,
					District = dutyInterestedParty.Address.District,
					Line = dutyInterestedParty.Address.Address,
					Postcode = dutyInterestedParty.Address.Postcode
				}
			};
		}

		FCFCRFBodyContainers PopulateContainers() => new FCFCRFBodyContainers
		{
			ContainerFlag = header.ContainerFlag,
			Container = header.ContainerIdentificationNumbers.Select(c => new FCFCRFBodyContainersContainer { IdentificationNumber = c }).ToArray(),
		};

		FCFCRFBodyForeignTradeStatistics PopulateForeignTradeStatistics() => new FCFCRFBodyForeignTradeStatistics
		{
			InlandTransportMode = header.ForeignTradeStatisticsInlandTransportMode,
			TotalGrossMassMeasure = header.ForeignTradeStatisticsTotalGrossMassMeasure,
			TotalGrossMassMeasureSpecified = !header.ForeignTradeStatisticsTotalGrossMassMeasure.IsZero(),
		};

		FCFCRFBodyDocument PopulateDocument(IImportDocument d) => new FCFCRFBodyDocument
		{
			Division = FCFCRFBodyDocumentDivision.Item4,
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault()
		};

		FCFCRFBodyGoodsItem PopulateLine(ICFCRECLine line)
		{
			var originCountry = line.OriginCountry;
			var preferentialOriginCountry = line.PreferentialOriginCountry;
			return new FCFCRFBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new FCFCRFBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = line.RequestedPreviousProcedure
				},
				CessionManagementFlag = line.CessionManagementFlag,
				GoodsDescription = line.GoodsDescription,
				NetMassMeasure = line.NetMassMeasure.Round(1).Normalize(),
				OriginCountry = originCountry != preferentialOriginCountry || int.TryParse(line.PreferentialTreatment.RequestedPreferentialTreatment, out var requestedPreferentialTreatmentAsInteger) && requestedPreferentialTreatmentAsInteger < 200
					? originCountry.ValueOrNullIfEmpty() : null,
				PreferentialOriginCountry = preferentialOriginCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				TobaccoRevenueStampNumber = line.TobaccoRevenueStampNumber,
				CommodityCode = new FCFCRFBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateSupplementaryCode(p)).ToArray(),
				Package = line.Package != null ? PopulatePackage(line.Package) : null,
				ForeignTradeStatistics = line.ForeignTradeStatisticsGrossMassMeasure.IsZero() ? null : new FCFCRFBodyGoodsItemForeignTradeStatistics
				{
					GrossMassMeasure = line.ForeignTradeStatisticsGrossMassMeasure
				},
				Assessment = PopulateAssessment(line),
				ExciseDuty = line.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray(),
				PreferentialTreatment = line.PreferentialTreatment != null ? PopulatePreferentialTreatment(line) : null,
				Document = line.Documents.Select(d => PopulateLineDocuments(d)).ToArray()
			};
		}

		FCFCRFBodyGoodsItemAdditionalProcedure PopulateAdditionalProcedure(string additionalProcedure)
		{
			return new FCFCRFBodyGoodsItemAdditionalProcedure
			{
				Code = additionalProcedure
			};
		}

		FCFCRFBodyGoodsItemSupplementaryCodes PopulateSupplementaryCode(string supplementaryCode)
		{
			return new FCFCRFBodyGoodsItemSupplementaryCodes
			{
				Code = supplementaryCode.LeftOrNull(SupplementaryCodeMaxLength)
			};
		}

		FCFCRFBodyGoodsItemPackage PopulatePackage(IImportPackage package)
		{
			var result = new FCFCRFBodyGoodsItemPackage { Kind = package.Kind };
			if (package.Quantity.HasValue)
			{
				result.Quantity = package.Quantity.ToString();
				result.MarksNumbers = package.MarksNumbers;
			}
			return result;
		}

		FCFCRFBodyGoodsItemAssessment PopulateAssessment(ICFCRECLine line)
		{
			return new FCFCRFBodyGoodsItemAssessment
			{
				CustomsValue = line.AssessmentCustomsValue,
				CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
				Amount = line.AssessmentAmount.Select(a => MessageBuilderExtensions.CreateCommonAmount<FCFCRFBodyGoodsItemAssessmentAmount>(a)).ToArray(),
				SpecificRate = line.AssessmentSpecificRate.Select(r => PopulateAssessmentSpecificRate(r)).ToArray(),
				ContentInformation = line.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};

			FCFCRFBodyGoodsItemAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
			{
				return new FCFCRFBodyGoodsItemAssessmentSpecificRate
				{
					Type = rate.Type,
					Value = rate.Value.Round(2).Normalize()
				};
			}

			FCFCRFBodyGoodsItemAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
			{
				return new FCFCRFBodyGoodsItemAssessmentContentInformation
				{
					Type = contentInformation.ContentType,
					DegreePercentage = contentInformation.DegreePercentage.Round(2).Normalize()
				};
			}
		}

		FCFCRFBodyGoodsItemExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
		{
			return new FCFCRFBodyGoodsItemExciseDuty
			{
				Code = exciseDuty.Code,
				DegreePercentage = exciseDuty.DegreePercentage,
				DegreePercentageSpecified = !exciseDuty.DegreePercentage.IsZero(),
				Value = exciseDuty.Value,
				ValueSpecified = !exciseDuty.Value.IsZero(),
				Amount = MessageBuilderExtensions.CreateCommonAmount<FCFCRFBodyGoodsItemExciseDutyAmount>(exciseDuty.Amount)
			};
		}

		FCFCRFBodyGoodsItemPreferentialTreatmentDeclarationContingent PopulateContingentNumber(string contingentNumber)
		{
			return new FCFCRFBodyGoodsItemPreferentialTreatmentDeclarationContingent
			{
				ContingentNumber = contingentNumber.LeftOrNull(ContingentNumberMaxLength)
			};
		}

		FCFCRFBodyGoodsItemPreferentialTreatment PopulatePreferentialTreatment(ICFCRECLine line)
		{
			return new FCFCRFBodyGoodsItemPreferentialTreatment
			{
				RequestedPreferentialTreatment = line.PreferentialTreatment.RequestedPreferentialTreatment,
				Declaration = new FCFCRFBodyGoodsItemPreferentialTreatmentDeclaration
				{
					Contingent = line.PreferentialTreatment.ContingentNumber.Select(cn => PopulateContingentNumber(cn)).ToArray(),
					PreferentialTreatmentQuantity = line.PreferentialTreatment.Quantity != null ? PopulatePreferentialTreatmentQuantity(line.PreferentialTreatment.Quantity) : null,
				}
			};
		}

		FCFCRFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity PopulatePreferentialTreatmentQuantity(IAmount amount)
		{
			return new FCFCRFBodyGoodsItemPreferentialTreatmentDeclarationPreferentialTreatmentQuantity
			{
				Quantity = amount.Quantity.RoundAndNormalize(0).ToString(),
				MeasurementUnit = amount.MeasurementUnit,
				Qualifier = amount.Qualifier
			};
		}

		FCFCRFBodyGoodsItemDocument PopulateLineDocuments(IImportLineDocument document)
		{
			return new FCFCRFBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				AtHandFlag = document.AtHandFlag,
				WriteOff = MessageBuilderExtensions.CreateCommonAmount<FCFCRFBodyGoodsItemDocumentWriteOff>(document.WriteOff)
			};
		}

		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;

		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;

		IInwardProcessing inwardProcessing => header.InwardProcessing;

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
