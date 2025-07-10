using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRelatedInvoicingLineBaseCollection))]
	public class JobRelatedInvoicingLineBaseCollectionTest : InvoicingLineBaseCollectionTest
	{
		public override void FAT_TestSetDefaultsForNewChildAL_Sequence_Case8()
		{
			Assert("Can't test anything new from tested for AP Invoice line collection,", true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<APInvoice>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new JobRelatedInvoicingLineBaseCollection(parent);
		}
	}
}
