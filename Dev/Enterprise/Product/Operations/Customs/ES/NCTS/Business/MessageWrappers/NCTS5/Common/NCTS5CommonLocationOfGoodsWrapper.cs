using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonLocationOfGoodsWrapper : INCTSCommonLocationOfGoods
	{
		public NCTS5CommonLocationOfGoodsWrapper(NctsCusGoodsLocation location)
		{
			this.location = Argument.NotNull(location, nameof(location));
		}
		readonly NctsCusGoodsLocation location;

		public ZString TypeOfLocation => location.CGL_Type;

		public ZString QualifierOfIdentification => location.CGL_Qualifier;

		public ZString AuthorisationNumber => location.CGL_AdditionalIdentifier.Length > 10 ? location.CGL_AdditionalIdentifier.SubstringSafe(4) : location.CGL_AdditionalIdentifier;
	}
}
