using System.Net.Http;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.ZArchitecture.GUI
{
	public static class HttpResponseMessageExtensions
	{
		public static bool IsProblemDetails(this HttpResponseMessage response)
		{
			var mediaType = response.Content?.Headers?.ContentType?.MediaType;
			return mediaType == ProblemDetails.MediaType;
		}
	}
}
