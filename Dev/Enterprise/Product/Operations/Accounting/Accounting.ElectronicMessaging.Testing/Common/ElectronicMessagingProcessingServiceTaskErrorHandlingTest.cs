using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Italy;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Accounting.ElectronicMessaging.Italy.Testing.ElectronicMessagingProcessingServiceTaskForItalyTest;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskWithException))]
	class ElectronicMessagingProcessingServiceTaskErrorHandlingTest : ServiceTaskTestCase<ElectronicMessagingProcessingServiceTaskForItaly>
	{
		public void TestExceptionHandling()
		{
			var company1 = Helper.CreateCompanyAndBranch("MN1", "BR1", "IT", true);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, company1);
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Queued);
				Factory.Save();
			}

			var serviceTask = new ElectronicMessagingProcessingServiceTaskWithException();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			var log = logger.ToString();

			AssertContains("Debug|E Invoice with Error service task started", log);
			AssertContains("Error|E Invoice with Error service task ended abruptly.", log);
			AssertContains("Exception: System.InvalidOperationException", log);
			AssertContains("Exception Message: This is an exception created for testing.", log);

			using (Env.Instance.TemporaryServiceTaskContext(ElectronicMessagingProcessingServiceTaskForItaly.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		public new void TestCategoryExists() => Assert(true);

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes =>
			new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					AccEInvoicingTransactionPivotSchema.Constants.TableName,
					null,
					AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
					AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + new ElectronicMessagingProcessingServiceTaskForItaly_ForTest().CountryCode),
			};

		protected EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}

	public class ElectronicMessagingProcessingServiceTaskWithException : ElectronicMessagingProcessingServiceTaskForItaly
	{
		public override ZString MessageName => "E Invoice with Error";

		public override ZString TaskName => "E Invoice with Error";

		protected override void RunTaskCore(ZGuid companyPK)
		{
			throw new InvalidOperationException("This is an exception created for testing");
		}
	}
}
