using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
	{
		public override ZBool AdditionalInfosSupport(BusinessObject businessObject) => false;

		protected override EU.NCTS.Business.INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsArrivalCargoDescPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalCargoDescPhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsAdditionalInfoPhase5ValidationDecider GetAdditionalInfoPhase5ValidationDecider() => new NctsAdditionalInfoPhase5ValidationDecider();

		protected override EU.NCTS.Business.NctsPreviousDocumentConfiguration GetNewNctsPreviousDocumentConfiguration() => new NctsPreviousDocumentConfiguration();

		protected override EU.NCTS.Business.NctsSupportingDocumentConfiguration GetNewNctsSupportingDocumentConfiguration() => new NctsSupportingDocumentConfiguration();
	}
}
