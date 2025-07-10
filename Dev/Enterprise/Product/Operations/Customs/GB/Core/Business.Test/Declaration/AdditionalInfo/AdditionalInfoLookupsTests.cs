using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class AdditionalInfoLookupsTests : BusinessObjectLookupsTestCase
	{
		protected void SetupReferenceData(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test AdditionalInfoLookups");

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
				dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = applicationCode;

				var invoice = dec.Invoices.AddNew();
				var invLine = invoice.InvoiceLines.AddNew();

				dec.AdditionalInfos.AddNew();
				invoice.AdditionalInfos.AddNew();
				invLine.AdditionalInfos.AddNew();
				dec.CustomsEntryInstructions[0].AdditionalInfos.AddNew();

				return dec;
			}
		}

		protected List<string> GetListOfCodes(AdditionalInfoLookups lookups)
		{
			var list = new List<string>();
			foreach (var obj in lookups.CodeList)
			{
				var pair = obj as CodeDescriptionPair;
				if (pair != null)
				{
					list.Add(pair.Code);
				}
			}
			return list;
		}

		public virtual void TestAdditionalInfoLookupLists()
		{
			const string countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			SetupReferenceData(countryCode);
			var dec = CreateJobDeclaration(countryCode, "");

			var codesForDec = GetListOfCodes(dec.AdditionalInfos[0].Lookups);
			var codesForInv = GetListOfCodes(dec.Invoices[0].AdditionalInfos[0].Lookups);
			var codesForInvLine = GetListOfCodes(dec.Invoices[0].InvoiceLines[0].AdditionalInfos[0].Lookups);

			const string IMPORT_HEADER = "IMHDR";
			const string IMPORT_ITEM = "IMITM";
			const string EXPORT_HEADER = "EXHDR";
			const string EXPORT_ITEM = "EXITM";

			const string Msg = "Testing AdditionalInfoLookups for GB.";

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
