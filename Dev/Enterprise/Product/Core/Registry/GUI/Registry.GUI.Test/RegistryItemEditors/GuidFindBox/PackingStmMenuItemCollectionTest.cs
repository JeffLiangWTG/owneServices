using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GuidFindBoxBusinessObject.PackingStmMenuItemCollection))]
	sealed class PackingStmMenuItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GuidFindBoxBusinessObject.PackingStmMenuItemCollection(Factory, new ZQuery());
		}
	}
}
