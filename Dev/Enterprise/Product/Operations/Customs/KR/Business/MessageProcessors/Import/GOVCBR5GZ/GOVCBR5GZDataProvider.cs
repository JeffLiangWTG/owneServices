using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GZ;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5GZDataProvider
	{
		public IGOVCBR5GZMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5GZMessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = new ZDateTime(dt);
			}
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.CustomsManagerName = response.Authenticator.Name.Value;

			var unsettled = new List<KeyValuePair<ZString, ZString>>();

			foreach (var code in response.Status)
			{
				unsettled.Add(new KeyValuePair<ZString, ZString>(code.NameCode.Value, code.Description?.Value ?? ZString.Empty));
			}

			result.Unsettled = unsettled.ToArray();

			return result;
		}
	}
}
