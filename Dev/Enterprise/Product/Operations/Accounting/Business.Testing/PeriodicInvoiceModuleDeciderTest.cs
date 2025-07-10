using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JobFilterProvider))]
	public class PeriodicInvoiceModuleDeciderTest : JobFilterProviderTest
	{
		public void TestJobToModuleMapping()
		{
			var jobTypeToModuleMapping = PeriodicInvoiceModuleDecider.GetJobTypeToCategoryMapping();

			var jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.CFS].ToArray();
			AssertEquals("JobType Count", 1, jobTypes.Length);
			Assert("CFSShipment jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.CFSShipment.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Consol].ToArray();
			AssertEquals("JobType Count", 1, jobTypes.Length);
			Assert("CFSLoadList jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Customs].ToArray();
			AssertEquals("JobType Count", 1, jobTypes.Length);
			Assert("Brokerage jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.Brokerage.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Forwarding].ToArray();
			AssertEquals("JobType Count", 3, jobTypes.Length);
			Assert("Shipment jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.Shipment.Code));
			Assert("QuotedBooking jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code));
			Assert("OneOffQuotation jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Consignment].ToArray();
			AssertEquals("JobType Count", 3, jobTypes.Length);
			Assert("TransportBooking jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.TransportBooking.Code));
			Assert("TransportConsignment jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code));
			Assert("TransportConsignment jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.TransportConsignment.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Transport].ToArray();
			AssertEquals("JobType Count", 1, jobTypes.Length);
			Assert("LocalCartage jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.LocalCartage.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Warehouse].ToArray();
			AssertEquals("JobType Count", 2, jobTypes.Length);
			Assert("WarehouseInwards jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.WarehouseInwards.Code));
			Assert("WarehouseOutwards jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.WarehouseOutwards.Code));

			jobTypes = jobTypeToModuleMapping[PeriodicInvoiceModule.Agency].ToArray();
			AssertEquals("JobType Count", 2, jobTypes.Length);
			Assert("AgencyBillOfLading jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code));
			Assert("AgencyBooking jobType should Exist", jobTypes.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code));
		}
	}
}
