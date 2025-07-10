using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class DEOfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var dec = Factory.New<JobDeclaration>();
			NUnit.Framework.Assert.Multiple(() =>
			{
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				var officeCode = dec.CustomsOffices.AddNew();
				NUnit.Framework.Assert.That(officeCode.Lookups.CY_CodeList.GetAllCodes(), NUnit.Framework.Is.EqualTo(new[]
				{
					EuOfficeCodesTypes.Codes.ActualExitOffice,
					EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice,
					EuOfficeCodesTypes.Codes.OfficeOfExit,
					EuOfficeCodesTypes.Codes.OfficeOfPresentation
				}), "Export declaration");

				dec.JE_MessageType = MessageTypeList.Codes.Import;
				officeCode = dec.CustomsOffices.AddNew();
				NUnit.Framework.Assert.That(officeCode.Lookups.CY_CodeList.GetAllCodes(), NUnit.Framework.Is.EqualTo(new[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }), "Import declaration");

				dec.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				officeCode = dec.CustomsOffices.AddNew();
				NUnit.Framework.Assert.That(officeCode.Lookups.CY_CodeList.Count, NUnit.Framework.Is.EqualTo(0), "Miscellaneous declaration");
			});
		}

		public void TestOfficeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeDE000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();

			NUnit.Framework.Assert.Multiple(() =>
			{
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
				var officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("CY_Code 'EXP' not in the list", new[] { "DE000001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				officeCode.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
				officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("CY_Code 'EAM' in the list", new[] { "DE000001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				officeCode.CY_Code = ZString.Empty;
				officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("CY_Code empty", new[] { "DE000001", "IEDUB100" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});
		}

		public void TestOfficeCodeList_AEC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeDE000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			var codeDE000002 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000002", "Central Community Transit Office 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000002.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();

			NUnit.Framework.Assert.Multiple(() =>
			{
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
				var officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("CY_Code AEC", new[] { "DE000001", "DE000002" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				NUnit.Framework.Assert.That(officeCodeList.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "Country/Region or Grouping");
			});
		}

		[ExpectNoExceptions]
		public void TestOfficeCodeList_EAM()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			var officeCodeList = officeCode.Lookups.OfficeCodeList;

			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(officeCodeList.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Germany).Using(CustomComparers.TypeComparison), "Value");
				NUnit.Framework.Assert.That(officeCodeList.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].IsRemovable, NUnit.Framework.Is.EqualTo(false), "IsRemovable");
			});
		}
	}
}
