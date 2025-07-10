using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class EconomicOperatorWrapper : IEconomicOperator
	{
		EconomicOperatorWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public static EconomicOperatorWrapper New(CusGoodsLocationAddress goodsLocationAddress) => goodsLocationAddress == null ? null : new EconomicOperatorWrapper(goodsLocationAddress);

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = goodsLocationAddress.E2_GovRegNum);
		string identificationNumber;
	}
}
