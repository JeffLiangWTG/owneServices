using System.Diagnostics;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[CodeAlive("Used for compiler checking only, not compiled into build")]
	public static class ZBindToChecker
	{
		/// <summary>
		/// This will provide compile-time checking of bind-tos. This is NEVER executed. Even the expression is not executed.
		/// </summary>
		[Conditional("dont_run_this_thing_EVER")]
		public static void CheckBindTo(object x)
		{
		}
	}
}
