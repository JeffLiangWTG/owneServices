using System;
using GlowIndexQueryService.Common;

namespace GlowIndexQueryService.Tests.Common
{
	public class MockGlowWebRequestWithCaption : IGlowWebRequest
	{
		public MockGlowWebRequestWithCaption(string jsonResponse)
		{
			JsonResponse = jsonResponse;
		}

		public string Get(string relativeAddress)
		{
			CurrentRequestCount++;
			lastRequestUrl = relativeAddress;
			if (relativeAddress.Contains(GlowIndexQueryService.Business.Constants.CAPTION_SERVICE))
			{
				if (relativeAddress.EndsWith("ZH-CN", StringComparison.OrdinalIgnoreCase))
				{
					return GlowTestData.LanguageSwitch_CaptionCN_TestData;
				}
				return GlowTestData.LanguageSwitch_CaptionEN_TestData;
			}

			if (relativeAddress.Contains(GlowIndexQueryService.Business.Constants.LOOKUP_SERVICE))
			{
				return GlowTestData.LookupMetaData;
			}

			if (relativeAddress.Contains(GlowIndexQueryService.Business.Constants.LOOKUP_RULE_GET_SERVICE))
			{
				return GlowTestData.LookupListData;
			}
			return JsonResponse;
		}

		public string Post(string relativeAddress, string content)
			=> throw new NotImplementedException();

		public void SetAdditionalHeader(string headerName, string headerValue)
		{
			throw new NotImplementedException();
		}

		public string LastRequestUrl => lastRequestUrl;
		string lastRequestUrl;

		public int CurrentRequestCount { get; set; }

		public string JsonResponse { get; set; }
	}
}
