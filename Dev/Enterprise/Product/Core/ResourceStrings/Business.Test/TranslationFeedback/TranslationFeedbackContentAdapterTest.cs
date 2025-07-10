using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using WTG.TranslationMemoryExchange;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	sealed class TranslationFeedbackContentAdapterTest : TranslationFeedbackTestCase
	{
		public void TestImportTranslationFeedbackWithoutFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				AssertNoExceptionThrown(
					() => TranslationFeedbackContentAdapter.Import(Core.SharedConstants.Languages.ChineseTraditional, tempDirectory));
			}
		}

		public void TestImportTranslationFeedbackStartsWithWhiteSpace()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", " Dog", " 狗");
				var resourceStringData = Res.GetData("kdg1", " Dog");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, resourceStringData);
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export(Core.SharedConstants.Languages.ChineseTraditional, tempDirectory);
				TranslationFeedbackContentAdapter.Import(Core.SharedConstants.Languages.ChineseTraditional, tempDirectory);

				var source = ResourceStringsFactory.Lookup(Res.DefaultLanguage, resourceStringData.Key);
				var result = ResourceStringsFactory.LookupWithLanguageFallback(Core.SharedConstants.Languages.ChineseTraditional, resourceStringData.Key);
				AssertEquals(" 犬", result.HD_Caption);
			}
		}

		public void TestApprovedFeedbackInMatchingLanguage()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertXMLEquals((@$"
<?xml version=""1.0"" encoding=""utf-8""?>
<tmx version=""1.4"">
	<header creationtool=""{Core.Constants.ProductName} Translation Feedback"" creationtoolversion=""" + ReleaseInfo.Instance.VersionNumber.ToString() + @$""" datatype=""Text"" segtype=""sentence"" o-tmf=""{Core.Constants.ProductName} Translation Feedback"" adminlang=""en-US"" srclang=""en-US"" />
	<body>
		<tu>
			<prop type=""x-CW1-ResourceStringContext"">CAP:kdg1</prop>
			<prop type=""x-CW1-OriginalTranslation"">狗</prop>
			<tuv xml:lang=""en-US"">
				<seg>Dog</seg>
			</tuv>
			<tuv xml:lang=""zh-HK"">
				<seg>犬</seg>
			</tuv>
		</tu>
	</body>
</tmx>").Replace("\r", "").Replace("\n", "").Replace("\t", ""),
			File.ReadAllText(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), Encoding.UTF8));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestApprovedFeedbackInDifferentLanguageNotExported()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-CN", tempDirectory);

				AssertEquals(0, Directory.GetFiles(tempDirectory).Length);
			}
		}

		public void TestApprovedFeedbackInDifferentApplicationNotExported()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseSimplified, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks[0].XT_Application = TranslationFeedbackApplicationsList.Codes.Glow;
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export(Core.SharedConstants.Languages.ChineseSimplified, tempDirectory);

				AssertEquals(0, Directory.GetFiles(tempDirectory).Length);
			}
		}

		public void TestNonApprovedFeedbackNotExported()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				foreach (CodeDescriptionPair status in new TranslationFeedbackStatusList())
				{
					if (status.Code != TranslationFeedbackStatusList.Codes.Approved)
					{
						AddMockTranslation("k" + status.Code, status.Code, status.Code + Core.SharedConstants.Languages.ChineseTraditional);
						var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("k" + status.Code, status.Code));
						feedbacks[0].XT_SuggestedTranslation = status.Code + "NEW";
						feedbacks[0].XT_Status = status.Code;
						feedbacks.Factory.Save();
					}
				}

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertEquals(0, Directory.GetFiles(tempDirectory).Length);
			}
		}

		public void TestMultipleFeedbackOnDifferentLevelsWithSameKey()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation(
					new ResourceStringData("kdg1", "Short Dog", "", "Caption Dog", "Full Description Dog"),
					new ResourceStringData("kdg1", "Short 狗", "", "Caption 狗", "Full Description 狗"));
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("kdg1", "Short 狗", "", "Caption 狗", "Full Description 狗"));
				foreach (StmTranslationFeedback feedback in feedbacks)
				{
					feedback.XT_SuggestedTranslation = feedback.XT_OriginalTranslation.Replace("狗", "犬");
				}
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.ShortCaption + ":kdg1" }, "zh-HK", "Short Dog", "Short 狗", "Short 犬"),
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Caption Dog", "Caption 狗", "Caption 犬"),
					new TmxTu(new [] { ResourceStringDataLevels.Codes.FullDescription + ":kdg1" }, "zh-HK", "Full Description Dog", "Full Description 狗", "Full Description 犬"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestFeedbackOnPartialLevelOfString()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation(
					new ResourceStringData("kdg1", "Short Dog", "", "Caption Dog", "Full Description Dog"),
					new ResourceStringData("kdg1", "Short 狗", "", "Caption 狗", "Full Description 狗"));
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("kdg1", "Short 狗", "", "Caption 狗", "Full Description 狗"));
				((StmTranslationFeedback)feedbacks.Single(f => ((StmTranslationFeedback)f).ResourceStringLevel == ResourceStringDataLevels.Codes.FullDescription)).XT_SuggestedTranslation = "Full Description 犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.FullDescription + ":kdg1" }, "zh-HK", "Full Description Dog", "Full Description 狗", "Full Description 犬")
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestMultipleFeedbackOnDifferentLevelsWithDifferentKeys()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation(
					new ResourceStringData("kdg1", "Dog", "", "", ""),
					new ResourceStringData("kdg1", "狗", "", "", ""));
				AddMockTranslation(
					new ResourceStringData("kdg2", "", "Dog", "", ""),
					new ResourceStringData("kdg2", "", "狗", "", ""));
				AddMockTranslation(
					new ResourceStringData("kdg3", "", "", "Dog", ""),
					new ResourceStringData("kdg3", "", "", "狗", ""));
				AddMockTranslation(
					new ResourceStringData("kdg4", "", "", "", "Dog"),
					new ResourceStringData("kdg4", "", "", "", "狗"));
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, "狗");
				foreach (StmTranslationFeedback feedback in feedbacks)
				{
					feedback.XT_SuggestedTranslation = "犬";
				}
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new []
					{
						ResourceStringDataLevels.Codes.ShortCaption + ":kdg1",
						ResourceStringDataLevels.Codes.MediumCaption + ":kdg2",
						ResourceStringDataLevels.Codes.Caption + ":kdg3",
						ResourceStringDataLevels.Codes.FullDescription + ":kdg4",
					}, "zh-HK", "Dog", "狗", "犬"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestSegmentation()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Sit Dog. Good Dog.", "坐狗。好狗。");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Sit Dog. Good Dog."));
				feedbacks[0].XT_SuggestedTranslation = "坐犬。好犬。";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Sit Dog.", "坐狗。", "坐犬。"),
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Good Dog.", "好狗。", "好犬。"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestSegmentationUsingAbbreviationsList()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("k", "Dest. Co. Tariff Level", "目的港 公司定价层级");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("k", "Dest. Co. Tariff Level"));
				feedbacks[0].XT_SuggestedTranslation = "目的港 公司价目表层级";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":k" }, "zh-HK", "Dest. Co. Tariff Level", "目的港 公司定价层级", "目的港 公司价目表层级"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestSegmentationWhenSingleSourceSegmentIsTranslatedToMulipleTargetSegments()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Sit dog, good dog.", "坐狗,好狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Sit dog, good dog."));
				feedbacks[0].XT_SuggestedTranslation = "坐犬。好犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Sit dog, good dog.", "坐狗,好狗", "坐犬。好犬"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestSegmentationWithNewLine()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Sit Dog\r\n\r\nGood Dog", "坐狗\r\n\r\n好狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Sit Dog\r\n\r\nGood Dog"));
				feedbacks[0].XT_SuggestedTranslation = "坐犬\r\n\r\n好犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.FullDescription + ":kdg1" }, "zh-HK", "Sit Dog", "坐狗", "坐犬"),
					new TmxTu(new [] { ResourceStringDataLevels.Codes.FullDescription + ":kdg1" }, "zh-HK", "Good Dog", "好狗", "好犬"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestFeedbackWithNonExistingTranslation()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				ENG.Put("k", new ResourceStringData("k", "Dog"));
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("k", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":k" }, "zh-HK", "Dog", "Dog", "犬")
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestFeedbackOnInvalidatedTranslation()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Dog", "狗", "犬")
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				CHT.Put("kdg1", null);
				ClearFeedback();
				TranslationFeedbackContentAdapter.Import("zh-HK", tempDirectory);
				AssertEquals(Res.GetData("kdg1", "Dog"), GetCheckedOutStrings().Single());
			}
		}

		public void TestSegmentationWhenOriginalTranslationSingleSegmentIsTranslatedToMulipleTargetSegments()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Sit dog. Good dog.", "坐狗,好狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Sit dog. Good dog."));
				feedbacks[0].XT_SuggestedTranslation = "坐犬。好犬";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Sit dog. Good dog.", "坐狗,好狗", "坐犬。好犬"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestSegmentationWhenSegmentsDontMatch()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Sit dog. Good dog. Here, have a treat.", "坐狗。好狗在這裡。有一个待遇");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Sit dog. Good dog. Here, have a treat."));
				feedbacks[0].XT_SuggestedTranslation = "坐犬，好犬。有一個待遇";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new TmxTu(new [] { ResourceStringDataLevels.Codes.Caption + ":kdg1" }, "zh-HK", "Sit dog. Good dog. Here, have a treat.", "坐狗。好狗在這裡。有一个待遇", "坐犬，好犬。有一個待遇"),
				}, TmxReader.Read(Path.Combine(tempDirectory, "TranslationFeedback.tmx"), TranslationFeedbackApplicationsList.Codes.Cargowise));

				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestACChargeTypeFullDescription()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var eng = new ResourceStringData("k", "Type", string.Empty, "Charge Type", @"DSB - Disbursement.  Where 100% of the Revenue is to be Accrued or no Profit is expected. Can only be used for Job related charges.
MRG - Margin. Where there are Cost or Revenue or Both. Can only be used for Job related charges.  
REV - Revenue.  Where there are No Cost involved, pure Revenue only. Can be used for both Job related charges and Non-Job related charges. 
OVR - Overhead.  Where there are No Revenue involved, pure Overhead only. Can be used only for Non-Job related charges. 
NON – Non-Accruing.  Where there are Cost or Revenue or Both. Can only be used for Non-Job related charges. .");
				var chs = new ResourceStringData("k", "类型", string.Empty, "费用类型", @"DSB - 代收代付费用。当 100% 的收入为应计或没有预期收益。只能用于工作相关的费用。
MRG - 利润。当有成本或利润或两者时。只能用于工作相关的费用。  
REV - 收入。当没有成本牵涉时，纯粹只是收入。能用于工作相关及非工作相关费用。 
OVR - 日常开支。当没有收入牵涉时，纯粹只是日常开支。只能用于非工作相关的费用。 
NON – 非应计。当有成本或利润或两者时。只能用于非工作相关的费用。.");
				AddMockTranslation(eng, chs);
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, chs);
				var feedback = feedbacks.Cast<StmTranslationFeedback>().Single(f => f.PrimaryContext.XQ_ResourceStringLevel == "FUL");
				feedback.XT_SuggestedTranslation = @"DSB - 代收代付。适用于全部收入为应计或预期没有利润的情况。只能用于业务相关的费用。
MRG - 利润。适用于有成本或收入或两者皆有的情况。只能用于业务相关的费用。  
REV - 收入。适用于没有成本只有收入的情况。。可用于业务和非业务相关的费用。 
OVR - 间接费用。适用于没有收入只有间接费用的情况。只能用于非业务相关的费用。 
NON – 非应计项目。适用于有成本或收入或两者皆有的情况。只能用于非工作相关的费用。.";
				feedbacks.Factory.Save();

				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);
				AssertImportMatchesFeedbackCheckedOutStrings(tempDirectory);
			}
		}

		public void TestAppendToTranslationMultipleTimes()
		{
			using (var tempDirectory = new TempDirectory())
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("k", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("k", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "狗称";
				feedbacks.Factory.Save();

				var feedbackCheckedOutStrings = GetCheckedOutStrings();
				TranslationFeedbackContentAdapter.Export("zh-HK", tempDirectory);
				ClearFeedback();
				CHT.Put("k", new ResourceStringData("k", "狗称"));
				TranslationFeedbackContentAdapter.Import("zh-HK", tempDirectory);
				AssertContainsExactElementsInAnyOrder(feedbackCheckedOutStrings, GetCheckedOutStrings());
			}
		}

		void AssertImportMatchesFeedbackCheckedOutStrings(string contentDirectory)
		{
			var feedbackCheckedOutStrings = GetCheckedOutStrings();
			AssertNotEquals(0, feedbackCheckedOutStrings.Length);
			ClearFeedback();
			AssertEquals(0, GetCheckedOutStrings().Length);

			TranslationFeedbackContentAdapter.Import("zh-HK", contentDirectory);

			AssertContainsExactElementsInAnyOrder(feedbackCheckedOutStrings, GetCheckedOutStrings());
		}

		ResourceStringData[] GetCheckedOutStrings()
		{
			var query = new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true);
			query.AddToFilter(HelpDataStringSchema.HD_Language, Core.SharedConstants.Languages.ChineseTraditional);
			return ResourceStringsFactory.Load(query).Select(hd => hd.ToResourceStringData()).ToArray();
		}

		static void ClearFeedback()
		{
			var factory = new BusinessObjectFactory();
			var feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.AddRange(factory.Load<StmTranslationFeedback>(new ZQuery()));
			foreach (StmTranslationFeedback feedback in feedbacks)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			}
			factory.Save();
		}

		protected override void SetUp()
		{
			ShouldSwitchLanguage = false;
			base.SetUp();
		}
	}
}
