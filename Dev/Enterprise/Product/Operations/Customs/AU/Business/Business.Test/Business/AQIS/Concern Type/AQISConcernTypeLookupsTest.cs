using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISConcernTypeLookupsTest : TestCaseWithFactory
	{
		public void TestZA_AQISConcernType_List()
		{
			AQISConcernType concernType = new AQISConcernType(Factory);
			AssertNotNull("Concern Type List", concernType.Lookups.AQISConcernTypeList);
		}

		public void TestConcernTypeListWithRecords()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var concern = CMRAqisConcern.New(Factory);
				concern.QN_AQISConcernType = "Type";
				concern.QN_AQISConcernDescription = "Description";
				Factory.Save();

				var concernType = new AQISConcernType(Factory);
				var concernList = concernType.Lookups.AQISConcernTypeList;
				AssertNotNull("Concern List", concernList);
				AssertEquals("Has at least one in list", true, concernList.Count > 0);
				AssertEquals("Type", "Type", concernList.GetCodeFromDescription("Description"));
				AssertEquals("Description", "Description", concernList.GetDescriptionFromCode("Type"));
			}

			Factory.ClearCachedValue<CodeDescriptionPairList>("AQISConcernTypeLookups.AQISConcernCodeList");

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRCN, "AQIS Concern Code Type", Core.Constants.CountryCodes.Australia);
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRCN, "REFConcernType", "REFConcernDescription", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var aqisConcernList = new AQISConcernType(Factory).Lookups.AQISConcernTypeList;
				AssertEquals("REFConcernType - REFConcernDescription", aqisConcernList.ElementsAsString);
			}
		}

		public void TestConcernTypeListSortOrder()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var concern = CMRAqisConcern.New(Factory);
				concern.QN_AQISConcernType = "Type";
				concern.QN_AQISConcernDescription = "Description";

				var concern1 = CMRAqisConcern.New(Factory);
				concern1.QN_AQISConcernType = "ATyp";
				concern1.QN_AQISConcernDescription = "A Description";

				Factory.Save();

				var concernType = new AQISConcernType(Factory);
				var concernList = concernType.Lookups.AQISConcernTypeList;
				AssertNotNull("Concern List", concernList);
				AssertEquals("Has at least one in list", true, concernList.Count > 0);
				AssertEquals("First in list", "A Description", concernList[0].Description);
				AssertEquals("Second in list", "Description", concernList[1].Description);
			}

			Factory.ClearCachedValue<CodeDescriptionPairList>("AQISConcernTypeLookups.AQISConcernCodeList");

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRCN, "AQIS Concern Type", Core.Constants.CountryCodes.Australia);
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRCN, "REFConcernType1", "REFConcernDescription1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRCN, "REFConcernType2", "A REFConcernDescription2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRCN, "REFConcernType3", "Description3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var aqisConcernTypeList = new AQISConcernType(Factory).Lookups.AQISConcernTypeList;
				AssertEquals("Elements are sorted by description", "REFConcernType2, REFConcernType3, REFConcernType1", aqisConcernTypeList.CodesAsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisConcern.Schema.TableName);
			universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
		}
		UniversalReferenceTestDataHelper universalDataHelper;
	}
}
