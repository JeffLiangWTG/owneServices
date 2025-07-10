using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	[TestedType(typeof(CaroTransShipmentDataExporter))]
	public class CaroTransShipmentDataExporterTest : FlatFileDataExporterTestCase
	{
		public void TestEnglishDescription()
		{
			AssertEquals("EnglishDescription", "CaroTrans Shipments Automated Data Export", Exporter.EnglishDescription);
		}

		public void TestExportData()
		{
			ShipmentCollection shipments = (ShipmentCollection)GetPopulatedCollectionToSaveAndExport();
			Factory.Save();
			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(shipments);
			FileInfo info = new FileInfo(Exporter.ExportedFileForTesting);
			AssertEquals("Temp file should exist", true, info.Exists);
			AssertEquals("Temp file should be empty", 0, info.Length);
			Exporter.ExportData(reader, new NotificationBuffer());
			info.Refresh();
			AssertEquals("Temp file should exist", true, info.Exists);
			Assert("Temp file should contain the exported data", info.Length > 0);
		}

		public void TestFlatFileFormat()
		{
			AssertEquals("FlatFileFormat", typeof(CaroTransShipmentFlatFileFormat), Exporter.FlatFileFormat.GetType());
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CaroTransShipmentDataExporter(Factory, Constants.CargoTransDefaultFileExtension, Env.TempPath);
		}

		public override FlatFileDataExporter GetDataExporter()
		{
			return new MockCaroTransShipmentDataExporter(Factory);
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			ShipmentCollection result = new ShipmentCollection(Factory);
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.FillWithValidTestData();
			result.Add(shipment);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Exporter = (MockCaroTransShipmentDataExporter)GetDataExporter();
		}

		protected override void TearDown()
		{
			base.TearDown();
			File.Delete(Exporter.ExportedFileForTesting);
		}

		#endregion
		#region Implementation
		MockCaroTransShipmentDataExporter Exporter;
		class MockCaroTransShipmentDataExporter : CaroTransShipmentDataExporter
		{
			public MockCaroTransShipmentDataExporter(BusinessObjectFactory factory) : base(factory, Constants.CargoTransDefaultFileExtension, Env.TempPath)
			{
			}

			public new void ExportData(BusinessObjectReader bizObjReader, INotifications notificationSubscriber)
			{
				base.ExportData(bizObjReader, notificationSubscriber);
			}

			public new IFlatFileFormat FlatFileFormat
			{
				get
				{
					return base.FlatFileFormat;
				}
			}
		}
		#endregion
	}
}
