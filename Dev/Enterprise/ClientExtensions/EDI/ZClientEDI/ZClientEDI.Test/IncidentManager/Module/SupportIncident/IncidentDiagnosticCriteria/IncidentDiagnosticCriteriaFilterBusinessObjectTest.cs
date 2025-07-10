using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentDiagnosticCriteriaFilterBusinessObject))]
	public class IncidentDiagnosticCriteriaFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IncidentDiagnosticCriteriaFilterBusinessObject();
		}

		public void TestTypeFilter()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			Factory.Save();
			var incidentDiagnosticCriteriaFilter = new IncidentDiagnosticCriteriaFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentDiagnosticCriteriaFilter["Type"];
			var incidentDiagnosticCriteria = new IncidentDiagnosticCriteriaCollection(Factory);

			filter.Property = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only one Diagnostic Criteria", incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));

			filter.Property = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should not contain Diagnostic Criteria", !incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));
		}

		public void TestDescriptionFilter()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria.IMD_Description = "Test1";
			Factory.Save();
			var incidentDiagnosticCriteriaFilter = new IncidentDiagnosticCriteriaFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentDiagnosticCriteriaFilter["Description"];
			var incidentDiagnosticCriteria = new IncidentDiagnosticCriteriaCollection(Factory);

			filter.Property = "Test1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only one Diagnostic Criteria", incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));

			filter.Property = "Test2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should not contain Diagnostic Criteria", !incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));
		}

		public void TestKeywordsFilter()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria.IMD_Keywords = "Test1";
			Factory.Save();
			var incidentDiagnosticCriteriaFilter = new IncidentDiagnosticCriteriaFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentDiagnosticCriteriaFilter["Keywords"];
			var incidentDiagnosticCriteria = new IncidentDiagnosticCriteriaCollection(Factory);

			filter.Property = "Test1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only one Diagnostic Criteria", incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));

			filter.Property = "Test2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should not contain Diagnostic Criteria", !incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));
		}

		public void TestQuestionFilter()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria.IMD_Question = "Test1";
			Factory.Save();
			var incidentDiagnosticCriteriaFilter = new IncidentDiagnosticCriteriaFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentDiagnosticCriteriaFilter["Question"];
			var incidentDiagnosticCriteria = new IncidentDiagnosticCriteriaCollection(Factory);

			filter.Property = "Test1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only one Diagnostic Criteria", incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));

			filter.Property = "Test2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should not contain Diagnostic Criteria", !incidentDiagnosticCriteria.Contains(diagnosticCriteria.PK));
		}

		public void TestIncidentTriageFilter()
		{
			var diagnosticCriteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria1.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria1.IMD_Question = "Test1";
			var diagnosticCriteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria2.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			diagnosticCriteria2.IMD_Question = "Test2";
			var diagnosticCriteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria3.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			diagnosticCriteria3.IMD_Question = "Test3";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Product = ProductTypes.Codes.Enterprise;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_Product = ProductTypes.Codes.CargoWiseOne;
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			triage3.IMT_Product = ProductTypes.Codes.GLOW;

			var pivot1 = triage1.DiagnosticCriteriaPivots.AddNew();
			pivot1.IMO_IMD_DiagnosticCriteria = diagnosticCriteria1.PK;
			var pivot2 = triage2.DiagnosticCriteriaPivots.AddNew();
			pivot2.IMO_IMD_DiagnosticCriteria = diagnosticCriteria2.PK;
			var pivot3 = triage3.DiagnosticCriteriaPivots.AddNew();
			pivot3.IMO_IMD_DiagnosticCriteria = diagnosticCriteria3.PK;

			Factory.Save();

			var incidentDiagnosticCriteriaFilter = new IncidentDiagnosticCriteriaFilterBusinessObject();
			var filter = (ModuleGuidForeignCollectionFilter)incidentDiagnosticCriteriaFilter["IncidentTriage"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbProductFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Product");
			dbProductFilter.IsActive = true;
			dbProductFilter.Property = ProductTypes.Codes.Enterprise;

			var incidentDiagnosticCriteria = new IncidentDiagnosticCriteriaCollection(Factory);
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should only contain diagnostic criteria 1", incidentDiagnosticCriteria.Contains(diagnosticCriteria1.PK));
			Assert("Should not contain diagnostic criteria 2", !incidentDiagnosticCriteria.Contains(diagnosticCriteria2.PK));
			Assert("Should not contain diagnostic criteria 3", !incidentDiagnosticCriteria.Contains(diagnosticCriteria3.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbProductFilter.Property = ProductTypes.Codes.CargoWiseOne;
			incidentDiagnosticCriteria.Load(incidentDiagnosticCriteriaFilter.Filter);

			Assert("Should contain diagnostic criteria 1", incidentDiagnosticCriteria.Contains(diagnosticCriteria1.PK));
			Assert("Should not contain diagnostic criteria 2", !incidentDiagnosticCriteria.Contains(diagnosticCriteria2.PK));
			Assert("Should contain diagnostic criteria 3", incidentDiagnosticCriteria.Contains(diagnosticCriteria3.PK));
		}
	}
}
