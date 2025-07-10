using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SC;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._5SC)]
	public class GOVCBR5SCMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportFTAHeader dataProvider;
		public GOVCBR5SCMessageBuilder(IImportFTAHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				Exporter = PopulateExporter(),
				GoodsShipment = PopulateGoodsShipment(),
				Importer = PopulateImporter(),
				Manufacturer = PopulateManufacturer(),
				AdditionalDocument = PopulateAdditionalDocument(),
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType
			{
				Value = dataProvider.ImportDeclarationNumber
			};
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType
			{
				Value = GOVCBR + ElectronicDocumentTypeList.Codes._5SC
			};
		}

		DeclarationExporter PopulateExporter()
		{
			return new DeclarationExporter
			{
				Name = new ExporterNameTextType { Value = dataProvider.Supplier?.CompanyName },
				Address = new DeclarationExporterAddress
				{
					CountryCode = new AddressCountryCodeType { Value = dataProvider.Supplier?.CountryCode },
					Line = new AddressLineTextType { Value = dataProvider.Supplier?.AddressLine1 }
				},
				Contact = new DeclarationExporterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Supplier?.RepresentativeName }
				},
				Communication = PopulateExporterCommunication()
			};
		}

		Collection<DeclarationExporterCommunication> PopulateExporterCommunication()
		{
			var phone = dataProvider.Supplier?.PhoneNumber ?? ZString.Empty;
			var fax = dataProvider.Supplier?.FaxNumber ?? ZString.Empty;
			var communication = new Collection<DeclarationExporterCommunication>();

			if (!phone.IsEmpty)
			{
				communication.Add(new DeclarationExporterCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
					Id = new CommunicationIdentificationIdType { Value = phone }
				});
			}

			if (!fax.IsEmpty)
			{
				communication.Add(new DeclarationExporterCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
					Id = new CommunicationIdentificationIdType { Value = fax }
				});
			}

			return communication;
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var entryLines = new Collection<DeclarationGoodsShipment>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipment
				{
					SequenceNumeric = entryLine.SequenceNo,
					ExportationCountryCode = dataProvider.DepartureCountryCode.IsEmpty ? null : new GoodsShipmentExportationCountryCodeType { Value = dataProvider.DepartureCountryCode },
					TransactionNatureCode = entryLine.AdditionalInvoiceIssuedInThirdCountryYN.IsEmpty ? null : new GoodsShipmentTransactionNatureCodeType { Value = entryLine.AdditionalInvoiceIssuedInThirdCountryYN },
					AdditionalDocument = new DeclarationGoodsShipmentAdditionalDocument
					{
						SequenceNumeric = entryLine.EntryLineNo
					},
					AdditionalInformation = dataProvider.TransshipmentYN.IsEmpty ? null : new DeclarationGoodsShipmentAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.TransshipmentYN }
					},
					Consignment = new DeclarationGoodsShipmentConsignment
					{
						BorderTransportMeans = !dataProvider.DepartureDate.IsValid && !dataProvider.TransshipmentDate.IsValid ? null : new DeclarationGoodsShipmentConsignmentBorderTransportMeans
						{
							DepartureDateTime = !dataProvider.DepartureDate.IsValid ? null : dataProvider.DepartureDate.ToString(DateFormatType.Date),
							Itinerary = !dataProvider.TransshipmentDate.IsValid ? null : new DeclarationGoodsShipmentConsignmentBorderTransportMeansItinerary
							{
								DepartureDateTime = dataProvider.TransshipmentDate.ToString(DateFormatType.Date)
							}
						},
						DutyTaxFee = new DeclarationGoodsShipmentConsignmentDutyTaxFee
						{
							DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType { Value = entryLine.DutyRateCode },
							TaxRateNumeric = entryLine.TariffRate
						},
						LoadingLocation = dataProvider.DeparturePort.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentLoadingLocation
						{
							Name = new LoadingLocationNameTextType { Value = dataProvider.DeparturePort }
						},
						TranshipmentLocation = dataProvider.TransshipmentCountryCode.IsEmpty && dataProvider.TransshipmentPort.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentTranshipmentLocation
						{
							Id = dataProvider.TransshipmentCountryCode.IsEmpty ? null : new TranshipmentLocationIdentificationIdType { Value = dataProvider.TransshipmentCountryCode },
							Name = dataProvider.TransshipmentPort.IsEmpty ? null : new TranshipmentLocationNameTextType { Value = dataProvider.TransshipmentPort }
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
							AdditionalDocument = entryLine.AssociatedCOOIssuingCountryCode.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument
							{
								IssueLocationId = new AdditionalDocumentIssueLocationIdType { Value = entryLine.AssociatedCOOIssuingCountryCode }
							},
							AdditionalInformation = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation
							{
								StatementCode = new AdditionalInformationStatementCodeType { Value = entryLine.CountryOfOriginSupportingDocType },
								StatementTypeCode = entryLine.CertifiticateOfOriginIssuerType.IsEmpty ? null : new AdditionalInformationStatementTypeCodeType { Value = entryLine.CertifiticateOfOriginIssuerType }
							},
							CertificateOfOrigin = PopulateCertificateOfOrigin(entryLine),
							Classification = entryLine.HSCode.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
							{
								Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode }
							},
							DetailedCommodity = entryLine.NetWeight.IsDefault ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDetailedCommodity
							{
								WeightMeasure = new DetailedCommodityWeightMeasureType { Value = entryLine.NetWeight }
							},
							GoodsMeasure = entryLine.CertificateOfOriginTotalNetWeight.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure
							{
								NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = entryLine.CertificateOfOriginTotalNetWeight }
							},
							Producer = PopulateProducer(entryLine.Manufacturer),
						},
						Origin = entryLine.CountryOfOrigin.IsEmpty && entryLine.CertificateOfOriginProductType.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
						{
							CountryCode = entryLine.CountryOfOrigin.IsEmpty ? null : new OriginCountryCodeType { Value = entryLine.CountryOfOrigin },
							RuleCode = entryLine.CertificateOfOriginProductType.IsEmpty ? null : new OriginRuleCodeType { Value = entryLine.CertificateOfOriginProductType }
						},
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

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCertificateOfOrigin PopulateCertificateOfOrigin(IImportFTALine entryLine)
		{
			var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCertificateOfOrigin
			{
				IssueDateTime = entryLine.CertificateOfOriginIssueDate.IsValid ? entryLine.CertificateOfOriginIssueDate.ToString(DateFormatType.Date) : null,
				IssueId = new CertificateOfOriginIssueIdType { Value = entryLine.CertificateOfOriginNo },
				IssueAgencyName = entryLine.CertificateOfOriginAgencyName.IsEmpty ? null : new CertificateOfOriginIssueAgencyNameTextType { Value = entryLine.CertificateOfOriginAgencyName },
				IssueAgencyTypeCode = entryLine.CertifiticateOfOriginIssuingAgencyType.IsEmpty ? null : new CertificateOfOriginIssueAgencyTypeCodeType { Value = entryLine.CertifiticateOfOriginIssuingAgencyType },
				SplitVersionNumeric = entryLine.CertificateOfOriginSplitOrder.IsEmpty ? null : (decimal?)entryLine.CertificateOfOriginSplitOrder,
				SplitVersionNumericValueSpecified = !entryLine.CertificateOfOriginSplitOrder.IsEmpty
			};
			return item;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer PopulateProducer(IOrganization manufacturer)
		{
			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer item = null;
			if (manufacturer != null)
			{
				item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer
				{
					Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducerAddress
					{
						Line = manufacturer.GetAddressDetails().IsEmpty ? null : new AddressLineTextType { Value = manufacturer.GetAddressDetails() },
						PostcodeId = manufacturer.Postcode.IsEmpty ? null : new AddressPostcodeIdType { Value = manufacturer.Postcode }
					}
				};
			}
			return item;
		}

		DeclarationImporter PopulateImporter()
		{
			var matchedNumber = dataProvider.Importer?.GetBusinessOrIndividualRegistrationNumber();
			var roleCode = matchedNumber?.Type ?? ZString.Empty;

			return new DeclarationImporter
			{
				Id = PopulateImporterID(matchedNumber),
				Name = new ImporterNameTextType { Value = dataProvider.Importer?.CompanyName },
				RoleCode = new CargoWise.Customs.KR.MessageDefinitions.DS.ImporterRoleCodeType { Value = roleCode },
				Address = new DeclarationImporterAddress
				{
					CountrySubDivisionId = dataProvider.Importer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Importer?.RoadNameCode },
					Line = dataProvider.Importer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataProvider.Importer?.AddressLine2 },
					PostcodeId = new AddressPostcodeIdType { Value = dataProvider.Importer?.Postcode },
					BuildingNumber = dataProvider.Importer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataProvider.Importer?.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Importer?.AddressLine1 }
				},
				Contact = new DeclarationImporterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Importer?.RepresentativeName }
				},
				Communication = new Collection<DeclarationImporterCommunication>
					{
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.PhoneNumber }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.FaxNumber }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.Email },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.Email }
						}
					}
			};
		}

		Collection<ImporterIdentificationIdType> PopulateImporterID(IDNumberAndType matchedNumber)
		{
			var result = new Collection<ImporterIdentificationIdType>();
			var importerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var unipassID = dataProvider.Importer?.GetRegistrationNumber(IdentificationType.UnipassIDForIndividual);
			if (!importerIdentificationID.IsEmpty)
			{
				var idNumber = new ImporterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
					Value = importerIdentificationID
				};
				result.Add(idNumber);
			}
			if (!unipassID.IsEmpty())
			{
				var idNumber = new ImporterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
					Value = unipassID
				};
				result.Add(idNumber);
			}
			return result;
		}

		DeclarationManufacturer PopulateManufacturer()
		{
			if (dataProvider.Manufacturer != null)
			{
				return new DeclarationManufacturer
				{
					Name = new ManufacturerNameTextType { Value = dataProvider.Manufacturer.CompanyName },
					Address = new DeclarationManufacturerAddress
					{
						Line = new AddressLineTextType { Value = dataProvider.Manufacturer.AddressLine1 }
					},
					Contact = new DeclarationManufacturerContact
					{
						Name = new ContactNameTextType { Value = dataProvider.Manufacturer.RepresentativeName }
					},
					Communication = new Collection<DeclarationManufacturerCommunication>
					{
						new DeclarationManufacturerCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Manufacturer.PhoneNumber }
						},
						new DeclarationManufacturerCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Manufacturer.FaxNumber }
						}
					}
				};
			}
			return null;
		}

		DeclarationAdditionalDocument PopulateAdditionalDocument()
		{
			return dataProvider.LawCode.IsEmpty && dataProvider.StatementNumber5WN.IsEmpty ? null : new DeclarationAdditionalDocument
			{
				TypeCode = dataProvider.LawCode.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = dataProvider.LawCode },
				Id = dataProvider.StatementNumber5WN.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataProvider.StatementNumber5WN }
			};
		}
	}
}





