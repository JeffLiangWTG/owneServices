using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Common.CA
{
	public partial class B3EntryTypeList
	{
		public static bool IsInwardWarehouseEntryType(ZString entryType)
		{
			return GetWarehouseEntryType(entryType) == WarehouseEntryType.Inward;
		}

		public static bool IsExWarehouseEntryType(ZString entryType)
		{
			return GetWarehouseEntryType(entryType) == WarehouseEntryType.Outward;
		}

		public static WarehouseEntryType GetWarehouseEntryType(ZString entryType)
		{
			switch (entryType)
			{
				case B3EntryTypeList.Codes.Warehouse10:
				case B3EntryTypeList.Codes.ReWarehouse13:
					return WarehouseEntryType.Inward;
				case B3EntryTypeList.Codes.ExWarehouse20:
				case B3EntryTypeList.Codes.ExWarehouse21:
				case B3EntryTypeList.Codes.ExWarehouse22:
				case B3EntryTypeList.Codes.TransferOfGoods30:
					return WarehouseEntryType.Outward;
				default:
					return WarehouseEntryType.NonWarehouse;
			}
		}

		public static List<string> WarehouseEntryTypes
		{
			get
			{
				return new List<string>
						{
							B3EntryTypeList.Codes.Warehouse10,
							B3EntryTypeList.Codes.ReWarehouse13,
							B3EntryTypeList.Codes.ExWarehouse20,
							B3EntryTypeList.Codes.ExWarehouse21,
							B3EntryTypeList.Codes.ExWarehouse22,
							B3EntryTypeList.Codes.TransferOfGoods30
						};
			}
		}
		public enum WarehouseEntryType
		{
			NonWarehouse,
			Inward,
			Outward
		}
	}
}
