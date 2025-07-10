using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusSealCollection))]
sealed class CusSealCollectionTest : ActiveBusinessObjectCollectionTestCase<CusSealCollection>
{
	protected override CusSealCollection GetCollectionToTest()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var container = header.Containers.AddNew();
		return new CusSealCollection(container);
	}
}

