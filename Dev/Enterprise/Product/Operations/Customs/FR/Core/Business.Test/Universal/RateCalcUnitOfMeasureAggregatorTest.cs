using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class RateCalcUnitOfMeasureAggregatorTest : TestCaseWithFactory
	{
		public void TestGetUomQtyDictionary()
		{
			var invLine1 = Factory.New<JobComInvoiceLine>();
			invLine1.JI_CustomsQuantity = 90;
			invLine1.JI_CustomsUnitQty = "001";
			invLine1.JI_CustomsSecondQuantity = 2;
			invLine1.JI_CustomsSecondUnitQty = "003";
			invLine1.JI_CustomsThirdQuantity = 100000;
			invLine1.JI_CustomsThirdUnitQty = "001";
			var invLine2 = Factory.New<JobComInvoiceLine>();
			invLine2.JI_CustomsQuantity = 30;
			invLine2.JI_CustomsUnitQty = "001";
			invLine2.JI_CustomsSecondQuantity = 40;
			invLine2.JI_CustomsSecondUnitQty = "004";
			invLine2.JI_CustomsThirdQuantity = 500000;
			invLine2.JI_CustomsThirdUnitQty = "004";
			var invLine3 = Factory.New<JobComInvoiceLine>();
			invLine3.JI_CustomsQuantity = 0.5;
			invLine3.JI_CustomsUnitQty = "005";
			invLine3.JI_CustomsSecondQuantity = 120;
			invLine3.JI_CustomsSecondUnitQty = "004";
			invLine3.JI_CustomsThirdQuantity = 3;
			invLine3.JI_CustomsThirdUnitQty = "003";
			var invLine4 = Factory.New<JobComInvoiceLine>();
			invLine4.JI_CustomsQuantity = 0;
			invLine4.JI_CustomsUnitQty = "001";
			invLine4.JI_CustomsSecondQuantity = 0.8;
			invLine4.JI_CustomsSecondUnitQty = "006";
			var invLine5 = Factory.New<JobComInvoiceLine>();
			invLine5.JI_CustomsQuantity = 10;
			invLine5.JI_CustomsUnitQty = "002";
			var invLine6 = Factory.New<JobComInvoiceLine>();
			invLine6.JI_CustomsQuantity = 20;
			invLine6.JI_CustomsUnitQty = "HMT";
			var invLine7 = Factory.New<JobComInvoiceLine>();
			invLine7.JI_CustomsQuantity = 30;
			invLine7.JI_CustomsUnitQty = "MTK";
			var invLine8 = Factory.New<JobComInvoiceLine>();
			invLine8.JI_CustomsQuantity = 40;
			invLine8.JI_CustomsUnitQty = "MWH";
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.InvoiceLines.Add(invLine2);
			entryLine.InvoiceLines.Add(invLine3);
			entryLine.InvoiceLines.Add(invLine4);
			entryLine.InvoiceLines.Add(invLine5);
			entryLine.InvoiceLines.Add(invLine6);
			entryLine.InvoiceLines.Add(invLine7);
			entryLine.InvoiceLines.Add(invLine8);

			var uomQtyAggregator = new RateCalcUnitOfMeasureAggregator();
			var uomQtyDictionary = uomQtyAggregator.GetUomQtyDictionary(entryLine);

			CombineAssertions("UnitOfMeasureValueList", () =>
			{
				AssertEquals("Count", 50, uomQtyDictionary.Count);
				uomQtyDictionary.AssertDictionaryValue("001", 120m);
				uomQtyDictionary.AssertDictionaryValue("002", 10m);
				uomQtyDictionary.AssertDictionaryValue("003", 5m);
				uomQtyDictionary.AssertDictionaryValue("004", 160m);
				uomQtyDictionary.AssertDictionaryValue("005", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("006", 0.8m);
				uomQtyDictionary.AssertDictionaryValue("007", 0.5m);

				uomQtyDictionary.AssertDictionaryValue("ASV", 50m);
				uomQtyDictionary.AssertDictionaryValue("CCT", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("CEN", 0.1m);
				uomQtyDictionary.AssertDictionaryValue("CTM", 2500000m);
				uomQtyDictionary.AssertDictionaryValue("DHS", 500m);
				uomQtyDictionary.AssertDictionaryValue("DTN", 5m);
				uomQtyDictionary.AssertDictionaryValue("GFI", 500000m);
				uomQtyDictionary.AssertDictionaryValue("GRM", 500000m);
				uomQtyDictionary.AssertDictionaryValue("HLT", 0.12m);
				uomQtyDictionary.AssertDictionaryValue("HMT", 20m);
				uomQtyDictionary.AssertDictionaryValue("KCC", 500m);
				uomQtyDictionary.AssertDictionaryValue("KCL", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("KGM", 500m);
				uomQtyDictionary.AssertDictionaryValue("KLT", 0.012m);
				uomQtyDictionary.AssertDictionaryValue("KMA", 500m);
				uomQtyDictionary.AssertDictionaryValue("KMT", 2m);
				uomQtyDictionary.AssertDictionaryValue("KNI", 500m);
				uomQtyDictionary.AssertDictionaryValue("KNS", 500m);
				uomQtyDictionary.AssertDictionaryValue("KPH", 500m);
				uomQtyDictionary.AssertDictionaryValue("KPO", 500m);
				uomQtyDictionary.AssertDictionaryValue("KPP", 500m);
				uomQtyDictionary.AssertDictionaryValue("KSD", 500m);
				uomQtyDictionary.AssertDictionaryValue("KSH", 500m);
				uomQtyDictionary.AssertDictionaryValue("KUR", 500m);
				uomQtyDictionary.AssertDictionaryValue("LTR", 12m);
				uomQtyDictionary.AssertDictionaryValue("MIL", 0.01m);
				uomQtyDictionary.AssertDictionaryValue("MPR", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("MTK", 30m);
				uomQtyDictionary.AssertDictionaryValue("MTQ", 0.012m);
				uomQtyDictionary.AssertDictionaryValue("MTR", 2000m);
				uomQtyDictionary.AssertDictionaryValue("MWH", 40m);
				uomQtyDictionary.AssertDictionaryValue("NAR", 10m);
				uomQtyDictionary.AssertDictionaryValue("NCL", 10m);
				uomQtyDictionary.AssertDictionaryValue("NPR", 10m);
				uomQtyDictionary.AssertDictionaryValue("TJO", 0.1111111111111111111111111111m);
				uomQtyDictionary.AssertDictionaryValue("TNE", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("TNE3", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("KGMN", 500m);
				uomQtyDictionary.AssertDictionaryValue("TNEN", 0.5m);
				uomQtyDictionary.AssertDictionaryValue("FLAT", 1m);

				uomQtyDictionary.AssertDictionaryValue(Core.Constants.Weight.Kilograms, 500m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, 500m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Gram, 500000m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram, 5m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne, 0.5m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Litre, 12m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Hectolitre, 0.12m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Kilolitre, 0.012m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.LitrePureAlcohol, 50m);
				uomQtyDictionary.AssertDictionaryValue(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.PercentageVolumeHectolitre, 50m);
			});
		}

		public void TestIsConvertableFrom()
		{
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.WeightConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.VolumeConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.AlcoholConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.NumberConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.LengthConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.SurfaceConversionDictionary);
			AssertIsConvertableFrom(FRConvertibleUnitsOfMeasure.EnergyConversionDictionary);
		}

		void AssertIsConvertableFrom(ImmutableDictionary<string, decimal> conversionDictionary)
		{
			foreach (var dictonary in conversionDictionary)
			{
				var uq = dictonary.Key;
				var list = new ZString[] { uq };
				AssertEquals(uq + " is convertable", true, RateCalcUnitOfMeasureAggregator.IsConvertableFrom(uq, list, Core.Constants.CountryCodes.France, Factory));
			}
		}
	}

	static class DictionaryTestExtension
	{
		public static void AssertDictionaryValue(this IDictionary<string, decimal> dictionary, string key, decimal expectedValue)
		{
			if (dictionary.TryGetValue(key, out decimal actualValue))
			{
				Assertion.AssertEquals($"[{key}]", expectedValue, actualValue);
			}
			else
			{
				Assertion.Fail($"Key [{key}] not present in the dictionary");
			}
		}
	}
}
