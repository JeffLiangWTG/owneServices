using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceJobsFilterBusinessObject))]
	public class PeriodicInvoiceJobsFilterBusinessObjectTest : PeriodicInvoiceBaseJobsFilterBusinessObjectTest
	{
		protected override void AssertFilterCount()
		{
			FilterStripBusinessObject filterBO = GetNewFilterStripBusinessObject();

			int count = 0;

			foreach (ModuleFilter filter in filterBO.ModuleFilters)
			{
				if (filter.Description == "Currency" || filter.Description == "Debtor" || filter.Description == "Invoice Type" || filter.Description == "Tax Branch" || filter.Description == "Current Company" || filter.Description == "Transactions Eligible for Posting")
				{
					AssertEquals(string.Format("For Periodic Invoice, {0} filter is always hidden", filter.Description), FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);
				}
				else
				{
					AssertEquals(string.Format("For Periodic Invoice, {0} filter is always visible", filter.Description), FilterVisibility.Visible, filter.Visibility);
				}

				count++;
			}

			AssertEquals("There should be 39 including both visible and invisible filters)", 39, count);
		}

		protected override PeriodicInvoiceBase CreatePeriodicInvoice()
		{
			return new PeriodicInvoice(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PeriodicInvoiceJobsFilterBusinessObject();
		}
	}
}
