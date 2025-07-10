using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCommunityProtectionRisk))]
	sealed class CMRCommunityProtectionRiskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetMatchingProfilesExcludesNotInTariff()
		{
			var profileWithNotTariffAndStat = CMRCommunityProtectionProfile.New(Factory);
			profileWithNotTariffAndStat.CP_TariffClassificationNumberfield = "NOT=000000";
			profileWithNotTariffAndStat.CP_StatisticalClassificationCodefield = "NOT=00";
			profileWithNotTariffAndStat.CP_CommunityProtectionRiskIdentifier = 500;

			var profileWithTariff = CMRCommunityProtectionProfile.New(Factory);
			profileWithTariff.CP_TariffClassificationNumberfield = "0000";
			profileWithTariff.CP_CommunityProtectionRiskIdentifier = 500;

			var dummyAttachee = Factory.New<DummyLineAttachee>();
			CPQuestionKeys cPQuestionKey = new CPQuestionKeys();
			cPQuestionKey.TariffNumber = "00000000";
			cPQuestionKey.StatCode = "00";
			dummyAttachee.CPQuestionKeyExposed = cPQuestionKey;

			Factory.Save();

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);
			AssertEquals("No record should be there", 0, result.Length);
		}

		public void TestWithNotInTariffAndDifferentStat()
		{
			var profileWithNotTariffAndDifferentStat = CMRCommunityProtectionProfile.New(Factory);
			profileWithNotTariffAndDifferentStat.CP_TariffClassificationNumberfield = "NOT=000000";
			profileWithNotTariffAndDifferentStat.CP_StatisticalClassificationCodefield = "NOT=01";
			profileWithNotTariffAndDifferentStat.CP_CommunityProtectionRiskIdentifier = 500;

			var profileWithTariff = CMRCommunityProtectionProfile.New(Factory);
			profileWithTariff.CP_TariffClassificationNumberfield = "0000";
			profileWithTariff.CP_CommunityProtectionRiskIdentifier = 500;

			var dummyAttachee = Factory.New<DummyLineAttachee>();
			var cPQuestionKey = new CPQuestionKeys();
			cPQuestionKey.TariffNumber = "00000000";
			cPQuestionKey.StatCode = "00";
			dummyAttachee.CPQuestionKeyExposed = cPQuestionKey;

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);
			AssertEquals("One Profile should be there", 1, result.Length);
			AssertEquals("Profile with tariff should be there", profileWithTariff, result[0]);
		}

		public void TestLoadWithLodgementQuestion()
		{
			var question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 500;

			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 401;
			risk1.CK_StartDate = new ZDateTime(2001, 1, 1);
			risk1.CK_EndDate = new ZDateTime(2001, 12, 31);
			risk1.CK_LodgementQuestionIdentifier = 500;

			var risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 401;
			risk2.CK_StartDate = new ZDateTime(2002, 1, 1);
			risk2.CK_EndDate = ZDateTime.Empty;
			risk2.CK_LodgementQuestionIdentifier = 500;

			var risk3 = CMRCommunityProtectionRisk.New(Factory);
			risk3.CK_Identifier = 401;
			risk3.CK_StartDate = new ZDateTime(2002, 1, 1);
			risk3.CK_EndDate = ZDateTime.Empty;
			risk3.CK_LodgementQuestionIdentifier = 501;

			var result = CMRCommunityProtectionRisk.Load(question);
			AssertEquals("There are two risks", 2, result.Length);
			AssertEquals("First risk", risk1, result[0]);
			AssertEquals("Second risk", risk2, result[1]);
		}

		public void TestGetRisks()
		{
			var risk1WithInDateRange = Factory.New<CMRCommunityProtectionRisk>();
			risk1WithInDateRange.CK_Identifier = 400;
			risk1WithInDateRange.CK_StartDate = dummyAttachee.SelectionDate;
			risk1WithInDateRange.CK_EndDate = dummyAttachee.SelectionDate;

			var risk1WithOutOfDateRange = Factory.New<CMRCommunityProtectionRisk>();
			risk1WithOutOfDateRange.CK_Identifier = 400;
			risk1WithOutOfDateRange.CK_StartDate = dummyAttachee.SelectionDate.AddDays(1);
			risk1WithOutOfDateRange.CK_EndDate = ZDateTime.Empty;

			var risk2WithInDateRange = Factory.New<CMRCommunityProtectionRisk>();
			risk2WithInDateRange.CK_Identifier = 401;
			risk2WithInDateRange.CK_StartDate = dummyAttachee.SelectionDate;
			risk2WithInDateRange.CK_EndDate = dummyAttachee.SelectionDate;

			SetUnmatchingProfiles(400);
			SetUnmatchingProfilesWithNOT(400);
			SetMatchingProfilesWithPartialInfo(400);
			SetMatchingProfilesWithPartialInfo(400);

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);
			AssertEquals("Ten Profiles records are matching", true, result.Length >= 10);

			var risks = CMRCommunityProtectionRisk.Load(dummyAttachee);
			AssertEquals("One Risk is retrived", 1, risks.Length);
			AssertEquals("One Risk is retrived", risk1WithInDateRange, risks[0]);
		}

		public void TestGetMatchingProfilesWhenNoMatchingRecordsAreThere()
		{
			SetUnmatchingProfiles();
			SetUnmatchingProfilesWithNOT();

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);

			bool noUnmatchingRecordFound = true;

			foreach (var profile in result)
			{
				noUnmatchingRecordFound
					&= profile.PK != cPProfileUnMatchingWithDifferentTariff.PK
					&& profile.PK != cPProfileUnMatchingWithDifferentStat.PK
					&& profile.PK != cPProfileUnMatchingWithDifferentOrigin.PK
					&& profile.PK != cPProfileUnMatchingWithDifferentMode.PK
					&& profile.PK != cPProfileUnMatchingWithDifferentNature.PK
					&& profile.PK != cPProfileUnMatchingWithNotInTariff.PK
					&& profile.PK != cPProfileUnMatchingWithNotInStat.PK
					&& profile.PK != cPProfileUnMatchingWithNotInOrigin.PK
					&& profile.PK != cPProfileUnMatchingWithNotInMode.PK
					&& profile.PK != cPProfileUnMatchingWithNotInNature.PK;
			}

			Assert(noUnmatchingRecordFound);
		}

		public void TestGetMatchingProfilesWhenMatchingRecordsWithPartialInfoAreThere()
		{
			SetUnmatchingProfiles();
			SetUnmatchingProfilesWithNOT();
			SetMatchingProfilesWithPartialInfo();

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);
			AssertEquals("Five records are matching", true, result.Length >= 5);

			bool hasSeenCPProfileMatchingWithPartialTariff = false;
			bool hasSeenCPProfileMatchingWithEmptyStat = false;
			bool hasSeenCPProfileMatchingWithEmptyOrigin = false;
			bool hasSeenCPProfileMatchingWithEmptyMode = false;
			bool hasSeenCPProfileMatchingWithEmptyNature = false;

			foreach (var profile in result)
			{
				hasSeenCPProfileMatchingWithPartialTariff |= profile.PK == cPProfileMatchingWithPartialTariff.PK;
				hasSeenCPProfileMatchingWithEmptyStat |= profile.PK == cPProfileMatchingWithEmptyStat.PK;
				hasSeenCPProfileMatchingWithEmptyOrigin |= profile.PK == cPProfileMatchingWithEmptyOrigin.PK;
				hasSeenCPProfileMatchingWithEmptyMode |= profile.PK == cPProfileMatchingWithEmptyMode.PK;
				hasSeenCPProfileMatchingWithEmptyNature |= profile.PK == cPProfileMatchingWithEmptyNature.PK;
			}

			Assert(hasSeenCPProfileMatchingWithPartialTariff);
			Assert(hasSeenCPProfileMatchingWithEmptyStat);
			Assert(hasSeenCPProfileMatchingWithEmptyOrigin);
			Assert(hasSeenCPProfileMatchingWithEmptyMode);
			Assert(hasSeenCPProfileMatchingWithEmptyNature);
		}

		public void TestGetMatchingProfilesWhenUnMatchingRecordsWithNotAreThere()
		{
			SetUnmatchingProfiles();
			SetUnmatchingProfilesWithNOT();

			var result = CMRCommunityProtectionRisk.GetMatchingProfiles(dummyAttachee);
			AssertEquals("Five records are matching", 0, result.Length);

			bool hasSeenCPProfileMatchingWithNotInTariff = false;
			bool hasSeenCPProfileMatchingWithNotInStat = false;
			bool hasSeenCPProfileMatchingWithNotInOrigin = false;
			bool hasSeenCPProfileMatchingWithNotInMode = false;
			bool hasSeenCPProfileMatchingWithNotInNature = false;

			foreach (var profile in result)
			{
				hasSeenCPProfileMatchingWithNotInTariff |= profile.PK == cPProfileUnMatchingWithNotInTariff.PK;
				hasSeenCPProfileMatchingWithNotInStat |= profile.PK == cPProfileUnMatchingWithNotInStat.PK;
				hasSeenCPProfileMatchingWithNotInOrigin |= profile.PK == cPProfileUnMatchingWithNotInOrigin.PK;
				hasSeenCPProfileMatchingWithNotInMode |= profile.PK == cPProfileUnMatchingWithNotInMode.PK;
				hasSeenCPProfileMatchingWithNotInNature |= profile.PK == cPProfileUnMatchingWithNotInNature.PK;
			}

			Assert(!hasSeenCPProfileMatchingWithNotInTariff);
			Assert(!hasSeenCPProfileMatchingWithNotInStat);
			Assert(!hasSeenCPProfileMatchingWithNotInOrigin);
			Assert(!hasSeenCPProfileMatchingWithNotInMode);
			Assert(!hasSeenCPProfileMatchingWithNotInNature);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRCommunityProtectionRisk.New(Factory);

		DummyLineAttachee dummyAttachee;
		CPQuestionKeys questionKey;

		protected override void SetUp()
		{
			base.SetUp();

			questionKey = new CPQuestionKeys();
			questionKey.TariffNumber = "0000.00.00";
			questionKey.StatCode = "00";
			questionKey.OriginCode = "XX";
			questionKey.Nature = "N10";
			questionKey.ModeOfTransport = "A";
			questionKey.HasValidOriginOrNatureOrModeOfTransport = true;

			dummyAttachee = Factory.New<DummyLineAttachee>();
			dummyAttachee.SelectionDateExposed = new ZDateTime(2005, 1, 1);
			dummyAttachee.CPQuestionKeyExposed = questionKey;
		}

		CMRCommunityProtectionProfile cPProfileMatchingWithPartialTariff;
		CMRCommunityProtectionProfile cPProfileMatchingWithEmptyStat;
		CMRCommunityProtectionProfile cPProfileMatchingWithEmptyOrigin;
		CMRCommunityProtectionProfile cPProfileMatchingWithEmptyMode;
		CMRCommunityProtectionProfile cPProfileMatchingWithEmptyNature;

		CMRCommunityProtectionProfile cPProfileUnMatchingWithNotInTariff;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithNotInStat;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithNotInOrigin;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithNotInMode;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithNotInNature;

		CMRCommunityProtectionProfile cPProfileUnMatchingWithDifferentTariff;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithDifferentStat;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithDifferentOrigin;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithDifferentMode;
		CMRCommunityProtectionProfile cPProfileUnMatchingWithDifferentNature;

		void SetMatchingProfilesWithPartialInfo()
		{
			cPProfileMatchingWithPartialTariff = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileMatchingWithPartialTariff.CP_TariffClassificationNumberfield = "000000";
			cPProfileMatchingWithPartialTariff.CP_StatisticalClassificationCodefield = "00";
			cPProfileMatchingWithPartialTariff.CP_OriginCountryCodefield = "XX";
			cPProfileMatchingWithPartialTariff.CP_ModeofTransportfield = "A";
			cPProfileMatchingWithPartialTariff.CP_LineNatureTypefield = "N10";

			cPProfileMatchingWithEmptyStat = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileMatchingWithEmptyStat.CP_TariffClassificationNumberfield = "0000";
			cPProfileMatchingWithEmptyStat.CP_StatisticalClassificationCodefield = "";
			cPProfileMatchingWithEmptyStat.CP_OriginCountryCodefield = "XX";
			cPProfileMatchingWithEmptyStat.CP_ModeofTransportfield = "A";
			cPProfileMatchingWithEmptyStat.CP_LineNatureTypefield = "N10";

			cPProfileMatchingWithEmptyOrigin = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileMatchingWithEmptyOrigin.CP_TariffClassificationNumberfield = "00";
			cPProfileMatchingWithEmptyOrigin.CP_StatisticalClassificationCodefield = "00";
			cPProfileMatchingWithEmptyOrigin.CP_OriginCountryCodefield = "";
			cPProfileMatchingWithEmptyOrigin.CP_ModeofTransportfield = "A";
			cPProfileMatchingWithEmptyOrigin.CP_LineNatureTypefield = "N10";

			cPProfileMatchingWithEmptyMode = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileMatchingWithEmptyMode.CP_TariffClassificationNumberfield = "000000";
			cPProfileMatchingWithEmptyMode.CP_StatisticalClassificationCodefield = "00";
			cPProfileMatchingWithEmptyMode.CP_OriginCountryCodefield = "XX";
			cPProfileMatchingWithEmptyMode.CP_ModeofTransportfield = "";
			cPProfileMatchingWithEmptyMode.CP_LineNatureTypefield = "N10";

			cPProfileMatchingWithEmptyNature = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileMatchingWithEmptyNature.CP_TariffClassificationNumberfield = "000000";
			cPProfileMatchingWithEmptyNature.CP_StatisticalClassificationCodefield = "00";
			cPProfileMatchingWithEmptyNature.CP_OriginCountryCodefield = "XX";
			cPProfileMatchingWithEmptyNature.CP_ModeofTransportfield = "";
			cPProfileMatchingWithEmptyNature.CP_LineNatureTypefield = "";
		}
		void SetMatchingProfilesWithPartialInfo(ZInt riskID)
		{
			SetMatchingProfilesWithPartialInfo();
			cPProfileMatchingWithPartialTariff.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileMatchingWithEmptyStat.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileMatchingWithEmptyOrigin.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileMatchingWithEmptyMode.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileMatchingWithEmptyNature.CP_CommunityProtectionRiskIdentifier = riskID;
		}

		void SetUnmatchingProfilesWithNOT()
		{
			cPProfileUnMatchingWithNotInTariff = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithNotInTariff.CP_TariffClassificationNumberfield = "NOT=000000";
			cPProfileUnMatchingWithNotInTariff.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithNotInTariff.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithNotInTariff.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithNotInTariff.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithNotInStat = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithNotInStat.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithNotInStat.CP_StatisticalClassificationCodefield = "NOT=00";
			cPProfileUnMatchingWithNotInStat.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithNotInStat.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithNotInStat.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithNotInOrigin = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithNotInOrigin.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithNotInOrigin.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithNotInOrigin.CP_OriginCountryCodefield = "NOT=XX";
			cPProfileUnMatchingWithNotInOrigin.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithNotInOrigin.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithNotInMode = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithNotInMode.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithNotInMode.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithNotInMode.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithNotInMode.CP_ModeofTransportfield = "NOT=AS";
			cPProfileUnMatchingWithNotInMode.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithNotInNature = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithNotInNature.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithNotInNature.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithNotInNature.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithNotInNature.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithNotInNature.CP_LineNatureTypefield = "NOT=N10";
		}

		void SetUnmatchingProfiles()
		{
			cPProfileUnMatchingWithDifferentTariff = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithDifferentTariff.CP_TariffClassificationNumberfield = "000001";
			cPProfileUnMatchingWithDifferentTariff.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithDifferentTariff.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithDifferentTariff.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithDifferentTariff.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithDifferentStat = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithDifferentStat.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithDifferentStat.CP_StatisticalClassificationCodefield = "01";
			cPProfileUnMatchingWithDifferentStat.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithDifferentStat.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithDifferentStat.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithDifferentOrigin = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithDifferentOrigin.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithDifferentOrigin.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithDifferentOrigin.CP_OriginCountryCodefield = "XY";
			cPProfileUnMatchingWithDifferentOrigin.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithDifferentOrigin.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithDifferentMode = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithDifferentMode.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithDifferentMode.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithDifferentMode.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithDifferentMode.CP_ModeofTransportfield = "S";
			cPProfileUnMatchingWithDifferentMode.CP_LineNatureTypefield = "N10";

			cPProfileUnMatchingWithDifferentNature = Factory.New<CMRCommunityProtectionProfile>();
			cPProfileUnMatchingWithDifferentNature.CP_TariffClassificationNumberfield = "000000";
			cPProfileUnMatchingWithDifferentNature.CP_StatisticalClassificationCodefield = "00";
			cPProfileUnMatchingWithDifferentNature.CP_OriginCountryCodefield = "XX";
			cPProfileUnMatchingWithDifferentNature.CP_ModeofTransportfield = "A";
			cPProfileUnMatchingWithDifferentNature.CP_LineNatureTypefield = "N20";
		}

		void SetUnmatchingProfilesWithNOT(ZInt riskID)
		{
			SetUnmatchingProfilesWithNOT();
			cPProfileUnMatchingWithNotInTariff.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithNotInStat.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithNotInOrigin.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithNotInMode.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithNotInNature.CP_CommunityProtectionRiskIdentifier = riskID;
		}

		void SetUnmatchingProfiles(ZInt riskID)
		{
			SetUnmatchingProfiles();
			cPProfileUnMatchingWithDifferentTariff.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithDifferentStat.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithDifferentOrigin.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithDifferentMode.CP_CommunityProtectionRiskIdentifier = riskID;
			cPProfileUnMatchingWithDifferentNature.CP_CommunityProtectionRiskIdentifier = riskID;
		}
	}
}
