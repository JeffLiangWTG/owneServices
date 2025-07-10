using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public class AccTaxGLMovementSubscriber : AccountingSubscriberBase
	{
		public override ITableSchema Table => AccTaxGLMovementSchema.Instance;

		public override string Code => "ATM";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "AccTaxGLMovement Subscriber";

		public override Action<DataRow> CustomFilter => null;

		protected override Guid GetCompanyPKForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			var taxTransactionPK = GetDataRowValue<Guid>(changeRow[AccTaxGLMovementSchema.ATM_ATT_TaxTransaction.Name]);
			var taxTransaction = GetTaxTransaction(taxTransactionPK);

			return taxTransaction.ATT_GC.ToGuid();
		}

		protected override DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow)
		{
			return GetDataRowValue<DateTime>(changeRow[AccTaxGLMovementSchema.ATM_SystemCreateTimeUtc.Name]);
		}

		AccTaxTransaction GetTaxTransaction(Guid taxTransactionPK)
		{
			var taxTransactionIsInCache = TaxTransactionCache.TryGetValue(taxTransactionPK, out var taxTransaction);
			if (!taxTransactionIsInCache)
			{
				taxTransaction = DataFactory.Load<AccTaxTransaction>(taxTransactionPK);
				TaxTransactionCache.Add(taxTransactionPK, taxTransaction);
			}

			return taxTransaction;
		}

		readonly Dictionary<Guid, AccTaxTransaction> TaxTransactionCache = new Dictionary<Guid, AccTaxTransaction>();
	}
}
