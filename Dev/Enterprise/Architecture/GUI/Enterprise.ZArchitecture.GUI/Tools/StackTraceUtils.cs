using System;

namespace Enterprise.ZArchitecture.GUI
{
	public static class StackTraceUtils
	{
		/// <summary>
		/// Returns true if called directly or indirectly from the given method; otherwise, false.
		/// </summary>
		/// <param name="className">Declaring class of the expected calling method.</param>
		/// <param name="methodName">Name of the expected calling methid.</param>
		/// <param name="stackDepth">Number of stack frames to analyze. If one, will only detect method that called <see cref="IsCalledFrom"/> directly. If less than one, then always returns false.</param>
		/// <returns></returns>
		internal static bool IsCalledFrom(string className, string methodName, int stackDepth)
		{
			var stackTrace = new System.Diagnostics.StackTrace(1, false);
			var len = Math.Min(stackDepth, stackTrace.FrameCount);
			for (var i = 0; i < len; i++)
			{
				var method = stackTrace.GetFrame(i).GetMethod();
				if (method != null && method.DeclaringType?.Name == className && method.Name == methodName)
				{
					return true;
				}
			}
			return false;
		}
	}
}
