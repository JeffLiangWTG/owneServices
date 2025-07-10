using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentGroupAutoCascadeWorkItemCollection))]
	public class IncidentGroupAutoCascadeWorkItemCollectionTest : ProcessManagement.Business.Test.WorkTaskRelatedItemCollectionTestCase<IncidentManagementGroup>
	{
		public override void TestShouldAddToCollection()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link = Factory.NewWithValidTestData<IncidentManagementLink>();

			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();

			Factory.Save();
			AssertEquals(0, group.AutoCascadeRelatedItems.Count);

			group.AutoCascadeRelatedItems.Add(workItem1);
			group.AutoCascadeRelatedItems.Add(workItem2);
			group.AutoCascadeRelatedItems.Add(workItem3);
			group.AutoCascadeRelatedItems.Add(incident1);

			AssertEquals("Should contain only Work Item", 3, group.AutoCascadeRelatedItems.Count);
			Assert("Should have workItem1", group.AutoCascadeRelatedItems.Contains(workItem1));
			Assert("Should have workItem2", group.AutoCascadeRelatedItems.Contains(workItem2));
			Assert("Should have workItem3", group.AutoCascadeRelatedItems.Contains(workItem3));
			Assert("Should not have incident1", !group.AutoCascadeRelatedItems.Contains(incident1));
		}

		public void TestPivot()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem3 = Factory.NewWithValidTestData<NewWorkItem>();

			var cascadeCollection = new IncidentGroupAutoCascadeWorkItemCollection(group)
			{
				workItem1,
				workItem2
			};
			group.RelatedItems.Add(workItem3);
			Factory.Save();

			AssertEquals("Precondition", 2, cascadeCollection.Count);
			Assert("Precondition", cascadeCollection.Contains(workItem1));
			Assert("Precondition", cascadeCollection.Contains(workItem2));
			AssertEquals("Precondition", 3, group.RelatedItems.Count);
			Assert("Precondition", group.RelatedItems.Contains(workItem1));
			Assert("Precondition", group.RelatedItems.Contains(workItem2));
			Assert("Precondition", group.RelatedItems.Contains(workItem3));

			var newFactory = new BusinessObjectFactory();
			var groupReloaded = newFactory.Load<IncidentManagementGroup>(group.PK);
			var cascadeCollectionReloaded = new IncidentGroupAutoCascadeWorkItemCollection(groupReloaded);
			AssertEquals("Should only load work items added via cascade collection", 2, cascadeCollection.Count);
			Assert("Should only load work items added via cascade collection", cascadeCollection.Contains(workItem1));
			Assert("Should only load work items added via cascade collection", cascadeCollection.Contains(workItem2));

			var groupPivots = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, group.PK));
			var wkcCount = groupPivots.Count(c => c.XX_RelationType == Core.Constants.GenPivotTypes.WorkItemCascade);
			var wrkCount = groupPivots.Count(c => c.XX_RelationType == Core.Constants.GenPivotTypes.ProcessManagement);
			AssertEquals("The count of WKC should be 2", 2, wkcCount);
			AssertEquals("The count of WRK should be 1", 1, wrkCount);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(IncidentGroupAutoCascadeWorkItemCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentGroupAutoCascadeWorkItemCollection(Factory.NewWithValidTestData<IncidentManagementGroup>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<NewWorkItem>();
		}
	}
}
