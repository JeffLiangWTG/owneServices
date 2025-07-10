using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BD;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BD)]
	public class GOVCBR5BDMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5BDHeader dataProvider;
		public GOVCBR5BDMessageBuilder(IImport5BDHeader dataProvider)
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
				Reason = PopulateReason(),
				ReasonCode = PopulateReasonCode(),
				AdditionalInformation = PopulateAdditionalInformation(),
				ObligationGuarantee = PopulateObligationGuarantee(),
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5BD };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType { Value = dataProvider.RequestReason };
		}

		DeclarationReasonCodeType PopulateReasonCode()
		{
			return new DeclarationReasonCodeType { Value = dataProvider.ReasonForEarlyRemoval };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return dataProvider.OtherSecurityType.IsEmpty ? null : new DeclarationAdditionalInformation
			{
				Content = new AdditionalInformationContentTextType { Value = dataProvider.OtherSecurityType }
			};
		}

		DeclarationObligationGuarantee PopulateObligationGuarantee()
		{
			return new DeclarationObligationGuarantee
			{
				LpcoExpirationDateTime = !dataProvider.SecurityEndDate.IsValid ? string.Empty : dataProvider.SecurityEndDate.ToString(DateFormatType.Date),
				SecurityDetailsCode = new ObligationGuaranteeSecurityDetailsCodeType { Value = dataProvider.SecurityType },
				SecurityAmount = new ObligationGuaranteeSecurityAmountType { Value = dataProvider.SecurityAmount },
				SecurityEffectiveDateTime = !dataProvider.SecurityStartDate.IsValid ? string.Empty : dataProvider.SecurityStartDate.ToString(DateFormatType.Date)
			};
		}
	}
}
