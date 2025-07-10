using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsDocketsInvoiceJobHistory))]
	sealed class DocWhsReceiveInvoiceJobHistoryTest : DocWhsDocketsInvoiceJobHistoryTest
	{
		#region Related Business Objects

		protected override void AssertJobChargeLines()
		{
			AssertEquals(1, DocWrapper.JobChargeLines.Count);
			AssertEquals(true, DocWrapper.JobChargeLines.Contains(JobCharge2));
		}

		protected override void AssertJobChargeLinesSort()
		{
			AssertEquals("Charge2 Should be 1st", JobCharge2, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
		}

		#endregion
	}
}
