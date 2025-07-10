using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaPackValidation))]
sealed class CGMAsycudaPackValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPA_PackQty()
	{
		const string expectedMessage = "Entered quantity is not equal to Packages specified at Bills level.";
		CombineAssertions(() =>
		{
			Header.Containers.AddNew();
			AssertEquals("Container Count", 1, Header.Containers.Count);
			Bill.ABL_ManifestQty = 10;
			Pack.APA_PackQty = 5;
			AssertHasMessageErrorContaining("When ABL_ManifestQty and APA_PackQty are different, one container", pack.APA_PackQtyInfo, expectedMessage);

			Pack.APA_PackQty = 10;
			AssertNoMessageErrorContaining("When ABL_ManifestQty and APA_PackQty are same, one container", pack.APA_PackQtyInfo, expectedMessage);

			Header.Containers.AddNew();
			AssertEquals("Container Count", 2, Header.Containers.Count);
			Pack.APA_PackQty = 5;
			AssertNoMessageErrorContaining("When ABL_ManifestQty and APA_PackQty are different, two containers", pack.APA_PackQtyInfo, expectedMessage);
		});
	}

	public void TestCheckAPA_Weighty()
	{
		const string expectedMessage = "Entered weight is not equal to gross weight specified at Bills level.";
		CombineAssertions(() =>
		{
			Header.Containers.AddNew();
			AssertEquals("Container Count", 1, Header.Containers.Count);
			Bill.ABL_GrossWeight = 10;
			Pack.APA_Weight = 5;
			AssertHasMessageErrorContaining("When ABL_GrossWeight and APA_Weight are different, one container", pack.APA_WeightInfo, expectedMessage);

			Pack.APA_Weight = 10;
			AssertNoMessageErrorContaining("When ABL_GrossWeight and APA_Weight are same, one container", pack.APA_WeightInfo, expectedMessage);

			Header.Containers.AddNew();
			AssertEquals("Container Count", 2, Header.Containers.Count);
			Pack.APA_Weight = 5;
			AssertNoMessageErrorContaining("When ABL_GrossWeight and APA_Weight are different, two containers", pack.APA_WeightInfo, expectedMessage);
		});
	}

	public void TestCheckContainerPKDuplicateInSameBill()
	{
		const string expectedMessage = "This container is already present in the grid for the current house bill.";
		var container0 = Header.Containers.AddNew();
		var container1 = Header.Containers.AddNew();
		var pack1 = Bill.Packs.AddNew();

		CombineAssertions("Same Bill", () =>
		{
			Pack.ContainerPK = container0.PK;
			pack1.ContainerPK = container0.PK;
			AssertNoErrorContaining("When two packs has same container, First Pack", Pack.ContainerPKInfo, expectedMessage);
			AssertHasErrorContaining("When two packs has same container, Second Pack", pack1.ContainerPKInfo, expectedMessage);

			pack1.ContainerPK = container1.PK;
			AssertNoErrorContaining("When two packs has different containers, First Pack", Pack.ContainerPKInfo, expectedMessage);
			AssertNoErrorContaining("When two packs has different containers, Second Pack", pack1.ContainerPKInfo, expectedMessage);
		});
	}

	public void TestCheckContainerPKFCLDuplicateInAnyBill()
	{
		const string expectedMessage = "This FCL container is already selected in house bill number '123'";
		Bill.ABL_BillNumber = "123";
		var container0 = Header.Containers.AddNew();
		container0.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
		var container1 = Header.Containers.AddNew();
		container1.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
		var bill1 = Header.Bills.AddNew();
		var pack1 = bill1.Packs.AddNew();

		CombineAssertions("Different Bills", () =>
		{
			Pack.ContainerPK = container0.PK;
			pack1.ContainerPK = container0.PK;
			AssertNoErrorContaining("When two packs has same container - FCL, First Pack", Pack.ContainerPKInfo, expectedMessage);
			AssertHasErrorContaining("When two packs has same container - FCL, Second Pack", pack1.ContainerPKInfo, expectedMessage);

			pack1.ContainerPK = container1.PK;
			AssertNoErrorContaining("When two packs has different containers - FCL, First Pack", Pack.ContainerPKInfo, expectedMessage);
			AssertNoErrorContaining("When two packs has different containers - FCL, Second Pack", pack1.ContainerPKInfo, expectedMessage);

			container0.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;
			pack1.ContainerPK = container0.PK;
			AssertNoErrorContaining("When two packs has same container - LCL, First Pack", Pack.ContainerPKInfo, expectedMessage);
			AssertNoErrorContaining("When two packs has same container - LCL, Second Pack", pack1.ContainerPKInfo, expectedMessage);
		});
	}

	CGMAsycudaPack Pack => pack ??= Bill.Packs.AddNew();
	CGMAsycudaPack pack;

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
