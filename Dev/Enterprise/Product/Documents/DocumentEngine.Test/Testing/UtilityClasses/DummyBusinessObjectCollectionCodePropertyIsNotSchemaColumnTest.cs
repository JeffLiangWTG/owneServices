using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestedType(typeof(DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn))]
	sealed class DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumnTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn(base.Factory);
		}
	}
}
