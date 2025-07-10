using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class DocDataFunctionExtractor : BaseFunctionExtractor
	{
		public static DocDataFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public static DocDataFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string getDocDataValueParameters = ExtractMethodAndReturnParametersAsString((NoResString)"docdatavalue", ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (getDocDataValueParameters != null)
			{
				return new DocDataFunctionExtractor(getDocDataValueParameters);
			}
			return null;
		}

		public string PropertyLabel
		{
			get { return DocDataIdentifier; }
		}

		internal DocDataFunctionExtractor(string getDocDataValueParameters)
		{
			DocDataIdentifier = ExtractFirstParameter(ref getDocDataValueParameters);

			var regex = new Regex(@"^(?:[\s]*)""(?<fallbackvalue>.+)""(?:[\s]*)$", RegexOptions.Compiled);
			getDocDataValueParameters = getDocDataValueParameters.UnEscapeAngleBrackets();
			if (regex.IsMatch(getDocDataValueParameters))
			{
				var match = regex.Match(getDocDataValueParameters);
				FormatStringForFallbackValue = match.Groups["fallbackvalue"].Value;
			}
			else
			{
				FormatStringForFallbackValue = getDocDataValueParameters.Trim('"');
			}
		}
		public readonly ZString DocDataIdentifier;
		public readonly ZString FormatStringForFallbackValue;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;

			if (BODocDataProvider.IsBODocDataProvider(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(BODocDataProvider).GetMethod("GetDocDataValue", new Type[] { typeof(BusinessObject), typeof(ZString), typeof(ZString) }), new object[] { DocDataIdentifier, FormatStringForFallbackValue })
				{
					TypeToReflect = typeof(ZString)
				};
			}

			return result;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			if (typeToReflect is IBODocDataProviderCollection || typeToReflect is IBusinessObjectCollection)
			{
				var propertyInfo = typeToReflect.GetProperty("Item", new Type[] { typeof(int) });

				chainLinks.Add(new MethodInfoChainLink(propertyInfo.GetGetMethod()));
				typeToReflect = propertyInfo.PropertyType;
			}

			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);
			}

			return typeof(ZString);
		}
	}
}
