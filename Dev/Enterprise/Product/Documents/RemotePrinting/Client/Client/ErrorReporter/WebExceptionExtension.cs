using System.Net;
using System.Text;
using WTG.ErrorReporting.Extensibility;

namespace Enterprise.RemotePrinting.Client
{
	public sealed class WebExceptionExtension : IAdditionalDetailContributor
	{
		void IAdditionalDetailContributor.Analyze(in ExceptionContext context)
		{
			if (context.Exception is WebException ex && ex.Response != null)
			{
				var webResponse = ex.Response;
				context.AddDetail("ResponseUri", webResponse.ResponseUri?.ToString() ?? string.Empty);

				if (ex.Response is HttpWebResponse httpWebResponse)
				{
					context.AddDetail("ResponseStatusCode", httpWebResponse.StatusCode.ToString());
					context.AddDetail("ResponseStatusDescription", httpWebResponse.StatusDescription);
				}

				if (webResponse.SupportsHeaders && webResponse.Headers != null)
				{
					var sb = new StringBuilder();

					for (int i = 0; i < webResponse.Headers.Count; i++)
					{
						var header = webResponse.Headers.GetKey(i);
						var values = webResponse.Headers.GetValues(header);
						string headerValue = values == null ? string.Empty : string.Join(",", values);

						sb.Append(header).Append(": ").AppendLine(headerValue);
					}

					context.AddDetail("ResponseHeaders", sb.ToString().TrimEnd());
				}
			}
		}
	}
}
