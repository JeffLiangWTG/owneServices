using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business
{
	public class T2LClearanceEmailResponseMessageProcessor : ESResponseMessageProcessor<IT2LClearanceEmailProvider>
	{
		public T2LClearanceEmailResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const int CSVClearanceLength = 16;
		const string CSVClearanceString = "CSV:";

		protected override string MessageFriendlyNameCore => T2LMessageName;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string T2LMessageName = "T2L Clearance Email Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail, DeclarationMessageTypeList.Codes.T2cClearanceEmail };
		protected override ZBool ShouldHaveSentInterchange => false;

		protected override IT2LClearanceEmailProvider GetMessageProviderCore(EDIMessage message)
		{
			ZString GetSpecificData(ZString fullText, ZString initialLineText, ZInt dataLength)
			{
				var result = ZString.Empty;

				if (fullText.Contains(initialLineText))
				{
					int startIndex = fullText.IndexOf(initialLineText, StringComparison.OrdinalIgnoreCase) + initialLineText.Length;
					result = fullText.SubstringSafe(startIndex, dataLength);
				}

				return result;
			}

			var messageText = message.EM_MessageText.Replace(" ", "");

			var csvClearance = GetSpecificData(messageText, CSVClearanceString, CSVClearanceLength);

			if (csvClearance.IsEmpty)
			{
				throw new InvalidOperationException(Res.GetString("FF92798A-E038-404B-8EC2-D04CBDD739A7", "Email Body doesn't have the correct data"));
			}

			return new T2LClearanceEmailObject(csvClearance);
		}

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			var entryTypeToSearch = ZString.Empty;
			var extraQuery = (ZDBOnlyQuery)null;

			if (message.EM_MessageType == DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail)
			{
				entryTypeToSearch = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				extraQuery = ExtraSubQueryForGetRelevantBusinessObjectForEmailResponse();
			}
			else if (message.EM_MessageType == DeclarationMessageTypeList.Codes.T2cClearanceEmail)
			{
				entryTypeToSearch = CusEntryNumberTypes.Spain.T2CMovementReferenceNumber;
			}

			return MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, entryTypeToSearch, extraQuery);
		}

		ZDBOnlyQuery ExtraSubQueryForGetRelevantBusinessObjectForEmailResponse()
		{
			var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Common.Shared.SharedJobMessageTypeList.Codes.Export);

			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			query.AddSubQuery(CusEntryHeaderSchema.CH_JE, declarationQuery, JoinCondition.And);
			return query;
		}

		protected override void ProcessMessageCore(EDIMessage message, CusEntryHeader linkedBusinessObject, IT2LClearanceEmailProvider provider)
		{
			linkedBusinessObject.SetCSVClearanceNum(provider.CSVClearance);

			TriggerMisingDocumentRequestForEmails(linkedBusinessObject, message);

			SetMessageStatusAsReceived(message);
			SetMessageSubTypeAsAccepted(message);
		}

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message)
		{
			if (message.EM_MessageType == DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail)
			{
				return new T2LExpeditionDocumentRequest(businessObject, certName);
			}
			else if (message.EM_MessageType == DeclarationMessageTypeList.Codes.T2cClearanceEmail)
			{
				return new T2LClearanceDocumentRequest(businessObject, certName);
			}
			else
			{
				return null;
			}
		}
	}
}
