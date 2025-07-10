using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business
{
	public static class NumberGeneratorHelper
	{
		public static ZBool IsUniqueEntryNumber(BusinessObjectFactory factory, ZString parentTable, ZString entryNum, ZString entryType, ZString country)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNum);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, parentTable);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
			query.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			return !factory.ExistsInDatabase(CusEntryNumber.Schema.TableName, query);
		}
	}
}
