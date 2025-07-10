using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.Common.CA.B3EntryTypeList;

namespace Enterprise.Customs.Common.CA
{
	public partial class CADEntryTypeList
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
				case CADEntryTypeList.Codes.Warehouse101:
				case CADEntryTypeList.Codes.Warehouse102:
				case CADEntryTypeList.Codes.ReWarehouse131:
				case CADEntryTypeList.Codes.ReWarehouse132:
					return WarehouseEntryType.Inward;
				case CADEntryTypeList.Codes.ExWarehouse201:
				case CADEntryTypeList.Codes.ExWarehouse211:
				case CADEntryTypeList.Codes.ExWarehouse212:
				case CADEntryTypeList.Codes.ExWarehouse213:
				case CADEntryTypeList.Codes.ExWarehouse214:
				case CADEntryTypeList.Codes.ExWarehouse215:
				case CADEntryTypeList.Codes.ExWarehouse216:
				case CADEntryTypeList.Codes.ExWarehouse22:
				case CADEntryTypeList.Codes.TransferOfGoods301:
				case CADEntryTypeList.Codes.TransferOfGoods302:
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
							CADEntryTypeList.Codes.Warehouse101,
							CADEntryTypeList.Codes.Warehouse102,
							CADEntryTypeList.Codes.ReWarehouse131,
							CADEntryTypeList.Codes.ReWarehouse132,
							CADEntryTypeList.Codes.ExWarehouse201,
							CADEntryTypeList.Codes.ExWarehouse211,
							CADEntryTypeList.Codes.ExWarehouse212,
							CADEntryTypeList.Codes.ExWarehouse213,
							CADEntryTypeList.Codes.ExWarehouse214,
							CADEntryTypeList.Codes.ExWarehouse215,
							CADEntryTypeList.Codes.ExWarehouse216,
							CADEntryTypeList.Codes.ExWarehouse22,
							CADEntryTypeList.Codes.TransferOfGoods301,
							CADEntryTypeList.Codes.TransferOfGoods302
						};
			}
		}

		public static string ConvertToShortCode(ZString code) => code.Replace("-", "");

		public static string ConvertToLongCode(ZString code)
		{
			var longCode = code.InsertSafe(2, "-");
			return WarehouseEntryTypes.Contains(longCode) ? longCode : code;
		}

		static List<string> GetWarehouseEntryTypesShortCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Customs|CA|WarehouseEntryTypesShortCodes", () =>
			{
				var result = new List<string>();
				WarehouseEntryTypes.ForEach(x =>
				{
					if (x.Length > 3)
					{
						result.Add(x.Remove(2, 1));
					}
				});
				return result;
			});
		}

		public static bool IsWarehouseEntryTypesShortCodes(BusinessObjectFactory factory, string code)
		{
			return GetWarehouseEntryTypesShortCodes(factory).Contains(code);
		}

		public static bool IsWarehouseEntryTypeForReleaseLocationST(ZString entryType)
		{
			return entryType == CADEntryTypeList.Codes.Warehouse101 ||
					entryType == CADEntryTypeList.Codes.Warehouse102 ||
					entryType == CADEntryTypeList.Codes.ReWarehouse131 ||
					entryType == CADEntryTypeList.Codes.ReWarehouse132 ||
					entryType == CADEntryTypeList.Codes.TransferOfGoods301 ||
					entryType == CADEntryTypeList.Codes.TransferOfGoods302;
		}

		public static bool IsWarehouseEntryTypeForReleaseLocationSF(ZString entryType)
		{
			return entryType == CADEntryTypeList.Codes.ReWarehouse131 ||
					entryType == CADEntryTypeList.Codes.ReWarehouse132 ||
					entryType == CADEntryTypeList.Codes.ExWarehouse201 ||
					entryType == CADEntryTypeList.Codes.ExWarehouse211 || entryType == CADEntryTypeList.Codes.ExWarehouse212 || entryType == CADEntryTypeList.Codes.ExWarehouse213 || entryType == CADEntryTypeList.Codes.ExWarehouse214 || entryType == CADEntryTypeList.Codes.ExWarehouse215 || entryType == CADEntryTypeList.Codes.ExWarehouse216 ||
					entryType == CADEntryTypeList.Codes.TransferOfGoods301 ||
					entryType == CADEntryTypeList.Codes.TransferOfGoods302;
		}
	}
}
