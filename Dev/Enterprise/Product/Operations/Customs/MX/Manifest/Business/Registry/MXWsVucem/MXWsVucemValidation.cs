using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXWsVucemValidation
	{
		public MXWsVucemValidation(MXWsVucem parent)
		{
			this.parent = parent;
		}
		readonly MXWsVucem parent;

		internal void ValidateAll()
		{
			ValidateURLWSAirMode();
			ValidateUsernameWSAirMode();
			ValidatePasswordWSAirMode();
			ValidateURLWSSeaMode();
			ValidateUsernameWSSeaMode();
			ValidatePasswordWSSeaMode();
		}

		string UrlInvalidMsg => Res.GetString("CD6FC080-D6F3-45C8-81F7-2450F8A3A42A", "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format \"{0}\" or \"{1}\"", "http://", "www.");

		public void ValidateURLWSAirMode()
		{
			var targetInfo = parent.AirModeWSResponseInfo;
			targetInfo.ClearAllNotifications();

			if (parent.AirModeWSResponse.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(targetInfo);
			}
			else if (!targetInfo.HasErrors() && !UrlValidation.IsValidUrl(parent.AirModeWSResponse))
			{
				targetInfo.AddError(UrlInvalidMsg);
			}
		}

		public void ValidateUsernameWSAirMode()
		{
			var targetInfo = parent.AirModeWSUsernameInfo;
			targetInfo.ClearAllNotifications();

			if (parent.AirModeWSUsername.IsEmpty && !parent.AirModeWSResponse.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		public void ValidatePasswordWSAirMode()
		{
			var targetInfo = parent.AirModeWSPasswordInfo;
			targetInfo.ClearAllNotifications();

			if (parent.AirModeWSPassword.IsEmpty && !parent.AirModeWSResponse.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		public void ValidateURLWSSeaMode()
		{
			var targetInfo = parent.SeaModeWSResponseInfo;
			targetInfo.ClearAllNotifications();

			if (parent.SeaModeWSResponse.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(targetInfo);
			}
			else if (!targetInfo.HasErrors() && !UrlValidation.IsValidUrl(parent.SeaModeWSResponse))
			{
				targetInfo.AddError(UrlInvalidMsg);
			}
		}

		public void ValidateUsernameWSSeaMode()
		{
			var targetInfo = parent.SeaModeWSUsernameInfo;
			targetInfo.ClearAllNotifications();

			if (parent.SeaModeWSUsername.IsEmpty && !parent.SeaModeWSResponse.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		public void ValidatePasswordWSSeaMode()
		{
			var targetInfo = parent.SeaModeWSPasswordInfo;
			targetInfo.ClearAllNotifications();

			if (parent.SeaModeWSPassword.IsEmpty && !parent.SeaModeWSResponse.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}
	}
}
