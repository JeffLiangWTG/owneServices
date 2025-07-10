using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DP;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5DP)]
	public class GOVCBR5DPMessageBuilder : MessageBuilder<Declaration>
	{
		readonly ILocalExportEntryHeader dataProvider;
		public GOVCBR5DPMessageBuilder(ILocalExportEntryHeader dataProvider)
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
				Consignment = PopulateConsignment(),
				Manufacturer = PopulateManufacturer(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter(),
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
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5DP };
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

		DeclarationConsignment PopulateConsignment()
		{
			return new DeclarationConsignment
			{
				Consignee = new DeclarationConsignmentConsignee
				{
					Id = new ConsigneeIdentificationIdType { Value = dataProvider.Importer?.GetRegistrationNumber(IdentificationType.BusinessRegNo) ?? ZString.Empty },
					Name = new ConsigneeNameTextType { Value = dataProvider.Importer?.CompanyName ?? ZString.Empty },
					Address = PopulateConsigneeAddress(),
					Contact = new DeclarationConsignmentConsigneeContact
					{
						RepresentativeName = new ContactRepresentativeNameTextType { Value = dataProvider.Importer?.RepresentativeName ?? ZString.Empty }
					}
				},
				ConsignmentItem = PopulateConsignmentItem(),
				Warehouse = PopulateConsignmentWarehouse()
			};
		}

		DeclarationConsignmentConsigneeAddress PopulateConsigneeAddress()
		{
			var countrySubDivisionID = dataProvider.Importer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Importer.RoadNameCode };
			var line = dataProvider.Importer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataProvider.Importer.AddressLine2 };
			var postcodeID = dataProvider.Importer?.Postcode.IsEmpty ?? true ? null : new AddressPostcodeIdType { Value = dataProvider.Importer.Postcode };
			var buildingNumber = dataProvider.Importer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataProvider.Importer.BuildingNumber };
			var description = dataProvider.Importer?.AddressLine1.IsEmpty ?? true ? null : new AddressDescriptionTextType { Value = dataProvider.Importer.AddressLine1 };

			if ((countrySubDivisionID ?? line ?? postcodeID ?? (object)buildingNumber ?? description) == null)
			{
				return null;
			}
			return new DeclarationConsignmentConsigneeAddress
			{
				CountrySubDivisionId = countrySubDivisionID,
				Line = line,
				PostcodeId = postcodeID,
				BuildingNumber = buildingNumber,
				Description = description
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
					AdditionalDocument = PopulateConsignmentItemAdditionalDocument(entryLine),
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.InvoiceDescription },
						CountQuantity = new CommodityCountQuantityType
						{
							KcsUnitCode = entryLine.QuantityUnit,
							Value = entryLine.Quantity
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
					PreviousDocument = entryLine.MaterialCode.IsEmpty ? null : new DeclarationConsignmentConsignmentItemPreviousDocument
					{
						Id = new PreviousDocumentIdentificationIdType { Value = entryLine.MaterialCode }
					}
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationConsignmentConsignmentItemAdditionalDocument PopulateConsignmentItemAdditionalDocument(ILocalExportEntryLine entryLine)
		{
			return new DeclarationConsignmentConsignmentItemAdditionalDocument
			{
				TypeCode = new AdditionalDocumentTypeCodeType { Value = entryLine.DocumentType },
				Id = new AdditionalDocumentIdentificationIdType { Value = entryLine.DocumentNo }
			};
		}

		DeclarationConsignmentConsignmentItemCommodityAdditionalDocument PopulateCommodityAdditionalDocument(ILocalExportEntryLine entryLine)
		{
			var id = entryLine.PreviousTransactionReferenceNo.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = entryLine.PreviousTransactionReferenceNo };
			var typeCode = entryLine.PreviousTransactionReferenceNoType.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = entryLine.PreviousTransactionReferenceNoType };

			if (id == null && typeCode == null)
			{
				return null;
			}
			return new DeclarationConsignmentConsignmentItemCommodityAdditionalDocument
			{
				Id = id,
				TypeCode = typeCode
			};
		}

		DeclarationConsignmentWarehouse PopulateConsignmentWarehouse()
		{
			var arrivalDateTime = dataProvider.DeclarationDate == ZDate.Invalid ? null : dataProvider.DeclarationDate.ToString(DateFormatType.Date);
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

		Collection<ManufacturerIdentificationIdType> PopulateManufacturer()
		{
			var unipassID = dataProvider.Manufacturer?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var businessRegNo = dataProvider.Manufacturer?.GetRegistrationNumber(IdentificationType.BusinessRegNo);

			Collection<ManufacturerIdentificationIdType> result = null;

			if (!string.IsNullOrEmpty(unipassID) || !string.IsNullOrEmpty(businessRegNo))
			{
				result = new Collection<ManufacturerIdentificationIdType>();
				if (!string.IsNullOrEmpty(unipassID))
				{
					result.Add(new ManufacturerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = unipassID
					});
				}

				if (!string.IsNullOrEmpty(businessRegNo))
				{
					result.Add(new ManufacturerIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
						Value = businessRegNo
					});
				}
			}
			return result;
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
			var submitterIDs = new Collection<SubmitterIdentificationIdType>();
			var unipassID = dataProvider.Supplier.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var businessRegNo = dataProvider.Supplier.GetRegistrationNumber(IdentificationType.BusinessRegNo);
			if (unipassID != null)
			{
				submitterIDs.Add(new SubmitterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
					Value = unipassID
				});
			}

			if (businessRegNo != null)
			{
				submitterIDs.Add(new SubmitterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
					Value = businessRegNo
				});
			}
			return new DeclarationSubmitter
			{
				Id = submitterIDs,
				RoleCode = new SubmitterRoleCodeType { Value = dataProvider.DrawbackApplicantType }
			};
		}
	}
}
