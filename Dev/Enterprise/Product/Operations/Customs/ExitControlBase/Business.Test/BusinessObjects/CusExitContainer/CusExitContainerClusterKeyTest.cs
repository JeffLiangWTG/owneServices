using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitContainer))]
public abstract class CusExitContainerClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

	protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
	{
			CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem,
			CusExitReportItemSchema.ERI_CER_Report,
	};

	protected override IClusterKeyEntity NewClusterKeyEntity() => ((CusExitHeader)NewParentObject()).CusExitContainers.AddNew();

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		return header;
	}
}

