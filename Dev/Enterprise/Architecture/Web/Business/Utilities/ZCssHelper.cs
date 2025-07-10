using System;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public static class ZCssHelper
	{
		/// <summary>
		/// Joins multiple CSS classes, trims space and skips empty classes
		/// </summary>
		/// <param name="styles">An array or set of CSS classes</param>
		/// <returns>A single CSS class string</returns>
		public static string Join(params string[] styles)
		{
			string[] s = Array.ConvertAll(styles, delegate(string value) { return value.Trim(); });
			return string.Join(" ", Array.FindAll(s, delegate(string value) { return !string.IsNullOrEmpty(value); }));
		}
	}
}
