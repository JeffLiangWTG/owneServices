using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RefDbExtendedPropertyHelperForTesting : RefDbExtendedPropertyHelper
	{
		protected override ZString RefDbName
		{
			get
			{
				return "";
			}
		}
	}
}
