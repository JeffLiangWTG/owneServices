using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class ExciseUniversalRateCalcDataTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Should be exception when param: entryLine is null", () => new ExciseUniversalRateCalcData(null));
				AssertNoExceptionThrown("No exception expected", () => new ExciseUniversalRateCalcData(entryLine));
			});
		}

		[TestDate(2020, 1, 31)]
		public void TestDateOfValuation()
		{
			CombineAssertions(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				var rateCalcData = new ExciseUniversalRateCalcData(entryLine);
				AssertEquals("DateOfValuation", TestDateAttribute.Date, rateCalcData.DateOfValuation);

				var invHeader = Factory.New<JobComInvoiceHeader>();
				invHeader.JZ_ValuationDateOverride = new ZDateTime(2016, 2, 29);
				entryLine.InvoiceLines.Add(invHeader.JobComInvoiceLines.AddNew());
				rateCalcData = new ExciseUniversalRateCalcData(entryLine);
				AssertEquals("DateOfValuation", invHeader.JZ_ValuationDateOverride, rateCalcData.DateOfValuation);
			});
		}

		public void TestValueForDuty()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 3;
			invLine.JI_CustomsUnitQty = CustomsUq.Weight.Kilogram;
			entryLine.InvoiceLines.Add(invLine);

			CombineAssertions(() =>
			{
				var rateCalcData = new ExciseUniversalRateCalcData(entryLine);
				AssertEquals("ValueForDuty", 0m, rateCalcData.ValueForDuty);

				entryLine.CL_CustomsValue = 2.34;
				rateCalcData = new ExciseUniversalRateCalcData(entryLine);
				AssertEquals("ValueForDuty", 2.34m, rateCalcData.ValueForDuty);

				rateCalcData.CustomsValueFormula = "1.23456 * [KGM]";
				AssertEquals("ValueForDuty", 3.704m, rateCalcData.ValueForDuty);
			});
		}

		public void TestCustomsValue()
		{
			entryLine.CL_CustomsValue = 50m;
			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			AssertEquals(50m, exciseUniversalRateData.CustomsValue);
		}

		public void TestCountrySpecificValueList()
		{
			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			Assert("CountrySpecificValueList contains code PVP", exciseUniversalRateData.CountrySpecificValueList.ContainsKey(UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode));
		}

		public void TestUOMList_CustomsQuantity()
		{
			CombineAssertions(() =>
			{
				var invLine = Factory.New<JobComInvoiceLine>();
				invLine.JI_CustomsUnitQty = CustomsUq.Weight.Gram;
				entryLine.InvoiceLines.Add(invLine);
				var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				var uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code KGM", uomList.ContainsKey(CustomsUq.Weight.Kilogram));
				Assert("UnitOfMeasureValueList contains code TNE", uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList doesn't contain code HLT", !uomList.ContainsKey(CustomsUq.Volume.Hectolitre));

				invLine.JI_CustomsUnitQty = CustomsUq.Volume.Litre;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code TNE", !uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList contains code HLT", uomList.ContainsKey(CustomsUq.Volume.Hectolitre));
				Assert("UnitOfMeasureValueList contains code KLT", uomList.ContainsKey(CustomsUq.Volume.Kilolitre));

				invLine.JI_CustomsUnitQty = CustomsUq.Number.NumberOfItems;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code MIL", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				invLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));
				Assert("UnitOfMeasureValueList contains code HG", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));

				invLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HG", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));
				Assert("UnitOfMeasureValueList contains code HP", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));

				invLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code GJ", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules));
			});
		}

		public void TestUOMList_SecondQuantity()
		{
			CombineAssertions(() =>
			{
				var invLine = Factory.New<JobComInvoiceLine>();
				invLine.JI_CustomsUnitQty = ZString.Empty;
				invLine.JI_CustomsSecondUnitQty = CustomsUq.Weight.Gram;
				entryLine.InvoiceLines.Add(invLine);
				var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				var uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code KGM", uomList.ContainsKey(CustomsUq.Weight.Kilogram));
				Assert("UnitOfMeasureValueList contains code TNE", uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList doesn't contain code HLT", !uomList.ContainsKey(CustomsUq.Volume.Hectolitre));

				invLine.JI_CustomsSecondUnitQty = CustomsUq.Volume.Litre;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code TNE", !uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList contains code HLT", uomList.ContainsKey(CustomsUq.Volume.Hectolitre));
				Assert("UnitOfMeasureValueList contains code KLT", uomList.ContainsKey(CustomsUq.Volume.Kilolitre));

				invLine.JI_CustomsSecondUnitQty = CustomsUq.Number.NumberOfItems;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code MIL", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				invLine.JI_CustomsSecondUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));
				Assert("UnitOfMeasureValueList contains code HG", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));

				invLine.JI_CustomsSecondUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HG", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));
				Assert("UnitOfMeasureValueList contains code HP", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));

				invLine.JI_CustomsSecondUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code GJ", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules));
			});
		}

		public void TestUOMList_ThirdQuantity()
		{
			CombineAssertions(() =>
			{
				var invLine = Factory.New<JobComInvoiceLine>();
				invLine.JI_CustomsUnitQty = ZString.Empty;
				invLine.JI_CustomsThirdUnitQty = CustomsUq.Weight.Gram;
				entryLine.InvoiceLines.Add(invLine);
				var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				var uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code KGM", uomList.ContainsKey(CustomsUq.Weight.Kilogram));
				Assert("UnitOfMeasureValueList contains code TNE", uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList doesn't contain code HLT", !uomList.ContainsKey(CustomsUq.Volume.Hectolitre));

				invLine.JI_CustomsThirdUnitQty = CustomsUq.Volume.Litre;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code TNE", !uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList contains code HLT", uomList.ContainsKey(CustomsUq.Volume.Hectolitre));
				Assert("UnitOfMeasureValueList contains code KLT", uomList.ContainsKey(CustomsUq.Volume.Kilolitre));

				invLine.JI_CustomsThirdUnitQty = CustomsUq.Number.NumberOfItems;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code MIL", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				invLine.JI_CustomsThirdUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));
				Assert("UnitOfMeasureValueList contains code HG", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));

				invLine.JI_CustomsThirdUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HG", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));
				Assert("UnitOfMeasureValueList contains code HP", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));

				invLine.JI_CustomsThirdUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code GJ", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules));
			});
		}

		public void TestUOMList_FourthQuantity()
		{
			CombineAssertions(() =>
			{
				var invLine = Factory.New<JobComInvoiceLine>();
				invLine.JI_CustomsUnitQty = ZString.Empty;
				invLine.JI_CustomsFourthUnitQty = CustomsUq.Weight.Gram;
				entryLine.InvoiceLines.Add(invLine);
				var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				var uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList contains code KGM", uomList.ContainsKey(CustomsUq.Weight.Kilogram));
				Assert("UnitOfMeasureValueList contains code TNE", uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList doesn't contain code HLT", !uomList.ContainsKey(CustomsUq.Volume.Hectolitre));

				invLine.JI_CustomsFourthUnitQty = CustomsUq.Volume.Litre;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code TNE", !uomList.ContainsKey(CustomsUq.Weight.Tonne));
				Assert("UnitOfMeasureValueList contains code HLT", uomList.ContainsKey(CustomsUq.Volume.Hectolitre));
				Assert("UnitOfMeasureValueList contains code KLT", uomList.ContainsKey(CustomsUq.Volume.Kilolitre));

				invLine.JI_CustomsFourthUnitQty = CustomsUq.Number.NumberOfItems;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code MIL", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));

				invLine.JI_CustomsFourthUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code MIL", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems));
				Assert("UnitOfMeasureValueList contains code HG", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));

				invLine.JI_CustomsFourthUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HG", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.AlcoholHectolitres));
				Assert("UnitOfMeasureValueList contains code HP", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));

				invLine.JI_CustomsFourthUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules;
				exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
				uomList = exciseUniversalRateData.UnitOfMeasureValueList;
				Assert("UnitOfMeasureValueList doesn't contain code HP", !uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.HectolitresPlate));
				Assert("UnitOfMeasureValueList contains code GJ", uomList.ContainsKey(UniversalReferenceConstants.RefCusCodeList.CustomsUq.GigaJoules));
			});
		}

		public void TestWeightConversion_CustomsQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 30000;
			invLine.JI_CustomsUnitQty = CustomsUq.Weight.Gram;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("1000 GRM equals to 1 KGM", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Kilogram]);
				AssertEquals("1000000 GRM equals to 1 TNE", 0.03m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Tonne]);
			});
		}

		public void TestWeightConversion_SecondQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsUnitQty = ZString.Empty;
			invLine.JI_CustomsSecondQuantity = 30000;
			invLine.JI_CustomsSecondUnitQty = CustomsUq.Weight.Gram;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("1000 GRM equals to 1 KGM", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Kilogram]);
				AssertEquals("1000000 GRM equals to 1 TNE", 0.03m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Tonne]);
			});
		}

		public void TestWeightConversion_ThirdQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsUnitQty = ZString.Empty;
			invLine.JI_CustomsThirdQuantity = 30000;
			invLine.JI_CustomsThirdUnitQty = CustomsUq.Weight.Gram;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("1000 GRM equals to 1 KGM", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Kilogram]);
				AssertEquals("1000000 GRM equals to 1 TNE", 0.03m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Tonne]);
			});
		}

		public void TestWeightConversion_FourthQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsUnitQty = ZString.Empty;
			invLine.JI_CustomsFourthQuantity = 30000;
			invLine.JI_CustomsFourthUnitQty = CustomsUq.Weight.Gram;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("1000 GRM equals to 1 KGM", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Kilogram]);
				AssertEquals("1000000 GRM equals to 1 TNE", 0.03m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Weight.Tonne]);
			});
		}

		public void TestVolumeConversion_CustomsQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 30000;
			invLine.JI_CustomsUnitQty = CustomsUq.Volume.Litre;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("100 LTR equals to 1 HLT", 300m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Hectolitre]);
				AssertEquals("1000 LTR equals to 1 KLT", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Kilolitre]);
			});
		}

		public void TestVolumeConversion_SecondQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsSecondQuantity = 30000;
			invLine.JI_CustomsSecondUnitQty = CustomsUq.Volume.Litre;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("100 LTR equals to 1 HLT", 300m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Hectolitre]);
				AssertEquals("1000 LTR equals to 1 KLT", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Kilolitre]);
			});
		}

		public void TestVolumeConversion_ThirdQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsThirdQuantity = 30000;
			invLine.JI_CustomsThirdUnitQty = CustomsUq.Volume.Litre;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("100 LTR equals to 1 HLT", 300m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Hectolitre]);
				AssertEquals("1000 LTR equals to 1 KLT", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Kilolitre]);
			});
		}

		public void TestVolumeConversion_FourthQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsFourthQuantity = 30000;
			invLine.JI_CustomsFourthUnitQty = CustomsUq.Volume.Litre;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("100 LTR equals to 1 HLT", 300m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Hectolitre]);
				AssertEquals("1000 LTR equals to 1 KLT", 30m, exciseUniversalRateData.UnitOfMeasureValueList[CustomsUq.Volume.Kilolitre]);
			});
		}

		public void TestItemConversion_CustomsQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 3000;
			invLine.JI_CustomsUnitQty = CustomsUq.Number.NumberOfItems;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			AssertEquals("1000 NAR equals to 1 MIL", 3m, exciseUniversalRateData.UnitOfMeasureValueList[UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems]);
		}

		public void TestItemConversion_SecondQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsSecondQuantity = 3000;
			invLine.JI_CustomsSecondUnitQty = CustomsUq.Number.NumberOfItems;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			AssertEquals("1000 NAR equals to 1 MIL", 3m, exciseUniversalRateData.UnitOfMeasureValueList[UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems]);
		}

		public void TestItemConversion_ThirdQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsThirdQuantity = 3000;
			invLine.JI_CustomsThirdUnitQty = CustomsUq.Number.NumberOfItems;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			AssertEquals("1000 NAR equals to 1 MIL", 3m, exciseUniversalRateData.UnitOfMeasureValueList[UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems]);
		}

		public void TestItemConversion_FourthQuantity()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsFourthQuantity = 3000;
			invLine.JI_CustomsFourthUnitQty = CustomsUq.Number.NumberOfItems;
			entryLine.InvoiceLines.Add(invLine);

			var exciseUniversalRateData = new ExciseUniversalRateCalcData(entryLine);
			AssertEquals("1000 NAR equals to 1 MIL", 3m, exciseUniversalRateData.UnitOfMeasureValueList[UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLine = Factory.New<CusEntryLine>();
		}
		CusEntryLine entryLine;
	}
}
