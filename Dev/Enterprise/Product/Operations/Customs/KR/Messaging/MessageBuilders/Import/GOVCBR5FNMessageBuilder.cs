using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FN;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FN)]
	public class GOVCBR5FNMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5FNLine dataProvider;
		public GOVCBR5FNMessageBuilder(IImport5FNLine dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				GoodsShipment = PopulateGoodsShipment(),
				Payer = PopulatePayer()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.Header.DeclarationCustomsOffice + dataProvider.Header.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.Header.ImportDeclarationNumber };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5FN };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType { Value = dataProvider.DutyReductionClassification };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.PostClearanceProcedureYN },
			};
		}

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			return (dataProvider == null || dataProvider.Header == null) ? null : new DeclarationGoodsShipment
			{
				SequenceNumeric = Convert.ToDecimal(dataProvider.EntryLineNo),
				Consignment = PopulateConsignment(),
				ExitOffice = dataProvider.ScheduledReExportCustomsOffice.IsEmpty ? null : new DeclarationGoodsShipmentExitOffice
				{
					Id = new ExitOfficeIdentificationIdType { Value = dataProvider.ScheduledReExportCustomsOffice }
				},
				Agent = PopulateAgent(),
				GovernmentAgencyGoodsItem = PopulateGovernmentAgencyGoodsItem(),

				Importer = dataProvider.Header.TypeOfBusiness.IsEmpty ? null : new DeclarationGoodsShipmentImporter
				{
					TypeOfBusiness = new ImporterTypeOfBusinessTextType { Value = dataProvider.Header.TypeOfBusiness }
				},

				Warehouse = PopulateWarehouse()
			};
		}

		DeclarationGoodsShipmentConsignment PopulateConsignment()
		{
			var borderTransportMeans = !dataProvider.ScheduledReExportDate.IsValid ? null : new DeclarationGoodsShipmentConsignmentBorderTransportMeans
			{
				DepartureDateTime = dataProvider.ScheduledReExportDate.ToString(DateFormatType.Date)
			};
			var goodsConsignedPlace = dataProvider.ReExportDestinationCountryCode.IsEmpty ? null : new DeclarationGoodsShipmentConsignmentGoodsConsignedPlace
			{
				Id = new GoodsConsignedPlaceIdentificationIdType { Value = dataProvider.ReExportDestinationCountryCode }
			};

			if (borderTransportMeans == null && goodsConsignedPlace == null)
			{
				return null;
			}

			return new DeclarationGoodsShipmentConsignment
			{
				BorderTransportMeans = borderTransportMeans,
				GoodsConsignedPlace = goodsConsignedPlace
			};
		}

		Collection<DeclarationGoodsShipmentAgent> PopulateAgent()
		{
			if (dataProvider.Header.Payer == null && dataProvider.Header.CustomsBroker == null)
			{
				return null;
			}

			var customsBrokers = new Collection<DeclarationGoodsShipmentAgent>();

			if (dataProvider.Header.Payer != null)
			{
				var item = PopulateAgentRole(dataProvider.Header.Payer, CustomsBrokerRoleCode.Taxpayer);
				customsBrokers.Add(item);
			}

			if (dataProvider.Header.CustomsBroker != null)
			{
				var item = PopulateAgentRole(dataProvider.Header.CustomsBroker, CustomsBrokerRoleCode.Customs);
				customsBrokers.Add(item);
			}
			return customsBrokers;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItem PopulateGovernmentAgencyGoodsItem()
		{
			var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
			{
				AdditionalDocument = PopulateAdditionalDocument(),
				AdditionalInformation = dataProvider.Remark.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
				{
					Content = new AdditionalInformationContentTextType { Value = dataProvider.Remark }
				},
				Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
				{
					IntendedUse = dataProvider.UseCodeDescription.IsEmpty ? null : new CommodityIntendedUseTextType { Value = dataProvider.UseCodeDescription },
					IntendedUseCode = dataProvider.ProductTypeCode.IsEmpty ? null : new CommodityIntendedUseCodeType { Value = dataProvider.ProductTypeCode },
					LotNumberId = dataProvider.SerialNumber.IsEmpty ? null : new CommodityLotNumberIdType { Value = dataProvider.SerialNumber },
					Name = dataProvider.ModelName.IsEmpty ? null : new CommodityNameTextType { Value = dataProvider.ModelName },
					Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
					{
						Id = new ClassificationIdentificationIdType { Value = dataProvider.HSCode }
					},
					ResponsibleGovernmentAgency = dataProvider.JurisdictionalCustomsOffice.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityResponsibleGovernmentAgency
					{
						Id = new ResponsibleGovernmentAgencyIdentificationIdType { Value = dataProvider.JurisdictionalCustomsOffice }
					}
				}
			};
			return item;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument PopulateAdditionalDocument()
		{
			var typeCode = dataProvider.ReductionRateRegulationGroupNumber.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = dataProvider.ReductionRateRegulationGroupNumber };
			var id = dataProvider.ReductionRateRegulationItemNumber.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataProvider.ReductionRateRegulationItemNumber };
			var sequenceNumeric = dataProvider.ReductionRateRegulationSeqNumber.IsEmpty ? 0m : Convert.ToDecimal(dataProvider.ReductionRateRegulationSeqNumber);

			if (typeCode == null && id == null && sequenceNumeric == 0m)
			{
				return null;
			}

			return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument
			{
				TypeCode = typeCode,
				Id = id,
				SequenceNumeric = sequenceNumeric
			};
		}

		DeclarationGoodsShipmentAgent PopulateAgentRole(IOrganization agent, string roleCode)
		{
			Collection<DeclarationGoodsShipmentAgentCommunication> communication = null;

			if (!agent.MobileNumber.IsEmpty)
			{
				communication = new Collection<DeclarationGoodsShipmentAgentCommunication>()
				{
					new DeclarationGoodsShipmentAgentCommunication
					{
						TypeId = new CommunicationTypeIdType { Value = Communication.Phone },
						Id = new CommunicationIdentificationIdType { Value = agent.MobileNumber }
					}
				};
			}

			if (!agent.FaxNumber.IsEmpty)
			{
				(communication = communication ?? new Collection<DeclarationGoodsShipmentAgentCommunication>()).Add(new DeclarationGoodsShipmentAgentCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = Communication.Fax },
					Id = new CommunicationIdentificationIdType { Value = agent.FaxNumber }
				});
			}

			if (!agent.Email.IsEmpty)
			{
				(communication = communication ?? new Collection<DeclarationGoodsShipmentAgentCommunication>()).Add(new DeclarationGoodsShipmentAgentCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = Communication.Email },
					Id = new CommunicationIdentificationIdType { Value = agent.Email }
				});
			}

			var item = new DeclarationGoodsShipmentAgent()
			{
				RoleCode = new CargoWise.Customs.KR.MessageDefinitions.DS.AgentRoleCodeType { Value = roleCode },
				Name = agent.CompanyName.IsEmpty ? null : new AgentNameTextType { Value = agent.CompanyName },
				Communication = communication
			};

			return item;
		}

		DeclarationPayer PopulatePayer()
		{
			var matchedNumber = dataProvider.Header.Payer?.GetBusinessOrIndividualRegistrationNumber() ?? dataProvider.Header.Payer?.GetRegistrationTypeAndNumber(IdentificationType.ForeignCompanyID);
			return new DeclarationPayer
			{
				Id = new PayerIdentificationIdType { Value = matchedNumber?.Number ?? ZString.Empty },
				RoleCode = new PayerRoleCodeType { Value = matchedNumber?.Type ?? ZString.Empty }
			};
		}

		DeclarationGoodsShipmentWarehouse PopulateWarehouse()
		{
			if (dataProvider.GoodsLocation == null)
			{
				return null;
			}
			var warehouse = new DeclarationGoodsShipmentWarehouse()
			{
				Address = PopulateAddress(),
				Communication = dataProvider.GoodsLocation.PhoneNumber.IsEmpty ? null : new DeclarationGoodsShipmentWarehouseCommunication
				{
					Id = new CommunicationIdentificationIdType { Value = dataProvider.GoodsLocation.PhoneNumber }
				}
			};

			return warehouse;
		}

		DeclarationGoodsShipmentWarehouseAddress PopulateAddress()
		{
			var countrySubDivisionID = dataProvider.GoodsLocation.RoadNameCode.IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.GoodsLocation.RoadNameCode };
			var line = dataProvider.GoodsLocation.AddressLine2.IsEmpty ? null : new AddressLineTextType { Value = dataProvider.GoodsLocation.AddressLine2 };
			var postcodeID = dataProvider.GoodsLocation.Postcode.IsEmpty ? null : new AddressPostcodeIdType { Value = dataProvider.GoodsLocation.Postcode };
			var buildingNumber = dataProvider.GoodsLocation.BuildingNumber.IsEmpty ? null : new AddressBuildingNumberTextType { Value = dataProvider.GoodsLocation.BuildingNumber };
			var description = dataProvider.GoodsLocation.AddressLine1.IsEmpty ? null : new AddressDescriptionTextType { Value = dataProvider.GoodsLocation.AddressLine1 };

			if ((countrySubDivisionID ?? line ?? postcodeID ?? buildingNumber ?? (object)description) == null)
			{
				return null;
			}

			return new DeclarationGoodsShipmentWarehouseAddress()
			{
				CountrySubDivisionId = countrySubDivisionID,
				Line = line,
				PostcodeId = postcodeID,
				BuildingNumber = buildingNumber,
				Description = description
			};
		}
	}
}
