using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public class ProblemDetails
	{
		public string Type { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public int Status { get; set; }

		public string Detail { get; set; } = string.Empty;
		public string Instance { get; set; } = string.Empty;

		[JsonExtensionData]
		public IDictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Media type string")]
		public const string MediaType = "application/problem+json";
	}
}
