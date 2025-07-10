using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.MFI.CaroTrans
{
	public abstract class CaroTransShipmentListener : LogBatchListener
	{
		protected CaroTransShipmentListener(ZString exportPath)
		{
			ExportDirectory = exportPath;
		}

		protected ZString ExportDirectory;

		public override string HumanReadableName
		{
			get { return "CaroTrans Shipment Export"; }
		}

		#region Implementation

		#region IsSendingAgentDeclaredInTheRegistry

		protected bool IsSendingAgentDeclaredInTheRegistry(CommonConsol consol)
		{
			bool result = false;

			if (consol != null && consol.SendingForwarderPK.IsValid)
			{
				result = MFIDataRegistry.Instance.CaroTransAgents.Any(pk => pk == consol.SendingForwarderPK);
			}

			return result;
		}

		#endregion

		protected override void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
		{
			ShipmentCollection shipments = GetShipmentsToExport(matchingBusinessObject);

			if (shipments.Count > 0)
			{
				var shipmentsGrpByImpCntry = shipments.Cast<ForwardingShipment>().GroupBy(s => s.JS_RL_NKDestination.Left(2))
										.ToDictionary(grp => grp.Key, grp => grp.ToArray<ForwardingShipment>());

				foreach (var current in shipmentsGrpByImpCntry)
				{
					var fileExtension = MFIDataRegistry.Instance.CaroTransExportFileExtensions.GetDescriptionFromCode(current.Key);

					if (string.IsNullOrEmpty(fileExtension))
					{
						fileExtension = Constants.CargoTransDefaultFileExtension;
						notifications.Notify(new Notification(CargoWise.ComponentModel.NotificationType.Warning, Res.GetString("081c6ddc-3ef5-4731-accb-bc44b79cabd6", "File extension for Import Country/Region '{0}' is not setup. Default file extension '{1}' will be used.", current.Key, Constants.CargoTransDefaultFileExtension)));
					}

					var reader = new ArrayBusinessObjectReader(current.Value, typeof(ForwardingShipment));
					CaroTransShipmentDataExporter exporter = GetNewExporter(fileExtension);
					exporter.Export(reader, notifications);
				}
			}
		}

		#region GetNewExporter

		protected CaroTransShipmentDataExporter GetNewExporter(ZString fileExtension)
		{
			return new CaroTransShipmentDataExporter(Factory, fileExtension, ExportDirectory);
		}

		#endregion

		protected abstract ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject);

		#endregion
	}
}
