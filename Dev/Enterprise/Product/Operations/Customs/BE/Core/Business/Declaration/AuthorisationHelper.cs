using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public static class AuthorisationHelper
{
	public static ZString GetAuthorisationCode(ZString documentCode)
	{
		switch (documentCode)
		{
			case Constants.SupportingDocumentTypes.C517:
				return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			case Constants.SupportingDocumentTypes.C518:
				return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			case Constants.SupportingDocumentTypes.C519:
				return CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			default:
				return ZString.Empty;
		}
	}

	public static OrgAddress GetAuthoristationTarget(JobComInvoiceLine invLine)
	{
		OrgAddress result = null;
		var procedure = invLine?.CusProcedure;
		var entryInstruction = invLine?.EntryInstruction;
		if (procedure != null && entryInstruction != null)
		{
			if (procedure.IsIntoWarehouse())
			{
				result = entryInstruction.Warehouse2;
			}
			else if (procedure.IsOutOfWarehouse())
			{
				result = entryInstruction.Warehouse;
			}
		}
		return result;
	}
}
