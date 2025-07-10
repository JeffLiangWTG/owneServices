using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class ConsolBatchListenerTest : JobBatchListenerTestCase
	{
		protected override NavisionBatchListener BatchListener
		{
			get
			{
				return new ConsolBatchListenerTestClass();
			}
		}

		protected override Type BusinessObjectCollectionType
		{
			get
			{
				return typeof(MainFormConsolCollection);
			}
		}

		protected override string BusinessObjectTableName
		{
			get
			{
				return JobConsolSchema.Constants.TableName;
			}
		}

		protected override Type BusinessObjectType
		{
			get
			{
				return typeof(CommonConsol);
			}
		}

		protected override BusinessObject BusinessObjectForTesting()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			CommonShipment shipment = consol.Shipments.AddNew();
			return consol;
		}

		protected override Type ExporterType
		{
			get
			{
				return typeof(ConsolFlatFileExporter);
			}
		}

		class ConsolBatchListenerTestClass : ConsolBatchListener, NavisionBatchListenerTest.IBatchListenerTestClass
		{
			public ConsolBatchListenerTestClass() : base(ZDateTime.Now)
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
