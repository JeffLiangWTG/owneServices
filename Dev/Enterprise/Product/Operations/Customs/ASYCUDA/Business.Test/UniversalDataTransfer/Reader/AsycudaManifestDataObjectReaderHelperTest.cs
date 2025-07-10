using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaManifestDataObjectReaderHelperTest : TestCaseWithUniversalObjectFactory
	{
		public void TestHandlingOfUnprocessedBills()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";
			var bill2 = header1.Bills.AddNew();
			bill2.ABL_BillNumber = "HB2";
			var bill3 = header1.Bills.AddNew();
			bill3.ABL_BillNumber = "HB3";
			var bill4 = header1.Bills.AddNew();
			bill4.ABL_BillNumber = "HB4";

			var header2 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill5 = header2.Bills.AddNew();
			bill5.ABL_BillNumber = "HB5";
			var bill6 = header2.Bills.AddNew();
			bill6.ABL_BillNumber = "HB6";

			var header3 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill7 = header3.Bills.AddNew();
			bill7.ABL_BillNumber = "HB7";

			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			helper.BillsReaderHelper.MarkUnprocessedExistingObjectFor(Factory, header1);
			helper.BillsReaderHelper.MarkUnprocessedExistingObjectFor(Factory, header2);
			helper.BillsReaderHelper.MarkProcessed(bill2);
			helper.BillsReaderHelper.MarkProcessed(bill3);
			helper.BillsReaderHelper.MarkProcessed(bill6);
			helper.BillsReaderHelper.MarkProcessed(bill7);
			foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}

			var logger = new TestErrorLogger();
			helper.BillsReaderHelper.DeleteUnprocessedObjectsFor(header3, logger);
			foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}
			AssertEquals("logger.Logs", "", logger.Logs);

			helper.BillsReaderHelper.DeleteUnprocessedObjectsFor(header1, logger);
			AssertEquals("bill1.IsDeleted", true, bill1.IsDeleted);
			AssertEquals("bill4.IsDeleted", true, bill4.IsDeleted);
			foreach (var bill in new[] { bill2, bill3, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}
			AssertMultilineASCIIEquals("logger.Logs", @"Information - Deleted Manifest Bill HB1 from UniversalShipment.
Information - Deleted Manifest Bill HB4 from UniversalShipment.", logger.Logs);

			logger.ClearLogs();
			helper.BillsReaderHelper.DeleteUnprocessedObjectsFor(header2, logger);
			AssertEquals("bill5.IsDeleted", true, bill5.IsDeleted);
			foreach (var bill in new[] { bill2, bill3, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}
			AssertEquals("logger.Logs", "Information - Deleted Manifest Bill HB5 from UniversalShipment.", logger.Logs);
		}

		public void TestHandlingOfUnprocessedContainers()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var container1 = header1.Containers.AddNew();
			container1.ACN_ContainerNumber = "CONT1";
			var container2 = header1.Containers.AddNew();
			container2.ACN_ContainerNumber = "CONT2";
			var container3 = header1.Containers.AddNew();
			container3.ACN_ContainerNumber = "CONT3";

			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			helper.ContainersReaderHelper.MarkUnprocessedExistingObjectFor(Factory, header1);
			helper.ContainersReaderHelper.MarkProcessed(container2);
			helper.ContainersReaderHelper.MarkProcessed(container3);
			AssertEquals("NotDeleted", false, container1.IsDeleted);
			AssertEquals("NotDeleted", false, container2.IsDeleted);
			AssertEquals("NotDeleted", false, container3.IsDeleted);

			var logger = new TestErrorLogger();
			helper.ContainersReaderHelper.DeleteUnprocessedObjectsFor(header1, logger);

			AssertEquals("IsDeleted", true, container1.IsDeleted);
			AssertEquals("NotDeleted", false, container2.IsDeleted);
			AssertEquals("NotDeleted", false, container3.IsDeleted);

			AssertEquals("logger.Logs", "Information - Deleted Manifest Container CONT1 from UniversalShipment.", logger.Logs);
		}

		public void TestHandlingOfUnprocessedPacks()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header1.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 2;
			var pack3 = bill.Packs.AddNew();
			pack3.ConsignmentReference = 3;

			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(Factory, bill);
			helper.PacksReaderHelper.MarkProcessed(pack2);
			helper.PacksReaderHelper.MarkProcessed(pack3);

			AssertEquals("NotDeleted", false, pack1.IsDeleted);
			AssertEquals("NotDeleted", false, pack2.IsDeleted);
			AssertEquals("NotDeleted", false, pack3.IsDeleted);

			var logger = new TestErrorLogger();
			helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(bill, logger);

			AssertEquals("IsDeleted", true, pack1.IsDeleted);
			AssertEquals("NotDeleted", false, pack2.IsDeleted);
			AssertEquals("NotDeleted", false, pack3.IsDeleted);

			AssertEquals("logger.Logs", "Information - Deleted Manifest Pack Consignment Reference 1 from UniversalShipment.", logger.Logs);
		}

		public void TestFillVoyageFlightNoCore_WhenVoyageFlightNoIsNull()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header1.AMA_Voyage = "EXISTING";
			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var logger = new TestErrorLogger();
			helper.FillVoyageFlightNo(null, Core.Constants.TransportModes.Road, logger, header1);
			AssertEquals("AMA_Voyage", header1.AMA_Voyage, "EXISTING");
		}

		public void TestFillVoyageFlightNoCore_TransportMode_Road()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var logger = new TestErrorLogger();
			helper.FillVoyageFlightNo("NEW001", Core.Constants.TransportModes.Road, logger, header1);
			AssertNotEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
			AssertEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
		}

		public void TestFillVoyageFlightNoCore_TransportMode_Sea()
		{
			var header1 = Factory.BOFactory.New<AsycudaManifestHeader>();
			header1.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var logger = new TestErrorLogger();
			helper.FillVoyageFlightNo("NEW001", Core.Constants.TransportModes.Sea, logger, header1);
			AssertEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
			AssertNotEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
		}

		public void TestFilterAndSet_NotFilteredByDefault()
		{
			int countSet = 0;
			int countAllColumns = 0;
			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			foreach (var col in AsycudaManifestHeaderSchema.All)
			{
				helper.FilterAndSet(col, null, (_) => countSet++);
				countAllColumns++;
			}
			AssertEquals(countAllColumns, countSet);
		}

		public void TestFilterAndSet_CanBeOverriddenToFilter()
		{
			int countSet = 0;
			var helper = new TestHelper_FiltersAll(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			foreach (var col in AsycudaManifestHeaderSchema.All)
			{
				helper.FilterAndSet(col, null, (_) => countSet++);
			}
			AssertEquals(0, countSet);
		}

		sealed class TestHelper_FiltersAll : AsycudaManifestDataObjectReaderHelper
		{
			public TestHelper_FiltersAll(ZString countryCode, BusinessObjectFactory factory) : base(countryCode, factory)
			{
			}

			protected override bool ShouldReadColumn(SchemaColumn column, Shipment dataObject) => false;
		}
	}
}
