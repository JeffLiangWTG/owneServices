using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.AmnestyReporter.Business
{
	class AmnestyExceptionTest : TestCaseWithFactory
	{
		public void TestWrongConstructorCall()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: factory", () => new AmnestyException(null, new DatAmnestyFailure(), ZGuid.Empty));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: datAmnestyFailure", () => new AmnestyException(new BusinessObjectFactory(), null, ZGuid.Empty));
		}

		[TestDate(2015, 10, 04)]
		public void TestProcessContent()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode, subType4: NewWorkItemLookups.WorkItemTypeConstants.AmnestyFix);
			MasterFilesTestHelper.CreateTask(template);
			Factory.Save();
			var data = GetTestData();
			var pk = new AmnestyException(Factory, data[0], ZGuid.Empty).Process();
			Factory.Save();
			var workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			var workItem = workItems[0];
			AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
			AssertEquals(nameof(workItem.PK), pk.ToString(), workItem.PK.ToString());
			AssertEquals(nameof(workItem.WKI_WorkItemType), "ENT", workItem.WKI_WorkItemType);
			AssertEquals(nameof(workItem.WKI_WorkItemArea), "PER", workItem.WKI_WorkItemArea);
			AssertEquals(nameof(workItem.WKI_ActivityType), "APP", workItem.WKI_ActivityType);
			AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
			AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
			AssertEquals(nameof(workItem.WKI_SystemCreateUser), "E", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
			AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "E", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
			AssertEquals(nameof(workItem.WKI_SystemCreateTimeUtc), ZDateTime.UtcNow, workItem.WKI_SystemCreateTimeUtc);
			AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), ZDateTime.UtcNow, workItem.WKI_SystemLastEditTimeUtc);
			AssertEquals(nameof(workItem.WKI_Status), ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);
			AssertEquals(nameof(workItem.WKI_Details), "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n", workItem.WKI_Details.ToUTF8());
		}

		public void TestProcessNoProduct()
		{
			var data = GetTestData();
			data.ForEach(failure =>
			{
				// Make broken data
				failure.ST_Product = "";
				var pk = new AmnestyException(Factory, failure, ZGuid.Empty).Process();
				var workItems = Factory.Load<WorkItem>(new ZQuery());
				AssertEquals("Should not be any work items", 0, workItems.Length);
				AssertEquals("Should be empty Guid", ZGuid.Empty.ToString(), pk.ToString());
			});
		}

		[TestDate(2015, 10, 04, 0, 28, 0, 287)]
		public void TestProcessAddNewErrorToExistingWorkItem()
		{
			var data = GetTestData();
			var pk1 = new AmnestyException(Factory, data[0], ZGuid.Empty).Process();
			var workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			var pk2 = new AmnestyException(Factory, data[0], pk1).Process();
			workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			var pk3 = new AmnestyException(Factory, data[1], pk2).Process();
			workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			AssertEquals("Work item should be the same", pk1, pk2);
			AssertEquals("Work item should be the same", pk2, pk3);
			var workItem = workItems[0];
			AssertEquals(nameof(workItem.WKI_Details), "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n" + "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n" + "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI1\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n", workItem.WKI_Details.ToUTF8());
		}

		[TestDate(2015, 10, 04, 0, 28, 0, 287)]
		public void TestProcessUseHint()
		{
			var data = GetTestData();
			var pk1 = new AmnestyException(Factory, data[0], ZGuid.Empty).Process();
			var workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			var pk2 = new AmnestyException(Factory, data[0], pk1).Process();
			workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 1, workItems.Length);
			var pk3 = new AmnestyException(Factory, data[0], ZGuid.Empty).Process();
			workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertEquals("Number of WorkItems", 2, workItems.Length);
			var sortedByLength = workItems.OrderByDescending(w => w.WKI_Details.Length).ToArray();
			AssertEquals("Work item should be the same", pk1, pk2);
			AssertNotEquals("Work item should be the different", pk2, pk3);
			var workItem1 = sortedByLength[0];
			var workItem2 = sortedByLength[1];
			AssertEquals("Work item should be the same", workItem1.PK, pk1);
			AssertEquals("Work item should be the same", workItem2.PK, pk3);
			AssertEquals(nameof(workItem1.WKI_Details), "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n" + "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n", workItem1.WKI_Details.ToUTF8());
			AssertEquals(nameof(workItem2.WKI_Details), "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n", workItem2.WKI_Details.ToUTF8());
		}

		static DatAmnestyFailure[] GetTestData()
		{
			return new[] { new DatAmnestyFailure { AF_PK = Guid.Parse("D0884E31-834C-4552-8DF6-1290C8F13526"), AF_StartDate = new DateTime(2015, 10, 4, 0, 28, 0, 287, DateTimeKind.Local), E6_PK = Guid.Parse("496CA45F-76FD-4669-8CF3-FED342EBE114"), E6_MethodName = "TestPacking_GUI", E2_TestClass = "AnalyzersRunner.RunAnalyzers.Runner", E8_AssemblyName = "AnalyzersRunner.RunAnalyzers", ST_ResponsibleUser = "Bret.Ehlert", ST_Product = "ENT", ST_ProductArea = "PER", ST_Module = "APP" }, new DatAmnestyFailure { AF_PK = Guid.Parse("D0884E31-834C-4552-8DF6-1290C8F13527"), AF_StartDate = new DateTime(2015, 10, 4, 0, 28, 0, 287, DateTimeKind.Local), E6_PK = Guid.Parse("496CA45F-76FD-4669-8CF3-FED342EBE114"), E6_MethodName = "TestPacking_GUI1", E2_TestClass = "AnalyzersRunner.RunAnalyzers.Runner", E8_AssemblyName = "AnalyzersRunner.RunAnalyzers", ST_ResponsibleUser = "Bret.Ehlert", ST_Product = "ENT", ST_ProductArea = "PER", ST_Module = "APP" } };
		}
	}
}
