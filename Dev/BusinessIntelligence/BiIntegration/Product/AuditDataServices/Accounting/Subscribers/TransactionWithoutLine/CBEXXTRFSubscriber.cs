using System;
using System.Collections.Generic;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class CBEXXTRFSubscriber : TransactionWithoutLineSubscriberBase
	{
		public override string Code => "OEF";

		public override string Description => "AccTransactionHeader added for AH_Ledger = CB and AH_TransactionType in (EXX,TRF)";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var validTypes = new List<string> { "EXX", "TRF" };
			if (row[AccTransactionHeaderSchema.Constants.AH_Ledger] == DBNull.Value
				|| Convert.ToString(row[AccTransactionHeaderSchema.Constants.AH_Ledger]) != "CB"
				|| row[AccTransactionHeaderSchema.Constants.AH_TransactionType] == DBNull.Value
				|| !validTypes.Contains(Convert.ToString(row[AccTransactionHeaderSchema.Constants.AH_TransactionType])))
			{
				row.Delete();
			}
		};
	}
}
