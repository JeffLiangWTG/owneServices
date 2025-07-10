using CargoWise.Types;

namespace Enterprise.Customs.BR.Business;

public static class CustomsPostedStatusListExtension
{
	public static bool NeedsToSendMessage(this ZString status)
	{
		switch (status)
		{
			case CustomsPostedStatusList.Codes.Active:
			case CustomsPostedStatusList.Codes.UpdatePending:
			case CustomsPostedStatusList.Codes.DeletePending:
				return true;
			default:
				return false;
		}
	}

	public static bool IsAccepted(this ZString status) => status == CustomsPostedStatusList.Codes.Accepted;

	public static bool IsDeleted(this ZString status) => status == CustomsPostedStatusList.Codes.Deleted;

	public static bool IsDeletePending(this ZString status) => status == CustomsPostedStatusList.Codes.DeletePending;

	public static bool IsUpdatePending(this ZString status) => status == CustomsPostedStatusList.Codes.UpdatePending;
}
