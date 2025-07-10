using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignment))]
public abstract class CusExitConsignmentClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
	{
		var consignmentItem = ((CusExitConsignment)ClusterKeyEntityToTest).CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		return new IClusterKeyWorker[] { consignmentItem };
	}

	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var consignment = ((CusExitHeader)NewParentObject()).CusExitConsignments.AddNew();
		consignment.CXC_Status = "REJ";
		return consignment;
	}

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		return header;
	}

	protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
	{
			CusExitReportItemSchema.ERI_CER_Report,
			CusExitReportSchema.CER_CXH_Header,
	};
}
