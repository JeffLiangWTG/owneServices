using System.Text.RegularExpressions;

namespace Enterprise.LogWalker.Testing
{
	static class GUIDStrippingExtensions
	{
		public static string StripGUIDs(this string input, string guidReplacement = "{guidguid-guid-guid-guid-guidguidguid}")
		{
			return new Regex(@"[{|\(]?[0-9a-fA-F]{8}[-]?([0-9a-fA-F]{4}[-]?){3}[0-9a-fA-F]{12}[\)|}]?")
				.Replace(input, guidReplacement);
		}
	}
}
