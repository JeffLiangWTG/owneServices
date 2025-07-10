using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetMetricRankingFunctionExtractor : BaseFunctionExtractor
	{
		internal GetMetricRankingFunctionExtractor(string formatParameters)
		{
			if (!String.IsNullOrEmpty(formatParameters))
			{
				var parameters = ParamsRegex.Match(formatParameters);

				MetricCode = !String.IsNullOrEmpty(parameters.Groups["MetricCode"].Value) ? parameters.Groups["MetricCode"].Value.Trim().Trim('"') : string.Empty;
				Value = !String.IsNullOrEmpty(parameters.Groups["Value"].Value) ? ZDecimal.ParseSafe(parameters.Groups["Value"].Value.Trim(), 0) : ZDecimal.Zero;
				bool.TryParse(parameters.Groups["IsPercentage"].Value, out IsPercentage);
			}
		}

		public static GetMetricRankingFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public static GetMetricRankingFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString((NoResString)"getmetricranking", ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new GetMetricRankingFunctionExtractor(formatParameters);
			}
			return null;
		}

		internal static Regex ParamsRegex
		{
			get { return paramsRegex ?? (paramsRegex = new Regex(@"^\s*(?<MetricCode>\""[^\""]+\""|[^,]+){1}\s*,?\s*(?<Value>[^,]+)?\s*(?:|,?\s*(?<IsPercentage>[^,]+)?\s*)$")); }
		}
		[ThreadStatic]
		static Regex paramsRegex;

		public readonly ZString MetricCode;
		public readonly ZDecimal Value;
		public readonly bool IsPercentage;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;

			if (typeof(IDocDataMetricRanking).IsAssignableFrom(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeBeingReflected.GetMethod(nameof(IDocDataMetricRanking.GetMetricRanking), new Type[] { typeof(ZString), typeof(ZDecimal), typeof(bool) }), new object[] { MetricCode, Value, IsPercentage })
				{
					TypeToReflect = typeof(ZString)
				};
			}

			return result;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			if (chainLinks.Count == 0)
			{
				if (IsIBODocDataProviderCollection(typeToReflect) || IsIBusinessObjectCollection(typeToReflect))
				{
					PropertyInfo itemPropertyInfo = typeToReflect.GetProperty("Item", new Type[] { typeof(int) });
					chainLinks.Add(new MethodInfoChainLink(itemPropertyInfo.GetGetMethod()));
					typeToReflect = itemPropertyInfo.PropertyType;
				}
			}

			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);
			}

			return typeof(ZString);
		}

		static bool IsIBusinessObjectCollection(Type type)
		{
			return typeof(IBusinessObjectCollection).IsAssignableFrom(type);
		}

		static bool IsIBODocDataProviderCollection(Type type)
		{
			return typeof(IBODocDataProviderCollection).IsAssignableFrom(type);
		}
	}
}
