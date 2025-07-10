using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTax))]
	sealed class AsycudaTaxClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyWhenChangingParentTable()
		{
			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("AsycudaTax ClusterKey", 0, ClusterKeyEntityToTest.AET_ClusterKey);
				AssertEquals("bill (current parent) ClusterKey", 0, bill.ABL_ClusterKey);
			});

			Factory.Save();
			CombineAssertions("After 1st Save (Bill as parent)", () =>
			{
				AssertEquals("AsycudaTax ClusterKey", 1, ClusterKeyEntityToTest.AET_ClusterKey);
				AssertEquals("Bill (current parent) ClusterKey", 1, bill.ABL_ClusterKey);
			});

			var anotherManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var anotherBill = anotherManifestHeader.Bills.AddNew();
			var anotherPackedItem = anotherBill.PackedItems.AddNew();
			ClusterKeyEntityToTest.AET_ABL = ZGuid.Empty;
			ClusterKeyEntityToTest.AET_API_AsycudaPackedItem = anotherPackedItem.PK;
			Factory.Save();

			CombineAssertions("After setting FK to an PackedItem parent.", () =>
			{
				AssertEquals("AsycudaTax ClusterKey", 2, ClusterKeyEntityToTest.AET_ClusterKey);
				AssertEquals("Bill ClusterKey", 1, bill.ABL_ClusterKey);
				AssertEquals("PackedItem (current parent) ClusterKey", 2, anotherPackedItem.API_ClusterKey);
			});
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var bill = (AsycudaBill)NewParentObject();
			var tax = Factory.New<AsycudaTax>();
			tax.AET_ABL = bill.PK;
			tax.AET_MethodOfCalculation = "A";

			return tax;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			return bill;
		}

		AsycudaBill bill;

		protected override bool IsFkToParentMandatory() => true;

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		new AsycudaTax ClusterKeyEntityToTest => (AsycudaTax)base.ClusterKeyEntityToTest;
	}
}
