using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(DiagnosticCriteriaLinkResponseResultPivotCollection))]
	public class DiagnosticCriteriaLinkResponseResultPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var itemLink = Factory.NewWithValidTestData<DiagnosticCriteriaInvestigationItemLink>();
			return new DiagnosticCriteriaLinkResponseResultPivotCollection(itemLink, Factory);
		}
	}
}
