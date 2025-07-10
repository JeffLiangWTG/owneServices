using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AsycudaArrivalLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckATL_APA_AsycudaPack()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();

			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();
			var arrivalLine2 = arrivalHeader.ArrivalDetails.AddNew();

			arrivalLine.ATL_ABL_AsycudaBill = bill.PK;
			arrivalLine.ATL_APA_AsycudaPack = pack.PK;

			AssertNoError(arrivalLine.ATL_APA_AsycudaPackInfo, "Bill plus Pack should be unique under one Arrivals Header");

			arrivalLine2.ATL_ABL_AsycudaBill = bill.PK;
			arrivalLine2.ATL_APA_AsycudaPack = pack.PK;

			AssertHasError(arrivalLine2.ATL_APA_AsycudaPackInfo, "Bill plus Pack should be unique under one Arrivals Header");

			arrivalLine2.ATL_APA_AsycudaPack = pack2.PK;

			AssertNoError(arrivalLine2.ATL_APA_AsycudaPackInfo, "Bill plus Pack should be unique under one Arrivals Header");
		}

		public void TestCheckATL_WeightUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();

			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();

			arrivalLine.ATL_Weight = 410;
			arrivalLine.ATL_WeightUQ = CargoWise.Types.ZString.Empty;

			AssertHasError(arrivalLine.ATL_WeightUQInfo, "Please enter a Weight Unit.");

			arrivalLine.ATL_WeightUQ = "KG";

			AssertNoError(arrivalLine.ATL_WeightUQInfo, "Please enter a Weight Unit.");
		}
	}
}
