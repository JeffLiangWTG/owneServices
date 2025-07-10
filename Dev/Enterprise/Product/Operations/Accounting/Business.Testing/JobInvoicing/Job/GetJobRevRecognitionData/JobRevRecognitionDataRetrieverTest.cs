using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using static Enterprise.MasterFiles.Business.RevenueRecognitionLookups;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevRecognitionDataRetrieverTest : TestCaseWithFactory
	{
		#region CST and REV

		public void TestCSTAndREV_SameReverseDate()
		{
			PrepareTestData();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1m, 400m);
			arInvLine.AL_JH = JobForTest.PK;
			arInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;
			arInvLine.AL_ReverseDate = ZDateTime.Today;
			TestObjectCreator.CreateJobCharge(arInvLine, JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 300m, 0m, 0m, 300m, 0m, 0m);
			var apInvLine = apInv.Lines[0];
			apInvLine.AL_JH = JobForTest.PK;
			apInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;
			apInvLine.AL_ReverseDate = ZDateTime.Today;
			TestObjectCreator.CreateJobCharge(apInv.Lines[0], JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today));
			});
		}

		public void TestCSTAndREV_DifferentRecognitionTypes()
		{
			PrepareTestData();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1m, 400m);
			arInvLine.AL_JH = JobForTest.PK;
			arInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;
			arInvLine.AL_ReverseDate = ZDateTime.Today;
			TestObjectCreator.CreateJobCharge(arInvLine, JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 300m, 0m, 0m, 300m, 0m, 0m);
			var apInvLine = apInv.Lines[0];
			apInvLine.AL_JH = JobForTest.PK;
			apInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;
			apInvLine.AL_ReverseDate = ZDateTime.Today;
			TestObjectCreator.CreateJobCharge(apInv.Lines[0], JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today));
				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.JobClosure && x.recognizedDate == ZDateTime.Today));
			});
		}

		public void TestCSTAndREV_DifferentReverseDates()
		{
			PrepareTestData();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1m, 400m);
			arInvLine.AL_JH = JobForTest.PK;
			arInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;
			arInvLine.AL_ReverseDate = ZDateTime.Today.AddDays(-1);
			TestObjectCreator.CreateJobCharge(arInvLine, JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 300m, 0m, 0m, 300m, 0m, 0m);
			var apInvLine = apInv.Lines[0];
			apInvLine.AL_JH = JobForTest.PK;
			apInvLine.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;
			apInvLine.AL_ReverseDate = ZDateTime.Today;
			TestObjectCreator.CreateJobCharge(apInv.Lines[0], JobForTest, TestObjectCreator.CC3, TestObjectCreator.AUD);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.JobClosure && x.recognizedDate == ZDateTime.Today));
			});
		}

		#endregion

		#region Unreversed WIP and ACR

		public void TestWIPAndACR_Unreversed_SamePostDate()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today;
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today;
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today));
			});
		}

		public void TestWIPAndACR_Unreversed_DifferentRecognitionTypes()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.JobClosure && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		public void TestWIPAndACR_Unreversed_DifferentPostDates()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today;
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today;
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 4 records", 4, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today));
				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		#endregion

		#region Reversed WIP And ACR

		public void TestWIPAndACR_Reversed_SamePostDate()
		{
			PrepareTestData();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		public void TestWIPAndACR_Reversed_DifferentPostDates()
		{
			PrepareTestData();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-2);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-2)));
			});
		}

		public void TestWIPAndACR_Reversed_DifferentRecognitionTypes()
		{
			PrepareTestData();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 2 records", 2, result.Count);

				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(1, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.JobClosure && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		#endregion

		#region Reversed and unreversed WIP and ACR

		public void TestWIPAndACR_RevsersedAndUnreversedMixed_SameRecognitionDate()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 4 records", 4, result.Count);

				AssertEquals(4, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		public void TestWIPAndACR_RevsersedAndUnreversedMixed_DifferentRecognitionDates()
		{
			PrepareTestData();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-2);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-2);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 4 records", 4, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-2)));
			});
		}

		public void TestWIPAndACR_RevsersedAndUnreversedMixed_DifferentRecognitionTypes()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;

			Factory.Save();

			var wip1 = TestObjectCreator.CreateWIP(JobForTest);
			wip1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip1.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr1 = TestObjectCreator.CreateAccrual(JobForTest);
			acr1.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr1.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;

			wip1.RelatedJobCharge.ReverseWIP(ZDateTime.Today, true);
			acr1.RelatedJobCharge.ReverseAccrual(ZDateTime.Today, true);

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have 4 records", 4, result.Count);

				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
				AssertEquals(2, result.Count(x => x.recognitionType == RecognitionDateOptionCodes.JobClosure && x.recognizedDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		#endregion

		#region Misc cases

		public void TestNonJobPk()
		{
			PrepareTestData();

			var wip = TestObjectCreator.CreateWIP(JobForTest);
			wip.AL_PostDate = ZDateTime.Today.AddDays(-1);
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			var acr = TestObjectCreator.CreateAccrual(JobForTest);
			acr.AL_PostDate = ZDateTime.Today.AddDays(-1);
			acr.AL_RevRecognitionType = RecognitionDateOptionCodes.JobClosure;

			Factory.Save();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(Guid.NewGuid());
			var result = retriever.GetRevenueRecognitionData();

			AssertEquals("Result should have 0 records", 0, result.Count);
		}

		public void TestGetRevenueRecognitionData_NoRevRecognitionData()
		{
			PrepareTestData();

			IJobRevRecognitionDataRetriever retriever = new JobRevRecognitionDataRetriever(JobForTest.PK);
			var result = retriever.GetRevenueRecognitionData();
			Assert("Result should be empty", !result.Any());
		}

		#endregion

		#region Implementation

		Job JobForTest;

		void PrepareTestData()
		{
			var shipment = TestObjectCreator.CreateShipment("1001");
			JobForTest = TestObjectCreator.CreateJob(shipment);

			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
