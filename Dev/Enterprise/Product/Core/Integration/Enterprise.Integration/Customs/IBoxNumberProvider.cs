using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBoxNumberProvider
		{
			ICodeDescriptionPairList GetBoxNumberList(BusinessObjectFactory factory, ZString customsDistrict);

			ZString GetDefaultBoxNumber(BusinessObjectFactory factory, ZString customsDistrict);
		}
	}
}
