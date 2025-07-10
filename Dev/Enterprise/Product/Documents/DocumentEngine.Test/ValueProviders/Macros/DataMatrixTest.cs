using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DataMatrix))]
	sealed class DataMatrixTest : BarcodeTest
	{
		#region Implementations

		protected override string MacroWithUnExpectedContent { get; } = "<DataMatrix(\"DataMatrix不支持汉字\", 3, 3)>";

		protected override ValueProvider GetNewValueProvider()
		{
			return new DataMatrix();
		}

		protected override string CodeType { get; } = "DataMatrix";

		#endregion
	}
}
