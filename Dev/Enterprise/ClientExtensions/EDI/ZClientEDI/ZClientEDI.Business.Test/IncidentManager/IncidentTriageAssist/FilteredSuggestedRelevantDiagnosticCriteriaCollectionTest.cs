using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.IncidentManager.IncidentTriageAssist
{
	[TestedType(typeof(FilteredSuggestedRelevantDiagnosticCriteriaCollection))]
	public class FilteredSuggestedRelevantDiagnosticCriteriaCollectionTest : BusinessObjectCollectionViewTestCase<FilteredSuggestedRelevantDiagnosticCriteriaCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			AssertEquals(0, Assist.SuggestedCriteriaCollection.Count);
			AssertEquals(0, Assist.FilteredSuggestedCriteriaCollection.Count);

			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;

			criteria1.IMD_Description = "des1_1 des1_2";
			criteria1.IMD_Keywords = "des1_3 des1_4 apple";
			criteria2.IMD_Description = "des2_1 des2_2 apple";
			criteria2.IMD_Keywords = "des2_3 des2_4";
			criteria3.IMD_Description = "des3_1 des3_2";
			criteria3.IMD_Keywords = "des3_3 des3_4 apple";
			Factory.Save();

			Assist.Product = "";
			Assist.SearchTerm = "apple \"des2_1 des2_2\"";
			Assist.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom;
			Assist.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAll;
			Assist.ShouldSearchDescription = true;
			Assist.ShouldSearchKeywords = false;
			Assist.ShouldSearchSuggestedList = true;
			Assist.SuggestedCriteriaCollection.RemoveAll();
			Assist.SuggestedCriteriaCollection.AddRange(Assist.LoadRelevantCriteria(new ZQuery()));
			AssertEquals(3, Assist.SuggestedCriteriaCollection.Count);

			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(1, Assist.FilteredSuggestedCriteriaCollection.Count);
			AssertEquals(criteria2.PK, Assist.FilteredSuggestedCriteriaCollection[0].DiagnosticCriteria.PK);

			Assist.SearchTerm = "apple \"des2_1 des2_2\"";
			Assist.CriteriaTypeFilter = TriageAssistBusinessObject.ComparisonConstants.Any;
			Assist.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAny;
			Assist.ShouldSearchDescription = true;
			Assist.ShouldSearchKeywords = true;
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(3, Assist.FilteredSuggestedCriteriaCollection.Count);

			Assist.SearchTerm = "\"3 des1_4 ap\"";
			Assist.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.DiagnosticFactor;
			Assist.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.Exact;
			Assist.ShouldSearchDescription = false;
			Assist.ShouldSearchKeywords = true;
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(1, Assist.FilteredSuggestedCriteriaCollection.Count);
			AssertEquals(criteria1.PK, Assist.FilteredSuggestedCriteriaCollection[0].DiagnosticCriteria.PK);

			Assist.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom;
			Assist.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.Any;
			Assist.ShouldSearchDescription = true;
			Assist.ShouldSearchKeywords = true;
			Assist.SearchTerm = "des3_3 des"; //starts with
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { criteria1.PK, criteria2.PK, criteria3.PK }, Assist.FilteredSuggestedCriteriaCollection.GetPKs());
			Assist.SearchTerm = "des3_3 des "; //whole word
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { criteria3.PK }, Assist.FilteredSuggestedCriteriaCollection.GetPKs());
			Assist.SearchTerm = "des"; //starts with
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { criteria1.PK, criteria2.PK, criteria3.PK }, Assist.FilteredSuggestedCriteriaCollection.GetPKs());
			Assist.SearchTerm = "des "; //whole word
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(0, Assist.FilteredSuggestedCriteriaCollection.Count);

			Assist.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAll;
			Assist.SearchTerm = "des3_3 des"; //starts with
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { criteria3.PK }, Assist.FilteredSuggestedCriteriaCollection.GetPKs());
			Assist.SearchTerm = "des3_3 des "; //whole word
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(0, Assist.FilteredSuggestedCriteriaCollection.Count);
			Assist.SearchTerm = "des"; //starts with
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { criteria1.PK, criteria2.PK, criteria3.PK }, Assist.FilteredSuggestedCriteriaCollection.GetPKs());
			Assist.SearchTerm = "des "; //whole word
			Assist.FilteredSuggestedCriteriaCollection.Rebuild();
			AssertEquals(0, Assist.FilteredSuggestedCriteriaCollection.Count);
		}

		protected override FilteredSuggestedRelevantDiagnosticCriteriaCollection GetCollectionToTest()
		{
			return (FilteredSuggestedRelevantDiagnosticCriteriaCollection)Assist.FilteredSuggestedCriteriaCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RelevantDiagnosticCriteria(Factory.NewWithValidTestData<IncidentDiagnosticCriteria>(), null);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Triage = Factory.NewWithValidTestData<IncidentTriage>();
			Triage.IMT_IsActive = true;
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentTriage Triage;
	}
}
