using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(DiagnosticCriteriaInvestigationItemLink))]
	public class DiagnosticCriteriaInvestigationItemLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var link = factory.NewWithValidTestData<DiagnosticCriteriaInvestigationItemLink>();
			return link;
		}
	}
}
