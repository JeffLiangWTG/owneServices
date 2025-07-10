using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class ClassWithMethodToParseForTest : AnotherClassToParseForTest
	{
		[DocumentField("Get Lemon")]
		public ZString GetLemon(bool freshFirst)
		{
			return freshFirst ? "Fresh Lemon" : "Lemon";
		}
	}
}
