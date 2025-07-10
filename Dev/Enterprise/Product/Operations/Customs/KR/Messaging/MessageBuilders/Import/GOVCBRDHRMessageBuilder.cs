using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHR;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.KR.Messaging.Constants;
namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._DHR)]
	public class GOVCBRDHRMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportDHRHeader dataHeaderProvider;
		public GOVCBRDHRMessageBuilder(IImportDHRHeader dataHeaderProvider)
		{
			this.dataHeaderProvider = dataHeaderProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				AdditionalDocument = PopulateAdditionalDocument(),
				Consignment = PopulateConsignment(),
				Exporter = PopulateExporter(),
				GoodsShipment = PopulateGoodsShipment(),
				Importer = PopulateImporter(),
				Manufacturer = PopulateManufacturer()
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataHeaderProvider.ImportDeclarationNumber };
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._DHR };
		}

		DeclarationAdditionalDocument PopulateAdditionalDocument()
		{
			return dataHeaderProvider.LawCode.IsEmpty && dataHeaderProvider.StatementNumber5WN.IsEmpty ? null : new DeclarationAdditionalDocument
			{
				TypeCode = dataHeaderProvider.LawCode.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = dataHeaderProvider.LawCode },
				Id = dataHeaderProvider.StatementNumber5WN.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataHeaderProvider.StatementNumber5WN }
			};
		}

		Collection<DeclarationConsignment> PopulateConsignment()
		{
			if (!dataHeaderProvider.DHRInvoiceLines.Any())
			{
				return null;
			}

			var invoiceLines = new Collection<DeclarationConsignment>();
			foreach (var invoiceLine in dataHeaderProvider.DHRInvoiceLines)
			{
				var item = new DeclarationConsignment
				{
					SequenceId = new ConsignmentSequenceIdentifierIdType { Value = invoiceLine.EntryLineNo.ToString() },
					AdditionalDocument = invoiceLine.CertificateOfOriginNo.IsEmpty && invoiceLine.CertificateOfOriginSeqNo.IsEmpty ? null : new DeclarationConsignmentAdditionalDocument
					{
						Id = invoiceLine.CertificateOfOriginNo.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = invoiceLine.CertificateOfOriginNo },
						CriteriaConformanceId = invoiceLine.CertificateOfOriginSeqNo.IsEmpty ? null : new AdditionalDocumentCriteriaConformanceIdentificationIdType { Value = invoiceLine.CertificateOfOriginSeqNo.ToString() }
					},
					ConsignmentItem = invoiceLine.CertificateOfOriginUsedQuantity.IsEmpty && invoiceLine.CertificateOfOriginUsedUQ.IsEmpty && invoiceLine.InvoiceLineNo.IsEmpty ? null : new DeclarationConsignmentConsignmentItem
					{
						Commodity = new DeclarationConsignmentConsignmentItemCommodity
						{
							CountQuantity = invoiceLine.CertificateOfOriginUsedQuantity.IsEmpty && invoiceLine.CertificateOfOriginUsedUQ.IsEmpty ? null : new CommodityCountQuantityType { Value = invoiceLine.CertificateOfOriginUsedQuantity, KcsUnitCode = invoiceLine.CertificateOfOriginUsedUQ },
							IdentityQualifierCode = invoiceLine.InvoiceLineNo.IsEmpty ? null : new CommodityIdentityQualifierCodeType { Value = invoiceLine.InvoiceLineNo.ToString() },
						}
					}
				};
				invoiceLines.Add(item);
			}
			return invoiceLines;
		}

		DeclarationExporter PopulateExporter()
		{
			return new DeclarationExporter
			{
				Name = new ExporterNameTextType { Value = dataHeaderProvider.Supplier?.CompanyName ?? ZString.Empty },
				Address = new DeclarationExporterAddress
				{
					CountryCode = new AddressCountryCodeType { Value = dataHeaderProvider.Supplier?.CountryCode ?? ZString.Empty },
					Line = new AddressLineTextType { Value = dataHeaderProvider.Supplier?.AddressLine1 ?? ZString.Empty }
				},
				Contact = new DeclarationExporterContact
				{
					Name = new ContactNameTextType { Value = dataHeaderProvider.Supplier?.RepresentativeName ?? ZString.Empty }
				},
				Communication = new Collection<DeclarationExporterCommunication>
				{
					new DeclarationExporterCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
						Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Supplier?.PhoneNumber ?? ZString.Empty }
					},
					new DeclarationExporterCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
						Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Supplier?.FaxNumber ?? ZString.Empty }
					}
				}
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var entryLines = new Collection<DeclarationGoodsShipment>();
			foreach (var entryLine in dataHeaderProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipment
				{
					SequenceNumeric = entryLine.SequenceNo,
					ExportationCountryCode = dataHeaderProvider.DepartureCountryCode.IsEmpty ? null : new GoodsShipmentExportationCountryCodeType { Value = dataHeaderProvider.DepartureCountryCode },
					TransactionNatureCode = entryLine.AdditionalInvoiceIssuedInThirdCountryYN.IsEmpty ? null : new GoodsShipmentTransactionNatureCodeType { Value = entryLine.AdditionalInvoiceIssuedInThirdCountryYN },
					AdditionalDocument = new DeclarationGoodsShipmentAdditionalDocument { SequenceNumeric = entryLine.EntryLineNo },
					AdditionalInformation = dataHeaderProvider.TransshipmentYN.IsEmpty ? null : new DeclarationGoodsShipmentAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = dataHeaderProvider.TransshipmentYN }
					},
					Consignment = new DeclarationGoodsShipmentConsignment
					{
						BorderTransportMeans = !dataHeaderProvider.DepartureDate.IsValid && !dataHeaderProvider.TransshipmentDate.IsValid ? null : new DeclarationGoodsShipmentConsignmentBorderTransportMeans
						{
							DepartureDateTime = !dataHeaderProvider.DepartureDate.IsValid ? null : dataHeaderProvider.DepartureDate.ToString(DateFormatType.Date),
							Itinerary = !dataHeaderProvider.TransshipmentDate.IsValid ? null : new DeclarationGoodsShipmentConsignmentBorderTransportMeansItinerary
							{
								DepartureDateTime = dataHeaderProvider.TransshipmentDate.ToString(DateFormatType.Date)
							}
						},
						DutyTaxFee = new DeclarationGoodsShipmentConsignmentDutyTaxFee
						{
							DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.DutyRateCode },
							TaxRateNumeric = entryLine.TariffRate
						},
						LoadingLocation = dataHeaderProvider.DeparturePort.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentLoadingLocation
						{
							Name = new LoadingLocationNameTextType { Value = dataHeaderProvider.DeparturePort }
						},
						TranshipmentLocation = dataHeaderProvider.TransshipmentCountryCode.IsEmpty && dataHeaderProvider.TransshipmentPort.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentTranshipmentLocation
						{
							Id = dataHeaderProvider.TransshipmentCountryCode.IsEmpty ? null : new TranshipmentLocationIdentificationIdType { Value = dataHeaderProvider.TransshipmentCountryCode },
							Name = dataHeaderProvider.TransshipmentPort.IsEmpty ? null : new TranshipmentLocationNameTextType { Value = dataHeaderProvider.TransshipmentPort }
						}
					},
					Exporter = entryLine.CertificateOfOriginExporterNumber.IsEmpty ? null : new DeclarationGoodsShipmentExporter
					{
						Id = new ExporterIdentificationIdType { Value = entryLine.CertificateOfOriginExporterNumber }
					},
					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							AdditionalDocument = entryLine.AdditionalInvoiceIssuingThirdCountryCode.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument
							{
								IssueLocationId = new AdditionalDocumentIssueLocationIdType { Value = entryLine.AdditionalInvoiceIssuingThirdCountryCode }
							},
							AdditionalInformation = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation
							{
								StatementCode = new AdditionalInformationStatementCodeType { Value = entryLine.CountryOfOriginSupportingDocType },
								StatementTypeCode = entryLine.CertifiticateOfOriginIssuerType.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = entryLine.CertifiticateOfOriginIssuerType }
							},
							CertificateOfOrigin = PopulateOforigin(entryLine),
							Classification = entryLine.HSCode.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
							{
								Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode }
							},
							DetailedCommodity = entryLine.NetWeight.IsDefault ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
							{
								WeightMeasure = new DetailedCommodityWeightMeasureType { Value = entryLine.NetWeight }
							},
							GoodsMeasure = entryLine.CertificateOfOriginTotalNetWeight.IsDefault ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure
							{
								NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = entryLine.CertificateOfOriginTotalNetWeight }
							},
							Producer = PopulateProducer(entryLine)
						},
						Origin = PopulateOrigin(entryLine)
					},
					Invoice = entryLine.AdditionalInvoiceIssuingThirdCountryCode.IsEmpty ? null : new DeclarationGoodsShipmentInvoice
					{
						Submitter = new DeclarationGoodsShipmentInvoiceSubmitter
						{
							Address = new DeclarationGoodsShipmentInvoiceSubmitterAddress
							{
								CountryCode = new AddressCountryCodeType { Value = entryLine.AdditionalInvoiceIssuingThirdCountryCode }
							}
						}
					}
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer PopulateProducer(IImportFTALine entryLine)
		{
			if (dataHeaderProvider.Manufacturer == null || entryLine.CountryOfOrigin != CountryCodes.Israel)
			{
				return null;
			}

			var line = dataHeaderProvider.Manufacturer.GetAddressDetails().IsEmpty ? null : new AddressLineTextType() { Value = dataHeaderProvider.Manufacturer.GetAddressDetails() };
			var postcodeID = dataHeaderProvider.Manufacturer.Postcode.IsEmpty ? null : new AddressPostcodeIdType() { Value = dataHeaderProvider.Manufacturer.Postcode };

			if (line == null && postcodeID == null)
			{
				return null;
			}
			return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer()
			{
				Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducerAddress()
				{
					Line = line,
					PostcodeId = postcodeID
				}
			};
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin PopulateOrigin(IImportFTALine entryLine)
		{
			var countryCode = entryLine.CountryOfOrigin.IsEmpty ? null : new OriginCountryCodeType { Value = entryLine.CountryOfOrigin };
			var ruleCode = entryLine.CertificateOfOriginProductType.IsEmpty ? null : new OriginRuleCodeType { Value = entryLine.CertificateOfOriginProductType };

			if (countryCode == null && ruleCode == null)
			{
				return null;
			}

			return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
			{
				CountryCode = countryCode,
				RuleCode = ruleCode
			};
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCertificateOfOrigin PopulateOforigin(IImportFTALine entryLine)
		{
			var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCertificateOfOrigin
			{
				IssueDateTime = entryLine.CertificateOfOriginIssueDate.IsValid ? entryLine.CertificateOfOriginIssueDate.ToString(DateFormatType.Date) : null,
				IssueId = new CertificateOfOriginIssueIdType { Value = entryLine.CertificateOfOriginNo },
				IssueAgencyName = entryLine.CertificateOfOriginAgencyName.IsEmpty ? null : new CertificateOfOriginIssueAgencyNameTextType { Value = entryLine.CertificateOfOriginAgencyName },
				IssueAgencyTypeCode = entryLine.CertifiticateOfOriginIssuingAgencyType.IsEmpty ? null : new CertificateOfOriginIssueAgencyTypeCodeType { Value = entryLine.CertifiticateOfOriginIssuingAgencyType }
			};

			if (entryLine.CertificateOfOriginSplitOrder != decimal.Zero)
			{
				item.SplitVersionNumeric = entryLine.CertificateOfOriginSplitOrder;
				item.SplitVersionNumericValueSpecified = true;
			}
			return item;
		}

		DeclarationImporter PopulateImporter()
		{
			var matchedNumber = dataHeaderProvider.Importer?.GetBusinessOrIndividualRegistrationNumber();
			var matchedUnipassIDNumber = dataHeaderProvider.Importer?.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForOrganization);

			var importerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var importerUnipassIDNumber = matchedUnipassIDNumber?.Number ?? ZString.Empty;
			var roleCode = matchedNumber?.Type ?? ZString.Empty;
			var ids = new Collection<ImporterIdentificationIdType>
			{
						new ImporterIdentificationIdType
						{
							SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
							Value = importerIdentificationID
						}
			};

			if (!importerUnipassIDNumber.IsEmpty)
			{
				ids.Add(new ImporterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
					Value = importerUnipassIDNumber
				});
			}

			return new DeclarationImporter
			{
				Id = ids,
				Name = new ImporterNameTextType { Value = dataHeaderProvider.Importer?.CompanyName ?? ZString.Empty },
				RoleCode = new CargoWise.Customs.KR.MessageDefinitions.DS.ImporterRoleCodeType { Value = roleCode },
				Address = new DeclarationImporterAddress
				{
					CountrySubDivisionId = dataHeaderProvider.Importer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataHeaderProvider.Importer?.RoadNameCode },
					Line = dataHeaderProvider.Importer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataHeaderProvider.Importer?.AddressLine2 },
					PostcodeId = new AddressPostcodeIdType { Value = dataHeaderProvider.Importer?.Postcode ?? ZString.Empty },
					BuildingNumber = dataHeaderProvider.Importer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataHeaderProvider.Importer?.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataHeaderProvider.Importer?.AddressLine1 ?? ZString.Empty }
				},
				Contact = new DeclarationImporterContact
				{
					Name = new ContactNameTextType { Value = dataHeaderProvider.Importer?.RepresentativeName ?? ZString.Empty }
				},
				Communication = new Collection<DeclarationImporterCommunication>
					{
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.PhoneNumber ?? ZString.Empty }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.FaxNumber ?? ZString.Empty }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.Email },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.Email ?? ZString.Empty }
						}
					}
			};
		}

		DeclarationManufacturer PopulateManufacturer()
		{
			if (dataHeaderProvider.Manufacturer == null)
			{
				return null;
			}
			return new DeclarationManufacturer
			{
				Name = new ManufacturerNameTextType { Value = dataHeaderProvider.Manufacturer.CompanyName },
				Address = new DeclarationManufacturerAddress
				{
					Line = new AddressLineTextType { Value = dataHeaderProvider.Manufacturer.AddressLine1 }
				},
				Contact = new DeclarationManufacturerContact
				{
					Name = new ContactNameTextType { Value = dataHeaderProvider.Manufacturer.RepresentativeName }
				},
				Communication = CreateCommunication()
			};

			Collection<DeclarationManufacturerCommunication> CreateCommunication()
			{
				Collection<DeclarationManufacturerCommunication> result = null;

				if (!dataHeaderProvider.Manufacturer.PhoneNumber.IsEmpty)
				{
					result = new Collection<DeclarationManufacturerCommunication>();
					result.Add(new DeclarationManufacturerCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
						Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Manufacturer.PhoneNumber }
					});
				}

				if (!dataHeaderProvider.Manufacturer.FaxNumber.IsEmpty)
				{
					if (result == null)
					{
						result = new Collection<DeclarationManufacturerCommunication>();
					}
					result.Add(new DeclarationManufacturerCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
						Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Manufacturer.FaxNumber }
					});
				}
				return result;
			}
		}
	}
}
