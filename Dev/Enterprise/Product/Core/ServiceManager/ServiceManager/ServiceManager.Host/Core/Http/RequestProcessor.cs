using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class RequestProcessor : IRequestProcessor
	{
		public const string ErrorPrefix = "Error:";

		readonly IHostLogger logger;
		readonly IActionQueue actionQueue;

		public RequestProcessor(ResponseStringBuilder responseStringBuilder, IHostLogger logger, IActionQueue actionQueue)
		{
			this.logger = logger;
			ResponseStringBuilder = responseStringBuilder;
			this.actionQueue = actionQueue;
		}

		public ResponseStringBuilder ResponseStringBuilder { get; }

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We expect to catch and log all exceptions here")]
		public void ProcessRequest(HttpListenerContext context, CancellationToken cancellationToken)
		{
			using (var response = context.Response)
			{
				try
				{
					var request = context.Request;

					logger.Log(LogLevel.Debug, string.Format(CultureInfo.InvariantCulture, "Received \"{0}\" request from {1}", request.RawUrl, request.RemoteEndPoint));

					var acceptEncoding = request.Headers["Accept-Encoding"];
					response.ContentType = "text/plain";

					if (cancellationToken.IsCancellationRequested)
					{
						response.StatusCode = 404;
						return;
					}

					var responseString = ResponseStringBuilder.GetResponseString(new WebRequestInfo(request));
					if (responseString.StartsWith("The page cannot be found.", StringComparison.Ordinal) ||
							responseString.StartsWith(ErrorPrefix, StringComparison.Ordinal))
					{
						response.StatusCode = 404;
					}

					var buffer = Encoding.UTF8.GetBytes(responseString);

					using (var output = response.OutputStream)
					{
						if (acceptEncoding != null && acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
						{
							response.AddHeader("Content-Encoding", "gzip");

							using (var gZipStream = new GZipStream(output, CompressionMode.Compress))
							{
								gZipStream.Write(buffer, 0, buffer.Length);
							}
						}
						else if (acceptEncoding != null && acceptEncoding.Contains("deflate", StringComparison.OrdinalIgnoreCase))
						{
							response.AddHeader("Content-Encoding", "deflate");

							using (var deflateStream = new DeflateStream(output, CompressionMode.Compress))
							{
								deflateStream.Write(buffer, 0, buffer.Length);
							}
						}
						else
						{
							output.Write(buffer, 0, buffer.Length);
						}
					}
				}
				catch (SqlException)
				{
					response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
					response.StatusDescription = "Service Unavailable";
				}
				catch (Exception ex) when (ex is DatabaseUpgradeException || ex.InnerException is DatabaseUpgradeException)
				{
					response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
					response.StatusDescription = "Service Unavailable";
					actionQueue.Enqueue(() => ExceptionDispatchInfo.Capture(ex).Throw());
				}
				catch (HttpListenerException ex) when (ex.ErrorCode == 64 || ex.ErrorCode == 1229) // ERROR_NETNAME_DELETED, ERROR_NONEXISTENT_NETWORK_CONNECTION
				{
					logger.Log(LogLevel.Warning, "RequestProcessor failed to send the reply to a client due to reason: " + ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					response.StatusCode = (int)HttpStatusCode.InternalServerError;
					response.StatusDescription = "Internal Server Error";
					throw;
				}
			}
		}
	}
}
