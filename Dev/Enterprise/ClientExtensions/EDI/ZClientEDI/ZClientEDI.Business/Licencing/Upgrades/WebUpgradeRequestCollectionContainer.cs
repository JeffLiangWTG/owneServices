using System;
using System.Globalization;
using System.Text;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class WebUpgradeRequestCollectionContainer : UpgradeRequestCollectionContainer
	{
		public WebUpgradeRequestCollectionContainer(BusinessObjectFactory factory, UpgradeRequestCollection upgradeRequests)
			: base(factory, upgradeRequests)
		{
			SendEmailNotificationAutomatically = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool RequestUpgrade(out String resultDetails)
		{
			bool result = false;
			resultDetails = "";

			if (HasErrors)
			{
				resultDetails = NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString();
			}
			else if (ShowPreUpgradeStatusAndConfirm(GetPreUpgradeStatus(), out resultDetails))
			{
				ShowPostUpgradeMessages(PlaceUpgradesToClients(), out resultDetails);
				result = true;
			}
			return result;
		}

		bool ShowPreUpgradeStatusAndConfirm(IPreUpgradeStatus status, out String resultDetails)
		{
			bool result = false;

			if (!status.IsError)
			{
				StringBuilder message = new StringBuilder();
				if (!string.IsNullOrEmpty(status.Message))
				{
					message.Append(status.Message + "\r\n");
				}

				if (status.ActiveUpgradesTotalCount > 0)
				{
					message.Append(string.Format(CultureInfo.CurrentCulture, "\r\nTotal number of upgrade requests to add: {0}.\r\n", status.ActiveUpgradesTotalCount));
					for (UpgradeStatusType statusType = UpgradeStatusType.ViaHttp; statusType <= UpgradeStatusType.WithoutNotificationAddress; statusType++)
					{
						int count = status.GetRequestCountByStatusType(statusType);
						if (count > 0)
						{
							message.Append(string.Format(CultureInfo.CurrentCulture, "\r\nNumber of upgrade requests {0}: {1}.", status.GetStatusTypeDescription(statusType), count));
						}
					}
					resultDetails = message.ToString();
					result = true;
				}
				else
				{
					string noActiveRequestsMessage = "There are no upgrades supported for the selected orgainsations.\r\nPlease, change your selection and try again.";
					resultDetails = noActiveRequestsMessage;
				}
			}
			else
			{
				resultDetails = status.Message + "\r\n" + "Error Occurred";
			}
			return result;
		}

		void ShowPostUpgradeMessages(IPostUpgradeStatus status, out String resultDetails)
		{
			if (!status.IsError)
			{
				StringBuilder message = new StringBuilder();
				if (!string.IsNullOrEmpty(status.Message))
				{
					message.Append(status.Message + "\r\n\r\n");
				}

				if (status.CreatedUpgradesToClientsCount > 0)
				{
					message.Append(string.Format(CultureInfo.CurrentCulture, "Total number of scheduled upgrades: {0}\r\n", status.CreatedUpgradesToClientsCount));
				}
				if (status.NotificationEmailsCount > 0)
				{
					message.Append(string.Format(CultureInfo.CurrentCulture, "Total number of notification emails sent: {0}\r\n", status.NotificationEmailsCount));
				}

				resultDetails = message.ToString();
			}
			else
			{
				resultDetails = status.Message + "\r\n" + "Error Occurred";
			}
		}
	}
}
