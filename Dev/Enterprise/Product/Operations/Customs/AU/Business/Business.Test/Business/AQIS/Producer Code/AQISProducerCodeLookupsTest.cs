using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISProducerCodeLookupsTest : TestCaseWithFactory
	{
		public void TestAQISProducerCodeList_UseRefDatabaseDataFalse()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
				var producer1 = CMRAqisProducer.New(Factory);
				producer1.QR_AQISProducerCode = "A";
				producer1.QR_AQISProducerName = "Code A";

				var producer2 = CMRAqisProducer.New(Factory);
				producer2.QR_AQISProducerCode = "B";
				producer2.QR_AQISProducerName = "Code B";

				var list = (CMRAqisProducerCollection)new AQISProducerCode(Factory).Lookups.AQISProducerCodeList;
				list.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "A", "B" }, list.Select(x => x.QR_AQISProducerCode));
			}
		}

		public void TestAQISProducerCodeList_UseRefDatabaseDataTrue()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRPR, "AQIS Producer Code", Core.Constants.CountryCodes.Australia);
				var producerList1 = universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPR, "11111111", "REFProducerDescription1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				var producerListAttribute11 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList1.PK, "AQISProducerCountryCode", "NZ");
				var producerListAttribute12 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList1.PK, "AQISProducerLocality", "New Zealand");
				var producerList2 = universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPR, "22222222", "A REFConcernDescription2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				var producerListAttribute21 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList2.PK, "AQISProducerCountryCode", "SG");
				var producerListAttribute22 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList2.PK, "AQISProducerLocality", "Singapore");
				var producerList3 = universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPR, "33333333", "Description3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				var producerListAttribute31 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList3.PK, "AQISProducerCountryCode", "CN");
				var producerListAttribute32 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(producerList3.PK, "AQISProducerLocality", "China");
				Factory.Save();

				var aqisProducerCodeList = (ZZRefCusCodeListCombinedCollection)new AQISProducerCode(Factory).Lookups.AQISProducerCodeList;
				aqisProducerCodeList.Load();
				var filterBusinessObjectDefaults = aqisProducerCodeList.FilterBusinessObjectDefaults;
				AssertEquals("Default filters count", 3, filterBusinessObjectDefaults.Count);
				AssertEquals("Default filter 1", (ZString)AUConstants.RefCusCodeTypeCodes.CMRPR, filterBusinessObjectDefaults["List Type:Property"].Value);
				AssertEquals("Default filter 2", ZString.Empty, filterBusinessObjectDefaults["Attribute Name:Property"].Value);
				AssertEquals("Default filter 3", ZString.Empty, filterBusinessObjectDefaults["Attribute Value:Property"].Value);
				AssertContainsExactElementsInAnyOrder(new[] { "11111111", "22222222", "33333333" }, aqisProducerCodeList.Select(x => x.ZZD_Code));
			}
		}
	}
}
