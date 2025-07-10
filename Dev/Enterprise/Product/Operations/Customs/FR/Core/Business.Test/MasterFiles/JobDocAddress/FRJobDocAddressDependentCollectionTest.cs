using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(FRJobDocAddressDependentCollection))]
	class FRJobDocAddressDependentCollectionTest : JobDocAddressDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FRJobDocAddressDependentCollection(Factory.New<JobDeclarationForTest>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<FRJobDocAddress>();
		}
	}
}

