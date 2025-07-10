using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TE;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5TE)]
	public class GOVCBR5TEMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5TEHeader dataProvider;
		public GOVCBR5TEMessageBuilder(IImport5TEHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				Payer = PopulatePayer(),
				VersionId = PopulateVersionID(),
				TypeCode = PopulateTypeCode(),
				Reason = PopulateReason(),
				Submitter = PopulateSubmitter()
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
				Value = GOVCBR + ElectronicDocumentTypeList.Codes._5TE
			};
		}
		DeclarationVersionIdType PopulateVersionID()
		{
			return new DeclarationVersionIdType
			{
				Value = dataProvider.SequenceNo.ToString()
			};
		}
		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType
			{
				Value = dataProvider.ApplicationReason
			};
		}
		DeclarationPayer PopulatePayer()
		{
			IDNumberAndType matchedNumber = null;
			if (dataProvider.Payer != null)
			{
				matchedNumber = dataProvider.Payer.GetBusinessOrIndividualRegistrationNumber();

				var exporterIdentificationID = matchedNumber?.Number ?? ZString.Empty;
				var roleCode = matchedNumber?.Type ?? ZString.Empty;

				return new DeclarationPayer
				{
					Id = new PayerIdentificationIdType
					{
						Value = exporterIdentificationID
					},
					RoleCode = new PayerRoleCodeType
					{
						Value = roleCode
					}
				};
			}
			return null;
		}
		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType { Value = dataProvider.BrokerID }
			};
		}
	}
}
