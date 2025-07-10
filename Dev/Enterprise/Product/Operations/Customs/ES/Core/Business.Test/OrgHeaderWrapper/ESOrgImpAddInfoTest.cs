using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ESOrgImpAddInfo))]
	public class ESOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			return new ESOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		}

		public void TestZO_VATDeferment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = org.CountryData;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			var esAddInfo = ESOrgImpAddInfo.Get(org);
			AssertEquals(false, esAddInfo.ZO_VATDeferment);
			esAddInfo.ZO_VATDeferment = true;
			AssertEquals(true, esAddInfo.ZO_VATDeferment);
			Factory.Save();
			var newOrg = Factory.Load<OrgHeader>(org.PK);
			var newESAddInfo = ESOrgImpAddInfo.Get(newOrg);
			AssertEquals(true, newESAddInfo.ZO_VATDeferment);
		}

		public void TestZO_Retailer()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = org.CountryData;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			var esAddInfo = ESOrgImpAddInfo.Get(org);
			AssertEquals(false, esAddInfo.ZO_Retailer);
			esAddInfo.ZO_Retailer = true;
			AssertEquals(true, esAddInfo.ZO_Retailer);
			Factory.Save();
			var newOrg = Factory.Load<OrgHeader>(org.PK);
			var newESAddInfo = ESOrgImpAddInfo.Get(newOrg);
			AssertEquals(true, newESAddInfo.ZO_Retailer);
		}

		public void TestZO_MethodOfPayment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = org.CountryData;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			var esAddInfo = ESOrgImpAddInfo.Get(org);
			AssertEquals("", esAddInfo.ZO_MethodOfPayment);
			esAddInfo.ZO_MethodOfPayment = "A";
			AssertEquals("A", esAddInfo.ZO_MethodOfPayment);
			Factory.Save();
			var newOrg = Factory.Load<OrgHeader>(org.PK);
			var newESAddInfo = ESOrgImpAddInfo.Get(newOrg);
			AssertEquals("A", newESAddInfo.ZO_MethodOfPayment);
		}

		public void TestZO_MethodOfPaymentCan()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = org.CountryData;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			var esAddInfo = ESOrgImpAddInfo.Get(org);
			AssertEquals("", esAddInfo.ZO_MethodOfPaymentCan);
			esAddInfo.ZO_MethodOfPaymentCan = "C";
			AssertEquals("C", esAddInfo.ZO_MethodOfPaymentCan);
			Factory.Save();
			var newOrg = Factory.Load<OrgHeader>(org.PK);
			var newESAddInfo = ESOrgImpAddInfo.Get(newOrg);
			AssertEquals("C", newESAddInfo.ZO_MethodOfPaymentCan);
		}
	}
}
