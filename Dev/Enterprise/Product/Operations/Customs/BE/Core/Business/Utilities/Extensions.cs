using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public static class Extensions
{
	public static OrgCusCode[] getEoriRegNo(this OrgHeader orgHeader)
	{
		return orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
	}

	public static CusEntryNumber CreateMovementReferenceNumber(BusinessObject businessObject, string mrn)
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(businessObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Belgium);
		mrnEntryNumber.CE_EntryNum = mrn;
		return mrnEntryNumber;
	}
}
