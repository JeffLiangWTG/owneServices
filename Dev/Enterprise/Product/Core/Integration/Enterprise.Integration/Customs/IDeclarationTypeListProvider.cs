using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IDeclarationTypeListProvider
		{
			ICodeDescriptionPairList GetListFor(string country);
		}
	}
}