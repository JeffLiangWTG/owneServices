using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.WFN
{
	public class WFNForwardingConsolValueObjectDataAdapter : ForwardingConsolValueObjectDataAdapter
	{
		protected override bool ShouldUpdateExistingObject(ForwardingConsol consol, INotifications notifications)
		{
			string message = Res.GetString("9a55b92a-f3f4-4451-9419-a33bec391b7d", "{0} had already been imported and cannot be updated. XML file emailed to notification group.", consol.HumanReadableName);
			notifications.Notify(new ErrorNotification(ErrorType.ImportingDataError, message));
			return false;
		}
	}
}
