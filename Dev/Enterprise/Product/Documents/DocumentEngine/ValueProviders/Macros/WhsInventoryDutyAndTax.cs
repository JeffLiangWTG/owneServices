using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class WhsInventoryDutyAndTax : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<WhsInventoryDutyAndTax('{typeOfDataToReturn}', '{countryCode}', '{inventoryPK}', '{tariff}', '{countryOfOrigin}', '{customsValue}', '{customsQty1}', '{customsUQ1}', '{customsQty2}', '{customsUQ2}', '{customsQty3}', '{customsUQ3}', '{ratio}', '{arrivalDate}')>",
				ResString.GetMultilingualString("{640D7BE3-1AB3-42C1-B959-07FC5D2A4BDB}", @"Returns the Customs Duty or Tax data for a particular Warehouse Inventory record.
Please ensure that the {0} is {1}.", "arrivalDate", "ShortDateFormat"),
				new List<(string example, object expectedResult)>
				{
					((NoResString)"<WhsInventoryDutyAndTax('DTY', '<Inventory.WarehouseCountry>', '<Inventory.PK>', '<Inventory.Tariff>', '<Inventory.CountryOfOrigin>', '<Inventory.CustomsValue>', '<Inventory.CustomsQty1>', '<Inventory.CustomsUQ1>', '<Inventory.CustomsQty2>', '<Inventory.CustomsUQ2>', '<Inventory.CustomsQty3>', '<Inventory.CustomsUQ3>', '<Inventory.Ratio>', '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>')>", new ZDecimal(7940.10)),
					((NoResString)"<WhsInventoryDutyAndTax('VAT', '<Inventory.WarehouseCountry>', '<Inventory.PK>', '<Inventory.Tariff>', '<Inventory.CountryOfOrigin>', '<Inventory.CustomsValue>', '<Inventory.CustomsQty1>', '<Inventory.CustomsUQ1>', '<Inventory.CustomsQty2>', '<Inventory.CustomsUQ2>', '<Inventory.CustomsQty3>', '<Inventory.CustomsUQ3>', '<Inventory.Ratio>', '<DateTimeAsString('<Inventory.ArrivalDate>', 'ShortDateFormat')>')>", new ZDecimal(1209.52)),
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matchedData = Regex.Match(macro).Groups;
			var inventoryPK = GetData<ZGuid>(report, matchedData, Constants.InventoryPK);
			if (inventoryPK.IsValid)
			{
				var ratio = GetData<ZDecimal>(report, matchedData, Constants.Ratio);
				if (ratio > ZDecimal.Zero)
				{
					string countryCode = GetData<ZString>(report, matchedData, Constants.CountryCode);
					var whsInventoryDutyAndTaxCalculatorProvider = ObjectFactory.Get<Enterprise.Integration.Customs.IWhsInventoryDutyAndTaxCalculatorProvider>();
					var whsInventoryDutyAndTaxCalculator = TryParseWithReportOnFail(
						report: report,
						defaultValue: null,
						errorMessage: GetErrorMessage(countryCode, "DutyAndTaxCalculatorProvider"),
						parseFunc: () => whsInventoryDutyAndTaxCalculatorProvider.GetProviderFor(report.Factory, countryCode));

					if (whsInventoryDutyAndTaxCalculator != null)
					{
						return TryParseWithReportOnFail(
							report: report,
							defaultValue: ZDecimal.Zero,
							errorMessage: GetErrorMessage(countryCode, "DutyAndTaxCalculator"),
							parseFunc: () => Calculate(whsInventoryDutyAndTaxCalculator, report, matchedData, inventoryPK, ratio));
					}
				}
			}
			return ZDecimal.Zero;
		}

		object Calculate(IWhsInventoryDutyAndTaxCalculator calculator, Report report, GroupCollection matchedData, ZGuid inventoryPK, ZDecimal ratio)
		{
			var alreadyCalculatedData = calculator.Factory.GetCachedValue<Dictionary<ZString, Dictionary<ZString, IZType>>>("WhsInventoryDutyAndTaxAlreadyCalculatedData", () => new Dictionary<ZString, Dictionary<ZString, IZType>>());
			var key = inventoryPK.ToStringKey() + ratio;
			if (!alreadyCalculatedData.TryGetValue(key, out var data))
			{
				data = new Dictionary<ZString, IZType>();
				alreadyCalculatedData.Add(key, data);
				calculator.Calculate(data, ratio, GetData<ZDateTime>(report, matchedData, Constants.ArrivalDate), report.StartTime, GetData<ZString>(report, matchedData, Constants.Tariff), GetData<ZString>(report, matchedData, Constants.CountryOfOrigin), GetData<ZDecimal>(report, matchedData, Constants.CustomsValue), GetData<ZDecimal>(report, matchedData, Constants.CustomsQty1), GetData<ZString>(report, matchedData, Constants.CustomsUQ1), GetData<ZDecimal>(report, matchedData, Constants.CustomsQty2), GetData<ZString>(report, matchedData, Constants.CustomsUQ2), GetData<ZDecimal>(report, matchedData, Constants.CustomsQty3), GetData<ZString>(report, matchedData, Constants.CustomsUQ3));
			}

			var typesOfDataToReturn = GetData<ZString>(report, matchedData, Constants.TypeOfDataToReturn);

			var decimalResult = 0m;
			object result = null;

			foreach (var typeToReturn in typesOfDataToReturn.Split('|'))
			{
				if (!data.TryGetValue(typeToReturn, out var innerResult))
				{
					throw new NotSupportedException(string.Format(Culture.Invariant, "'{0}' is not a valid.", typeToReturn));
				}

				if (innerResult is ZDecimal)
				{
					decimalResult += (ZDecimal)innerResult;
					result = decimalResult;
				}
				else
				{
					if (decimalResult == 0m)
					{
						result = innerResult;
					}
				}
			}

			return result;
		}

		T GetData<T>(Report report, GroupCollection matchedData, string type)
			where T : IZType
		{
			var data = matchedData[type].ToString().Trim();
			return TryParseWithReportOnFail<T>(
				report: report,
				defaultValue: default(T),
				errorMessage: GetErrorMessage(data, type),
				parseFunc: () => (T)ZDataType.ObjectToZType(typeof(T), data));
		}

		string GetErrorMessage(object value, string type)
		{
			return string.Format(Culture.Invariant, (NoResString)"{2}: '{0}', Error: {1}", value, "{0}", type);
		}

		public override Passes PassToStartReplacingOn => Passes.SecondPass;
		public override Regex Regex
		{
			get { return regex; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		static class Constants
		{
			public const string TypeOfDataToReturn = "TypeOfDataToReturn";
			public const string CountryCode = "CountryCode";
			public const string InventoryPK = "InventoryPK";
			public const string Tariff = "Tariff";
			public const string CountryOfOrigin = "CountryOfOrigin";
			public const string CustomsValue = "CustomsValue";
			public const string CustomsQty1 = "CustomsQty1";
			public const string CustomsUQ1 = "CustomsUQ1";
			public const string CustomsQty2 = "CustomsQty2";
			public const string CustomsUQ2 = "CustomsUQ2";
			public const string CustomsQty3 = "CustomsQty3";
			public const string CustomsUQ3 = "CustomsUQ3";
			public const string Ratio = "Ratio";
			public const string ArrivalDate = "ArrivalDate";
		}

		static readonly Regex regex = new Regex(@"^<[\s]*WhsInventoryDutyAndTax[\s]*\([\s]*'(?<TypeOfDataToReturn>.*)'[\s]*,[\s]*'(?<CountryCode>.*)'[\s]*,[\s]*'(?<InventoryPK>.*)'[\s]*,[\s]*'(?<Tariff>.*)'[\s]*,[\s]*'(?<CountryOfOrigin>.*)'[\s]*,[\s]*'(?<CustomsValue>.*)'[\s]*,[\s]*'(?<CustomsQty1>.*)'[\s]*,[\s]*'(?<CustomsUQ1>.*)'[\s]*,[\s]*'(?<CustomsQty2>.*)'[\s]*,[\s]*'(?<CustomsUQ2>.*)'[\s]*,[\s]*'(?<CustomsQty3>.*)'[\s]*,[\s]*'(?<CustomsUQ3>.*)'[\s]*,[\s]*'(?<Ratio>.*)'[\s]*,[\s]*'(?<ArrivalDate>.*)'[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
