using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR008;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._008)]
	public class GOVCBR008MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport008Header dataProvider;
		public GOVCBR008MessageBuilder(IImport008Header dataProvider)
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
				AdditionalInformation = PopulateAdditionalInformation(),
				Agent = PopulateAgent(),
				Carrier = PopulateCarrier(),
				Consignment = PopulateConsignment(),
				GoodsShipment = PopulateGoodsShipment(),
				LoadingLocation = PopulateLoadingLocation(),
				Submitter = PopulateSubmitter(),
				TransportContractDocument = PopulateTransportContractDocument(),
				TransitDestination = PopulateTransitDestination(),
				UnloadingLocation = PopulateUnloadingLocation()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			if (dataProvider.DeclarationCustomsOffice.IsEmpty || dataProvider.DeclarationCustomsDivision.IsEmpty)
			{
				return null;
			}
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._008 };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				LimitDateTime = dataProvider.TransportationStartDate.IsValid ? dataProvider.TransportationStartDate.ToString(DateFormatType.Date) : null,
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.DecType },
				BeginningDateTime = (dataProvider.Owner?.ScheduledStartDateInKR.IsValid ?? false) ? dataProvider.Owner?.ScheduledStartDateInKR.ToString(DateFormatType.Date) : string.Empty,
				EndingDateTime = (dataProvider.Owner?.ScheduledEndDateInKR.IsValid ?? false) ? dataProvider.Owner?.ScheduledEndDateInKR.ToString(DateFormatType.Date) : string.Empty
			};
		}

		Collection<DeclarationAgent> PopulateAgent()
		{
			if (dataProvider.FamilyMembers?.Any() ?? false)
			{
				var familyMembers = new Collection<DeclarationAgent>();
				foreach (var familyMember in dataProvider.FamilyMembers)
				{
					var item = new DeclarationAgent
					{
						Id = familyMember.PassportNumber.IsEmpty ? null : new AgentIdentificationIdType { Value = familyMember.PassportNumber },
						Name = familyMember.Name.IsEmpty ? null : new AgentNameTextType { Value = familyMember.Name },
						Contact = PopulateContact(familyMember),
						AdditionalInformation = PopulateAdditionalInformation(familyMember)
					};
					familyMembers.Add(item);
				}
				return familyMembers;
			}

			return null;

			DeclarationAgentContact PopulateContact(IImport008Person familyMember)
			{
				var birthDate = familyMember.BirthDate.IsValid ? familyMember.BirthDate.ToString(DateFormatType.Date) : null;
				var relationship = familyMember.RelationshipToImporter.IsEmpty ? null : new ContactRelationshipTextType { Value = familyMember.RelationshipToImporter };
				var occupation = familyMember.JobCode.IsEmpty ? null : new ContactOccupationTextType { Value = familyMember.JobCode };

				if ((birthDate ?? relationship ?? (object)occupation) == null)
				{
					return null;
				}

				return new DeclarationAgentContact
				{
					BirthDate = birthDate,
					Relationship = relationship,
					Occupation = occupation
				};
			}

			DeclarationAgentAdditionalInformation PopulateAdditionalInformation(IImport008Person familyMember)
			{
				var statementCode = familyMember.EntryToKR_YN.IsEmpty ? null : new AdditionalInformationStatementCodeType { Value = familyMember.EntryToKR_YN };
				var beginningDateTime = familyMember.ScheduledStartDateInKR.IsValid ? familyMember.ScheduledStartDateInKR.ToString(DateFormatType.Date) : null;
				var endingDateTime = familyMember.ScheduledEndDateInKR.IsValid ? familyMember.ScheduledEndDateInKR.ToString(DateFormatType.Date) : null;

				if ((statementCode ?? beginningDateTime ?? (object)endingDateTime) == null)
				{
					return null;
				}

				return new DeclarationAgentAdditionalInformation
				{
					StatementCode = statementCode,
					BeginningDateTime = beginningDateTime,
					EndingDateTime = endingDateTime
				};
			}
		}

		Collection<DeclarationCarrier> PopulateCarrier()
		{
			return new Collection<DeclarationCarrier>()
			{
				new DeclarationCarrier
				{
					RoleCode = new CarrierRoleCodeType { Value = "1" },
					Name = new CarrierNameTextType { Value = dataProvider.DomesticCarrier }
				},
				new DeclarationCarrier
				{
					RoleCode = new CarrierRoleCodeType { Value = "2" },
					Name = new CarrierNameTextType { Value = dataProvider.ForeignCarrier }
				}
			};
		}

		Collection<DeclarationConsignment> PopulateConsignment()
		{
			if (dataProvider.Lines?.Any() ?? false)
			{
				var lines = new Collection<DeclarationConsignment>();
				foreach (var line in dataProvider.Lines)
				{
					var item = new DeclarationConsignment
					{
						ConsignmentItem = new DeclarationConsignmentConsignmentItem
						{
							Commodity = new DeclarationConsignmentConsignmentItemCommodity
							{
								CargoDescription = new CommodityCargoDescriptionTextType { Value = line.InvoiceDescription },
								CharacteristicCode = new CommodityCharacteristicCodeType { Value = line.ItemCategory },
								CountQuantity = line.Quantity.IsEmpty ? null : new CommodityCountQuantityType { Value = line.Quantity },
								Description = line.Model.IsEmpty ? null : new CommodityDescriptionTextType { Value = line.Model },
								Name = line.BrandName.IsEmpty ? null : new CommodityNameTextType { Value = line.BrandName },
								ValueAmount = line.Price.IsEmpty ? null : new CommodityValueAmountType { Value = line.Price },
								AdditionalInformation = line.MonthOfUse.IsEmpty ? null : new DeclarationConsignmentConsignmentItemCommodityAdditionalInformation
								{
									Content = new AdditionalInformationContentTextType { Value = line.MonthOfUse.ToString() }
								},
								Classification = line.ItemCode.IsEmpty ? null : new DeclarationConsignmentConsignmentItemCommodityClassification
								{
									Id = new ClassificationIdentificationIdType { Value = line.ItemCode }
								}
							}
						}
					};
					lines.Add(item);
				}
				return lines;
			}

			return null;
		}

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			return new DeclarationGoodsShipment
			{
				AdditionalInformation = PopulateGoodsShipmentAdditionalInformation(),
				CustomsValuation = new DeclarationGoodsShipmentCustomsValuation
				{
					FreightChargeAmount = new CustomsValuationFreightChargeAmountType { Value = dataProvider.Freight }
				},
				GovernmentAgencyGoodsItem = PopulateGovernmentAgencyGoodsItem()
			};
		}

		Collection<DeclarationGoodsShipmentAdditionalInformation> PopulateGoodsShipmentAdditionalInformation()
		{
			var answers = new Collection<DeclarationGoodsShipmentAdditionalInformation>();
			foreach (var answer in dataProvider.QuestionsAndAnswers)
			{
				var item = new DeclarationGoodsShipmentAdditionalInformation
				{
					StatementCode = new AdditionalInformationStatementCodeType { Value = answer.QuestionID },
					StatementTypeCode = new AdditionalInformationStatementTypeCodeType { Value = answer.Answer }
				};
				answers.Add(item);
			}
			return answers;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGovernmentAgencyGoodsItem()
		{
			if (dataProvider.Vehicle == null)
			{
				return null;
			}

			var additionalInformation = dataProvider.Vehicle.Type.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataProvider.Vehicle.Type }
			};
			var commodity = PopulateCommodity(dataProvider.Vehicle);
			var origin = dataProvider.Vehicle.ManufacturingCountry.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
			{
				CountryCode = new OriginCountryCodeType { Value = dataProvider.Vehicle.ManufacturingCountry }
			};

			if ((commodity ?? (object)origin) == null)
			{
				return null;
			}

			var result = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
			{
				AdditionalInformation = additionalInformation,
				Commodity = commodity,
				Origin = origin,
			};
			result.Add(item);
			return result;

			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity PopulateCommodity(IImport008BulkItem bulkItem)
			{
				var description = bulkItem.EngineDisplacement.IsEmpty ? null : new CommodityDescriptionTextType { Value = bulkItem.EngineDisplacement.ToString() };
				var id = bulkItem.IdentificationNumber.IsEmpty ? null : new CommodityIdentificationIdType { Value = bulkItem.IdentificationNumber };
				var manufactureDateTime = bulkItem.ModelYear.IsEmpty ? null : bulkItem.ModelYear.ToString();
				var name = bulkItem.ModelName.IsEmpty ? null : new CommodityNameTextType { Value = bulkItem.ModelName };
				var additionalInformation = PopulateAdditionalInformation(bulkItem);

				if ((description ?? id ?? manufactureDateTime ?? name ?? (object)additionalInformation) == null)
				{
					return null;
				}

				return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
				{
					Description = description,
					Id = id,
					ManufactureDateTime = manufactureDateTime,
					Name = name,
					AdditionalInformation = additionalInformation,
				};
			}

			DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation PopulateAdditionalInformation(IImport008BulkItem bulkItem)
			{
				var content = bulkItem.SeatingCapacity.IsEmpty ? null : new AdditionalInformationContentTextType { Value = bulkItem.SeatingCapacity.ToString() };
				var limitDateTime = !bulkItem.CurrentRegistrationDate.IsValid ? null : bulkItem.CurrentRegistrationDate.ToString(DateFormatType.Date);
				var registerDateTime = !bulkItem.FirstRegistrationDate.IsValid ? null : bulkItem.FirstRegistrationDate.ToString(DateFormatType.Date);

				if ((content ?? limitDateTime ?? (object)registerDateTime) == null)
				{
					return null;
				}

				return new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation
				{
					Content = content,
					LimitDateTime = limitDateTime,
					RegisterDateTime = registerDateTime
				};
			}
		}

		DeclarationLoadingLocation PopulateLoadingLocation()
		{
			return new DeclarationLoadingLocation
			{
				Id = new LoadingLocationIdentificationIdType { Value = dataProvider.LoadingPort }
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType { Value = dataProvider.Owner?.PassportNumber },
				Name = new SubmitterNameTextType { Value = dataProvider.Owner?.Name },
				RoleCode = new SubmitterRoleCodeType { Value = dataProvider.Owner?.NationalityClassCode },
				Address = new DeclarationSubmitterAddress
				{
					CountryCode = new AddressCountryCodeType { Value = dataProvider.Owner?.Nationality },
					CountrySubDivisionId = (dataProvider.Declarant?.RoadNameCode.IsEmpty ?? true) ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Declarant?.RoadNameCode },
					Line = (dataProvider.Declarant?.AddressLine2.IsEmpty ?? true) ? null : new AddressLineTextType { Value = dataProvider.Declarant?.AddressLine2 },
					PostcodeId = (dataProvider.Declarant?.Postcode.IsEmpty ?? true) ? null : new AddressPostcodeIdType { Value = dataProvider.Declarant?.Postcode },
					BuildingNumber = (dataProvider.Declarant?.BuildingNumber.IsEmpty ?? true) ? null : new AddressBuildingNumberTextType { Value = dataProvider.Declarant?.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Declarant?.AddressLine1 }
				},
				Contact = new DeclarationSubmitterContact
				{
					BirthDate = (dataProvider.Owner?.BirthDate.IsValid ?? false) ? dataProvider.Owner?.BirthDate.ToString(DateFormatType.Date) : null,
					Occupation = new ContactOccupationTextType { Value = dataProvider.Owner?.JobCode }
				},
				Communication = PopulateSubmitterCommunication()
			};
		}

		Collection<DeclarationSubmitterCommunication> PopulateSubmitterCommunication()
		{
			Collection<DeclarationSubmitterCommunication> communications = null;
			if (!(dataProvider.Declarant?.PhoneNumber.IsEmpty ?? true))
			{
				communications = communications ?? new Collection<DeclarationSubmitterCommunication>();
				communications.Add(new DeclarationSubmitterCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = "TE" },
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant?.PhoneNumber }
				});
			}

			if (!(dataProvider.Declarant?.Email.IsEmpty ?? true))
			{
				communications = communications ?? new Collection<DeclarationSubmitterCommunication>();
				communications.Add(new DeclarationSubmitterCommunication
				{
					TypeId = new CommunicationTypeIdType { Value = "EM" },
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant?.Email }
				});
			}
			return communications;
		}

		DeclarationTransportContractDocument PopulateTransportContractDocument()
		{
			return dataProvider.HBL.IsEmpty ? null : new DeclarationTransportContractDocument
			{
				Id = new TransportContractDocumentIdentificationIdType { Value = dataProvider.HBL }
			};
		}

		DeclarationTransitDestination PopulateTransitDestination()
		{
			return new DeclarationTransitDestination
			{
				Id = new TransitDestinationIdentificationIdType { Value = dataProvider.ForeignCountry },
				Name = new TransitDestinationNameTextType { Value = dataProvider.ForeignCity }
			};
		}

		DeclarationUnloadingLocation PopulateUnloadingLocation()
		{
			return dataProvider.TransportationArrivalDate.IsValid ? new DeclarationUnloadingLocation
			{
				ArrivalDateTime = dataProvider.TransportationArrivalDate.ToString(DateFormatType.Date)
			} : null;
		}
	}
}





