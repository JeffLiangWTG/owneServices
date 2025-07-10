using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._DHS)]
	public class GOVCBRDHSMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportFTAAmendmentHeader dataHeaderProvider;
		readonly IDHSAmendmentDetails detailsProvider;
		public GOVCBRDHSMessageBuilder(IImportFTAAmendmentHeader dataHeaderProvider, IDHSAmendmentDetails detailsProvider)
		{
			this.dataHeaderProvider = dataHeaderProvider;
			this.detailsProvider = detailsProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				FunctionCode = PopulateDeclarationFunctionCodeType(),
				IssueDateTime = PopulateIssueDateTime(),
				Id = PopulateDeclarationIdentificationIDType(),
				AuthenticationDateTime = PopulateAuthenticationDateTime(),
				TypeCode = PopulateDeclarationTypeCodeType(),
				VersionId = PopulateDeclarationVersionIDType(),
				Reason = PopulateDeclarationReasonTextType(),
				AdditionalInformation = PopulateDeclarationAdditionalInformation(),
				Consignment = PopulateDeclarationConsignment(),
				Importer = PopulateDeclarationImporter(),
				Submitter = PopulateDeclarationSubmitter(),
				Amendment = PopulateDeclarationAmendment(),
				AdditionalDocument = PopulateDeclarationAdditionalDocument()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataHeaderProvider.DeclarationCustomsOffice + dataHeaderProvider.DeclarationCustomsDivision };
		}

		DeclarationFunctionCodeType PopulateDeclarationFunctionCodeType()
		{
			return new DeclarationFunctionCodeType { Value = FunctionCode.Original };
		}

		DeclarationIdentificationIdType PopulateDeclarationIdentificationIDType()
		{
			return new DeclarationIdentificationIdType { Value = dataHeaderProvider.ImportDeclarationNumber };
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateDeclarationTypeCodeType()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._DHS };
		}

		DeclarationVersionIdType PopulateDeclarationVersionIDType()
		{
			return new DeclarationVersionIdType { Value = detailsProvider.AmendmentVersionNo.ToString() };
		}

		string PopulateAuthenticationDateTime()
		{
			return dataHeaderProvider.EntryReleaseDate.IsValid ? dataHeaderProvider.EntryReleaseDate.ToString(DateFormatType.Date) : null;
		}

		DeclarationReasonTextType PopulateDeclarationReasonTextType()
		{
			return new DeclarationReasonTextType { Value = detailsProvider.AmendReasonDescription };
		}

		DeclarationAdditionalDocument PopulateDeclarationAdditionalDocument()
		{
			var typeCode = dataHeaderProvider.LawCode.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = dataHeaderProvider.LawCode };
			var id = dataHeaderProvider.StatementNumber5WN.IsEmpty ? null : new AdditionalDocumentIdentificationIdType { Value = dataHeaderProvider.StatementNumber5WN };

			if (typeCode == null && id == null)
			{
				return null;
			}

			return new DeclarationAdditionalDocument
			{
				TypeCode = typeCode,
				Id = id
			};
		}

		DeclarationAdditionalInformation PopulateDeclarationAdditionalInformation()
		{
			return detailsProvider.AmendmentType.IsEmpty ? null : new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = detailsProvider.AmendmentType }
			};
		}

		DeclarationAmendment PopulateDeclarationAmendment()
		{
			return detailsProvider.AmendmentTypeForInvoiceLine.IsEmpty ? null : new DeclarationAmendment
			{
				ChangeReasonCode = new AmendmentChangeReasonCodeType { Value = detailsProvider.AmendmentTypeForInvoiceLine }
			};
		}

		Collection<DeclarationConsignment> PopulateDeclarationConsignment()
		{
			var amendmentItems = new Collection<DeclarationConsignment>();
			foreach (var item in dataHeaderProvider.Items)
			{
				var items = new DeclarationConsignment
				{
					SequenceNumeric = item.SequenceNo,
					AdditionalInformation = item.AmendType.IsEmpty ? null : new DeclarationConsignmentAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = item.AmendType }
					},
					Amendment = PopulateAmendment(item),
					ConsignmentItem = PopulateConsignmentItem(item)
				};
				amendmentItems.Add(items);
			}
			return amendmentItems;

			DeclarationConsignmentAmendment PopulateAmendment(IImportFTAAmendmentItem item)
			{
				var statementDescription = item.BeforeDescription.IsEmpty ? null : new AmendmentStatementDescriptionTextType { Value = item.BeforeDescription };
				var adjustmentDescription = item.AfterDescription.IsEmpty ? null : new AmendmentAdjustmentDescriptionTextType { Value = item.AfterDescription };
				var pointer = item.DataItemID.IsEmpty ? null : new DeclarationConsignmentAmendmentPointer
				{
					TagId = new PointerTagIdType { Value = item.DataItemID }
				};

				if ((statementDescription ?? adjustmentDescription ?? (object)pointer) == null)
				{
					return null;
				}

				return new DeclarationConsignmentAmendment
				{
					StatementDescription = statementDescription,
					AdjustmentDescription = adjustmentDescription,
					Pointer = pointer,
				};
			}

			DeclarationConsignmentConsignmentItem PopulateConsignmentItem(IImportFTAAmendmentItem item)
			{
				var identityQualifierCode = item.InvoiceLineNo.IsEmpty ? null : new CommodityIdentityQualifierCodeType { Value = item.InvoiceLineNo.ToString() };
				var sequenceID = item.EntryLineNo.IsEmpty ? null : new CommoditySequenceIdentifierIdType { Value = item.EntryLineNo.ToString() };

				if (identityQualifierCode == null && sequenceID == null)
				{
					return null;
				}

				return new DeclarationConsignmentConsignmentItem
				{
					Commodity = new DeclarationConsignmentConsignmentItemCommodity
					{
						IdentityQualifierCode = identityQualifierCode,
						SequenceId = sequenceID
					}
				};
			}
		}

		DeclarationImporter PopulateDeclarationImporter()
		{
			var matchedNumber = dataHeaderProvider.Importer?.GetBusinessOrIndividualRegistrationNumber();

			var importerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var roleCode = matchedNumber?.Type ?? ZString.Empty;
			return new DeclarationImporter
			{
				Id = CreateImporterIDCollection(),
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
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.TelNo },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.PhoneNumber ?? ZString.Empty }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.Fax },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.FaxNumber ?? ZString.Empty }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.Email },
							Id = new CommunicationIdentificationIdType { Value = dataHeaderProvider.Importer?.Email ?? ZString.Empty }
						}
					}
			};

			Collection<ImporterIdentificationIdType> CreateImporterIDCollection()
			{
				var result = new Collection<ImporterIdentificationIdType>();

				result.Add(new ImporterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
					Value = importerIdentificationID
				});

				var unipassID = dataHeaderProvider.Importer?.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForOrganization)?.Number;
				if (!string.IsNullOrEmpty(unipassID))
				{
					result.Add(new ImporterIdentificationIdType
					{
						SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
						Value = unipassID
					});
				}
				return result;
			}
		}

		DeclarationSubmitter PopulateDeclarationSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataHeaderProvider.Declarant?.CompanyName ?? ZString.Empty },
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType { Value = dataHeaderProvider.Declarant?.RepresentativeName ?? ZString.Empty }
				}
			};
		}
	}
}
