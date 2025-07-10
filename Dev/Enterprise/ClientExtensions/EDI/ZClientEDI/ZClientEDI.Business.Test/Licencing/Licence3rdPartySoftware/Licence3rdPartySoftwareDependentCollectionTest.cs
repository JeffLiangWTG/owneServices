using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(Licence3rdPartySoftwareDependentCollection))]
	public class Licence3rdPartySoftwareDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new Licence3rdPartySoftwareDependentCollection(Factory.New<LicenceCompany>());
		}
	}
}
