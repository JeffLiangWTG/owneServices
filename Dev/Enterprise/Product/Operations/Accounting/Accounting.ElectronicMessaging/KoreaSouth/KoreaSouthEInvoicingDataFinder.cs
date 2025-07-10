using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingDataFinder
	{
		public AccEInvoicingBatch FindSubmitBatch(AccEInvoicingBatch batch)
		{
			Argument.NotNull(batch, nameof(batch));
			return FindBatch(batch, EInvoicingPivotActionType.Submit);
		}

		public AccEInvoicingBatch FindStatusCheckBatch(AccEInvoicingBatch batch)
		{
			Argument.NotNull(batch, nameof(batch));
			return FindBatch(batch, EInvoicingPivotActionType.StatusCheck);
		}

		public string GetSubmitId(AccEInvoicingBatch batch)
		{
			var submitBatch = FindSubmitBatch(batch);
			var licenseCode = EncodeLicenceCode(submitBatch.Company);
			var lastPart = $"{licenseCode}{submitBatch.AIB_BatchNumber.ToString().PadLeft(32 - licenseCode.Length, '0')}";
			return $"{GetKoreaSouthRegistryNumber(submitBatch.Company)}-{submitBatch.AIB_SystemCreateTimeUtc:yyyyMMdd}-{lastPart}";

			string EncodeLicenceCode(ICompany company)
			{
				var result = new StringBuilder();
				foreach (byte charByte in company.GetLicenceCode())
				{
					var firstPos = charByte / 16;
					var secPos = charByte % 16;
					result.Append($"{firstPos:X}{secPos:X}");
				}

				return result.ToString().ToLower();
			}

			string GetKoreaSouthRegistryNumber(GlbCompany company)
			{
				return GEIMessageHelper.GetIsProductionSystem(company)
					? KoreaSouthRegistryNumber.Production
					: KoreaSouthRegistryNumber.Testing;
			}
		}

		AccEInvoicingBatch FindBatch(AccEInvoicingBatch batch, string actionType)
		{
			var actionTypes = batch.TransactionPivots.Select(x => x.AIP_ActionType).Distinct();
			if (actionTypes.Count() > 1)
			{
				ErrorReporter.ReportOnce("InvalidActionTypes", "The TransactionPivots under the batch should not have different action types. " + EventProcessorHelper.GetBatchInfo(batch)); // Developer error report
			}
			else if (actionTypes.Count() == 1 && actionTypes.First() == actionType)
			{
				return batch;
			}

			var queryPivots = batch.Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQuery(batch.TransactionPivots.Select(x => x.AIP_ParentID), actionType));
			if (queryPivots.Length > 1)
			{
				ErrorReporter.ReportOnce("InvalidTransactionPivots", "Only one AccEInvoicingTransactionPivot should be found. " + EventProcessorHelper.GetPivotsInfo(queryPivots)); // Developer error report
			}
			else if (queryPivots.Length == 1)
			{
				return queryPivots[0].Batch;
			}

			return null;
		}

		ZQuery GetPivotQuery(IEnumerable<ZGuid> transactinoPKs, string actionType)
		{
			var pivotQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType);
			pivotQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactinoPKs);
			pivotQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
			return pivotQuery;
		}
	}
}
