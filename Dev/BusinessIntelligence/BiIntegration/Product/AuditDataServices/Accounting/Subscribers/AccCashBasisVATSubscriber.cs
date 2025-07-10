using System;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class AccCashBasisVATSubscriber : AccountingSubscriberBase
	{
		public override string Code => "ACB";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "AccCashBasisVAT Subscriber";

		public override ITableSchema Table => AccCashBasisVATSchema.Instance;

		public override Action<DataRow> CustomFilter => null;

		protected override Guid GetCompanyPKForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<Guid>(changeRow[AccCashBasisVATSchema.YC_GC.Name]);
		}

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<DateTime>(changeRow[AccCashBasisVATSchema.YC_SystemCreateTimeUtc.Name]);
		}
	}
}
