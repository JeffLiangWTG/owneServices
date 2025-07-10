using System;
using System.Collections.Generic;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class AccTransactionHeaderWithARAPLedgerSubscriber : TransactionWithoutLineSubscriberBase
	{
		public override string Code => "HRP";

		public override string Description => "AccTransactionHeader added for AH_Ledger in ('AR','AP') and AH_TransactionType in ('JNL','TRF','CTR','REC','PAY','OVP','DSC','EXX')";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var validLedgers = new List<string> { "AR", "AP" };
			var validTransactionTypes = new List<string> { "TRF", "CTR", "REC", "PAY", "OVP", "DSC", "EXX" };

			var ledger = row[AccTransactionHeaderSchema.Constants.AH_Ledger] != DBNull.Value
				? row[AccTransactionHeaderSchema.Constants.AH_Ledger]
				: null;
			var transactionType = row[AccTransactionHeaderSchema.Constants.AH_TransactionType] != DBNull.Value
				? row[AccTransactionHeaderSchema.Constants.AH_TransactionType]
				: null;
			var transactionCategory = row[AccTransactionHeaderSchema.Constants.AH_TransactionCategory] != DBNull.Value
				? row[AccTransactionHeaderSchema.Constants.AH_TransactionCategory]
				: null;

			var isNeeded = validLedgers.Contains(Convert.ToString(ledger)) &&
				(validTransactionTypes.Contains(Convert.ToString(transactionType))
					|| (Convert.ToString(transactionType) == "JNL" && Convert.ToString(transactionCategory) != "PBW"));

			if (!isNeeded)
			{
				row.Delete();
			}
		};
	}
}
