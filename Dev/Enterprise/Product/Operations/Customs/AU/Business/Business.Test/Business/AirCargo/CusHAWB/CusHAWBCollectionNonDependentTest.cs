using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBCollectionNonDependent))]
	sealed class CusHAWBCollectionNonDependentTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusHAWBCollectionNonDependent(Factory);
	}
}
