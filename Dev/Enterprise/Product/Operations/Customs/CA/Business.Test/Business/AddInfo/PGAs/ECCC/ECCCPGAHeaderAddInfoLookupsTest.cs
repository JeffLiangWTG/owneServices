using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ECCCPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAOSConformityCodeList()
		{
			PrepareRefData();
			var list = header.AddInfoLookups.AOSConformityCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals(true, list.ContainsCode("ME01"));
				AssertEquals(true, list.ContainsCode("ME02"));
				AssertEquals(true, list.ContainsCode("ME03"));
				AssertEquals(true, list.ContainsCode("ME04"));
			});

			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, "ME00", "ME00", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSConformity, "AOSConformity", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.CountryCodes.Canada);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			Factory.Save();

			list = header.AddInfoLookups.AOSConformityCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals(true, list.ContainsCode("ME01"));
				AssertEquals(true, list.ContainsCode("ME02"));
				AssertEquals(true, list.ContainsCode("ME03"));
				AssertEquals(true, list.ContainsCode("ME04"));
			});
		}

		public void TestAOSReplacementCodeList()
		{
			PrepareRefData();
			var list = header.AddInfoLookups.AOSReplacementCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("ME05"));
				AssertEquals(true, list.ContainsCode("ME06"));
			});

			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, "ME01", "ME01", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSReplacement, "AOSReplacement", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.CountryCodes.Canada);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			Factory.Save();

			list = header.AddInfoLookups.AOSReplacementCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("ME05"));
				AssertEquals(true, list.ContainsCode("ME06"));
			});
		}

		public void TestAOSEvidenceCodeList()
		{
			PrepareRefData();
			var list = header.AddInfoLookups.AOSEvidenceCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("ME07"));
				AssertEquals(true, list.ContainsCode("ME08"));
			});

			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, "ME01", "ME01", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSEvidence, "AOSEvidence", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.CountryCodes.Canada);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			Factory.Save();

			list = header.AddInfoLookups.AOSEvidenceCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("ME07"));
				AssertEquals(true, list.ContainsCode("ME08"));
			});
		}

		public void TestAOSRetentionCodeList()
		{
			PrepareRefData();
			var list = header.AddInfoLookups.AOSRetentionCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(3, list.Count);
				AssertEquals(true, list.ContainsCode("ME09"));
				AssertEquals(true, list.ContainsCode("ME10"));
				AssertEquals(true, list.ContainsCode("ME11"));
			});

			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, "ME01", "ME01", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSRetention, "AOSRetention", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.CountryCodes.Canada);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			Factory.Save();

			list = header.AddInfoLookups.AOSRetentionCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(3, list.Count);
				AssertEquals(true, list.ContainsCode("ME09"));
				AssertEquals(true, list.ContainsCode("ME10"));
				AssertEquals(true, list.ContainsCode("ME11"));
			});
		}

		public void TestAlternativeStandardOfEngineClassCodeList()
		{
			PrepareRefData();
			var list = header.AddInfoLookups.AlternativeStandardOfEngineClassCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("EC0E"));
				AssertEquals(true, list.ContainsCode("EC0F"));
			});

			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCAlternativeStandardConformityStatements, "ME01", "ME01", startDate, endDate);
			Factory.Save();

			list = header.AddInfoLookups.AlternativeStandardOfEngineClassCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(true, list.ContainsCode("EC0E"));
				AssertEquals(true, list.ContainsCode("EC0F"));
			});
		}

		void PrepareRefData()
		{
			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var codeType01 = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement;
			// With attribute - AOSConformity
			var cusCode = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME01", "ME01", startDate, endDate);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME02", "ME02", startDate, endDate);
			var cusCode02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME03", "ME03", startDate, endDate);
			var cusCode03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME04", "ME04", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSConformity, "AOSConformity", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute02 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode02.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute03 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode03.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSReplacement
			var cusCode04 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME05", "ME05", startDate, endDate);
			var cusCode05 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME06", "ME06", startDate, endDate);
			var attributeType02 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSReplacement, "AOSReplacement", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute04 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode04.PK, attributeType02.ZXE_Name, YesNoList.Codes.Yes);
			var attribute05 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode05.PK, attributeType02.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSEvidence
			var cusCode06 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME07", "ME07", startDate, endDate);
			var cusCode07 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME08", "ME08", startDate, endDate);
			var attributeType03 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSEvidence, "AOSEvidence", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute06 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode06.PK, attributeType03.ZXE_Name, YesNoList.Codes.Yes);
			var attribute07 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode07.PK, attributeType03.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSRetention
			var cusCode08 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME09", "ME09", startDate, endDate);
			var cusCode09 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME10", "ME10", startDate, endDate);
			var cusCode10 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME11", "ME11", startDate, endDate);
			var attributeType04 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSRetention, "AOSRetention", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute08 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode08.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);
			var attribute09 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode09.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);
			var attribute10 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode10.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);

			// ECACS
			var codeType02 = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCAlternativeStandardConformityStatements;
			var cusCode11 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType02, "EC0E", "EC0E", startDate, endDate);
			var cusCode12 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType02, "EC0F", "EC0F", startDate, endDate);
			Factory.Save();
		}

		public void TestProgramCodesList()
		{
			AssertEquals(typeof(ECCCPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestIntendedUseCodes()
		{
			AssertEquals(0, header.AddInfoLookups.IntendedUseCodeList.Count);
			header.CA_WRMProgramInd = YesNoList.Codes.Yes;
			AssertEquals(27, header.AddInfoLookups.IntendedUseCodeList.Count);
			header.CA_WRMProgramInd = YesNoList.Codes.No;
			header.CA_WENProgramInd = YesNoList.Codes.Yes;
			AssertEquals(12, header.AddInfoLookups.IntendedUseCodeList.Count);
			header.CA_WRMProgramInd = YesNoList.Codes.Yes;
			AssertEquals(39, header.AddInfoLookups.IntendedUseCodeList.Count);
		}

		[TestDate(2018, 9, 16)]
		public void TestEngineModelYearList()
		{
			AssertSame(Factory.New<ECCCPGAHeader>().AddInfoLookups.EngineModelYearList, header.AddInfoLookups.EngineModelYearList);
			AssertEquals("2020", header.AddInfoLookups.EngineModelYearList[0].Code);
		}

		[TestDate(2018, 9, 16)]
		public void TestMachineModelYearList()
		{
			AssertSame(Factory.New<ECCCPGAHeader>().AddInfoLookups.MachineModelYearList, header.AddInfoLookups.MachineModelYearList);
			AssertEquals("2020", header.AddInfoLookups.MachineModelYearList[0].Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			header = invoiceLine.ECCCPGAHeader;
			Factory.Save();
		}
		ECCCPGAHeader header;

		#endregion
	}
}
