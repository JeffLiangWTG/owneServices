using System.Linq;

namespace GlowIndexQueryService.Common
{
	class SearchFieldsResponse
	{
		public SearchFieldsResponse(string entityType, string oDataContext, SearchFieldDto[] value)
		{
			EntityType = entityType;
			ODataContext = oDataContext;
			Value = value;
		}
		public string EntityType { get; }
		public string ODataContext { get; }
		public SearchFieldDto[] Value { get; }

		public bool IsReponseBlank => Value == null || Value.Length == 0 || Value.Any(f => f?.FieldName == null);
	}
}
