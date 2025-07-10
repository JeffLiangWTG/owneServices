
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision
{
	public class JobFlatFileConverter : FlatFileConverter
	{
		public JobFlatFileConverter(INotifications notification, BusinessObjectFactory factory, ZString shipmentNumber) : base(notification, factory)
		{
			this.ShipmentNumber = shipmentNumber;
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			Xsd.Consol consol = valueObject as Xsd.Consol;
			Xsd.ConsolAndShipment declaration = valueObject as Xsd.ConsolAndShipment;
			if (consol != null && IsValidToExport(consol.ConsolDetail))
			{
				foreach (Xsd.Shipment shipment in consol.Shipments)
				{
					if (ShipmentNumber.IsEmpty)
					{
						dataRows.Add(ExportJob(shipment, consol));
					}
					else if (shipment.ShipmentDetails.AgentReference == ShipmentNumber)
					{
						dataRows.Add(ExportJob(shipment, consol));
						break;
					}
				}
			}
			else if (declaration != null && IsValidToExport(declaration))
			{
				dataRows.Add(ExportJob(declaration.Shipment, declaration.Consol));
			}
			return dataRows;
		}

		#region Implementation

		JobFlatFileDataRow ExportJob(Xsd.Shipment shipment, Xsd.Consol consol)
		{
			JobFlatFileDataRow row = new JobFlatFileDataRow();

			row.JobNumber = shipment.ShipmentDetails.AgentReference;
			row.Description = GetOrderReferenceAsSingleString(shipment.ShipmentDetails.OrderReferences);
			row.Description2 = shipment.ShipmentDetails.Consignee.OrganisationDetails.Name;
			row.CreationDate = GetCreationDateFromEvent(shipment.Events.Event);
			row.OwnerReference = shipment.ShipmentDetails.OwnerReference;
			row.ConsignorName = shipment.ShipmentDetails.Consignor.OrganisationDetails.Name;
			row.MasterAWB = consol.Masterbill;
			row.HouseAWB = shipment.Housebill;
			row.VesselFlight = GetVoyageFlight(consol.ConsolDetail.Item);
			row.TransportMode = GetTransportMode(shipment.ShipmentDetails.TransportMode);
			row.ContainerNumbers = GetContainerNumbers(consol.ConsolDetail.Containers);
			row.LoadingPort = shipment.ShipmentDetails.PortOfOrigin.Port.Value;
			row.DischargePort = shipment.ShipmentDetails.PortofDestination.Port.Value;
			row.GoodsDescription = shipment.ShipmentDetails.GoodsDescription;
			row.ConsolidatedETA = consol.ConsolDetail.PortOfDischarge.EstimatedDateTime;
			row.ConsolidatedETD = consol.ConsolDetail.PortOfLoading.EstimatedDateTime;

			row.CompletionPercentage = Constants.CompletionPercentage;
			row.Status = Constants.Status;
			row.ApplicationMethod = Constants.ApplicationMethod;
			row.JobUsagePosting = Constants.JobUsagePosting;

			return row;
		}

		ZDateTime GetCreationDateFromEvent(Xsd.EventCollection eventsCollection)
		{
			ZDateTime result = ZDateTime.Now;
			foreach (Xsd.Event @event in eventsCollection)
			{
				if (@event.Code == Events.AddedARecordToTheSystem.Code && @event.DateTime < result)
				{
					result = @event.DateTime;
				}
			}
			return result;
		}

		#region GetTransportMode

		protected ZString GetTransportMode(Xsd.TransportMode transportMode)
		{
			ZString result = ZString.Empty;
			switch (transportMode)
			{
				case Xsd.TransportMode.AIR:
					result = Core.Constants.TransportModeDescriptions.Air;
					break;

				case Xsd.TransportMode.SEA:
					result = Core.Constants.TransportModeDescriptions.Sea;
					break;

				case Xsd.TransportMode.RAI:
					result = Core.Constants.TransportModeDescriptions.Rail;
					break;

				case Xsd.TransportMode.ROA:
					result = Core.Constants.TransportModeDescriptions.Road;
					break;

				case Xsd.TransportMode.COU:
					result = Core.Constants.TransportModeDescriptions.Courier;
					break;

				case Xsd.TransportMode.MAI:
					result = Core.Constants.TransportModeDescriptions.Mail;
					break;

				default:
					result = Core.Constants.TransportModeDescriptions.Other;
					break;
			}

			// The word 'Freight' was added to Constants.TransportModeDescriptions.
			// To keep with previous functionilty (max 10 chars) the word 'Freight' 
			// has been removed.
			string wordToRemove = "Freight";
			if (result.EndsWith(wordToRemove))
			{
				result = result.RemoveSafe(result.LastIndexOf(wordToRemove), wordToRemove.Length);
			}

			return result.Trim();
		}

		#endregion

		ZString GetVoyageFlight(Xsd.SailingBase sailingBase)
		{
			ZString result = ZString.Empty;
			Xsd.FlightWithFlightNumber flight = sailingBase as Xsd.FlightWithFlightNumber;
			Xsd.SailingWithVesselVoyage vessel = sailingBase as Xsd.SailingWithVesselVoyage;
			if (flight != null)
			{
				result = flight.FlightNoJourneyNoTruckRegNo;
			}
			else if (vessel != null)
			{
				result = vessel.VoyageNo;
			}
			return result;
		}

		ZString GetContainerNumbers(Xsd.ContainerCollection containers)
		{
			ZString result = ZString.Empty;
			foreach (Xsd.Container container in containers)
			{
				result += container.ContainerNumber + " ";
			}
			return result.Trim();
		}

		ZString GetOrderReferenceAsSingleString(string[] orderReferences)
		{
			ZString result = ZString.Empty;
			if (orderReferences != null && orderReferences.Length > 0)
			{
				foreach (string orderReference in orderReferences)
				{
					result += orderReference + " ";
				}
			}
			return result.Trim();
		}

		bool IsValidToExport(Xsd.ConsolConsolDetail consolDetail)
		{
			return !consolDetail.PortOfLoading.EstimatedDateTime.IsEmpty && !consolDetail.PortOfDischarge.EstimatedDateTime.IsEmpty;
		}

		bool IsValidToExport(Xsd.ConsolAndShipment jobDec)
		{
			bool result = false;
			if (jobDec.Shipment.ShipmentDetails.ShipmentTypeSpecified && jobDec.Shipment.ShipmentDetails.ShipmentType == Xsd.ShipmentType.EXW)
			{
				result = jobDec.Shipment.ShipmentDetails.Consignee.OrganisationDetails.IsSpecified && !jobDec.Shipment.ShipmentDetails.PortofDestination.Port.Value.IsEmpty;
			}
			else
			{
				result = IsValidToExport(jobDec.Consol.ConsolDetail);
			}

			return result;
		}

		readonly ZString ShipmentNumber;

		#endregion
	}
}
