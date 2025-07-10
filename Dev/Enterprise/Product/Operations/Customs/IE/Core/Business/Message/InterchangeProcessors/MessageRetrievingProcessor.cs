using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class MessageRetrievingProcessor : XTTInboundInterchangeProcessor
	{
		protected override string[] ApplicationCodes => new[] {
			EDIInterchange.ApplicationCodes.IECustomsCommon,
			EDIInterchange.ApplicationCodes.IECustomsExport,
			EDIInterchange.ApplicationCodes.IECustomsImport,
			EDIInterchange.ApplicationCodes.IECustomsUCC5Import,
			EDIInterchange.ApplicationCodes.IECustomsEMCS,
			EDIInterchange.ApplicationCodes.IECustomsNCTS,
			EDIInterchange.ApplicationCodes.IECustomsAndExcise,
			EDIInterchange.ApplicationCodes.IECustomsPBN
		};

		protected override ZQuery GetInterchangeTypeFilter()
		{
			var mailboxQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
			mailboxQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, new ZString[] { CommonInterchangeTypeList.Codes.MailboxRequest, CommonInterchangeTypeList.Codes.MailboxAcknowledge });

			var emcsMessageResponseQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
			emcsMessageResponseQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, SQLComparisonOperator.NotEqual, CommonInterchangeTypeList.Codes.TransactionID);

			var otherMessageResponseQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, new[]
			{
				EDIInterchange.ApplicationCodes.IECustomsExport,
				EDIInterchange.ApplicationCodes.IECustomsImport,
				EDIInterchange.ApplicationCodes.IECustomsUCC5Import,
				EDIInterchange.ApplicationCodes.IECustomsNCTS,
				EDIInterchange.ApplicationCodes.IECustomsAndExcise,
				EDIInterchange.ApplicationCodes.IECustomsPBN
			});
			return otherMessageResponseQuery.AddToFilter(emcsMessageResponseQuery, JoinCondition.Or).AddToFilter(mailboxQuery, JoinCondition.Or);
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			IInboundMessageCreator result;
			if (interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.IECustomsAndExcise)
			{
				result = new CustomsAndExciseReportInboundMessageCreator();
			}
			else if (interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.IECustomsPBN)
			{
				result = new PBNMessageCreator(Logger);
			}
			else
			{
				switch (interchange.EI_InterchangeType.ToUpperInvariant())
				{
					case CommonInterchangeTypeList.Codes.MailboxRequest:
						result = new MessageCreator(Logger);
						break;
					case CommonInterchangeTypeList.Codes.MailboxAcknowledge:
						result = new InboundMessageCreatorNoCreation();
						break;
					default:
						result = new MessageAcknowledgementCreator(Logger);
						break;
				}
			}
			return result;
		}
	}
}
