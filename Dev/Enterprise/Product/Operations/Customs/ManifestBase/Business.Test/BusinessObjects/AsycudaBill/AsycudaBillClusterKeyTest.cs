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
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var manifestHeader = (AsycudaManifestHeader)NewParentObject();
			var bill = manifestHeader.Bills.AddNew();

			return bill;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var pack = ClusterKeyEntityToTest.Packs.AddNew();
			var packedItem = ClusterKeyEntityToTest.PackedItems.AddNew();
			var billScreening = Factory.New<AsycudaBillScreening>();
			billScreening.ASR_ABL = ClusterKeyEntityToTest.PK;
			var tax = Factory.New<AsycudaTax>();
			tax.AET_ABL = ClusterKeyEntityToTest.PK;
			tax.AET_MethodOfCalculation = "A";

			return new IClusterKeyWorker[] { pack, packedItem, billScreening, tax };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new SchemaGuidColumn[]
		{
			AsycudaTransferBillSchema.ATB_ATF_TransferHeader,
			AsycudaArrivalLineSchema.ATL_ATH,
			AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container
		};

		new AsycudaBill ClusterKeyEntityToTest => (AsycudaBill)base.ClusterKeyEntityToTest;
	}
}
