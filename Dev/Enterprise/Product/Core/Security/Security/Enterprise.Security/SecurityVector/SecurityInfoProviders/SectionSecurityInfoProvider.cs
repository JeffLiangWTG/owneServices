using System.Collections.Generic;
using Enterprise.Core.Modules;

namespace Enterprise.Security.Provider
{
	class SectionSecurityInfoProvider : SecurityInfoProvider
	{
		public SectionSecurityInfoProvider(SecurityInfoProvider parent, ModuleSection section)
			: base(parent, GetSecurityCheckpoint(parent.Security, section.SecurityCheckpoint))
		{
			this.section = section;
		}

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			if (section.SecurityCheckpoint.LookupKey == Security.LinerAndAgency.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.AgencyPrincipalAccess);
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceShipping);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.Warehouse.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.WhsAllowedClients);
				yield return new CheckpointSecurityInfoProvider(this, Security.WhsAllowedWarehouses);
				yield return new CheckpointSecurityInfoProvider(this, Security.WhsRFScanning);
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceWarehouse);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.TransitWarehouse.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceTransitWarehouse);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.References.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.AllowVehicleMonitoringAndManagement);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.Forwarding.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.PickupDeliveryConfirmations);
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceForwarding);
				yield return new CheckpointSecurityInfoProvider(this, Security.ElectronicMessaging);
				yield return new CheckpointSecurityInfoProvider(this, Security.MaintainELoadList);
				yield return new CheckpointSecurityInfoProvider(this, Security.CustomsCargoManifestOnForwarding);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.CustomsMain.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceCustoms);
				yield return new CheckpointSecurityInfoProvider(this, Security.CustomsDIS);
				yield return new CheckpointSecurityInfoProvider(this, Security.CACustomsDIF);
				yield return new CheckpointSecurityInfoProvider(this, Security.SupervisorOverrides);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.CFSCTO.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceCFS);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.Transport.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceLocalTransport);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.OrderManager.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceOrders);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.DtbLandTransportOperations.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RoadDistanceCalculationServiceLandTransport);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.ClientRelationshipManagement.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.SalesRelations);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.TariffsAndRates.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.RatesSecurity);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.WiseRatesSection.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.WiseRatesCargoSphereContractManagement);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.AirCcsuk.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.AirCcsukShed);
				yield return new CheckpointSecurityInfoProvider(this, Security.AirCcsukHCITerminal);
				yield return new CheckpointSecurityInfoProvider(this, Security.AirCcsukCreateFallbackUnderbondRequest);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.BufferManagementConfig.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.BMFilterRule);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.DocManager.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.eDocsLevelSpecific);
				yield return new CheckpointSecurityInfoProvider(this, Security.eDocsPermanentDelete);
				yield return new CheckpointSecurityInfoProvider(this, Security.CutSpecificEDocTypes);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.CustomsFiles.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.HTSReferenceFilesDataVersion);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.BusinessIntelligenceAndAnalytics.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.BIAPI);
				yield return new CheckpointSecurityInfoProvider(this, Security.AnalyticsReports);
				yield return new CheckpointSecurityInfoProvider(this, Security.BiManager);
			}
			else if (section.SecurityCheckpoint.LookupKey == Security.UserAdmin.LookupKey)
			{
				yield return new CheckpointSecurityInfoProvider(this, Security.UserAdminForms);
			}

			foreach (INamedModule module in section.Modules.Values)
			{
				if (module.SecurityCheckpoint != null)
				{
					var moduleSecurity = new ModuleSecurityInfoProvider(this, module);
					if (moduleSecurity.Checkpoint != null && moduleSecurity.Checkpoint.Parent.LookupKey == Checkpoint.LookupKey)
					{
						yield return moduleSecurity;
					}
				}
			}
		}

		public override void FetchForGetChildren()
		{
			foreach (INamedModule module in section.Modules.Values)
			{
				if (module.SecurityCheckpoint != null)
				{
					var moduleSecurity = new ModuleSecurityInfoProvider(this, module);
					if (moduleSecurity.Checkpoint != null && moduleSecurity.Checkpoint.Parent.LookupKey == Checkpoint.LookupKey)
					{
						moduleSecurity.FetchForGetChildren();
					}
				}
			}
		}

		public override string Name { get { return section.DisplayTextWithoutAmpersand; } }

		readonly ModuleSection section;
	}
}
