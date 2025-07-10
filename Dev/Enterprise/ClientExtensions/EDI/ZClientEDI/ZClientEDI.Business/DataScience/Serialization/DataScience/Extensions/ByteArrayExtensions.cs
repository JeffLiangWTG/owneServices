using System.Linq;

namespace WTG.Serialization.DataScience.Extensions
{
	public static class ByteArrayExtensions
	{
		public const string SerializationPrefix = "0x";

		public static string ToHexString(this byte[] byteArray) =>
			byteArray.Length == 0
				? string.Empty
				: string.Join(
					null,
					new[] { SerializationPrefix }.Concat(byteArray.Select(b => $"{b:X2}")));
	}
}
