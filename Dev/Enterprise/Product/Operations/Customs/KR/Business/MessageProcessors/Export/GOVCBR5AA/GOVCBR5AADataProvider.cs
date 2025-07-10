using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AA;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5AADataProvider
	{
		public IGOVCBR5AAMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5AAMessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTimeNoSecond, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.EntryReleaseDateTime = dt;
			}
			if (DateTime.TryParseExact(response.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.LoadingDate = new ZDate(dt1);
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.CustomsValueUSD = response.Declaration.InvoiceAmount.FirstOrDefault(x => x.CurrencyId == Iso3AlphaCurrencyCodeContentType.Usd)?.Value ?? ZDecimal.Zero;
			result.CustomsValueKRW = response.Declaration.InvoiceAmount.FirstOrDefault(x => x.CurrencyId == Iso3AlphaCurrencyCodeContentType.Krw)?.Value ?? ZDecimal.Zero;
			result.RoadNameRequest = response.Declaration.Note?.Value ?? ZString.Empty;
			result.CustomsOfficeContent = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.ContentDescription = response.Declaration.AdditionalInformation?.Notice?.Value ?? ZString.Empty;

			return result;
		}
	}
}
