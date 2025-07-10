using System;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebCustomCssRegistryDataType))]
	sealed class WebCustomCssRegistryDataTypeTest : RegistryDataTypeTestCase<WebCustomCssRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataType = new WebCustomCssRegistryDataType();

			var css1 = new WebTrackerCustomCss("web1.com/", "body { color: red; }");
			var css2 = new WebTrackerCustomCss("web2.com/", "body { color: blue; }");
			var css3 = new WebTrackerCustomCss("web3.com/", "body { color: green; }");

			var cssBefore = new WebTrackerCustomCss[] { css1, css2, css3 };
			var cssDuring = dataType.Serialise(cssBefore);
			var cssAfter = dataType.Deserialise(cssDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", cssBefore, cssAfter);
		}

		#region Implementation

		protected override WebCustomCssRegistryDataType GetNewDataType()
		{
			return new WebCustomCssRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsCss = (WebTrackerCustomCss[])lhs;
			var rhsCss = (WebTrackerCustomCss[])rhs;

			AssertEquals(message, lhsCss.Length, rhsCss.Length);

			for (int i = 0; i < lhsCss.Length; i++)
			{
				AssertEquals(message, lhsCss[i].Url, rhsCss[i].Url);
				AssertEquals(message, lhsCss[i].Data, rhsCss[i].Data);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var css1 = new WebTrackerCustomCss("web1.com/", "body { color: red; }");
			var css2 = new WebTrackerCustomCss("web2.com/", "body { color: blue; }");
			var css3 = new WebTrackerCustomCss("web3.com/", "body { color: green; }");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(Array.Empty<WebTrackerCustomCss>(), Encoding.Unicode.GetBytes(JsonSerializer.Serialize(Array.Empty<WebTrackerCustomCss>()))),
				new ValidSampleAndBinaryValueInDB(new[] { css1 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { css1 }))),
				new ValidSampleAndBinaryValueInDB(new[] { css1, css2, css3 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { css1, css2, css3 })))
			};
		}

		#endregion
	}
}
