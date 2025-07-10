namespace WTG.TestHelpers.MockServer
{
	public static class ContentTypes
	{
		public const string ApplicationJson = "application/json";

		public const string ApplicationXml = "application/xml";

		public const string TextPlain = "text/plain";
	}

	public static class MockResponseFactory
	{
		// OK Responses
		public static MockResponse OkJson(string content)
		{
			return new MockResponseJson(content, 200);
		}

		public static MockResponse OkXml(string content)
		{
			return new MockResponseXml(content, 200);
		}

		public static MockResponse OkText(string content)
		{
			return new MockResponseText(content, 200);
		}

		// Bad Request Responses
		public static MockResponse BadRequestJson(string content)
		{
			return new MockResponseJson(content, 400);
		}

		public static MockResponse BadRequestXml(string content)
		{
			return new MockResponseXml(content, 400);
		}

		public static MockResponse BadRequestText(string content)
		{
			return new MockResponseText(content, 400);
		}

		// Not Found Responses
		public static MockResponse NotFoundJson(string content)
		{
			return new MockResponseJson(content, 404);
		}

		public static MockResponse NotFoundXml(string content)
		{
			return new MockResponseXml(content, 404);
		}

		public static MockResponse NotFoundText(string content)
		{
			return new MockResponseText(content, 404);
		}

		// Unauthorized Responses
		public static MockResponse UnauthorizedJson(string content)
		{
			return new MockResponseJson(content, 401);
		}

		public static MockResponse UnauthorizedXml(string content)
		{
			return new MockResponseXml(content, 401);
		}

		public static MockResponse UnauthorizedText(string content)
		{
			return new MockResponseText(content, 401);
		}

		// Internal Server Error Responses
		public static MockResponse InternalServerErrorJson(string content)
		{
			return new MockResponseJson(content, 500);
		}

		public static MockResponse InternalServerErrorXml(string content)
		{
			return new MockResponseXml(content, 500);
		}

		public static MockResponse InternalServerErrorText(string content)
		{
			return new MockResponseText(content, 500);
		}
	}

	public class MockResponseJson : MockResponse
	{
		public MockResponseJson(string content, int statusCode) : base(content, statusCode, ContentTypes.ApplicationJson)
		{
		}
	}
	public class MockResponseXml : MockResponse
	{
		public MockResponseXml(string content, int statusCode) : base(content, statusCode, ContentTypes.ApplicationXml)
		{
		}
	}

	public class MockResponseText : MockResponse
	{
		public MockResponseText(string content, int statusCode) : base(content, statusCode, ContentTypes.TextPlain)
		{
		}
	}

	public class MockResponse : IMockResponse
	{
		public MockResponse(string content, int statusCode, string contentType)
		{
			Content = content;
			ContentType = contentType;
			StatusCode = statusCode;
		}

		public string Content { get; set; }

		public int StatusCode { get; set; }

		public string ContentType { get; set; }
	}
}
