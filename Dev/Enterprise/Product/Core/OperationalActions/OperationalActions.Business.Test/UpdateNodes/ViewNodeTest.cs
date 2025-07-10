using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class ViewNodeTest : BaseUpdateNodeTest
	{
		public void TestCollectionViewsForConsol()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consol = Factory.New<IForwardingConsol>();
			consol.AddShipment(shipment);
			var consolBizo = consol as BusinessObject;
			var shipmentBizo = shipment as BusinessObject;
			IOperationalActionFieldValuePair[] fieldValuePairs = { new DummyOperationalActionFieldValuePair(Field("GridShipments.", JobShipmentSchema.JS_GoodsDescription, "Text Field"), new ZString("New Description")), };
			var root = new RootUpdateNode(consol.GetType());
			root.AddRange(fieldValuePairs);
			var affectedTargets = root.Apply(new BusinessObject[] { consolBizo });
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { consolBizo, shipmentBizo }, affectedTargets);
			AssertEquals("New Description", shipment.JS_GoodsDescription);
		}

		public void TestCollectionViews()
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var milestone = bizo.WorkflowItems.Milestones.AddNew();
			var task = bizo.WorkflowItems.Tasks.AddNew();
			IOperationalActionFieldValuePair[] fieldValuePairs = { new DummyOperationalActionFieldValuePair(Field("WorkflowItems.Milestones.", ProcessTasksSchema.P9_Description, "Text Field"), new ZString("New Description")), new DummyOperationalActionFieldValuePair(Field("WorkflowItems.Triggers.", ProcessTasksSchema.P9_Description, "Text Field"), new ZString("New Description")), new DummyOperationalActionFieldValuePair(Field("WorkflowItems.Tasks.", ProcessTasksSchema.P9_Description, "Text Field"), new ZString("New Description")), };
			RootUpdateNode root = new RootUpdateNode(typeof(DummyWithWorkflow));
			root.AddRange(fieldValuePairs);
			var affectedTargets = root.Apply(new BusinessObject[] { bizo });
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { bizo, trigger, milestone, task }, affectedTargets);
			AssertEquals("New Description", trigger.P9_Description);
			AssertEquals("New Description", milestone.P9_Description);
			AssertEquals("New Description", task.P9_Description);
		}

		public void TestViewTrumpsCollection()
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var milestone = bizo.WorkflowItems.Milestones.AddNew();
			var task = bizo.WorkflowItems.Tasks.AddNew();
			IOperationalActionFieldValuePair[] fieldValuePairs = {
				new DummyOperationalActionFieldValuePair(Field("WorkflowItems.Milestones.", ProcessTasksSchema.P9_Description, "Text Field"), new ZString("New Description")),
				new DummyOperationalActionFieldValuePair(Field("WorkflowItems.", ProcessTasksSchema.P9_Description, "Text Field"), new ZString("Something Else")), };

			RootUpdateNode root = new RootUpdateNode(typeof(DummyWithWorkflow));
			root.AddRange(fieldValuePairs);
			var affectedTargets = root.Apply(new BusinessObject[] { bizo });
			// If a view has been specified the overall collection should be ignored
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { bizo, milestone }, affectedTargets);
			AssertEquals("New Description", milestone.P9_Description);
			AssertEquals("", trigger.P9_Description);
			AssertEquals("", task.P9_Description);
		}

		[ExpectNoExceptions]
		public void TestViewForPropertyCollection()
		{
			var mock = new Mock<IProcessJobHeaderProvider>();
			mock.Setup(p => p.GetWorkflowsForParent(It.IsAny<IWorkflowProviderCore>(), It.IsAny<BusinessObjectFactory>())).Returns(new HeaderCollectionForTest(Factory));

			var bizo = Factory.New<DummyWithWorkflow>();

			var taskView = bizo.WorkflowItems.Tasks;
			var task = taskView.AddNew();

			using (ObjectFactory.Substitute(mock.Object))
			{
				var workflows = taskView.Workflows;

				var header = workflows.AddNew();

				IOperationalActionFieldValuePair[] fieldValuePairs = { new DummyOperationalActionFieldValuePair(Field("WorkflowItems.Tasks.Workflows.", ProcessHeaderSchema.FH_DoNotStartBeforeDate, "Text Field"), new ZDateTime(2024, 1, 1)) };

				var root = new RootUpdateNode(typeof(DummyWithWorkflow));
				root.AddRange(fieldValuePairs);
				var affectedTargets = root.Apply(new BusinessObject[] { bizo });

				Assert(workflows.Count > 0);
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { bizo, header as BusinessObject }, affectedTargets);
				AssertEquals(workflows[0].FH_DoNotStartBeforeDate, new ZDateTime(2024, 1, 1));
			}
		}

		class HeaderCollectionForTest : BusinessObjectCollection<BusinessObject>, IProcessHeaderCollection
		{
			public HeaderCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public HeaderCollectionForTest(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
			{
			}

			IProcessHeader IProcessHeaderCollection.this[int index] => this[index] as IProcessHeader;

			public IDictionary<IProcessHeader, IProcessHeader> CloneWorkflowsAndLinksForTemplates(IProcessJobHeader targetJobHeader, IProcessHeaderCollection targetCollection)
			{
				throw new System.NotImplementedException();
			}

			public void Delete(IBusiness bizo)
			{
				throw new System.NotImplementedException();
			}

			public void DeleteAll()
			{
				throw new System.NotImplementedException();
			}

			IProcessHeader IProcessHeaderCollection.AddNew()
			{
				var header = Factory.New<IProcessHeader>();
				Add(header as BusinessObject);
				return header;
			}
		}
	}
}
