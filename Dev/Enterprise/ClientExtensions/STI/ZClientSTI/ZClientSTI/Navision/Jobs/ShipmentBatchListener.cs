using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision
{
	public class ShipmentBatchListener : JobBatchListener
	{
		public ShipmentBatchListener(ZDateTime dateTimeExportStarted) : base(dateTimeExportStarted)
		{
		}

		protected override Type BusinessObjectCollectionType
		{
			get { return typeof(ShipmentCollection); }
		}

		public override string BusinessObjectTableName
		{
			get { return JobShipmentSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(CommonShipment); }
		}

		protected override void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
		{
			CommonShipment shipment = (CommonShipment)matchingBusinessObject;
			ConsolFlatFileExporter exporter = new ConsolFlatFileExporter(Factory, Instructions, FileName, shipment.JS_UniqueConsignRef);
			CollectionWrapperBusinessObjectReader businessObjectReader = new CollectionWrapperBusinessObjectReader(shipment.Consols);
			exporter.Export(businessObjectReader, notifications);
		}
	}
}
