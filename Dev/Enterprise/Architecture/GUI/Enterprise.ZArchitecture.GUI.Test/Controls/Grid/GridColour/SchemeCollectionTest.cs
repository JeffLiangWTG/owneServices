using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	[TestedType(typeof(SchemeCollection))]
	sealed class SchemeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SchemeCollection(Factory);
		}
	}
}
