using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public static class ValueProviderHelper
	{
		//Gets the business object from a data provider with the expression of property chain. e.g. Shipment.Consignee
		public static object GetBusinessObjectFromDataProvider(string macro, Report report)
		{
			object destBizO = report.BODocDataProvider;

			if (!string.IsNullOrEmpty(macro))
			{
				string[] propertyChain = macro.Split(new char[] { '.' });
				destBizO = GetPropertyFromBizO(destBizO, propertyChain);

				if (destBizO == null)//try getting property from wrapped business object if provider is a doc wrapper
				{
					destBizO = report.BODocDataProvider;
					while (destBizO is IBODocDataProvider)
					{
						destBizO = ((IBODocDataProvider)destBizO).BusinessObjectToLogAgainst;
					}

					destBizO = GetPropertyFromBizO(destBizO, propertyChain);
				}
			}

			if (destBizO != null)
			{
				while (destBizO is IBODocDataProvider)
				{
					destBizO = ((IBODocDataProvider)destBizO).BusinessObjectToLogAgainst;
				}
			}

			return destBizO;
		}

		static object GetPropertyFromBizO(object destBizO, string[] propertyChain)
		{
			foreach (string item in propertyChain)
			{
				if (destBizO != null)
				{
					var propertyInfo = destBizO.GetType().GetProperty(item, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					destBizO = propertyInfo == null ? null : propertyInfo.GetValue(destBizO, null);
				}
				else
				{
					break;
				}
			}
			return destBizO;
		}

		public static object ParseGuid(Report report, string matchedValue)
		{
			object value;
			ZGuid pk;
			if (ZGuid.TryParse(matchedValue, out pk))
			{
				value = pk;
			}
			else
			{
				string macro = matchedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && matchedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase) ? matchedValue : "<" + matchedValue + ">";
				value = report.MacroTranslator.GetValue(macro, Passes.FirstPass);
			}
			object parsedValue = null;

			if (value == DBNull.Value)
			{
				parsedValue = "";
			}
			else if (value is ZGuid || value is Guid)
			{
				parsedValue = ToGuid(value);
			}
			else
			{
				try
				{
					// Allow strings like "12345678-9abc-def0-1234-56789abcdef0"
					parsedValue = new Guid(value.ToString());
				}
				catch (FormatException)
				{
					parsedValue = "";
				}
			}

			return parsedValue;
		}

		static Guid ToGuid(object value)
		{
			Guid result = Guid.Empty;

			if (value is Guid)
			{
				result = (Guid)value;
			}
			else if (value is ZGuid)
			{
				ZGuid zGuid = (ZGuid)value;
				if (zGuid.IsValid)
				{
					result = zGuid.ToGuid();
				}
			}

			return result;
		}
	}
}
