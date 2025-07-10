using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.Common.GUI
{
	public class RedirectUrlResponse
	{
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string Url { get; set; }
		public DateTime TokenExpiry { get; set; }
	}
}
