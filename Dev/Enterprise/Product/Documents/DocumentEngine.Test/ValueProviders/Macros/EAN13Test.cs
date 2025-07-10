using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EAN13))]
	sealed class EAN13Test : BarcodeTest
	{
		#region Implementations

		protected override ValueProvider GetNewValueProvider()
		{
			return new EAN13();
		}

		protected override string CodeType { get; } = "EAN-13";

		protected override string MacroWithUnExpectedContent { get; } = "<EAN-13(\"This is a wrong content\",2,2)>";

		#endregion
	}
}
