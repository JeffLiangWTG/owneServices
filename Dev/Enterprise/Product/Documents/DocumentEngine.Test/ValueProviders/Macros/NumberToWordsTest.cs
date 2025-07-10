using System;
using System.Reflection;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(NumberToWords))]
	sealed class NumberToWordsTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<Number To Words(12)>");
			AssertIsResponsibleForReplacing("<NumberToWords(table.field)>");
			AssertIsResponsibleForReplacing("<NUMBER TO WORDS(12)>");
			AssertIsResponsibleForReplacing("<Number To Words( )>");
			AssertIsResponsibleForReplacing("<Number To Words(0)>");
			AssertIsResponsibleForReplacing("<Number To Words(0, \"EN-US\")>");
		}

		public void TestGettingTheCorrectNumberToWordConverter()
		{
			AssertEquals("elf", ValueProviderToTest.GetReplacement("< NUMBER TO WORDS( 11 ,DE-DE )>", Report));
			AssertEquals("eleven", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(11 ,EN-US)>", Report));
			AssertEquals("壹拾贰", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12,ZH-CN)>", Report));
			AssertEquals("mười hai", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12 ,VI-VN)>", Report));
			AssertEquals("eleven", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(11)>", Report));
		}

		public void TestGenericCallOfAllNumberToWordConverter()
		{
			Type[] allAssemblyTypes = Assembly.Load("Enterprise.DocumentEngine").GetTypes();

			foreach (Type converterType in allAssemblyTypes)
			{
				if (converterType.Name.Length == 18 && converterType.Name.StartsWith("NumberToString"))
				{
					String languageCode = converterType.Name.Substring(15);
					AssertNotEquals("String is empty for number 12", "", ValueProviderToTest.GetReplacement("< NUMBER TO WORDS( 11 ," + languageCode + " )>", Report));
				}
			}
		}

		public void TestRationalNumbers()
		{
			AssertEquals("elf Komma zwei", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(11.2 ,DE-DE)>", Report));
			AssertEquals("twelve point twenty five", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12.25 ,EN-US)>", Report));
			AssertEquals("twelve point two", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12.2)>", Report));
		}

		public void TestEnterWrongLanguage()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(11 ,GER)>", Report));
		}

		public void TestEnterWrongNumberFormat()
		{
			AssertEquals("zwölf Komma fünfundzwanzig", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12.25 ,DE-DE)>", Report));
		}

		public void TestReplacement()
		{
			AssertEquals("twelve", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12)>", Report));
			AssertEquals("zero", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(0)>", Report));
			AssertEquals("thirty two", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(32)>", Report));
			AssertEquals("twenty one", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(21)>", Report));
			AssertEquals("thirteen", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(13)>", Report));
			AssertEquals("ninety nine", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(99)>", Report));
			AssertEquals("eight", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(8)>", Report));
			AssertEquals("six", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(6)>", Report));
			AssertEquals("one hundred and twenty three", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(123)>", Report));
			AssertEquals("four hundred and ninety nine", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(499)>", Report));
			AssertEquals("one hundred and twenty three thousand, five hundred and thirty four", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(123534)>", Report));
			AssertEquals("six hundred thousand", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(600000)>", Report));
			AssertEquals("one million", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(1000000)>", Report));
			AssertEquals("twelve point two", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(12.2)>", Report));
			AssertEquals("twelve", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(\"12\")>", Report));
		}

		public void TestReplacementWorksWithFields()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(1, 2, Report, "");
			AssertEquals("thirteen", ValueProviderToTest.GetReplacement("<NUMBER TO WORDS(Header.GST)>", Report));
		}

		public void TestReplacementInCurrentLanguage()
		{
			Report.Parent.Language = Core.SharedConstants.Languages.Spanish;
			AssertEquals("diecinueve", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Portuguese;
			AssertEquals("dezenove", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Italian;
			AssertEquals("diciannove", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.French;
			AssertEquals("dix-neuf", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Dutch;
			AssertEquals("negentien", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Turkish;
			AssertEquals("ondokuz", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Bulgarian;
			AssertEquals("деветнадесет", ValueProviderToTest.GetReplacement("<NumberToWords(19)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("壹拾贰", ValueProviderToTest.GetReplacement("<NumberToWords(12)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.ChineseTraditional;
			AssertEquals("壹拾貳", ValueProviderToTest.GetReplacement("<NumberToWords(12)>", Report));
		}

		public void TestReplacementInUnsupportedLanguageForNumbers1ToTen()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter((string key) =>
				{
					string value = string.Empty;
					foreach (CodeDescriptionPair pair in new CodeDescriptionPairList(OLookUpEditType.Numbers1To10))
					{
						if (((ResourceString)pair.MultilingualDescription).ResourceKey == key)
						{
							switch (pair.Code)
							{
								case "1":
									value = "O";
									break;
								case "2":
									value = "TT";
									break;
								case "3":
									value = "TTT";
									break;
								case "4":
									value = "FFFF";
									break;
								case "5":
									value = "FFFFF";
									break;
								case "6":
									value = "SSSSSS";
									break;
								case "7":
									value = "SSSSSSS";
									break;
								case "8":
									value = "EEEEEEEE";
									break;
								case "9":
									value = "NNNNNNNNN";
									break;
								case "10":
									value = "TTTTTTTTTT";
									break;
							}
						}
					}
					return new ResourceStringData(key, value);
				});

				Report.Parent.Language = "XXX";

				AssertEquals("O", ValueProviderToTest.GetReplacement("<NumberToWords(1)>", Report));
				AssertEquals("TT", ValueProviderToTest.GetReplacement("<NumberToWords(2)>", Report));
				AssertEquals("TTT", ValueProviderToTest.GetReplacement("<NumberToWords(3)>", Report));
				AssertEquals("FFFF", ValueProviderToTest.GetReplacement("<NumberToWords(4)>", Report));
				AssertEquals("FFFFF", ValueProviderToTest.GetReplacement("<NumberToWords(5)>", Report));
				AssertEquals("SSSSSS", ValueProviderToTest.GetReplacement("<NumberToWords(6)>", Report));
				AssertEquals("SSSSSSS", ValueProviderToTest.GetReplacement("<NumberToWords(7)>", Report));
				AssertEquals("EEEEEEEE", ValueProviderToTest.GetReplacement("<NumberToWords(8)>", Report));
				AssertEquals("NNNNNNNNN", ValueProviderToTest.GetReplacement("<NumberToWords(9)>", Report));
				AssertEquals("TTTTTTTTTT", ValueProviderToTest.GetReplacement("<NumberToWords(10)>", Report));

				AssertEquals("", ValueProviderToTest.GetReplacement("<NumberToWords(555)>", Report));
				AssertEquals("", ValueProviderToTest.GetReplacement("<NumberToWords(2.01)>", Report));
			}
		}

		public void TestAllEnglishVariants()
		{
			foreach (ICodeDescription language in new AvailableDocBuilderLanguageList(Factory))
			{
				if (Res.IsEnglish(language.Code))
				{
					using (Res.TemporarilySwitchLanguage(language.Code))
					{
						AssertEquals("twelve", ValueProviderToTest.GetReplacement("<NumberToWords(12)>", Report));
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new NumberToWords();
		}
	}
}
