using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericJob.Testing
{
	[TestedType(typeof(GenericJobInvoicingSupporter))]
	class GenericJobInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestJobNumberSchema()
		{
			Assert(ViewGenericJobSchema.VJ_JobNumber.IsNonBlankFilteredIndexParticipant);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Factory.Save();

			var genericJob = Factory.LoadGenericJob<GenericJob>(shipment.PK, JobShipmentSchema.Constants.Prefix);

			return genericJob;
		}

		protected override bool ExcludeFromTestBecauseNoBillingTab
		{
			get
			{
				return true;
			}
		}
	}
}
