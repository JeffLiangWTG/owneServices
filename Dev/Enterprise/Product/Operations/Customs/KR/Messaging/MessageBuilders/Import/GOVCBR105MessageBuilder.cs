using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR105;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._105)]
	public class GOVCBR105MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportFTAAmendmentHeader dataProvider;
		readonly IAmendmentDetails dataDetailsProvider;
		public GOVCBR105MessageBuilder(IImportFTAAmendmentHeader dataProvider, IAmendmentDetails dataDetailsProvider)
		{
			this.dataProvider = dataProvider;
			this.dataDetailsProvider = dataDetailsProvider;
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
				AdditionalDocument = PopulateAdditionalDocument(),
				AdditionalInformation = PopulateDeclarationAdditionalInformation(),
				Consignment = PopulateDeclarationConsignment(),
				Importer = PopulateDeclarationImporter(),
				Submitter = PopulateDeclarationSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationFunctionCodeType PopulateDeclarationFunctionCodeType()
		{
			return new DeclarationFunctionCodeType { Value = Constants.FunctionCode.Original };
		}

		DeclarationIdentificationIdType PopulateDeclarationIdentificationIDType()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateDeclarationTypeCodeType()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._105 };
		}

		DeclarationVersionIdType PopulateDeclarationVersionIDType()
		{
			return new DeclarationVersionIdType { Value = dataDetailsProvider.AmendmentVersionNo.ToString() };
		}

		string PopulateAuthenticationDateTime()
		{
			return !dataProvider.EntryReleaseDate.IsValid ? null : dataProvider.EntryReleaseDate.ToString(DateFormatType.Date);
		}

		DeclarationReasonTextType PopulateDeclarationReasonTextType()
		{
			return new DeclarationReasonTextType { Value = dataDetailsProvider.AmendReasonDescription };
		}

		DeclarationAdditionalDocument PopulateAdditionalDocument()
		{
			return dataProvider.LawCode.IsEmpty && dataProvider.StatementNumber5WN.IsEmpty ? null : new DeclarationAdditionalDocument
			{
				TypeCode = dataProvider.LawCode.IsEmpty ? null : new AdditionalDocumentTypeCodeType { Value = dataProvider.LawCode },
				Id = dataProvider.StatementNumber5WN.IsEmpty ? null :  new AdditionalDocumentIdentificationIdType { Value = dataProvider.StatementNumber5WN }
			};
		}

		DeclarationAdditionalInformation PopulateDeclarationAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataDetailsProvider.AmendmentType }
			};
		}

		Collection<DeclarationConsignment> PopulateDeclarationConsignment()
		{
			var amendmentItems = new Collection<DeclarationConsignment>();
			foreach (var item in dataProvider.Items)
			{
				var items = new DeclarationConsignment
				{
					SequenceNumeric = item.EntryLineNo.IsDefault ? null : (decimal?)item.EntryLineNo,
					AdditionalDocument = new DeclarationConsignmentAdditionalDocument
					{
						SequenceNumeric = item.SequenceNo
					},
					AdditionalInformation = item.AmendType.IsEmpty ? null : new DeclarationConsignmentAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = item.AmendType }
					},
					Amendment = PopulateAmendment(item),
				};
				amendmentItems.Add(items);
			}
			return amendmentItems;
		}

		DeclarationConsignmentAmendment PopulateAmendment(IImportFTAAmendmentItem item)
		{
			if (item.BeforeDescription.IsEmpty && item.AfterDescription.IsEmpty && item.DataItemID.IsEmpty)
			{
				return null;
			}

			return new DeclarationConsignmentAmendment
			{
				StatementDescription = item.BeforeDescription.IsEmpty ? null : new AmendmentStatementDescriptionTextType { Value = item.BeforeDescription },
				AdjustmentDescription = item.AfterDescription.IsEmpty ? null : new AmendmentAdjustmentDescriptionTextType { Value = item.AfterDescription },
				Pointer = item.DataItemID.IsEmpty ? null : new DeclarationConsignmentAmendmentPointer
				{
					TagId = new PointerTagIdType { Value = item.DataItemID }
				}
			};
		}

		DeclarationImporter PopulateDeclarationImporter()
		{
			var matchedNumber = dataProvider.Importer?.GetBusinessOrIndividualRegistrationNumber();

			var importerIdentificationID = matchedNumber?.Number ?? ZString.Empty;
			var roleCode = matchedNumber?.Type ?? ZString.Empty;
			return new DeclarationImporter
			{
				Id = new Collection<ImporterIdentificationIdType>
					{
						new ImporterIdentificationIdType
						{
							SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
							Value = importerIdentificationID
						},
						new ImporterIdentificationIdType
						{
							SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
							Value = dataProvider.Importer?.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForOrganization)?.Number ?? ZString.Empty
						}
					},
				Name = new ImporterNameTextType { Value = dataProvider.Importer?.CompanyName },
				RoleCode = new CargoWise.Customs.KR.MessageDefinitions.DS.ImporterRoleCodeType { Value = roleCode },
				Address = new DeclarationImporterAddress
				{
					CountrySubDivisionId = dataProvider.Importer?.RoadNameCode.IsEmpty ?? true ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Importer.RoadNameCode },
					Line = dataProvider.Importer?.AddressLine2.IsEmpty ?? true ? null : new AddressLineTextType { Value = dataProvider.Importer.AddressLine2 },
					PostcodeId = new AddressPostcodeIdType { Value = dataProvider.Importer?.Postcode },
					BuildingNumber = dataProvider.Importer?.BuildingNumber.IsEmpty ?? true ? null : new AddressBuildingNumberTextType { Value = dataProvider.Importer.BuildingNumber },
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
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.TelNo },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.PhoneNumber }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.Fax },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.FaxNumber }
						},
						new DeclarationImporterCommunication
						{
							TypeId = new CommunicationTypeIdType { Value = Constants.Communication.Email },
							Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer?.Email }
						}
					}
			};
		}

		DeclarationSubmitter PopulateDeclarationSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant.CompanyName },
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Declarant.RepresentativeName }
				}
			};
		}
	}
}

