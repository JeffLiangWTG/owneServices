namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on TabControl objects to enable TabControlTabKeyHandler functionality.
	/// </summary>
	public interface ITabOrderExtendedToTabPages
	{
		/// <summary>
		/// Get whether to extend tab ordering across multiple tab pages.
		/// </summary>
		bool TabOrderExtendedToTabPages { get; }
	}
}
