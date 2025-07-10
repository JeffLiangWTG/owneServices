namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Initialize reference type static fields inline
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Suppressed for testing purposes")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Suppressed for testing purposes")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Suppressed for testing purposes")]
	class CA1810
	{
		static int someInteger;

		// CA1810 Initialize all static fields in 'CA1810' when those fields are declared and remove the explicit static constructor
		static CA1810()
		{
			someInteger = 3;
		}
	}
}
