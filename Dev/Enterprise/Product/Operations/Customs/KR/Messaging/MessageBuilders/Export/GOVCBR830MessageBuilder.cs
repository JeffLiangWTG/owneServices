using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR830;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	public class GOVCBR830MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IExportEntryHeader dataProvider;
		public GOVCBR830MessageBuilder(IExportEntryHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				FunctionCode = PopulateFunctionCode(),
				GoodsItemQuantity = PopulateGoodsItemQuantity(),
				Id = PopulateID(),
				InvoiceAmount = PopulateInvoiceAmount(),
				IssueDateTime = PopulateIssueDateTime(),
				TotalGrossMassMeasure = PopulateTotalGrossMassMeasure(),
				TotalPackageQuantity = PopulateTotalPackageQuantity(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				ResponseTypeCode = PopulateResponseTypeCode(),
				AdditionalCode = PopulateAdditionalCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = PopulateAgent(),
				BorderTransportMeans = PopulateBorderTransportMeans(),
				Carrier = PopulateCarrier(),
				CurrencyExchange = PopulateCurrencyExchange(),
				CustomsProcedure = PopulateCustomsProcedure(),
				Consignment = PopulateConsignment(),
				Control = PopulateControl(),
				Exporter = PopulateExporter(),
				FinalTransportMeansLoadingPlace = PopulateFinalTransportMeansLoadingPlace(),
				GoodsShipment = PopulateGoodsShipment(),
				Manufacturer = PopulateManufacturer(),
				LoadingLocation = PopulateLoadingLocation(),
				Packaging = PopulatePackaging(),
				PreviousDocument = PopulatePreviousDocument(),
				SouthNorthTrade = PopulateSouthNorthTrade(),
				Submitter = PopulateSubmitter(),
				Ucr = PopulateUCR()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return string.IsNullOrEmpty(dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision) ? null : new DeclarationDeclarationOfficeIdType
			{
				Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision
			};
		}

		DeclarationFunctionCodeType PopulateFunctionCode()
		{
			return new DeclarationFunctionCodeType { Value = FunctionCode.Original };
		}

		DeclarationGoodsItemQuantityType PopulateGoodsItemQuantity()
		{
			return new DeclarationGoodsItemQuantityType { Value = dataProvider.EntryLines.Count() };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ExportDeclarationNumber };
		}

		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType { Value = dataProvider.TotalCustomsValue };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTotalGrossMassMeasureType PopulateTotalGrossMassMeasure()
		{
			return new DeclarationTotalGrossMassMeasureType { Value = dataProvider.TotalGrossWeightInKG, KcsUnitCode = DefaultWeightUnit };
		}

		DeclarationTotalPackageQuantityType PopulateTotalPackageQuantity()
		{
			return new DeclarationTotalPackageQuantityType { Value = dataProvider.TotalPackQty };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._830 };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.TransactionType };
		}

		DeclarationResponseTypeCodeType PopulateResponseTypeCode()
		{
			return new DeclarationResponseTypeCodeType { Value = ResponseTypeCode.Required };
		}

		DeclarationAdditionalCode PopulateAdditionalCode()
		{
			var returnReasonCode = dataProvider.ReturnReason.IsEmpty ? null : new AdditionalCodeReturnReasonCodeType { Value = dataProvider.ReturnReason };
			var returnScopeCode = dataProvider.ReturnType.IsEmpty ? null : new AdditionalCodeReturnScopeCodeType { Value = dataProvider.ReturnType };

			if ((returnReasonCode ?? (object)returnScopeCode) == null)
			{
				return null;
			}

			return new DeclarationAdditionalCode
			{
				ReturnReasonCode = returnReasonCode,
				ReturnScopeCode = returnScopeCode
			};
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			var statementCode = dataProvider.OutOfHoursDeclarationIndicator.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = dataProvider.OutOfHoursDeclarationIndicator };
			var statementDescription = dataProvider.DeclarantAdditionalDescription.IsEmpty ? null : new AdditionalInformationStatementDescriptionTextType { Value = dataProvider.DeclarantAdditionalDescription };
			var periodDateTime = PopulatePeriodDateTime();

			if ((statementCode ?? statementDescription ?? (object)periodDateTime) == null)
			{
				return null;
			}

			return new DeclarationAdditionalInformation
			{
				StatementCode = statementCode,
				StatementDescription = statementDescription,
				PeriodDateTime = periodDateTime
			};
		}

		string PopulatePeriodDateTime()
		{
			if (dataProvider.BondedTransportationFromDate.IsValid && dataProvider.BondedTransportationToDate.IsValid)
			{
				return dataProvider.BondedTransportationFromDate.ToString(DateFormatType.Date) + dataProvider.BondedTransportationToDate.ToString(DateFormatType.Date);
			}
			return null;
		}

		DeclarationAgent PopulateAgent()
		{
			var unipassId = dataProvider.Exporter?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var officeID = dataProvider.Exporter?.GetRegistrationNumber(IdentificationType.OfficeID);
			var agent = new DeclarationAgent
			{
				Id = null,
				Name = new AgentNameTextType { Value = dataProvider.Exporter?.CompanyName ?? ZString.Empty }
			};

			if (!string.IsNullOrEmpty(unipassId))
			{
				agent.Id = new Collection<AgentIdentificationIdType>
				{
					new AgentIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = unipassId
					}
				};
			}

			if (!string.IsNullOrEmpty(officeID))
			{
				if (agent.Id == null)
				{
					agent.Id = new Collection<AgentIdentificationIdType>();
				}
				agent.Id.Add(new AgentIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Kts,
					Value = officeID
				});
			}

			return agent;
		}

		DeclarationBorderTransportMeans PopulateBorderTransportMeans()
		{
			if (dataProvider.VesselNameOrFlightNo.IsEmpty && !dataProvider.DepartureDate.IsValid && !dataProvider.Containers.Any())
			{
				return null;
			}

			return new DeclarationBorderTransportMeans
			{
				Name = !dataProvider.VesselNameOrFlightNo.IsEmpty ? new BorderTransportMeansNameTextType { Value = dataProvider.VesselNameOrFlightNo } : null,
				EstimatedDepartureDateTime = !dataProvider.DepartureDate.IsValid ? string.Empty : dataProvider.DepartureDate.ToString(DateFormatType.Date),
				TransportEquipment = PopulateTransportEquipment()
			};
		}

		Collection<DeclarationBorderTransportMeansTransportEquipment> PopulateTransportEquipment()
		{
			if (!dataProvider.Containers.Any())
			{
				return null;
			}

			var containers = new Collection<DeclarationBorderTransportMeansTransportEquipment>();
			foreach (var container in dataProvider.Containers)
			{
				var item = new DeclarationBorderTransportMeansTransportEquipment
				{
					SequenceNumeric = new TransportEquipmentSequenceTextType { Value = container.SequenceNo },
					Id = container.ContainerNo.IsEmpty ? null : new TransportEquipmentIdentificationIdType { Value = container.ContainerNo }
				};
				containers.Add(item);
			}
			return containers;
		}

		DeclarationCarrier PopulateCarrier()
		{
			if (dataProvider.CarrierID.IsEmpty && dataProvider.ShippingLineOrAirlineName.IsEmpty && dataProvider.FreightForwarderContactName.IsEmpty)
			{
				return null;
			}

			return new DeclarationCarrier
			{
				Id = dataProvider.CarrierID.IsEmpty ? null : new CarrierIdentificationIdType { Value = dataProvider.CarrierID },
				Name = dataProvider.ShippingLineOrAirlineName.IsEmpty ? null : new CarrierNameTextType { Value = dataProvider.ShippingLineOrAirlineName },
				Contact = dataProvider.FreightForwarderContactName.IsEmpty ? null : new DeclarationCarrierContact
				{
					Name = new ContactNameTextType { Value = dataProvider.FreightForwarderContactName }
				}
			};
		}

		DeclarationCurrencyExchange PopulateCurrencyExchange()
		{
			return dataProvider.ExchangeRate == 0m ? null : new DeclarationCurrencyExchange { RateNumeric = dataProvider.ExchangeRate };
		}

		DeclarationCustomsProcedure PopulateCustomsProcedure()
		{
			return new DeclarationCustomsProcedure
			{
				ProcessTypeCode = new CustomsProcedureProcessTypeCodeType { Value = dataProvider.DeclarationProcedureType },
				TypeCode = new CustomsProcedureTypeCodeType { Value = dataProvider.ExportTypeCode }
			};
		}

		DeclarationConsignment PopulateConsignment()
		{
			return new DeclarationConsignment
			{
				ConsignmentItem = new DeclarationConsignmentConsignmentItem
				{
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						ValueAmount = new CommodityValueAmountType
						{
							Value = dataProvider.TotalInvoiceAmount,
							CurrencyId = Enum.TryParse(dataProvider.Currency, true, out Iso3AlphaCurrencyCodeContentType result) ? result : new Iso3AlphaCurrencyCodeContentType?(),
						}
					},
					PreviousDocument = dataProvider.LocationIDInBondedArea.IsEmpty ? null : new DeclarationConsignmentConsignmentItemPreviousDocument
					{
						Id = new PreviousDocumentIdentificationIdType { Value = dataProvider.LocationIDInBondedArea }
					}
				},
				GoodsLocation = dataProvider.GoodsLocationBondedAreaCode.IsEmpty ? null : new DeclarationConsignmentGoodsLocation
				{
					Id = new GoodsLocationIdentificationIdType { Value = dataProvider.GoodsLocationBondedAreaCode }
				}
			};
		}

		DeclarationControl PopulateControl()
		{
			return new DeclarationControl
			{
				InspectionStartDateTime = !dataProvider.PreferredInspectionDate.IsValid ? string.Empty : dataProvider.PreferredInspectionDate.ToString(DateFormatType.Date)
			};
		}

		DeclarationExporter PopulateExporter()
		{
			var matchedNumber = dataProvider.Supplier?.GetBusinessOrIndividualRegistrationNumber();

			var exporterIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var roleCode = matchedNumber?.Type ?? ZString.Empty;
			var countrySubDivisionID = dataProvider.Supplier?.RoadNameCode ?? ZString.Empty;
			var line = dataProvider.Supplier?.AddressLine2 ?? ZString.Empty;
			var buildingNumber = dataProvider.Supplier?.BuildingNumber ?? ZString.Empty;

			return new DeclarationExporter
			{
				Id = PopulateExporterID(dataProvider.Supplier, exporterIdentificationID),
				RoleCode = new CargoWise.Customs.KR.MessageDefinitions.DS.ExporterRoleCodeType { Value = roleCode },
				Name = new ExporterNameTextType { Value = dataProvider.Supplier?.CompanyName ?? ZString.Empty },
				TypeCode = new ExporterTypeCodeType { Value = dataProvider.ExporterType },
				Address = new DeclarationExporterAddress
				{
					CountrySubDivisionId = countrySubDivisionID.IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = countrySubDivisionID },
					PostcodeId = new AddressPostcodeIdType { Value = dataProvider.Supplier?.Postcode ?? ZString.Empty },
					Line = line.IsEmpty ? null : new AddressLineTextType { Value = line },
					BuildingNumber = buildingNumber.IsEmpty ? null : new AddressBuildingNumberTextType { Value = buildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Supplier?.AddressLine1 ?? ZString.Empty }
				},
				Contact = new DeclarationExporterContact
				{
					RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.Supplier?.RepresentativeName ?? ZString.Empty }
				}
			};
		}

		Collection<ExporterIdentificationIdType> PopulateExporterID(IOrganization exporter, string registrationID)
		{
			Collection<ExporterIdentificationIdType> result = new Collection<ExporterIdentificationIdType>();
			var unipassID = exporter?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var officeID = exporter?.GetRegistrationNumber(IdentificationType.OfficeID);

			if (!registrationID.IsEmpty() || !unipassID.IsEmpty() || !officeID.IsEmpty())
			{
				if (!registrationID.IsEmpty())
				{
					var idNumber = new ExporterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Zzz,
						Value = registrationID
					};
					result.Add(idNumber);
				}
				if (!unipassID.IsEmpty())
				{
					var idNumber = new ExporterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = unipassID
					};
					result.Add(idNumber);
				}
				if (!officeID.IsEmpty())
				{
					var idNumber = new ExporterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Kts,
						Value = officeID
					};
					result.Add(idNumber);
				}
			}
			else
			{
				result.Add(new ExporterIdentificationIdType());
			}
			return result;
		}

		DeclarationFinalTransportMeansLoadingPlace PopulateFinalTransportMeansLoadingPlace()
		{
			return dataProvider.FinalLoadingPlace.IsEmpty ? null : new DeclarationFinalTransportMeansLoadingPlace
			{
				Id = new FinalTransportMeansLoadingPlaceIdentificationIdType { Value = dataProvider.FinalLoadingPlace }
			};
		}

		DeclarationGoodsShipment PopulateGoodsShipment() => new DeclarationGoodsShipment
		{
			Buyer = new DeclarationGoodsShipmentBuyer
			{
				Id = PopulateBuyerID(dataProvider.Importer),
				Name = new BuyerNameTextType
				{
					Value = dataProvider.Importer?.CompanyName ?? ZString.Empty
				},
				Address = new DeclarationGoodsShipmentBuyerAddress
				{
					CountryCode = new AddressCountryCodeType { Value = dataProvider.CountryOfDestination }
				}
			},
			AdditionalDocument = PopulateAdditionDocument(),
			ApprovedEstablishmentPlace = PopulateApprovedEstablishmentPlace(),
			Consignment = new DeclarationGoodsShipmentConsignment
			{
				ContainerIndicator = dataProvider.ContainerizedIndicator,
				GoodsStatusCode = dataProvider.GoodsStatus.IsEmpty ? null : new ConsignmentGoodsStatusCodeType { Value = dataProvider.GoodsStatus },
				BorderTransportMeans = PopulateGoodsShipmentBorderTransportMeans(),
				GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation
				{
					Id = new GoodsLocationIdentificationIdType { Value = dataProvider.GoodsLocationPostcode },
					Address = new DeclarationGoodsShipmentConsignmentGoodsLocationAddress
					{
						Description = new AddressDescriptionTextType { Value = dataProvider.GoodsLocationAddress }
					}
				}
			},
			CustomsValuation = PopulateCustomsValuation(),
			DrawBack = new DeclarationGoodsShipmentDrawBack
			{
				RoleCode = dataProvider.DrawbackApplicantType.IsEmpty ? null : new DrawBackRoleCodeType { Value = dataProvider.DrawbackApplicantType },
				ApplicationTypeCode = new DrawBackApplicationTypeCodeType { Value = dataProvider.ApplicationForSimpleDrawback }
			},
			GovernmentAgencyGoodsItem = PopulateGovernmentAgencyGoodsItem(),
			TradeTerms = new DeclarationGoodsShipmentTradeTerms
			{
				ConditionCode = new TradeTermsConditionCodeType { Value = dataProvider.Incoterm },
				SettlementConditionCode = dataProvider.InvoicePaymentTerm.IsEmpty ? null : new TradeTermsSettlementConditionCodeType { Value = dataProvider.InvoicePaymentTerm }
			},
			Warehouse = new DeclarationGoodsShipmentWarehouse
			{
				Name = new WarehouseNameTextType { Value = dataProvider.GoodsLocationAdditionalDetails }
			}
		};

		BuyerIdentificationIdType PopulateBuyerID(IOrganization buyer)
		{
			BuyerIdentificationIdType result = null;
			var buyerID = buyer?.GetRegistrationNumber(IdentificationType.ForeignCompanyID);
			if (!buyerID.IsEmpty())
			{
				result = new BuyerIdentificationIdType();
				result.Value = buyerID;
			}
			return result;
		}

		DeclarationGoodsShipmentAdditionalDocument PopulateAdditionDocument()
		{
			DeclarationGoodsShipmentAdditionalDocument result = null;
			if (!dataProvider.LCNo.IsEmpty)
			{
				result = new DeclarationGoodsShipmentAdditionalDocument();
				result.Id = new AdditionalDocumentIdentificationIdType { Value = dataProvider.LCNo };
			}
			return result;
		}

		DeclarationGoodsShipmentApprovedEstablishmentPlace PopulateApprovedEstablishmentPlace()
		{
			DeclarationGoodsShipmentApprovedEstablishmentPlace result = null;
			if (!dataProvider.IndustrialParkCode.IsEmpty)
			{
				result = new DeclarationGoodsShipmentApprovedEstablishmentPlace();
				result.Name = new ApprovedEstablishmentPlaceNameTextType { Value = dataProvider.IndustrialParkCode };
			}
			return result;
		}

		DeclarationGoodsShipmentConsignmentBorderTransportMeans PopulateGoodsShipmentBorderTransportMeans()
		{
			DeclarationGoodsShipmentConsignmentBorderTransportMeans result = null;
			if (!dataProvider.TransportMode.IsEmpty || !dataProvider.ContainerPackMode.IsEmpty)
			{
				result = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				result.TypeCode = dataProvider.TransportMode.IsEmpty ? null : new BorderTransportMeansTypeCodeType { Value = dataProvider.TransportMode };
				if (!dataProvider.ContainerPackMode.IsEmpty)
				{
					result.TransportEquipment = new DeclarationGoodsShipmentConsignmentBorderTransportMeansTransportEquipment
					{
						CharacteristicCode = new TransportEquipmentCharacteristicCodeType { Value = dataProvider.ContainerPackMode }
					};
				}
			}
			return result;
		}

		DeclarationGoodsShipmentCustomsValuation PopulateCustomsValuation()
		{
			DeclarationGoodsShipmentCustomsValuation result = null;
			if (!dataProvider.Insurance.IsEmpty || !dataProvider.Freight.IsEmpty)
			{
				result = new DeclarationGoodsShipmentCustomsValuation
				{
					ExitToEntryChargeAmount = dataProvider.Insurance.IsEmpty ? null : new CustomsValuationExitToEntryChargeAmountType { Value = dataProvider.Insurance },
					FreightChargeAmount = dataProvider.Freight.IsEmpty ? null : new CustomsValuationFreightChargeAmountType { Value = dataProvider.Freight }
				};
			}
			return result;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGovernmentAgencyGoodsItem()
		{
			var entryLines = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
				{
					SequenceNumeric = new GovernmentAgencyGoodsItemSequenceTextType { Value = entryLine.EntryLineNo },
					AdditionalCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalCode
					{
						AttachmentIndicatorCode = new AdditionalCodeAttachmentIndicatorCodeType { Value = entryLine.DocumentAttached }
					},
					AdditionalInformation = PopulateGovernmentAgencyGoodsItemAdditionalInformation(entryLine),
					Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
					{
						CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.TradeName },
						CountQuantity = PopulateCommodityCountQuantityType(entryLine),
						Description = entryLine.HSDescription.IsEmpty ? null : new CommodityDescriptionTextType { Value = entryLine.HSDescription },
						Name = entryLine.BrandName.IsEmpty ? null : new CommodityNameTextType { Value = entryLine.BrandName },
						ValueAmount = new CommodityValueAmountType { Value = entryLine.CustomsValue },
						AdditionalInformation = PopulateCommodityAdditionalInformation(entryLine),
						Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
						{
							Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode }
						},
						DetailedCommodity = PopulateDetailedCommodity(entryLine.InvoiceLines),
						Invoice = PopulateInvoice(entryLine),
						PreviousDocument = PopulatePreviousDocument(entryLine),
						CriteriaConformanceCode = entryLine.FTAType.IsEmpty ? null : new CommodityCriteriaConformanceCodeType { Value = entryLine.FTAType }
					},
					GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure
					{
						NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = entryLine.NetWeightInKG, KcsUnitCode = entryLine.NetWeightUQ }
					},
					Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
					{
						CountryCode = new OriginCountryCodeType { Value = entryLine.CountryOfOrigin },
						RuleCode = entryLine.CountryOfOriginDeterminationRule.IsEmpty ? null : new OriginRuleCodeType { Value = entryLine.CountryOfOriginDeterminationRule },
						OriginDescription = entryLine.CountryOfOriginLabelLocation.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginOriginDescription
						{
							DisplayIndicatorCode = new OriginDescriptionDisplayIndicatorCodeType { Value = entryLine.CountryOfOriginLabelLocation }
						}
					},
					Packaging = PopulateGovernmentAgencyGoodsItemPacking(entryLine)
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation PopulateGovernmentAgencyGoodsItemAdditionalInformation(IExportEntryLine entryLine)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation result = null;
			if (!entryLine.SkipManifestReporting.IsEmpty || !entryLine.CertificateOfOriginIssued.IsEmpty)
			{
				result = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
				{
					StatementCode = entryLine.CertificateOfOriginIssued.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = entryLine.CertificateOfOriginIssued },
					StatementTypeCode = entryLine.SkipManifestReporting.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = entryLine.SkipManifestReporting }
				};
			}
			return result;
		}

		CommodityCountQuantityType PopulateCommodityCountQuantityType(IExportEntryLine entryLine)
		{
			CommodityCountQuantityType result = null;
			if (!entryLine.Qty.IsEmpty || !entryLine.QtyUnit.IsEmpty)
			{
				result = new CommodityCountQuantityType { Value = entryLine.Qty, KcsUnitCode = entryLine.QtyUnit };
			}
			return result;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation PopulateCommodityAdditionalInformation(IExportEntryLine entryLine)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation additionalInformation = null;
			if (!entryLine.PreApprovalNo.IsEmpty || entryLine.PreApprovalEffectiveFromDate.IsValid || entryLine.PreApprovalEffectiveToDate.IsValid)
			{
				additionalInformation = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation();
				additionalInformation.Id = new AdditionalInformationIdentificationIdType { Value = entryLine.PreApprovalNo };
				additionalInformation.StatementCode = new AdditionalInformationStatementCodeType { Value = entryLine.PreApprovalType };
				additionalInformation.BeginningDateTime = !entryLine.PreApprovalEffectiveFromDate.IsValid ? string.Empty : entryLine.PreApprovalEffectiveFromDate.ToString(DateFormatType.Date);
				additionalInformation.EndingDateTime = !entryLine.PreApprovalEffectiveToDate.IsValid ? string.Empty : entryLine.PreApprovalEffectiveToDate.ToString(DateFormatType.Date);
			}
			return additionalInformation;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument PopulatePreviousDocument(IExportEntryLine entryLine)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument previousDocument = null;
			if (!entryLine.ImportDeclarationNumber.IsEmpty || !entryLine.ImportEntryLineNo.IsEmpty)
			{
				previousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument();
				if (!entryLine.ImportDeclarationNumber.IsEmpty)
				{
					previousDocument.Id = new PreviousDocumentIdentificationIdType { Value = entryLine.ImportDeclarationNumber };
				}
				if (!entryLine.ImportEntryLineNo.IsEmpty)
				{
					previousDocument.LineNumeric = new PreviousDocumentLineTextType { Value = entryLine.ImportEntryLineNo };
				}
			}
			return previousDocument;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging PopulateGovernmentAgencyGoodsItemPacking(IExportEntryLine entryLine)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging packing = null;
			if (!entryLine.PackQty.IsEmpty || !entryLine.PackType.IsEmpty)
			{
				packing = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging
				{
					QuantityQuantity = entryLine.PackQty.IsEmpty ? null : new PackagingQuantityQuantityType { Value = entryLine.PackQty },
					TypeCode = entryLine.PackType.IsEmpty ? null : new PackagingTypeCodeType { Value = entryLine.PackType }
				};
			}
			return packing;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice PopulateInvoice(IExportEntryLine entryLine)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice invoice = null;
			if (!entryLine.InvoiceNo.IsEmpty)
			{
				invoice = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice
				{
					Id = new InvoiceIdentificationIdType { Value = entryLine.InvoiceNo }
				};
			}
			return invoice;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity> PopulateDetailedCommodity(IEnumerable<IExportInvoiceLine> invoiceLines)
		{
			if (!invoiceLines.Any())
			{
				return null;
			}

			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity>();
			foreach (var invoiceLine in invoiceLines)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
				{
					SequenceNumeric = new DetailedCommoditySequenceTextType { Value = invoiceLine.InvoiceLineNo },
					CargoDescription = new DetailedCommodityCargoDescriptionTextType { Value = invoiceLine.DetailDescription },
					CountQuantity = new DetailedCommodityCountQuantityType { Value = invoiceLine.QtyOrWeight, KcsUnitCode = invoiceLine.QtyOrWeightUnit },
					LotNumberId = invoiceLine.LotNumber.IsEmpty ? null : new DetailedCommodityLotNumberIdType { Value = invoiceLine.LotNumber },
					UnitPriceAmount = new DetailedCommodityUnitPriceAmountType { Value = invoiceLine.UnitPrice },
					ValueAmount = new CargoWise.Customs.KR.MessageDefinitions.KCSDS.DetailedCommodityValueAmountType { Value = invoiceLine.Amount },
					AdditionalDocument = PopulateAdditionalDocument(invoiceLine.GAApprovalDocuments),
					Vehicle = PopulateVehicle(invoiceLine.VehicleNumbers),
					Constituent = PopulateConstituentElementName(invoiceLine.Ingredient)
				};
				result.Add(item);
			}
			return result;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument> PopulateAdditionalDocument(IEnumerable<IExportGAApprovalDocument> gAApprovalDocuments)
		{
			if (!gAApprovalDocuments.Any())
			{
				return null;
			}

			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument>();
			foreach (var gAApprovalDocument in gAApprovalDocuments)
			{
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityAdditionalDocument
				{
					Id = new AdditionalDocumentIdentificationIdType { Value = gAApprovalDocument.RequirementApprovalNumber },
					IssueDateTime = gAApprovalDocument.ApprovalDate == ZDate.Invalid ? null : gAApprovalDocument.ApprovalDate.ToString(DateFormatType.Date),
					TypeCode = gAApprovalDocument.RequirementDocumentType.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = gAApprovalDocument.RequirementDocumentType },
					Name = gAApprovalDocument.DocumentName.IsEmpty ? null : new AdditionalDocumentNameTextType { Value = gAApprovalDocument.DocumentName },
					CriteriaConformanceIndicatorCode = new AdditionalDocumentCriteriaConformanceIndicatorCodeType { Value = gAApprovalDocument.RequirementType },
					CriteriaConformanceId = new AdditionalDocumentCriteriaConformanceIdentificationTextType { Value = gAApprovalDocument.SequenceNo },
					NonDescriptionReason = gAApprovalDocument.ReasonForMissingApprovalNumber.IsEmpty ? null : new AdditionalDocumentNonDescriptionReasonTextType { Value = gAApprovalDocument.ReasonForMissingApprovalNumber },
					CriteriaConformanceCode = new CargoWise.Customs.KR.MessageDefinitions.KCSDS.AdditionalDocumentCriteriaConformanceCodeType { Value = gAApprovalDocument.RegulationCategoryCode },
					CommercialCategorizationId = gAApprovalDocument.UniqueItemID.IsEmpty ? null : new AdditionalDocumentCommercialCategorizationIdType { Value = gAApprovalDocument.UniqueItemID },
					NonObjectReasonCode = gAApprovalDocument.NonGAReasonType.IsEmpty ? null : new AdditionalDocumentNonObjectReasonCodeType { Value = gAApprovalDocument.NonGAReasonType }
				};
				result.Add(item);
			}
			return result;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityVehicle> PopulateVehicle(IEnumerable<IExportVehicleNo> vehicleNumbers)
		{
			if (!vehicleNumbers.Any())
			{
				return null;
			}

			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityVehicle>();
			foreach (var vehicleNumber in vehicleNumbers)
			{
				var vehicle = PopulateVehicleNumber(vehicleNumber);
				if (vehicle != null)
				{
					result.Add(vehicle);
				}
			}
			return result.Count == 0 ? null : result;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityVehicle PopulateVehicleNumber(IExportVehicleNo vehicleNo)
		{
			var id = vehicleNo.VIN.IsEmpty ? null : new VehicleIdentificationIdType { Value = vehicleNo.VIN };
			var sequenceNumeric = vehicleNo.SequenceNo.IsEmpty ? null : new VehicleSequenceTextType { Value = vehicleNo.SequenceNo };
			return id == null && sequenceNumeric == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityVehicle
			{
				Id = id,
				SequenceNumeric = sequenceNumeric
			};
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityConstituent PopulateConstituentElementName(ZString ingredient)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityConstituent constituent = null;
			if (!ingredient.IsEmpty)
			{
				constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodityConstituent()
				{
					ElementName = new ConstituentElementNameTextType { Value = ingredient }
				};
			}
			return constituent;
		}

		DeclarationManufacturer PopulateManufacturer()
		{
			return new DeclarationManufacturer
			{
				Id = PopulateManufacturerID(dataProvider.Manufacturer),
				Name = new ManufacturerNameTextType { Value = dataProvider.Manufacturer?.CompanyName ?? ZString.Empty },
				Address = new DeclarationManufacturerAddress
				{
					PostcodeId = new AddressPostcodeIdType { Value = dataProvider.Manufacturer?.Postcode ?? ZString.Empty }
				}
			};
		}
		Collection<ManufacturerIdentificationIdType> PopulateManufacturerID(IOrganization manufacturer)
		{
			Collection<ManufacturerIdentificationIdType> result = null;
			var unipassID = manufacturer?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var officeID = manufacturer?.GetRegistrationNumber(IdentificationType.OfficeID);
			if (!unipassID.IsEmpty() || !officeID.IsEmpty())
			{
				result = new Collection<ManufacturerIdentificationIdType>();
				if (!unipassID.IsEmpty())
				{
					var idNumber = new ManufacturerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = unipassID
					};
					result.Add(idNumber);
				}
				if (!officeID.IsEmpty())
				{
					var idNumber = new ManufacturerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Kts,
						Value = officeID
					};
					result.Add(idNumber);
				}
			}
			return result;
		}

		DeclarationLoadingLocation PopulateLoadingLocation()
		{
			var typeCode = dataProvider.PortOfLoading.Length > 3 ? "10" : "40";

			return new DeclarationLoadingLocation
			{
				Id = !dataProvider.PortOfLoading.IsEmpty ? new LoadingLocationIdentificationIdType { Value = dataProvider.PortOfLoading } : null,
				TypeCode = new LoadingLocationTypeCodeType { Value = typeCode }
			};
		}

		DeclarationPackaging PopulatePackaging()
		{
			return new DeclarationPackaging
			{
				TypeCode = new PackagingTypeCodeType { Value = dataProvider.PackType }
			};
		}

		DeclarationPreviousDocument PopulatePreviousDocument()
		{
			if (dataProvider.CargoManagement.ImportCargoManagementNumber.IsEmpty)
			{
				return null;
			}
			else
			{
				return new DeclarationPreviousDocument
				{
					Id = new PreviousDocumentIdentificationIdType { Value = dataProvider.CargoManagement.ImportCargoManagementNumber }
				};
			}
		}

		DeclarationSouthNorthTrade PopulateSouthNorthTrade()
		{
			if (dataProvider.SouthNorthTradeIdentification.IsEmpty && dataProvider.SouthNorthTradeYN.IsEmpty)
			{
				return null;
			}
			return new DeclarationSouthNorthTrade
			{
				IdentificationId = !dataProvider.SouthNorthTradeIdentification.IsEmpty ? new SouthNorthTradeIdentificationIdType { Value = dataProvider.SouthNorthTradeIdentification } : null,
				TradeIndicatorCode = !dataProvider.SouthNorthTradeYN.IsEmpty ? new SouthNorthTradeTradeIndicatorCodeType { Value = dataProvider.SouthNorthTradeYN } : null,
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType { Value = !dataProvider.UnipassDeclarantID.IsEmpty() ? dataProvider.UnipassDeclarantID : string.Empty },
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant?.CompanyName ?? ZString.Empty },
				Contact = new DeclarationSubmitterContact
				{
					RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.Declarant?.RepresentativeName ?? ZString.Empty }
				}
			};
		}

		DeclarationUcr PopulateUCR()
		{
			if (dataProvider.UCR.IsEmpty)
			{
				return null;
			}
			else
			{
				return new DeclarationUcr
				{
					Id = new UcrIdentificationIdType { Value = dataProvider.UCR }
				};
			}
		}
	}
}
