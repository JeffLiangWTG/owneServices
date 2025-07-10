using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BF;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BF)]
	public class GOVCBR5BFMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5BFHeader dataProvider;
		public GOVCBR5BFMessageBuilder(IImport5BFHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}
		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				TypeCode = PopulateTypeCode(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				Reason = PopulateReason()
			};
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5BF };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType { Value = dataProvider.ApplicationReason };
		}
	}
}
