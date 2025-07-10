using System.IO;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransShipmentDataExporter : FlatFileDataExporter
	{
		public CaroTransShipmentDataExporter(BusinessObjectFactory factory, ZString fileExtension, ZString exportDirectory)
			: base(factory)
		{
			ExportedFile = Path.Combine(exportDirectory, Constants.ShipmentDataExportFile + "." + fileExtension);
			if (!File.Exists(ExportedFile))
			{
				using (FileStream fs = File.Create(ExportedFile))
				{
				}
			}
		}

		public override ZString EnglishDescription
		{
			get { return "CaroTrans Shipments Automated Data Export"; }
		}

		#region Implementation

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { return new ForwardingShipmentValueObjectDataAdapter(); }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CaroTransShipmentFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new CaroTransShipmentConverter(notificationSubscriber, new BusinessObjectFactory());
		}

		protected override bool AppendToFile
		{
			get { return true; }
		}

		#endregion
	}
}
