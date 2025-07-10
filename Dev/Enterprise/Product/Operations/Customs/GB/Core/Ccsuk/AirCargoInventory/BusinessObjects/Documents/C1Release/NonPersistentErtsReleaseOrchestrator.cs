using System.Linq;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentErtsReleaseOrchestrator
	{
		public NonPersistentErtsReleaseOrchestrator(ICcsukCusAwb awb, ILogger logger = null)
		{
			ErtsReleaseHelper = new NonPersistentErtsRelease(awb);
			ErtsReleaseHelper.NumberOfPiecesReadOnly = true;
			ErtsReleaseHelper.NumberOfPieces = (from CusOutTurn ot in awb.OutTurns where ot.IsBeingReleasedNow select (int)ot.C5_PackagesOutturned).Sum();
			this.awb = awb;
			this.logger = logger;
		}

		public NonPersistentErtsRelease ErtsReleaseHelper { get; private set; }

		public bool ReleaseAndPrintRRAOnFsnOrAwb(GbEDIMessage fsnMessage)
		{
			if (ErtsReleaseHelper.NumberOfPieces > 0)
			{
				var helper = new CusAwbIsReadOnlyHelper(awb);
				if (helper.IsActionAllowed(Actions.CW_EcStatusRelease) || helper.CanReleaseAwbForShedRRA(fsnMessage))
				{
					awb.ReleaseThisNumberOfPieces(ErtsReleaseHelper.NumberOfPieces, NumberOfPiecesReleasedHelper.ShedEvent);
					ReleasePrintHelper.FindFsnThenPrintReleaseOriginalAndReprint(awb, logger, fsnMessage);
					return true;
				}
			}
			return false;
		}

		readonly ICcsukCusAwb awb;
		readonly ILogger logger;
	}
}
