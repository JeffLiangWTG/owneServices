using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridWithoutColumnStylesSerialisationTest : TestCase
	{
		public void TestColumnsNotSerializedByDesigner()
		{
			ZGridWithoutColumnStylesSerialisationTest.AssertColumnStylesNotSerializedByDesigner(typeof(ZModuleButtonGridWithoutColumnStylesSerialisation));
		}
	}
}
