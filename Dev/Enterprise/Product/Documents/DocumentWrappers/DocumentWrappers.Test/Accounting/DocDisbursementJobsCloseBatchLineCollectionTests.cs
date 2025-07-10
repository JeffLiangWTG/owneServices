using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocDisbursementJobsCloseBatchLineCollection))]
	public class DocDisbursementJobsCloseBatchLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocDisbursementJobsCloseBatchLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocDisbursementJobsCloseBatchLine.New(batchLine, Factory);
		}

		protected override DocDisbursementJobsCloseBatchLineCollection GetCollectionToTest()
		{
			return new DocDisbursementJobsCloseBatchLineCollection(dsbJobBatch, Factory);
		}

		DsbJobCloseBatch dsbJobBatch;
		DisbursementJobsCloseBatchLine batchLine;

		protected override void SetUp()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			dsbJobBatch = testObjectCreator.CreateDsbJobCloseBatch("123456");

			var shipment = testObjectCreator.CreateShipment("1000");

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = testObjectCreator.AALSHI.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var chargeCode = testObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, testObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = testObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = testObjectCreator.GLHeader2.PK;

			var invoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.CNY, 1M, testObjectCreator.ABIGAS);
			var line1 = testObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, testObjectCreator.CNY, 1M, "Test", 100m);
			line1.AL_JBB = dsbJobBatch.PK;
			testObjectCreator.CreateCharge(line1);

			Factory.Save();

			var sql = $@"SELECT * FROM GetDsbJobCloseBatchDetails ('{dsbJobBatch.PK}')"; // SQL query
			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load(sql);
			batchLine = new DisbursementJobsCloseBatchLine(coll.ToArray()[0]);

			base.SetUp();
		}
	}
}
