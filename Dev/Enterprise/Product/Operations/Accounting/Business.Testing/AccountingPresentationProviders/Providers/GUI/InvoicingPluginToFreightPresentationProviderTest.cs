using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.MasterFiles.Business.RevenueRecognitionLookups;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class InvoicingPluginToFreightPresentationProviderTest : TestCaseWithFactory
	{
		#region Closed Job Reopener

		public void TestExceptionWhenClosedJobReopenerIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: closedJobReopener",
				() => new InvoicingPluginToFreightPresentationProvider(null, new Mock<IJobRevRecognitionDataRetriever>().Object));
			AssertNoExceptionThrown(() => new InvoicingPluginToFreightPresentationProvider(new Mock<IClosedJobReopener>().Object, new Mock<IJobRevRecognitionDataRetriever>().Object));
		}

		public void TestPreSaveAction_Success_WhenReopenClosedJobIsTrue()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockJobRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockClosedJobReopener.Setup(x => x.ReopenClosedJobs()).Returns(true);
			IInvoicingPluginToFreightPresentationProvider invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockJobRevRecognitionDataRetriever.Object);

			var actualResult = invoicingPluginToFreightPresentationProvider.PreSaveActions();

			Assert(actualResult.CanProceed);
			Assert(!actualResult.HasError);
		}

		public void TestPreSaveAction_Failure_WhenReopenClosedJobIsFalse()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockJobRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockClosedJobReopener.Setup(x => x.ReopenClosedJobs()).Returns(false);
			IInvoicingPluginToFreightPresentationProvider invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockJobRevRecognitionDataRetriever.Object);

			var actualResult = invoicingPluginToFreightPresentationProvider.PreSaveActions();

			Assert(!actualResult.CanProceed);
			AssertEquals("Saving requires job reopening.", actualResult.ErrorMessage);
		}

		IInvoicingPluginToFreightPresentationProvider GetPresentationProvider(IClosedJobReopener closedJobReopener, IJobRevRecognitionDataRetriever revRecognitionDatRetriever) => ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoicingPluginToFreightPresentationProvider(closedJobReopener, revRecognitionDatRetriever);

		#endregion

		#region Fix Revenue Recognition Data

		public void TestExceptionWhenRevRecognitionDataRetrieverIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: jobRevRecognitionDataRetriever", () => new InvoicingPluginToFreightPresentationProvider(new Mock<IClosedJobReopener>().Object, null));
			AssertNoExceptionThrown(() => new InvoicingPluginToFreightPresentationProvider(new Mock<IClosedJobReopener>().Object, new Mock<IJobRevRecognitionDataRetriever>().Object));
		}

		public void TestFixRevenueRecognitionData_NoRevRecognizedData()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>());

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(NotFixMessage, result);
		}

		public void TestFixRevenueRecognitionData_RevRecognitionDataNotBroken()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
			{
				(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today)
			});

			TestObjectCreator.CreateJobChargeRevRecognition(JobForTest, RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today);
			Factory.Save();

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(NotFixMessage, result);
		}

		public void TestFixRevenueRecognitionData_PartRevRecognitionDataBroken()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today), (RecognitionDateOptionCodes.JobClosure, ZDateTime.Today)
				});

			TestObjectCreator.CreateJobChargeRevRecognition(JobForTest, RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today);
			Factory.Save();

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(2, revRecognizedData.Length);
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.D3_RecognitionDate == ZDateTime.Today));
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.JobClosure && x.D3_RecognitionDate == ZDateTime.Today));
			});
		}

		public void TestFixRevenueRecognitionData_AllRevRecognitionDataBroken()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today), (RecognitionDateOptionCodes.JobClosure, ZDateTime.Today)
				});

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(2, revRecognizedData.Length);
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.D3_RecognitionDate == ZDateTime.Today));
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.JobClosure && x.D3_RecognitionDate == ZDateTime.Today));
			});
		}

		public void TestFixRevenueRecognitionData_RecognitionTypeIsIMM()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.Immediate, ZDateTime.Today)
				});

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(1, revRecognizedData.Length);
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.Immediate && x.D3_RecognitionDate == ZDateTime.MinSmallDateTimeValue));
			});
		}

		public void TestFixRevenueRecognitionData_MultipleIMMRevRecognitionTypes()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.Immediate, ZDateTime.Today), (RecognitionDateOptionCodes.Immediate, ZDateTime.Today.AddDays(-1))
				});

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(1, revRecognizedData.Length);
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.Immediate && x.D3_RecognitionDate == ZDateTime.MinSmallDateTimeValue));
			});
		}

		public void TestFixRevenueRecognitionData_GroupByRecognitionTypes()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today), (RecognitionDateOptionCodes.JobClosure, ZDateTime.Today.AddDays(-1)),
					(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today), (RecognitionDateOptionCodes.JobClosure, ZDateTime.Today.AddDays(-1)),
				});

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(2, revRecognizedData.Length);

				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.D3_RecognitionDate == ZDateTime.Today));
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.JobClosure && x.D3_RecognitionDate == ZDateTime.Today.AddDays(-1)));
			});
		}

		public void TestFixRevenueRecognitionData_HasEmptyRecognitionType()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(string.Empty, ZDateTime.Today), (RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today),
				});

			AssertNull("Pre-assertion: No D3 records of Job", Factory.LoadTop1<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK)));

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(1, revRecognizedData.Length);

				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.D3_RecognitionDate == ZDateTime.Today));
			});
		}

		public void TestFixRevenueRecognitionData_ExistingJobChargeRevRecognitionMoreThanRetrieved()
		{
			PrepareTestData();

			TestObjectCreator.CreateJobChargeRevRecognition(JobForTest, RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today);
			TestObjectCreator.CreateJobChargeRevRecognition(JobForTest, RecognitionDateOptionCodes.JobClosure, ZDateTime.Today);
			Factory.Save();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.PickupDate, ZDateTime.Today)
				});

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => true);

			AssertEquals(FixedMessage, result);

			var revRecognizedData = Factory.Load<JobChargeRevRecognition>(new ZQuery(JobChargeRevRecognitionSchema.D3_JH, JobForTest.PK));

			CombineAssertions(() =>
			{
				AssertEquals(3, revRecognizedData.Length);

				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.ActualDepartureDate && x.D3_RecognitionDate == ZDateTime.Today));
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.JobClosure && x.D3_RecognitionDate == ZDateTime.Today));
				AssertNotNull(revRecognizedData.First(x => x.D3_RecognitionType == RecognitionDateOptionCodes.PickupDate && x.D3_RecognitionDate == ZDateTime.Today));
			});
		}

		public void TestFixRevenueRecognitionData_StopToFix()
		{
			PrepareTestData();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();
			mockRevRecognitionDataRetriever.Setup(x => x.JobPk).Returns(JobForTest.PK);
			mockRevRecognitionDataRetriever.Setup(x => x.GetRevenueRecognitionData())
				.Returns(new List<(ZString, ZDateTime)>
				{
					(RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today)
				});

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			var result = invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(x => false);

			AssertEquals(string.Empty, result);
		}

		public void TestExceptionWhenSavingDecisionFuncIsNull()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			var mockRevRecognitionDataRetriever = new Mock<IJobRevRecognitionDataRetriever>();

			var invoicingPluginToFreightPresentationProvider = GetPresentationProvider(mockClosedJobReopener.Object, mockRevRecognitionDataRetriever.Object);
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: savingDecisionFunc", () => invoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(null));
		}

		#endregion

		#region Implementation

		const string FixedMessage = "Job revenue recognition data fixed.";
		const string NotFixMessage = "No broken job revenue recognition data detected.";

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
