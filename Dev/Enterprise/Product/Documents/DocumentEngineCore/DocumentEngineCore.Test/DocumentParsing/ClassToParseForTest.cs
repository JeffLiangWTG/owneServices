using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class ClassToParseForTest : BaseClassToParse
	{
		[DocumentField("Yum")]
		public ZString Orange
		{
			get { return "Orange"; }
		}

		[DocumentField("All about Apples")]
		public ZString Apple
		{
			get { return "I like Apples"; }
		}
	}
}
