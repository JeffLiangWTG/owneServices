using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class TranslationFeedbackResourceStringsSourceTest : TestCaseWithFactory
	{
		public void TestNoFeedback()
		{
			AssertEquals(0, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll().Count());
		}

		public void TestLanguageSpecific()
		{
			ResourceStringCacheBuilder.Instance.ResetCache(SharedConstants.Languages.ChineseSimplified);
			ResourceStringCacheBuilder.Instance.ResetCache(SharedConstants.Languages.French);
			ResourceStringCacheBuilder.Instance.ResetCache(SharedConstants.Languages.German);

			SetupSystemDefined(Res.DefaultLanguage, new ResourceStringData("k1", "eng1"), new ResourceStringData("k2", "eng2"), new ResourceStringData("k3", "eng3"));
			SetupSystemDefined(SharedConstants.Languages.ChineseSimplified, new ResourceStringData("k1", "chs1"), new ResourceStringData("k2", "chs2"), new ResourceStringData("k3", "chs3"));
			SetupSystemDefined(SharedConstants.Languages.French, new ResourceStringData("k1", "frn1"), new ResourceStringData("k2", "frn2"), new ResourceStringData("k3", "frn3"));
			SetupSystemDefined(SharedConstants.Languages.German, new ResourceStringData("k1", "grm1"), new ResourceStringData("k2", "grm2"), new ResourceStringData("k3", "grm3"));

			AddFeedback(SharedConstants.Languages.ChineseSimplified, new ResourceStringData("k1", "chs1"), "chs1", "newchs1");
			AddFeedback(SharedConstants.Languages.ChineseSimplified, new ResourceStringData("k2", "chs2"), "chs2", "newchs2");
			AddFeedback(SharedConstants.Languages.French, new ResourceStringData("k1", "frn1"), "frn1", "newfrn1");
			AddFeedback(SharedConstants.Languages.French, new ResourceStringData("k3", "frn3"), "frn3", "newfrn3");
			AddFeedback(SharedConstants.Languages.German, new ResourceStringData("k1", "grm1"), "grm1", "newgrm1");
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "newchs1"), new ResourceStringData("k2", "newchs2") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll());
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "newfrn1"), new ResourceStringData("k3", "newfrn3") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.French).ReadAll());
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "newgrm1") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.German).ReadAll());
		}

		public void TestStatusSpecific()
		{
			SetupSystemDefined(Res.DefaultLanguage, new ResourceStringData("k1", "eng1"), new ResourceStringData("k2", "eng2"), new ResourceStringData("k3", "eng3"), new ResourceStringData("k4", "eng4"), new ResourceStringData("k5", "eng5"));
			SetupSystemDefined(Constants.Languages.ChineseSimplified, new ResourceStringData("k1", "chs1"), new ResourceStringData("k2", "chs2"), new ResourceStringData("k3", "chs3"), new ResourceStringData("k4", "chs4"), new ResourceStringData("k5", "chs5"));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k1", "chs1"), "chs1", "newchs1").XT_Status = TranslationFeedbackStatusList.Codes.New;
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k2", "chs2"), "chs2", "newchs2").XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k3", "chs3"), "chs3", "newchs3").XT_Status = TranslationFeedbackStatusList.Codes.Current;
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k4", "chs4"), "chs4", "newchs4").XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k5", "chs5"), "chs5", "newchs5").XT_Status = TranslationFeedbackStatusList.Codes.Approved;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k1", "newchs1"), new ResourceStringData("k5", "newchs5") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll());

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k5", "newchs5") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll());
			}
		}

		public void TestLevelSpecific()
		{
			SetupSystemDefined(Res.DefaultLanguage,
				new ResourceStringData("k1", "engsho1", "", "", ""),
				new ResourceStringData("k2", "", "engmed2", "", ""),
				new ResourceStringData("k3", "", "", "engcap3", ""),
				new ResourceStringData("k4", "", "", "", "engful4"));
			SetupSystemDefined(Constants.Languages.ChineseSimplified,
				new ResourceStringData("k1", "chssho1", "", "", ""),
				new ResourceStringData("k2", "", "chsmed2", "", ""),
				new ResourceStringData("k3", "", "", "chscap3", ""),
				new ResourceStringData("k4", "", "", "", "chsful4"));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k1", "chssho1", "", "", ""), "chssho1", "newchssho1");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k2", "", "chsmed2", "", ""), "chsmed2", "newchsmed2");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k3", "", "", "chscap3", ""), "chscap3", "newchscap3");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k4", "", "", "", "chsful4"), "chsful4", "newchsful4");
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] {
				new ResourceStringData("k1", "newchssho1", "", "", ""),
				new ResourceStringData("k2", "", "newchsmed2", "", ""),
				new ResourceStringData("k3", "", "", "newchscap3", ""),
				new ResourceStringData("k4", "", "", "", "newchsful4") },
				TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll()
			);
		}

		public void TestCreateAll()
		{
			SetupSystemDefined(Res.DefaultLanguage,
				new ResourceStringData("k1", "engsho1", "", "", ""),
				new ResourceStringData("k2", "", "engmed2", "", ""),
				new ResourceStringData("k3", "", "", "engcap3", ""),
				new ResourceStringData("k4", "", "", "", "engful4"));
			SetupSystemDefined(Constants.Languages.ChineseSimplified,
				new ResourceStringData("k1", "chssho1", "", "", ""),
				new ResourceStringData("k2", "", "chsmed2", "", ""),
				new ResourceStringData("k3", "", "", "chscap3", ""),
				new ResourceStringData("k4", "", "", "", "chsful4"));
			SetupSystemDefined(Constants.Languages.French,
				new ResourceStringData("k1", "frnsho1", "", "", ""),
				new ResourceStringData("k2", "", "frnmed2", "", ""),
				new ResourceStringData("k3", "", "", "frncap3", ""),
				new ResourceStringData("k4", "", "", "", "frnful4"));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k1", "chssho1", "", "", ""), "chssho1", "newchssho1");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k2", "", "chsmed2", "", ""), "chsmed2", "newchsmed2");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k3", "", "", "chscap3", ""), "chscap3", "newchscap3");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k4", "", "", "", "chsful4"), "chsful4", "newchsful4");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k1", "frnsho1", "", "", ""), "frnsho1", "newfrnsho1");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k2", "", "frnmed2", "", ""), "frnmed2", "newfrnmed2");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k3", "", "", "frncap3", ""), "frncap3", "newfrncap3");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k4", "", "", "", "frnful4"), "frnful4", "newfrnful4");
			Factory.Save();

			var res = TranslationFeedbackResourceStringsSource.CreateAll(new string[] { Constants.Languages.ChineseSimplified, Constants.Languages.French });
			AssertEquals(2, res.Count);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, res[Core.SharedConstants.Languages.ChineseSimplified].Source.Language);
			AssertContainsExactElementsInAnyOrder(new[] {
				new ResourceStringData("k1", "newchssho1", "", "", ""),
				new ResourceStringData("k2", "", "newchsmed2", "", ""),
				new ResourceStringData("k3", "", "", "newchscap3", ""),
				new ResourceStringData("k4", "", "", "", "newchsful4") }, res[Core.SharedConstants.Languages.ChineseSimplified].Data);
			AssertEquals(Core.SharedConstants.Languages.French, res[Core.SharedConstants.Languages.French].Source.Language);
			AssertContainsExactElementsInAnyOrder(new[] {
				new ResourceStringData("k1", "newfrnsho1", "", "", ""),
				new ResourceStringData("k2", "", "newfrnmed2", "", ""),
				new ResourceStringData("k3", "", "", "newfrncap3", ""),
				new ResourceStringData("k4", "", "", "", "newfrnful4") }, res[Core.SharedConstants.Languages.French].Data);
		}

		public void TestFallbacks()
		{
			SetupSystemDefined(Res.DefaultLanguage,
				new ResourceStringData("k1", "engsho1", "engmed1", "engcap1", "engful1"),
				new ResourceStringData("k2", "engsho2", "engmed2", "engcap2", "engful2"),
				new ResourceStringData("k3", "engsho3", "engmed3", "engcap3", "engful3"),
				new ResourceStringData("k4", "engsho4", "engmed4", "engcap4", "engful4"),
				new ResourceStringData("k5", "engsho5", "engmed5", "engcap5", "engful5"),
				new ResourceStringData("k6", "engsho6", "engmed6", "engcap6", "engful6"),
				new ResourceStringData("k7", "engsho7", "engmed7", "engcap7", "engful7")
				);
			SetupSystemDefined(Constants.Languages.ChineseSimplified,
				new ResourceStringData("k1", "chssho1", "chsmed1", "chscap1", "chsful1"),
				new ResourceStringData("k2", "chssho2", "chsmed2", "chscap2", "chsful2"),
				new ResourceStringData("k3", "chssho3", "chsmed3", "chscap3", "chsful3"),
				new ResourceStringData("k4", "chssho4", "chsmed4", "chscap4", "chsful4"),
				new ResourceStringData("k5", "chssho5", "chsmed5", "chscap5", "chsful5"),
				new ResourceStringData("k6", "chssho6", "chsmed6", "chscap6", "chsful6"),
				new ResourceStringData("k7", "chssho7", "chsmed7", "chscap7", "chsful7")
				);
			SetupSystemDefined(Constants.Languages.French);

			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k1", "chssho1", "chsmed1", "chscap1", "chsful1"), "chssho1", "newchssho1");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k2", "chssho2", "chsmed2", "chscap2", "chsful2"), "chsmed2", "newchsmed2");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k3", "chssho3", "chsmed3", "chscap3", "chsful3"), "chscap3", "newchscap3");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k4", "chssho4", "chsmed4", "chscap4", "chsful4"), "chsful4", "newchsful4");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k5", "chssho5", "chsmed5", "chscap5", "chsful5"), "chssho5", "newchssho5");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k5", "chssho5", "chsmed5", "chscap5", "chsful5"), "chscap5", "newchscap5");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k6", "chssho6", "chsmed6", "chscap6", "chsful6"), "chscap6", "newchscap6");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k6", "chssho6", "chsmed6", "chscap6", "chsful6"), "chsful6", "newchsful6");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k7", "chssho7", "chsmed7", "chscap7", "chsful7"), "chscap7", "newchscap7");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k7", "chssho7", "chsmed7", "chscap7", "chsful7"), "chssho7", "newchssho7");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k7", "chssho7", "chsmed7", "chscap7", "chsful7"), "chsmed7", "newchsmed7");
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k7", "chssho7", "chsmed7", "chscap7", "chsful7"), "chsful7", "newchsful7");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k1", "engsho1", "engmed1", "engcap1", "engful1"), "engsho1", "newfrnsho1");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k2", "engsho2", "engmed2", "engcap2", "engful2"), "engmed2", "newfrnmed2");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k3", "engsho3", "engmed3", "engcap3", "engful3"), "engcap3", "newfrncap3");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k4", "engsho4", "engmed4", "engcap4", "engful4"), "engful4", "newfrnful4");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k5", "engsho5", "engmed5", "engcap5", "engful5"), "engsho5", "newfrnsho5");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k5", "engsho5", "engmed5", "engcap5", "engful5"), "engcap5", "newfrncap5");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k6", "engsho6", "engmed6", "engcap6", "engful6"), "engcap6", "newfrncap6");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k6", "engsho6", "engmed6", "engcap6", "engful6"), "engful6", "newfrnful6");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k7", "engsho7", "engmed7", "engcap7", "engful7"), "engcap7", "newfrncap7");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k7", "engsho7", "engmed7", "engcap7", "engful7"), "engsho7", "newfrnsho7");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k7", "engsho7", "engmed7", "engcap7", "engful7"), "engmed7", "newfrnmed7");
			AddFeedback(Constants.Languages.French, new ResourceStringData("k7", "engsho7", "engmed7", "engcap7", "engful7"), "engful7", "newfrnful7");
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] {
				new ResourceStringData("k1", "newchssho1", "chsmed1", "chscap1", "chsful1"),
				new ResourceStringData("k2", "chssho2", "newchsmed2", "chscap2", "chsful2"),
				new ResourceStringData("k3", "chssho3", "chsmed3", "newchscap3", "chsful3"),
				new ResourceStringData("k4", "chssho4", "chsmed4", "chscap4", "newchsful4"),
				new ResourceStringData("k5", "newchssho5", "chsmed5", "newchscap5", "chsful5"),
				new ResourceStringData("k6", "chssho6", "chsmed6", "newchscap6", "newchsful6"),
				new ResourceStringData("k7", "newchssho7", "newchsmed7", "newchscap7", "newchsful7") },
			TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll());

			AssertContainsExactElementsInAnyOrder(new[] {
				new ResourceStringData("k1", "newfrnsho1", "engmed1", "engcap1", "engful1"),
				new ResourceStringData("k2", "engsho2", "newfrnmed2", "engcap2", "engful2"),
				new ResourceStringData("k3", "engsho3", "engmed3", "newfrncap3", "engful3"),
				new ResourceStringData("k4", "engsho4", "engmed4", "engcap4", "newfrnful4"),
				new ResourceStringData("k5", "newfrnsho5", "engmed5", "newfrncap5", "engful5"),
				new ResourceStringData("k6", "engsho6", "engmed6", "newfrncap6", "newfrnful6"),
				new ResourceStringData("k7", "newfrnsho7", "newfrnmed7", "newfrncap7", "newfrnful7") },
			TranslationFeedbackResourceStringsSource.Create(Constants.Languages.French).ReadAll());
		}

		public void TestEngSystemDefinedIsNotLoadedIfNotNeeded()
		{
			SetupSystemDefined(Res.DefaultLanguage, new ResourceStringData("k", "eng"));
			SetupSystemDefined(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "chs"));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "chs"), "chs", "newchs");
			Factory.Save();
			research.systemDefined.Remove(Res.DefaultLanguage);
			AssertContainsExactElementsInAnyOrder(new[] { new ResourceStringData("k", "newchs") }, TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll());
		}

		public void TestEditReason()
		{
			SetupSystemDefined(Res.DefaultLanguage, new ResourceStringData("k", "eng"));
			SetupSystemDefined(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "chs"));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "chs"), "chs", "newchs");
			Factory.Save();
			var data = TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll().Single();
			AssertEquals(EditReasons.Codes.TranslationFeedback, data.EditReason);
			var dataString = HelpDataString.CreateFromResourceStringData(data, Constants.Languages.ChineseSimplified);
			AssertExceptionThrown<InvalidOperationException>(() => ResourceStringsFactory.Save(EditReasons.Codes.TranslationFeedback, dataString));
		}

		public void TestEmptySuggestedTranslationOneLevelRestoresEngValue()
		{
			SetupSystemDefined(Res.DefaultLanguage, new ResourceStringData("k", "Short", "", "Caption", ""));
			SetupSystemDefined(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "短", "", "标题", ""));
			AddFeedback(Constants.Languages.ChineseSimplified, new ResourceStringData("k", "短", "", "标题", ""), "短", "");
			Factory.Save();
			var data = TranslationFeedbackResourceStringsSource.Create(Constants.Languages.ChineseSimplified).ReadAll().Single();
			AssertEquals(data, new ResourceStringData("k", "Short", "", "标题", ""));
		}

		public void TestBaseSystemLanguage()
		{
			var factory = new BusinessObjectFactory();

			var testLanguage1 = factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			factory.Save();

			var testLanguage2 = factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			factory.Save();

			var source = TranslationFeedbackResourceStringsSource.Create(testLanguage2.FullLanguageCode);
			AssertEquals("Language", testLanguage2.FullLanguageCode, source.Language);
			AssertEquals("BaseSystemLanguage", Res.DefaultLanguage, ((TranslationFeedbackResourceStringsSource)source).BaseSystemLanguage);
		}

		StmTranslationFeedback AddFeedback(string language, ResourceStringData originalData, string originalText, string suggestedText)
		{
			var feedback = TranslationFeedbackFactory.Get(Factory, language, originalData, originalText).Cast<StmTranslationFeedback>().Single(f => f.XT_OriginalTranslation == originalText);
			feedback.XT_SuggestedTranslation = suggestedText;
			return feedback;
		}

		void SetupSystemDefined(string language, params ResourceStringData[] data)
		{
			research.systemDefined.Add(language, new SimpleResourceStringCache(new MockSource(language, data)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			research = new MockResearch();
			rearchOverride = ResourceStringsResearch.OverrideInstance(research);
		}

		protected override void TearDown()
		{
			rearchOverride.Dispose();
			base.TearDown();
		}

		MockResearch research;
		IDisposable rearchOverride;

		class MockResearch : ResourceStringsResearch
		{
			public override ISimpleResourceStringCache GetSystemDefinedResourceStrings(string language)
			{
				return systemDefined[language];
			}

			public Dictionary<string, SimpleResourceStringCache> systemDefined = new Dictionary<string, SimpleResourceStringCache>();
		}

		class MockSource : ResourceStringSource
		{
			public MockSource(string language, params ResourceStringData[] data)
				: base(language)
			{
				this.data = data;
			}

			public override IEnumerable<ResourceStringData> ReadAll()
			{
				return data;
			}

			public override void WriteAll(IEnumerable<ResourceStringData> resources)
			{
				throw new NotImplementedException();
			}

			public ResourceStringData[] data;
		}
	}
}
