using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class EconomicOperatorWrapper : IEconomicOperator
	{
		EconomicOperatorWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = goodsLocationAddress.E2_GovRegNum);
		string identificationNumber;

		public static EconomicOperatorWrapper New(CusGoodsLocationAddress goodsLocationAddress) => goodsLocationAddress == null ? null : new EconomicOperatorWrapper(goodsLocationAddress);
	}
}
