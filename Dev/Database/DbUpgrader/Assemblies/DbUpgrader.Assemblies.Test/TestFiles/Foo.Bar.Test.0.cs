using System.Reflection;
using Microsoft.SqlServer.Server;

namespace Foo
{
	public class Bar
	{
		[SqlFunction(Name = "Test")]
		public static string Test()
		{
			return "zero";
		}
	}
}