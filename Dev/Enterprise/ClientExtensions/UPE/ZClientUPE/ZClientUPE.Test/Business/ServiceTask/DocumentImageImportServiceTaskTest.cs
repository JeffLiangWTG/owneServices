using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.UPE.DocumentImaging.Testing;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.Business.ServiceTask.Testing
{
	[TestedType(typeof(DocumentImageImportServiceTask))]
	sealed class DocumentImageImportServiceTaskTest : ServiceTaskTestCase<DocumentImageImportServiceTask>
	{
		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("Document Image Importer", attributes[0].Description);
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUpCore();
		}
	}

	sealed class DocumentImageImportBatchProcessorTest : DocumentImageImportingTestCase
	{
		protected override void TearDown()
		{
			Logger.ClearLog();
			base.TearDown();
		}

		#region Overrides

		public void TestIsEnvironmentDataValid_WhenNotificationGroupNotSet()
		{
			AssertEquals("Precondition", true, BatchProcessor.IsEnvironmentDataValid());
			UPEDataRegistry.Instance.DocumentImagingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid()); // non-existent group
			AssertEquals("Not valid without notification group set", false, BatchProcessor.IsEnvironmentDataValid());
			Assert("Not valid without notification group set", Logger.ToString().Contains("You must set the 'Document Imaging Notification Group' in the registry to a valid staff group"));
			Logger.ClearLog();

			UPEDataRegistry.Instance.DocumentImagingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationGroup.PK.ToGuid());
			AssertEquals("Valid with notification group set", true, BatchProcessor.IsEnvironmentDataValid());
			AssertEquals("Valid with notification group set", false, Logger.Count > 0);
		}

		#endregion

		#region Purging Documents

		[TestDate(2013, 04, 17, 9, 18, 00)]
		public void TestPurgeOldDocuments()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			CusHAWBStorageDoc.ParentMain.SM_DB = 1;
			CusHAWBStorageDoc.SC_Date = ZDateTime.Now;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 07, 22, 9, 18, 00);

			BatchProcessor.RunTask();
			AssertEquals("Documents should be purged if it is more than 3 months old", true, CusHAWBStorageDoc.IsDeleted);
			Assert(Logger.ToString().Contains("1 document(s) were purged."));
		}

		#endregion

		#region Valid Branch

		public void TestValidBranch()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			BatchProcessor.RunTask();

			AssertEquals("No processing occurs if UPE customisations not enabled", 0, Logger.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			BatchProcessor.RunTask();

			AssertGreaterThan("Processing as expected when UPE customisations are enabled", Logger.Count, 0);
		}

		#endregion

		#region Test Classes

		class TestDocumentImageImportBatchProcessor : DocumentImageImportServiceTask
		{
			public TestDocumentImageImportBatchProcessor(ILogger logger)
				: base(logger)
			{
			}

			public new bool IsEnvironmentDataValid()
			{
				return base.IsEnvironmentDataValid();
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		static readonly TestServiceLogger Logger = new TestServiceLogger();

		static TestDocumentImageImportBatchProcessor BatchProcessor
		{
			get { return fBatchProcessor ?? (fBatchProcessor = new TestDocumentImageImportBatchProcessor(Logger)); }
		}
		[ThreadStatic]
		static TestDocumentImageImportBatchProcessor fBatchProcessor;

		#endregion
	}
}
