namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	interface INumberToWords
	{
		/// <summary>
		/// Returns a given number in written languages
		/// </summary>
		/// <returns>returns the Number property in words</returns>
		string GetNumberAsString(long number);

		/// <summary>
		/// Seperator for decimal numbers, seperating whole numbers from decimal numbers.
		/// </summary>
		string DecimalSeperatorAsString { get; }
	}
}
