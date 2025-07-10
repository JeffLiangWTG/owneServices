using System;
using System.Text;

namespace CargoWise.EntityFramework
{
	static class StringBuilderPool
	{
		internal static IDisposable Get(out StringBuilder builder)
		{
			builder = new StringBuilder();
			return null;
		}
	}
}
