using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(InvestigationItemFilterBusinessObject))]
	public class InvestigationItemControllerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvestigationItemFilterBusinessObject();
		}

		public void TestTypeFilter()
		{
			var investigationItem = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			Factory.Save();
			var investigationItemFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleTextFilter)investigationItemFilter["Type"];
			var investigationItemLoad = new InvestigationItemCollection(Factory);

			filter.Property = InvestigationItemTypes.Codes.MaterialRequest;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should only one Investigation Item", investigationItemLoad.Contains(investigationItem.PK));

			filter.Property = InvestigationItemTypes.Codes.Question;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should not contain Investigation Item", !investigationItemLoad.Contains(investigationItem.PK));
		}

		public void TestDescriptionFilter()
		{
			var investigationItem = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			investigationItem.INV_Description = "Test1";
			Factory.Save();

			var investigationItemFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleTextFilter)investigationItemFilter["Description"];
			var investigationItemLoad = new InvestigationItemCollection(Factory);

			filter.Property = "Test1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should only one Investigation Item", investigationItemLoad.Contains(investigationItem.PK));

			filter.Property = "Test2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should not contain Investigation Item", !investigationItemLoad.Contains(investigationItem.PK));
		}

		public void TestItemTextFilter()
		{
			var investigationItem = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			investigationItem.INV_ItemText = "Test1";
			Factory.Save();

			var investigationItemFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleTextFilter)investigationItemFilter["Item Text"];
			var investigationItemLoad = new InvestigationItemCollection(Factory);

			filter.Property = "Test1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should only one Investigation Item", investigationItemLoad.Contains(investigationItem.PK));

			filter.Property = "Test2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should not contain Investigation Item", !investigationItemLoad.Contains(investigationItem.PK));
		}

		public void TestAskTheClientFilter()
		{
			var investigationItem1 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem1.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			investigationItem1.INV_AskClient = true;
			var investigationItem2 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem2.INV_Type = InvestigationItemTypes.Codes.Question;
			investigationItem2.INV_AskClient = false;
			Factory.Save();

			var investigationItemFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleFlagsFilter)investigationItemFilter["Ask the Client"];
			var investigationItemLoad = new InvestigationItemCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should contain Investigation Item1", investigationItemLoad.Contains(investigationItem1.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should contain Investigation Item2", investigationItemLoad.Contains(investigationItem2.PK));
		}

		public void TestIncidentDiagnosticCriteriaFilter()
		{
			var investigationItem1 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem1.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			investigationItem1.INV_ItemText = "investigationItem1";
			var investigationItem2 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem2.INV_Type = InvestigationItemTypes.Codes.Question;
			investigationItem2.INV_ItemText = "investigationItem2";
			var investigationItem3 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem3.INV_Type = InvestigationItemTypes.Codes.Question;
			investigationItem3.INV_ItemText = "investigationItem3";

			var diagnosticCriteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria1.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria1.IMD_Question = "Test1";
			var diagnosticCriteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria2.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			diagnosticCriteria2.IMD_Question = "Test2";
			var diagnosticCriteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria3.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria3.IMD_Question = "Test3";

			var incidentDiagnosticCriteriaLink1 = Factory.NewWithValidTestData<DiagnosticCriteriaInvestigationItemLink>();
			incidentDiagnosticCriteriaLink1.DIL_IMD_DiagnosticCriteria = diagnosticCriteria1.PK;
			incidentDiagnosticCriteriaLink1.DIL_INV_InvestigationItem = investigationItem1.PK;

			var pivot1 = diagnosticCriteria1.InvestigationItemPivots.AddNew();
			pivot1.DIL_INV_InvestigationItem = investigationItem1.PK;
			var pivot2 = diagnosticCriteria2.InvestigationItemPivots.AddNew();
			pivot2.DIL_INV_InvestigationItem = investigationItem2.PK;
			var pivot3 = diagnosticCriteria3.InvestigationItemPivots.AddNew();
			pivot3.DIL_INV_InvestigationItem = investigationItem3.PK;

			Factory.Save();

			var incidentDiagnosticCriteriaFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleGuidForeignCollectionFilter)incidentDiagnosticCriteriaFilter["DiagnosticCriteria"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbProductFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Question");
			dbProductFilter.IsActive = true;
			dbProductFilter.Property = "Test1";

			var investigationItemCollection = new InvestigationItemCollection(Factory);
			investigationItemCollection.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only contain diagnostic investigationItem 1", investigationItemCollection.Contains(investigationItem1.PK));
			Assert("Should not contain diagnostic investigationItem 2", !investigationItemCollection.Contains(investigationItem2.PK));
			Assert("Should not contain diagnostic investigationItem 3", !investigationItemCollection.Contains(investigationItem3.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbProductFilter.Property = "Test2";
			investigationItemCollection.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should contain diagnostic investigationItem 1", investigationItemCollection.Contains(investigationItem1.PK));
			Assert("Should not contain diagnostic investigationItem 2", !investigationItemCollection.Contains(investigationItem2.PK));
			Assert("Should contain diagnostic investigationItem 3", investigationItemCollection.Contains(investigationItem3.PK));
		}

		public void TestItemNumberFilter()
		{
			var investigationItem1 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem1.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			investigationItem1.INV_AskClient = true;
			var investigationItem2 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem2.INV_Type = InvestigationItemTypes.Codes.Question;
			investigationItem2.INV_AskClient = false;
			Factory.Save();

			var investigationItemFilter = new InvestigationItemFilterBusinessObject();
			var filter = (ModuleTextFilter)investigationItemFilter["Investigation Item ID"];
			var investigationItemLoad = new InvestigationItemCollection(Factory);

			filter.Property = investigationItem2.INV_ItemNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should not contain investigationItem1", !investigationItemLoad.Contains(investigationItem1.PK));
			Assert("Should only contain investigationItem2", investigationItemLoad.Contains(investigationItem2.PK));

			filter.Property = investigationItem2.INV_ItemNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			investigationItemLoad.Load(investigationItemFilter.Filter);

			Assert("Should only contain investigationItem1", investigationItemLoad.Contains(investigationItem1.PK));
			Assert("Should not contain investigationItem2", !investigationItemLoad.Contains(investigationItem2.PK));
		}
	}
}
