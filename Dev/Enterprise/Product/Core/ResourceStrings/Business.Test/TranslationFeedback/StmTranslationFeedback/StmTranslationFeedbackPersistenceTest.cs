using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	sealed class StmTranslationFeedbackPersistenceTest : TranslationFeedbackTestCase
	{
		public void TestSaveChangedItemsOnly()
		{
			AddMockTranslation(
				new ResourceStringData("kdg1", "Dog", "Dog Dog", "Dog Dog Dog", "Dog Dog Dog Dog"),
				new ResourceStringData("kdg1", "狗", "狗狗", "狗狗狗", "狗狗狗狗"));
			AddMockTranslation("kdg2", "Dog", "獒");

			var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog", "Dog Dog", "Dog Dog Dog", "Dog Dog Dog Dog"));
			AssertEquals(4, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(1, feedbacks[0].OtherTranslations.Count);
			AssertFeedback("kdg2", "Dog", "獒", ResourceStringDataLevels.Codes.Caption, string.Empty, feedbacks[0].OtherTranslations[0]);
			feedbacks[0].XT_SuggestedTranslation = "犬";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var allFeedbacks = newFactory.Load<StmTranslationFeedback>(new ZQuery());
			AssertEquals(1, allFeedbacks.Length);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.ShortCaption, TranslationFeedbackMatchTypes.Codes.Exact, allFeedbacks[0]);
		}

		public void TestAllContextsNeedToMatchApplication()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdg3", "Dog", "狗");

			var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(1, feedbacks.Count);
			feedbacks[0].XT_Application = "GLW";
			AssertEquals(1, feedbacks[0].AllContexts.Count);

			feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(1, feedbacks.Count);
			feedbacks[0].XT_Application = "CW1";
			AssertEquals(3, feedbacks[0].AllContexts.Count);
		}

		public void TestSaveUpdateContexts()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			AddMockTranslation("kdg3", "Dog", "狗");
			var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(3, feedbacks[0].AllContexts.Count);
			AssertEquals(false, feedbacks[0].AllContexts[0].ReadOnly);
			AssertEquals(false, feedbacks[0].AllContexts[1].ReadOnly);
			AssertEquals(false, feedbacks[0].AllContexts[2].ReadOnly);
			feedbacks[0].XT_SuggestedTranslation = "犬";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(newFactory);
			feedbacks.AddRange(newFactory.Load<StmTranslationFeedback>(new ZQuery()));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(1, feedbacks[0].SavedContexts.Count);
			AssertEquals("kdg1", feedbacks[0].SavedContexts[0].XQ_ResourceStringKey);
			AssertEquals(ResourceStringDataLevels.Codes.Caption, feedbacks[0].SavedContexts[0].XQ_ResourceStringLevel);
			AssertEquals(TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0].SavedContexts[0].XQ_MatchType);
			AssertEquals(3, feedbacks[0].AllContexts.Count);
			AssertEquals(true, feedbacks[0].AllContexts[0].Update);
			AssertEquals(false, feedbacks[0].AllContexts[1].Update);
			AssertEquals(false, feedbacks[0].AllContexts[2].Update);
			AssertEquals(true, feedbacks[0].AllContexts[0].ReadOnly);
			AssertEquals(false, feedbacks[0].AllContexts[1].ReadOnly);
			AssertEquals(false, feedbacks[0].AllContexts[2].ReadOnly);
			feedbacks[0].AllContexts[1].Update = true;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(newFactory);
			feedbacks.AddRange(newFactory.Load<StmTranslationFeedback>(new ZQuery()));
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(2, feedbacks[0].SavedContexts.Count);
			AssertEquals("kdg1", feedbacks[0].SavedContexts[0].XQ_ResourceStringKey);
			AssertEquals("kdg2", feedbacks[0].SavedContexts[1].XQ_ResourceStringKey);
			AssertEquals(3, feedbacks[0].AllContexts.Count);
			AssertEquals(true, feedbacks[0].AllContexts[0].Update);
			AssertEquals(true, feedbacks[0].AllContexts[1].Update);
			AssertEquals(false, feedbacks[0].AllContexts[2].Update);
			AssertEquals(true, feedbacks[0].AllContexts[0].ReadOnly);
			AssertEquals(true, feedbacks[0].AllContexts[1].ReadOnly);
			AssertEquals(false, feedbacks[0].AllContexts[2].ReadOnly);
		}

		public void TestAutoApprovedOnMaster()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				var allFeedbacks = newFactory.Load<StmTranslationFeedback>(new ZQuery());
				AssertEquals(1, allFeedbacks.Length);
				AssertEquals(TranslationFeedbackStatusList.Codes.Approved, allFeedbacks[0].XT_Status);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals(Core.SharedConstants.Languages.ChineseTraditional, checkedOutStrings[0].HD_Language);
				AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
				AssertEquals("犬", checkedOutStrings[0].HD_Caption);
			}
		}

		public void TestSuggestedTranslationReadOnly()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
			feedbacks.Factory.Save();
			AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
			feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
			feedbacks.Factory.Save();
			AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
			feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
			feedbacks.Factory.Save();
			AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
			feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			feedbacks.Factory.Save();
			AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);

			feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.New;
			feedbacks.Factory.Save();
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
				feedbacks.Factory.Save();
				AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
				feedbacks.Factory.Save();
				AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
				feedbacks.Factory.Save();
				AssertEquals(true, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);

				feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg2", "Dog"));
				AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);
				AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
				feedbacks.Factory.Save();
				AssertEquals(false, feedbacks[0].XT_SuggestedTranslationInfo.ReadOnly);
			}
		}

		public void TestTranslationMaxLengthForTranslatableDataFields()
		{
			var key = CustomizableDataResourceStrings.GetCustomizableDataKey(RefCountrySchema.RN_Desc.Name, "Australia");
			ENG.Put(key, Res.GetData(key, "Australia"));
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData(key, "Australia"));
			AssertEquals(1, feedbacks.Count);
			AssertEquals(RefCountrySchema.RN_Desc.MaxLength, feedbacks[0].XT_SuggestedTranslationInfo.MaxLength);
			feedbacks[0].XT_SuggestedTranslation = "澳大利亚";
			feedbacks.Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var allFeedbacks = newFactory.Load<StmTranslationFeedback>(new ZQuery());
			AssertEquals(1, allFeedbacks.Length);
			AssertEquals(RefCountrySchema.RN_Desc.MaxLength, feedbacks[0].XT_SuggestedTranslationInfo.MaxLength);
		}

		public void TestTranslationMaxLengthForTranslatableDataFields2()
		{
			var key = CustomizableDataResourceStrings.GetCustomizableDataKey(RefCountryStatesSchema.RW_Description.Name, "Queensland");
			ENG.Put(key, Res.GetData(key, "Queensland"));
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData(key, "Queensland"));
			AssertEquals(1, feedbacks.Count);
			AssertEquals(200, feedbacks[0].XT_SuggestedTranslationInfo.MaxLength);
			feedbacks[0].XT_SuggestedTranslation = "昆士兰";
			feedbacks.Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var allFeedbacks = newFactory.Load<StmTranslationFeedback>(new ZQuery());
			AssertEquals(1, allFeedbacks.Length);
			AssertEquals(200, feedbacks[0].XT_SuggestedTranslationInfo.MaxLength);
		}

		public void TestStatusReadOnly()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			AddMockTranslation("kdg2", "Dog", "狗");
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(true, feedbacks[0].XT_StatusInfo.ReadOnly);

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				AssertEquals(false, feedbacks[0].XT_StatusInfo.ReadOnly);
			}
		}

		public void TestSettingEmptySuggestedTranslationRestoresSource()
		{
			AddMockTranslation(
				new ResourceStringData("k", "Dog", "", "Dog Dog", ""),
				new ResourceStringData("k", "狗", "", "狗狗", ""));
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("k", "狗", "", "狗狗", ""));
			feedbacks[0].XT_SuggestedTranslation = "";
			AssertEquals("Dog", feedbacks[0].XT_SuggestedTranslation);
		}

		public void TestSuggestedTranslationTrimmed()
		{
			AddMockTranslation(
				new ResourceStringData("k", "Dog", "", "Dog Dog", ""),
				new ResourceStringData("k", "狗", "", "狗狗", ""));
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("k", "狗", "", "狗狗", ""));
			feedbacks[0].XT_SuggestedTranslation = "   \r\n狗\r\n   ";
			AssertEquals("狗", feedbacks[0].XT_SuggestedTranslation);
		}
	}
}
