using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(UPCA))]
	sealed class UPCATest : BarcodeTest
	{
		#region Implementations

		protected override ValueProvider GetNewValueProvider()
		{
			return new UPCA();
		}

		protected override string CodeType { get; } = "UPC-A";

		protected override string MacroWithUnExpectedContent { get; } = "<UPC-A(\"This is a wrong content\",2,2)>";

		#endregion
	}
}
