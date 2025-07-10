using System;
using GlowIndexQueryService.Common;

namespace GlowIndexQueryService.Tests.Common
{
	public class MockGlowWebRequestThrows : IGlowWebRequest
	{
		public MockGlowWebRequestThrows(Exception toThrow)
			=> exToThrow = toThrow;

		readonly Exception exToThrow;

		public string Get(string relativeAddress)
			=> throw exToThrow;

		public string Post(string relativeAddress, string content)
			=> throw exToThrow;

		public void SetAdditionalHeader(string headerName, string headerValue)
		{
			throw new NotImplementedException();
		}
	}
}
