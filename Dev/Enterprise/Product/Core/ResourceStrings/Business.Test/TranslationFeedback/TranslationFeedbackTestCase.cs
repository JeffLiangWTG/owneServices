using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	public class TranslationFeedbackTestCase : TestCaseWithFactory
	{
		protected void SortByKey(StmTranslationFeedbackCollection feedbacks)
		{
			feedbacks.Sort(new Comparison<StmTranslationFeedback>(delegate(StmTranslationFeedback item1, StmTranslationFeedback item2)
			{ return item1.ResourceStringKey.CompareTo(item2.ResourceStringKey); }));
		}

		protected void AssertFeedback(string expectedKey, string expectedSource, string expectedTranslation, string expectedLevel, string expectedMatchType, StmTranslationFeedback actualEntry)
		{
			AssertFeedback(expectedKey, expectedSource, expectedTranslation, expectedTranslation, expectedLevel, expectedMatchType, actualEntry);
		}

		protected void AssertFeedback(string expectedKey, string expectedSource, string expectedOriginalTranslation, string expectedSuggestedTranslation, string expectedLevel, string expectedMatchType, StmTranslationFeedback actualEntry)
		{
			AssertEquals("ResourceStringKey", expectedKey, actualEntry.ResourceStringKey);
			AssertEquals("Source", expectedSource, actualEntry.XT_Source);
			AssertEquals("OriginalTranslation", expectedOriginalTranslation, actualEntry.XT_OriginalTranslation);
			AssertEquals("SuggestedTranslation", expectedSuggestedTranslation, actualEntry.XT_SuggestedTranslation);
			AssertEquals("ResourceStringLevel", expectedLevel, actualEntry.ResourceStringLevel);
			AssertEquals("MatchType", expectedMatchType, actualEntry.MatchType);
			AssertEquals("HasChanges", false, actualEntry.HasChanges);
		}

		protected void AddMockTranslation(string key, string english, string translation)
		{
			AddMockTranslation(new ResourceStringData(key, english), new ResourceStringData(key, translation));
		}

		protected void AddMockTranslation(ResourceStringData english, ResourceStringData translation)
		{
			ENG.Put(english.Key, english);
			CHT.Put(translation.Key, translation);
		}

		protected void ClearRecentlyUsed()
		{
			for (int i = 0; i < 1000; i++)
			{
				Res.GetData(i.ToString(), i.ToString());
			}
		}

		protected override void SetUp()
		{
			Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseTraditional).ResetCache();
			mockSources = ResourceStringsFactory.MockSources();
			ENG = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			CHT = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.ChineseTraditional);
			if (ShouldSwitchLanguage)
			{
				switchLanguage = Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional);
			}
			ClearRecentlyUsed();
			TranslationFeedbackFactory.ClearCaptionCache();
			ClearData();
			base.SetUp();
		}

		void ClearData()
		{
			Db.Connection.ExecuteNonQuery("delete from " + StmTranslationFeedbackResourceSchema.Constants.SqlSchemaName + "." + StmTranslationFeedbackResourceSchema.Constants.TableName);
			Db.Connection.ExecuteNonQuery("delete from " + StmTranslationFeedbackSchema.Constants.SqlSchemaName + "." + StmTranslationFeedbackSchema.Constants.TableName);
			Db.Connection.ExecuteNonQuery("delete from " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName);
			Db.Connection.ExecuteNonQuery("delete from " + EDIInterchangeSchema.Constants.SqlSchemaName + "." + EDIInterchangeSchema.Constants.TableName);
		}

		protected void ResetConnection()
		{
			TestConnection.RollbackTransaction();
			TestConnection.BeginTransaction();
			ClearData();
		}

		protected override void TearDown()
		{
			mockSources.Dispose();
			switchLanguage?.Dispose();
			TranslationFeedbackFactory.ClearCaptionCache();
			base.TearDown();
		}

		protected bool ShouldSwitchLanguage { get; set; } = true;

		IDisposable mockSources;
		IDisposable switchLanguage;
		protected IMockResourceStringCache ENG;
		protected IMockResourceStringCache CHT;
	}
}
