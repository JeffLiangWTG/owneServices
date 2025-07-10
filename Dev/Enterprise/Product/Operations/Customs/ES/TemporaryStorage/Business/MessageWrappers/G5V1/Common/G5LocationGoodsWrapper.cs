using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5LocationGoodsWrapper : IG5LocationGoods
	{
		public G5LocationGoodsWrapper(ZString customsOffice, CusGoodsLocation location)
		{
			this.location = Argument.NotNull(location, nameof(location));
			customsOfficeIsES = customsOffice.StartsWith(Core.Constants.CountryCodes.Spain);
		}
		readonly CusGoodsLocation location;
		readonly ZBool customsOfficeIsES;

		public ZString NationalLocation => customsOfficeIsES ? location.Address?.AuthorisationNumber ?? ZString.Empty : ZString.Empty;

		public IGenericLocation GenericLocation => locationOfGoods ??= customsOfficeIsES ? null : new G5GenericLocationWrapper(location);
		G5GenericLocationWrapper locationOfGoods;
	}
}
