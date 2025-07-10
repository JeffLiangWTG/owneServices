using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLayoutBuilder))]
	sealed class JobComInvoiceHeaderLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<JobComInvoiceHeaderLayoutBuilder, JobComInvoiceHeader, CommercialInvoiceDetailsControlBag>
	{
		protected override JobComInvoiceHeaderLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new JobComInvoiceHeaderLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 1;
	}
}
