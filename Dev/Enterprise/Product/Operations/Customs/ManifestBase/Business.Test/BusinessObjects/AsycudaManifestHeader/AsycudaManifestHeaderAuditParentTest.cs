using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing;

[TestedType(typeof(AsycudaManifestHeader))]
class AsycudaManifestHeaderAuditParentTest : AuditParentTest<AsycudaManifestHeader>
{
	protected override AsycudaManifestHeader NewTestAuditParent() => Factory.New<AsycudaManifestHeader>();

	public void TestRelatedAuditChildren()
	{
		var auditParent = (IAuditParent)Factory.New<ManifestBase.AsycudaManifestHeader>();
		AssertContainsExactElementsInAnyOrder([
			new(AsycudaArrivalHeaderSchema.ATH_ClusterKey, null),
			new(AsycudaArrivalLineSchema.ATL_ClusterKey, null),
			new(AsycudaBillSchema.ABL_ClusterKey, null),
			new(AsycudaBillScreeningSchema.ASR_ClusterKey, null),
			new(AsycudaContainerSchema.ACN_ClusterKey, null),
			new(AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, null),
			new(AsycudaPackSchema.APA_ClusterKey, null),
			new(AsycudaPackedItemSchema.API_ClusterKey, null),
			new(AsycudaTaxSchema.AET_ClusterKey, null),
			new(AsycudaTransferBillSchema.ATB_ClusterKey, null),
			new(AsycudaTransferHeaderSchema.ATF_ClusterKey, null),
			new(AsycudaPackPackedItemPivotSchema.APP_ClusterKey, null),
		], auditParent.RelatedAuditChildren);
	}
}
