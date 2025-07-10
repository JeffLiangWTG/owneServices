using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._934)]
	class GOVCBRR99_934Supporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
			message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;
			entryNum.CE_EntryNum = messageData.NoticeNumber;
			entryNum.CE_IssueDate = messageData.AcceptDateTime;

			if (originalMessage != null)
			{
				using (var reader = originalMessage.GetEM_MessageTextReader())
				{
					var declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR934.Declaration>(reader);

					if (DateTime.TryParseExact(declaration.Consignment.AdditionalInformation?.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
					{
						entryNum.CE_ExpiryDate = dt1;
					}
				}
			}
		}
	}
}
