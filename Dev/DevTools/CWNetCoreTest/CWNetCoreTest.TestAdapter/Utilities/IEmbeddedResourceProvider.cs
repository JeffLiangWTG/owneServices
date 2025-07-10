using System;
using System.Collections.Generic;

namespace CWNetCoreTest.TestAdapter.Utilities
{
	public interface IEmbeddedResourceProvider
	{
		HashSet<string> LoadExplicitTestsFromContentFiles(string relativeDirectoryName, Func<string, bool>? filter = null);
	}
}
