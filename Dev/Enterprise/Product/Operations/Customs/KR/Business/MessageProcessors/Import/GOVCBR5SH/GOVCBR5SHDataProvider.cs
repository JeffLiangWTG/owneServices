using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SH;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5SHDataProvider
	{
		public IGOVCBR5SHMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5SHMessageData();

			result.ApplicationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ApprovalDate = new ZDate(dt);
			}
			result.ResultType = response.Status.NameCode.Value;
			result.DismissalReason = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.CustomsManagerName = response.Authenticator.Name.Value;
			result.CustomsOffice = response.Declaration.DeclarationOfficeId.Value;

			var additionalList = new List<IEntryDetail>();
			foreach (var item in response.Declaration.AdditionalDocument)
			{
				var additional = new EntryDetail();

				additional.ImportDeclarationNumber = item.Id.Value;
				if (DateTime.TryParseExact(item.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					additional.EntryReleaseDate = new ZDate(dt1);
				}
				additionalList.Add(additional);
			}
			result.EntryDetails = additionalList.ToArray();

			return result;
		}
	}
}
