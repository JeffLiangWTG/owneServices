using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ArrayExtensionTest : TestCaseWithFactory
	{
		public void TestConvertBusinessObjectArrayToElementType()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var dummies = (new BusinessObject[] { dummy }).OfType(typeof(DummyChildBusinessObject));
			AssertEquals("Should have 1 element in the resultant array", 1, dummies.Length);
		}

		[ExpectNoExceptions]
		public void TestConvertBusinessObjectArrayToElementType_WithEmptyArray()
		{
			object notUsed = (System.Array.Empty<BusinessObject>()).OfType(typeof(DummyBusinessObject));
		}
	}
}
