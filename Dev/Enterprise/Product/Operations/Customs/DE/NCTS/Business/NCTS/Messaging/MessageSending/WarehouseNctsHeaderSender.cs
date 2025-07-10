using System.Linq;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public static class WarehouseNctsHeaderSender
	{
		public static bool PreSend(NctsHeader nctsHeader)
		{
			var shouldSend = true;

			if (!nctsHeader.MovementHeader.BM_OA_WarehouseAddress.IsEmpty && nctsHeader.Bills.All(x => !x.IsOutwardOrderImported))
			{
				var publishResult = nctsHeader.PublishShipmentForWHSOutward();
				shouldSend = publishResult is { ResultType: not UniversalResult.HadErrors };
				if (!shouldSend && nctsHeader.HasMessageInitiator && publishResult?.ErrorMessage is { IsEmpty: false } errorMessage)
				{
					nctsHeader.MessageInitiator.NotifyUserOfAnInvalidOperation(errorMessage);
				}
			}

			return shouldSend;
		}
	}
}
