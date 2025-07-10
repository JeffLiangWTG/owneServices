using System.Globalization;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	static class AwbSerialNumberHelper
	{
		internal static void WarnIfAwbNumberIsChanging(ZPropertyInfo propertyInfo, ICcsukCusAwb awb)
		{
			if (awb.IsInDatabase && HasValueChanged(propertyInfo) && awb.IsLodgedOrAssumedAtCcsuk)
			{
				AwbSerialNumberHelper.AddValidationError(propertyInfo, awb);
			}
		}

		static bool HasValueChanged(ZPropertyInfo propertyInfo)
		{
			return !string.Equals(propertyInfo.OriginalValue.ToString(), propertyInfo.Value.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		static void AddValidationError(ZPropertyInfo propertyInfo, ICcsukCusAwb awb)
		{
			var errorText = string.Format(CultureInfo.CurrentCulture, "You are changing the value of this key field away from '{0}' after the record has been registered on the CCS-UK network (Presence='{1}').", propertyInfo.OriginalValue.ToString(), awb.PresenceOnNetworkStatus);
			if (MasterFiles.Business.GlbStaff.CurrentUser.IsSupportUser || MasterFiles.Business.GlbStaff.CurrentUser.GS_IsController)
			{
				propertyInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, "{0}\r\nYou should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. As a controller, you may proceed with caution.", errorText));
			}
			else
			{
				propertyInfo.AddError(string.Format(CultureInfo.CurrentCulture, "{0}\r\nYou should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. Only your administrator can save this change.", errorText));
			}
		}
	}
}
