using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	public class AnotherClassToParseForTest
	{
		[DocumentField("Lemons melons")]
		public ZString Lemon
		{
			get { return "Lemon"; }
		}

		[DocumentField("All about mummies")]
		public ZString Mummies
		{
			get { return "Mummies are Egyptian"; }
		}

		[DocumentField("All about QuickGetLatest")]
		public ZString QGL
		{
			get { return "it doesnt work"; }
		}

		public ZString NoDesc
		{
			get { return "no desc"; }
		}
	}
}
