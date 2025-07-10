using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentDiagnosticCriteria))]
	public class IncidentDiagnosticCriteriaTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
		}

		public void TestIMD_IsActiveDefault()
		{
			var incidentDiagnosticCriteria = Factory.New<IncidentDiagnosticCriteria>();
			AssertEquals("Default should be true", true, incidentDiagnosticCriteria.IMD_IsActive);
		}
	}
}
