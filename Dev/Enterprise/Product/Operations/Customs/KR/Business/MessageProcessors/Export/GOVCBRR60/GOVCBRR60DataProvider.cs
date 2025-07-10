using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR60;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR60DataProvider
	{
		public IGOVCBRR60MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR60MessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.SupplierName = response.Declaration.Exporter.Name.Value;

			if (response.Declaration.GoodsShipment != null)
			{
				var commodityList = new List<Commodity>();
				foreach (var security in response.Declaration.GoodsShipment)
				{
					var commodity = new Commodity();

					commodity.ModelName = security.GovernmentAgencyGoodsItem?.Commodity?.CargoDescription?.Value ?? ZString.Empty;
					commodity.ComplementDescription = security.GovernmentAgencyGoodsItem?.Commodity?.Description?.Value ?? ZString.Empty;

					commodityList.Add(commodity);
				}
				result.Commodity = commodityList.ToArray();
			}

			return result;
		}
	}
}
