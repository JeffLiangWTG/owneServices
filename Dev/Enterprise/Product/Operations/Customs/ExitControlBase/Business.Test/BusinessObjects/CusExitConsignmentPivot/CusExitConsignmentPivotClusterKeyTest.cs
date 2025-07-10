using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPivot))]
public abstract class CusExitConsignmentPivotClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var consignmentItem = (CusExitConsignmentItem)NewParentObject();
		consignmentItem.CCI_LineNumber = 1;
		var consignmentPivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
		return consignmentPivot;
	}

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		var consignment = header.CusExitConsignments.AddNew();
		consignment.CXC_Status = "REJ";
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		return consignmentItem;
	}
}
