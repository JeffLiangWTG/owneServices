using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentTriageChecklistItemFilterBusinessObject))]
	public class IncidentTriageChecklistItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IncidentTriageChecklistItemFilterBusinessObject();
		}

		(IncidentTriageChecklistItem, IncidentTriageChecklistItem, IncidentTriageChecklistItem) CreateIncidentTriageChecklistItemsForTest()
		{
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem3 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem1.IMC_IsPublished = true;
			checklistItem1.IMC_SupportDescription = "YUI";
			checklistItem1.IMC_Category = "CAC";
			checklistItem1.IMC_ResponseType = "DYB";

			checklistItem2.IMC_IsPublished = false;
			checklistItem2.IMC_Category = "CAS";
			checklistItem2.IMC_SupportDescription = "UUU";
			checklistItem2.IMC_ResponseType = "TTT";

			checklistItem3.IMC_SupportDescription = "PLK";
			checklistItem3.IMC_IsPublished = false;
			checklistItem3.IMC_Category = "DID";
			checklistItem3.IMC_ResponseType = "SGH";
			Factory.Save();

			return (checklistItem1, checklistItem2, checklistItem3);
		}

		public void TestResponseTypeFilter()
		{
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemsForTest();

			var incidentTriageChecklistItemFilter = new IncidentTriageChecklistItemFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageChecklistItemFilter["Response Type"];
			var incidentTriageChecklistItem = new IncidentTriageChecklistItemCollection(Factory);

			filter.Property = "DYB";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should only contain triage1", incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should not contain triage2", !incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should not contain triage3", !incidentTriageChecklistItem.Contains(checklistItem3.PK));

			filter.Property = "DYB";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should not contain triage1", !incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should contain triage2", incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should contain triage3", incidentTriageChecklistItem.Contains(checklistItem3.PK));
		}

		public void TestChecklistDescriptionFilter()
		{
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemsForTest();

			var incidentTriageChecklistItemFilter = new IncidentTriageChecklistItemFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageChecklistItemFilter["Checklist Description"];
			var incidentTriageChecklistItem = new IncidentTriageChecklistItemCollection(Factory);

			filter.Property = "YUI";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should only contain triage1", incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should not contain triage2", !incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should not contain triage3", !incidentTriageChecklistItem.Contains(checklistItem3.PK));

			filter.Property = "YUI";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should not contain triage1", !incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should contain triage2", incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should contain triage3", incidentTriageChecklistItem.Contains(checklistItem3.PK));
		}

		public void TestChecklistCategoryFilter()
		{
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemsForTest();

			var incidentTriageChecklistItemFilter = new IncidentTriageChecklistItemFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageChecklistItemFilter["Checklist Category"];
			var incidentTriageChecklistItem = new IncidentTriageChecklistItemCollection(Factory);

			filter.Property = "CAC";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should only contain triage1", incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should not contain triage2", !incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should not contain triage3", !incidentTriageChecklistItem.Contains(checklistItem3.PK));

			filter.Property = "CAC";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should not contain triage1", !incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should contain triage2", incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should contain triage3", incidentTriageChecklistItem.Contains(checklistItem3.PK));
		}

		public void TestIsChecklistPublishedFilter()
		{
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemsForTest();

			var incidentTriageChecklistItemFilter = new IncidentTriageChecklistItemFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentTriageChecklistItemFilter["Is Checklist Published"];
			var incidentTriageChecklistItem = new IncidentTriageChecklistItemCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should only contain triage1", incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should not contain triage2", !incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should not contain triage3", !incidentTriageChecklistItem.Contains(checklistItem3.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should not contain triage1", !incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should contain triage2", incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should contain triage3", incidentTriageChecklistItem.Contains(checklistItem3.PK));
		}

		public void TestChecklistNumberFilter()
		{
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemsForTest();

			var incidentTriageChecklistItemFilter = new IncidentTriageChecklistItemFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageChecklistItemFilter["Checklist Number"];
			var incidentTriageChecklistItem = new IncidentTriageChecklistItemCollection(Factory);

			filter.Property = checklistItem1.IMC_ChecklistNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should only contain triage1", incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should not contain triage2", !incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should not contain triage3", !incidentTriageChecklistItem.Contains(checklistItem3.PK));

			filter.Property = checklistItem1.IMC_ChecklistNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriageChecklistItem.Load(incidentTriageChecklistItemFilter.Filter);

			Assert("Should not contain triage1", !incidentTriageChecklistItem.Contains(checklistItem1.PK));
			Assert("Should contain triage2", incidentTriageChecklistItem.Contains(checklistItem2.PK));
			Assert("Should contain triage3", incidentTriageChecklistItem.Contains(checklistItem3.PK));
		}
	}
}
