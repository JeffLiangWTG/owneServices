using System;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public abstract class TransactionWithoutLineSubscriberBase : AccountingSubscriberBase
	{
		public override ITableSchema Table => AccTransactionHeaderSchema.Instance;

		protected override Guid GetCompanyPKForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<Guid>(changeRow[AccTransactionHeaderSchema.AH_GC.Name]);
		}

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<DateTime>(changeRow[AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.Name]);
		}
	}
}
