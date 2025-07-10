using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class RevenueRecognizerCreatorTest : TestCaseWithFactory
	{
		public void TestCreateRevenueRecognizerForShipment()
		{
			var expectedDate = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_E_ARV = expectedDate;

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);
			Factory.Save();
			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job.RevenueRecognitionCollection.Count);

			new RevenueRecognizerCreator().CreateRevenueRecognizer(shipment).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", expectedDate, job.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate, line.AL_ReverseDate);
		}

		public void TestCreateRevenueRecognizerForConsol()
		{
			var expectedDate = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.Transports[0].JW_ETA = expectedDate;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job1, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge1.JR_AL_APLine = line1.PK;

			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job1);

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job2, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge2.JR_AL_APLine = line2.PK;

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job2);

			Factory.Save();

			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line1.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job1.RevenueRecognitionCollection.Count);

			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line2.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job2.RevenueRecognitionCollection.Count);

			new RevenueRecognizerCreator().CreateRevenueRecognizer(consol).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", expectedDate, job1.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate, line1.AL_ReverseDate);

			AssertEquals("Correct recognition date should be added.", expectedDate, job2.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate, line2.AL_ReverseDate);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
