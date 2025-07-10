using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.ServiceManager;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask))]
	public class ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTaskTest : ServiceTaskTestCase<ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask>
	{
		[TestDate(2018, 08, 05)]
		public void TestElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask_RunTask()
		{
			var scheduleTask = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
			scheduleTask.S5_ScheduleType = "EPC";

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;
			Factory.Save();

			AssertEquals("Pre - ElectronicProcessingChargePayableClearingAccount is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
			AssertEquals("Pre - ElectronicProcessingChargeDisbursementClearingAccount is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
			AssertEquals("Pre - ElectronicProcessingChargeCode is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			var serviceTaskQuery = new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, "EPC");
			var task = new ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask();

			electronicProcessingChargeProviderMock.Setup(x => x.InsertAndSetElectronicProcessingChargeRegistry()).Callback(() =>
			{
				AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			});

			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				var serviceLog = InitialiseAndRunTaskSchedule(task);
				electronicProcessingChargeProviderMock.Verify(x => x.InsertAndSetElectronicProcessingChargeRegistry(), Times.Once);

				var expectedMessage = $@"Debug|Electronic Processing Charge Insert GLAccount Charge Code service task started.
Information|Electronic Processing Charge Insert GLAccount Charge Code service task completed.";
				AssertMultilineASCIIEquals(expectedMessage, serviceLog.ToString());

				var ePCServiceTask = Factory.LoadTop1<IStmScheduleTask>(serviceTaskQuery);
				AssertEquals(true, ePCServiceTask.S5_IsActive);
			}
		}

		[ExpectNoExceptions]
		public void TestElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask_Cancel()
		{
			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			var task = new ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask();

			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				var cancellationToken = new CancellationToken(true);
				InitialiseAndRunTaskSchedule(task, cancellationToken);
				electronicProcessingChargeProviderMock.Verify(x => x.InsertAndSetElectronicProcessingChargeRegistry(), Times.Never);

				cancellationToken = new CancellationToken(false);
				InitialiseAndRunTaskSchedule(task, cancellationToken);
				electronicProcessingChargeProviderMock.Verify(x => x.InsertAndSetElectronicProcessingChargeRegistry(), Times.Once);
			}
		}

		public void TestElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask_Exception()
		{
			var exception = new Exception("EPC excpetion");
			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			electronicProcessingChargeProviderMock.Setup(x => x.InsertAndSetElectronicProcessingChargeRegistry()).Callback(() => throw exception);

			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				var task = new ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask();
				InitialiseAndRunTaskSchedule(task);
				AssertEquals(ErrorReporter.LastMessageReported, $"Electronic Processing Charge Insert GLAccount Charge Code service task ended abruptly.\r\nException: System.Exception\r\nException Message: EPC excpetion StackTrace: {exception.StackTrace}.");
			}

			ErrorReporter.Clear();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
