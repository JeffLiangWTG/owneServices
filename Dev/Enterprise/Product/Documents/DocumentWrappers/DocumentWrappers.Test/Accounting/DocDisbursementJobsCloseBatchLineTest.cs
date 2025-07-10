using System.Linq;
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
	[TestedType(typeof(DocDisbursementJobsCloseBatchLine))]
	sealed class DocDisbursementJobsCloseBatchLineTest : DocumentWrapperTestCase
	{
		public void TestPropertiesOfDocDisbursementJobsCloseBatchLine()
		{
			AssertEquals("Property JobNumber", batchLine.JobNumber, wrapper.JobNumber);
			AssertEquals("Property JobOpenedDate", batchLine.JobOpenedDate, wrapper.JobOpenedDate);
			AssertEquals("Property LineBranch", batchLine.LineBranch, wrapper.LineBranch);
			AssertEquals("Property LineDepartment", batchLine.LineDepartment, wrapper.LineDepartment);
			AssertEquals("Property ChargeCode", batchLine.ChargeCode, wrapper.ChargeCode);
			AssertEquals("Property LineGLAccount", batchLine.LineGLAccount, wrapper.LineGLAccount);
			AssertEquals("Property DSBSurplusGLAccount", batchLine.DSBSurplusGLAccount, wrapper.DSBSurplusGLAccount);
			AssertEquals("Property DSBShortFallGLAccount", batchLine.DSBShortFallGLAccount, wrapper.DSBShortFallGLAccount);
			AssertEquals("Property LineType", batchLine.LineType, wrapper.LineType);
			AssertEquals("Property LineLocalAmount", batchLine.LineLocalAmount, wrapper.LineLocalAmount);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocDisbursementJobsCloseBatchLine.New(batchLine, Factory) };
		}

		DocDisbursementJobsCloseBatchLine wrapper;
		DisbursementJobsCloseBatchLine batchLine;

		protected override void SetUp()
		{
			var dsbJobBatch = TestObjectCreator.CreateDsbJobCloseBatch("123456");

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

			Factory.Save();

			var sql = $@"SELECT * FROM GetDsbJobCloseBatchDetails ('{dsbJobBatch.PK}')";
			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load(sql);
			batchLine = new DisbursementJobsCloseBatchLine(coll.ToArray()[0]);

			wrapper = (DocDisbursementJobsCloseBatchLine)GetDocumentWrappers()[0];

			base.SetUp();
		}
	}
}
