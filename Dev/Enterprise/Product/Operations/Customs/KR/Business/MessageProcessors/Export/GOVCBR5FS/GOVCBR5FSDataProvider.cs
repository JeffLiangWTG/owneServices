using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FS;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5FSDataProvider
	{
		public IGOVCBR5FSMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5FSMessageData();

			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.EntryReleaseDate = new ZDate(dt);
			}
			result.SupplierCompanyName = response.Declaration.Exporter.Name.Value;
			result.DeclarantID = response.Declaration.Submitter.Id.Value;
			result.ManufacturerID = response.Declaration.Manufacturer?.Id?.Value ?? ZString.Empty;
			result.TradeName = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value;
			result.TotalPackQty = Convert.ToInt32(response.Declaration.TotalPackageQuantity.Value);
			result.PackType = response.Declaration.Packaging.TypeCode.Value;
			result.TotalGrossWeightInKG = new ZDecimal(response.Declaration.TotalGrossMassMeasure.Value);

			return result;
		}
	}
}
