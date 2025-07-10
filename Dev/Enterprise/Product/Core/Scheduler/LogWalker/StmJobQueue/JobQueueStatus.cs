using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.LogWalker
{
	public static class JobQueueStatus
	{
		public static string StatusQueued => "QUE";
		public static string StatusProcessed => "PRS";
		public static string StatusFailed => "FAI";
		public static IEnumerable<string> All()
		{
			return typeof(JobQueueStatus).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(p => (string)p.GetValue(null));
		}
	}
}
