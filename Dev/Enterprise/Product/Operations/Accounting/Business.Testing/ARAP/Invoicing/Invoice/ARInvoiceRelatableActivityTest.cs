using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoice))]
	class ARInvoiceRelatableActivityTest : RelatableActivityTestCase<ARInvoice>
	{
		protected override ARInvoice GetNewActivity()
		{
			return Factory.NewWithValidTestData<ARInvoice>();
		}
	}
}
