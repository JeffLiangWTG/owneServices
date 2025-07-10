using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CN.Business.Constants.UniversalReferenceConstants;
using ECC = Enterprise.Core.Constants;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNRefCusCodeListTypesTest : TestCaseWithFactory
	{
		public void TestGetDangerousChemicalList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "Customs Dangerous Chemical");
			helper.CreateNewOrGetExistingCusCodeList("CN", RefCusCodeListTypesCodes.CNDangerousChemical, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var dangerousChemical = CNRefCusCodeListTypes.GetDangerousChemicalList(Factory, ZDateTime.Now);
			dangerousChemical.Load();
			AssertEquals(1, dangerousChemical.Count);
			AssertEquals("7664-41-7", dangerousChemical[0].ZZD_Code);
			AssertEquals("氨", dangerousChemical[0].ZZD_Description);
		}

		public void TestGetDistrictList()
		{
			var list1 = CNRefCusCodeListTypes.GetDistrictList(Factory, ZDateTime.Today);
			var list2 = CNRefCusCodeListTypes.GetDistrictList(Factory, ZDateTime.Today);
			AssertSame("IsCached", list1, list2);
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.DistrictCode, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "NEWCODE";
			codeList.ZZD_Description = "New District Code";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.China;
			codeList.ZZD_CodeType = RefCusCodeListTypesCodes.DistrictCode;
			codeList.ZZD_StartDate = ZDateTime.Today;
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			list1 = CNRefCusCodeListTypes.GetDistrictList(new BusinessObjectFactory(), ZDateTime.Today);
			list1.Load();
			AssertEquals(count + 1, list1.Count);
			AssertEquals(codeList.ZZD_Description, list1.OfType<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == codeList.ZZD_Code).ZZD_Description);
		}

		public void TestGetCNDOCCodeList()
		{
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.CNRequiredDocuments, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", RefCusCodeListTypesCodes.CNRequiredDocuments, Core.Constants.CountryCodes.China, RefCusCodeListTypesCodes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", RefCusCodeListTypesCodes.CNRequiredDocuments, Core.Constants.CountryCodes.China, RefCusCodeListTypesCodes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", RefCusCodeListTypesCodes.CNRequiredDocuments, Core.Constants.CountryCodes.China, RefCusCodeListTypesCodes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "1A", "Code 1");
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "A");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "1B", "Code 2");
			code2.Attributes.AddNew("Import", ZString.Empty);
			code2.Attributes.AddNew("Export", ZString.Empty);
			code2.Attributes.AddNew("DisplayCode", "B");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "1C", "Code 3");
			code3.Attributes.AddNew("Export", ZString.Empty);
			code3.Attributes.AddNew("DisplayCode", "C");
			Factory.Save();
			var newCollection = CNRefCusCodeListTypes.GetCusSupportingDocumentList(new BusinessObjectFactory(), ZDateTime.Today);
			newCollection.Load();
			var list = newCollection.OfType<ZZRefCusCodeListCombined>();
			AssertEquals(count + 3, list.Count());
			Assert(list.Any(refCodeList => refCodeList.ZZD_Code == "1A"));
			Assert(list.Any(refCodeList => refCodeList.ZZD_Code == "1B"));
			Assert(list.Any(refCodeList => refCodeList.ZZD_Code == "1C"));
		}

		[TestDate(2019, 12, 26)]
		public void TestGetCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", RefCusCodeListTypesCodes.CustomsOffice, "CNJ", "Nanjing Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			helper.CreateNewOrGetExistingCusCodeList("CN", RefCusCodeListTypesCodes.CustomsOffice, "CSH", "Shanghai Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 25));
			helper.CreateNewOrGetExistingCusCodeList("US", RefCusCodeListTypesCodes.CustomsOffice, "CNY", "New York Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			Factory.Save();
			var offices = CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Now);
			offices.Load();
			AssertEquals(1, offices.Count);
			AssertEquals("Customs Office must be 'CNJ'", "CNJ", offices[0].ZZD_Code);
		}

		[TestDate(2020, 9, 9)]
		public void TestGetGetDecTpAccessCodeTypeList()
		{
			var countryCode = Core.Constants.CountryCodes.China;
			var codeDateMin = ZDateTime.MinSmallDateTimeValue;
			var codeDateMax = ZDateTime.MaxSmallDateTime;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeListValid = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNWEH42S201", "Valid Code List", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(codeListValid.PK, "CustomsOffice", "42");

			var codeListNoOfficeAttribute = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNWEH42S202", "Code List without Attribute CustomsOffice", codeDateMin, codeDateMax);

			var codeListWithOtherOffice = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSF", "CNWEH42S203", "Code List without Attribute CustomsOffice", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(codeListWithOtherOffice.PK, "CustomsOffice", "43");

			var codeListWithOtherType = helper.CreateNewOrGetExistingCusCodeList(countryCode, "DSSSX", "CNWEH42S204", "Code List without Attribute CustomsOffice", codeDateMin, codeDateMax);
			helper.CreateCusCodeListAttribute(codeListWithOtherType.PK, "CustomsOffice", "42");

			Factory.Save();
			var testList = CNRefCusCodeListTypes.GetTransitionSiteList(Factory, "42", ZDateTime.Today);
			testList.Load();
			AssertEquals("Only 1 item fits the rules.", 1, testList.Count);
			AssertEquals("Should return the only 1 item.", "CNWEH42S201", testList[0].ZZD_Code);
		}

		public void TestGetTradeAgreementCodesSupportsDeclarationOfOrigin()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "01", "亚太贸易协定", new[] { (CusCodeListAttributeName.SupportsDeclarationOfOrigin, "N") });
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "02", "中国—东盟自贸协定");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "10", "中国—新西兰自贸协定", new[] { (CusCodeListAttributeName.SupportsDeclarationOfOrigin, "Y") });
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "16", "中国-冰岛自贸协定", new[] { (CusCodeListAttributeName.SupportsDeclarationOfOrigin, "Y") });

			var tradeAgreementCodes = CNRefCusCodeListTypes.GetTradeAgreementCodesSupportDeclarationOfOrigin(Factory, ZDateTime.Today);
			AssertContainsExactElementsInAnyOrder(new[] { "10", "16" }, tradeAgreementCodes.GetAllCodes());
			AssertSame("List should be cached", tradeAgreementCodes, CNRefCusCodeListTypes.GetTradeAgreementCodesSupportDeclarationOfOrigin(Factory, ZDateTime.Today));
		}

		public void TestGetSupportingDocumentsSupportsTSD()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CD3", "Code 3");
			code1.Attributes.AddNew(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, ZString.Empty);
			code2.Attributes.AddNew(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, ZString.Empty);
			code3.Attributes.AddNew(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, ZString.Empty);
			Factory.Save();

			var documentCodesSupportesTSD = CNRefCusCodeListTypes.GetSupportingDocumentsSupportsTSD(Factory, ZDateTime.Today);
			AssertContainsExactElementsInAnyOrder(new[] { "CD1", "CD2" }, documentCodesSupportesTSD.GetAllCodes());
		}
	}
}
