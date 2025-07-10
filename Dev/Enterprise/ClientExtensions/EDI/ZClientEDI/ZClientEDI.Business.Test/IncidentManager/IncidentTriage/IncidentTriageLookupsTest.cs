using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants.CustomerService;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIMT_Product_ShouldMatchIM_Product()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			AssertArrayEqualsByElements(incident.Lookups.ProductList.ToArray(), triage.Lookups.ProductList.ToArray());
		}

		public void TestIMT_ProductArea_ShouldMatchIM_ProgramArea()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			AssertArrayEqualsByElements(incident.Lookups.ProductAreaList.ToArray(), triage.Lookups.ProductAreaList.ToArray());
		}

		public void TestIMT_Module_ShouldMatchIM_Module()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Product = incident.IM_Product;
			triage.IMT_ProductArea = incident.ProductArea;

			incident.IM_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			AssertEquals("Precondition", incident.ModuleType, triage.ModuleType);
			AssertArrayEqualsByElements(incident.Lookups.ModuleListAllModules.ToArray(), triage.Lookups.ModuleListAllModules.ToArray());

			incident.IM_Priority = CriticalityCodes.CR8_ComplianceRequirement;
			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			AssertEquals("Precondition", incident.ModuleType, triage.ModuleType);
			AssertArrayEqualsByElements(incident.Lookups.ModuleListAllModules.ToArray(), triage.Lookups.ModuleListAllModules.ToArray());

			incident.IM_Priority = CriticalityCodes.CR9_CustomerServiceRequest;
			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertEquals("Precondition", incident.ModuleType, triage.ModuleType);
			AssertArrayEqualsByElements(incident.Lookups.ModuleListAllModules.ToArray(), triage.Lookups.ModuleListAllModules.ToArray());
		}

		public void TestDiagnosticCriteriaNotLinked()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var item1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var collection = new IncidentTriageDiagnosticCriteriaPivotCollection(triage, Factory);
			var pivot1 = collection.AddNew();
			pivot1.IMO_IMD_DiagnosticCriteria = item1.PK;
			Factory.Save();
			var triageInNewFactory = new BusinessObjectFactory().Load<IncidentTriage>(triage.PK);
			triageInNewFactory.Lookups.DiagnosticCriteriaNotLinked.Load();
			AssertEquals(item2.PK, triageInNewFactory.Lookups.DiagnosticCriteriaNotLinked.Single().PK);
		}
	}
}
