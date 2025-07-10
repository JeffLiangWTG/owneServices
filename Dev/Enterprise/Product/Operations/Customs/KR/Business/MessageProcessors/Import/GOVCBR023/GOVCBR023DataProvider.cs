using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR023;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR023DataProvider
	{
		public IGOVCBR023MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR023MessageData();
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTimeNoSecond, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.IssueDateTime = dt1;
			}
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.NoticeNumber = response.Declaration.DutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty;
			result.CustomsManagerID = response.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsManagerName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.ResultType = response.Status.NameCode.Value;
			result.DocumentSubmitType = response.Declaration.AttachedDocument?.TypeCode?.Value ?? ZString.Empty;

			if (DateTime.TryParseExact(response.Declaration.ExpirationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.PaymentDate = (ZDate)dt2;
			}
			result.CustomsOfficeContent = response.AdditionalInformation?.Content?.Value ?? ZString.Empty;

			if (response.Declaration.GoodsShipment != null)
			{
				var csCodesDictionary = new Dictionary<string, List<int>>();
				foreach (var goodsShipment in response.Declaration.GoodsShipment)
				{
					var csCode = goodsShipment.AdditionalInformation?.StatementCode?.Value ?? string.Empty;
					List<int> entryLineNos;
					if (!csCodesDictionary.TryGetValue(csCode, out entryLineNos))
					{
						entryLineNos = new List<int>();
						csCodesDictionary.Add(csCode, entryLineNos);
					}
					entryLineNos.Add((int)goodsShipment.SequenceNumeric);
				}
				result.CSCodes = csCodesDictionary;
			}

			return result;
		}
	}
}
