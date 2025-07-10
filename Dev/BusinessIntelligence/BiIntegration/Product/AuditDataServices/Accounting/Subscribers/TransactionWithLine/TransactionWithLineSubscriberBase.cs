using System;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public abstract class TransactionWithLineSubscriberBase : AccountingSubscriberBase
	{
		public override ITableSchema Table => AccTransactionLinesSchema.Instance;

		protected override Guid GetCompanyPKForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<Guid>(changeRow[AccTransactionLinesSchema.AL_GC.Name]);
		}

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<DateTime>(changeRow[AccTransactionLinesSchema.AL_SystemCreateTimeUtc.Name]);
		}
	}
}
