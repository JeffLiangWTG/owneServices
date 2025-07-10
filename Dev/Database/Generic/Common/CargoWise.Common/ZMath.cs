namespace CargoWise.Common
{
	public static class ZMath
	{
		/// <summary>
		/// Returns Value if it lies within min and max, otherwise it returns min or max
		/// </summary>
		/// <param name="value">The value you wish to clamp</param>
		/// <param name="min">The minimum value (inclusive)</param>
		/// <param name="max">The maximum value (inclusive)</param>
		/// <returns></returns>
		public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
		{
			if (min.CompareTo(max) > 0)
			{
				throw new System.ArgumentException("Invalid argument, minimum value must be smaller than maximum value", nameof(min)); // Hard coded exception message
			}

			return value.CompareTo(min) < 0	? min : value.CompareTo(max) > 0 ? max : value;
		}
	}
}
