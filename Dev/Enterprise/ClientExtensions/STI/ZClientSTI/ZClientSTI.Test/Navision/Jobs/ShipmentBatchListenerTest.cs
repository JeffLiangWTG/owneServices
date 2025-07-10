using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class ShipmentBatchListenerTest : JobBatchListenerTestCase
	{
		protected override NavisionBatchListener BatchListener
		{
			get
			{
				return new ShipmentBatchListenerTestClass();
			}
		}

		protected override Type BusinessObjectCollectionType
		{
			get
			{
				return typeof(BusinessObjectCollection);
			}
		}

		protected override string BusinessObjectTableName
		{
			get
			{
				return JobShipmentSchema.Constants.TableName;
			}
		}

		protected override Type BusinessObjectType
		{
			get
			{
				return typeof(CommonShipment);
			}
		}

		protected override BusinessObject BusinessObjectForTesting()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			return shipment;
		}

		protected override Type ExporterType
		{
			get
			{
				return typeof(ConsolFlatFileExporter);
			}
		}

		protected override bool BusinessObjectCollectionIsSupported
		{
			get
			{
				return false;
			}
		}

		class ShipmentBatchListenerTestClass : ShipmentBatchListener, NavisionBatchListenerTest.IBatchListenerTestClass
		{
			public ShipmentBatchListenerTestClass() : base(ZDateTime.Now)
			{
			}

			public new void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
			{
				base.Process(matchingBusinessObject, log, notifications);
			}

			public Type BusinessObjectCollectionTypeExposed
			{
				get
				{
					return base.BusinessObjectCollectionType;
				}
			}

			public NavisionFlatFileExporter ExporterExposed
			{
				get
				{
					return base.Exporter;
				}
			}
		}
	}
}
