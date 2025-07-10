using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderClusterKeyTest : TestCaseWithFactory
	{
		public void TestLoadClusterKeyChildrenContainsCountrySpecificElements()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var quarantineExDocHeader = invoice.QuarantineExDocHeader;

			var invoiceAsClusterKeyWorker = invoice as IClusterKeyWorker;
			AssertNotNull(nameof(invoiceAsClusterKeyWorker), invoiceAsClusterKeyWorker);

			var clusterKeyChildren = ClusterKeyChildInfoTest.LoadChildClusterKeyEntities(invoiceAsClusterKeyWorker.ClusterKeyChildList, invoice);
			AssertEquals("Loaded cluster key children contain invoice.QuarantineExDocHeader.", true, clusterKeyChildren.Contains(quarantineExDocHeader));
		}
	}
}
