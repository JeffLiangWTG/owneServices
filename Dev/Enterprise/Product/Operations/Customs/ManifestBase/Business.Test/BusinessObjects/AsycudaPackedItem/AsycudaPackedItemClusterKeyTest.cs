using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var bill = (AsycudaBill)NewParentObject();
			var packedItem = bill.PackedItems.AddNew();

			return packedItem;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			return bill;
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new SchemaGuidColumn[]
		{
			AsycudaPackPackedItemPivotSchema.APP_APA_Pack,
		};

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var tax = Factory.New<AsycudaTax>();
			tax.AET_API_AsycudaPackedItem = ClusterKeyEntityToTest.PK;
			tax.AET_MethodOfCalculation = "A";
			return new IClusterKeyWorker[] { tax };
		}

		new AsycudaPackedItem ClusterKeyEntityToTest => (AsycudaPackedItem)base.ClusterKeyEntityToTest;
	}
}
