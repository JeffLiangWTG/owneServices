using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

static class TemporaryStorageTestHelper
{
	public static CusEntryNumber CreateCusEntryNumber(BusinessObject parent, ZString entryType)
	{
		var cusEntryNumber = parent.Factory.New<CusEntryNumber>();

		cusEntryNumber.CE_ParentID = parent.PK;
		cusEntryNumber.CE_ParentTable = parent.TableName;
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

		return cusEntryNumber;
	}
}
