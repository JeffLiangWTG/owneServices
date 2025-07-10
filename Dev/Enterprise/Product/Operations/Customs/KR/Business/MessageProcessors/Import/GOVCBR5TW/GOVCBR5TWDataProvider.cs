using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TW;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5TWDataProvider
	{
		public GOVCBR5TWMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5TWMessageData(factory);

			result.NoticeNumber = response.Declaration.Id.Value;
			result.FormattedNoticeNumber = MessageFunctions.GetFormattedNumber(response.Declaration.Id.Value, new int[] { 0, 3, 5 });
			result.RequestDocumentNumber = response.Declaration.FunctionalReferenceId?.Value ?? ZString.Empty;
			result.PayerCompanyName = response.Declaration.Payer.Name.Value;
			result.PayerRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.ImportDeclarationNumber = response.Declaration.PreviousDocument.Id.Value;
			result.FormattedImportDeclarationNumber = MessageFunctions.DeclarationNumberFormat(response.Declaration.PreviousDocument.Id.Value);
			result.ImportEntryLineNo = (ZInt)response.Declaration.PreviousDocument.SequenceNumeric;

			if (DateTime.TryParseExact(response.Declaration.PreviousDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.ImportDeclarationDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.BeginningDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.ExamineStartDate = new ZDate(dt2);
			}
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.EndingDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.ExamineEndDate = new ZDate(dt3);
			}

			result.ContentDescription = response.Declaration.AdditionalInformation.Content?.Value;
			result.CorrectionResult = response.Declaration.AdditionalInformation.StatementDescription?.Value;

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt4))
			{
				result.NoticeDate = new ZDate(dt4);
			}
			result.NoticeCustomsOffice = response.Declaration.DeclarationOfficeId.Value;
			result.NoticeCustomsOfficeName = MessageFunctions.GetCustomsOffice(factory, result.NoticeCustomsOffice);
			result.DeclarationsCount = (ZInt)response.Declaration.LoadingListQuantity.Value;
			result.DeclarantID = response.Declaration.Submitter?.Id?.Value ?? ZString.Empty;

			if (response.Declaration.GoodsShipment != null)
			{
				GOVCBR5TWLineMessageData lineMessageData = null;
				foreach (var entry in response.Declaration.GoodsShipment)
				{
					lineMessageData = result.Lines.AddNew();
					lineMessageData.ImportEntryLineNo = (ZInt)entry.AdditionalDocument.SequenceNumeric;
					lineMessageData.ContentDescription = entry.AdditionalInformation.Content?.Value ?? ZString.Empty;
					lineMessageData.CorrectionResult = entry.AdditionalInformation.StatementDescription?.Value ?? ZString.Empty;
					lineMessageData.RequestDocumentNumber = entry.AttachedDocument?.Id?.Value ?? ZString.Empty;
					lineMessageData.AttachedDeclarationNumber = entry.AdditionalDocument.Id.Value;
					lineMessageData.FormattedAttachedDeclarationNumber = MessageFunctions.DeclarationNumberFormat(entry.AdditionalDocument.Id.Value);
					if (DateTime.TryParseExact(entry.AdditionalInformation.BeginningDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt5))
					{
						lineMessageData.ExamineStartDate = new ZDate(dt5);
					}
					if (DateTime.TryParseExact(entry.AdditionalInformation.EndingDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt6))
					{
						lineMessageData.ExamineEndDate = new ZDate(dt6);
					}
					if (DateTime.TryParseExact(entry.AdditionalDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt7))
					{
						lineMessageData.IssueDate = new ZDate(dt7);
					}
				}
			}
			return result;
		}
	}
}
