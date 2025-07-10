using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.CDS.Declaration.AdditionalInformation.Testing
{
	public class CDSAdditionalInfoLookupsTests : AdditionalInfoLookupsTests
	{
		public override void TestAdditionalInfoLookupLists()
		{
			const string countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gb = helper.CreateNewOrGetExistingDataGrouping(countryCode, "United Kingdom");
			helper.CreateNewOrGetExistingDataGrouping(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "GB Customs Declaration Services (CDS)").ZZZ_ZZZ_Grouping = gb.PK;
			Factory.Save();

			SetupReferenceData(CDS);
			SetupReferenceData(countryCode);
			var dec = CreateJobDeclaration(countryCode, CDS);

			var codesForDec = GetListOfCodes(dec.AdditionalInfos[0].Lookups);
			var codesForInv = GetListOfCodes(dec.Invoices[0].AdditionalInfos[0].Lookups);
			var codesForInvLine = GetListOfCodes(dec.Invoices[0].InvoiceLines[0].AdditionalInfos[0].Lookups);
			var codesForEntryInstruction = GetListOfCodes(dec.CustomsEntryInstructions[0].AdditionalInfos[0].Lookups);

			const string IMPORT_HEADER = "IMHDR";
			const string IMPORT_ITEM = "IMITM";
			const string EXPORT_HEADER = "EXHDR";
			const string EXPORT_ITEM = "EXITM";

			const string Msg = "Testing AdditionalInfoLookups for CDS.";

			AssertEquals(Msg, 2, codesForDec.Count);
			AssertEquals(Msg, 1, codesForInv.Count);
			AssertEquals(Msg, 1, codesForInvLine.Count);
			AssertEquals(Msg, 2, codesForEntryInstruction.Count);

			Assert(Msg, codesForDec.Contains(IMPORT_HEADER));
			Assert(Msg, codesForDec.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForDec.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForDec.Contains(EXPORT_ITEM));

			Assert(Msg, !codesForInv.Contains(IMPORT_HEADER));
			Assert(Msg, codesForInv.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForInv.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForInv.Contains(EXPORT_ITEM));

			Assert(Msg, !codesForInvLine.Contains(IMPORT_HEADER));
			Assert(Msg, codesForInvLine.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForInvLine.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForInvLine.Contains(EXPORT_ITEM));

			Assert(Msg, codesForEntryInstruction.Contains(IMPORT_HEADER));
			Assert(Msg, codesForEntryInstruction.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForEntryInstruction.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForEntryInstruction.Contains(EXPORT_ITEM));
		}
	}
}
