using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPackage))]
public abstract class CusExitConsignmentPackageClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var consignmentPackage = ((CusExitHeader)NewParentObject()).CusExitConsignmentPackages.AddNew();
		consignmentPackage.CXP_Sequence = 1;
		return consignmentPackage;
	}

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		return header;
	}

	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

	protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
	{
			CusExitReportItemSchema.ERI_CER_Report,
			CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem,
	};
}
