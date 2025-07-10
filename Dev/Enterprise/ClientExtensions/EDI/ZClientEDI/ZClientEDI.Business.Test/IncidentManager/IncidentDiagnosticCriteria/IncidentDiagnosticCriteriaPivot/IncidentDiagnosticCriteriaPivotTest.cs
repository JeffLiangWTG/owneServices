using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentDiagnosticCriteriaPivot))]
	public class IncidentDiagnosticCriteriaPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var diagnosticPivot = Factory.NewWithValidTestData<IncidentDiagnosticCriteriaPivot>();
			diagnosticPivot.IMV_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			return diagnosticPivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var diagnosticPivot = factory.NewWithValidTestData<IncidentDiagnosticCriteriaPivot>();
			diagnosticPivot.IMV_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			return diagnosticPivot;
		}
	}
}
