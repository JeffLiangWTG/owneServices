using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ViewProcessHeader))]
	class ViewProcessHeaderTest : EnterpriseBusinessObjectTestCase
	{
		IEnumerable<Func<BusinessObjectFactory, BusinessObject>> GetBizos()
		{
			var descriptors = WorkflowDescriptors.Instance;
			WorkflowDescriptor Get(string code) => descriptors.TryGetValueSafe(code);
			yield return f => f.New(Get(WorkflowDescriptors.AccComplianceReportCode).WorkflowProviderType);
			yield return f => f.New(Get(WorkflowDescriptors.AccPayableOrderHeaderCode).WorkflowProviderType);
			yield return f => f.New(Get(WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode).WorkflowProviderType);
			yield return f =>
			{
				var header = f.New(Get(WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode).WorkflowProviderType);
				var bill1 = (BusinessObject)f.New<Enterprise.Integration.Customs.US.ISF.ICusISFBill>();
				bill1.FillWithValidTestData();
				var bill2 = (BusinessObject)f.New<Enterprise.Integration.Customs.US.ISF.ICusISFBill>();
				bill2.FillWithValidTestData();
				bill1[CusISFBillSchema.BB_BF] = header.PK;
				bill2[CusISFBillSchema.BB_BF] = header.PK;
				return header;
			};
		}

		public void TestView_NoCrash()
		{
			foreach (var fnc in GetBizos())
			{
				var bizo = fnc(Factory);
				bizo.FillWithValidTestData();
				var header = BMSTestHelper.CreateJobHeader((IWorkflowProvider)bizo);
				BMSTestHelper.CreateWorkflow(header, "Ayo");
			}
			Factory.Save();

			var bizos = Factory.Load<ViewProcessHeader>(new ZQuery());
			AssertEquals(true, bizos.Any());
		}

		public void TestSaveNewWorkflow_ShouldThrowException()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Nope.");

			Factory.Save();

			var view1 = workflow1.GetView();

			AssertNoExceptionThrown(Factory.Save);

			var view2 = Factory.New<ViewProcessHeader>();

			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}

		public void TestUpdateExistingWorkflow_ShouldNotSaveColumnsThatShouldntBeSaved()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Nope.");

			Factory.Save();

			var view = workflow.GetView();

			view.ReleaseToBuffer(config.Buffer, config.ComponentLink);

			Factory.Save();
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			view.VFH_CompletionStatement = "Yeah, ok.";
			view.VFH_AgreedDeliveryDate = ZDateTime.UtcNow;
			Factory.Save();

			AssertMultilineASCIIEquals("", @"You're updating aspects of ProcessHeader via ViewProcessHeader for which this view was not designed. It is intended for lightweight loading in places like the Release Gate where we also want the ability to mutate limited state. These changes won't be updated by the 'instead of update' trigger on ViewProcessHeader.
Properties edited:
VFH_AgreedDeliveryDate
VFH_CompletionStatement", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestReleaseWorkflow_ShouldLogDataVersionsAgainstProcessHeaderTableWithAuditLog()
		{
			using var adminConnection = Db.NewAdminConnection();
			var factory = new BusinessObjectFactory(adminConnection);
			var auditLogsHelperForTesting = new AuditLogsHelperForTesting(factory, ProcessHeaderSchema.Instance);
			var config = TestConfigsHelper.CreateSchematicTestConfig(factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Nope.");

			config.Buffer.FC_Name = "Buffeir";

			factory.Save();

			Thread.Sleep(100);
			var newFactory = factory.CreateNewFactory();
			var view = newFactory.LoadFromPrimaryKeysAndTableCode(new[] { workflow.PK }, new[] { new ZString("OH") }).Single();

			view.ReleaseToBuffer(config.Buffer, config.ComponentLink);
			newFactory.Save();

			newFactory = factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var dataVersionLogs = auditLogsHelperForTesting.GetAuditLogCollection(loadedWorkflow);

			var log = dataVersionLogs.Cast<AuditEvent>().OrderByDescending(l => l.TimeUtc).First();

			AssertNotNull(log);
			AssertEquals(ProcessHeaderSchema.Constants.Prefix, log.TablePrefix);

			var columns = log.ChangeCollection.Cast<AuditChange>().Select(x => x.RealColumnName);
			AssertCollectionContains(ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, columns);
			AssertCollectionContains(ProcessHeaderSchema.Constants.FH_ReleaseDateTime, columns);
			AssertCollectionContains(ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc, columns);

			AssertCollectionNotContains(ViewProcessHeaderSchema.Constants.VFH_FC_CurrentComponent, columns);
			AssertCollectionNotContains(ViewProcessHeaderSchema.Constants.VFH_ReleaseDateTime, columns);
			AssertCollectionNotContains(ViewProcessHeaderSchema.Constants.VFH_SystemLastEditTimeUtc, columns);

			Assert("Should use the human-readable name of the buffer component rather than its PK", log.ChangeCollection.Any(x => ((AuditChange)x).ValueAfter == "Buffeir"));
		}

		public void TestLastTransferType_ShouldIgnoreConcurrencyCheck()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			var view1 = Factory.LoadFromPrimaryKeysAndTableCode(new[] { workflow.PK }, new List<ZString> { OrgHeaderSchema.Constants.Prefix }).Single();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var view2 = newFactory.LoadFromPrimaryKeysAndTableCode(new[] { workflow.PK }, new List<ZString> { OrgHeaderSchema.Constants.Prefix }).Single();

			view1.VFH_LastTransferType = TransferTypeList.Codes.ManualTransfer;
			view2.VFH_LastTransferType = TransferTypeList.Codes.SchematicTransfer;

			AssertNoExceptionThrown("There should not be a concurrency exception for last transfer type... SAD!", () =>
			{
				newFactory.Save();
				Factory.Save();
			});

			AssertEquals(TransferTypeList.Codes.ManualTransfer, view1.VFH_LastTransferType);
			AssertEquals(TransferTypeList.Codes.SchematicTransfer, view2.VFH_LastTransferType);
		}

		protected override bool IsDeleteSupported()
		{
			return false; // This bizo is for a view and that view should never be updated except by the release gate for the specific columns it needs.
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
