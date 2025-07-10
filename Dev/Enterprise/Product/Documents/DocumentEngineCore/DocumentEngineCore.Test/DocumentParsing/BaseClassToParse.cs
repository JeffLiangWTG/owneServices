using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	class BaseClassToParse
	{
		[DocumentField("somasdasd")]
		public ZString InheritedProperty
		{
			get { return "Inherited Property"; }
		}
	}
}
