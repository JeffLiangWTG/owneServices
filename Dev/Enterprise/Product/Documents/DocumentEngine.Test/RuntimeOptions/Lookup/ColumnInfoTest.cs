using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnInfoTest : TestCase
	{
		public void TestAddProperty()
		{
			ColumnInfo columnInfo = new ColumnInfo("Caption");
			columnInfo.AddProperty("Name", "Value");

			Assert(columnInfo.Properties.ContainsKey("Name"));
			Assert(columnInfo.Properties.ContainsValue("Value"));
		}

		public void TestCaptionLocalized()
		{
			var columnInfo = new ColumnInfo("Caption");
			AssertEquals("Caption", columnInfo.CaptionLocalized);

			columnInfo.CaptionLocalizedData = new ResourceStringData("Caption", "虚拟名称");
			AssertEquals("虚拟名称", columnInfo.CaptionLocalized);
		}
	}
}
