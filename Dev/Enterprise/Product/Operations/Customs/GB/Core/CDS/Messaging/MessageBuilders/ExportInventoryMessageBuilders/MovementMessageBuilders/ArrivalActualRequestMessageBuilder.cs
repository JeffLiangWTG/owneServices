using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class ArrivalActualRequestMessageBuilder : MovementRequestMessageBuilder
	{
		public ArrivalActualRequestMessageBuilder(IUkCinvWrapper messageDataProvider, GbInventoryManagementMessageFunction.MasterOrDeclaration level) : base(messageDataProvider, level)
		{
		}

		protected override void DoCustomSetting(inventoryLinkingMovementRequest request)
		{
			base.DoCustomSetting(request);
			if (!messageDataProvider.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.IsEmpty)
			{
				request.goodsArrivalDateTime = messageDataProvider.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.ToDateTime();
				request.goodsArrivalDateTimeSpecified = true;
			}
		}

		protected override messageCodeMovement MessageCode => messageCodeMovement.EAL;
	}
}
