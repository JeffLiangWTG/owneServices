using System.Globalization;

namespace CargoWise.Types
{
	/// <summary>
	/// Interface to provide culture
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public interface ICultureProvider
	{
		CultureInfo Culture { get; }
	}
}
