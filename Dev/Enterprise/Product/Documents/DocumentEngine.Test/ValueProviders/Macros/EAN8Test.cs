using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EAN8))]
	sealed class EAN8Test : BarcodeTest
	{
		#region Implementations

		protected override ValueProvider GetNewValueProvider()
		{
			return new EAN8();
		}

		protected override string CodeType { get; } = "EAN-8";

		protected override string MacroWithUnExpectedContent { get; } = "<EAN-8(\"This is a wrong content\",2,2)>";

		#endregion
	}
}
