using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNJobDocAddressDependentCollection))]
	class CNJobDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CNJobDocAddressDependentCollection(Factory.New<JobDeclaration>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CNJobDocAddress>();
	}
}
