using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert.Testing
{
	class SqlCpuUsageAlertProviderTest : TransactionedTestCase
	{
		public void TestObtainGlbCapabilitiesIdBatch()
		{
			var factory = new BusinessObjectFactory();
			var defaultCapability = factory.NewWithValidTestData<GlbCapability>();
			defaultCapability.G4_Code = AlertProcessor.DefaultCapabilityCode;
			var capability = factory.NewWithValidTestData<GlbCapability>();
			factory.Save();

			var mocklogger = new Mock<ILogger>().Object;
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			using (var dbConnection = Db.Connection)
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults);
				var capabilityMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { capability.G4_Code.ToString() }, factory);
				AssertEquals(capability.PK, capabilityMap[capability.G4_Code]);

				var notExistCapabilityMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { "NAV" }, factory);
				AssertEquals(defaultCapability.PK, notExistCapabilityMap["NAV"]);

				var defaultCapMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { "" }, factory);
				AssertEquals(defaultCapability.PK, defaultCapMap[""]);
			}
		}

		public void TestObtainGlbCapabilitiesIdBatchWithoutDefaultCapability()
		{
			var factory = new BusinessObjectFactory();
			var capability = factory.NewWithValidTestData<GlbCapability>();
			factory.Save();

			var mocklogger = new Mock<ILogger>();
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			using (var dbConnection = Db.Connection)
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger.Object, dbConnection, alertDefaults);
				var capabilityMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { capability.G4_Code.ToString() }, factory);
				AssertEquals(capability.PK, capabilityMap[capability.G4_Code]);

				var notExistCapabilityMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { "NAV" }, factory);
				Assert(!notExistCapabilityMap.TryGetValue("NAV", out _));

				var defaultCapMap = sqlCpuUsageAlertProvider.ObtainGlbCapabilitiesIdBatch(new List<string>() { "" }, factory);
				Assert(!notExistCapabilityMap.TryGetValue("", out _));

				mocklogger.Verify(x => x.Log(LogType.Error, It.Is<string>(s => s.StartsWith($"Default capability '{AlertProcessor.DefaultCapabilityCode}' does not exist"))), Times.Exactly(2));
			}
		}

		public void TestFindAlertTargetCreateIfNotExists()
		{
			var mocklogger = new Mock<ILogger>().Object;
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			var capabilityCounts = new Dictionary<string, int>();
			var cpuUsageAlert = new SqlCpuUsageAlert();
			cpuUsageAlert.AlertName = "Customs";
			cpuUsageAlert.Path = "1234567";
			cpuUsageAlert.Owner = "CW1";
			cpuUsageAlert.ServerInstanceName = "whatever.server\\WHATEVER_INSTANCE";
			cpuUsageAlert.QueryHash = "0x19981008abcde";
			var alertRule = new AlertRule();
			alertRule.AlertName = "EXPENSIVE_SQL";
			alertRule.Path = "Customs";
			alertRule.Capability = "TLR";
			var alertCapability = new AlertCapability();
			alertCapability.Capability = "TLR";
			alertCapability.Product = "ENT";
			alertCapability.Module = "CUS";
			alertCapability.ProgramArea = "CUS";
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var defaultPlanUri = @"https://eye.wtg.ws/.../discover#/?_a=(columns:!(System,QueryHash,Owner,CpuTimeMilliseconds,Qty,LogicalReads,PhysicalReads,Writes,ExeDate),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,index:f869ba17-8818-56c3-8b70-c74095a6801d,key:QueryHash,negate:!f,params:(query:'{0}'),type:phrase),query:(match_phrase:(QueryHash:'{0}')))))";
			var capabilityJobsList = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertCapability.Capability, (1, new[] { alertRule.Path }, Array.Empty<ZString>(), alertCapability.CapabilityId) }
			};

			using (var dbConnection = Db.Connection)
			using (EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPlanUri))
			{
				var expectedExecutionPlan = "Pretend it is an execution plan";
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults, new SqlExecutionPlanRetrieverForTest(expectedExecutionPlan));

				var (result, alertTarget, descriptivePath, task) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert, alertRule, boFactory, alertCapability.CapabilityId, capabilityJobsList);
				AssertEquals(AddAlertTargetResult.TargetAdded, result);
				Assert(alertTarget is WorkItem);
				var workItem = (WorkItem)alertTarget;
				Assert(workItem.WKI_Summary.EndsWith("1234567"));
				var workItemDetails = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails.Contains("1234567"));
				Assert(workItemDetails.Contains("WtgQueryHash"));
				Assert(!workItemDetails.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails.Contains("WTG Query Hash"));
				Assert(!workItemDetails.Replace("WTG Query Hash", "").Contains("Query Hash"));

				AssertSqlExecutionAttachedToWI(workItem, cpuUsageAlert, expectedExecutionPlan);
			}
		}

		void AssertSqlExecutionAttachedToWI(WorkItem workItem, SqlCpuUsageAlert alert, string expectedExecutionPlan)
		{
			var eDocs = workItem.DocManagerInfo.AllEDocs;
			AssertNotNull(eDocs);
			AssertEquals(1, eDocs.Count);

			var sqlExecutionPlanDoc = eDocs[0];
			AssertNotNull(sqlExecutionPlanDoc);
			AssertEquals($"SQL Execution Plan - {alert.ServerInstanceName.Replace('\\', '_')}.sqlplan", sqlExecutionPlanDoc.FileName);
			AssertEquals("The execution plan of this SQL CPU Usage alert.", sqlExecutionPlanDoc.Description.ToString());
			AssertEquals("LTR", sqlExecutionPlanDoc.DocType);
			AssertEquals(expectedExecutionPlan, Encoding.ASCII.GetString(sqlExecutionPlanDoc.ImageData));
		}

		public void TestFindAlertTargetCreateIfNotExists_ShouldCreateNewWI_WhenSameAlertOccursButTheOldHasClosed()
		{
			var mocklogger = new Mock<ILogger>().Object;
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			var capabilityCounts = new Dictionary<string, int>();
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var (cpuUsageAlert, alertRule, glbCapabilityId) = MockAlertAndCapabilityData("1234567");
			var (cpuUsageAlert1, alertRule1, glbCapabilityId1) = MockAlertAndCapabilityData("1234567");
			var defaultPlanUri = @"https://eye.wtg.ws/.../discover#/?_a=(columns:!(System,QueryHash,Owner,CpuTimeMilliseconds,Qty,LogicalReads,PhysicalReads,Writes,ExeDate),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,index:f869ba17-8818-56c3-8b70-c74095a6801d,key:QueryHash,negate:!f,params:(query:'{0}'),type:phrase),query:(match_phrase:(QueryHash:'{0}')))))";
			var capabilityJobsList = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule.Capability, (1, new[] { alertRule.Path }, Array.Empty<ZString>(), glbCapabilityId) }
			};
			var capabilityJobsList1 = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule1.Capability, (1, new[] { alertRule1.Path }, Array.Empty<ZString>(), glbCapabilityId1) }
			};

			using (var dbConnection = Db.Connection)
			using (EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPlanUri))
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults);
				Assert(sqlCpuUsageAlertProvider != null);

				var (result, alertTarget, descriptivePath, task) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert, alertRule, boFactory, glbCapabilityId, capabilityJobsList);
				AssertEquals(AddAlertTargetResult.TargetAdded, result);
				Assert(alertTarget is WorkItem);
				var workItem = (WorkItem)alertTarget;
				workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Assert(workItem.WKI_Summary.EndsWith("1234567"));
				var workItemDetails = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails.Contains("1234567"));
				Assert(workItemDetails.Contains("WtgQueryHash"));
				Assert(!workItemDetails.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails.Contains("WTG Query Hash"));
				Assert(!workItemDetails.Replace("WTG Query Hash", "").Contains("Query Hash"));

				boFactory.Save();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

				var (result1, alertTarget1, descriptivePath1, task1) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert1, alertRule1, boFactory, glbCapabilityId1, capabilityJobsList1);
				AssertEquals(AddAlertTargetResult.TargetAdded, result1);
				var workItem1 = (WorkItem)alertTarget1;
				Assert(workItem1.WKI_Summary.EndsWith("1234567"));
				var workItemDetails1 = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails1.Contains("1234567"));
				Assert(workItemDetails1.Contains("WtgQueryHash"));
				Assert(!workItemDetails1.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails1.Contains("WTG Query Hash"));
				Assert(!workItemDetails1.Replace("WTG Query Hash", "").Contains("Query Hash"));
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem1.WKI_Status);
				AssertNotEquals(workItem.PK, workItem1.PK);
			}
		}

		public void TestFindAlertTargetCreateIfNotExists_ShouldCreateRecently_WhenSameAlertOccursAndTheOldOneOpenAndMaxCountGreaterThanOne()
		{
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "2" } };
			var mocklogger = new Mock<ILogger>().Object;
			var capabilityCounts = new Dictionary<string, int>();
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var (cpuUsageAlert, alertRule, glbCapabilityId) = MockAlertAndCapabilityData("1234567");
			var (cpuUsageAlert1, alertRule1, glbCapabilityId1) = MockAlertAndCapabilityData("1234567");
			var defaultPlanUri = @"https://eye.wtg.ws/.../discover#/?_a=(columns:!(System,QueryHash,Owner,CpuTimeMilliseconds,Qty,LogicalReads,PhysicalReads,Writes,ExeDate),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,index:f869ba17-8818-56c3-8b70-c74095a6801d,key:QueryHash,negate:!f,params:(query:'{0}'),type:phrase),query:(match_phrase:(QueryHash:'{0}')))))";
			var capabilityJobsList = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule.Capability, (2, new[] { alertRule.Path }, Array.Empty<ZString>(), glbCapabilityId) }
			};
			var capabilityJobsList1 = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule1.Capability, (2, new[] { alertRule1.Path }, new ZString[] { "SQL CPU Usage alert for Customs - 1234567" }, glbCapabilityId1) }
			};

			using (var dbConnection = Db.Connection)
			using (EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPlanUri))
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults);
				Assert(sqlCpuUsageAlertProvider != null);

				var (result, alertTarget, descriptivePath, task) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert, alertRule, boFactory, glbCapabilityId, capabilityJobsList);
				AssertEquals(AddAlertTargetResult.TargetAdded, result);
				Assert(alertTarget is WorkItem);
				var workItem = (WorkItem)alertTarget;
				var newTask = workItem.WorkflowItems.AddNew();
				newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				newTask.P9_GS_NKAssignedStaffMember = "E";
				Assert(workItem.WKI_Summary.EndsWith("1234567"));
				var workItemDetails = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails.Contains("1234567"));
				Assert(workItemDetails.Contains("WtgQueryHash"));
				Assert(!workItemDetails.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails.Contains("WTG Query Hash"));
				Assert(!workItemDetails.Replace("WTG Query Hash", "").Contains("Query Hash"));

				boFactory.Save();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

				var (result1, alertTarget1, descriptivePath1, task1) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert1, alertRule1, boFactory, glbCapabilityId1, capabilityJobsList1);
				AssertEquals(AddAlertTargetResult.AlreadyCreatedRecently, result1);
			}
		}

		public void TestFindAlertTargetCreateIfNotExists_ShouldReachMaxCount_WhenSameAlertOccursAndTheOldOneOpen()
		{
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			var mocklogger = new Mock<ILogger>().Object;
			var capabilityCounts = new Dictionary<string, int>();
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var (cpuUsageAlert, alertRule, glbCapabilityId) = MockAlertAndCapabilityData("1234567");
			var (cpuUsageAlert1, alertRule1, glbCapabilityId1) = MockAlertAndCapabilityData("1234567");
			var defaultPlanUri = @"https://eye.wtg.ws/.../discover#/?_a=(columns:!(System,QueryHash,Owner,CpuTimeMilliseconds,Qty,LogicalReads,PhysicalReads,Writes,ExeDate),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,index:f869ba17-8818-56c3-8b70-c74095a6801d,key:QueryHash,negate:!f,params:(query:'{0}'),type:phrase),query:(match_phrase:(QueryHash:'{0}')))))";
			var capabilityJobsList = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule.Capability, (1, new[] { alertRule.Path }, Array.Empty<ZString>(), glbCapabilityId) }
			};
			var capabilityJobsList1 = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule1.Capability, (1, new[] { alertRule1.Path }, new ZString[1], glbCapabilityId1) }
			};

			using (var dbConnection = Db.Connection)
			using (EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPlanUri))
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults);
				Assert(sqlCpuUsageAlertProvider != null);

				var (result, alertTarget, descriptivePath, task) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert, alertRule, boFactory, glbCapabilityId, capabilityJobsList);
				AssertEquals(AddAlertTargetResult.TargetAdded, result);
				Assert(alertTarget is WorkItem);
				var workItem = (WorkItem)alertTarget;
				var newTask = workItem.WorkflowItems.AddNew();
				newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				newTask.P9_GS_NKAssignedStaffMember = "E";
				Assert(workItem.WKI_Summary.EndsWith("1234567"));
				var workItemDetails = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails.Contains("1234567"));
				Assert(workItemDetails.Contains("WtgQueryHash"));
				Assert(!workItemDetails.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails.Contains("WTG Query Hash"));
				Assert(!workItemDetails.Replace("WTG Query Hash", "").Contains("Query Hash"));

				boFactory.Save();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

				var (result1, alertTarget1, descriptivePath1, task1) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert1, alertRule1, boFactory, glbCapabilityId1, capabilityJobsList1);
				AssertEquals(AddAlertTargetResult.ReachedMaxAmount, result1);
			}
		}

		public void TestFindAlertTargetCreateIfNotExists_ShouldCreateNewWI_WhenDifferentAlertForClientEvenIfReachMax()
		{
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			var mocklogger = new Mock<ILogger>().Object;
			var capabilityCounts = new Dictionary<string, int>();
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var (cpuUsageAlert, alertRule, glbCapabilityId) = MockAlertAndCapabilityData("1234567");
			var (cpuUsageAlert1, alertRule1, glbCapabilityId1) = MockAlertAndCapabilityData("12345678", "EDI");
			cpuUsageAlert1.Path = "12345678";
			var defaultPlanUri = @"https://eye.wtg.ws/.../discover#/?_a=(columns:!(System,QueryHash,Owner,CpuTimeMilliseconds,Qty,LogicalReads,PhysicalReads,Writes,ExeDate),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,index:f869ba17-8818-56c3-8b70-c74095a6801d,key:QueryHash,negate:!f,params:(query:'{0}'),type:phrase),query:(match_phrase:(QueryHash:'{0}')))))";
			var capabilityJobsList = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule.Capability, (1, new[] { alertRule.Path }, Array.Empty<ZString>(), glbCapabilityId) }
			};
			var capabilityJobsList1 = new Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)>
			{
				{ alertRule1.Capability, (1, new[] { alertRule1.Path }, new ZString[1], glbCapabilityId1) }
			};

			using (var dbConnection = Db.Connection)
			using (EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPlanUri))
			{
				var sqlCpuUsageAlertProvider = new SqlCpuUsageAlertProvider(mocklogger, dbConnection, alertDefaults);
				Assert(sqlCpuUsageAlertProvider != null);

				var (result, alertTarget, descriptivePath, task) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert, alertRule, boFactory, glbCapabilityId, capabilityJobsList);
				AssertEquals(AddAlertTargetResult.TargetAdded, result);
				Assert(alertTarget is WorkItem);
				var workItem = (WorkItem)alertTarget;
				var newTask = workItem.WorkflowItems.AddNew();
				newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				newTask.P9_GS_NKAssignedStaffMember = "E";
				Assert(workItem.WKI_Summary.EndsWith("1234567"));
				var workItemDetails = workItem.WKI_Details.ToUTF8();
				Assert(workItemDetails.Contains("1234567"));
				Assert(workItemDetails.Contains("WtgQueryHash"));
				Assert(!workItemDetails.Replace("WtgQueryHash", "").Contains("QueryHash"));
				Assert(workItemDetails.Contains("WTG Query Hash"));
				Assert(!workItemDetails.Replace("WTG Query Hash", "").Contains("Query Hash"));
				Assert(workItemDetails.Contains("Affected Client"));

				boFactory.Save();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

				var (result1, alertTarget1, descriptivePath1, task1) = sqlCpuUsageAlertProvider.FindAlertTargetCreateIfNotExists(cpuUsageAlert1, alertRule1, boFactory, glbCapabilityId1, capabilityJobsList1);
				AssertEquals(AddAlertTargetResult.TargetAdded, result1);
				var workItem1 = (WorkItem)alertTarget1;
				Assert(workItem1.WKI_Summary.EndsWith("12345678"));
				var workItemDetails1 = workItem1.WKI_Details.ToUTF8();
				Assert(workItemDetails1.Contains("Affected Client"));
			}
		}

		static (SqlCpuUsageAlert cpuUsageAlert, AlertRule alertRule, Guid glbCapabilityId) MockAlertAndCapabilityData(string path, string affectedClient = "ALL")
		{
			var cpuUsageAlert = new SqlCpuUsageAlert();
			cpuUsageAlert.AlertName = "Customs";
			cpuUsageAlert.Path = path;
			cpuUsageAlert.Owner = "CW1";
			cpuUsageAlert.AffectedClient = affectedClient;
			var alertRule = new AlertRule();
			alertRule.AlertName = "EXPENSIVE_SQL";
			alertRule.Path = "Customs";
			alertRule.Capability = "TLR";
			var glbCapabilityId = Guid.Parse("dfe18b83-52c1-48b3-9c62-128b17bf9281");
			return (cpuUsageAlert, alertRule, glbCapabilityId);
		}

		class SqlExecutionPlanRetrieverForTest(string result) : ISqlExecutionPlanRetriever
		{
			public Task<string> RetrieveExecutionPlan(string searchLink)
			{
				return Task.FromResult(result);
			}
		}
	}
}
