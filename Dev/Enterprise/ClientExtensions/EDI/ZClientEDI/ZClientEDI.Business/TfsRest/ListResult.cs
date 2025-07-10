using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.TfsRest
{
	class ListResult<T>
	{
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("value")]
		public List<T> Value { get; set; }
	}
}
