using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	[UniversalDataContext(DataContextType.AccEInvoicingBatch)]
	[SystemDefinedValues]
	public class AccEInvoicingBatch : AutoAccEInvoicingBatch
	{
		public new abstract class Schema : AutoAccEInvoicingBatch.Schema
		{
			public const string AIB_QueryTimes = "AIB_QueryTimes";
		}

		public AccEInvoicingBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AIB_Status

		[List("AIB_Status_List")]
		public override ZString AIB_Status
		{
			get { return base.AIB_Status; }
			set
			{
				if (value != base.AIB_Status)
				{
					base.AIB_Status = value;

					if (base.AIB_Status == EInvoicingBatchState.Discarded)
					{
						ClearQueryTimes();
					}
				}
			}
		}

		public static CodeDescriptionPairList AIB_Status_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.EInvoicingBatchState);
			}
		}

		#endregion

		public ZInt AIB_QueryTimes
		{
			get
			{
				return this.GetSystemDefinedValue<ZInt>(Schema.AIB_QueryTimes);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.AIB_QueryTimes, AddOnColumnDataType.Codes.Integer, value);
			}
		}

		public void ClearQueryTimes()
		{
			AIB_QueryTimes = ZInt.Zero;
		}

		public InvoicingBase[] GetInvoicesWithStatus(params ZString[] status)
		{
			var subQuery = GetSubQueryWithStatus(status);
			var invoiceQuery = new ZDBOnlyQuery(typeof(InvoicingBase));
			invoiceQuery.AddSubQuery(subQuery, JoinCondition.And);
			var invoiceResult = new InvoicingBaseCollection(Factory, invoiceQuery);
			invoiceResult.Load();
			return invoiceResult.Cast<InvoicingBase>().ToArray();
		}

		public AccComplianceDocumentHeader[] GetComplianceDocumentsWithStatus(params ZString[] status)
		{
			var subQuery = GetSubQueryWithStatus(status);
			var complianceQuery = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			complianceQuery.AddSubQuery(subQuery, JoinCondition.And);
			var complianceResult = new AccComplianceDocumentHeaderCollection(Factory, complianceQuery);
			complianceResult.Load();
			return complianceResult.Cast<AccComplianceDocumentHeader>().ToArray();
		}

		ZDBOnlySubQuery GetSubQueryWithStatus(params ZString[] status)
		{
			var subquery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			subquery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_AIB, PK.ToGuid());
			if (status?.Any() ?? false)
			{
				subquery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, status);
			}
			return subquery;
		}

		AccEInvoicingTransactionPivotCollection fTransactionPivots;
		[ChildEditable(true)]
		public AccEInvoicingTransactionPivotCollection TransactionPivots
		{
			get
			{
				if (fTransactionPivots == null)
				{
					fTransactionPivots = new AccEInvoicingTransactionPivotCollection(this);
					RegisterEditableChildObject(fTransactionPivots);
					fTransactionPivots.Load();
				}

				return fTransactionPivots;
			}
		}

		public void UpdatePivotsStatus(string status, IEnumerable<ZGuid> transactionPKs, string errorDescription, ZDateTime? lastResponseReceived)
		{
			if (transactionPKs == null || !transactionPKs.Any())
			{
				return;
			}
			foreach (AccEInvoicingTransactionPivot pivot in TransactionPivots)
			{
				if (transactionPKs.Contains(pivot.AIP_ParentID))
				{
					pivot.AIP_Status = status;
					pivot.AIP_ErrorDescription = errorDescription;
					if (lastResponseReceived.HasValue)
					{
						pivot.AIP_LastResponseReceivedUtc = lastResponseReceived.Value;
					}
				}
			}
		}

		public void UpdatePivotsLastSentTimeUtc(ZDateTime updatedDateTime, IEnumerable<ZGuid> transactionPKs)
		{
			if (transactionPKs == null || !transactionPKs.Any())
			{
				return;
			}
			foreach (AccEInvoicingTransactionPivot pivot in TransactionPivots)
			{
				if (transactionPKs.Contains(pivot.AIP_ParentID))
				{
					pivot.AIP_LastSentTimeUtc = updatedDateTime;
				}
			}
		}

		public void RemovePivotAndDiscardBatchIfEmpty(AccEInvoicingTransactionPivot pivot)
		{
			if (pivot == null)
			{
				return;
			}

			pivot.AIP_AIB = ZGuid.Empty;
			TransactionPivots.Remove(pivot);
			if (TransactionPivots.Cast<AccEInvoicingTransactionPivot>().All(p => p.AIP_Status == EInvoicingPivotState.Discarded))
			{
				AIB_Status = EInvoicingBatchState.Discarded;
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AIB_BatchNumber = (new Random()).Next(1, 100);
		}
#endif
	}
}
