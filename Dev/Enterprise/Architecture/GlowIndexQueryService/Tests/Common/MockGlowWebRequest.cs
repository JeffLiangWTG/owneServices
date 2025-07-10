using System;
using System.Collections.Generic;
using System.Linq;
using GlowIndexQueryService.Common;

namespace GlowIndexQueryService.Tests.Common
{
	public class MockGlowWebRequest : IGlowWebRequest
	{
		public MockGlowWebRequest(params string[] jsonResponses)
			=> JsonResponses = jsonResponses.ToList();

		public string Get(string relativeAddress)
		{
			RequestURLs.Add(relativeAddress);
			return CurrentRequestCount < JsonResponses.Count
					? JsonResponses[CurrentRequestCount++]
					: throw new InvalidOperationException("Attempted to resubmit a request to Glow service but no more requests were expected");
		}

		public string Post(string relativeAddress, string content)
			=> throw new NotImplementedException();

		public void SetAdditionalHeader(string headerName, string headerValue)
		{
			HasAdditionalHeader = true;
			AdditionalHeaders[headerName] = headerValue;
		}

		public List<string> RequestURLs { get; } = [];

		public Dictionary<string, string> AdditionalHeaders = [];

		public bool HasAdditionalHeader { get; set; }

		public int CurrentRequestCount { get; set; }

		public List<string> JsonResponses { get; set; }
	}
}
