using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration.Testing;

namespace Enterprise.Customs.GB.CDS.Declaration.AdditionalInformation.Testing
{
	public class CDSAdditionalInfoValidationTests : AdditionalInfoValidationTests
	{
		public override void TestAdditionalInfoValidation()
		{
			SetupReferenceData(CdsApplicationCode);
			var dec = CreateJobDeclaration(UkCountryCode, CdsApplicationCode);

			var dictionaryDec = CreateAddInfoDictionary(dec.AdditionalInfos);
			var dictionaryInv = CreateAddInfoDictionary(dec.Invoices[0].AdditionalInfos);
			var dictionaryInvLine = CreateAddInfoDictionary(dec.Invoices[0].InvoiceLines[0].AdditionalInfos);

			const string IMPORT_HEADER = "IMHDR";
			const string IMPORT_ITEM = "IMITM";
			const string EXPORT_HEADER = "EXHDR";
			const string EXPORT_ITEM = "EXITM";
			const string INVALID_CODE = "BOGUS";

			const string Msg = "Testing AdditionalInfo Validation for CDS.";

			CheckAdditionalInfoValidation(Msg, dictionaryDec, IMPORT_HEADER, true);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, IMPORT_ITEM, true);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, EXPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, EXPORT_ITEM, false);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, INVALID_CODE, false);

			CheckAdditionalInfoValidation(Msg, dictionaryInv, IMPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInv, IMPORT_ITEM, true);
			CheckAdditionalInfoValidation(Msg, dictionaryInv, EXPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInv, EXPORT_ITEM, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInv, INVALID_CODE, false);

			CheckAdditionalInfoValidation(Msg, dictionaryInvLine, IMPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInvLine, IMPORT_ITEM, true);
			CheckAdditionalInfoValidation(Msg, dictionaryInvLine, EXPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInvLine, EXPORT_ITEM, false);
			CheckAdditionalInfoValidation(Msg, dictionaryInvLine, INVALID_CODE, false);
		}

		public void TestCheckZG_IsTrainingDeclaration()
		{
			var dec = CreateJobDeclaration(UkCountryCode, CdsApplicationCode);
			dec.ZG_IsTrainingDeclaration = ZBool.True;
			AssertHasError(dec.ZG_IsTrainingDeclarationInfo, "CDS does not support training entries. Untick this box.");

			dec.ZG_IsTrainingDeclaration = ZBool.False;
			AssertNoNotifications(dec.ZG_IsTrainingDeclarationInfo);
		}

		string UkCountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		string CdsApplicationCode => Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
	}
}
