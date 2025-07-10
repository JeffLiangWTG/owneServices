using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(PDF417))]
	sealed class PDF417Test : BarcodeTest
	{
		#region Replace

		public override void TestIsResponsibleForReplacing()
		{
			Assert("should match <PDF417(\"abc\",3,4,\"L1\",\"UTF-8\")>", ValueProviderToTest.IsResponsibleForReplacing("<PDF417(\"abc\",3,4,\"L1\",\"UTF-8\")>", Passes.FirstPass));
			Assert("should match <PDF417(\"abc\",3,4,\"AUTO\",\"UTF-8\")>", ValueProviderToTest.IsResponsibleForReplacing("<PDF417(\"abc\",3,4,\"AUTO\",\"UTF-8\")>", Passes.FirstPass));
			base.TestIsResponsibleForReplacing();
		}

		#endregion

		#region Implementations

		protected override string MacroWithUnExpectedContent { get; } = "<PDF417(\"Your Australia Company\", 3, 3, \"AUTO\",\"whatever\")>";

		protected override ValueProvider GetNewValueProvider()
		{
			return new PDF417();
		}

		protected override string CodeType { get; } = "PDF417";

		#endregion
	}
}
