using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	class IncidentCreator : IIncidentCreator
	{
		public IncidentCreator(IIncidentConfigurationRegistry incidentConfigurationRegistry)
		{
			this.incidentConfigurationRegistry = incidentConfigurationRegistry ?? throw new ArgumentNullException(nameof(incidentConfigurationRegistry));
		}

		public void Create(IExportResult exportResult, ILogger logger)
		{
			try
			{
				var linkToAssetFolder = exportResult.RemotePath.Replace("/endpoints/", "/assets/");
				var now = ZDateTime.Now;
				var currentDate = now.ToString("dd/MM/yyyy");
				var currentMonth = now.ToString("MMMM yyyy");
				var incident = Factory.New<SupportIncident>();
				incident.IM_Product = incidentConfigurationRegistry.IncidentProduct;
				incident.IM_Priority = incidentConfigurationRegistry.IncidentPriority;
				incident.IM_Module = incidentConfigurationRegistry.IncidentModule;
				incident.IM_Description = "Source Code Escrow";
				incident.DetailNoteText = incidentConfigurationRegistry.IncidentMessage;

				Factory.Save();
				logger.Log(LogType.Information, $"Incident created, incident number {incident.IM_IncidentNumber}");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log(LogType.Error, $"Failed to create Escrow incident: {ex.Message}");
				throw;
			}
		}

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();

		BusinessObjectFactory factory;
		readonly IIncidentConfigurationRegistry incidentConfigurationRegistry;
	}
}
