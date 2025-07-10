using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUJobDocAddressDependentCollection))]
	sealed class AUJobDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AUJobDocAddressDependentCollection(Factory.New<JobDeclaration>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<AUJobDocAddress>();
	}
}
