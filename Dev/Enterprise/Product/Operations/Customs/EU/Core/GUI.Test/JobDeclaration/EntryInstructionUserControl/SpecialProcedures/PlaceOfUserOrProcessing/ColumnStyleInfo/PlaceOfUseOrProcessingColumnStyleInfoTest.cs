using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			AssertEquals(typeof(PlaceOfUseOrProcessingColumnStyle), new PlaceOfUseOrProcessingColumnStyleInfo().ColumnStyleType);
		}
	}
}
