using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRDKJ;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._DKJ)]
	public class GOVCBRDKJMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IExportCancellationHeader dataHeaderProvider;
		readonly IAmendmentDetails dataAmendProvider;
		public GOVCBRDKJMessageBuilder(IExportCancellationHeader dataHeaderProvider, IAmendmentDetails dataAmendProvider)
		{
			this.dataHeaderProvider = dataHeaderProvider;
			this.dataAmendProvider = dataAmendProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				TypeCode = PopulateDeclarationTypeCodeType(),
				VersionId = PopulateDeclarationVersionIDType(),
				TransactionNatureCode = PopulateDeclarationTransactionNatureCodeType(),
				Reason = PopulateDeclarationReasonTextType(),
				AdditionalInformation = PopulateDeclarationAdditionalInformation(),
				Exporter = PopulateDeclarationExporter(),
				Submitter = PopulateDeclarationSubmitter(),
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataHeaderProvider.DeclarationCustomsOffice + dataHeaderProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataHeaderProvider.ExportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateDeclarationTypeCodeType()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._DKJ };
		}

		DeclarationVersionIdType PopulateDeclarationVersionIDType()
		{
			return new DeclarationVersionIdType { Value = dataAmendProvider.AmendmentVersionNo.ToString() };
		}

		DeclarationTransactionNatureCodeType PopulateDeclarationTransactionNatureCodeType()
		{
			return new DeclarationTransactionNatureCodeType
			{
				Value = _5ASAmendmentType.Codes.Cancellation
			};
		}

		DeclarationReasonTextType PopulateDeclarationReasonTextType()
		{
			return new DeclarationReasonTextType { Value = dataAmendProvider.FaultParty };
		}

		DeclarationAdditionalInformation PopulateDeclarationAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataAmendProvider.ReasonCode },
				StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = dataAmendProvider.AmendReasonDescription }
			};
		}

		DeclarationExporter PopulateDeclarationExporter()
		{
			var matchedNumber = dataHeaderProvider.Exporter?.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForOrganization);
			return new DeclarationExporter
			{
				Id = string.IsNullOrEmpty(matchedNumber?.Number) ? null : new ExporterIdentificationIdType { Value = matchedNumber.Number },
				Name = new ExporterNameTextType { Value = dataHeaderProvider.Exporter?.CompanyName ?? "" }
			};
		}

		DeclarationSubmitter PopulateDeclarationSubmitter()
		{
			if (!dataHeaderProvider.UnipassDeclarantID.IsEmpty)
			{
				return new DeclarationSubmitter
				{
					Id = new SubmitterIdentificationIdType { Value = dataHeaderProvider.UnipassDeclarantID },
				};
			}
			return null;
		}
	}
}
