using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentDiagnosticCriteriaCollection))]
	public class IncidentDiagnosticCriteriaCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentDiagnosticCriteriaCollection(Factory);
		}
	}
}
