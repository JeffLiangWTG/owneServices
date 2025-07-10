using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusVehicleCollection))]
	sealed class CusVehicleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTotalCountValidationExport() => CombineAssertions(() =>
		{
			JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			var vehicles = JobComInvoiceLine.Vehicles;

			var vehicle1 = vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
			vehicle1.CVH_BrandName = "Brand1";
			vehicle1.CVH_ModelName = "Model1";

			var vehicle2 = vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN2";
			vehicle1.CVH_BrandName = "Brand2";
			vehicle1.CVH_ModelName = "Model2";
			Factory.Save();

			AssertEquals("Count vehicles saved 2", 2, vehicles.Count);
		});

		public void TestTotalCountValidationImport() => CombineAssertions(() =>
		{
			JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			var vehicles = JobComInvoiceLine.Vehicles;

			var vehicle1 = vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
			vehicle1.CVH_BrandName = "Brand1";
			vehicle1.CVH_ModelName = "Model1";

			var vehicle2 = vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN2";
			vehicle1.CVH_BrandName = "Brand2";
			vehicle1.CVH_ModelName = "Model2";
			Factory.Save();

			AssertEquals("Count vehicles saved 2", 2, vehicles.Count);
		});

		protected override BusinessObjectCollection GetCollectionToTest() => JobComInvoiceLine.Vehicles as BusinessObjectCollection;

		JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
		JobDeclaration jobDeclaration;

		JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		JobComInvoiceLine jobComInvoiceLine;
	}
}
