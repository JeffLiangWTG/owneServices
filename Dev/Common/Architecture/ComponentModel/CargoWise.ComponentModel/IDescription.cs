using System.Globalization;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A list of descriptions in order of length.
	/// </summary>
	public interface IDescription
	{
		/// <summary>
		/// Get the number of descriptions available.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Get a description given the index using the current culture.
		/// </summary>
		string GetDescription(int index);

		/// <summary>
		/// Get a description given the index and culture.
		/// </summary>
		string GetDescription(int index, CultureInfo culture);
	}
}
