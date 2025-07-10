using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UA;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UA)]
	public class GOVCBR5UAMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5UAHeader dataProvider;
		public GOVCBR5UAMessageBuilder(IImport5UAHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				InvoiceAmount = PopulateInvoiceAmount(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				VersionId = PopulateVersionID(),
				SubTypeCode = PopulateSubTypeCode(),
				Reason = PopulateReason(),
				ReasonCode = PopulateReasonCode(),
				Amendment = PopulateAmendment(),
				Submitter = PopulateSubmitter(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}
		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}
		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType { Value = dataProvider.PenaltyExemptionAmount };
		}
		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}
		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5UA };
		}
		DeclarationVersionIdType PopulateVersionID()
		{
			return new DeclarationVersionIdType { Value = dataProvider.SequenceNo.ToString() };
		}
		DeclarationSubTypeCodeType PopulateSubTypeCode()
		{
			return new DeclarationSubTypeCodeType { Value = dataProvider.PenaltyType };
		}
		DeclarationReasonTextType PopulateReason()
		{
			return dataProvider.PenaltyExemptionReason.IsEmpty ? null : new DeclarationReasonTextType { Value = dataProvider.PenaltyExemptionReason };
		}
		DeclarationReasonCodeType PopulateReasonCode()
		{
			return new DeclarationReasonCodeType { Value = dataProvider.PenaltyExemptionReasonsCode };
		}
		DeclarationAmendment PopulateAmendment()
		{
			return new DeclarationAmendment
			{
				ChangeReasonCode = new AmendmentChangeReasonCodeType() { Value = dataProvider.ExemptionProcessCode },
				AmendmentDateTime = !dataProvider.AmendmentDeclarationDate.IsValid ? string.Empty : dataProvider.AmendmentDeclarationDate.ToString(DateFormatType.Date),
				Pointer = new DeclarationAmendmentPointer()
				{
					SequenceNumeric = dataProvider.AmendmentVersionNo
				}
			};
		}
		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType() { Value = dataProvider.Declarant?.CompanyName },
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Declarant?.RepresentativeName }
				}
			};
		}
	}
}
