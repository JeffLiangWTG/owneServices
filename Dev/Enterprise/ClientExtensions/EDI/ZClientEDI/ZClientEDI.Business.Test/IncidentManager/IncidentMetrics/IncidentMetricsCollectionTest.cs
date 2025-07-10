using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentMetricsCollection))]
	public class IncidentMetricsCollectionTest : ActiveBusinessObjectCollectionTestCase<IncidentMetricsCollection>
	{
		protected override IncidentMetricsCollection GetCollectionToTest()
		{
			return new IncidentMetricsCollection(Factory);
		}
	}
}
