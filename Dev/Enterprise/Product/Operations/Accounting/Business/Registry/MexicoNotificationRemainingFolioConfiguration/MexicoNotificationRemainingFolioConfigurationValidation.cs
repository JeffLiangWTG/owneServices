using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class MexicoNotificationRemainingFolioConfigurationValidation
	{
		readonly MexicoNotificationRemainingFolioConfiguration Parent;

		public MexicoNotificationRemainingFolioConfigurationValidation(MexicoNotificationRemainingFolioConfiguration parent)
		{
			Parent = parent;
		}

		public void ValidateFoliosQuantityAndInterval()
		{
			Parent.FoliosQuantityInfo.ClearAllNotifications();
			Parent.IntervalInfo.ClearAllNotifications();

			if (Parent.FoliosQuantity < 0)
			{
				Parent.FoliosQuantityInfo.AddError(ErrorMessageNegativeNumber);
			}

			if (Parent.Interval < 0)
			{
				Parent.IntervalInfo.AddError(ErrorMessageNegativeNumber);
			}
			else if (Parent.Interval > Parent.FoliosQuantity)
			{
				Parent.IntervalInfo.AddError(ErrorMessageIntervalHighestThanFoliosQuantity);
			}
		}

		string ErrorMessageNegativeNumber => Res.GetString("75493D1C-BC88-4C20-8CBA-93C3D844CFE2", "Must be a positive number.");
		string ErrorMessageIntervalHighestThanFoliosQuantity => Res.GetString("6E953442-B8B6-4199-BCB8-CA7E0FB51D2E", "Interval value must be smaller than or equal to the value of Quantity of remaining Folios/Timbres field.");
	}
}
