using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceHeaderGroupingDefinitionProvider
	{
		public InvoiceHeaderGroupingDefinitionProvider(IWhsBondedWarehouseAttribute bondedWarehouseAttribute, BusinessObjectFactory factory)
		{
			this.bondedWarehouseAttribute = Argument.NotNull(bondedWarehouseAttribute, nameof(bondedWarehouseAttribute));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly IWhsBondedWarehouseAttribute bondedWarehouseAttribute;
		readonly BusinessObjectFactory factory;

		Dictionary<ZString, ZString> AddInfoDictionary => addInfoDictionary ?? (addInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(bondedWarehouseAttribute.WB_AddInfo));
		Dictionary<ZString, ZString> addInfoDictionary;

		public ZString LinePriceCurrency => RefCurrency.LoadFromCurrencyCode(factory, AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.LinePriceCurrency))?.Code ?? ZString.Empty;

		public ZString IncotermCode => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.IncotermCode);

		public ZString IncotermPlace => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.IncotermPlace);

		public ZString TransNature => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.TransNature);

		public ZDateTime InvoiceDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var zDateString = AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants
					.WarehouseCustomsLineDetailsAddInfoKeys.InvoiceDate);

				if (ZDateTime.TryParseIgnoreTimezone(zDateString, CultureInfo.InvariantCulture, out var parsedDateTime))
				{
					result = parsedDateTime;
				}

				return result;
			}
		}

		public ZString InvoiceNumber => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.InvoiceNumber);
	}
}
