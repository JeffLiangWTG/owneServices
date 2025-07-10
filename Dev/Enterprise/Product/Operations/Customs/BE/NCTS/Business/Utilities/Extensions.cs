using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public static class Extensions
{
	public static ZString GetInvalidationJustification(this NctsDepartureMovementHeader movementHeader)
	{
		return GetValueFromAdditonalText(movementHeader, Constants.NctsDepartureMovementHeaderAdditionalTextKeys.InvalidationJustification);
	}

	public static ZString GetValueFromAdditonalText(this NctsDepartureMovementHeader movementHeader, ZString key)
	{
		var value = movementHeader.BM_AdditionalText.Split("*").FirstOrDefault(s => s.StartsWith(key + "="));
		return value.IsEmpty ? value : value.Split("=")[1];
	}

	public static ZString GetEORI(this OrgHeader orgHeader)
	{
		return orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault()?.OK_CustomsRegNo ?? ZString.Empty;
	}

	public static ZString GetCBR(this OrgHeader orgHeader)
	{
		return orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.BrokerageRegistration).FirstOrDefault()?.OK_CustomsRegNo ?? ZString.Empty;
	}

	public static void CreateMovementReferenceNumber(BusinessObject businessObject, string mrn)
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(businessObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Belgium);
		mrnEntryNumber.CE_EntryNum = mrn;
	}
}
