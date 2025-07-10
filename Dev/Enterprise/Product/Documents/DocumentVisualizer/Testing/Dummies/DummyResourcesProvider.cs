using System;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyResourcesProvider : IResourceProvider
	{
		public IReadOnlyDictionary<string, Func<object>> Resources => new Dictionary<string, Func<object>>();
	}
}
