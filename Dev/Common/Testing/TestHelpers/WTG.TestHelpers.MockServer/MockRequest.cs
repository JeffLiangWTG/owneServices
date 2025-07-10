using System.Collections.Generic;

namespace WTG.TestHelpers.MockServer
{
	public class MockRequest : IMockRequest
	{
		public string Content { get; set; }

		public Dictionary<string, string> Headers { get; set; }
	}
}
