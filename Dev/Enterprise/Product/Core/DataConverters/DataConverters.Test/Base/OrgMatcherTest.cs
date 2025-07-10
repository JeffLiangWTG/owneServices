using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters.Testing.Base
{
	sealed internal class OrgMatcherTest : TestCaseWithFactory
	{
		public void TestMatchToLegacyRecord()
		{
			var orgHeader = OrgHeader.New(Factory);
			orgHeader.OH_Code = "NOMATCH";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orgCusCode.OK_CustomsRegNo = "MATCH";

			var matcher = new OrgMatcher("MATCH", "", Factory);
			AssertEquals("matcher.MatchedCode", "NOMATCH", matcher.MatchedCode);
			AssertEquals("matcher.MatchedPK", orgHeader.PK, matcher.MatchedPK);
			AssertEquals("matcher.OriginalCode", "MATCH", matcher.OriginalCode);
		}

		public void TestMatchToDeliveranceRecord()
		{
			ZString deliveranceSystemIDCode = "MYTEST";
			var orgHeader = OrgHeader.New(Factory);
			orgHeader.OH_Code = "NOMATCH";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DeliveranceCode;
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orgCusCode.OK_CustomsRegNo = deliveranceSystemIDCode + "_" + "MATCH";

			var matcher = new OrgMatcher("MATCH", deliveranceSystemIDCode, Factory);
			AssertEquals("matcher.MatchedCode", "NOMATCH", matcher.MatchedCode);
			AssertEquals("matcher.MatchedPK", orgHeader.PK, matcher.MatchedPK);
			AssertEquals("matcher.OriginalCode", "MATCH", matcher.OriginalCode);
		}

		public void TestMatchToAccountCode()
		{
			var orgHeader = OrgHeader.New(Factory);
			orgHeader.OH_Code = "MATCH";

			var matcher = new OrgMatcher("MATCH", "", Factory);
			AssertEquals("matcher.MatchedCode", "MATCH", matcher.MatchedCode);
			AssertEquals("matcher.MatchedPK", orgHeader.PK, matcher.MatchedPK);
			AssertEquals("matcher.OriginalCode", "MATCH", matcher.OriginalCode);
		}

		public void TestNoMatch()
		{
			var orgHeader = OrgHeader.New(Factory);
			orgHeader.OH_Code = "NOMATCH";

			var matcher = new OrgMatcher("MATCH", "", Factory);
			AssertEquals("matcher.MatchedCode", "", matcher.MatchedCode);
			AssertEquals("matcher.MatchedPK", ZGuid.Empty, matcher.MatchedPK);
			AssertEquals("matcher.OriginalCode", "MATCH", matcher.OriginalCode);
		}

		public void TestEmptyAccountCode()
		{
			var matcher = new OrgMatcher("", "", Factory);
			AssertEquals("matcher.MatchedCode", "", matcher.MatchedCode);
			AssertEquals("matcher.MatchedPK", ZGuid.Empty, matcher.MatchedPK);
			AssertEquals("matcher.OriginalCode", "", matcher.OriginalCode);
		}
	}
}
