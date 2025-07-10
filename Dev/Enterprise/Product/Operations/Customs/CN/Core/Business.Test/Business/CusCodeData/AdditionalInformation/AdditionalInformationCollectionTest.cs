using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AdditionalInformationCollection))]
	class AdditionalInformationCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AdditionalInformation>
	{
		public void TestGetAndSetValue()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(ZString.Empty, pivot.AdditionalInformationCodes.GetValue("00000"));
			pivot.AdditionalInformationCodes.SetValue("00000", "XXX");
			AssertEquals("XXX", pivot.AdditionalInformationCodes["00000"].CY_Data);
			AssertEquals("XXX", pivot.AdditionalInformationCodes.GetValue("00000"));
		}

		public void TestCleanByTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff1 = helper.CreateCustomsTariff("2713200010", "00000", "00423", "99999");
			var tariff2 = helper.CreateCustomsTariff("2713200090", "00000", "00352", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var code1 = pivot.AdditionalInformationCodes.AddNew("00000", "111");
			var code2 = pivot.AdditionalInformationCodes.AddNew("00423", "222");
			var code3 = pivot.AdditionalInformationCodes.AddNew("00352", "333");
			var code4 = pivot.AdditionalInformationCodes.AddNew("99999", "444");
			pivot.AdditionalInformationCodes.CleanByTariff(tariff1, pivot.IsEnteringOrExiting);
			Assert("00352 should be deleted", code3.IsDeleted);
			pivot.AdditionalInformationCodes.SetValue("99999", "");
			pivot.AdditionalInformationCodes.CleanByTariff(tariff2, pivot.IsEnteringOrExiting);
			AssertEquals(1, pivot.AdditionalInformationCodes.Count);
			Assert("00423 should be deleted", code2.IsDeleted);
			Assert("99999 should be deleted", code4.IsDeleted);
			pivot.AdditionalInformationCodes.AddNew("00423", "222");
			pivot.AdditionalInformationCodes.AddNew("00352", "333");
			pivot.AdditionalInformationCodes.AddNew("99999", "444");
			pivot.AdditionalInformationCodes.CleanByTariff(null, pivot.IsEnteringOrExiting);
			AssertEquals(1, pivot.AdditionalInformationCodes.Count);
			Assert("00000 should NOT be deleted", !code1.IsDeleted);
		}

		protected override CusCodeDataCollection<AdditionalInformation> GetCusCodeDataCollection()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			return new AdditionalInformationCollection(pivot);
		}
	}
}
