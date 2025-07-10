using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UniversalReferenceConstantsTest : TestCaseWithFactory
	{
		public void TestGetCLVSThreshold()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var caDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var taxOrFee = helper.CreateTaxOrFee("RT1", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 20.00m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals(20.00m, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today.AddDays(-1), "RT1"));
			AssertEquals(20.00m, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today, "RT1"));
			AssertEquals(20.00m, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today.AddDays(1), "RT1"));
			AssertEquals(ZDecimal.Zero, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today.AddDays(2), "RT1"));
			AssertEquals(ZDecimal.Zero, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today.AddDays(-2), "RT1"));
			AssertEquals(ZDecimal.Zero, UniversalReferenceConstants.GetCLVSThreshold(newFactory, ZDateTime.Today, "RT2"));
		}

		public void TestIsCAMQWAR()
		{
			AssertIsFunctionalityValid(Constants.FunctionalityTypes.CAMQWAR, () => UniversalReferenceConstants.IsCAMQWAR);
		}

		public void TestIsCarmR2()
		{
			AssertIsFunctionalityValid(Constants.FunctionalityTypes.CarmR2, () => UniversalReferenceConstants.IsCarmR2);
		}

		public void TestIsCBSABOValid()
		{
			AssertIsFunctionalityValid(Constants.FunctionalityTypes.CBSABO, () => UniversalReferenceConstants.IsCBSABOValid(ZDateTime.Today));
		}

		public void TestIsRPPGrace()
		{
			AssertIsFunctionalityValid(Constants.FunctionalityTypes.RPPGrace, () => UniversalReferenceConstants.IsRPPGrace);
		}

		void AssertIsFunctionalityValid(ZString code, Func<bool> getValueToAssert)
		{
			CreateOrUpdateFUNCS(ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(14), code);
			Assert(code + ": From FUNCS, but is not within the validity period ", !getValueToAssert());

			CreateOrUpdateFUNCS(ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(14), code);
			Assert(code + ": From FUNCS, it is within the validity period ", getValueToAssert());

			var licenceKeyIdentifier = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			CreateOrUpdatePFUNC(ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(14), licenceKeyIdentifier, code);
			Assert(code + ": From PFUNC, but is not within the validity period ", !getValueToAssert());

			CreateOrUpdatePFUNC(ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(14), licenceKeyIdentifier, code);
			Assert(code + ": From PFUNC, it is within the validity period ", getValueToAssert());

			GlbCompany.CurrentCompany.GC_Code = "HXU";
			GlbCompany.CurrentCompany.UpdateLicenceKeyIdentifier();
			ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
			Assert(code + ": From FUNCS, can not find a PFUNC use the new LicenceKeyIdentifier", getValueToAssert());
		}

		void CreateOrUpdateFUNCS(ZDateTime startTime, ZDateTime endTime, ZString code)
		{
			ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Canada);
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS);
			query.AddToFilter(RefCusCodeListSchema.ZZD_Code, code);
			var cusCodeList = Factory.LoadTop1<RefCusCodeList>(query);
			if (cusCodeList == null)
			{
				cusCodeList = Factory.New<RefCusCodeList>();
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
				CreateOrUpdateCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Core.Constants.CountryCodes.Canada);
				cusCodeList.ZZD_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Canada;
				cusCodeList.ZZD_ZZK_NKCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS;
				cusCodeList.ZZD_Code = code;
			}

			cusCodeList.ZZD_Description = "Desc";
			cusCodeList.ZZD_StartDate = startTime;
			cusCodeList.ZZD_EndDate = endTime;
			Factory.Save();
		}

		RefCusCodeType CreateOrUpdateCodeType(ZString code, ZString description, ZString dataGrouping)
		{
			var query = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, code);
			query.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, dataGrouping);
			var cusCodeType = Factory.LoadTop1<RefCusCodeType>(query);
			if (cusCodeType == null)
			{
				cusCodeType = Factory.New<RefCusCodeType>();
				cusCodeType.ZZK_CodeType = code;
				cusCodeType.ZZK_ZZZ_NKDataGrouping = dataGrouping;
			}

			cusCodeType.ZZK_Description = description;
			return cusCodeType;
		}

		void CreateOrUpdatePFUNC(ZDateTime startTime, ZDateTime endTime, ZString companyKey, ZString code)
		{
			ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Canada);
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			query.AddToFilter(RefCusCodeListSchema.ZZD_Code, code);
			var cusCodeList = Factory.LoadTop1<RefCusCodeList>(query);
			if (cusCodeList == null)
			{
				cusCodeList = Factory.New<RefCusCodeList>();
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
				CreateOrUpdateCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Canada);
				cusCodeList.ZZD_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Canada;
				cusCodeList.ZZD_ZZK_NKCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC;
				cusCodeList.ZZD_Code = code;
			}

			cusCodeList.ZZD_Description = "Desc";
			cusCodeList.ZZD_StartDate = startTime;
			cusCodeList.ZZD_EndDate = endTime;

			var attributeQuery = new ZQuery(RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, cusCodeList.PK);
			attributeQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, RefCusCodeListAttributeTypes.Codes.Company);
			attributeQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, companyKey);
			var cusCodeListAttribute = Factory.LoadTop1<RefCusCodeListAttribute>(attributeQuery);
			if (cusCodeListAttribute == null)
			{
				cusCodeListAttribute = cusCodeList.Attributes.AddNew();
				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Canada);
				cusCodeListAttribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Company;
				cusCodeListAttribute.ZZE_Value = companyKey;
			}

			Factory.Save();
		}
	}
}
