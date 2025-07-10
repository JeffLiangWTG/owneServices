using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZClientWebCargoWiseEDI.Translations
{
	// As much as I hate to do this, .NET Web API has no inbuilt support for renaming
	// the parameters of a request object when using x-www-form-urlencoded, only XML
	// and JSON.
	// Do NOT rename the properties in this class to be PascalCase, and do not 'fix' the tests that enforce this.
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public class ImportRequestData
	{
		public string contents { get; set; }

		public string language { get; set; }
	}
}