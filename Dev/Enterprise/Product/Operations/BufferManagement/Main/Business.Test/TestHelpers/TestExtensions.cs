using System.Text.RegularExpressions;

namespace Enterprise.BufferManagement.Business.Test
{
	public static class TestExtensions
	{
		public static string StripTaskIds(this string str)
		{
			return Regex.Replace(str, "T\\d{8}", "TASK_ID");
		}
	}
}
