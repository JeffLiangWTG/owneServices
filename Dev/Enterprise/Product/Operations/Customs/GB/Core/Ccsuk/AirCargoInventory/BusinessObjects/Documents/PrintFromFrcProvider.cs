using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class PrintFromFrcProvider : PrintFromMessageProvider
	{
		public PrintFromFrcProvider(ICcsukCusAwb awb, ZShort piecesReceivedBeforeUpdate, EDIMessage baseMessage, ILogger logger)
			: base(baseMessage, logger)
		{
			this.piecesReceivedBeforeUpdate = piecesReceivedBeforeUpdate;
		}

		public override void DoPrinting()
		{
			var newPiecesReceived = Awb.NumberOfPiecesReceived;
			if (newPiecesReceived > piecesReceivedBeforeUpdate
				&& newPiecesReceived == Awb.NumberOfPiecesExpected
				&& (
						Awb.CustomsActionCode == CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval
						|| Awb.CustomsActionCode == CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval
					)
				&& GBCustomsDataRegistry.Instance.CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased.Value
				)
			{
				var piecesRemainingToBeAutomaticallyReleased = Awb.NumberOfPiecesReceived - Awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event);
				Awb.ReleaseThisNumberOfPieces(piecesRemainingToBeAutomaticallyReleased, NumberOfPiecesReleasedHelper.AgentC1Event);  // FRC only received by agent, to release type must be agent
				ReleasePrintHelper.PrintC1OriginalAndReprint(gbEdiMessage, logger);
			}
		}
		readonly ZShort piecesReceivedBeforeUpdate;
	}
}
