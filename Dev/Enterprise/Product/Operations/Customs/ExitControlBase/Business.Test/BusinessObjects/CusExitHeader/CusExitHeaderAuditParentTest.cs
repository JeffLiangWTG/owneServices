using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Test;

[TestedType(typeof(CusExitHeader))]
sealed class CusExitHeaderAuditParentTest : AuditParentTest<CusExitHeader>
{
	protected override CusExitHeader NewTestAuditParent() => Factory.New<CusExitHeader>();

	public void TestRelatedAuditChildren()
	{
		IAuditParent header = Factory.New<CusExitHeader>();
		AssertContainsExactElementsInAnyOrder(
		[
			new (CusExitConsignmentSchema.CXC_ClusterKey, null),
			new (CusExitConsignmentItemSchema.CCI_ClusterKey, null),
			new (CusExitConsignmentPackageSchema.CXP_ClusterKey, null),
			new (CusExitConsignmentPivotSchema.CNP_ClusterKey, null),
			new (CusExitContainerSchema.CXN_ClusterKey, null),
			new (CusExitReportSchema.CER_ClusterKey, null),
			new (CusExitReportItemSchema.ERI_ClusterKey, null),
		], header.RelatedAuditChildren);
	}
}
