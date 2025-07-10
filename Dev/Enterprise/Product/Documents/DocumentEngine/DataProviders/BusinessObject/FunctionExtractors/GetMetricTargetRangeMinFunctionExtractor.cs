using System.Globalization;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetMetricTargetRangeMinFunctionExtractor : GetMetricTargetRankingFunctionExtractor
	{
		internal GetMetricTargetRangeMinFunctionExtractor(string formatParameters)
			: base(formatParameters)
		{
		}

		public new static GetMetricTargetRangeMinFunctionExtractor ParseAndExtract(string propertyIdentifier)
		{
			string lowerCasedPropertyIdentifier = propertyIdentifier.ToLower(CultureInfo.InvariantCulture);
			return ParseAndExtract(ref lowerCasedPropertyIdentifier, ref propertyIdentifier);
		}

		public new static GetMetricTargetRangeMinFunctionExtractor ParseAndExtract(ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatParameters = ExtractMethodAndReturnParametersAsString(lowerCasedFunctionName, ref lowerCasedPropertyIdentifier, ref unModifiedPropertyIdentifier);
			if (formatParameters != null)
			{
				return new GetMetricTargetRangeMinFunctionExtractor(formatParameters);
			}
			return null;
		}

		protected override string MethodName => nameof(IDocDataMetricRanking.GetMetricTargetRangeMin);

		internal static new string lowerCasedFunctionName = (NoResString)"getmetrictargetrangemin";
	}
}
