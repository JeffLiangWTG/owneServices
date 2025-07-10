using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToConsol;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(JobARAPInvoicePrintingAggregator))]
	public class JobARAPInvoicePrintingAggregatorTest : NonPersistentBusinessObjectTestCase
	{
	}
}
