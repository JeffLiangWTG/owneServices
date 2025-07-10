using System;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebThemeRegistryDataType))]
	sealed class WebThemeRegistryDataTypeTest : RegistryDataTypeTestCase<WebThemeRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataType = new WebThemeRegistryDataType();

			var theme1 = new WebTrackerTheme("web1.com/", "STD");
			var theme2 = new WebTrackerTheme("web2.com/", "ALT");
			var theme3 = new WebTrackerTheme("web3.com/", "CLS");

			var themeBefore = new[] { theme1, theme2, theme3 };
			var themeDuring = dataType.Serialise(themeBefore);
			var themeAfter = dataType.Deserialise(themeDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", themeBefore, themeAfter);
		}

		#region Implementation

		protected override WebThemeRegistryDataType GetNewDataType()
		{
			return new WebThemeRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsTheme = (WebTrackerTheme[])lhs;
			var rhsTheme = (WebTrackerTheme[])rhs;

			AssertEquals(message, lhsTheme.Length, rhsTheme.Length);

			for (int i = 0; i < lhsTheme.Length; i++)
			{
				AssertEquals(message, lhsTheme[i].Url, rhsTheme[i].Url);
				AssertEquals(message, lhsTheme[i].Code, rhsTheme[i].Code);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var theme1 = new WebTrackerTheme("web1.com/", "STD");
			var theme2 = new WebTrackerTheme("web2.com/", "ALT");
			var theme3 = new WebTrackerTheme("web3.com/", "CLS");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Array.Empty<WebTrackerTheme>(), Encoding.Unicode.GetBytes(JsonSerializer.Serialize(Array.Empty<WebTrackerTheme>()))),
				new ValidSampleAndBinaryValueInDB(new[] { theme1 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { theme1 }))),
				new ValidSampleAndBinaryValueInDB(new[] { theme1, theme2, theme3 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { theme1, theme2, theme3 })))
			};
		}

		#endregion
	}
}
