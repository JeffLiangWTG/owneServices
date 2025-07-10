using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentTriageFilterBusinessObject))]
	public class IncidentTriageFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IncidentTriageFilterBusinessObject();
		}

		(IncidentTriage, IncidentTriage, IncidentTriage) CreateIncidentTriagesForTest()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FMT";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			Factory.Save();

			return (triage1, triage2, triage3);
		}

		(IncidentTriageChecklistItem, IncidentTriageChecklistItem, IncidentTriageChecklistItem) CreateIncidentTriageChecklistItemPivotsForTest(IncidentTriage triage1, IncidentTriage triage2, IncidentTriage triage3)
		{
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem3 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			Factory.Save();

			var pivot1 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_IMT_Triage = triage1.PK;

			var pivot2 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			pivot2.IMP_IMT_Triage = triage2.PK;

			var pivot3 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot3.IMP_IMC_ChecklistItem = checklistItem3.PK;
			pivot3.IMP_IMT_Triage = triage3.PK;
			Factory.Save();

			return (checklistItem1, checklistItem2, checklistItem3);
		}

		public void TestTypeFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_Type = "TY1";
			triage2.IMT_Type = "TY2";
			triage3.IMT_Type = "TY3";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Type"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "TY2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should only contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property = "TY2";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestLevelFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_Level = "1";
			triage2.IMT_Level = "2";
			triage3.IMT_Level = "2";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Level"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "2";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
		}

		public void TestTriageDescriptionFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_SupportDescription = "triage1";
			triage2.IMT_SupportDescription = "triage2";
			triage3.IMT_SupportDescription = "triage3";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Triage Description"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "triage3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "triage3";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
		}

		public void TestProductFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_Product = ProductTypes.Codes.CargoWiseOne;
			triage2.IMT_Product = ProductTypes.Codes.CargoWiseOne;
			triage3.IMT_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Product"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "ENT";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "ENT";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
		}

		public void TestModuleFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_Module = "mo1";
			triage2.IMT_Module = "mo2";
			triage3.IMT_Module = "mo3";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Sec. / Req. / Srv. Code"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "mo3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "mo2";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestProductAreaFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_ProductArea = "XRM";
			triage2.IMT_ProductArea = "a2";
			triage3.IMT_ProductArea = "a3";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Product Area"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "XRM";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should only contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property = "XRM";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestIsPublishedToERequestFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_IsPublished = false;
			triage2.IMT_IsPublished = true;
			triage3.IMT_IsPublished = false;
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentTriageFilter["Is Published to eRequest"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should only contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestIsInternalFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_IsInternal = true;
			triage2.IMT_IsInternal = true;
			triage3.IMT_IsInternal = false;
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentTriageFilter["Is Internal?"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestIsPublishedToTriageAssistFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			triage1.IMT_IsPublishedToAssist = true;
			triage2.IMT_IsPublishedToAssist = false;
			triage3.IMT_IsPublishedToAssist = false;
			Factory.Save();
			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentTriageFilter["Is Published to Triage Assist"];
			var incidentTriage = new IncidentTriageCollection(Factory);
			filter.Property0 = true;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);
			Assert("Should only contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
			filter.Property0 = false;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);
			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestTriageNumberFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Triage Number"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = triage2.IMT_TriageNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should only contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property = triage2.IMT_TriageNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestIsChecklistPublishedFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemPivotsForTest(triage1, triage2, triage3);
			checklistItem1.IMC_IsPublished = true;
			checklistItem2.IMC_IsPublished = false;
			checklistItem3.IMC_IsPublished = false;
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentTriageFilter["Is Checklist Published?"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should only contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestChecklistDescriptionFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemPivotsForTest(triage1, triage2, triage3);
			checklistItem1.IMC_SupportDescription = "YUI";
			checklistItem2.IMC_SupportDescription = "UUU";
			checklistItem3.IMC_SupportDescription = "PLK";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Checklist Description"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "YUI";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should only contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.Property = "YUI";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}

		public void TestChecklistCategoryFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemPivotsForTest(triage1, triage2, triage3);
			checklistItem1.IMC_Category = "OO";
			checklistItem2.IMC_Category = "GG";
			checklistItem3.IMC_Category = "TR";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Checklist Category"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "TR";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "TR";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
		}

		public void TestChecklistResponseTypeFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemPivotsForTest(triage1, triage2, triage3);
			checklistItem1.IMC_ResponseType = "DYB";
			checklistItem2.IMC_ResponseType = "TTT";
			checklistItem3.IMC_ResponseType = "SGH";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentTriageFilter["Checklist Response Type"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = "SG";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = "SGH";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should contain triage2", incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));
		}

		public void TestChecklistItemFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();
			var (checklistItem1, checklistItem2, checklistItem3) = CreateIncidentTriageChecklistItemPivotsForTest(triage1, triage2, triage3);
			checklistItem1.IMC_SupportDescription = "YUI";
			checklistItem2.IMC_SupportDescription = "UUU";
			checklistItem3.IMC_SupportDescription = "PLK";
			Factory.Save();

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleGuidFilter)incidentTriageFilter["Checklist Item"];
			var incidentTriage = new IncidentTriageCollection(Factory);

			filter.Property = checklistItem3.PK;
			filter.IsActive = true;

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should not contain triage1", !incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should only contain triage3", incidentTriage.Contains(triage3.PK));

			filter.Property = checklistItem2.PK;
			filter.IsActive = true;
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should only contain triage2", incidentTriage.Contains(triage2.PK));
		}

		public void TestDiagnosticCriteriaFilter()
		{
			var (triage1, triage2, triage3) = CreateIncidentTriagesForTest();

			var diagnostic1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnostic1.IMD_Description = "Test 1";
			var diagnostic2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnostic2.IMD_Description = "Desc 2";
			var diagnostic3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = triage1.DiagnosticCriteriaPivots.AddNew();
			pivot1.IMO_IMD_DiagnosticCriteria = diagnostic1.PK;
			var pivot2 = triage2.DiagnosticCriteriaPivots.AddNew();
			pivot2.IMO_IMD_DiagnosticCriteria = diagnostic2.PK;
			var pivot3 = triage3.DiagnosticCriteriaPivots.AddNew();
			pivot3.IMO_IMD_DiagnosticCriteria = diagnostic3.PK;

			Factory.Save();

			var incidentTriage = new IncidentTriageCollection(Factory);
			incidentTriage.Load();

			Assert("Precondition", incidentTriage.Contains(triage1.PK));
			Assert("Precondition", incidentTriage.Contains(triage2.PK));
			Assert("Precondition", incidentTriage.Contains(triage3.PK));

			var incidentTriageFilter = new IncidentTriageFilterBusinessObject();
			var filter = (ModuleGuidForeignCollectionFilter)incidentTriageFilter["DiagnosticCriteria"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Description");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "Test 1";

			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should only contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should not contain triage3", !incidentTriage.Contains(triage3.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "Desc 2";
			incidentTriage.Load(incidentTriageFilter.Filter);

			Assert("Should contain triage1", incidentTriage.Contains(triage1.PK));
			Assert("Should not contain triage2", !incidentTriage.Contains(triage2.PK));
			Assert("Should contain triage3", incidentTriage.Contains(triage3.PK));
		}
	}
}
