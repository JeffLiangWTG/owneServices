using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class APCashAdvanceChargeCollection : ChargeCollection
	{
		public APCashAdvanceChargeCollection(ChargeWithCost parentCostCharge)
			: base(parentCostCharge.InvoicingJob)
		{
			ParentCostCharge = parentCostCharge;
		}

		public readonly ChargeWithCost ParentCostCharge;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery();
			var linkedCAL = ParentCostCharge.APCashAdvanceRequestLine;
			if (linkedCAL != null)
			{
				var charges = linkedCAL.RequestHeader.Lines.Select(l => l.RelatedJobCharge);
				query.AddToFilter(JobChargeSchema.PK, charges.Select(x => x.PK));
			}
			else
			{
				query = base.CreateRelationshipFilter();  // call base logic first, so eligible charges must come from the same job etc.
				query.AddToFilter(JobChargeSchema.JR_CAL_APLine, ZGuid.Empty); // not in any cash advance request
				query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, ParentCostCharge.JR_OH_CostAccount); // same creditor
				query.AddToFilter(JobChargeSchema.JR_RX_NKCostCurrency, ParentCostCharge.JR_RX_NKCostCurrency); // same currency
				query.AddToFilter(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.GreaterThan, ZDecimal.Zero);  // positive cost amount

				//Cost not posted yet
				var queryPayableLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
				queryPayableLine.AddToFilter(AccTransactionLinesSchema.AL_LineType, new[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual });
				var queryCheckingUnpostedPayable = new ZDBOnlyQuery(typeof(JobCharge));
				queryCheckingUnpostedPayable.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_APLine, DBNull.Value);
				queryCheckingUnpostedPayable.AddSubQuery(JobChargeSchema.JR_AL_APLine, queryPayableLine, JoinCondition.Or);
				query.AddToFilter(queryCheckingUnpostedPayable, JoinCondition.And);
			}
			return query;
		}
	}
}
