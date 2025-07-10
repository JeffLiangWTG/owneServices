using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class CHNCTSValidationHelper
{
	public static void CheckMRN(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, INotificationType notificationType = null)
	{
		CheckGDRNorMRN(factory, propertyInfo, false, notificationType);
	}

	public static void CheckGDRNorMRN(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, INotificationType notificationType = null)
	{
		CheckGDRNorMRN(factory, propertyInfo, true, notificationType);
	}

	static void CheckGDRNorMRN(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, bool allowGDRN, INotificationType notificationType)
	{
		var mrn = (ZString)propertyInfo.Value;
		if (!mrn.IsEmpty)
		{
			var ruleName = MRNRuleName;
			var errors = new ZStringBuilder();
			errors.AppendIfNotEmpty(NctsValidationHelper.CheckMRNFormat(mrn, factory, ZString.Empty));
			errors.AppendIfNotEmpty(CheckMRNYear(mrn));
			if (allowGDRN && mrn.SubstringSafe(2, 2) == Core.Constants.CountryCodes.Switzerland && mrn.SubstringSafe(16, 1) == GDRNSuffix)
			{
				ruleName = GDRNRuleName;
				errors.AppendIfNotEmpty(CheckGDRNMonth(mrn));
			}
			if (!errors.IsEmpty)
			{
				propertyInfo.AddNotification(notificationType ?? NotificationType.MessageError, Res.GetString("1120B6BB-5EDA-4F2B-9A03-28EF96E7F5EC", "[{1}]: {0}", errors.ToStringWithNewLineBetweenAppends(), ruleName));
			}
		}
	}

	static ZString CheckMRNYear(ZString mrn)
	{
		return mrn.SubstringSafe(0, 2).CompareTo(MinYear) >= 0 ? string.Empty : Res.GetString("570625ED-E53E-4DB5-A906-509F3782C503", "The year (the first two digits) must be at least 22.");
	}

	static ZString CheckGDRNMonth(ZString mrn)
	{
		var month = mrn.SubstringSafe(4, 2);
		if (!month.IsNumbersOnlyOrEmpty || month.CompareTo("01") < 0 || month.CompareTo("12") > 0)
		{
			return Res.GetString("EA5FA4B9-C0C9-4B8D-B2A7-76FA0E9624AF", "The month (the two digits after the country) must be between 01 and 12.");
		}
		return ZString.Empty;
	}

	const string MinYear = "22";
	const string GDRNSuffix = "N";
	const string MRNRuleName = "NS30006";
	const string GDRNRuleName = "NS30118";
}
