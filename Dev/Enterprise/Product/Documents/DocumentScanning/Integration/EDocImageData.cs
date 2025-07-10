using System.Diagnostics.CodeAnalysis;

namespace Enterprise.DocumentScanning.Integration
{
	public class EDocImageData
	{
		public string FullFileName { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] Data { get; set; }
	}
}
