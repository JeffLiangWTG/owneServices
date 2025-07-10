using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusReconDeclaLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var lookups = new CusReconDeclarationLookups(reconDeclaration);
			AssertEquals(lookups.GetType(), reconDeclaration.Lookups.GetType());
		}

		public void TestMessageAndEntryStatusList()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var lookups = new CusReconDeclarationLookups(reconDeclaration);
			AssertEquals(5, lookups.MessageStatusList.Count);
			AssertEquals("ESO, OST, ORJ, OAC, CAB", lookups.MessageStatusList.CodesAsString);

			AssertEquals(5, lookups.EntryStatusList.Count);
			AssertEquals("NDC, DMS, ANT, PNR, PFL", lookups.EntryStatusList.CodesAsString);
		}

		public void TestRefundLists()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var lookups = new CusReconDeclarationLookups(reconDeclaration);
			AssertEquals(5, lookups.RefundTypeList.Count);
			AssertEquals("A, B, C, D, E", lookups.RefundTypeList.CodesAsString);

			AssertEquals(13, lookups.RefundCauseCodeList.Count);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13", lookups.RefundCauseCodeList.CodesAsString);

			AssertEquals(3, lookups.RefundReasonCodeList.Count);
			AssertEquals("01, 02, 03", lookups.RefundReasonCodeList.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "012", "성남세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "013", "인천공항세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var customsOfficeList = reconDeclaration.Lookups.CustomsOfficeList;
			customsOfficeList.Load();
			AssertEquals(3, customsOfficeList.Count);

			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "010"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울세관"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "012"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "성남세관"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "013"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "인천공항세관"));
		}

		public void TestCustomsDivisionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsDepartment, "00", "미지정 및 해당과 없음", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsDepartment, "01", "경인항지소", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsDepartment, "02", "진해센터", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var customsDivisionList = reconDeclaration.Lookups.CustomsDivisionList;
			customsDivisionList.Load();
			AssertEquals(3, customsDivisionList.Count);

			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "00"));
			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "미지정 및 해당과 없음"));
			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "01"));
			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "경인항지소"));
			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "02"));
			Assert(customsDivisionList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "진해센터"));
		}

		public void TestTaxOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "100", "서울청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "101", "종로", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "104", "남대문", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var taxOfficeList = reconDeclaration.Lookups.TaxOfficeList;
			taxOfficeList.Load();
			AssertEquals(3, taxOfficeList.Count);

			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울청"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "101"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "종로"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "104"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "남대문"));
		}
		public void TestBankTypeList()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var lookups = new CusReconDeclarationLookups(reconDeclaration);
			AssertEquals(28, lookups.BankTypeList.Count);
			AssertEquals("002, 003, 006, 007, 011, 012, 020, 023, 027, 031, 032, 034, 035, 037, 039, 045, 048, 050, 064, 071, 081, 088, 288, 431, 441, 442, 443, 452", lookups.BankTypeList.CodesAsString);
		}
	}
}
