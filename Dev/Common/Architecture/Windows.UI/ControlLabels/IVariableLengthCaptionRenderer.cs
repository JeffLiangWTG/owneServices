namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on a control or component that implements variable length captions.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public interface IVariableLengthCaptionRenderer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		string[] Captions { get; set; }

		bool IsCaptionOverridden { get; set; }
	}
}
