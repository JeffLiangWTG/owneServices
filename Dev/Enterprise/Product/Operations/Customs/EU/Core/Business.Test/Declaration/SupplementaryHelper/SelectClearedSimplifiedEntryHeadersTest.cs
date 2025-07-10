using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper.Testing
{
	sealed class SelectClearedSimplifiedEntryHeadersTest : TestCaseWithFactory
	{
		public void TestEntryHeaders()
		{
			var countryCode = GlbBranch.CurrentBranch.Company.Country.Code;
			var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
			cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			var clearCode = cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, EntryStatusList.Codes.Clear, EntryStatusList.Descriptions.Clear, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			cusCodeHelper.CreateNewOrGetExistingCusCodeListAttribute(clearCode.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");
			var awaitingCode = cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, EntryStatusList.Codes.AwaitingResponse, EntryStatusList.Descriptions.AwaitingResponse, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			cusCodeHelper.CreateNewOrGetExistingCusCodeListAttribute(awaitingCode.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, "false");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H1;

			CombineAssertions("Pre-requisites", () =>
			{
				AssertEquals("Instruction 1: IsSimplifiedEntryInstruction", expected: true, entryInstruction1.IsSimplifiedEntryInstruction);
				AssertEquals("Instruction 2: IsSimplifiedEntryInstruction", expected: false, entryInstruction2.IsSimplifiedEntryInstruction);
			});

			var headers = Enumerable.Range(0, 16).Select(i =>
			{
				var header = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				header.CH_CEI_Instruction = ((i & 1) == 0 ? entryInstruction1 : entryInstruction2).PK;
				if ((i & 2) == 0)
				{
					header.MovementReferenceNumberSetter("MRN" + i.ToString());
				}
				header.CH_EntryStatus = (i & 4) == 0 ? EntryStatusList.Codes.Clear : EntryStatusList.Codes.AwaitingResponse;
				return header;
			}).ToArray();

			var testFilter = new SelectClearedSimplifiedEntryHeaders(declaration);
			var entryHeaders = testFilter.EntryHeaders.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaders should contain headers[0]", expected: true, entryHeaders.Contains(headers[0]));
				AssertEquals("EntryHeaders should contain headers[8]", expected: true, entryHeaders.Contains(headers[8]));
				AssertEquals("EntryHeaders Length", expected: 2, entryHeaders.Length);
			});
		}
	}
}
