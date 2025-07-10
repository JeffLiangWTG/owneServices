using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class EInvoicingBatchCreatorForTaiwan : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreatorForTaiwan(GlbCompany company)
			: base(company)
		{ }

		protected override DynamicBusinessObjectCollection GetAllTransactionsForCompany(string[] actionTypes = null, string[] actionTypesWithTtransID = null, bool queryJoinWithHeadersOnly = false)
		{
			var selectQuery = @"SELECT AIP_PK, AIP_GC, ADH_TransactionType, ADH_VoidingReason FROM dbo.AccEInvoicingTransactionPivot
								INNER JOIN dbo.AccComplianceDocumentHeader ON AIP_ParentID = ADH_PK
								WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK";
			var sqlParameters = new[]
			{
				ZSqlParameter.New("@Status", Core.Constants.EInvoicingPivotState.Queued, AccEInvoicingTransactionPivotSchema.AIP_Status),
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC)
			};
			var allTransactionsForCompany = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			allTransactionsForCompany.Load(selectQuery, sqlParameters);

			return allTransactionsForCompany;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?
				.GroupBy(x => (x[AccEInvoicingTransactionPivotSchema.Constants.AIP_GC], x[AccComplianceDocumentHeaderSchema.Constants.ADH_TransactionType], string.IsNullOrEmpty(x[AccComplianceDocumentHeaderSchema.Constants.ADH_VoidingReason] as ZString?)))
				.Select(g => QueuedPivotPKs.FromDynamicBizOCollection(g)) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}
