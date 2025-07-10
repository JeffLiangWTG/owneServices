using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class REAUniversalRateCalcDataTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Should be exception when param: invoiceLine is null", () => new REAUniversalRateCalcData(null));
				AssertNoExceptionThrown("No exception expected", () => new REAUniversalRateCalcData(invoiceLine));
			});
		}

		[TestDate(2020, 1, 31)]
		public void TestDateOfValuation()
		{
			CombineAssertions(() =>
			{
				var rateCalcData = new REAUniversalRateCalcData(invoiceLine);
				AssertEquals("DateOfValuation today since there is no invoice header", TestDateAttribute.Date, rateCalcData.DateOfValuation);

				var invoiceHeader = Factory.New<JobComInvoiceHeader>();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2016, 2, 29);
				invoiceLine.JI_JZ = invoiceHeader.PK;
				rateCalcData = new REAUniversalRateCalcData(invoiceLine);
				AssertEquals("DateOfValuation is invoice header's date", invoiceHeader.JZ_ValuationDateOverride, rateCalcData.DateOfValuation);
			});
		}

		public void TestValueForDuty()
		{
			invoiceLine.JI_CustomsQuantity = 3;
			invoiceLine.JI_CustomsUnitQty = CustomsUq.Weight.Kilogram;

			CombineAssertions(() =>
			{
				var rateCalcData = new REAUniversalRateCalcData(invoiceLine);
				AssertEquals("ValueForDuty", 0m, rateCalcData.ValueForDuty);

				invoiceLine.CusEntryLine.CL_CustomsValue = 2.34;
				rateCalcData = new REAUniversalRateCalcData(invoiceLine);
				AssertEquals("ValueForDuty", 2.34m, rateCalcData.ValueForDuty);

				rateCalcData.CustomsValueFormula = "1.23456 * [KGM]";
				AssertEquals("ValueForDuty", 3.704m, rateCalcData.ValueForDuty);
			});
		}

		public void TestCustomsValue()
		{
			invoiceLine.CusEntryLine.CL_CustomsValue = 50m;
			var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
			AssertEquals(50m, exciseUniversalRateData.CustomsValue);
		}

		public void TestCountrySpecificValueList()
		{
			var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
			AssertEquals("CountrySpecificValueList is empty", false, exciseUniversalRateData.CountrySpecificValueList.Any());
		}

		public void TestUOMList_CustomsQuantity()
		{
			AssertUOMList((value) => invoiceLine.JI_CustomsUnitQty = value);
		}

		public void TestUOMList_SecondQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertUOMList((value) => invoiceLine.JI_CustomsSecondUnitQty = value);
		}

		public void TestUOMList_ThirdQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertUOMList((value) => invoiceLine.JI_CustomsThirdUnitQty = value);
		}

		public void TestUOMList_FourthQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertUOMList((value) => invoiceLine.JI_CustomsFourthUnitQty = value);
		}

		void AssertUOMList(Action<ZString> setUnitQty)
		{
			CombineAssertions(() =>
			{
				setUnitQty(CustomsUq.Weight.Gram);
				var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				var uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code KGM", uomList.ContainsKey(CustomsUq.Weight.Kilogram));
				Assert("UnitOfMeasureValueList contains code TNE", uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList contains code KG", uomList.ContainsKey(Core.Constants.Weight.Kilograms));
				Assert("UnitOfMeasureValueList contains code GRM", uomList.ContainsKey(CustomsUq.Weight.Gram));
				Assert("UnitOfMeasureValueList contains code DTN", uomList.ContainsKey(CustomsUq.Weight.Hectokilogram));
				Assert("UnitOfMeasureValueList doesn't contain code HLT", !uomList.ContainsKey(CustomsUq.Volume.Hectolitre));

				setUnitQty(CustomsUq.Volume.Litre);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code LTR", uomList.ContainsKey(CustomsUq.Volume.Litre));
				Assert("UnitOfMeasureValueList contains code HLT", uomList.ContainsKey(CustomsUq.Volume.Hectolitre));
				Assert("UnitOfMeasureValueList contains code KLT", uomList.ContainsKey(CustomsUq.Volume.Kilolitre));
				Assert("UnitOfMeasureValueList doesn't contain code TNE", !uomList.ContainsKey(CustomsUq.Weight.Tonne));

				setUnitQty(CustomsUq.Number.NumberOfItems);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code MIL", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));
				Assert("UnitOfMeasureValueList contains code NAR", uomList.ContainsKey(CustomsUq.Number.NumberOfItems));
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));

				setUnitQty(CustomsUq.Alcohol.LitrePureAlcohol);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code LPA", uomList.ContainsKey(CustomsUq.Alcohol.LitrePureAlcohol));
				Assert("UnitOfMeasureValueList contains code ASVX", uomList.ContainsKey(CustomsUq.Alcohol.PercentageVolumeHectolitre));
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				setUnitQty(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code HG", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				setUnitQty(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code HP", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList doesn't contain code HG", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));

				setUnitQty(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules);
				exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code GJ", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules));
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
			});
		}

		public void TestWeightConversion_CustomsQuantity()
		{
			AssertWeightConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsUnitQty = unit;
				invoiceLine.JI_CustomsQuantity = value;
			});
		}

		public void TestWeightConversion_SecondQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertWeightConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsSecondUnitQty = unit;
				invoiceLine.JI_CustomsSecondQuantity = value;
			});
		}

		public void TestWeightConversion_ThirdQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertWeightConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = unit;
				invoiceLine.JI_CustomsThirdQuantity = value;
			});
		}

		public void TestWeightConversion_FourthQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertWeightConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsFourthUnitQty = unit;
				invoiceLine.JI_CustomsFourthQuantity = value;
			});
		}

		void AssertWeightConversion(Action<ZString, ZDecimal> setUnitQtyAndValue)
		{
			setUnitQtyAndValue(CustomsUq.Weight.Gram, 30000);

			var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("1000 GRM equals to 1 KGM", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Kilogram]);
				AssertEquals("1000000 GRM equals to 1 TNE", 0.03m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Tonne]);
				AssertEquals("100000 GRM equals to 1 DTN", 0.3m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Hectokilogram]);
				AssertEquals("1000 GRM equals to 1 KG", 30m, exciseUniversalRateData.UnitOfMeasureValueList[Core.Constants.Weight.Kilograms]);
			});
		}

		public void TestVolumeConversion_CustomsQuantity()
		{
			AssertVolumeConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsUnitQty = unit;
				invoiceLine.JI_CustomsQuantity = value;
			});
		}

		public void TestVolumeConversion_SecondQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertVolumeConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsSecondUnitQty = unit;
				invoiceLine.JI_CustomsSecondQuantity = value;
			});
		}

		public void TestVolumeConversion_ThirdQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertVolumeConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = unit;
				invoiceLine.JI_CustomsThirdQuantity = value;
			});
		}

		public void TestVolumeConversion_FourthQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertVolumeConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsFourthUnitQty = unit;
				invoiceLine.JI_CustomsFourthQuantity = value;
			});
		}

		void AssertVolumeConversion(Action<ZString, ZDecimal> setUnitQtyAndValue)
		{
			setUnitQtyAndValue(CustomsUq.Volume.Litre, 30000);

			var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("100 LTR equals to 1 HLT", 300m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Hectolitre]);
				AssertEquals("1000 LTR equals to 1 KLT", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Kilolitre]);
			});
		}

		public void TestItemConversion_CustomsQuantity()
		{
			AssertItemConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsUnitQty = unit;
				invoiceLine.JI_CustomsQuantity = value;
			});
		}

		public void TestItemConversion_SecondQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertItemConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsSecondUnitQty = unit;
				invoiceLine.JI_CustomsSecondQuantity = value;
			});
		}

		public void TestItemConversion_ThirdQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertItemConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = unit;
				invoiceLine.JI_CustomsThirdQuantity = value;
			});
		}

		public void TestItemConversion_FourthQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertItemConversion((unit, value) =>
			{
				invoiceLine.JI_CustomsFourthUnitQty = unit;
				invoiceLine.JI_CustomsFourthQuantity = value;
			});
		}

		void AssertItemConversion(Action<ZString, ZDecimal> setUnitQtyAndValue)
		{
			setUnitQtyAndValue(CustomsUq.Number.NumberOfItems, 3000);

			var exciseUniversalRateData = new REAUniversalRateCalcData(invoiceLine);
			AssertEquals("1000 NAR equals to 1 MIL", 3m, exciseUniversalRateData.UnitOfMeasureValueList[UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
		}
		JobComInvoiceLine invoiceLine;
	}
}
