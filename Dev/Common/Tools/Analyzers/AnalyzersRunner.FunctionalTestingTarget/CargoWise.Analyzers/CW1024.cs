using System.Collections.Concurrent;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1024
	{
		public void Method()
		{
			var dictionary = new ConcurrentDictionary<string, object>();
			dictionary.ContainsKey("test");

			//CW1024:Bad Concurrent Collection Access
			_ = dictionary.TryGetValue("test", out _);
		}
	}
}
