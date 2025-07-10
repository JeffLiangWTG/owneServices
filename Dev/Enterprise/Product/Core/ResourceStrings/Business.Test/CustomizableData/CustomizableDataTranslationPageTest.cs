using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(CustomizableDataTranslationPage))]
	sealed class CustomizableDataTranslationPageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportEntries()
		{
			var pageRef = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null);
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null);
			var edited =
				"\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
				+ "\"EN-US\",\"zero\",\"RU-RU\",\"0\"\r\n"
				+ "\"EN-US\",\"one\",\"AR-AE\",\"1\"\r\n"
				+ "\"EN-US\",\"two\",\"FR-FR\",\"2\"\r\n"
				+ "\"EN-US\",\"three\",\"ZH-CN\",\"3\"\r\n"
				+ "\"EN-US\",\"bamboozle\",\"ZZZ\",\":-)\"\r\n";

			using (var tempfile = TempFile.NewWithExtension("csv"))
			{
				File.WriteAllText(tempfile.Filename, edited);

				using (var fileStream = File.OpenRead(tempfile.Filename))
				{
					var error = page.ImportEntries(fileStream, tempfile.Filename);

					AssertEquals(string.Empty, error);
					AssertEquals("should not add additional entries", pageRef.All.Count, page.All.Count);
				}
			}

			foreach (CustomizableDataTranslationEntry entry in page.All)
			{
				var refItem = pageRef.All.Cast<CustomizableDataTranslationEntry>().FirstOrDefault(e => e.English.Equals(entry.English) && e.Language.Equals(entry.Language));
				AssertNotNull("should not add additional entries", refItem);

				if (entry.Language == Core.SharedConstants.Languages.Russian && entry.English == "zero")
				{
					AssertEquals("0", entry.Translation);
				}
				else if (entry.Language == Core.SharedConstants.Languages.Arabic && entry.English == "one")
				{
					AssertEquals("1", entry.Translation);
				}
				else if (entry.Language == Core.SharedConstants.Languages.French && entry.English == "two")
				{
					AssertEquals("2", entry.Translation);
				}
				else if (entry.Language == Core.SharedConstants.Languages.ChineseSimplified && entry.English == "three")
				{
					AssertEquals("3", entry.Translation);
				}
				else
				{
					AssertEquals(refItem.Translation, entry.Translation);
				}
			}
		}

		public void TestImportEntries_ReadOnly()
		{
			var pageRef = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null);
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null);
			page.ReadOnly = true;

			var edited =
				@"Language|Original|Language|Translation
EN-US|zero|RU-RU|0
EN-US|one|AR-AE|1
EN-US|two|FR-FR|2
EN-US|three|ZH-CN|3
EN-US|bamboozle|ZZZ|:-)";

			using (var tempfile = TempFile.NewWithExtension("csv"))
			{
				File.WriteAllText(tempfile.Filename, edited);

				using (var fileStream = File.OpenRead(tempfile.Filename))
				{
					var error = page.ImportEntries(fileStream, tempfile.Filename);

					AssertEquals("Editing is not allowed", error);
					AssertEquals("should not add additional entries", pageRef.All.Count, page.All.Count);
				}
			}

			foreach (CustomizableDataTranslationEntry entry in page.All)
			{
				var refItem = pageRef.All.Cast<CustomizableDataTranslationEntry>().FirstOrDefault(e => e.English.Equals(entry.English) && e.Language.Equals(entry.Language));
				AssertNotNull("should not add additional entries", refItem);
				AssertEquals(refItem.Translation, entry.Translation);
			}
		}

		public void TestExportEntries()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null);
			var result = page.ExportEntries();

			var contentLines =
				("\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
				+ "\"EN\",\"zero\"\r\n"
				+ "\"EN\",\"one\"\r\n"
				+ "\"EN\",\"two\"\r\n"
				+ "\"EN\",\"three\"\r\n"
				+ "\"EN\",\"four\"\r\n"
				+ "\"EN\",\"five\"\r\n"
				+ "\"EN\",\"six\"\r\n"
				+ "\"EN\",\"seven\"\r\n"
				+ "\"EN\",\"eight\"\r\n"
				+ "\"EN\",\"nine\"\r\n"
				+ "\"EN\",\"ten\"")
					.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var entry in result)
			{
				var lines = entry.Value.ToStringWithNewLineBetweenAppends().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];
					Assert(string.Format("{0} should start with {1}", line, contentLines[index]), line.StartsWith(contentLines[index]));
				}
			}
		}

		public void TestAllTranslationsHaveSaveResourceKeyWithCaptionValueSet()
		{
			var salutations = new ContactSalutationCollection();
			SalutationHelper.LoadDefaultSalutations(salutations);
			var allLanguages = DataFile.GetAvailableLanguages().Where(language => CargoWiseOne.ResourceStrings.ResourceStrings.Normalize(language) != Res.DefaultLanguage);

			var salutationRegistryItemCaptionResouce = new TranslatableRegistryItemValueCaptionSource(OrganisationsDataRegistry.Instance.ContactSalutation, salutations);
			var page = new CustomizableDataTranslationPage(new CustomizableDataResourceStrings(salutationRegistryItemCaptionResouce), (ResourceString)Constants.DefaultSalutations.Sir, null);
			page.CurrentCaption = (ResourceString)Constants.DefaultSalutations.DearMale;
			AssertEquals(allLanguages.Count(), page.AllTranslationsOfCurrentValue.Count);
		}

		public void TestAllValues()
		{
			var allLanguages = DataFile.GetAvailableLanguages().Where(language => CargoWiseOne.ResourceStrings.ResourceStrings.Normalize(language) != Res.DefaultLanguage);
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("zero"), null);

			var englishNumbers = testHelper.CustomizableDataResourceStrings.Source.GetRuntimeCaptions().ToArray();
			for (int i = 0; i < englishNumbers.Length; i++)
			{
				page.CurrentCaption = (ResourceString)englishNumbers[i];
				var array = page.AllTranslationsOfCurrentValue.ToArray<CustomizableDataTranslationEntry>();
				AssertArrayEqualsByElements(allLanguages.ToArray(), Array.ConvertAll(array, entry => entry.Language.ToString()));
				AssertNull(array.FirstOrDefault(entry => entry.English != englishNumbers[i].EnglishText));
				AssertNotNull(array.FirstOrDefault(entry => entry.Language == Core.SharedConstants.Languages.ChineseSimplified && entry.Translation == testHelper.ChineseNumbers[i]));
				AssertNotNull(array.FirstOrDefault(entry => entry.Language == Core.SharedConstants.Languages.French && entry.Translation == testHelper.FrenchNumbers[i]));
				AssertNotNull(array.FirstOrDefault(entry => entry.Language == Core.SharedConstants.Languages.Spanish && entry.Translation == englishNumbers[i].EnglishText));
			}

			foreach (string language in allLanguages)
			{
				page.CurrentLanguage = language;
				var array = page.AllValuesInCurrentLanguage.ToArray<CustomizableDataTranslationEntry>();
				AssertArrayEqualsByElements(testHelper.CustomizableDataResourceStrings.Source.GetRuntimeCaptions().Select(c => c.EnglishText).ToArray(), Array.ConvertAll(array, entry => entry.English.ToString()));
				AssertNull(array.FirstOrDefault(entry => entry.Language != language));
				if (language == Core.SharedConstants.Languages.ChineseSimplified)
				{
					AssertArrayEqualsByElements(testHelper.ChineseNumbers, Array.ConvertAll(array, entry => entry.Translation.ToString()));
				}
				else if (language == Core.SharedConstants.Languages.French)
				{
					AssertArrayEqualsByElements(testHelper.FrenchNumbers, Array.ConvertAll(array, entry => entry.Translation.ToString()));
				}
				else
				{
					AssertArrayEqualsByElements(testHelper.CustomizableDataResourceStrings.Source.GetRuntimeCaptions().Select(c => c.EnglishText).ToArray(), Array.ConvertAll(array, entry => entry.Translation.ToString()));
				}
			}
		}

		public void TestSaveWithExistingTranslation()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("三", (string)testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three"));

				var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
				AssertEquals(false, page.HasChanges);
				var three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
				AssertEquals("three", three.English);
				AssertEquals("三", three.Translation);
				three.Translation = "叄";
				AssertEquals(true, page.HasChanges);
				page.Save();
				AssertEquals(false, page.HasChanges);
				AssertEquals("叄", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three"));
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, checkedOutStrings[0].HD_Language);
				AssertEquals("叄", checkedOutStrings[0].HD_Caption);

				page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
				AssertEquals(false, page.HasChanges);
				three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
				AssertEquals("three", three.English);
				AssertEquals("叄", three.Translation);
				three.Translation = "叁";
				AssertEquals(true, page.HasChanges);
				page.Save();
				AssertEquals(false, page.HasChanges);
				AssertEquals("叁", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three"));
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, checkedOutStrings[0].HD_Language);
				AssertEquals("叁", checkedOutStrings[0].HD_Caption);

				page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
				AssertEquals(false, page.HasChanges);
				three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
				AssertEquals("three", three.English);
				AssertEquals("叁", three.Translation);
				three.Translation = "三";
				AssertEquals(true, page.HasChanges);
				page.Save();
				AssertEquals(false, page.HasChanges);
				AssertEquals("三", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three"));
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);

				page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
				AssertEquals(false, page.HasChanges);
				three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
				AssertEquals("three", three.English);
				AssertEquals("三", three.Translation);
				three.Translation = "three";
				AssertEquals(true, page.HasChanges);
				page.Save();
				AssertEquals(false, page.HasChanges);
				AssertEquals("three", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three"));
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, checkedOutStrings[0].HD_Language);
				AssertEquals("three", checkedOutStrings[0].HD_Caption);
			}
		}

		public void TestSaveWithoutExistingTranslation()
		{
			AssertEquals("three", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three").ToString(Core.SharedConstants.Languages.Spanish));

			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
			AssertEquals(false, page.HasChanges);
			var three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.Spanish);
			AssertEquals("three", three.English);
			AssertEquals("three", three.Translation);
			three.Translation = "tres";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals(false, page.HasChanges);
			AssertEquals("tres", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three").ToString(Core.SharedConstants.Languages.Spanish));
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals(Core.SharedConstants.Languages.Spanish, checkedOutStrings[0].HD_Language);
			AssertEquals("tres", checkedOutStrings[0].HD_Caption);

			page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
			AssertEquals(false, page.HasChanges);
			three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.Spanish);
			AssertEquals("three", three.English);
			AssertEquals("tres", three.Translation);
			three.Translation = "bla";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals(false, page.HasChanges);
			AssertEquals("bla", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three").ToString(Core.SharedConstants.Languages.Spanish));
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals(Core.SharedConstants.Languages.Spanish, checkedOutStrings[0].HD_Language);
			AssertEquals("bla", checkedOutStrings[0].HD_Caption);

			page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("three"), null);
			AssertEquals(false, page.HasChanges);
			three = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.Spanish);
			AssertEquals("three", three.English);
			AssertEquals("bla", three.Translation);
			three.Translation = "three";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals(false, page.HasChanges);
			AssertEquals("three", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "three").ToString(Core.SharedConstants.Languages.Spanish));
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
		}

		public void TestSaveMultipleUsingAllValuesInCurrentLanguage()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("zero"), null);
			AssertEquals(false, page.HasChanges);
			page.CurrentLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			var item = (CustomizableDataTranslationEntry)page.AllValuesInCurrentLanguage.First(entry => ((CustomizableDataTranslationEntry)(entry)).English == "one");
			item.Translation = "貳";
			page.CurrentLanguage = Core.SharedConstants.Languages.French;
			item = (CustomizableDataTranslationEntry)page.AllValuesInCurrentLanguage.First(entry => ((CustomizableDataTranslationEntry)(entry)).English == "seven");
			item.Translation = "bla";
			page.CurrentLanguage = Core.SharedConstants.Languages.Spanish;
			item = (CustomizableDataTranslationEntry)page.AllValuesInCurrentLanguage.First(entry => ((CustomizableDataTranslationEntry)(entry)).English == "seven");
			item.Translation = "siete ";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals(false, page.HasChanges);

			AssertEquals("貳", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "one").ToString(Core.SharedConstants.Languages.ChineseSimplified));
			AssertEquals("un", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "one").ToString(Core.SharedConstants.Languages.French));
			AssertEquals("bla", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "seven").ToString(Core.SharedConstants.Languages.French));
			AssertEquals("siete", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "seven").ToString(Core.SharedConstants.Languages.Spanish));
			AssertEquals("七", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "seven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
		}

		public void TestNewEnglishString()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("eleven"), null);
			AssertEquals(false, page.HasChanges);
			var eleven = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
			AssertEquals("eleven", eleven.Translation);
			eleven.Translation = "十一";
			eleven.Translation = "eleven";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals("eleven", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "eleven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);

			page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("eleven"), null);
			AssertEquals(false, page.HasChanges);
			eleven = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
			AssertEquals("eleven", eleven.Translation);
			eleven.Translation = "十一";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals("十一", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "eleven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
			AssertEquals("eleven", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "eleven").ToString(Core.SharedConstants.Languages.French));
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, checkedOutStrings[0].HD_Language);
			AssertEquals("十一", checkedOutStrings[0].HD_Caption);

			page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("eleven"), null);
			AssertEquals(false, page.HasChanges);
			eleven = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
			AssertEquals("十一", eleven.Translation);
			eleven.Translation = "拾壹";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals("拾壹", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "eleven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, checkedOutStrings[0].HD_Language);
			AssertEquals("拾壹", checkedOutStrings[0].HD_Caption);

			page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("eleven"), null);
			AssertEquals(false, page.HasChanges);
			eleven = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.ChineseSimplified);
			AssertEquals("拾壹", eleven.Translation);
			eleven.Translation = "eleven";
			AssertEquals(true, page.HasChanges);
			page.Save();
			AssertEquals("eleven", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "eleven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
		}

		[ExpectNoExceptions]
		public void TestStringLengthOutOfRange()
		{
			const string english = @"Prompt payment is appreciated or a late fee of 1.5% will be incurred. 
							In accordance with 19 CFR 111.29(b)(1), we are obliged to advise you of the following: 
							If you are the importer of record, payment to the broker will not relieve you of liability for Customs charges 
							(duties, taxes, or other debts owed Customs) in the event these charges are note paid by the broker. 
							Therefore, if you pay by check, Customs charges may be paid with a separate check payable to U.S. 
							Bureau of Customs and Border Protection which shall be delivered to Customs by the broker";

			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("test"), null);
			AssertEquals(false, page.HasChanges);
			var test = (CustomizableDataTranslationEntry)page.AllTranslationsOfCurrentValue.First(entry => ((CustomizableDataTranslationEntry)(entry)).Language == Core.SharedConstants.Languages.Bulgarian);
			AssertEquals("test", test.Translation);
			test.Translation = "test";
			test.English = english;
		}

		public void TestEgbIsIncluded()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("zero"), null);
			var languages = page.AllTranslationsOfCurrentValue.Select(t => t.Language);
			AssertCollectionContains(Core.SharedConstants.Languages.EnglishBritish, languages);
			AssertCollectionNotContains(Core.SharedConstants.Languages.EnglishAmerican, languages);
		}

		public void TestCustomLanguageIncluded()
		{
			var testLanguage1 = Factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			testLanguage1.RA_IsActive = true;

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_IsActive = true;

			Factory.Save();

			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("zero"), null);
			var languages = page.AllTranslationsOfCurrentValue.Select(t => t.Language);
			AssertCollectionContains(testLanguage1.FullLanguageCode, languages);
			AssertCollectionContains(testLanguage2.FullLanguageCode, languages);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
		}

		protected override void SetUp()
		{
			testHelper = new CustomizableDataTestHelper();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testHelper.Dispose();
			base.TearDown();
		}

		CustomizableDataTestHelper testHelper;
	}
}
