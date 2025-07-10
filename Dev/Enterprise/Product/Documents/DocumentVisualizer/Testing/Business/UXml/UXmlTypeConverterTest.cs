using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.UXml
{
	sealed class UXmlTypeConverterTest : TestCase
	{
		public void TestConvertStringToZDateTimeAndZDateTimeOffset()
		{
			var converter = new UXmlTypeConverter();

			var timeString = "17-May-24 17:27:07";
			var tryResult = converter.Convert(timeString, typeof(ZDateTime));
			AssertEquals(false, tryResult.IsFaulted);
			AssertEquals(timeString, tryResult.Value.ToString());

			timeString = "17-May-24 17:27:07 +14:00";
			tryResult = converter.Convert(timeString, typeof(ZDateTimeOffset));
			AssertEquals(false, tryResult.IsFaulted);
			AssertEquals(timeString, tryResult.Value.ToString());
		}
	}
}
