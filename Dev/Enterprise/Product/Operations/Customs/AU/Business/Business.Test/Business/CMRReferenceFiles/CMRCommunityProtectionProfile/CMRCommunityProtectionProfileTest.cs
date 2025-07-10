using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCommunityProtectionProfile))]
	sealed class CMRCommunityProtectionProfileTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsMatchingNatureOrModeOfTransport()
		{
			var testKeys = new CPQuestionKeys();
			testKeys.StatCode = "00";
			testKeys.HasValidOriginOrNatureOrModeOfTransport = false;

			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_StatisticalClassificationCodefield = testKeys.StatCode;
			profile.CP_OriginCountryCodefield = "XX";
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "NOT=P";
			AssertEquals("Line attachee does not have Valid Nature or Mode of transport and matches the profile", true, profile.IsMatchingOtherThanTariff(testKeys));
			AssertEquals("Line attachee does have Valid Origin and matches the profile", true, profile.IsMatchingOtherThanTariff(testKeys));

			testKeys.HasValidOriginOrNatureOrModeOfTransport = true;
			AssertEquals("Line attachee does have Valid Nature or Mode of transport and thus, does not match the profile", false, profile.IsMatchingOtherThanTariff(testKeys));

			testKeys.ModeOfTransport = "A";
			AssertEquals("Line attachee does have Valid Nature or Mode of transport and thus, does not match the profile", false, profile.IsMatchingOtherThanTariff(testKeys));

			testKeys.HasValidOriginOrNatureOrModeOfTransport = false;

			testKeys.ModeOfTransport = "A";
			AssertEquals("Line attachee does have Valid Nature or Mode of transport and thus, does not match the profile", true, profile.IsMatchingOtherThanTariff(testKeys));

			testKeys.Nature = "N10";
			AssertEquals("Line attachee does have Valid Nature or Mode of transport and matches the profile", true, profile.IsMatchingOtherThanTariff(testKeys));

			testKeys.OriginCode = "XX";
			AssertEquals("Line attachee does have Valid Origin and matches the profile", true, profile.IsMatchingOtherThanTariff(testKeys));
		}

		public void TestIsMatchingOtherThanTariff()
		{
			var testKeys = new CPQuestionKeys();
			testKeys.StatCode = "00";
			testKeys.OriginCode = "XX";
			testKeys.Nature = "N70";
			testKeys.ModeOfTransport = "X";
			testKeys.HasValidOriginOrNatureOrModeOfTransport = true;

			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_StatisticalClassificationCodefield = testKeys.StatCode;
			profile.CP_OriginCountryCodefield = testKeys.OriginCode;
			profile.CP_LineNatureTypefield = testKeys.Nature;
			profile.CP_ModeofTransportfield = testKeys.ModeOfTransport;
			AssertEquals("IsMatching", true, profile.IsMatchingOtherThanTariff(testKeys));

			profile.CP_ModeofTransportfield = "";
			AssertEquals("IsMatching", true, profile.IsMatchingOtherThanTariff(testKeys));

			profile.CP_ModeofTransportfield = "Not=ABC";
			AssertEquals("IsMatching", true, profile.IsMatchingOtherThanTariff(testKeys));

			profile.CP_ModeofTransportfield = "Y";
			AssertEquals("IsMatching", false, profile.IsMatchingOtherThanTariff(testKeys));
		}

		protected override BusinessObject GetNewBusinessObject() => CMRCommunityProtectionProfile.New(Factory);
	}
}
