using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestLinesInvoicingSupporter))]
	sealed class ExportCustomsManifestLinesInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<ExportCustomsManifestLines>();
	}
}
