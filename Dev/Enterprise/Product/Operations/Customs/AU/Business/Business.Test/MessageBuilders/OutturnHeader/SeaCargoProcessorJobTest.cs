using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoProcessorJobTest : TestCaseWithFactory
	{
		public void TestSendChildren()
		{
			Assert(!processorJob.SendChildren);
		}

		public void TestMutex()
		{
			var mutex = processorJob.Mutex;
			AssertNotNull(mutex);
			AssertEquals(mutex.MutexID, CusOutturnHeaderSendSEAOUTMutex.Instance);
		}

		public void TestJobNumber()
		{
			outturnHeader.C6_SendersMessageReference = "test1";
			AssertEquals("test1", processorJob.JobNumber);
		}

		public void TestBranch()
		{
			AssertNull(processorJob.Branch);
		}

		public void TestMasterBill()
		{
			AssertSame(outturnHeader, processorJob.MasterBill);
		}

		public void TestChildren()
		{
			AssertSame(outturnHeader.Outturns, processorJob.Children);
		}

		public void TestAcceptable()
		{
			outturnHeader.C6_LloydsIMO = "";
			Assert(!processorJob.IsAcceptable);
			AssertEquals("Error - C6_LloydsIMO: Please enter a value.", processorJob.ReasonWhyNotAcceptable);

			outturnHeader.C6_LloydsIMO = "132456";
			Assert(processorJob.IsAcceptable);
			AssertEquals("", processorJob.ReasonWhyNotAcceptable);
		}

		public void TestGetReferenceNumber()
		{
			outturnHeader.C6_SendersMessageReference = "test1";
			var iProcessorJob = processorJob as IHouseBillsCargoMessageProcessorJobWithMutex;
			AssertEquals("test1", iProcessorJob.GetReferenceNumber(outturnHeader));
		}

		protected override void SetUp()
		{
			base.SetUp();
			outturnHeader = Factory.New<CusOutturnHeader>();
			processorJob = new SeaCargoOutturnHeaderProcessorJob(outturnHeader);
		}
		CusOutturnHeader outturnHeader;
		SeaCargoOutturnHeaderProcessorJob processorJob;
	}
}
