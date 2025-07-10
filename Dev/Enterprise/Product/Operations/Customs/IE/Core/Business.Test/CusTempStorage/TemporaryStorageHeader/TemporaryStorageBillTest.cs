using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageBill))]
	class TemporaryStorageBillTest : EU.Business.CusTempStorage.Testing.TemporaryStorageBillAbstractTest<TemporaryStorageBill, TemporaryStorageHeader>
	{
		public void TestAdditionalInfos()
		{
			AssertType<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>>(bill.AdditionalInfos);
		}

		public void TestPackedItems()
		{
			AssertType<TemporaryStoragePackedItem>("Should get IE TemporaryStoragePackedItem for PackedItems.", bill.PackedItems.AddNew());
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var supporter = Factory.New<TemporaryStorageBill>() as Integration.Customs.ICusSupportingInfoTypeSupporter;
			AssertEquals(typeof(TemporaryStorageAdditionalInfo), supporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestTemporaryStorageBillValidationType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			var bill = header.Bills.AddNew();
			AssertType(typeof(TemporaryStorageBillValidationUCC5), bill.Validation);

			var header1 = Factory.New<TemporaryStorageHeader>();
			header1.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			var bill1 = header1.Bills.AddNew();
			AssertType(typeof(TemporaryStorageBillValidation), bill1.Validation);
		}

		public void TestGrossWeightInKilogramsSafe()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			const decimal testGrossWeight = 8.3m;
			const string testGrossWeightUQ = "LB";

			var expectedInKilogramsSafe = new ZWeight(testGrossWeight, testGrossWeightUQ).InKilogramsSafe;
			bill.ABL_GrossWeight = testGrossWeight;
			bill.ABL_GrossWeightUQ = testGrossWeightUQ;

			AssertEquals(expectedInKilogramsSafe, bill.GrossWeightInKilogramsSafe);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = (TemporaryStorageBill)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tempHeader = factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			return tempHeader.Bills.AddNew();
		}

		TemporaryStorageBill bill;
	}
}
