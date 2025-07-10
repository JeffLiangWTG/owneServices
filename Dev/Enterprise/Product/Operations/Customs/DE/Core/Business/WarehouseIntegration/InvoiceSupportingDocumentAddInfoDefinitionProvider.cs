using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using static Enterprise.Customs.DE.Business.BondedWarehousingHelper.Constants;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceSupportingDocumentAddInfoDefinitionProvider
	{
		public InvoiceSupportingDocumentAddInfoDefinitionProvider(IWarehouseCustomsAddInfo addInfo)
		{
			this.addInfo = Argument.NotNull(addInfo, nameof(addInfo));
		}
		readonly IWarehouseCustomsAddInfo addInfo;

		Dictionary<ZString, ZString> AddInfoDictionary => addInfoDictionary ?? (addInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(addInfo.B7_AddInfoData));
		Dictionary<ZString, ZString> addInfoDictionary;

		public ZString Type => AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.Type);

		public ZString Reference => AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.Reference);

		public ZDateTime DateOfIssue
		{
			get
			{
				var result = ZDateTime.Empty;
				var dateOfIssueString = AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.DateOfIssue);

				if (ZDateTime.TryParseIgnoreTimezone(dateOfIssueString, CultureInfo.InvariantCulture,
						out var parsedDateTime))
				{
					result = parsedDateTime;
				}

				return result;
			}
		}

		public ZString Available => AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.Available);

		public ZDecimal Quantity => ZDecimal.ParseSafe(AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.Quantity), ZDecimal.Zero);

		public ZString UnitOfMeasure => AddInfoDictionary.GetValueSafe(WarehouseCustomsLineAddInfoSupportingInfoKeys.UnitofMeasure);
	}
}
