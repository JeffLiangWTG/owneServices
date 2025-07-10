using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CODE39))]
	sealed class CODE39Test : BarcodeTest
	{
		#region Implementations

		protected override ValueProvider GetNewValueProvider()
		{
			return new CODE39();
		}

		protected override string CodeType { get; } = "CODE-39";

		protected override string MacroWithUnExpectedContent { get; } = @"<CODE-39(""code39 does not support chinese characters这是一个错误码"",2,2)>";

		#endregion
	}
}
