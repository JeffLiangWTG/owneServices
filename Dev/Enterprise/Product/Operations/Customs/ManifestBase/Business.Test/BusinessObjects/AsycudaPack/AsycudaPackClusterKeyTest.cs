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
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var bill = (AsycudaBill)NewParentObject();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			return pack;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			return bill;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var packedItemPivot = ClusterKeyEntityToTest.PackedItems.AddNew();
			var packedItem = ClusterKeyEntityToTest.Bill.PackedItems.AddNew();
			packedItemPivot.APP_API_Item = packedItem.PK;

			return new IClusterKeyWorker[] { packedItemPivot };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new SchemaGuidColumn[]
		{
			AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container,
			AsycudaArrivalLineSchema.ATL_ATH
		};

		new AsycudaPack ClusterKeyEntityToTest => (AsycudaPack)base.ClusterKeyEntityToTest;
	}
}
