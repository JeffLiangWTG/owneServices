using Enterprise.Customs.GB.Business.Declaration.Testing;

namespace Enterprise.Customs.GB.Chief.Declaration.AdditionalInformation.Testing
{
	public class ChiefAdditionalInfoLookupsTests : AdditionalInfoLookupsTests
	{
		public override void TestAdditionalInfoLookupLists()
		{
			const string countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			const string CHF = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			SetupReferenceData(countryCode);
			var dec = CreateJobDeclaration(countryCode, CHF);

			var codesForDec = GetListOfCodes(dec.AdditionalInfos[0].Lookups);
			var codesForInv = GetListOfCodes(dec.Invoices[0].AdditionalInfos[0].Lookups);
			var codesForInvLine = GetListOfCodes(dec.Invoices[0].InvoiceLines[0].AdditionalInfos[0].Lookups);

			const string IMPORT_HEADER = "IMHDR";
			const string IMPORT_ITEM = "IMITM";
			const string EXPORT_HEADER = "EXHDR";
			const string EXPORT_ITEM = "EXITM";

			const string Msg = "Testing AdditionalInfoLookups for CHIEF.";

			Assert(Msg, codesForDec.Contains(IMPORT_HEADER));
			Assert(Msg, !codesForDec.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForDec.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForDec.Contains(EXPORT_ITEM));

			Assert(Msg, codesForInv.Contains(IMPORT_HEADER));
			Assert(Msg, codesForInv.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForInv.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForInv.Contains(EXPORT_ITEM));

			Assert(Msg, !codesForInvLine.Contains(IMPORT_HEADER));
			Assert(Msg, codesForInvLine.Contains(IMPORT_ITEM));
			Assert(Msg, !codesForInvLine.Contains(EXPORT_HEADER));
			Assert(Msg, !codesForInvLine.Contains(EXPORT_ITEM));
		}
	}
}
