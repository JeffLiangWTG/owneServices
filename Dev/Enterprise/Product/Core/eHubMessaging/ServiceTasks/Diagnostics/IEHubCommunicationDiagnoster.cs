namespace Enterprise.eHubMessaging.ServiceTasks
{
	interface IEHubCommunicationDiagnoster
	{
		string Run();
	}

	interface IEHubCommunicationDiagnosterFactory
	{
		IEHubCommunicationDiagnoster Create(string serverAddress);
	}
}
