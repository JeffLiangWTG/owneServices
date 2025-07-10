using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	sealed class TranslationFeedbackFactoryTest : TranslationFeedbackTestCase
	{
		public void TestMultilingualStrings()
		{
			AddMockTranslation("kttrfl", "Butterfly", "蝶");
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgn2", "Dragon", "龍");
			AddMockTranslation("kmnk", "Monkey", "猴");
			AddMockTranslation("kmnk2", "Monkey", "猴");
			AddMockTranslation("klck", "{0} Luck", "{0} 祥");
			AddMockTranslation("klck2", "{0} Luck", "{0} 祥");
			AddMockTranslation("kvr", "{0} Over {1}", "{0} 超 {1}");
			ENG.Put("kcw", new ResourceStringData("kcw", "Cow"));

			StmTranslationFeedbackCollection feedbacks;

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kdrgn", "Dragon"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("klck", "{0} Luck", ResString.GetMultilingualString("kmnk", "Monkey")));
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("klck", "{0} Luck", "{0} 祥", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kmnk", "Monkey", "猴", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kvr", "{0} Over {1}", ResString.GetMultilingualString("kdrgn", "Dragon"), MultilingualString.Join(" , ", ResString.GetMultilingualString("kdg", "Dog"), ResString.GetMultilingualString("kmnk", "Monkey"))));
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("kvr", "{0} Over {1}", "{0} 超 {1}", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[2]);
			AssertFeedback("kmnk", "Monkey", "猴", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[3]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kdg", "Dog"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, (NoResString)"蝶");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kttrfl", "Butterfly", "蝶", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kcw", "Cow"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kcw", "Cow", "Cow", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("klck", "{0} Luck", new ZString("Zero")));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("klck", "{0} Luck", "{0} 祥", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("klck", "{0} Luck", string.Empty));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("klck", "{0} Luck", "{0} 祥", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
		}

		public void TestNonResString()
		{
			AddMockTranslation("kttrfl", "Butterfly", "蝶");
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgn2", "Dragon", "龍");
			AddMockTranslation("kmnk", "Monkey", "猴");
			AddMockTranslation("kmnk2", "Monkey", "猴");
			AddMockTranslation("klck", "{0} Luck", "{0} 祥");
			AddMockTranslation("klck2", "{0} Luck", "{0} 祥");
			AddMockTranslation("kvr", "{0} Over {1}", "{0} 超 {1}");
			ENG.Put("kcw", new ResourceStringData("kcw", "Cow"));

			StmTranslationFeedbackCollection feedbacks;

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kvr", "{0} Over {1}", ResString.GetMultilingualString("kdrgn", "Dragon"), (NoResString)"Dog"));
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("kvr", "{0} Over {1}", "{0} 超 {1}", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, ResString.GetMultilingualString("kvr", "{0} Over {1}", ResString.GetMultilingualString("kdrgn", "Dragon"), ResString.GetMultilingualString("kdg", "Dog")));
			AssertEquals(3, feedbacks.Count);
			AssertFeedback("kvr", "{0} Over {1}", "{0} 超 {1}", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[2]);
		}

		public void TestResourceStringDatas()
		{
			AddMockTranslation(
				new ResourceStringData("f1", "Fish", "Fish Fish", "Fish Fish Fish", "Fish Fish Fish Fish"),
				new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"));
			AddMockTranslation(
				new ResourceStringData("f2", "Fish", string.Empty, "Fish Fish Fish", string.Empty),
				new ResourceStringData("f2", "魚", string.Empty, "魚魚魚", string.Empty));
			AddMockTranslation(
				new ResourceStringData("f3", string.Empty, string.Empty, "Fish Fish Fish", string.Empty),
				new ResourceStringData("f3", string.Empty, string.Empty, "魚魚魚", string.Empty));
			AddMockTranslation(
				new ResourceStringData("f4", "Fish", string.Empty, "Fish Fish Fish", string.Empty),
				new ResourceStringData("f4", string.Empty, string.Empty, "魚魚魚", string.Empty));
			ENG.Put("kcw", new ResourceStringData("kcw", string.Empty, string.Empty, "Cow", "Cow Cow"));

			StmTranslationFeedbackCollection feedbacks;

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"));
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("f1", "Fish", "魚", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);
			AssertFeedback("f1", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[2]);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[3]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"), "魚魚魚");
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("f1", "Fish", "魚", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[0]);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[1]);
			AssertFeedback("f1", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[2]);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[3]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"), "魚魚");
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("f1", "Fish", "魚", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[0]);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);
			AssertFeedback("f1", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[2]);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[3]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f2", "魚", string.Empty, "魚魚魚", string.Empty), "魚");
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("f2", "Fish", "魚", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("f2", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f3", string.Empty, string.Empty, "魚魚魚", string.Empty));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f3", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f4", string.Empty, string.Empty, "魚魚魚", string.Empty), "魚魚魚");
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("f4", "Fish", "", "Fish", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[0]);
			AssertFeedback("f4", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData(), "魚魚魚魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("kcw", "Cow"), "Cow");
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("kcw", "Cow", "Cow", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kcw", "Cow Cow", "Cow Cow", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.RelatedLevel, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f2", "魚", string.Empty, "魚魚魚", string.Empty), "魚魚魚魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("f3", string.Empty, string.Empty, "魚魚魚", string.Empty), "魚魚魚 ");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f3", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
		}

		public void TestICodeDescriptions()
		{
			AddMockTranslation("kttrfl", "Butterfly", "蝶");
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgn2", "Dragon", "龍");

			StmTranslationFeedbackCollection feedbacks;

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair("ddd", ResString.GetMultilingualString("kdg", "Dog")));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair("bbb", "蝶"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kttrfl", "Butterfly", "蝶", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair("Butterfly", "蝶"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kttrfl", "Butterfly", "蝶", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair("蝶", ResString.GetMultilingualString("kdrgn", "Dragon")));
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kttrfl", "Butterfly", "蝶", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair(ResString.GetMultilingualString("kdg", "Dog"), "ddd"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new CodeDescriptionPair(ResString.GetMultilingualString("kdg", "Dog"), ResString.GetMultilingualString("kdrgn", "Dragon")));
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[1]);
		}

		public void TestAStringLookups()
		{
			AddMockTranslation(
				new ResourceStringData("f1", "Fish", "Fish Fish", "Fish Fish Fish", "Fish Fish Fish Fish"),
				new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"));
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgn2", "DRAGON", "龍");
			AddMockTranslation("kmnk", "Monkey", "猴");
			AddMockTranslation("kmnk2", "Monkey", "猴");
			AddMockTranslation("klck", "{0} Luck", "{0} 祥");
			AddMockTranslation("klck2", "{0} Luck", "{0} 祥");
			AddMockTranslation("kvr", "{0} Over {1}", "{0} 超 {1}");
			AddMockTranslation("zzz", "{0} {1}", "{0} {1}");
			ENG.Put("kcw", new ResourceStringData("kcw", "Cow"));

			StmTranslationFeedbackCollection feedbacks;

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetString("kdg", "Dog"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetString("klck", "{0} Luck", Res.GetString("kmnk", "Monkey")));
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("klck", "{0} Luck", "{0} 祥", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[0]);
			AssertFeedback("kmnk", "Monkey", "猴", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetString("kvr", "{0} Over {1}", Res.GetString("kdrgn", "Dragon"), Res.GetString("klck", "{0} Luck", Res.GetString("kmnk", "Monkey"))));
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("kvr", "{0} Over {1}", "{0} 超 {1}", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[0]);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[1]);
			AssertFeedback("klck", "{0} Luck", "{0} 祥", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[2]);
			AssertFeedback("kmnk", "Monkey", "猴", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, feedbacks[3]);

			ClearRecentlyUsed();

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "狗");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("", "Dog", "狗", string.Empty, TranslationFeedbackMatchTypes.Codes.None, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "龍");
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);
			AssertFeedback("kdrgn2", "DRAGON", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "猴 祥");
			AssertEquals(2, feedbacks.Count);
			AssertFeedback("", "{0} Luck", "{0} 祥", string.Empty, TranslationFeedbackMatchTypes.Codes.None, feedbacks[0]);
			AssertFeedback("", "Monkey", "猴", string.Empty, TranslationFeedbackMatchTypes.Codes.None, feedbacks[1]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish", "魚", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "魚魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "魚魚魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "魚魚魚魚");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "Cow");
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kcw", "Cow", "Cow", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.FullText, feedbacks[0]);
		}

		public void TestCachePerformance()
		{
			for (int i = 0; i < 100; i++)
			{
				AddMockTranslation("X" + i, "ENG" + i, "CHT" + i);
			}

			StmTranslationFeedbackCollection feedbacks;

			// warm up the cache
			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "CHT1");
			AssertEquals(1, feedbacks.Count);
			AssertEquals(1, feedbacks[0].AllContexts.Count);
			AssertEquals(0, feedbacks[0].OtherTranslations.Count);

			var stopWatch1 = Stopwatch.StartNew();
			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "CHT55");
			AssertEquals(1, feedbacks.Count);
			AssertEquals(1, feedbacks[0].AllContexts.Count);
			AssertEquals(0, feedbacks[0].OtherTranslations.Count);
			stopWatch1.Stop();

			TranslationFeedbackFactory.ClearCaptionCache();
			for (int i = 101; i < 100000; i++)
			{
				AddMockTranslation("X" + i, "ENG" + i, "CHT" + i);
			}

			// warm up the cache
			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "CHT1");
			AssertEquals(1, feedbacks.Count);
			AssertEquals(1, feedbacks[0].AllContexts.Count);
			AssertEquals(0, feedbacks[0].OtherTranslations.Count);

			var stopWatch2 = Stopwatch.StartNew();
			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "CHT55555");
			AssertEquals(1, feedbacks.Count);
			AssertEquals(1, feedbacks[0].AllContexts.Count);
			AssertEquals(0, feedbacks[0].OtherTranslations.Count);
			stopWatch2.Stop();

			Assert(string.Format("Looking up strings by caption using the cache should be a constant time operation, but it took more than four times as long with 100000 strings ({0}) as with 100 strings ({1})", stopWatch2.ElapsedMilliseconds, stopWatch1.ElapsedMilliseconds), stopWatch2.ElapsedTicks <= stopWatch1.ElapsedTicks * 4);
		}

		public void TestRelatedCollectionsAndSaveResourceStrings()
		{
			AddMockTranslation(
				new ResourceStringData("kdg1", "Dog", "Dog Dog", "Dog Dog Dog", "Dog Dog Dog Dog"),
				new ResourceStringData("kdg1", "狗", "狗狗", "狗狗狗", "狗狗狗狗"));
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdg3", "Dog", "獒");
			AddMockTranslation("kdg4", "Canis Familiaris", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgn3", "DRAGON", "虯");

			TopLevelTranslationFeedbackCollection feedbacks;
			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog", "Dog Dog", "Dog Dog Dog", "Dog Dog Dog Dog"));
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(3, feedbacks[0].AllContexts.Count);
			AssertEquals("kdg1", feedbacks[0].AllContexts[0].XQ_ResourceStringKey);
			AssertEquals(true, feedbacks[0].AllContexts[0].Update);
			Assert((feedbacks[0].AllContexts[1].XQ_ResourceStringKey == "kdg" && feedbacks[0].AllContexts[2].XQ_ResourceStringKey == "kdg2") ||
					 (feedbacks[0].AllContexts[2].XQ_ResourceStringKey == "kdg" && feedbacks[0].AllContexts[1].XQ_ResourceStringKey == "kdg2"));
			AssertEquals(false, feedbacks[0].AllContexts[1].Update);
			AssertEquals(false, feedbacks[0].AllContexts[2].Update);
			AssertEquals(1, feedbacks[0].OtherTranslations.Count);
			AssertEquals("kdg3", feedbacks[0].OtherTranslations[0].ResourceStringKey);
			AssertEquals(1, feedbacks[1].AllContexts.Count);
			AssertEquals("kdg1", feedbacks[1].AllContexts[0].XQ_ResourceStringKey);
			AssertEquals(0, feedbacks[1].OtherTranslations.Count);
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
			AssertEquals("犬", checkedOutStrings[0].HD_ShortCaption);
			AssertEquals("狗狗狗", checkedOutStrings[0].HD_Caption);

			foreach (StmTranslationFeedback feedback in feedbacks)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			}
			feedbacks.Factory.Save();

			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg", "Dog"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(3, feedbacks[0].AllContexts.Count);
			AssertEquals("kdg", feedbacks[0].AllContexts[0].XQ_ResourceStringKey);
			feedbacks[0].AllContexts[1].Update = true;
			feedbacks[0].AllContexts[2].Update = true;
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(3, checkedOutStrings.Length);
			AssertEquals("犬", ResourceStringsFactory.Lookup(Core.SharedConstants.Languages.ChineseTraditional, "kdg2", true).HD_Caption);

			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg", "Dog"));
			AssertEquals(1, feedbacks.Count);
			AssertEquals("Cache should be modified", 3, feedbacks[0].AllContexts.Count);
			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "狗");
			AssertEquals("Cache should be modified", 1, feedbacks.Count);
			AssertEquals("Cache should be modified", "kdg4", feedbacks[0].ResourceStringKey);

			feedbacks = new TopLevelTranslationFeedbackCollection(Factory);
			feedbacks.Load();
			foreach (StmTranslationFeedback feedback in feedbacks)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			}
			feedbacks.Factory.Save();

			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog", "Dog Dog", "Dog Dog Dog", "Dog Dog Dog Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks[1].XT_SuggestedTranslation = "犬犬";
			feedbacks[2].XT_SuggestedTranslation = "犬犬犬";
			feedbacks[3].XT_SuggestedTranslation = "犬犬犬犬";
			feedbacks.Factory.Save();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals("犬", checkedOutStrings[0].HD_ShortCaption);
			AssertEquals("犬犬", checkedOutStrings[0].HD_MidCaption);
			AssertEquals("犬犬犬", checkedOutStrings[0].HD_Caption);
			AssertEquals("犬犬犬犬", checkedOutStrings[0].HD_FullDescription);

			foreach (StmTranslationFeedback feedback in feedbacks)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			}
			feedbacks.Factory.Save();

			ClearRecentlyUsed();
			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, "狗");
			AssertEquals(2, feedbacks.Count);
			AssertEquals(true, feedbacks[0].AllContexts.All(c => ((StmTranslationFeedbackResource)c).Update));
			AssertEquals(true, feedbacks[1].AllContexts.All(c => ((StmTranslationFeedbackResource)c).Update));
		}

		public void TestSearch()
		{
			AddMockTranslation(
				new ResourceStringData("f1", "Fish", "Fish Fish", "Fish Fish Fish", "Fish Fish Fish Fish"),
				new ResourceStringData("f1", "魚", "魚魚", "魚魚魚", "魚魚魚魚"));
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "dog", "狗");
			AddMockTranslation("kdgfsh", "Dog Fish", "狗魚");
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdrgnfsh", "Dragon Fish", "龍魚");
			AddMockTranslation("kmprrfsh", "Emperor Fish", "龍魚");
			ENG.Put("kcw", new ResourceStringData("kcw", "Cow"));

			StmTranslationFeedbackCollection searchResults;

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "龍", TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional });
			AssertEquals(3, searchResults.Count);
			SortByKey(searchResults);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
			AssertFeedback("kdrgnfsh", "Dragon Fish", "龍魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[1]);
			AssertFeedback("kmprrfsh", "Emperor Fish", "龍魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[2]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "龍", SearchSource = true, SourceText = "Emperor", TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional });
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kmprrfsh", "Emperor Fish", "龍魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "龍.+", UseRegularExpressions = true, TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional });
			AssertEquals(2, searchResults.Count);
			AssertFeedback("kdrgnfsh", "Dragon Fish", "龍魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
			AssertFeedback("kmprrfsh", "Emperor Fish", "龍魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[1]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "龍.+", UseRegularExpressions = false, TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional });
			AssertEquals(0, searchResults.Count);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "魚魚", TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional });
			AssertEquals(3, searchResults.Count);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
			AssertFeedback("f1", "Fish Fish Fish", "魚魚魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[1]);
			AssertFeedback("f1", "Fish Fish Fish Fish", "魚魚魚魚", ResourceStringDataLevels.Codes.FullDescription, TranslationFeedbackMatchTypes.Codes.None, searchResults[2]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { SearchTarget = false, SearchSource = true, SourceText = "dog" });
			AssertEquals(3, searchResults.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
			AssertFeedback("kdg2", "dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[1]);
			AssertFeedback("kdgfsh", "Dog Fish", "狗魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[2]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { SearchTarget = false, SearchSource = true, SourceText = "do.", UseRegularExpressions = true });
			AssertEquals(3, searchResults.Count);
			AssertFeedback("kdg", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
			AssertFeedback("kdg2", "dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[1]);
			AssertFeedback("kdgfsh", "Dog Fish", "狗魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[2]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { SearchTarget = false, SearchSource = true, SourceText = "dog", MatchCase = true });
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kdg2", "dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { SearchTarget = false, SearchSource = true, SourceText = "do.", UseRegularExpressions = true, MatchCase = true });
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kdg2", "dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "Cow" });
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kcw", "Cow", "Cow", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "魚魚", TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional, Replace = true, ReplacementText = "魚魚s" });
			AssertEquals(3, searchResults.Count);
			AssertEquals("魚魚s", searchResults[0].XT_SuggestedTranslation);
			AssertEquals("魚魚s魚", searchResults[1].XT_SuggestedTranslation);
			AssertEquals("魚魚s魚魚s", searchResults[2].XT_SuggestedTranslation);

			searchResults = TranslationFeedbackFactory.Search(Factory, new TranslationSearchCriteria() { TargetText = "魚魚", TargetLanguage = Core.SharedConstants.Languages.ChineseTraditional, MatchWord = true });
			AssertEquals(1, searchResults.Count);
			AssertFeedback("f1", "Fish Fish", "魚魚", ResourceStringDataLevels.Codes.MediumCaption, TranslationFeedbackMatchTypes.Codes.None, searchResults[0]);
		}

		[ExpectNoExceptions]
		public void TestWithUnavailableData()
		{
			var searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseSimplified, new ResourceStringData("XXX", "Nothing"));
			AssertEquals(0, searchResults.Count);
			searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseSimplified, new ResourceStringData("XXX", "Nothing"), "Nothing");
			AssertEquals(0, searchResults.Count);
			searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseSimplified, ResString.GetMultilingualString("XXX", "Nothing"));
			AssertEquals(0, searchResults.Count);
			searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseSimplified, MultilingualString.Join(" , ", null, ResString.GetMultilingualString("XXX", "Nothing")));
			AssertEquals(0, searchResults.Count);
		}

		public void TestApproveInvalidResourceDoesNotBreakCache()
		{
			var feedback = StmTranslationFeedback.New(Factory, new HelpDataString() { HD_Language = Res.DefaultLanguage, HD_Code = "XXX", HD_Caption = "Dog" }, new HelpDataString() { HD_Language = Core.SharedConstants.Languages.ChineseTraditional, HD_Code = "XXX", HD_Caption = "狗" }, TranslationFeedbackMatchTypes.Codes.Exact);
			feedback.XT_SuggestedTranslation = "犬";
			feedback.AllContexts[0].Update = true;
			feedback.Factory.Save();

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var feedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				feedbacks.Load();
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				feedbacks.Factory.Save();

				var searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "犬");
				AssertEquals(0, searchResults.Count);
			}
		}

		public void TestCheckedOutStringWithoutFeedbackEntry()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			ResourceStringsFactory.Save("TST", new HelpDataString[] { new HelpDataString() { HD_Code = "kdg1", HD_Language = Core.SharedConstants.Languages.ChineseTraditional, HD_Caption = "犬" } });
			var searchResults = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "犬");
			AssertEquals(1, searchResults.Count);
		}

		public void TestGetForWeb()
		{
			AddMockTranslation("kdrgn", "Dragon", "龍");
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdgfsh", "Dog Fish", "狗魚");
			AddMockTranslation("kdgx", "Dog {0}", "狗{0}");
			AddMockTranslation("kfsh", "Fish", "魚");

			var usedKeys = new string[] { "kdrgn", "kdg", "kdgx", "kfsh" };

			var searchResults = TranslationFeedbackFactory.GetForWeb(Factory, Core.SharedConstants.Languages.ChineseTraditional, "龍", usedKeys);
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, searchResults[0]);

			searchResults = TranslationFeedbackFactory.GetForWeb(Factory, Core.SharedConstants.Languages.ChineseTraditional, "龍:", usedKeys);
			AssertEquals(1, searchResults.Count);
			AssertFeedback("kdrgn", "Dragon", "龍", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, searchResults[0]);

			searchResults = TranslationFeedbackFactory.GetForWeb(Factory, Core.SharedConstants.Languages.ChineseTraditional, "狗魚", usedKeys);
			AssertEquals(2, searchResults.Count);
			SortByKey(searchResults);
			AssertFeedback("kdgx", "Dog {0}", "狗{0}", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, searchResults[0]);
			AssertFeedback("kfsh", "Fish", "魚", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, searchResults[1]);
		}

		public void TestResourceStringDatasWithAccelerators()
		{
			AddMockTranslation(
				new ResourceStringData("d1", "Desc1", "Desc2", "&Edit", "Desc3"),
				new ResourceStringData("d1", "test1", "test2", "test3", "test4"));
			AssertEquals("Accelerator keys", 4, TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.EnglishAmerican, new ResourceStringData("d1", "Desc1", "Desc2", "&Edit", "Desc3"), "Edit", true).Count);
		}
	}
}
