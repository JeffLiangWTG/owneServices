using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(MockJobFlatFileExporter))]
	public class JobFlatFileExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter()
		{
			return new MockJobFlatFileExporter(Factory, null, "", "");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetDataExporter();
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			MainFormConsolCollection consols = new MainFormConsolCollection(Factory);
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "GBLON";
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2006, 2, 22, 10, 22, 29);
			transport.JW_ETA = new ZDateTime(2006, 2, 22, 23, 23, 23);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Qantas";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "Air";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "QF1234";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = new ZDateTime(2006, 2, 22, 10, 22, 29);
			origin.JA_A_DEP = new ZDateTime(2006, 2, 22, 10, 22, 29);
			origin.JA_RL_NKPortOfLoading = "USLAX";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = new ZDateTime(2006, 2, 22, 23, 23, 23);
			destination.JB_A_ARV = new ZDateTime(2006, 2, 22, 23, 23, 23);
			destination.JB_RL_NKPortOfDischarge = "GBLON";
			voyage.GenerateSailings();
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.JW_ATA = new ZDateTime(2006, 2, 22, 23, 23, 23);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			consols.Add(consol);
			return consols;
		}

		public void TestFlatFileFormat()
		{
			MockJobFlatFileExporter exporter = new MockJobFlatFileExporter(Factory, null, "", "");
			IFlatFileFormat format = exporter.FlatFileFormat;
			AssertSame("Flat File Format was not lazy loaded", format, exporter.FlatFileFormat);
			AssertEquals("Format should be CsvFlatFileFormat type", typeof(CsvFlatFileFormat), format.GetType());
		}

		public void TestAppendToFile()
		{
			MockJobFlatFileExporter exporter = new MockJobFlatFileExporter(Factory, null, "", "");
			AssertEquals("Append to file should be true", true, exporter.AppendToFile);
		}

		public void TestConverter()
		{
			MockJobFlatFileExporter exporter = new MockJobFlatFileExporter(Factory, null, "", "");
			IFlatFileConverter converter = exporter.CreateConverter(new NotificationBuffer());
			AssertEquals("Converter should be a JobFlatFileConverter", typeof(JobFlatFileConverter), converter.GetType());
		}

		class MockJobFlatFileExporter : JobFlatFileExporter
		{
			public MockJobFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile, ZString shipmentNumber) : base(factory, instructions, exportFile, shipmentNumber)
			{
			}

			protected override IValueObjectDataAdapter DataAdapter
			{
				get
				{
					return new ForwardingConsolValueObjectDataAdapter();
				}
			}

			public new IFlatFileFormat FlatFileFormat
			{
				get
				{
					return base.FlatFileFormat;
				}
			}

			public new bool AppendToFile
			{
				get
				{
					return base.AppendToFile;
				}
			}

			public new IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return base.CreateConverter(notifications);
			}
		}
	}
}
