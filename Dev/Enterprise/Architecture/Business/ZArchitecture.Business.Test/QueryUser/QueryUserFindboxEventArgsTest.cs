using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	[TestedType(typeof(QueryUserFindboxEventArgs))]
	sealed class QueryUserFindboxEventArgsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new QueryUserFindboxEventArgs(DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));
		}
	}
}
