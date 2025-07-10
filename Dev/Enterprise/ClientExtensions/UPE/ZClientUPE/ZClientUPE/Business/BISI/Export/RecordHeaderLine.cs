
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class RecordHeaderLine : RecordLine
	{
		public RecordHeaderLine(int batchNo, int totalShipments, int totalLines)
		{
			this.BatchNo = batchNo;
			this.TotalShipments = totalShipments;
			this.TotalLines = totalLines;
		}

		protected override void AppendFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, ZDateTime.Now, false, Length.CurrentDate);
			AppendFixedLengthField(lineBuilder, ZDateTime.Now, true, Length.CurrentTime);
			AppendFixedLengthField(lineBuilder, BatchNo, Length.BatchNo);
			AppendFixedLengthField(lineBuilder, TotalShipments, Length.TotalShipments);
			AppendFixedLengthField(lineBuilder, TotalLines, Length.TotalLines);
			AppendFixedLengthField(lineBuilder, "", Length.Filler);
		}

		#region Constants

		static class Length
		{
			public const int CurrentDate = 10;
			public const int CurrentTime = 8;
			public const int BatchNo = 6;
			public const int TotalShipments = 5;
			public const int TotalLines = 6;
			public const int Filler = 256;
		}

		#endregion

		readonly int BatchNo;
		readonly int TotalShipments;
		readonly int TotalLines;
	}
}
