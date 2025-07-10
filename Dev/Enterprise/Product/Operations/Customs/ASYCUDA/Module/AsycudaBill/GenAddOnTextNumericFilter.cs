using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class GenAddOnTextNumericFilter : ModuleTextFilter
	{
		readonly int noOfDecimalPlaces;
		internal GenAddOnTextNumericFilter(ZString description, FilterStripBusinessObject filterBusinessObject, string column, string tablePrefix, Type businessObjectType, SchemaColumn schemaKeyOverride, int noOfDecimals = 0)
			: base(description, (comparisonOperator, value) => GetQuery(comparisonOperator, value, column, tablePrefix, businessObjectType, noOfDecimals, schemaKeyOverride))
		{
			this.filterBusinessObject = filterBusinessObject;
			noOfDecimalPlaces = noOfDecimals;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		static ZQuery GetQuery(SQLComparisonOperator comparisonOperator, ZString value, string column, string tablePrefix, Type businessObjectType, int noOfDecimals, SchemaColumn schemaKeyOverride)
		{
			var query = new ZDBOnlyQuery(businessObjectType);
			var helper = new GenAddOnColumnQueryHelper(businessObjectType);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				var referenceDataSubQuery = helper.GetQueryHandlingBlanks(column, comparisonOperator, value);
				query.AddToFilter(referenceDataSubQuery);
			}
			else
			{
				var cultureValue = ConvertToCultureFormat(value);

				if (ZDecimal.TryParse(cultureValue, out var decimalValue))
				{
					var filterValues = PrepareFilterValues(noOfDecimals, cultureValue, decimalValue);
					var referenceDataSubQuery = schemaKeyOverride != null
						? helper.GetQueryOnGenAddOnColumn(column, comparisonOperator, filterValues.Distinct().ToArray(), false, schemaKeyOverride, tablePrefix)
						: helper.GetQueryOnGenAddOnColumn(column, comparisonOperator, filterValues.Distinct().ToArray(), false, tablePrefix);
					query.AddToFilter(referenceDataSubQuery);
				}
			}
			return query;
		}

		static IEnumerable<ZString> PrepareFilterValues(int noOfDecimals, string auValue, ZDecimal decimalValue)
		{
			var filterValues = new ZString[]
			{
				auValue
			};

			char decimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.CurrentCulture);

			if (!auValue.Contains(decimalSeparator))
			{
				for (int i = 1; i <= noOfDecimals; i++)
				{
					var textFilterWithDecimalPlaces = decimalValue.ToString(i);
					filterValues = filterValues.Concat(new ZString[] { textFilterWithDecimalPlaces }).ToArray();
				}
			}
			else
			{
				var decimalPart = auValue.Split(decimalSeparator)[1];

				for (int i = decimalPart.Length; i < noOfDecimals; i++)
				{
					filterValues = filterValues.Concat(new ZString[] { auValue + "0".PadRight(i) }).ToArray();
				}

				if (auValue.EndsWith("0", StringComparison.OrdinalIgnoreCase))
				{
					for (int i = decimalPart.Length - 1; i >= 0; i--)
					{
						if (decimalPart[i] == '0')
						{
							filterValues = filterValues.Concat(new ZString[] { decimalValue.ToString(string.Format(CultureInfo.InvariantCulture, "N{0}", i), CultureInfo.InvariantCulture) }).ToArray();
						}
					}
				}
			}

			return filterValues;
		}

		static string ConvertToCultureFormat(string value)
		{
			if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture, out decimal decimalValue))
			{
				return decimalValue.ToString(ObjectCache.CultureProvider.Culture);
			}

			return value;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new GenAddOnTextNumericFilterValidation(this, filterBusinessObject, noOfDecimalPlaces, SqlComparisonOperator);
		}
	}
}
