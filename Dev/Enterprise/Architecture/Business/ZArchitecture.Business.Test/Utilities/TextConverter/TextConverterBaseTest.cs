using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Utilities.Testing
{
	sealed class TextConverterBaseTest : TestCase
	{
		public void TestConvert()
		{
			var converter = new TestTextConverter();
			AssertEquals("BThisA BisA BaA Bunconverted-stringA", converter.Convert("This is a unconverted-string"));
			AssertEquals("  BThisA BisA BanA BanotherA Bstring!A  ", converter.Convert("  This is an another string!  "));

			AssertEquals("BWORDA BwordA BWordA BwOrdA", converter.Convert("WORD word Word wOrd"));
			AssertEquals("WORD BwordA BWordA BwOrdA", converter.Convert("WORD word Word wOrd", new string[] { "WORD" }, StringComparer.Ordinal));
			AssertEquals("WORD word Word wOrd", converter.Convert("WORD word Word wOrd", new string[] { "Word" }, StringComparer.OrdinalIgnoreCase));
		}

		#region Implementation

		class TestTextConverter : TextConverterBase
		{
			protected override void AddWordConverters(IList<IWordConverter> wordConverters)
			{
				wordConverters.Add(new AppendCharA());
				wordConverters.Add(new PrependCharB());
			}
		}

		class AppendCharA : IWordConverter
		{
			public string Convert(string word)
			{
				return word + 'A';
			}
		}

		class PrependCharB : IWordConverter
		{
			public string Convert(string word)
			{
				return 'B' + word;
			}
		}

		#endregion
	}
}
