using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	internal static class CIQDetailsExtractStrategy
	{
		public static ZString ExtractIngredient(IEnumerable<AdditionalElementValue> additionalElementValues)
		{
			var pair = additionalElementValues
				.Where(kvp => kvp.Description.Contains((NoResString)"成分含量"))
				.OrderBy(kvp => kvp.Description.Length)
				.FirstOrDefault();

			return pair?.Value ?? ZString.Empty;
		}

		public static ZString ExtractSpecification(IEnumerable<AdditionalElementValue> additionalElementValues)
		{
			var result = ZString.Empty;
			var pair = additionalElementValues
				.FirstOrDefault(kvp => kvp.Code == SpecModelElementStrategy.AdditionalElementCode);

			if (pair != null)
			{
				var pattern = (NoResString)@"^规格：(\S+)、型号：(\S+)$";
				var value = pair.Value;
				var match = Regex.Match(value, pattern);
				result = match.Success ? match.Groups[1].Value : string.Empty;
			}
			else
			{
				pair = additionalElementValues
				.Where(kvp => kvp.Description.Contains((NoResString)"规格") && !kvp.Description.Contains((NoResString)"包装规格"))
				.OrderBy(kvp => kvp.Description.Length)
				.FirstOrDefault();

				result = pair?.Value ?? ZString.Empty;
			}

			return result;
		}

		public static ZString ExtractBrand(IEnumerable<AdditionalElementValue> additionalElementValues)
		{
			var pair = additionalElementValues
				.Where(kvp => kvp.Description.Contains((NoResString)"品牌") && kvp.Code != BrandTypeElementStrategy.AdditionalElementCode)
				.OrderBy(kvp => kvp.Description.Length)
				.FirstOrDefault();

			return pair?.Value ?? ZString.Empty;
		}

		public static ZString ExtractModel(IEnumerable<AdditionalElementValue> additionalElementValues)
		{
			var result = ZString.Empty;
			var pair = additionalElementValues
				.FirstOrDefault(kvp => kvp.Code == SpecModelElementStrategy.AdditionalElementCode);

			if (pair != null)
			{
				var pattern = (NoResString)@"^规格：(\S+)、型号：(\S+)$";
				var value = pair.Value;
				var match = Regex.Match(value, pattern);
				result = match.Success ? match.Groups[2].Value : string.Empty;
			}
			else
			{
				pair = additionalElementValues
					.Where(kvp => kvp.Description.Contains((NoResString)"型号"))
					.OrderBy(kvp => kvp.Description.Length)
					.FirstOrDefault();

				result = pair?.Value ?? ZString.Empty;
			}

			return result;
		}

		public static ZDateTime[] ExtractManufactureDates(IEnumerable<AdditionalElementValue> additionalElementValues)
		{
			var list = new List<ZDateTime>();

			var manufactureDateString = additionalElementValues.FirstOrDefault(kvp => kvp.Code == ManufactureDateElementStrategy.AdditionalElementCode)?.Value ?? ZString.Empty;
			if (ManufactureDateElementStrategy.IsValidManufactureDateString(manufactureDateString))
			{
				var dateStrings = manufactureDateString.Split(';');
				foreach (var dateString in dateStrings)
				{
					list.Add(DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture));
				}
			}

			return list.ToArray();
		}
	}
}
