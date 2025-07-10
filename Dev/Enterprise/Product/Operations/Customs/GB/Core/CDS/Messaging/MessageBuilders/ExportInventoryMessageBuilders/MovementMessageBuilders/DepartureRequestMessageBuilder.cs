using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class DepartureRequestMessageBuilder : MovementRequestMessageBuilder
	{
		public DepartureRequestMessageBuilder(IUkCinvWrapper messageDataProvider, GbInventoryManagementMessageFunction.MasterOrDeclaration level) : base(messageDataProvider, level)
		{
		}

		protected override void DoCustomSetting(inventoryLinkingMovementRequest request)
		{
			base.DoCustomSetting(request);
			if (!messageDataProvider.DateAndTimeTheGoodsWillBeLeavingLCPPremises.IsEmpty)
			{
				request.goodsDepartureDateTime = messageDataProvider.DateAndTimeTheGoodsWillBeLeavingLCPPremises.ToDateTime();
				request.goodsDepartureDateTimeSpecified = true;
			}
		}

		protected override messageCodeMovement MessageCode => messageCodeMovement.EDL;
	}
}
