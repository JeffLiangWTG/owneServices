using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocDisbursementJobsCloseBatch))]
	sealed class DocDisbursementJobsCloseBatchTest : DocumentWrapperTestCase
	{
		public void TestPropertiesOfDocDisbursementJobsCloseBatch()
		{
			AssertEquals("Property BatchPrintedBy", GlbStaff.CurrentUser.GS_FullName, wrapper.BatchPrintedBy);
			AssertEquals("Property CompanyName", GlbCompany.CurrentCompany.CompanyName, wrapper.CompanyName);
			AssertEquals("Property CompanyCode", GlbCompany.CurrentCompany.GC_Code, wrapper.CompanyCode);
		}

		public void TestBatchDetailsLines()
		{
			AssertEquals(3, wrapper.BatchDetailsLines.Count);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocDisbursementJobsCloseBatch.New(dsbJobBatch, Factory) };
		}

		DocDisbursementJobsCloseBatch wrapper;
		DsbJobCloseBatch dsbJobBatch;

		protected override void SetUp()
		{
			//Prepare date for batch 1
			dsbJobBatch = TestObjectCreator.CreateDsbJobCloseBatch("123456");

			var shipment = TestObjectCreator.CreateShipment("1000");

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 100m);
			line1.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line1);
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 200m);
			line2.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line2);

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.CNY, 1m, 1000m, 0m, 0m, 1000m, 0m, 0m, TestObjectCreator.AALSHI);
			var line3 = TestObjectCreator.CreateAPInvoiceLine(invoice2, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 1000m);
			line3.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line3);

			//Prepare date for batch 2
			var dsbJobBatch1 = TestObjectCreator.CreateDsbJobCloseBatch("456789");
			var shipment1 = TestObjectCreator.CreateShipment("2000");
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.JH_ParentID = shipment1.PK;

			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var line4 = TestObjectCreator.CreateARInvoiceLine(invoice3, job1, chargeCode, TestObjectCreator.CNY, 1M, "Test", 400m);
			line4.AL_JBB = dsbJobBatch1.PK;
			TestObjectCreator.CreateCharge(line4);
			var line5 = TestObjectCreator.CreateARInvoiceLine(invoice3, job1, chargeCode, TestObjectCreator.CNY, 1M, "Test", 500m);
			line5.AL_JBB = dsbJobBatch1.PK;
			TestObjectCreator.CreateCharge(line5);

			Factory.Save();

			var sql = $@"SELECT * FROM GetDsbJobCloseBatchDetails ('{dsbJobBatch.PK}')"; // SQL query
			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load(sql);

			wrapper = (DocDisbursementJobsCloseBatch)GetDocumentWrappers()[0];

			base.SetUp();
		}
	}
}
