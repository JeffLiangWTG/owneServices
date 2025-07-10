namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Customer only has email to discover the status of their incident.
	/// </summary>
	public class EmailOnlyCustomerNotificationSender : IIncidentCustomerNotificationSender
	{
		public void AddHyperlinks(SupportIncidentEmail email)
		{
			// None applicable
		}

		public void SendEmail(SupportIncidentEmail email)
		{
			email?.SendEmail();
		}

		public void SendChanges(SupportIncident incident, SupportIncidentEmail email)
		{
			if (email != null)
			{
				email.ShouldSaveBizOFactoryOnSent = false;
				email.SendEmail();
			}

			this.CreateAndSendConversationMessageToCustomer(incident, email);
		}
	}
}
