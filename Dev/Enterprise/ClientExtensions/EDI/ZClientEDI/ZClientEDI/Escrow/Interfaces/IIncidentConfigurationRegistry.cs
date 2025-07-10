namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	interface IIncidentConfigurationRegistry
	{
		string IncidentProduct { get; }
		string IncidentModule { get; }
		string IncidentPriority { get; }
		string IncidentMessage { get; }
	}
}
