using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISCommodityCodeLookupsTest : TestCaseWithFactory
	{
		public void TestZA_AQISCommodityCode_List()
		{
			var commodityCode = new AQISCommodityCode(Factory);
			AssertNotNull("Commodity Code List", commodityCode.Lookups.AQISCommodityCodeList);
		}

		public void TestCommodityCodeListWithRecords()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var commodity = CMRAqisCommodity.New(Factory);
				commodity.QC_AQISCommodityCode = "Code";
				commodity.QC_AQISCommodityDescription = "Description";
				Factory.Save();

				var commodityCode = new AQISCommodityCode(Factory);
				var commodityList = commodityCode.Lookups.AQISCommodityCodeList;
				AssertEquals("Code - Description", commodityList.ElementsAsString);
			}

			Factory.ClearCachedValue<CodeDescriptionPairList>("AQISCommodityCodeLookups.AQISCommodityCodeList");

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRAC, "AQIS Commodity Code Type", Core.Constants.CountryCodes.Australia);
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAC, "REFCode", "REFDescription", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var commodityList = new AQISCommodityCode(Factory).Lookups.AQISCommodityCodeList;
				AssertEquals("REFCode - REFDescription", commodityList.ElementsAsString);
			}
		}

		public void TestCommodityCodeListSortOrder()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var commodity1 = CMRAqisCommodity.New(Factory);
				commodity1.QC_AQISCommodityCode = "Cod1";
				commodity1.QC_AQISCommodityDescription = "Description";

				var commodity2 = CMRAqisCommodity.New(Factory);
				commodity2.QC_AQISCommodityCode = "Cod2";
				commodity2.QC_AQISCommodityDescription = "A Description";
				Factory.Save();

				var commodityCode = new AQISCommodityCode(Factory);
				var commodityList = commodityCode.Lookups.AQISCommodityCodeList;
				AssertEquals("Elements are sorted by description", "Cod2, Cod1", commodityList.CodesAsString);
			}

			Factory.ClearCachedValue<CodeDescriptionPairList>("AQISCommodityCodeLookups.AQISCommodityCodeList");

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRAC, "AQIS Commodity Code Type", Core.Constants.CountryCodes.Australia);
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAC, "REFCod1", "REFDescription", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAC, "REFCod2", "A REFDescription", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAC, "REFCod3", "Description", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var commodityList = new AQISCommodityCode(Factory).Lookups.AQISCommodityCodeList;
				AssertEquals("Elements are sorted by description", "REFCod2, REFCod3, REFCod1", commodityList.CodesAsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
		}
		UniversalReferenceTestDataHelper universalDataHelper;
	}
}
