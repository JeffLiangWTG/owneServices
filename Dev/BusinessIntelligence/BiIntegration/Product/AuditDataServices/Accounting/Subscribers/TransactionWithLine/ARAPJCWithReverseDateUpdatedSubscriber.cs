using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class ARAPJCWithReverseDateUpdatedSubscriber : TransactionWithLineSubscriberBase
	{
		public override string Code => "URD";

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override string Description => "A subscriber responsible for updating AL_ReverseDate for AR/AR INV/ADJ/CRD JC WIP/ACR/JNL/JRJ";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var validTypes = new List<string> { "CST", "REV", "WIP", "ACR" };
			if (row["AL_ReverseDate"] == DBNull.Value || row["AL_LineType"] == DBNull.Value || !validTypes.Contains(Convert.ToString(row["AL_LineType"])))
			{
				row.Delete();
			}
		};

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			AccTransactionLinesSchema.AL_ReverseDate,
		};

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return DateTime.MinValue;
		}
	}
}
