using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitReport))]
public abstract class CusExitReportClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
	{
		var reportItem = ((CusExitReport)ClusterKeyEntityToTest).CusExitReportItems.AddNew();
		return new IClusterKeyWorker[] { reportItem };
	}

	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var header = (CusExitHeader)NewParentObject();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		return report;
	}

	protected override EnterpriseBusinessObject NewParentObject()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		return header;
	}
}
