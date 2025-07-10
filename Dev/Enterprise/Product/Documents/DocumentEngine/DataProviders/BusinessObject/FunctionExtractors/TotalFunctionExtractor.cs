using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class TotalFunctionExtractor : BaseFunctionExtractor
	{
		public static TotalFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public static TotalFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString((NoResString)"total", ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new TotalFunctionExtractor(formatParameters);
			}
			return null;
		}

		public string PropertyLabel
		{
			get { return (NoResString)"Total" + FieldToTotal; }
		}

		internal TotalFunctionExtractor(string totalParameters)
		{
			FieldToTotal = ExtractFirstParameter(ref totalParameters);
			DecimalPlaces = ExtractFirstParameter(ref totalParameters);
			Filter = totalParameters;
		}

		public readonly ZString FieldToTotal;
		public readonly ZString DecimalPlaces;
		public readonly ZString Filter;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;

			if (typeof(IBODocDataProviderCollection).IsAssignableFrom(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Total", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString) }), new object[] { FieldToTotal, DecimalPlaces, Filter });
			}
			else if (typeof(IBusinessObjectCollection).IsAssignableFrom(typeBeingReflected))
			{
				result = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Total", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString) }), new object[] { FieldToTotal, DecimalPlaces, Filter });
			}
			if(result != null)
			{
				result.TypeToReflect = typeof(ZString);
			}

			return result;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);
				return chainLink.TypeToReflect;
			}

			return null;
		}
	}
}
