namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentC1ReleaseOrchestrator
	{
		public NonPersistentC1ReleaseOrchestrator(ICcsukCusAwb awb)
		{
			C1ReleaseHelper = new NonPersistentC1Release(awb);
			this.awb = awb;
		}

		public NonPersistentC1Release C1ReleaseHelper { get; private set; }

		public void ReleaseAndPrintC1OnFsn()
		{
			if (new CusAwbIsReadOnlyHelper(awb).IsActionAllowed(Actions.CW_C1Release))
			{
				awb.ReleaseThisNumberOfPieces(C1ReleaseHelper.NumberOfPieces, NumberOfPiecesReleasedHelper.AgentC1Event);
				ReleasePrintHelper.FindFsnThenPrintReleaseOriginalAndReprint(awb, null, null);
			}
		}

		readonly ICcsukCusAwb awb;
	}
}
