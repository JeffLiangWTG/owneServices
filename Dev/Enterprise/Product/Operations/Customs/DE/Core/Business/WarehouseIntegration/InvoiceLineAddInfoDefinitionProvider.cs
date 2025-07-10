using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceLineAddInfoDefinitionProvider
	{
		public InvoiceLineAddInfoDefinitionProvider(IWhsBondedWarehouseAttribute bondedWarehouseAttribute)
		{
			this.bondedWarehouseAttribute = Argument.NotNull(bondedWarehouseAttribute, nameof(bondedWarehouseAttribute));
		}

		readonly IWhsBondedWarehouseAttribute bondedWarehouseAttribute;

		Dictionary<ZString, ZString> AddInfoDictionary => addInfoDictionary ?? (addInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(bondedWarehouseAttribute.WB_AddInfo));
		Dictionary<ZString, ZString> addInfoDictionary;

		public ZString CountryOfSupply => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.CountryOfSupply);

		public ZDecimal LineNetPrice => ZDecimal.ParseSafe(AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.LineNetPrice), ZDecimal.Zero);

		public ZDecimal LinePrice => ZDecimal.ParseSafe(AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.LinePrice), ZDecimal.Zero);
	}
}
