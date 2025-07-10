using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class FormatFunctionExtractor : BaseFunctionExtractor
	{
		public static FormatFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public static FormatFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString((NoResString)"format", ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new FormatFunctionExtractor(formatParameters);
			}
			return null;
		}

		public string PropertyLabel
		{
			get
			{
				using (var interpreter = new FormatStringInterpreter())
				{
					return interpreter.Interpret(null, FormatString);
				}
			}
		}

		internal FormatFunctionExtractor(string formatParameters)
		{
			if (!string.IsNullOrEmpty(formatParameters))
			{
				var parameters = ParamsRegex.Match(formatParameters);

				FormatString = !string.IsNullOrEmpty(parameters.Groups["FormatString"].Value) ? parameters.Groups["FormatString"].Value.Trim().Trim('"') : string.Empty;
				Delimiter = !string.IsNullOrEmpty(parameters.Groups["Delimiter"].Value) ? parameters.Groups["Delimiter"].Value.Trim() : string.Empty;
				FilterString = !string.IsNullOrEmpty(parameters.Groups["FilterString"].Value) ? parameters.Groups["FilterString"].Value.Trim() : string.Empty;

				if (FilterString == "\"\"")
				{
					FilterString = string.Empty;
				}
				else if (FilterString.Length - FilterString.Replace("\"", string.Empty).Length == 2)
				{
					FilterString = FilterString.Trim('"');
				}

				GroupByParameters = !string.IsNullOrEmpty(parameters.Groups["GroupByParameters"].Value) ? parameters.Groups["GroupByParameters"].Value.Trim().Trim('"') : string.Empty;

				var maxItemsParam = !string.IsNullOrEmpty(parameters.Groups["MaxItems"].Value) ? parameters.Groups["MaxItems"].Value.Trim() : string.Empty;

				if (!ZInt.TryParse(maxItemsParam, out MaxItems))
				{
					MaxItems = 0;
				}
			}
		}

		internal static Regex ParamsRegex
		{
			get { return paramsRegex ?? (paramsRegex = new Regex(@"^\s*(?<FormatString>\""[^\""]+\""|[^,]+){1}\s*,?\s*(?<Delimiter>[^,]+)?\s*,?\s*(?<FilterString>[^,]+)?\s*,?\s*(?<GroupByParameters>\""[^\""]*\"")?\s*,?\s*(?<MaxItems>[\d]+)?\s*$")); }
		}
		[ThreadStatic]
		static Regex paramsRegex;

		public readonly ZString FormatString;
		public readonly ZString Delimiter;
		public readonly ZString FilterString;
		public readonly ZString GroupByParameters;
		public readonly ZInt MaxItems;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;

			if (BODocDataProvider.IsBODocDataProvider(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(FormatStringInterpreter).GetMethod(nameof(FormatStringInterpreter.Format), new Type[] { typeof(BusinessObject), typeof(ZString) }), new object[] { FormatString });
			}
			else if (typeof(IBODocDataProviderCollection).IsAssignableFrom(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod(nameof(IBODocDataProviderCollection.Format), new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { FormatString, Delimiter, FilterString, GroupByParameters, MaxItems });
			}
			else if (typeof(IBusinessObjectCollection).IsAssignableFrom(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod(nameof(BODocDataProviderCollectionHelper.Format), new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { FormatString, Delimiter, FilterString, GroupByParameters, MaxItems });
			}
			if (result != null)
			{
				result.TypeToReflect = typeof(ZString);
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
