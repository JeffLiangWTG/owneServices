using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	public class EDIWorkItemControllerTest : TestCaseWithFactory
	{
		public void TestDefaultsFromLastAddedWorkItem()
		{
			var someStaff = Factory.New<GlbStaff>();
			someStaff.GS_Code = "NEW";
			var controller = new EDIWorkItemController();
			using (var form = (ZForm)controller.ShowNewForm())
			{
				var item1 = (NewWorkItem)form.BusinessEntity;
				AssertEquals("", item1.WKI_WorkItemType);
				AssertEquals("", item1.WKI_WorkItemArea);
				AssertEquals("", item1.WKI_ActivityType);
				item1.WKI_SystemCreateUser = someStaff.GS_Code;
				item1.WKI_WorkItemType = "RRR";
				item1.WKI_WorkItemArea = "ZZZ";
				item1.WKI_ActivityType = "ACT";
				controller.Factory.Save();
			}

			using (var form = (ZForm)controller.ShowNewForm())
			{
				var item2 = (NewWorkItem)form.BusinessEntity;
				AssertEquals("", item2.WKI_WorkItemType);
				AssertEquals("", item2.WKI_WorkItemArea);
				AssertEquals("", item2.WKI_ActivityType);
				item2.WKI_WorkItemType = "LLL";
				item2.WKI_WorkItemArea = "RLZ";
				item2.WKI_ActivityType = "AC2";
				controller.Factory.Save();
			}

			using (var form = (ZForm)controller.ShowNewForm())
			{
				var item3 = (NewWorkItem)form.BusinessEntity;
				AssertEquals("LLL", item3.WKI_WorkItemType);
				AssertEquals("RLZ", item3.WKI_WorkItemArea);
				AssertEquals("AC2", item3.WKI_ActivityType);
			}
		}

		public void TestGetNewAndPopulateFromSource()
		{
			var factory = new BusinessObjectFactory();
			var workItemSource = factory.NewWithValidTestData<WorkItemSourceForTest>();
			workItemSource.PopulateWorkItemAction = w =>
			{
				w.WKI_WorkItemType = "AAA";
				w.WKI_WorkItemArea = "BBB";
				w.WKI_ActivityType = "CCC";
			};

			var bizo = new EDIWorkItemController().GetNewAndPopulateFromSource(workItemSource) as NewWorkItem;

			AssertNotNull(bizo);
			AssertEquals("AAA", bizo.WKI_WorkItemType);
			AssertEquals("BBB", bizo.WKI_WorkItemArea);
			AssertEquals("CCC", bizo.WKI_ActivityType);
			AssertSame(factory, bizo.Factory);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new EDIWorkItemController();
			AssertEquals(typeof(NewWorkItem), controller.TypeOfTopLevelBusinessObject);
		}

		#region Implementations

		class WorkItemSourceForTest : DummyBusinessObject, IWorkItemSource
		{
			public WorkItemSourceForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Action<NewWorkItem> PopulateWorkItemAction;

			public void PopulateWorkItem(NewWorkItem workItem) => PopulateWorkItemAction?.Invoke(workItem);
		}

		#endregion
	}
}
