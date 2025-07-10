using System.Globalization;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetMetricTargetRangeMaxFunctionExtractor : GetMetricTargetRankingFunctionExtractor
	{
		internal GetMetricTargetRangeMaxFunctionExtractor(string formatParameters)
			: base(formatParameters)
		{
		}

		public new static GetMetricTargetRangeMaxFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public new static GetMetricTargetRangeMaxFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString(lowerCasedFunctionName, ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new GetMetricTargetRangeMaxFunctionExtractor(formatParameters);
			}
			return null;
		}

		protected override string MethodName => nameof(IDocDataMetricRanking.GetMetricTargetRangeMax);

		internal static new string lowerCasedFunctionName = (NoResString)"getmetrictargetrangemax";
	}
}
