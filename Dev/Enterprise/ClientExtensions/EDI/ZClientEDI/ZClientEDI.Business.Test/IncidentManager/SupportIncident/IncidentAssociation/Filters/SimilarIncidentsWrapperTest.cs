using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	[TestedType(typeof(SimilarIncidentsWrapper))]
	public class SimilarIncidentsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SimilarIncidentsWrapper(Factory.New<SupportIncident>());
		}
	}
}
