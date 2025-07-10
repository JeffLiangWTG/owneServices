using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR99;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR99DataProvider
	{
		public IGOVCBRR99MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR99MessageData();

			result.DeclarationType = ((ZString)response.Declaration.TypeCode.Value).SubstringSafe(6);
			result.ApplicationNumber = result.DeclarationType == ElectronicDocumentTypeList.Codes._5FN ?
													response.Declaration.Id?.Value.Substring(0, 14) ?? ZString.Empty : response.Declaration.Id?.Value ?? ZString.Empty;
			if (result.DeclarationType == ElectronicDocumentTypeList.Codes._5FN)
			{
				result.EntryLineNo5FN = response.Declaration.Id?.Value.Substring(14, 3);
			}
			result.AmendSequence = new ZInt(response.Declaration.VersionId?.Value ?? ZString.Empty);
			result.DeclarationSubType = response.Declaration.SubTypeCode?.Value ?? ZString.Empty;
			result.CustomsOffice = response.Declaration.DeclarationOfficeId?.Value ?? ZString.Empty;
			result.CustomsPersonID = response.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.DeclarationDate = new ZDate(dt);
			}
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AcceptDateTime = dt1;
			}
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.NoticeDateTime = dt2;
			}
			result.NoticeNumber = response.Id?.Value ?? ZString.Empty;

			if (response.Declaration.AdditionalInformation != null)
			{
				var contents = new List<IContent>();
				foreach (var content in response.Declaration.AdditionalInformation)
				{
					var statement = new Content();

					statement.ContentType = content.StatementCode?.Value ?? ZString.Empty;
					statement.ContentDescription = content.StatementDescription?.Value ?? ZString.Empty;

					contents.Add(statement);
				}
				result.Content = contents.ToArray();
			}

			return result;
		}
	}
}
