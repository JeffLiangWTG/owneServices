using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UT;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5UTDataProvider
	{
		public IGOVCBR5UTMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5UTMessageData();

			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.ShippingYN = response.Declaration.BorderTransportMeans.BallastOrCargoTypeCode.Value;
			if (DateTime.TryParseExact(response.Declaration.LoadingLocation.LoadingDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ShippingDate = new ZDate(dt);
			}

			return result;
		}
	}
}
