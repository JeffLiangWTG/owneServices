#if NETFRAMEWORK
using System.IO.Pipes;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CargoWise.Data.SqlProxy.Interface;

public class NamedPipeMessageHandler : HttpMessageHandler
{
	public NamedPipeMessageHandler(string pipeName)
	{
		if (string.IsNullOrWhiteSpace(pipeName))
		{
			throw new ArgumentNullException(nameof(pipeName));
		}

		this.pipeName = pipeName;
		pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		try
		{
			await pipeClient.ConnectAsync(cancellationToken).ConfigureAwait(false);

			var writer = new StreamWriter(pipeClient, Encoding.UTF8) { AutoFlush = true };
			var reader = new StreamReader(pipeClient, Encoding.UTF8);

			var requestString = await SerializeRequestAsync(request).ConfigureAwait(false);
			await writer.WriteLineAsync(requestString).ConfigureAwait(false);

			var responseString = await reader.ReadToEndAsync().ConfigureAwait(false);
			var response = await DeserializeResponseAsync(responseString).ConfigureAwait(false);

			response.Content = new StreamContent(pipeClient);
			return response;
		}
		catch (Exception ex) when (ex is IOException or TimeoutException)
		{
			throw new HttpRequestException($"Failed to communicate with the named pipe: {pipeName}.", ex);
		}
	}

	static async Task<string> SerializeRequestAsync(HttpRequestMessage request)
	{
		var stringBuilder = new StringBuilder();
		_ = stringBuilder.AppendLine($"{request.Method} {request.RequestUri} HTTP/{request.Version}");
		_ = stringBuilder.AppendLine($"Host: localhost");
		foreach (var header in request.Headers)
		{
			_ = stringBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
		}

		if (request.Content != null)
		{
			foreach (var header in request.Content.Headers)
			{
				stringBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
			}

			_ = stringBuilder.AppendLine();
			_ = stringBuilder.AppendLine(await request.Content.ReadAsStringAsync().ConfigureAwait(false));
		}
		else
		{
			_ = stringBuilder.AppendLine();
		}

		return stringBuilder.ToString();
	}

	static Task<HttpResponseMessage> DeserializeResponseAsync(string responseString)
	{
		try
		{
			using var reader = new StringReader(responseString);
			var statusLine = reader.ReadLine()?.Trim() ?? throw new InvalidDataException("Empty response received.");

			var statusLineParts = statusLine.Split([' '], 3);
			if (statusLineParts.Length < 3)
			{
				throw new InvalidDataException("Invalid status line.");
			}

			if (!int.TryParse(statusLineParts[1], out var statusCodeInt))
			{
				throw new InvalidDataException("Invalid HTTP status code.");
			}

			var response = new HttpResponseMessage((HttpStatusCode)statusCodeInt);
			if (!Version.TryParse(statusLineParts[0].Replace("HTTP/", ""), out var httpVersion))
			{
				httpVersion = HttpVersion.Version11;
			}
			response.Version = httpVersion;

			string? line;
			while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
			{
				var headerParts = line.Split([": "], 2, StringSplitOptions.None);
				if (headerParts.Length == 2)
				{
					if (!response.Headers.TryAddWithoutValidation(headerParts[0], headerParts[1]))
					{
						response.Content ??= new StringContent(string.Empty);
						response.Content.Headers.TryAddWithoutValidation(headerParts[0], headerParts[1]);
					}
				}
			}

			var content = reader.ReadToEnd();
			response.Content = new StringContent(content, Encoding.UTF8);

			return Task.FromResult(response);
		}
		catch (Exception ex)
		{
			throw new InvalidDataException("Failed to deserialize response.", ex);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			pipeClient.Dispose();
		}

		base.Dispose(disposing);
	}

	readonly NamedPipeClientStream pipeClient;
	readonly string pipeName;
}

#endif
