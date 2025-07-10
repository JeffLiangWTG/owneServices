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
	class GetMetricTargetRankingFunctionExtractor : BaseFunctionExtractor
	{
		internal GetMetricTargetRankingFunctionExtractor(string formatParameters)
		{
			if (!String.IsNullOrEmpty(formatParameters))
			{
				var parameters = ParamsRegex.Match(formatParameters);

				MetricCode = !String.IsNullOrEmpty(parameters.Groups["MetricCode"].Value) ? parameters.Groups["MetricCode"].Value.Trim().Trim('"') : string.Empty;
			}
		}

		public static GetMetricTargetRankingFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public static GetMetricTargetRankingFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString(lowerCasedFunctionName, ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new GetMetricTargetRankingFunctionExtractor(formatParameters);
			}
			return null;
		}

		internal static string lowerCasedFunctionName = (NoResString)"getmetrictargetranking";

		internal static Regex ParamsRegex
		{
			get { return paramsRegex ?? (paramsRegex = new Regex(@"^\s*(?<MetricCode>\""[^\""]+\""|[^,]+){1}\s*$")); }
		}
		[ThreadStatic]
		static Regex paramsRegex;

		public readonly ZString MetricCode;

		protected virtual string MethodName => nameof(IDocDataMetricRanking.GetMetricTargetRanking);

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			if (typeof(IDocDataMetricRanking).IsAssignableFrom(typeBeingReflected))
			{
				return new MethodInfoChainLink(typeBeingReflected.GetMethod(MethodName, new Type[] { typeof(ZString) }), new object[] { MetricCode })
				{
					TypeToReflect = typeof(ZString)
				};
			}
			else
			{
				return null;
			}
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
