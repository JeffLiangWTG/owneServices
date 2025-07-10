using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineClusterKeyTest : TestCaseWithFactory
	{
		public void TestLoadClusterKeyChildrenContainsCountrySpecificElements()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invLine.QuarantineExDocLine;

			var invLineAsClusterKeyWorker = invLine as IClusterKeyWorker;
			AssertNotNull(nameof(invLineAsClusterKeyWorker), invLineAsClusterKeyWorker);

			var clusterKeyChildren = ClusterKeyChildInfoTest.LoadChildClusterKeyEntities(invLineAsClusterKeyWorker.ClusterKeyChildList, invLine);
			AssertEquals("Loaded cluster key children contain invLine.QuarantineExDocLine.", true, clusterKeyChildren.Contains(quarantineExDocLine));
		}
	}
}
