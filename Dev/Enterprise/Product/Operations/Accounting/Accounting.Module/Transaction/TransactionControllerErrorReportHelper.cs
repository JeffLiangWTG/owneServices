using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	internal static class TransactionControllerErrorReportHelper
	{
		public static void ReportInvalidDataSource(ZController controller, ControllerID correctId, object dataSource)
		{
			var transaction = dataSource as InvoicingBase;
			var messageBuilder = new ZStringBuilder();

			messageBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"Message: ZController {0} is handling an invalid bizO {1}. The correct controller ID should be {2}", controller.ID.Name, dataSource == null ? "NULL" : dataSource.GetType().ToString(), correctId));
			if (transaction != null)
			{
				messageBuilder.Append(string.Format(CultureInfo.InvariantCulture, "AH_TransactionType: {0}", transaction.AH_TransactionType));
				messageBuilder.Append(string.Format(CultureInfo.InvariantCulture, "AH_Ledger: {0}", transaction.AH_Ledger));
				messageBuilder.Append((NoResString)"Details:");
				messageBuilder.Append(transaction.GetTransactionHeaderWithLinesInfo());
			}
			messageBuilder.AppendLine();

			var errorMessage = messageBuilder.ToStringWithNewLineBetweenAppends();
			ErrorReporter.ReportOnce(controller.ID.Name + "_2022170D-A4EB-46e8-A863-4EF64435BE1A", errorMessage);
		}
	}

	internal static class GetValidControllerIdHelper
	{
		public static bool IsToSkipModuleId(ControllerID controllerId)
		{
			return new List<ControllerID>
			{
					ControllerIDs.APInvoiceLinkedToApproval, ControllerIDs.APInvoiceNewForApproval,
					ControllerIDs.APCreditNoteLinkedToApproval, ControllerIDs.APCreditNoteNewForApproval
			}
			.Contains(controllerId);
		}
	}
}
