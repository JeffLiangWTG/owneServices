using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR67;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR67DataProvider
	{
		public IGOVCBRR67MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRR67MessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.ApplicationNumber = response.Declaration.Id.Value;
			result.SequenceNo = response.Declaration.VersionId?.Value == null ? ZInt.Zero : new ZInt(response.Declaration.VersionId.Value);
			if (response.Declaration.AcceptanceDateTime != null)
			{
				if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					result.AcceptDateTime = dt1;
				}
			}

			result.NoticeDescription = response.Status?.Description?.Value ?? ZString.Empty;

			return result;
		}
	}
}
