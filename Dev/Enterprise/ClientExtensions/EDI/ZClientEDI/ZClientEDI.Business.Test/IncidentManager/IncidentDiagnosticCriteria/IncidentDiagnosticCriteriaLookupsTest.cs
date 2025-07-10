using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentDiagnosticCriteriaLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIMD_Type_ShouldMatchLookups()
		{
			var diagnosticCriteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var diagnosticCriteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var diagnosticCriteriaTypes = diagnosticCriteria1.Lookups.Types.ToArray();
			AssertArrayEqualsByElements(diagnosticCriteriaTypes, diagnosticCriteria2.Lookups.Types.ToArray());
			AssertEquals(true, diagnosticCriteriaTypes.Any(x => x.Code.Equals(IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom) && x.Description.Equals(IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom)));
			AssertEquals(true, diagnosticCriteriaTypes.Any(x => x.Code.Equals(IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor) && x.Description.Equals(IncidentDiagnosticCriteriaTypes.Descriptions.DiagnosticFactor)));
		}

		public void TestIncidentTriageNotLinked()
		{
			var criteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item1 = Factory.NewWithValidTestData<IncidentTriage>();
			var item2 = Factory.NewWithValidTestData<IncidentTriage>();
			var collection = new IncidentDiagnosticCriteriaTriagePivotCollection(criteria, Factory);
			var pivot1 = collection.AddNew();
			pivot1.IMO_IMT_Triage = item1.PK;
			Factory.Save();
			var criteriaInNewFactory = new BusinessObjectFactory().Load<IncidentDiagnosticCriteria>(criteria.PK);
			criteriaInNewFactory.Lookups.IncidentTriageNotLinked.Load();
			AssertEquals(item2.PK, criteriaInNewFactory.Lookups.IncidentTriageNotLinked.Single().PK);
		}
	}
}
