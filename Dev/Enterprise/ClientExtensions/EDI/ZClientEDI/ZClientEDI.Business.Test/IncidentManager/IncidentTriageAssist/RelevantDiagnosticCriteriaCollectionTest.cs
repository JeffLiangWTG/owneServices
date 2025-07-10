using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(RelevantDiagnosticCriteriaCollection))]
	public class RelevantDiagnosticCriteriaCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RelevantDiagnosticCriteriaCollection>
	{
		public void TestLoadBySearchTerm()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Description = "des1_1 des1_2";
			criteria1.IMD_Keywords = "des1_3 des1_4 apple";
			criteria2.IMD_Description = "des2_1 des2_2 apple";
			criteria2.IMD_Keywords = "des2_3 des2_4";
			criteria3.IMD_Description = "des3_1 des3_2";
			criteria3.IMD_Keywords = "des3_3 des3_4";
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.Parent.SearchTerm = "apple \"des1_1 des1_4\" \"des2_3 des2_4\"";
			collection.LoadBySearchTerm();
			AssertEquals(2, collection.Count);
			AssertEquals(criteria2.PK, collection[0].DiagnosticCriteria.PK);
			AssertEquals(criteria1.PK, collection[1].DiagnosticCriteria.PK);
		}

		public void TestLoadBySearchTerm_Operators()
		{
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

			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.Parent.SearchTerm = "apple \"des2_1 des2_2\"";
			collection.Parent.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom;
			collection.Parent.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAll;
			collection.Parent.ShouldSearchDescription = true;
			collection.Parent.ShouldSearchKeywords = false;
			collection.LoadBySearchTerm();
			AssertEquals(1, collection.Count);
			AssertEquals(criteria2.PK, collection[0].DiagnosticCriteria.PK);

			collection.Parent.SearchTerm = "apple \"des2_1 des2_2\"";
			collection.Parent.CriteriaTypeFilter = TriageAssistBusinessObject.ComparisonConstants.Any;
			collection.Parent.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAny;
			collection.Parent.ShouldSearchDescription = true;
			collection.Parent.ShouldSearchKeywords = true;
			collection.LoadBySearchTerm();
			AssertEquals(3, collection.Count);

			collection.Parent.SearchTerm = "\"3 des1_4 ap\"";
			collection.Parent.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.DiagnosticFactor;
			collection.Parent.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.Exact;
			collection.Parent.ShouldSearchDescription = false;
			collection.Parent.ShouldSearchKeywords = true;
			collection.LoadBySearchTerm();
			AssertEquals(1, collection.Count);
			AssertEquals(criteria1.PK, collection[0].DiagnosticCriteria.PK);

			collection.Parent.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom;
			collection.Parent.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.Any;
			collection.Parent.ShouldSearchDescription = true;
			collection.Parent.ShouldSearchKeywords = true;
			collection.Parent.SearchTerm = "des3_3 des"; //starts with
			collection.LoadBySearchTerm();
			AssertContainsExactElementsInAnyOrder(new[] { criteria2.PK, criteria3.PK }, collection.GetPKs());
			collection.Parent.SearchTerm = "des3_3 des "; //whole word
			collection.LoadBySearchTerm();
			AssertContainsExactElementsInAnyOrder(new[] { criteria3.PK }, collection.GetPKs());
			collection.Parent.SearchTerm = "des"; //starts with
			collection.LoadBySearchTerm();
			AssertContainsExactElementsInAnyOrder(new[] { criteria2.PK, criteria3.PK }, collection.GetPKs());
			collection.Parent.SearchTerm = "des "; //whole word
			collection.LoadBySearchTerm();
			AssertEquals(false, collection.Any());

			collection.Parent.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAll;
			collection.Parent.SearchTerm = "des3_3 des"; //starts with
			collection.LoadBySearchTerm();
			AssertContainsExactElementsInAnyOrder(new[] { criteria3.PK }, collection.GetPKs());
			collection.Parent.SearchTerm = "des3_3 des "; //whole word
			collection.LoadBySearchTerm();
			AssertEquals(false, collection.Any());
			collection.Parent.SearchTerm = "des"; //starts with
			collection.LoadBySearchTerm();
			AssertContainsExactElementsInAnyOrder(new[] { criteria2.PK, criteria3.PK }, collection.GetPKs());
			collection.Parent.SearchTerm = "des "; //whole word
			collection.LoadBySearchTerm();
			AssertEquals(false, collection.Any());
		}

		public void TestLoadBySearchTerm_OnlyIsActiveIsTrue()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_IsActive = false;
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_IsActive = false;
			criteria1.IMD_Description = "apple1";
			criteria2.IMD_Description = "apple2";
			criteria3.IMD_Description = "apple3";
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.Parent.SearchTerm = "apple";
			collection.LoadBySearchTerm();
			AssertEquals(1, collection.Count);
			AssertEquals(criteria2.PK, collection[0].DiagnosticCriteria.PK);
		}

		public void TestLoadBySearchTerm_SortOrder()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Description = "it is key1, it is key2";
			criteria1.IMD_Keywords = "it is key1, it is key3";
			criteria2.IMD_Description = "it is key1, it is key4";
			criteria2.IMD_Keywords = "it is key6";
			criteria3.IMD_Description = "it is key5";
			criteria3.IMD_Keywords = "it is key5";
			Factory.Save();

			AssertSortOrder("one token, sort by description", "key1", new[] { criteria1.PK, criteria2.PK });
			AssertSortOrder("multiple tokens, sort by match count", "key1 key4", new[] { criteria2.PK, criteria1.PK });
			AssertSortOrder("multiple tokens, sort by match count", "key1 key3", new[] { criteria1.PK, criteria2.PK });

			AssertSortOrder("token in quotes", "\"it is key5\"", new[] { criteria3.PK });
			AssertSortOrder("token in quotes + token", "\"it is key5\" key4", new[] { criteria2.PK, criteria3.PK });
		}

		void AssertSortOrder(string message, string searchTerm, IEnumerable<ZGuid> guids)
		{
			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.Parent.SearchTerm = searchTerm;
			collection.LoadBySearchTerm();
			AssertEquals(message, guids.Count(), collection.Count);
			AssertEquals(message, true, collection.OfType<RelevantDiagnosticCriteria>().Select(x => x.DiagnosticCriteria.PK).SequenceEqual(guids));
		}

		public void TestLoadLinkedCriteria()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "INV";
			var pivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot2.LinkParent(Incident);
			pivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			pivot2.IMV_Status = "VER";
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.LoadLinkedCriteria();
			AssertEquals(2, collection.Count);
			AssertEquals(criteria2.PK, collection[0].DiagnosticCriteria.PK);
			AssertEquals(criteria1.PK, collection[1].DiagnosticCriteria.PK);
			AssertEquals(true, collection[0].Confirm);
			AssertEquals(true, collection[1].Investigate);
		}

		public void TestLoadLinkedCriteria_OnlyIsActiveIsTrue()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_IsActive = false;
			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			var pivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot2.LinkParent(Incident);
			pivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Parent.Product = "";
			collection.LoadLinkedCriteria();
			AssertEquals("Only criteria1 should be retrieved", 1, collection.Count);
			AssertEquals("Only criteria1 should be retrieved", criteria1.PK, collection[0].DiagnosticCriteria.PK);
		}

		public void TestLoadSuggestedCriteria()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot3 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot3.IMO_IMT_Triage = Triage.PK;
			pivot3.IMO_IMD_DiagnosticCriteria = criteria3.PK;

			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			obj.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			AssertEquals(1, obj.SuggestedCriteriaCollection.Count);
			AssertEquals(true, obj.SuggestedCriteriaCollection.Select(x => x.PK).Contains(criteria3.PK));
		}

		public void TestLoadSuggestedCriteria_OnlyIsActiveIsTrue()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_IsActive = false;

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot3 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot3.IMO_IMT_Triage = Triage.PK;
			pivot3.IMO_IMD_DiagnosticCriteria = criteria3.PK;

			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			obj.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			AssertEquals("There should be no criteria retrieved", 0, obj.SuggestedCriteriaCollection.Count);

			criteria3.IMD_IsActive = true;
			Factory.Save();
			obj.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			AssertEquals(1, obj.SuggestedCriteriaCollection.Count);
			AssertEquals("Only criteria3 should be retrieved", true, obj.SuggestedCriteriaCollection.Select(x => x.PK).Contains(criteria3.PK));
		}

		public void TestLoadSuggestedCriteria_IsFocused()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot3 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot3.IMO_IMT_Triage = Triage.PK;
			pivot3.IMO_IMD_DiagnosticCriteria = criteria3.PK;

			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			obj.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			AssertEquals(1, obj.SuggestedCriteriaCollection.Count);
			AssertEquals(true, obj.SuggestedCriteriaCollection.Select(x => x.PK).Contains(criteria3.PK));
			AssertEquals(true, obj.SuggestedCriteriaCollection[0].IsFocused);
		}

		protected override RelevantDiagnosticCriteriaCollection GetCollectionToTest()
		{
			return new RelevantDiagnosticCriteriaCollection(new TriageAssistBusinessObject(Incident));
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
		}

		SupportIncident Incident;
		IncidentTriage Triage;
	}
}
