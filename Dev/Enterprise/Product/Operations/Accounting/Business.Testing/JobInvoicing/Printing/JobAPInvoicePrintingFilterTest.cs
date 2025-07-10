using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobAPInvoicePrintingFilter))]
	public class JobAPInvoicePrintingFilterTest : JobInvoicePrintingFilterTest
	{
		protected override JobInvoicePrintingFilter GetNewBusinessObject(IBusiness hostBusinessObject, Job jobHeader)
		{
			JobAPInvoicePrintingFilter result = new JobAPInvoicePrintingFilter(hostBusinessObject, jobHeader.PK);
			result.RefreshInvoiceList();
			return result;
		}

		protected override JobInvoicePrintingFilter GetNewBusinessObject(ForwardingConsol hostBusinessObject, Job[] jobHeaders)
		{
			return new JobAPInvoicePrintingFilter(hostBusinessObject, jobHeaders.Select(x => x.PK).ToArray());
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		public void TestDefaultFiltering()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00101", Consol);
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice1 = Factory.NewWithValidTestData<APInvoice>();
			InvoicingLineBase line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_JH = job1.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var shipment2 = TestObjectCreator.CreateShipment("S00102", Consol);
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice2 = Factory.NewWithValidTestData<APInvoice>();
			InvoicingLineBase line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(Consol, new[] { job1.PK, job2.PK });
			filter.RefreshInvoiceList();

			AssertEquals(2, filter.Transactions.Count);
			Assert("Filter result should include invoice1", filter.Transactions.Contains(invoice1.PK));
			Assert("Filter result should include invoice2", filter.Transactions.Contains(invoice2.PK));
		}

		public void TestJobNumberFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00101", Consol);
			var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_JH = job1.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var shipment2 = TestObjectCreator.CreateShipment("S00102", Consol);
			var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			var filter = new JobAPInvoicePrintingFilter(Consol, new[] { job1.PK, job2.PK });
			filter.JobNumber = job1.PK;
			filter.RefreshInvoiceList();

			AssertEquals(1, filter.Transactions.Count);
			Assert("Filter result should include invoice1", filter.Transactions.Contains(invoice1.PK));
		}

		public void TestTransactionTypeFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00101", Consol);
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice1 = Factory.NewWithValidTestData<APInvoice>();
			InvoicingLineBase line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_JH = job1.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var shipment2 = TestObjectCreator.CreateShipment("S00102", Consol);
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice2 = Factory.NewWithValidTestData<APCreditNote>();
			InvoicingLineBase line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var shipment3 = TestObjectCreator.CreateShipment("S00103", Consol);
			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = shipment3.PK;
			job3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice3 = Factory.NewWithValidTestData<APAdjustmentNote>();
			InvoicingLineBase line3 = (InvoicingLineBase)invoice3.Lines.AddNew();
			line3.AL_JH = job3.PK;
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line3, job3, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(Consol, new[] { job1.PK, job2.PK, job3.PK });
			filter.TransactionType = TransactionTypes.CreditNote;
			filter.RefreshInvoiceList();

			AssertEquals(1, filter.Transactions.Count);
			AssertEquals(invoice2.PK, filter.Transactions[0].PK);
		}

		public void TestCreditorFilter()
		{
			Consol.Shipments.Add(Shipment1);
			Job job1 = Header1;
			InvoicingBase invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_JH = job1.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Consol.Shipments.Add(Shipment2);
			Job job2 = Header2;
			job2.JH_ParentID = Shipment2.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			InvoicingBase invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(line2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(Consol, new[] { job1.PK, job2.PK });
			filter.DebtorOrCreditor = TestObjectCreator.ABIGAS.PK;
			filter.RefreshInvoiceList();

			AssertEquals(1, filter.Transactions.Count);
			AssertEquals(invoice2.PK, filter.Transactions[0].PK);
		}

		public void TestIsFreightConsolWithGatewayConsol()
		{
			GlbDepartment nonGatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "BRN"));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), nonGatewayDepartment.PK.ToGuid()))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				gatewayAgentPort.O5_PortOrCountry = "AUBNE";
				gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var shipment1 = TestObjectCreator.CreateShipment("S00101", Consol);
				var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();

				var shipment2 = TestObjectCreator.CreateShipment("S00102", Consol);
				var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();

				JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(consol, job1.PK);
				AssertEquals("Is Freight Consol", false, filter.IsFreightConsol);
			}
		}

		#region Get Query Overrides

		protected override JobInvoicePrintingFilter CreateInstanceForTest(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
		{
			return new JobAPInvoicePrintingFilter(hostBusinessObject, jobHeader, from, to);
		}

		[TestDate(2017, 10, 15)]
		public override void TestGetQueryForConsolWithoutJobNumber()
		{
			TestAPGetQueryForConsolJobWithoutJobNumberCore();
		}

		[TestDate(2017, 10, 15)]
		public override void TestGetQueryWithJobNumber()
		{
			TestAPGetQueryWithJobNumberCore();
		}

		#endregion
	}
}
