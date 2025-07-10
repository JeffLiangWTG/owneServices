using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationLineCollection))]
	class BMControlCustomisationLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BMControlCustomisationLineCollection>
	{
		protected override BMControlCustomisationLineCollection GetCollectionToTest()
		{
			return Factory.New<BMControlCustomisation>().CustomisationLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BMControlCustomisationLine(Factory.New<BMControlCustomisation>());
		}
	}
}
