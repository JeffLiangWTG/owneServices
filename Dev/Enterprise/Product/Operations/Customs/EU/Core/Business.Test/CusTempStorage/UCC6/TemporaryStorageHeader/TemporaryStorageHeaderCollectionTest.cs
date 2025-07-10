using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderCollection))]
	sealed class TemporaryStorageHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<TemporaryStorageHeaderCollection>
	{
	}
}
