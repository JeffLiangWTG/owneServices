using System.Diagnostics.CodeAnalysis;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.CodeStyle
{
	class IDE0044
	{
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Only testing IDE0044")]
		//IDE0044:Add readonly modifier
		string message;

		public IDE0044()
		{
			message = "Hello world";
		}
	}
}
