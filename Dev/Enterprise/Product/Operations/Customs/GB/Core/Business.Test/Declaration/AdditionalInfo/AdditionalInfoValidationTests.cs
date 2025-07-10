using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class AdditionalInfoValidationTests : BusinessObjectValidationTestCase
	{
		protected void SetupReferenceData(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test AdditionalInfoValidation");

			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string IMPORT = "IMPORT";
			const string EXPORT = "EXPORT";
			const string HEADER = "HEADER";
			const string ITEM = "ITEM";

			helper.CreateNewOrGetExistingCusCodeType(ADDIN, "AdditionalInformation");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", ADDIN, countryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", ADDIN, countryCode);

			CreateAdditionalInfo(helper, countryCode, "IMHDR", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMITM", IMPORT, ITEM);

			CreateAdditionalInfo(helper, countryCode, "EXHDR", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXITM", EXPORT, ITEM);

			Factory.Save();
		}

		void CreateAdditionalInfo(UniversalReferenceTestDataHelper helper, string countryCode, string code, string directionValue, string levelValue)
		{
			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string Direction = "Direction";
			const string Level = "Level";

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			var addInfo = helper.CreateCusCodeList(countryCode, ADDIN, code, code + " - Description", dtMIN, dtMAX);
			addInfo.Attributes.AddNew(Direction, directionValue);
			addInfo.Attributes.AddNew(Level, levelValue);
		}

		protected JobDeclaration CreateJobDeclaration(string countryCode, string applicationCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				dec.JE_ApplicationCode = applicationCode;

				var invoice = dec.Invoices.AddNew();
				var invLine = invoice.InvoiceLines.AddNew();

				PopulateAdditionalInfos(dec.AdditionalInfos);
				PopulateAdditionalInfos(invoice.AdditionalInfos);
				PopulateAdditionalInfos(invLine.AdditionalInfos);

				return dec;
			}
		}

		void PopulateAdditionalInfos(AdditionalInfoCollection addInfoCol)
		{
			var listCodes = new List<string>(new string[] { "IMHDR", "IMITM", "EXHDR", "EXITM", "BOGUS" });

			foreach (var code in listCodes)
			{
				var addInfo = addInfoCol.AddNew();
				addInfo.CSI_Code = code;
			}
		}

		protected Dictionary<string, AdditionalInfo> CreateAddInfoDictionary(AdditionalInfoCollection addInfoCol)
		{
			var dictionary = new Dictionary<string, AdditionalInfo>();

			foreach (var obj in addInfoCol)
			{
				var addInfo = obj;
				if (addInfo != null)
				{
					var key = addInfo.CSI_Code;
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, addInfo);
					}
				}
			}
			return dictionary;
		}

		protected void CheckAdditionalInfoValidation(string msg, Dictionary<string, AdditionalInfo> dictionary, string code, bool isValid)
		{
			AdditionalInfo addInfo = dictionary[code];
			Assert(msg, addInfo != null);

			if (isValid)
			{
				AssertNoMessageErrors(msg, addInfo.CSI_CodeInfo);
			}
			else
			{
				AssertHasMessageErrorContaining(msg, addInfo.CSI_CodeInfo, "The code you have selected is not in the list.");
			}
		}

		public virtual void TestAdditionalInfoValidation()
		{
			const string countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			SetupReferenceData(countryCode);
			var dec = CreateJobDeclaration(countryCode, "");

			var dictionaryDec = CreateAddInfoDictionary(dec.AdditionalInfos);
			var dictionaryInv = CreateAddInfoDictionary(dec.Invoices[0].AdditionalInfos);
			var dictionaryInvLine = CreateAddInfoDictionary(dec.Invoices[0].InvoiceLines[0].AdditionalInfos);

			const string IMPORT_HEADER = "IMHDR";
			const string IMPORT_ITEM = "IMITM";
			const string EXPORT_HEADER = "EXHDR";
			const string EXPORT_ITEM = "EXITM";
			const string INVALID_CODE = "BOGUS";

			const string Msg = "Testing AdditionalInfo Validation for GB.";

			CheckAdditionalInfoValidation(Msg, dictionaryDec, IMPORT_HEADER, true);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, IMPORT_ITEM, false);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, EXPORT_HEADER, false);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, EXPORT_ITEM, false);
			CheckAdditionalInfoValidation(Msg, dictionaryDec, INVALID_CODE, false);

			CheckAdditionalInfoValidation(Msg, dictionaryInv, IMPORT_HEADER, true);
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
	}
}
