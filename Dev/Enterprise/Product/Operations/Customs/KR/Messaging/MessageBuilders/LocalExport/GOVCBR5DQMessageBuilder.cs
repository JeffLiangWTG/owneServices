using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DQ;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._5DQ)]
	public class GOVCBR5DQMessageBuilder : MessageBuilder<Declaration>
	{
		readonly ILocalExportEntryHeader dataProvider;
		public GOVCBR5DQMessageBuilder(ILocalExportEntryHeader dataProvider)
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
				TotalGrossMassMeasure = PopulateTotalGrossMassMeasure(),
				TotalPackageQuantity = PopulateTotalPackageQuantity(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = PopulateAgent(),
				BorderTransportMeans = PopulateBorderTransportMeans(),
				Consignment = PopulateConsignment(),
				GoodsShipment = PopulateGoodsShipment(),
				Manufacturer = PopulateManufacturer(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
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
			return new DeclarationIdentificationIdType { Value = dataProvider.DeclarationNumber };
		}

		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType { Value = dataProvider.TotalDeclarationAmount };
		}

		DeclarationTotalGrossMassMeasureType PopulateTotalGrossMassMeasure()
		{
			return new DeclarationTotalGrossMassMeasureType { Value = dataProvider.TotalGrossWeight };
		}

		DeclarationTotalPackageQuantityType PopulateTotalPackageQuantity()
		{
			return new DeclarationTotalPackageQuantityType { Value = dataProvider.TotalPackages };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5DQ };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.DeclarationType };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType
				{
					Value = dataProvider.GoodsType
				}
			};
		}

		DeclarationAgent PopulateAgent()
		{
			var regNo = dataProvider.Exporter?.GetRegistrationNumber(IdentificationType.BusinessRegNo);
			if (string.IsNullOrEmpty(regNo))
			{
				return null;
			}
			return new DeclarationAgent
			{
				Id = new AgentIdentificationIdType { Value = regNo }
			};
		}

		DeclarationBorderTransportMeans PopulateBorderTransportMeans()
		{
			var crewQuantity = dataProvider.CrewCount.IsEmpty ? null : new BorderTransportMeansCrewQuantityType { Value = dataProvider.CrewCount };
			var id = dataProvider.VesselRadioCallSign.IsEmpty ? null : new BorderTransportMeansIdentificationIdType { Value = dataProvider.VesselRadioCallSign };
			var name = dataProvider.FlightNoOrVesselName.IsEmpty ? null : new BorderTransportMeansNameTextType { Value = dataProvider.FlightNoOrVesselName };
			var voyageDatesNumeric = dataProvider.ScheduledSailingDays.IsEmpty ? (decimal?)null : dataProvider.ScheduledSailingDays;
			var personOnBoard = PopulatePersonOnBoard();

			if ((crewQuantity ?? id ?? name ?? voyageDatesNumeric ?? (object)personOnBoard) == null)
			{
				return null;
			}

			return new DeclarationBorderTransportMeans
			{
				CrewQuantity = crewQuantity,
				Id = id,
				Name = name,
				VoyageDatesNumeric = voyageDatesNumeric,
				PersonOnBoard = personOnBoard,
			};
		}

		Collection<DeclarationBorderTransportMeansPersonOnBoard> PopulatePersonOnBoard()
		{
			var stevedores = new Collection<DeclarationBorderTransportMeansPersonOnBoard>();
			foreach (var stevedore in dataProvider.Stevedores)
			{
				var item = new DeclarationBorderTransportMeansPersonOnBoard
				{
					SequenceNumeric = stevedore.SequenceNo,
					GivenName = new PersonOnBoardGivenNameTextType { Value = stevedore.FullName },
					BirthDateTime = stevedore.Birthday.ToString(DateFormatType.Date),
					Address = new DeclarationBorderTransportMeansPersonOnBoardAddress
					{
						CountrySubDivisionId = stevedore.RoadNameCode.IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = stevedore.RoadNameCode },
						Line = stevedore.AddressLine2.IsEmpty ? null : new AddressLineTextType { Value = stevedore.AddressLine2 },
						PostcodeId = stevedore.Postcode.IsEmpty ? null : new AddressPostcodeIdType { Value = stevedore.Postcode },
						BuildingNumber = stevedore.BuildingNumber.IsEmpty ? null : new AddressBuildingNumberTextType { Value = stevedore.BuildingNumber },
						Description = new AddressDescriptionTextType { Value = stevedore.AddressLine1 }
					}
				};
				stevedores.Add(item);
			}
			return stevedores;
		}

		DeclarationConsignment PopulateConsignment()
		{
			return new DeclarationConsignment
			{
				Consignee = PopulateConsignee(),
				ConsignmentItem = PopulateConsignmentItem(),
				Warehouse = PopulateWarehouse()
			};
		}

		DeclarationConsignmentWarehouse PopulateWarehouse()
		{
			var arrivalDateTime = dataProvider.DeclarationDate.IsValid ? dataProvider.DeclarationDate.ToString(DateFormatType.Date) : null;
			var name = dataProvider.BondedAreaCode.IsEmpty ? null : new WarehouseNameTextType { Value = dataProvider.BondedAreaCode };

			if (((object)arrivalDateTime ?? name) == null)
			{
				return null;
			}

			return new DeclarationConsignmentWarehouse
			{
				ArrivalDateTime = arrivalDateTime,
				Name = name
			};
		}

		DeclarationConsignmentConsignee PopulateConsignee()
		{
			var countrySubDivisionID = dataProvider.Importer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Importer.RoadNameCode };
			var line = dataProvider.Importer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataProvider.Importer.AddressLine2 };
			var postcodeID = dataProvider.Importer?.Postcode.IsEmpty ?? true ? null : new AddressPostcodeIdType { Value = dataProvider.Importer.Postcode };
			var buildingNumber = dataProvider.Importer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataProvider.Importer.BuildingNumber };
			var description = dataProvider.Importer?.AddressLine1.IsEmpty ?? true ? null : new AddressDescriptionTextType { Value = dataProvider.Importer.AddressLine1 };

			var tester = countrySubDivisionID ?? line ?? postcodeID ?? (object)buildingNumber ?? description;

			var address = tester == null ? null : new DeclarationConsignmentConsigneeAddress
			{
				CountrySubDivisionId = countrySubDivisionID,
				Line = line,
				PostcodeId = postcodeID,
				BuildingNumber = buildingNumber,
				Description = description
			};

			return new DeclarationConsignmentConsignee
			{
				Id = new ConsigneeIdentificationIdType { Value = dataProvider.Importer?.GetRegistrationNumber(IdentificationType.BusinessRegNo) ?? ZString.Empty },
				Name = new ConsigneeNameTextType { Value = dataProvider.Importer?.CompanyName ?? ZString.Empty },
				Address = address,
				Contact = new DeclarationConsignmentConsigneeContact
				{
					RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.Importer?.RepresentativeName ?? ZString.Empty }
				}
			};
		}

		Collection<DeclarationConsignmentConsignmentItem> PopulateConsignmentItem()
		{
			var entryLines = new Collection<DeclarationConsignmentConsignmentItem>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationConsignmentConsignmentItem
				{
					SequenceNumeric = entryLine.EntryLineNo,
					AdditionalDocument = new DeclarationConsignmentConsignmentItemAdditionalDocument
					{
						TypeCode = new AdditionalDocumentTypeCodeType { Value = entryLine.DocumentType },
						Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.DocumentNo }
					},
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.InvoiceDescription },
						CountQuantity = new CommodityCountQuantityType
						{
							Value = entryLine.Quantity,
							KcsUnitCode = entryLine.QuantityUnit
						},
						Id = entryLine.GoodsNo.IsEmpty ? null : new CommodityIdentificationIdType { Value = entryLine.GoodsNo },
						JurisdictionDateTime = entryLine.InboundDate.IsValid ? entryLine.InboundDate.ToString(DateFormatType.Date) : null,
						ValueAmount = new CommodityValueAmountType { Value = entryLine.FOBAmount },
						AdditionalDocument = PopulateCommodityAdditionalDocument(entryLine),
						Classification = new DeclarationConsignmentConsignmentItemCommodityClassification
						{
							Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode }
						}
					},
					GoodsMeasure = new DeclarationConsignmentConsignmentItemGoodsMeasure
					{
						NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = entryLine.NetWeight }
					},
					Packaging = new DeclarationConsignmentConsignmentItemPackaging
					{
						QuantityQuantity = new PackagingQuantityQuantityType { Value = entryLine.Packages },
						TypeCode = new PackagingTypeCodeType { Value = entryLine.PackagesType }
					},
					PreviousDocument = PopulateConsignmentPreviousDocument(entryLine)
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationConsignmentConsignmentItemCommodityAdditionalDocument PopulateCommodityAdditionalDocument(ILocalExportEntryLine entryLine)
		{
			var id = entryLine.PreviousTransactionReferenceNo.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = entryLine.PreviousTransactionReferenceNo };
			var typeCode = entryLine.PreviousTransactionReferenceNoType.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = entryLine.PreviousTransactionReferenceNoType };

			if (((object)id ?? typeCode) == null)
			{
				return null;
			}
			return new DeclarationConsignmentConsignmentItemCommodityAdditionalDocument
			{
				Id = id,
				TypeCode = typeCode,
			};
		}

		DeclarationConsignmentConsignmentItemPreviousDocument PopulateConsignmentPreviousDocument(ILocalExportEntryLine entryLine)
		{
			return entryLine.MaterialCode.IsEmpty ? null : new DeclarationConsignmentConsignmentItemPreviousDocument
			{
				Id = new PreviousDocumentIdentificationIdType { Value = entryLine.MaterialCode }
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			if (!dataProvider.OtherTransportMeans.Any())
			{
				return null;
			}

			var otherTransportMeans = new Collection<DeclarationGoodsShipment>();
			foreach (var otherTransportMean in dataProvider.OtherTransportMeans)
			{
				var item = new DeclarationGoodsShipment
				{
					SequenceNumeric = otherTransportMean.SequenceNo,
					Consignment = PopulateGoodsShipmentConsignment(otherTransportMean)
				};
				otherTransportMeans.Add(item);
			}
			return otherTransportMeans;
		}

		DeclarationGoodsShipmentConsignment PopulateGoodsShipmentConsignment(ILocalExportOtherTransportMeans otherTransportMean)
		{
			DeclarationGoodsShipmentConsignment result = null;

			if (!otherTransportMean.TransportVehicleRegNo.IsEmpty || !otherTransportMean.WorkingVesselLloydsNumber.IsEmpty || !otherTransportMean.WorkingVesselName.IsEmpty)
			{
				Collection<BorderTransportMeansIdentificationIdType> ids = null;

				if (!otherTransportMean.TransportVehicleRegNo.IsEmpty)
				{
					ids = new Collection<BorderTransportMeansIdentificationIdType>();
					ids.Add(new BorderTransportMeansIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Mlt,
						Value = otherTransportMean.TransportVehicleRegNo
					});
				}
				if (!otherTransportMean.WorkingVesselLloydsNumber.IsEmpty)
				{
					if (ids == null)
					{
						ids = new Collection<BorderTransportMeansIdentificationIdType>();
					}
					ids.Add(new BorderTransportMeansIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item54,
						Value = otherTransportMean.WorkingVesselLloydsNumber
					});
				}

				result = new DeclarationGoodsShipmentConsignment
				{
					BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans
					{
						Id = ids,
						Name = otherTransportMean.WorkingVesselName.IsEmpty ? null : new BorderTransportMeansNameTextType
						{
							Value = otherTransportMean.WorkingVesselName
						}
					}
				};
			}
			return result;
		}

		Collection<ManufacturerIdentificationIdType> PopulateManufacturer()
		{
			Collection<ManufacturerIdentificationIdType> manufacturer = null;

			var unipassID = dataProvider.Manufacturer?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);

			if (!string.IsNullOrEmpty(unipassID))
			{
				var id = new ManufacturerIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
					Value = unipassID
				};

				manufacturer = new Collection<ManufacturerIdentificationIdType> { id };
			}

			var businessRegNo = dataProvider.Manufacturer?.GetRegistrationNumber(IdentificationType.BusinessRegNo);

			if (!string.IsNullOrEmpty(businessRegNo))
			{
				var item = new ManufacturerIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
					Value = businessRegNo
				};

				if (manufacturer == null)
				{
					manufacturer = new Collection<ManufacturerIdentificationIdType>();
				}

				manufacturer.Add(item);
			}

			return manufacturer;
		}

		DeclarationPreviousDocument PopulatePreviousDocument()
		{
			return dataProvider.MRNNo.IsEmpty ? null : new DeclarationPreviousDocument
			{
				Id = new PreviousDocumentIdentificationIdType { Value = dataProvider.MRNNo }
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Id = new Collection<SubmitterIdentificationIdType>
				{
					new SubmitterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = dataProvider.Supplier?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization)
					},
					new SubmitterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
						Value = dataProvider.Supplier?.GetRegistrationNumber(IdentificationType.BusinessRegNo)
					}
				},
				RoleCode = new SubmitterRoleCodeType { Value = dataProvider.DrawbackApplicantType }
			};
		}
	}
}
