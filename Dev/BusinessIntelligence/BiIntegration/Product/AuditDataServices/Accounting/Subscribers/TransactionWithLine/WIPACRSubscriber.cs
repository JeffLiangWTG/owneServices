using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class WIPACRSubscriber : TransactionWithLineSubscriberBase
	{
		public override string Code => "WAR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "WIP and Accrual Subscriber";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[AccTransactionLinesSchema.AL_LineType.Name] == DBNull.Value
				|| (Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "WIP" && Convert.ToString(row[AccTransactionLinesSchema.AL_LineType.Name]) != "ACR"))
			{
				row.Delete();
			}
		};
	}
}
