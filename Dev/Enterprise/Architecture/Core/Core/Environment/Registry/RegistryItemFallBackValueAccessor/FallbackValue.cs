using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Used by the RegistryItemFallBackValueAccessor to store the default fallback value obtained as well as the level the value
	/// was obtained from.
	/// </summary>
	public class FallbackValue
	{
		public FallbackValue(RegistryStorageFlags level, object value) : this(level, false, value)
		{
		}

		public FallbackValue(RegistryStorageFlags level, bool isMergedWithDefaultValue, object value)
		{
			Level = level;
			IsMergedWithDefaultValue = isMergedWithDefaultValue;
			Value = value;
		}

		/// <summary>
		/// The fallback that the default value was obtained from.
		/// RegistryStorageFlags.All in this case is used to indicate that no fallback parent exists (using registry datatype default).
		/// </summary>
		public readonly RegistryStorageFlags Level;

		/// <summary>
		/// Is the value obtained by merging values from the default value and multiple fallback values?
		/// </summary>
		public readonly bool IsMergedWithDefaultValue;

		/// <summary>
		/// The default value obtained.
		/// </summary>
		public readonly object Value;
	}
}
