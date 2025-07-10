using System;
using CargoWise.Common;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	public class MachineLearningTeamAssignmentRetrieverIntegrationTest : AssignmentTestHelper
	{
		const string apiUrl = "https://issue-management-testdev.sand.wtg.zone/api/v1/assignment";
		const int apiTimeout = 50;

		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 100;
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_NoModel()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			AssertAssignment(log, "A", "B", "C");
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_ARC()
		{
			var report = GetExceptionXml(@"
				 <Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.get_StackTrace()</Call>
<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" Parameters=""System.String;System.String;System.Exception"">at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner"" Method=""AddRecords"" Parameters=""System.DateTime"">at Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner.AddRecords(DateTime endTime)</Call>
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner"" Method=""Execute"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner.Execute()</Call>
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.TimeZoneOffsetCacheServiceTask"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.MasterFiles.Business.TimeZoneOffsetCacheServiceTask.RunTask(CancellationToken unused)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.ServiceProviderImplProxy"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask(CancellationToken token)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.BasicServiceProvider"" Method=""Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run(CancellationToken token)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy"" Method=""RunServiceTask"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.RunServiceTask(IServiceTaskHandler serviceTask, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy"" Method=""RunServiceTaskLocked"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.ICommandInfo;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.RunServiceTaskLocked(IServiceTaskHandler serviceTask, ICommandInfo commandInfo, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy"" Method=""Run"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.ICommandInfo"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, ICommandInfo commandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.CommandExecutionStrategy"" Method=""Execute"" Parameters=""Enterprise.ServiceManager.Runner.ICommandInfo"">at Enterprise.ServiceManager.Runner.CommandExecutionStrategy.Execute(ICommandInfo commandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.IpcRunner"" Method=""RunInternal"" Parameters=""System.Boolean"">at Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""RunInternal"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadHelper"" Method=""ThreadStart"" Parameters=""NoParameters"">at System.Threading.ThreadHelper.ThreadStart()</Call>");

			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadHelper.ThreadStart()", 19948);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)", 26467);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42877);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42883);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.TaskRunner.c__DisplayClass2_0.RunB0()", 3);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)", 27345);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.CommandExecutionStrategy.Execute(ICommandInfo commandInfo)", 2);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, ICommandInfo commandInfo)", 3);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.RunServiceTaskLocked(IServiceTaskHandler serviceTask, ICommandInfo commandInfo, CancellationTokenSource cancellationTokenSource)", 3);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.RunServiceTask(IServiceTaskHandler serviceTask, CancellationTokenSource cancellationTokenSource)", 1);
			AddStackLineCount("Enterprise.ServiceManager.Business.dll", "Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run(CancellationToken token)", 74);
			AddStackLineCount("Enterprise.ServiceManager.Business.dll", "Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask(CancellationToken token)", 74);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.TimeZoneOffsetCacheServiceTask.RunTask(CancellationToken unused)", 1);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner.Execute()", 77);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.TimeZoneOffsetCacheRunner.AddRecords(DateTime endTime)", 74);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66140);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136646);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136798);

			AddPublishedAssembly("Enterprise.MasterFiles.Business.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business");
			AddPublishedAssembly("CargoWiseOne.ServiceManager.Runner.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner");
			AddPublishedAssembly("Enterprise.ServiceManager.Business.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Business");
			AddPublishedAssembly("CargoWise.Common.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner", "ENT", "PRC", "PRC");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Business", "ENT", "PRC", "PRC");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common", "ENT", "PER", "APP");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			AssertAssignment(log, "ENT", "ARC", "COR");
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_PER()
		{
			var report = GetExceptionXml(@"<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""RecordDatabaseUpgradedExceptionAndContinue"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.RecordDatabaseUpgradedExceptionAndContinue()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""CheckDbSchemaVersionIsTheSame"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.CheckDbSchemaVersionIsTheSame()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.AfterReconnectionTask"" Method=""Run"" Parameters=""NoParameters"">at CargoWise.Data.AfterReconnectionTask.Run()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""RunTasksAfterOpenConnection"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.RunTasksAfterOpenConnection()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""OpenConnectionIfClosed"" Parameters=""System.Boolean"">at CargoWise.Data.DbConnection.OpenConnectionIfClosed(Boolean useErrorHandler)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""get_InternalConnection"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.get_InternalConnection()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""Command"" Parameters=""System.String;System.Nullable`1[System.Int32]"">at CargoWise.Data.DbConnection.Command(String sqlText, Nullable`1 cmdTimeoutInSeconds)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""Command"" Parameters=""System.String"">at CargoWise.Data.DbConnection.Command(String sqlText)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryDataAccessor"" Method=""GetBinaryValue"" Parameters=""System.String;System.TimeSpan;System.Guid;System.Guid"">at Enterprise.ZArchitecture.Environment.RegistryDataAccessor.GetBinaryValue(String name, TimeSpan maximumCacheAge, Guid owner, Guid department)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryDataAccessor"" Method=""GetBinaryValue"" Parameters=""System.String;System.Guid;System.Guid"">at Enterprise.ZArchitecture.Environment.RegistryDataAccessor.GetBinaryValue(String name, Guid owner, Guid department)</Call>


<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemDictionary"" Method=""GetCurrentRegistryVersion"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemDictionary.GetCurrentRegistryVersion()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemDictionary"" Method=""PurgeAllIfUpdatedByUser"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemDictionary.PurgeAllIfUpdatedByUser()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Web.GUI.dll"" Type=""Enterprise.ZArchitecture.Web.GUI.ZGlobal"" Method=""RefreshRegistryItemCache"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Web.GUI.ZGlobal.RefreshRegistryItemCache()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Web.GUI.dll"" Type=""Enterprise.ZArchitecture.Web.GUI.ZGlobal"" Method=""Application_BeginRequest"" Parameters=""System.Object;System.EventArgs"">at Enterprise.ZArchitecture.Web.GUI.ZGlobal.Application_BeginRequest(Object sender, EventArgs e)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication.SyncEventExecutionStep"" Method=""System.Web.HttpApplication.IExecutionStep.Execute"" Parameters=""NoParameters"">at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""ExecuteStepImpl"" Parameters=""System.Web.HttpApplication.IExecutionStep"">at System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""ExecuteStep"" Parameters=""System.Web.HttpApplication.IExecutionStep;System.Boolean"">at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean completedSynchronously)</Call>


<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication.PipelineStepManager"" Method=""ResumeSteps"" Parameters=""System.Exception"">at System.Web.HttpApplication.PipelineStepManager.ResumeSteps(Exception error)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""BeginProcessRequestNotification"" Parameters=""System.Web.HttpContext;System.AsyncCallback"">at System.Web.HttpApplication.BeginProcessRequestNotification(HttpContext context, AsyncCallback cb)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpRuntime"" Method=""ProcessRequestNotificationPrivate"" Parameters=""System.Web.Hosting.IIS7WorkerRequest;System.Web.HttpContext"">at System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.Hosting.PipelineRuntime"" Method=""ProcessRequestNotificationHelper"" Parameters=""System.IntPtr;System.IntPtr;System.IntPtr;System.Int32"">at System.Web.Hosting.PipelineRuntime.ProcessRequestNotificationHelper(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.Hosting.PipelineRuntime"" Method=""ProcessRequestNotification"" Parameters=""System.IntPtr;System.IntPtr;System.IntPtr;System.Int32"">at System.Web.Hosting.PipelineRuntime.ProcessRequestNotification(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.get_StackTrace()</Call>
<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" Parameters=""System.String;System.String;System.Exception"">at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""RecordDatabaseUpgradedExceptionAndContinue"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.RecordDatabaseUpgradedExceptionAndContinue()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""CheckDbSchemaVersionIsTheSame"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.CheckDbSchemaVersionIsTheSame()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.AfterReconnectionTask"" Method=""Run"" Parameters=""NoParameters"">at CargoWise.Data.AfterReconnectionTask.Run()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""RunTasksAfterOpenConnection"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.RunTasksAfterOpenConnection()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""OpenConnectionIfClosed"" Parameters=""System.Boolean"">at CargoWise.Data.DbConnection.OpenConnectionIfClosed(Boolean useErrorHandler)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""get_InternalConnection"" Parameters=""NoParameters"">at CargoWise.Data.DbConnection.get_InternalConnection()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""Command"" Parameters=""System.String;System.Nullable`1[System.Int32]"">at CargoWise.Data.DbConnection.Command(String sqlText, Nullable`1 cmdTimeoutInSeconds)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbConnection"" Method=""Command"" Parameters=""System.String"">at CargoWise.Data.DbConnection.Command(String sqlText)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryDataAccessor"" Method=""GetBinaryValue"" Parameters=""System.String;System.TimeSpan;System.Guid;System.Guid"">at Enterprise.ZArchitecture.Environment.RegistryDataAccessor.GetBinaryValue(String name, TimeSpan maximumCacheAge, Guid owner, Guid department)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemImpl"" Method=""GetBinaryValue"" Parameters=""System.String;Enterprise.ZArchitecture.Environment.RegistryCacheKey;System.Boolean"">at Enterprise.ZArchitecture.Environment.RegistryItemImpl.GetBinaryValue(String name, RegistryCacheKey cacheKey, Boolean useFallback)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemImpl"" Method=""GetValueOrBytes"" Parameters=""Enterprise.ZArchitecture.Environment.RegistryCacheKey;System.Boolean"">at Enterprise.ZArchitecture.Environment.RegistryItemImpl.GetValueOrBytes(RegistryCacheKey cacheKey, Boolean useFallback)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemImpl"" Method=""Enterprise.Integration.IRegistryItemInternals.HasActualValue"" Parameters=""System.Guid;System.Guid;System.Guid"">at Enterprise.ZArchitecture.Environment.RegistryItemImpl.Enterprise.Integration.IRegistryItemInternals.HasActualValue(Guid companyPK, Guid branchPK, Guid departmentPK)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor"" Method=""HasActualValueAtThisLevelCore"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor.HasActualValueAtThisLevelCore()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor"" Method=""GetFallBackValue"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor.GetFallBackValue()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemImpl"" Method=""get_Value"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemImpl.get_Value()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.ConvertableRegistryItem`2[TGet,TSet]"" Method=""get_ValueCore"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.ConvertableRegistryItem`2[TGet,TSet].get_ValueCore()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Environment.RegistryItemWrapper"" Method=""Enterprise.Integration.IRegistryItem.get_Value"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Environment.RegistryItemWrapper.Enterprise.Integration.IRegistryItem.get_Value()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Web.GUI.dll"" Type=""Enterprise.ZArchitecture.Web.GUI.ZGlobal"" Method=""Application_BeginRequest"" Parameters=""System.Object;System.EventArgs"">at Enterprise.ZArchitecture.Web.GUI.ZGlobal.Application_BeginRequest(Object sender, EventArgs e)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication.SyncEventExecutionStep"" Method=""System.Web.HttpApplication.IExecutionStep.Execute"" Parameters=""NoParameters"">at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""ExecuteStepImpl"" Parameters=""System.Web.HttpApplication.IExecutionStep"">at System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""ExecuteStep"" Parameters=""System.Web.HttpApplication.IExecutionStep;System.Boolean"">at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean completedSynchronously)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication.PipelineStepManager"" Method=""ResumeSteps"" Parameters=""System.Exception"">at System.Web.HttpApplication.PipelineStepManager.ResumeSteps(Exception error)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpApplication"" Method=""BeginProcessRequestNotification"" Parameters=""System.Web.HttpContext;System.AsyncCallback"">at System.Web.HttpApplication.BeginProcessRequestNotification(HttpContext context, AsyncCallback cb)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.HttpRuntime"" Method=""ProcessRequestNotificationPrivate"" Parameters=""System.Web.Hosting.IIS7WorkerRequest;System.Web.HttpContext"">at System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.Hosting.PipelineRuntime"" Method=""ProcessRequestNotificationHelper"" Parameters=""System.IntPtr;System.IntPtr;System.IntPtr;System.Int32"">at System.Web.Hosting.PipelineRuntime.ProcessRequestNotificationHelper(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)</Call>
<Call Assembly=""System.Web.dll"" Type=""System.Web.Hosting.PipelineRuntime"" Method=""ProcessRequestNotification"" Parameters=""System.IntPtr;System.IntPtr;System.IntPtr;System.Int32"">at System.Web.Hosting.PipelineRuntime.ProcessRequestNotification(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)</Call>
");

			AddStackLineCount("System.Web.dll", "System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step)", 11);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor.GetFallBackValue()", 4412);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.get_InternalConnection()", 1484);
			AddStackLineCount("System.Web.dll", "System.Web.HttpApplication.PipelineStepManager.ResumeSteps(Exception error)", 5456);
			AddStackLineCount("System.Web.dll", "System.Web.Hosting.PipelineRuntime.ProcessRequestNotification(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)", 10497);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemImpl.GetValueOrBytes(RegistryCacheKey cacheKey, Boolean UseFallback)", 5851);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.OpenConnectionIfClosed(Boolean useErrorHandler)", 50);
			AddStackLineCount("System.Web.dll", "System.Web.HttpApplication.BeginProcessRequestNotification(HttpContext context, AsyncCallback cb)", 5438);
			AddStackLineCount("System.Web.dll", "System.Web.Hosting.PipelineRuntime.ProcessRequestNotificationHelper(IntPtr rootedObjectsPointer, IntPtr nativeRequestContext, IntPtr moduleData, Int32 flags)", 10526);
			AddStackLineCount("System.Web.dll", "System.Web.HttpRuntime.ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context)", 5417);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136646);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemDictionary.GetCurrentRegistryVersion()", 221);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.RecordDatabaseUpgradedExceptionAndContinue()", 3738);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemImpl.Enterprise.Integration.IRegistryItemInternals.HasActualValue(Guid CompanyPK, Guid BranchPK, Guid DepartmentPK)", 2959);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryDataAccessor.GetBinaryValue(String name, TimeSpan maximumCacheAge, Guid owner, Guid department)", 4005);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.Command(String sqlText, Nullable`1 cmdTimeoutInSeconds)", 1569);
			AddStackLineCount("Enterprise.ZArchitecture.Web.GUI.dll", "Enterprise.ZArchitecture.Web.GUI.ZGlobal.Application_BeginRequest(Object sender, EventArgs e)", 222);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.Command(String sqlText)", 340);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.RunTasksAfterOpenConnection()", 5219);
			AddStackLineCount("System.Web.dll", "System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()", 369);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.AfterReconnectionTask.Run()", 2550);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemWrapper.Enterprise.Integration.IRegistryItem.get_Value()", 2960);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136798);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemDictionary.PurgeAllIfUpdatedByUser()", 135);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemFallBackValueAccessor.HasActualValueAtThisLevelCore()", 2954);
			AddStackLineCount("System.Web.dll", "System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean completedSynchronously)", 3354);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66140);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbConnection.CheckDbSchemaVersionIsTheSame()", 3965);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemImpl.get_Value()", 4451);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryItemImpl.GetBinaryValue(String Name, RegistryCacheKey cacheKey, Boolean UseFallback)", 5658);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.ConvertableRegistryItem`2[TGet,TSet].get_ValueCore()", 3624);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Environment.RegistryDataAccessor.GetBinaryValue(String name, Guid owner, Guid department)", 132);
			AddStackLineCount("Enterprise.ZArchitecture.Web.GUI.dll", "Enterprise.ZArchitecture.Web.GUI.ZGlobal.RefreshRegistryItemCache()", 26);

			AddPublishedAssembly("Enterprise.ZArchitecture.Core.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Core/Core");
			AddPublishedAssembly("CargoWise.Common.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common");
			AddPublishedAssembly("Enterprise.ZArchitecture.Web.GUI.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Web/GUI");
			AddPublishedAssembly("CargoWise.Data.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Data/CargoWise.Data");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Data/CargoWise.Data", "ENT", "PER", "APP");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common", "ENT", "PER", "APP");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Web/GUI", "ENT", "ARC", "WEB");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Core/Core", "ENT", "ARC", "COR");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			AssertAssignment(log, "ENT", "PER", "APP");
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_PRC()
		{
			var report = GetExceptionXml(@"<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.SqlConnection"" Method=""OnError"" Parameters=""System.Data.SqlClient.SqlException;System.Boolean;System.Action`1[System.Action]"">at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)</Call>
<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.TdsParser"" Method=""ThrowExceptionAndWarning"" Parameters=""System.Data.SqlClient.TdsParserStateObject;System.Boolean;System.Boolean"">at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)</Call>
<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.TdsParser"" Method=""TryRun"" Parameters=""System.Data.SqlClient.RunBehavior;System.Data.SqlClient.SqlCommand;System.Data.SqlClient.SqlDataReader;System.Data.SqlClient.BulkCopySimpleResultSet;System.Data.SqlClient.TdsParserStateObject;System.Boolean"">at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean dataReady)</Call>
<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.SqlCommand"" Method=""RunExecuteNonQueryTds"" Parameters=""System.String;System.Boolean;System.Int32;System.Boolean"">at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)</Call>
<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.SqlCommand"" Method=""InternalExecuteNonQuery"" Parameters=""System.Threading.Tasks.TaskCompletionSource`1[System.Object];System.String;System.Boolean;System.Int32;System.Boolean;System.Boolean;System.Boolean"">at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean usedCache, Boolean asyncWrite, Boolean inRetry)</Call>
<Call Assembly=""System.Data.dll"" Type=""System.Data.SqlClient.SqlCommand"" Method=""ExecuteNonQuery"" Parameters=""NoParameters"">at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.NonQueryCommandRunner"" Method=""ExecuteCore"" Parameters=""System.Data.IDbCommand"">at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbCommand"" Method=""ExecuteCore"" Parameters=""CargoWise.Data.CommandRunner"">at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbCommand"" Method=""Execute"" Parameters=""CargoWise.Data.CommandRunner"">at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)</Call>
<Call Assembly=""CargoWise.Data.dll"" Type=""CargoWise.Data.DbCommand"" Method=""System.Data.IDbCommand.ExecuteNonQuery"" Parameters=""NoParameters"">at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZSaveCommand"" Method=""ExecuteStandardPart"" Parameters=""NoParameters"">at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZSqlSaver"" Method=""SaveSplitRows"" Parameters=""System.Collections.Generic.IList`1[System.Data.DataRow];System.Int32;System.Boolean;System.Boolean"">at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZSqlSaver"" Method=""SaveRows"" Parameters=""System.Collections.Generic.IList`1[System.Data.DataRow]"">at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZSaver"" Method=""Save"" Parameters=""NoParameters"">at CargoWise.EntityFramework.ZSaver.Save()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZAccessor"" Method=""Save"" Parameters=""NoParameters"">at CargoWise.EntityFramework.ZAccessor.Save()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""CargoWise.Integration.ITransactionParticipant.SaveInTransaction"" Parameters=""NoParameters"">at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveInTransactionCore"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveInTransactionCore"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()</Call>
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult]"" Method=""MoveNext"" Parameters=""NoParameters"">at System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult].MoveNext()</Call>
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Buffer`1[TElement]"" Method="".ctor"" Parameters=""System.Collections.Generic.IEnumerable`1[TElement]"">at System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)</Call>
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable"" Method=""ToArray"" Parameters=""System.Collections.Generic.IEnumerable`1[TSource]"">at System.Linq.Enumerable.ToArray(IEnumerable`1 source)</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator"" Method=""SaveInTransactions"" Parameters=""NoParameters"">at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)</Call>
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveCore"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""SaveProcessedLogs"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup;Enterprise.LogWalker.LogSubscriberResult;CargoWise.Integration.ITransactionManager"">at Enterprise.LogWalker.NewsTransmitter.SaveProcessedLogs(Int32 maximumDepth, ProcessableLogGroup logGroup, LogSubscriberResult subscriberResult, ITransactionManager transactionCommitter)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroup"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroup(Int32 maximumDepth, ProcessableLogGroup currentLogGroup)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.get_StackTrace()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.ExceptionFullTracer"" Method=""CaptureTraceToReporter"" Parameters=""System.Exception"">at Enterprise.ZArchitecture.Core.ExceptionFullTracer.CaptureTraceToReporter(Exception ex)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.ExceptionDetails"" Method=""WriteFullReport"" Parameters=""System.Xml.XmlTextWriter"">at Enterprise.ZArchitecture.Core.ExceptionDetails.WriteFullReport(XmlTextWriter xtw)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.ExceptionReportBuilder"" Method=""GenerateReport"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.Core.ExceptionReportBuilder.GenerateReport()</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.BaseExceptionReporter"" Method=""SendReport"" Parameters=""CargoWise.Common.IErrorReporter;Enterprise.ZArchitecture.Core.ExceptionReportArgs"">at Enterprise.ZArchitecture.Core.BaseExceptionReporter.SendReport(IErrorReporter reporter, ExceptionReportArgs reportArgs)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.BaseExceptionReporter"" Method=""ReportSilently"" Parameters=""System.String;System.String;System.Exception"">at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportSilently(String key, String message, Exception ex)</Call>
<Call Assembly=""Enterprise.ZArchitecture.Core.dll"" Type=""Enterprise.ZArchitecture.Core.BaseExceptionReporter"" Method=""ReportDeveloperException"" Parameters=""System.String;System.Exception"">at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportDeveloperException(String message, Exception ex)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroup"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroup(Int32 maximumDepth, ProcessableLogGroup currentLogGroup)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroup"" Parameters=""System.Int32;System.Collections.Generic.Stack`1[Enterprise.LogWalker.ProcessableLogGroup];Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroup(Int32 maximumDepth, Stack`1 logsProcessingStack, ProcessableLogGroup logsGroup)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessAndSave"" Parameters=""System.Collections.Generic.Stack`1[Enterprise.LogWalker.ProcessableLogGroup]"">at Enterprise.LogWalker.NewsTransmitter.ProcessAndSave(Stack`1 logsProcessingStack)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ReadAndProcessQueue"" Parameters=""System.Int32"">at Enterprise.LogWalker.NewsTransmitter.ReadAndProcessQueue(Int32 batchSize)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueSafe"" Parameters=""System.Int32"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueSafe(Int32 batchSize)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueBatchCore"" Parameters=""System.Int32"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatchCore(Int32 batchSize)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueBatch"" Parameters=""System.Int32"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatch(Int32 batchSize)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsBroadcaster"" Method=""ProcessLogs"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsBroadcaster.ProcessLogs(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsBroadcaster"" Method=""Enterprise.LogWalker.ILogWalkerOperation.Execute"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsBroadcaster.Enterprise.LogWalker.ILogWalkerOperation.Execute(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.OperationsManager"" Method=""RunOperationCycle"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Collections.Generic.IEnumerable`1[Enterprise.LogWalker.ILogWalkerOperation];System.Threading.CancellationToken"">at Enterprise.LogWalker.OperationsManager.RunOperationCycle(SubscriberParameters susbscriberParameters, LogSubscriber[] subscribers, IEnumerable`1 operations, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.OperationsManager"" Method=""QueueAndProcessLogs"" Parameters=""Enterprise.Integration.ILogger;System.Threading.CancellationToken"">at Enterprise.LogWalker.OperationsManager.QueueAndProcessLogs(ILogger notifier, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.LogWalkerRunner"" Method=""Process"" Parameters=""Enterprise.Integration.ICategoryLogger`1[Enterprise.LogWalker.LogWalkerCategories];System.Threading.CancellationToken"">at Enterprise.LogWalker.LogWalkerRunner.Process(ICategoryLogger`1 notifier, CancellationToken token)</Call>
<Call Assembly=""Enterprise.ServiceManager.Tasks.LogWalker.dll"" Type=""Enterprise.ServiceManager.Tasks.LogWalker.LogWalkerServiceTaskBase"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Tasks.LogWalker.LogWalkerServiceTaskBase.RunTask(CancellationToken token)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.ServiceProviderImplProxy"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask(CancellationToken cancellationToken)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.BasicServiceProvider"" Method=""Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run(CancellationToken cancellationToken)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunner"" Method=""RunServiceTask"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunner.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerWithNextRunTimeCheck"" Method=""RunServiceTask"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerWithNextRunTimeCheck.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy"" Method=""Run"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor"" Method=""Execute"" Parameters=""Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo"">at Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor.Execute(IRunCommandInfo commandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.IpcRunner"" Method=""RunInternal"" Parameters=""System.Boolean"">at Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.TaskRunner.DisplayClass1_0"" Method=""Runb0"" Parameters=""NoParameters"">at Enterprise.ServiceManager.Runner.TaskRunner.DisplayClass1_0.Runb0()</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""RunInternal"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadHelper"" Method=""ThreadStart"" Parameters=""NoParameters"">at System.Threading.ThreadHelper.ThreadStart()</Call>");

			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadHelper.ThreadStart()", 19948);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 Rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)", 4810);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()", 11188);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37826);
			AddStackLineCount("Enterprise.LogWalker.dll", "Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatch(Int32 batchSize)", 1758);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.ExceptionDetails.WriteFullReport(XmlTextWriter xtw)", 37607);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()", 17166);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()", 15521);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZAccessor.Save()", 10104);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZSaver.Save()", 10893);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)", 9704);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136798);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo)", 1);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)", 11126);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37628);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42877);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.TaskRunner.DisplayClass1_0.Runb0()", 9);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)", 17284);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()", 25795);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunner.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)", 1);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean dataReady)", 40672);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()", 10658);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor.Execute(IRunCommandInfo commandInfo)", 1);
			AddStackLineCount("Enterprise.LogWalker.dll", "Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueSafe(Int32 batchSize)", 1742);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.ToArray(IEnumerable`1 source)", 10833);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()", 18179);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)", 39790);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.BaseExceptionReporter.SendReport(IErrorReporter reporter, ExceptionReportArgs reportArgs)", 35495);
			AddStackLineCount("System.Core.dll", "System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)", 11279);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136646);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.ExceptionReportBuilder.GenerateReport()", 23903);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean usedCache, Boolean asyncWrite, Boolean inRetry)", 8041);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerWithNextRunTimeCheck.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)", 1);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)", 26467);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)", 27345);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbCommand.ExecuteCore(CommandRunner CmdRunner)", 39424);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.SqlCommand.ExecuteNonQuery()", 17382);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportDeveloperException(String message, Exception ex)", 203);
			AddStackLineCount("System.Data.dll", "System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)", 37881);
			AddStackLineCount("CargoWise.Data.dll", "CargoWise.Data.DbCommand.Execute(CommandRunner CmdRunner)", 39735);
			AddStackLineCount("Enterprise.LogWalker.dll", "Enterprise.LogWalker.NewsTransmitter.ReadAndProcessQueue(Int32 batchSize)", 1536);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult].MoveNext()", 7859);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.ExceptionFullTracer.CaptureTraceToReporter(Exception ex)", 32880);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42883);
			AddStackLineCount("Enterprise.LogWalker.dll", "Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatchCore(Int32 batchSize)", 173);
			AddStackLineCount("Enterprise.ZArchitecture.Core.dll", "Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportSilently(String key, String message, Exception ex)", 4099);
			AddPublishedAssembly("CargoWiseOne.ServiceManager.Runner.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner");
			AddPublishedAssembly("Enterprise.LogWalker.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/Scheduler/LogWalker");
			AddPublishedAssembly("CargoWise.Data.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Data/CargoWise.Data");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/EntityFramework/CargoWise.EntityFramework");
			AddPublishedAssembly("Enterprise.ZArchitecture.Core.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Core/Core");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Data/CargoWise.Data", "ENT", "PER", "APP");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner", "ENT", "PRC", "PRC");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/Scheduler/LogWalker", "ENT", "AUT", "PRO");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/EntityFramework/CargoWise.EntityFramework", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/Core/Core", "ENT", "ARC", "COR");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			AssertAssignment(log, "ENT", "PRC", "PRC");
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_INT()
		{
			var report = GetExceptionXml(@"<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>;
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.get_StackTrace()</Call>;
<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" Parameters=""System.String;System.String;System.Exception"">at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.ProcessTask.ProcessTaskDeleteHook"" Method=""&lt;.ctor&gt;b__0_0"" Parameters=""Enterprise.MasterFiles.Business.ProcessTask"">at Enterprise.MasterFiles.Business.ProcessTask.ProcessTaskDeleteHook.&lt;.ctor&gt;b__0_0(ProcessTask processTask)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.ProcessTask"" Method=""Delete"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.ProcessTask.Delete()</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.UniversalTriggerCreator"" Method=""CreateAndDelete"" Parameters=""Enterprise.MasterFiles.Business.IWorkflowProvider;Enterprise.MasterFiles.Business.UniversalTriggerCreator.TemplateTriggerSet"">at Enterprise.MasterFiles.Business.UniversalTriggerCreator.CreateAndDelete(IWorkflowProvider parent, TemplateTriggerSet set)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.UniversalTriggerCreator"" Method=""CreateAndDeleteNonPersistedUniversalTriggers"" Parameters=""Enterprise.MasterFiles.Business.IWorkflowProvider;System.Collections.Generic.IEnumerable`1[Enterprise.MasterFiles.Business.ProcessTask];Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.UniversalTriggerCreator.CreateAndDeleteNonPersistedUniversalTriggers(IWorkflowProvider workflowProvider, IEnumerable`1 currentItems, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowTriggerCollectionIncludingRelatedView.UniversalTriggerRebuilder"" Method=""GetRelatedTasksCore"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.WorkflowTriggerCollectionIncludingRelatedView.UniversalTriggerRebuilder.GetRelatedTasksCore()</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionIncludingRelatedView.SupersetRebuilder"" Method=""GetRelatedTasks"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionIncludingRelatedView.SupersetRebuilder.GetRelatedTasks()</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionIncludingRelatedView.SupersetRebuilder"" Method=""GetElementsForRebuild"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionIncludingRelatedView.SupersetRebuilder.GetElementsForRebuild()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.SubsetBusinessObjectCollection.Rebuilder"" Method=""RebuildCore"" Parameters=""NoParameters"">at CargoWise.EntityFramework.SubsetBusinessObjectCollection.Rebuilder.RebuildCore()</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionView.WorkflowItemCollectionViewRebuilder"" Method=""RebuildCore"" Parameters=""NoParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionView.WorkflowItemCollectionViewRebuilder.RebuildCore()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.SubsetBusinessObjectCollection.Rebuilder"" Method=""Rebuild"" Parameters=""NoParameters"">at CargoWise.EntityFramework.SubsetBusinessObjectCollection.Rebuilder.Rebuild()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollectionView"" Method=""RebuildCore"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectCollectionView.RebuildCore()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollectionView"" Method=""CollectionToFilter_ListChanged"" Parameters=""System.Object;System.ComponentModel.ListChangedEventArgs"">at CargoWise.EntityFramework.BusinessObjectCollectionView.CollectionToFilter_ListChanged(Object sender, ListChangedEventArgs e)</Call>;
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.ComponentModel.ListChangedEventHandler.Invoke(Object sender, ListChangedEventArgs e)</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection.ListChangedSuspender"" Method=""Dispose"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectCollection.ListChangedSuspender.Dispose()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""ReloadWith"" Parameters=""System.Collections.Generic.IEnumerable`1[CargoWise.EntityFramework.BusinessObject];System.Boolean"">at CargoWise.EntityFramework.BusinessObjectCollection.ReloadWith(IEnumerable`1 items, Boolean assumeRowsMissingFromQueryResultsAreDeleted)</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""Reload"" Parameters=""System.Boolean;System.Boolean"">at CargoWise.EntityFramework.BusinessObjectCollection.Reload(Boolean reLoadExistingRows, Boolean assumeRowsMissingFromQueryResultsAreDeleted)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionView"" Method=""CreateItemsFromTemplateCore"" Parameters=""Enterprise.MasterFiles.Business.ProcessTaskTemplate;Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionView.CreateItemsFromTemplateCore(ProcessTaskTemplate template, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionView.ItemWithUserDefinedConditionCollectionView"" Method=""CreateItemsFromTemplateCore"" Parameters=""Enterprise.MasterFiles.Business.ProcessTaskTemplate;Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionView.ItemWithUserDefinedConditionCollectionView.CreateItemsFromTemplateCore(ProcessTaskTemplate template, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowItemCollectionView"" Method=""CreateItemsFromAllTemplates"" Parameters=""Enterprise.MasterFiles.Business.IWorkflowProvider;System.Collections.Generic.IEnumerable`1[Enterprise.MasterFiles.Business.ProcessTaskTemplate];Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.WorkflowItemCollectionView.CreateItemsFromAllTemplates(IWorkflowProvider workflowProvider, IEnumerable`1 templates, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable.&lt;SelectManyIterator&gt;d__17`2[TSource,TResult]"" Method=""MoveNext"" Parameters=""NoParameters"">at System.Linq.Enumerable.&lt;SelectManyIterator&gt;d__17`2[TSource,TResult].MoveNext()</Call>;
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Buffer`1[TElement]"" Method="".ctor"" Parameters=""System.Collections.Generic.IEnumerable`1[TElement]"">at System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)</Call>;
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable"" Method=""ToArray"" Parameters=""System.Collections.Generic.IEnumerable`1[TSource]"">at System.Linq.Enumerable.ToArray(IEnumerable`1 source)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.ProcessTask.Loader"" Method=""ApplyWorkflowTemplates"" Parameters=""CargoWise.EntityFramework.BusinessObject;Enterprise.MasterFiles.Business.IWorkflowProvider;Enterprise.MasterFiles.Business.TemplateApplicationParameters;System.Collections.Generic.IEnumerable`1[Enterprise.MasterFiles.Business.ProcessTaskTemplate];System.Boolean"">at Enterprise.MasterFiles.Business.ProcessTask.Loader.ApplyWorkflowTemplates(BusinessObject parent, IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters, IEnumerable`1 templates, Boolean checkTemplatesForCurrentCompany)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.ProcessTask.Loader"" Method=""CreateTasksAndMilestonesFromTemplateIfRequiredCore"" Parameters=""Enterprise.MasterFiles.Business.IWorkflowProvider;Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.ProcessTask.Loader.CreateTasksAndMilestonesFromTemplateIfRequiredCore(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.ProcessTask.Loader"" Method=""CreateTasksAndMilestonesFromTemplateIfRequired"" Parameters=""Enterprise.MasterFiles.Business.IWorkflowProvider;Enterprise.MasterFiles.Business.TemplateApplicationParameters"">at Enterprise.MasterFiles.Business.ProcessTask.Loader.CreateTasksAndMilestonesFromTemplateIfRequired(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters)</Call>;
<Call Assembly=""Enterprise.Freight.Forwarding.Business.dll"" Type=""Enterprise.Freight.Forwarding.Business.ForwardingShipment"" Method=""CreateTasksAndMilestonesFromTemplate"" Parameters=""Enterprise.MasterFiles.Business.ProcessTask.Loader"">at Enterprise.Freight.Forwarding.Business.ForwardingShipment.CreateTasksAndMilestonesFromTemplate(Loader processLoader)</Call>;
<Call Assembly=""Enterprise.Freight.Forwarding.Business.dll"" Type=""Enterprise.Freight.Forwarding.Business.ForwardingShipment"" Method=""OnFactorySavingBeforeTransactionCore"" Parameters=""NoParameters"">at Enterprise.Freight.Forwarding.Business.ForwardingShipment.OnFactorySavingBeforeTransactionCore()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller"" Method=""CallMethodOnAllBusinessObjects"" Parameters=""CargoWise.EntityFramework.BusinessObjectFactory"">at CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller.CallMethodOnAllBusinessObjects(BusinessObjectFactory factory)</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""CargoWise.Integration.ITransactionParticipant.OnAllTransactionsBeginning"" Parameters=""NoParameters"">at CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsBeginning()</Call>;
<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.IEnumerableExtensions"" Method=""ForEach"" Parameters=""System.Collections.Generic.IEnumerable`1[TSource];System.Action`1[TSource]"">at CargoWise.Common.IEnumerableExtensions.ForEach(IEnumerable`1 source, Action`1 action)</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator"" Method=""BeginTransactionWithManager"" Parameters=""NoParameters"">at CargoWise.EntityFramework.TransactionCoordinator.BeginTransactionWithManager()</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>;
<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveInternal"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""PerformSave"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.GUI.ZForm.PerformSave()</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ValidateAndSave"" Parameters=""NoParameters"">at Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()</Call>;
<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.GUI.ShipmentForm"" Method=""ValidateAndSave"" Parameters=""NoParameters"">at Enterprise.Freight.Forwarding.GUI.ShipmentForm.ValidateAndSave()</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveFormCore"" Parameters=""System.Boolean"">at Enterprise.ZArchitecture.GUI.ZForm.SaveFormCore(Boolean closeOnSave)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClickUnsafe"" Parameters=""System.Boolean"">at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClick"" Parameters=""System.Boolean;System.Boolean"">at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveFormProper"" Parameters=""System.Boolean;System.Object;System.Boolean"">at Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""RaiseEvent"" Parameters=""System.Object;System.EventArgs"">at System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripButton"" Method=""OnClick"" Parameters=""System.EventArgs"">at System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZToolStripButton"" Method=""OnClick"" Parameters=""System.EventArgs"">at Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""HandleClick"" Parameters=""System.EventArgs"">at System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""PerformClick"" Parameters=""NoParameters"">at System.Windows.Forms.ToolStripItem.PerformClick()</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormMenuStrategy"" Method=""RaiseButtonClick"" Parameters=""System.Windows.Forms.Form;Enterprise.ZArchitecture.GUI.IButton;System.String"">at Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.RaiseButtonClick(Form form, IButton button, String fieldName)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.CDisplay240"" Method=""InitialiseMainMenub__1"" Parameters=""System.Object;System.EventArgs"">at Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.CDisplay240.InitialiseMainMenub__1(Object p0, EventArgs p1)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""OnClick"" Parameters=""System.EventArgs"">at System.Windows.Forms.MenuItem.OnClick(EventArgs e)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMenuItem"" Method=""OnClick"" Parameters=""System.EventArgs"">at Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""ShortcutClick"" Parameters=""NoParameters"">at System.Windows.Forms.MenuItem.ShortcutClick()</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMainMenu"" Method=""ProcessCmdKey"" Parameters=""System.Windows.Forms.Message;System.Windows.Forms.Keys"">at Enterprise.ZArchitecture.GUI.ZMainMenu.ProcessCmdKey(Message msg, Keys keyData)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Form"" Method=""ProcessCmdKey"" Parameters=""System.Windows.Forms.Message;System.Windows.Forms.Keys"">at System.Windows.Forms.Form.ProcessCmdKey(Message msg, Keys keyData)</Call>;
<Call Assembly=""CargoWise.Windows.UI.dll"" Type=""CargoWise.Windows.UI.KForm"" Method=""ProcessCmdKey"" Parameters=""System.Windows.Forms.Message;System.Windows.Forms.Keys"">at CargoWise.Windows.UI.KForm.ProcessCmdKey(Message msg, Keys keyData)</Call>;
<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ProcessCmdKey"" Parameters=""System.Windows.Forms.Message;System.Windows.Forms.Keys"">at Enterprise.ZArchitecture.GUI.ZForm.ProcessCmdKey(Message msg, Keys keyData)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" Parameters=""System.Windows.Forms.Message;System.Windows.Forms.Keys"">at System.Windows.Forms.Control.ProcessCmdKey(Message msg, Keys keyData)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessMessage"" Parameters=""System.Windows.Forms.Message"">at System.Windows.Forms.Control.PreProcessMessage(Message msg)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessControlMessageInternal"" Parameters=""System.Windows.Forms.Control;System.Windows.Forms.Message"">at System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message msg)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""PreTranslateMessage"" Parameters=""System.Windows.Forms.NativeMethods.MSG"">at System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG msg)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" Parameters=""System.IntPtr;System.Int32;System.Int32"">at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>;
<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>;
<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" Parameters=""System.String[]"">at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>;
<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" Parameters=""System.String[]"">at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>;");

			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveFormCore(Boolean closeOnSave)", 56);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG msg)", 11722);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)", 18633);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollectionView.CollectionToFilter_ListChanged(Object sender, ListChangedEventArgs e)", 434);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ProcessCmdKey(Message msg, Keys keyData)", 4983);
			AddStackLineCount("System.Core.dll", "System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)", 11279);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.ProcessTask.Loader.CreateTasksAndMilestonesFromTemplateIfRequiredCore(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters)", 406);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)", 3261);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.ProcessTask.Delete()", 7);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.BeginTransactionWithManager()", 115);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)", 17749);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66140);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80733);
			AddStackLineCount("CargoWise.Windows.UI.dll", "CargoWise.Windows.UI.KForm.ProcessCmdKey(Message msg, Keys keyData)", 2532);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.OnClick(EventArgs e)", 19807);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)", 102219);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.ProcessCmdKey(Message msg, Keys keyData)", 54594);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.IEnumerableExtensions.ForEach(IEnumerable`1 source, Action`1 action)", 1078);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller.CallMethodOnAllBusinessObjects(BusinessObjectFactory factory)", 3509);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.PerformSave()", 14176);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.ToArray(IEnumerable`1 source)", 10833);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Form.ProcessCmdKey(Message msg, Keys keyData)", 4933);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessMessage(Message msg)", 9791);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()", 14512);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)", 1513);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMainMenu.ProcessCmdKey(Message msg, Keys keyData)", 4924);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)", 1922);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136798);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.RaiseButtonClick(Form form, IButton button, String fieldName)", 4222);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.GUI.ShipmentForm.ValidateAndSave()", 4755);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)", 20664);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)", 16186);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37628);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.ProcessTask.Loader.CreateTasksAndMilestonesFromTemplateIfRequired(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters)", 406);
			AddStackLineCount("None", "System.ComponentModel.ListChangedEventHandler.Invoke(Object sender, ListChangedEventArgs e)", 3570);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.PerformClick()", 4599);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollectionView.RebuildCore()", 67);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()", 17704);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollection.ListChangedSuspender.Dispose()", 411);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 103045);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 103047);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37826);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.ShortcutClick()", 4957);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136646);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsBeginning()", 1986);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)", 20439);
			AddStackLineCount("Enterprise.Freight.Forwarding.Business.dll", "Enterprise.Freight.Forwarding.Business.ForwardingShipment.OnFactorySavingBeforeTransactionCore()", 530);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80735);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message msg)", 9793);
			AddPublishedAssembly("CargoWise.Windows.UI.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/Windows.UI");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.Business.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.Business");
			AddPublishedAssembly("CargoWise.Common.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.GUI.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.GUI");
			AddPublishedAssembly("Enterprise.MasterFiles.Business.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Main/Enterprise.Main/CargoWiseOneProjects");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/GUI/Enterprise.ZArchitecture.GUI");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/EntityFramework/CargoWise.EntityFramework");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/EntityFramework/CargoWise.EntityFramework", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.Business", "ENT", "INT", "FOR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Main/Enterprise.Main/CargoWiseOneProjects", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Common/Architecture/Windows.UI", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Architecture/GUI/Enterprise.ZArchitecture.GUI", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.GUI", "ENT", "INT", "FOR");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common", "ENT", "PER", "APP");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			AssertAssignment(log, "ENT", "INT", "FOR");
		}

		[DeveloperOnlyTest]
		public void TestGetAssignment_AUT()
		{
			var report = GetExceptionXml(@"<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Object.GetType()</Call>
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowSetFieldProcessor.&lt;&gt;c__DisplayClass16_0"" Method=""&lt;Process&gt;b__1"" Parameters=""CargoWise.EntityFramework.IBusiness"">at Enterprise.MasterFiles.Business.WorkflowSetFieldProcessor.&lt;&gt;c__DisplayClass16_0.&lt;Process&gt;b__1(IBusiness x)</Call>
<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable"" Method=""FirstOrDefault"" Parameters=""System.Collections.Generic.IEnumerable`1[TSource];System.Func`2[TSource,System.Boolean]"">at System.Linq.Enumerable.FirstOrDefault(IEnumerable`1 source, Func`2 predicate)</Call>
<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.WorkflowSetFieldProcessor"" Method=""Process"" Parameters=""CargoWise.ComponentModel.INotifications;System.Threading.CancellationToken"">at Enterprise.MasterFiles.Business.WorkflowSetFieldProcessor.Process(INotifications notifications, CancellationToken token)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.DefaultTriggerActionRunner"" Method=""Run"" Parameters=""Enterprise.WorkflowManager.ServiceTasks.IWorkflowTriggerAction"">at Enterprise.WorkflowManager.ServiceTasks.DefaultTriggerActionRunner.Run(IWorkflowTriggerAction workflowTriggerAction)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager"" Method=""RunAction"" Parameters=""Enterprise.WorkflowManager.ServiceTasks.IWorkflowTriggerAction"">at Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager.RunAction(IWorkflowTriggerAction action)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager"" Method=""RunActions"" Parameters=""CargoWise.EntityFramework.BusinessObject;Enterprise.MasterFiles.Integration.IWorkflowTrigger;Enterprise.MasterFiles.Integration.IQueuedLog"">at Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager.RunActions(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager"" Method=""Run"" Parameters=""CargoWise.EntityFramework.BusinessObject;Enterprise.MasterFiles.Integration.IWorkflowTrigger;Enterprise.MasterFiles.Integration.IQueuedLog"">at Enterprise.WorkflowManager.ServiceTasks.WorkflowTriggerActionManager.Run(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.WorkflowEventTriggerProcessor"" Method=""RunTriggerActions"" Parameters=""CargoWise.EntityFramework.BusinessObject;Enterprise.MasterFiles.Integration.IWorkflowTrigger;Enterprise.MasterFiles.Integration.IQueuedLog;CargoWise.ComponentModel.INotifications"">at Enterprise.WorkflowManager.ServiceTasks.WorkflowEventTriggerProcessor.RunTriggerActions(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog, INotifications notifications)</Call>
<Call Assembly=""Enterprise.WorkflowManager.ServiceTasks.dll"" Type=""Enterprise.WorkflowManager.ServiceTasks.WorkflowEventTriggerProcessor"" Method=""ProcessBatch"" Parameters=""Enterprise.MasterFiles.Integration.IQueuedLog[]"">at Enterprise.WorkflowManager.ServiceTasks.WorkflowEventTriggerProcessor.ProcessBatch(IQueuedLog[] queuedLogs)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.LogSubscriber"" Method=""ProcessLogs"" Parameters=""Enterprise.MasterFiles.Integration.IQueuedLog[]"">at Enterprise.LogWalker.LogSubscriber.ProcessLogs(IQueuedLog[] queuedLogs)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroupCore"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroupCore(Int32 maximumDepth, ProcessableLogGroup currentLogGroup)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
<Call Assembly=""None"" Type=""None"" Method=""None"" Parameters=""None"">at System.Environment.get_StackTrace()</Call>
<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" Parameters=""System.String;System.String;System.Exception"">at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ReportLWKFailure"" Parameters=""Enterprise.LogWalker.ProcessableLogGroup;System.Collections.Generic.IEnumerable`1[System.Exception];System.Boolean"">at Enterprise.LogWalker.NewsTransmitter.ReportLWKFailure(ProcessableLogGroup currentLogGroup, IEnumerable`1 exceptions, Boolean reportWithoutExceptionHandling)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroupCore"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroupCore(Int32 maximumDepth, ProcessableLogGroup currentLogGroup)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogGroup"" Parameters=""System.Int32;Enterprise.LogWalker.ProcessableLogGroup"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogGroup(Int32 maximumDepth, ProcessableLogGroup logsGroup)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessAndSave"" Parameters=""Enterprise.LogWalker.ProcessableLogGroupStack;System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsTransmitter.ProcessAndSave(ProcessableLogGroupStack logsProcessingStack, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ReadAndProcessQueue"" Parameters=""System.Int32;System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsTransmitter.ReadAndProcessQueue(Int32 batchSize, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueSafe"" Parameters=""System.Int32;System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueSafe(Int32 batchSize, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueBatchCore"" Parameters=""System.Int32;System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatchCore(Int32 batchSize, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsTransmitter"" Method=""ProcessLogQueueBatch"" Parameters=""System.Int32;System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsTransmitter.ProcessLogQueueBatch(Int32 batchSize, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsBroadcaster"" Method=""CallQueuedLogsProcessor"" Parameters=""Enterprise.LogWalker.LogSubscriber;Enterprise.LogWalker.LogSubscriber[];Enterprise.LogWalker.SubscriberParameters"">at Enterprise.LogWalker.NewsBroadcaster.CallQueuedLogsProcessor(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsBroadcaster"" Method=""ProcessLogs"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsBroadcaster.ProcessLogs(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.NewsBroadcaster"" Method=""Enterprise.LogWalker.ILogWalkerOperation.Execute"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Threading.CancellationToken"">at Enterprise.LogWalker.NewsBroadcaster.Enterprise.LogWalker.ILogWalkerOperation.Execute(SubscriberParameters subscriberParameters, LogSubscriber[] subscribers, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.OperationsManager"" Method=""RunOperationCycle"" Parameters=""Enterprise.LogWalker.SubscriberParameters;Enterprise.LogWalker.LogSubscriber[];System.Collections.Generic.IEnumerable`1[Enterprise.LogWalker.ILogWalkerOperation];System.Threading.CancellationToken"">at Enterprise.LogWalker.OperationsManager.RunOperationCycle(SubscriberParameters susbscriberParameters, LogSubscriber[] subscribers, IEnumerable`1 operations, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.OperationsManager"" Method=""QueueAndProcessLogs"" Parameters=""Enterprise.Integration.ILogger;System.Threading.CancellationToken"">at Enterprise.LogWalker.OperationsManager.QueueAndProcessLogs(ILogger notifier, CancellationToken token)</Call>
<Call Assembly=""Enterprise.LogWalker.dll"" Type=""Enterprise.LogWalker.LogWalkerRunner"" Method=""Process"" Parameters=""Enterprise.Integration.ICategoryLogger`1[Enterprise.LogWalker.LogWalkerCategories];System.Threading.CancellationToken"">at Enterprise.LogWalker.LogWalkerRunner.Process(ICategoryLogger`1 notifier, CancellationToken token)</Call>
<Call Assembly=""Enterprise.ServiceManager.Tasks.LogWalker.dll"" Type=""Enterprise.ServiceManager.Tasks.LogWalker.LogWalkerServiceTaskBase"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Tasks.LogWalker.LogWalkerServiceTaskBase.RunTask(CancellationToken token)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.ServiceProviderImplProxy"" Method=""RunTask"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask(CancellationToken cancellationToken)</Call>
<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.BasicServiceProvider"" Method=""Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run"" Parameters=""System.Threading.CancellationToken"">at Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run(CancellationToken cancellationToken)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunner"" Method=""RunServiceTask"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunner.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerWithNextRunTimeCheck"" Method=""RunServiceTask"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo;System.Threading.CancellationTokenSource"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerWithNextRunTimeCheck.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy"" Method=""Run"" Parameters=""Enterprise.ServiceManager.Shared.IServiceTaskHandler;Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo"">at Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor"" Method=""Execute"" Parameters=""Enterprise.ServiceManager.Runner.Commands.IRunCommandInfo"">at Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor.Execute(IRunCommandInfo commandInfo)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.IpcRunner"" Method=""RunInternal"" Parameters=""System.Boolean"">at Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)</Call>
<Call Assembly=""CargoWiseOne.ServiceManager.Runner.dll"" Type=""Enterprise.ServiceManager.Runner.TaskRunner.&lt;&gt;c__DisplayClass1_0"" Method=""&lt;Run&gt;b__0"" Parameters=""NoParameters"">at Enterprise.ServiceManager.Runner.TaskRunner.&lt;&gt;c__DisplayClass1_0.&lt;Run&gt;b__0()</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""RunInternal"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object"">at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)</Call>
<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadHelper"" Method=""ThreadStart"" Parameters=""NoParameters"">at System.Threading.ThreadHelper.ThreadStart()</Call>");

			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136646);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)", 27345);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)", 26467);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunnerStrategy.Run(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo)", 1);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66140);
			AddStackLineCount("None", "System.Object.GetType()", 14);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.ServiceTaskRunner.RunServiceTask(IServiceTaskHandler serviceTask, IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)", 1);
			AddStackLineCount("Enterprise.LogWalker.dll", "Enterprise.LogWalker.LogSubscriber.ProcessLogs(IQueuedLog[] queuedLogs)", 1392);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.TaskRunner.<>c__DisplayClass1_0.<Run>b__0()", 9);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42883);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136798);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42877);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.dll", "Enterprise.ServiceManager.Runner.CommandExecutors.RunCommandExecutor.Execute(IRunCommandInfo commandInfo)", 1);
			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadHelper.ThreadStart()", 19948);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.FirstOrDefault(IEnumerable`1 source, Func`2 predicate)", 486);
			AddPublishedAssembly("CargoWiseOne.ServiceManager.Runner.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner");
			AddPublishedAssembly("Enterprise.LogWalker.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/Scheduler/LogWalker");
			AddPublishedAssembly("CargoWise.Common.dll", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner", "ENT", "PRC", "PRC");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Database/Generic/Common/CargoWise.Common", "ENT", "PER", "APP");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=/Enterprise/Product/Core/Scheduler/LogWalker", "ENT", "AUT", "PRO");

			var log = CreateLog();
			CreateLogOccurrence(log, report);
			Factory.Save();

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiUrl))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiTimeout))
			{
				var assignment = new StackLinesWeightsLogAutoAssigner().GetAssignment(log, new DataFormatter());

				AssertAssignment(log, "ENT", "AUT", "PRO");
			}
		}

		#region Implementations

		void AssertAssignment(EdiHelpErrorLog log, string expectedProduct, string expectedProductArea, string expectedModule)
		{
			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiUrl))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiTimeout))
			{
				var assignment = new StackLinesWeightsLogAutoAssigner().GetAssignment(log, new DataFormatter());

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				AssertEquals(new IssueAssignment(expectedProduct, expectedProductArea, expectedModule), assignment);
				Assert(assignment.IsRetrievedFromAPI);
			}
		}

		#endregion
	}
}
