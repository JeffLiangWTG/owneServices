using System.Globalization;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetMetricTargetRangeFunctionExtractor : GetMetricTargetRankingFunctionExtractor
	{
		internal GetMetricTargetRangeFunctionExtractor(string formatParameters)
			: base(formatParameters)
		{
		}

		public new static GetMetricTargetRangeFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public new static GetMetricTargetRangeFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString(lowerCasedFunctionName, ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new GetMetricTargetRangeFunctionExtractor(formatParameters);
			}
			return null;
		}

		protected override string MethodName => nameof(IDocDataMetricRanking.GetMetricTargetRange);

		internal static new string lowerCasedFunctionName = (NoResString)"getmetrictargetrange";
	}
}
