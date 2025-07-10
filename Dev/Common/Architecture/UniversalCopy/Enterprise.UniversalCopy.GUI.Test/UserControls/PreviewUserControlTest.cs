using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class PreviewUserControlTest : TestCaseWithFactory
	{
	}

	[TestedType(typeof(PreviewUserControl.MockBusinessObjectCollection<DummyBaseBusinessObject>))]
	public class MockBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PreviewUserControl.MockBusinessObjectCollection<DummyBaseBusinessObject>(Factory);
		}
	}
}
