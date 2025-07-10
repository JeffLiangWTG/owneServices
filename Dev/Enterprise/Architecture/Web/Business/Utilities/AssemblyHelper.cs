using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class AssemblyHelper
	{
		internal static IEnumerable<string> GetDistinctAssemblyNames(IEnumerable<string> files)
		{
			return files
					.Where(x => Path.GetExtension(x).Equals(".dll", StringComparison.OrdinalIgnoreCase))
					.Select(Path.GetFileNameWithoutExtension)
					.Distinct();
		}
	}
}
