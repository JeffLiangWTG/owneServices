using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestMakeDocDataDynamic()
		{
			var docDataObject = new DummyDocDataObject();
			var dynamicDocDataObject = docDataObject.MakeDocDataDynamic();

			AssertNotNull("dynamic data created", dynamicDocDataObject);
			AssertType<DocDataObjectDynamicData>("correct dynamic data created", dynamicDocDataObject);

			var dynamicText = dynamicDocDataObject.GetDynamicProperty(nameof(DummyDocDataObject.Text));
			AssertType<DocDataObjectDynamicData>("correct dynamic data created", dynamicText);
		}

		public void TestMakeDocDataDynamic_Null()
		{
			DocDataObject docDataObject = null;

			var dynamicDocDataObject = docDataObject.MakeDocDataDynamic();

			AssertNull("dynamic data created", dynamicDocDataObject);
		}
	}
}
