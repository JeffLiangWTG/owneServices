using System.Collections.Generic;

namespace WTG.TestHelpers.MockServer
{
	public interface IMockRequest
	{
		string Content { get; set; }
		Dictionary<string, string> Headers { get; set; }
	}
}