using System;
using System.Globalization;
using System.Threading;
using CargoWise.Definitions;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.IncidentAssociationNewIncidentsServiceTask.Code,
	"Incident Association New Incidents",
	"SYS",
	typeof(Enterprise.Client.EDI.IncidentAssociationNewIncidentsServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Client.EDI.IncidentAssociationNewIncidentsServiceTask.Code,
	AutoIncidentMain.Schema.TableName,
	new[]
	{
		AutoIncidentMain.Schema.IM_IncidentType + "=" + IncidentConstants.IncidentType.SupportIncident,
	}, null, ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI
{
	public class IncidentAssociationNewIncidentsServiceTask : ServiceProviderImpl
	{
		public const string Code = "IAN";
		public const string DbAppLockCode = "Incident_Association_IAN";

		public override void RunTask(CancellationToken cancellationToken)
		{
			var incidentAssociationSystemStatus = IncidentAssociationSystemStatus.GetStatus();
			if (incidentAssociationSystemStatus != IncidentAssociationSystemStatus.ReadyForNewIncidents)
			{
				ServiceLogger?.Information("Not yet ready to process new incidents");
				return;
			}

			var runner = new IncidentAssociationNewIncidentsRunner(ServiceLogger);
			try
			{
				ServiceLogger?.Information("Cleaning up old incidents");
				runner.CleanUpOldIncidentAssociations();

				ServiceLogger?.Information("Processing new and updated incidents");
				runner.ProcessNewIncidentVectors();

				var maxToStore = EDIDataRegistry.Instance.RelatedIncidentsMaxTopToStore.Value;
				runner.ProcessNewIncidentSimilarities(maxToStore, cancellationToken);
			}
			catch (OperationCanceledException oce)
			{
				ServiceLogger?.Warning(string.Format(CultureInfo.InvariantCulture, "Cancellation requested before `{0}` completed", oce.TargetSite));
			}
		}
	}
}
