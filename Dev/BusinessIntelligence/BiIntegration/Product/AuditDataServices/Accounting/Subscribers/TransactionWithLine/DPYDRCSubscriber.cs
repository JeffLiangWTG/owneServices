using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class DPYDRCSubscriber : TransactionWithLineSubscriberBase
	{
		public override string Code => "DPR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "Direct Payment and Direct Receipt Subscriber";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[AccTransactionLinesSchema.AL_LineType.Name] == DBNull.Value
				|| (Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "DPY" && Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "DRC"))
			{
				row.Delete();
			}
		};
	}
}
