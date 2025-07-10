using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentItem))]
public abstract class CusExitConsignmentItemClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
	{
		var header = ((CusExitConsignmentItem)ClusterKeyEntityToTest).Consignment.Header;
		var container = header.CusExitContainers.AddNew();
		var package = header.CusExitConsignmentPackages.AddNew();
		var consignmentPivot = ((CusExitConsignmentItem)ClusterKeyEntityToTest).CusExitConsignmentPivots.AddNew();
		consignmentPivot.CNP_CXN_Container = container.PK;
		consignmentPivot.CNP_CXP_Package = package.PK;
		return new IClusterKeyWorker[] { consignmentPivot };
	}

	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var consignmentItem = ((CusExitConsignment)NewParentObject()).CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		return consignmentItem;
	}

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		var consignment = header.CusExitConsignments.AddNew();
		consignment.CXC_Status = "REJ";
		return consignment;
	}

	protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
	{
			CusExitReportItemSchema.ERI_CER_Report,
	};
}

