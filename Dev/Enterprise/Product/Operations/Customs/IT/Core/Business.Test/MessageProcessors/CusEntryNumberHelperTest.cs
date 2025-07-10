using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

public static class CusEntryNumberHelperTest
{
	static public CusEntryNumber[] GetEntryNumbers(BusinessObjectFactory factory, CusEntryHeader entryHeader, ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, entryHeader.CountryCode);
		query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return factory.Load<CusEntryNumber>(query);
	}

	static public CusEntryNumber GetEntryNumber(BusinessObjectFactory factory, ZString entryType, CusEntryHeader entryHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		return factory.LoadTop1<CusEntryNumber>(query);
	}
}
