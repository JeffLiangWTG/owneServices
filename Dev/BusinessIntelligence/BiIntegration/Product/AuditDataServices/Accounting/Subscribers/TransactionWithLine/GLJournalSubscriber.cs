using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class GLJournalSubscriber : TransactionWithLineSubscriberBase
	{
		public override string Code => "GJS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "GL Journal Subscriber";

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var validTypes = new List<string> { "GJL", "RJL", "AJL", "NJL" };
			if (row[AccTransactionLinesSchema.Constants.AL_LineType] == DBNull.Value || !validTypes.Contains(Convert.ToString(row[AccTransactionLinesSchema.Constants.AL_LineType])))
			{
				row.Delete();
			}
		};

		public override bool NotifyUpdate => true;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			AccTransactionLinesSchema.AL_LineAmount,
			AccTransactionLinesSchema.AL_OSAmount,
			AccTransactionLinesSchema.AL_OH,
			AccTransactionLinesSchema.AL_RX_NKTransactionCurrency,
			AccTransactionLinesSchema.AL_ExchangeRate,
			AccTransactionLinesSchema.AL_AG,
			AccTransactionLinesSchema.AL_GB,
			AccTransactionLinesSchema.AL_GE,
			AccTransactionLinesSchema.AL_PostDate,
			AccTransactionLinesSchema.AL_ReverseDate
		};

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return changeRow.RowState == DataRowState.Added ? GetDataRowValue<DateTime>(changeRow[AccTransactionLinesSchema.AL_SystemCreateTimeUtc.Name]) : DateTime.MinValue;
		}
	}
}
