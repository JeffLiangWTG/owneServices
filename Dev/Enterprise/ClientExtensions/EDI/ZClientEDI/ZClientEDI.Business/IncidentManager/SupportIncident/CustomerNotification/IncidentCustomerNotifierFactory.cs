using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IInteractiveIncidentCustomerNotifierFactory
	{
		IIncidentCustomerNotifier CreateNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender);
	}

	public static class IncidentCustomerNotifierFactory
	{
		public static IIncidentCustomerNotifier CreateNotifier(SupportIncident incident)
		{
			if (Globals.IsUserInteractive && !Globals.IsTest)
			{
				return ObjectFactory.Get<IInteractiveIncidentCustomerNotifierFactory>()
					.CreateNotifier(incident, CreateSender(incident));
			}
			else if (Env.CurrentUser.IsWebUser)
			{
				return CreateEmbeddedERequest(incident);
			}
			else
			{
				return new LocalSystemChangeIncidentCustomerNotifier(incident, CreateSender(incident));
			}
		}

		public static IIncidentCustomerNotificationSender CreateSender(SupportIncident incident)
		{
			if (incident.IsWebRequest)
			{
				return new WebRequestNotificationSender();
			}
			else
			{
				return (IIncidentCustomerNotificationSender)ERequestCustomerNotificationSender.CreateSenderIfSupported(incident)
					?? new EmailOnlyCustomerNotificationSender();
			}
		}

		public static bool IsLinkedToERequestInCustomerDatabase(SupportIncident incident)
		{
			return !incident.IsWebRequest
				&& (!incident.IM_ClientIncidentReference.IsEmpty || ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(incident));
		}

		public static IIncidentCustomerNotifier CreateNoActionNotifier(SupportIncident incident)
		{
			return new NoActionIncidentCustomerNotifier(incident);
		}

		/// <summary>
		/// Customer is updating an incident from the embedded-into-CW1 eRequest system calling the incident web service.
		/// </summary>
		public static IIncidentCustomerNotifier CreateEmbeddedERequest(SupportIncident incident)
		{
			// Note, the only notification should be for automatic reopening.
			// They used to be able to change to CR6, which would trigger a message, but criticality change was later disallowed.
			// Has to be a change from the customer via the eRequest web service
			return new CustomerChangeIncidentCustomerNotifier(incident, CreateSender(incident));
		}
	}
}

