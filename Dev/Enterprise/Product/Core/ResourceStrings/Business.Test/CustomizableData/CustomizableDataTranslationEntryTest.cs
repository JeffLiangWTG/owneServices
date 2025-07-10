using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(CustomizableDataTranslationEntry))]
	sealed class CustomizableDataTranslationEntryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null).All[0];
		}

		public void TestTranslation_MaxLength_NonTranslatableRegistryItemValueCaptionSource()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
			var first = (CustomizableDataTranslationEntry)page.All.First();
			AssertEquals(page.Customizable.Source.MaxLength, first.TranslationInfo.MaxLength);
		}

		public void TestTranslation_MaxLength_TranslatableRegistryItemValueCaptionSource()
		{
			var captionSource = new TranslatableRegistryItemValueCaptionSource(new RegistryItemCaptionSource("Subject", "Body"), "Subject");
			var page = new CustomizableDataTranslationPage(new CustomizableDataResourceStrings(captionSource), testHelper.GetMultilingualString("Subject"), null);
			var firstSubject = page.All.Cast<CustomizableDataTranslationEntry>().First(x => x.Caption == "Subject");
			var firstBody = page.All.Cast<CustomizableDataTranslationEntry>().First(x => x.Caption == "Body");
			AssertEquals(20, firstSubject.TranslationInfo.MaxLength);
			AssertEquals(100, firstBody.TranslationInfo.MaxLength);
		}

		public void TestTranslation_MaxLength_MultipleDataDataCaptionSource()
		{
			var currency = Factory.New<RefCurrency>();
			var captionSource = new MultipleDataCaptionSource(new ZPropertyInfo[] { currency.RX_UnitNameInfo, currency.RX_DescInfo }, "x");
			currency.RX_UnitName = "WWW";
			currency.RX_Desc = "description";
			var page = new CustomizableDataTranslationPage(new CustomizableDataResourceStrings(captionSource), testHelper.GetMultilingualString("WWW"), null);
			var firstWWW = page.All.Cast<CustomizableDataTranslationEntry>().First(x => x.Caption == "WWW");

			page = new CustomizableDataTranslationPage(new CustomizableDataResourceStrings(captionSource), testHelper.GetMultilingualString("description"), null);
			var firstDesc = page.All.Cast<CustomizableDataTranslationEntry>().First(x => x.Caption == "description");

			AssertEquals(currency.RX_UnitNameInfo.MaxLength, firstWWW.TranslationInfo.MaxLength);
			AssertEquals(currency.RX_DescInfo.MaxLength, firstDesc.TranslationInfo.MaxLength);
		}

		public void TestEmptyTranslationDefaultsToSystemTranslation()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
			var four = (CustomizableDataTranslationEntry)page.All.First(entry => ((CustomizableDataTranslationEntry)entry).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)entry).English == "four");
			AssertEquals(testHelper.ChineseNumbers[4], four.Translation);
			four.Translation = "bla";
			AssertEquals("bla", four.Translation);
			four.Translation = "";
			AssertEquals(testHelper.ChineseNumbers[4], four.Translation);

			four = (CustomizableDataTranslationEntry)page.All.First(entry => ((CustomizableDataTranslationEntry)entry).Language == Core.SharedConstants.Languages.Spanish && ((CustomizableDataTranslationEntry)entry).English == "four");
			AssertEquals("four", four.Translation);
			four.Translation = "bla";
			AssertEquals("bla", four.Translation);
			four.Translation = "";
			AssertEquals("four", four.Translation);
		}

		public void TestTranslationTrimmed()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
			var four = (CustomizableDataTranslationEntry)page.All.First(entry => ((CustomizableDataTranslationEntry)entry).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)entry).English == "four");
			AssertEquals(testHelper.ChineseNumbers[4], four.Translation);
			four.Translation = "  \r\nbla\r\n  ";
			AssertEquals("bla", four.Translation);
		}

		public void TestTranslationMaxLengthUsesTranslatableSourceMaxLength()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
			var four = (CustomizableDataTranslationEntry)page.All.First(entry => ((CustomizableDataTranslationEntry)entry).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)entry).English == "four");
			AssertEquals(50, four.TranslationInfo.MaxLength);
		}

		public void TestTranslation_MoreThanMaxLength()
		{
			var page = new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null);
			var entry = (CustomizableDataTranslationEntry)page.All.First(translation => ((CustomizableDataTranslationEntry)translation).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)translation).English == "four");
			entry.Translation = new string('a', 51);
			AssertEquals(new string('a', 50), entry.Translation);

			entry.Translation = new string('a', 49);
			AssertEquals(new string('a', 49), entry.Translation);
		}

		public void TestOriginalTranslationIsNullWithEdgeCasesOfSourceMaxLength()
		{
			using (var temptestHelper = new CustomizableDataTestHelper(-1))
			{
				var page = new CustomizableDataTranslationPage(temptestHelper.CustomizableDataResourceStrings, temptestHelper.GetMultilingualString("one"), null);
				var entry = (CustomizableDataTranslationEntry)page.All.First(translation => ((CustomizableDataTranslationEntry)translation).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)translation).English == "four");

				AssertEquals("Pre-condition: Source Max Length should be -1.", -1, temptestHelper.CustomizableDataResourceStrings.Source.MaxLength);
				AssertEquals("Pre-condition: Translation Max Length should be -1.", -1, entry.TranslationInfo.MaxLength);

				entry.Translation = new string('a', 51);
				AssertEquals("Translation should not be truncated.", new string('a', 51), entry.Translation);

				AssertNullOrEmpty("Original Translation should be empty.", entry.OriginalTranslation);
			}

			using (var temptestHelper = new CustomizableDataTestHelper(0))
			{
				var page = new CustomizableDataTranslationPage(temptestHelper.CustomizableDataResourceStrings, temptestHelper.GetMultilingualString("one"), null);
				var entry = (CustomizableDataTranslationEntry)page.All.First(translation => ((CustomizableDataTranslationEntry)translation).Language == Core.SharedConstants.Languages.ChineseSimplified && ((CustomizableDataTranslationEntry)translation).English == "four");

				AssertEquals("Pre-condition: Source Max Length should be 0.", 0, temptestHelper.CustomizableDataResourceStrings.Source.MaxLength);
				AssertEquals("Pre-condition: Translation Max Length should be 0.", 0, entry.TranslationInfo.MaxLength);

				entry.Translation = new string('a', 51);
				AssertEquals("Translation should be truncated.", string.Empty, entry.Translation);

				AssertEquals("Original Translation should not be empty.", new string('a', 51), entry.OriginalTranslation);
			}
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

		class RegistryItemCaptionSource : IRegistryItemVariantLengthCaptionSource, ITranslatableRegistryItemCaptionSource
		{
			public RegistryItemCaptionSource(string caption1, string caption2)
			{
				Caption1 = caption1;
				Caption2 = caption2;
			}

			readonly string Caption1;
			readonly string Caption2;

			ushort ICustomizableDataCaptionSource.Asmid
			{
				get
				{
					return ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId;
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			string ICustomizableDataCaptionSource.Description
			{
				get { return "RegistryItemCaptionSource Test"; }
			}

			string ICustomizableDataCaptionSource.GetKey(object context, string caption)
			{
				return CustomizableDataResourceStrings.GetCustomizableDataKey("RegistryItemCaptionSourceTest", caption);
			}

			IEnumerable<IResString> ICustomizableDataCaptionSource.GetCompileTimeSystemCaptions()
			{
				throw new NotImplementedException();
			}

			IEnumerable<IResString> ICustomizableDataCaptionSource.GetRuntimeCaptions(IResString userCaption, object context)
			{
				throw new NotImplementedException();
			}

			public int MaxLength
			{
				get { return -1; }
			}

			IEnumerable<ResourceString> IRegistryItemCaptionSource.DefaultStrings
			{
				get { return Array.Empty<ResourceString>(); }
			}

			IEnumerable<string> IRegistryItemCaptionSource.GetCaptions(object value)
			{
				yield return Caption1;
				yield return Caption2;
			}

			bool IRegistryItemCaptionSource.IsTranslatable
			{
				get { return true; }
			}

			int IRegistryItemVariantLengthCaptionSource.GetMaxLength(string caption)
			{
				switch (caption)
				{
					case "Subject":
						return 20;
					case "Body":
						return 100;
					default:
						return MaxLength;
				}
			}

			public IEnumerable<MultilingualString> GetMultilingualCaptions(object value)
			{
				yield return ResString._GetMultilingualString(((ICustomizableDataCaptionSource)this).Asmid, ((ICustomizableDataCaptionSource)this).GetKey(null, Caption1), Caption1);
				yield return ResString._GetMultilingualString(((ICustomizableDataCaptionSource)this).Asmid, ((ICustomizableDataCaptionSource)this).GetKey(null, Caption2), Caption2);
			}
		}
	}
}
