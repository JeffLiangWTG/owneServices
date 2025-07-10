using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	class AllocationInfoTest : TestCase
	{
		public void TestProcessAllocationInfo()
		{
			AssertAllocationInfoParsed("[ediDocManager SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001]", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager SHP CIV]", refType: "SHP", docType: "CIV");
			AssertAllocationInfoParsed("[ediDocManager SHP]", refType: "SHP");
			AssertAllocationInfoParsed("[SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[EDI:SHP CIV S00001001 IDE]", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 IDE");
			AssertAllocationInfoParsed("[ediDocManager] SHP CIV S00001001");
			AssertAllocationInfoParsed("[ediDocManager]");
			AssertAllocationInfoParsed("FW: [ediDocManager SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("FW: [ediDocManager SHP CIV S00001001] moo", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager SHP CIV S00001001 BLAH]", refType: "SHP", docType: "CIV", refCode: "S00001001 BLAH");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH C:EDI]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH C:EDI B:ABC D:EFG]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D:EFG B:ABC C:EDI]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D:EFG B:ABC C:EDI asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D:EFG B:ABC C:EDI S:DEF asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG", docSource: "DEF");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH C@EDI]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH C@EDI B@ABC D@EFG]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D@EFG B@ABC C@EDI]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D@EFG B@ABC C@EDI asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI:SHP CIV S00001001 BLAH D@EFG B@ABC C@EDI S@DEF asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG", docSource: "DEF");
			AssertAllocationInfoParsed("[ediDocManager PUC:SHP CIV S00001001]", refType: "PUC:SHP", docType: "CIV", refCode: "S00001001");
		}

		public void TestProcessAllocationInfoWithSeperator()
		{
			AssertAllocationInfoParsed("[ediDocManager SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001]", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager SHP CIV]", refType: "SHP", docType: "CIV");
			AssertAllocationInfoParsed("[ediDocManager SHP]", refType: "SHP");
			AssertAllocationInfoParsed("[SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[EDI%SHP CIV S00001001 IDE]", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 IDE");
			AssertAllocationInfoParsed("[ediDocManager] SHP CIV S00001001");
			AssertAllocationInfoParsed("[ediDocManager]");
			AssertAllocationInfoParsed("FW: [ediDocManager SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("FW: [ediDocManager SHP CIV S00001001] moo", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocManager SHP CIV S00001001 BLAH]", refType: "SHP", docType: "CIV", refCode: "S00001001 BLAH");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH C:EDI]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH C:EDI B:ABC D:EFG]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH D:EFG B:ABC C:EDI]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH D:EFG B:ABC C:EDI asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH C@EDI]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH C@EDI B@ABC D@EFG]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH D@EFG B@ABC C@EDI]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDocManager EDI%SHP CIV S00001001 BLAH D@EFG B@ABC C@EDI asdfasdfasdfasdf]", companyCode: "EDI", refType: "EDI%SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
		}

		public void TestProcessAllocationInfoCaseSensitive()
		{
			AssertAllocationInfoParsed("[EDIDOCMANAGER SHP CIV]", refType: "SHP", docType: "CIV");
			AssertAllocationInfoParsed("[edidocmanager EDI:SHP CIV S00001001 BLAH C:EDI B:ABC D:EFG]", companyCode: "EDI", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001 BLAH", visibleCompanyCode: "EDI", visibleBranchCode: "ABC", visibleDepartmentCode: "EFG");
			AssertAllocationInfoParsed("[ediDOCManager SHP CIV S00001001]", refType: "SHP", docType: "CIV", refCode: "S00001001");
			AssertAllocationInfoParsed("[ediDocMANAGER EDI:SHP CIV S00001001]", refType: "EDI:SHP", docType: "CIV", refCode: "S00001001");
		}

		void AssertAllocationInfoParsed(string rawAllocationInfo, string companyCode = "", string refType = "", string docType = "", string refCode = "", string visibleCompanyCode = "", string visibleBranchCode = "", string visibleDepartmentCode = "", string docSource = "")
		{
			var info = new AllocationInfo(rawAllocationInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Company code should match:", companyCode, info.RecordCompanyCode);
				AssertEquals("Ref type should match:", refType, info.RefType);
				AssertEquals("Doc type should match:", docType, info.DocType);
				AssertEquals("Doc Source should match:", docSource, info.DocSource);
				AssertEquals("Ref code should match:", refCode, info.RefCode);
				AssertEquals("Visible company code should match:", visibleCompanyCode, info.VisibleCompanyCode);
				AssertEquals("Visible branch code should match:", visibleBranchCode, info.VisibleBranchCode);
				AssertEquals("Visible department code should match:", visibleDepartmentCode, info.VisibleDepartmentCode);
			});
		}
	}
}
