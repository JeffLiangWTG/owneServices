using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class PersonPresentingTheGoodsWrapper : IPersonPresentingTheGoods
	{
		PersonPresentingTheGoodsWrapper(TemporaryStorageHeader goodsLocationAddress)
		{
			this.header = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly TemporaryStorageHeader header;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = GetIdentificationNumber());
		string identificationNumber;

		string GetIdentificationNumber() => header.Presenter?.GetEORI() ?? string.Empty;

		public static PersonPresentingTheGoodsWrapper New(TemporaryStorageHeader header) => header == null ? null : new PersonPresentingTheGoodsWrapper(header);
	}
}
