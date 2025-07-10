using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Tools.Testing
{
	[TestedType(typeof(DbCommandWrapperCollection))]
	public class DbCommandWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DbCommandWrapperCollection>
	{
		protected override DbCommandWrapperCollection GetCollectionToTest()
		{
			return new DbCommandWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DbCommandWrapper();
		}
	}
}
