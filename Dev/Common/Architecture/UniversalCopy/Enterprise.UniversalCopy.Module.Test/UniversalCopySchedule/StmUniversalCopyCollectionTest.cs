using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Module.Testing
{
	[TestedType(typeof(StmUniversalCopyCollection))]
	public class StmUniversalCopyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmUniversalCopyCollection(Factory);
		}
	}
}
