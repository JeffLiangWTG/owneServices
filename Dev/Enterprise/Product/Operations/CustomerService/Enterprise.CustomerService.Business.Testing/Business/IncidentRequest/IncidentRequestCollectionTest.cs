using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business
{
	[TestedType(typeof(IncidentRequestCollection))]
	sealed class IncidentRequestCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentRequestCollection(Factory);
		}
	}
}
