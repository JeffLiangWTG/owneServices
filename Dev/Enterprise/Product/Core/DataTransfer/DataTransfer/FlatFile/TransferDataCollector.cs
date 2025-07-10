namespace Enterprise.DataTransfer.Business
{
	public class TransferDataCollector : ITransferDataCollector
	{
		public int UnitsProcessed
		{
			get { return fUnitsProcessed; }
			set { fUnitsProcessed = value; }
		}

		bool ITransferDataCollector.BOF
		{
			get { return fBOF; }
			set { fBOF = value; }
		}

		bool ITransferDataCollector.EOF
		{
			get { return fEOF; }
			set { fEOF = value; }
		}

		bool fBOF;
		bool fEOF;
		int fUnitsProcessed;
	}
}
