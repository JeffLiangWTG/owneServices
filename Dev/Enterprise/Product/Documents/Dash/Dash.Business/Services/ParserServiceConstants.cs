using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dash.Business.Services
{
	public static class ParserServiceConstants
	{
		internal static class QueryParamNames
		{
			public const string DocPk = nameof(DocPk);
		}

		[CodeAlive("Used directly in GenerateRequestBody in RemoteParserServiceClient for data construction.")]
		internal static class MultiPartContentName
		{
			public const string DocType = "docType";
			public const string ParseType = "parseType";
			public const string DocTimestamp = "docTimestamp";
			public const string ResponseEndpointRoot = "responseEndpointRoot";
			public const string Cw1Version = "cw1Version";
			public const string CustomerId = "customerId";
			public const string FileContentType = "fileContentType";
			public const string UtilityData = "utilityData";
			public static readonly string File = (NoResString)"file";
		}
	}
}
