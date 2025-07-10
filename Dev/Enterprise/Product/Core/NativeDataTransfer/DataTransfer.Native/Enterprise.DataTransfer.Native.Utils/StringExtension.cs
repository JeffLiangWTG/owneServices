namespace Enterprise.DataTransfer.Native.Utils
{
	public static class StringExtension
	{
		public static bool IsEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}
	}
}