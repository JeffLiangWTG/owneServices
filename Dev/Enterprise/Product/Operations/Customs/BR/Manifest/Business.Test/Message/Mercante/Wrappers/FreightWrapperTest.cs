using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	class FreightWrapperTest : TestCaseWithFactory
	{
		public void TestFreightWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			PopulateTaxes(bill);

			IManifest wrapper = new ManifestWrapper(header);
			AssertEquals(2, wrapper.Bills.ElementAt(0).Freights.Count);

			var freight = wrapper.Bills.ElementAt(0).Freights.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals("001", freight.ComponentsCode);
				AssertEquals("110", freight.AmountCurrency);
				AssertEquals(200.33m, freight.Amount);	
				AssertEquals("P", freight.PaymentMode);
			});

			freight = wrapper.Bills.ElementAt(0).Freights.ElementAt(1);
			CombineAssertions(() =>
			{
				AssertEquals("017", freight.ComponentsCode);
				AssertEquals("220", freight.AmountCurrency);
				AssertEquals(100m, freight.Amount);
				AssertEquals("C", freight.PaymentMode);
			});
		}

		void PopulateTaxes(AsycudaBill bill)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "OUT", RefCusMapTypeList.Codes.Currency, true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, Core.Constants.CurrencyCodes.UnitedStates, "220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, Core.Constants.CurrencyCodes.Brazil, "110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var tax = bill.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeCodeList.Codes._001;
			tax.AET_ChargeAmount = 200.33;
			tax.AET_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			tax.AET_MethodOfPayment = Core.Constants.PaymentType.Prepaid;

			tax = bill.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeCodeList.Codes._017;
			tax.AET_ChargeAmount = 100;
			tax.AET_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			tax.AET_MethodOfPayment = Core.Constants.PaymentType.Collect;
		}
	}
}
