using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class REVCSTTransactionLineSubscriber : TransactionWithLineSubscriberBase
	{
		public override string Code => "RCS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "REV/CST TranssactionLine Subscriber";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[AccTransactionLinesSchema.AL_LineType.Name] == DBNull.Value
				|| (Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "REV" && Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "CST"))
			{
				row.Delete();
			}
		};
	}
}
