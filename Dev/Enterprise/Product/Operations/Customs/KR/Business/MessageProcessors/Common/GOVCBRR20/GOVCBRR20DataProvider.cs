using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR20;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR20DataProvider
	{
		public IGOVCBRR20MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRR20MessageData();

			result.DeclarationType = response.Declaration.TypeCode.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = dt1;
			}
			if (result.DeclarationType.Substring(result.DeclarationType.Length - 3, 3) == ElectronicDocumentTypeList.Codes._5FN)
			{
				result.ApplicationNumber = response.Declaration.Id.Value.Substring(0, 14);
				result.EntryLineNo5FN = response.Declaration.Id.Value.Substring(14, 3);
			}
			else
			{
				result.ApplicationNumber = response.Declaration.Id.Value;
			}
			result.AmendSequence = new ZInt(response.Declaration.VersionId?.Value);
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.AcceptDateTime = dt2;
			}
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId?.Value ?? ZString.Empty;

			if (response.Error != null)
			{
				var errorList = new List<Error>();
				foreach (var errorItem in response.Error)
				{
					var error = new Error();
					error.ErrorDescription = errorItem.Description?.Value;

					if (errorItem.Pointer.TagId != null)
					{
						var dict = new List<ZString>();
						foreach (var tagID in errorItem.Pointer.TagId)
						{
							dict.Add(tagID.Value);
						}
						error.ApplicationKey = dict;
					}
					errorList.Add(error);
				}
				result.Error = errorList.ToArray();
			}
			return result;
		}
	}
}
