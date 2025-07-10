using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaBillCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills;
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("ABL_BolType default value should be STD", "STD", bill.ABL_BolType);

			AssertEquals("ABL_RX_NKFreightValueCurrency default value should be USD", "USD", bill.ABL_RX_NKFreightValueCurrency);
			AssertEquals("ABL_RX_NKTransportValueCurrency default value should be USD", "USD", bill.ABL_RX_NKTransportValueCurrency);
			AssertEquals("ABL_RX_NKInsuranceValueCurrency default value should be USD", "USD", bill.ABL_RX_NKInsuranceValueCurrency);
			AssertEquals("DiscountValueCurrency default value should be USD", "USD", bill.DiscountValueCurrency);
			AssertEquals("OtherChargesValueCurrency default value should be USD", "USD", bill.OtherChargesValueCurrency);
			AssertEquals("ABL_RX_NKCustomsValueCurrency default value should be USD", "USD", bill.ABL_RX_NKCustomsValueCurrency);
		}
	}
}
