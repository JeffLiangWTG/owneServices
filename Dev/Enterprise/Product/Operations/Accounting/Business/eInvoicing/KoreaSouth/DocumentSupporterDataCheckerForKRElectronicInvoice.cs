using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business
{
	public static class DocumentSupporterDataCheckerForKRElectronicInvoice
	{
		internal static DocumentSupporterDataState CheckerForKRElectronicInvoiceDocument(InvoicingBase invoice)
		{
			var result = new DocumentSupporterDataState();
			if (!invoice.IsEligibleToCreateEInvoicingTransactionPivot)
			{
				var errorMessage = Res.GetString("bef96a41-99f6-4b3b-b637-c93a44be02b3", "The following transaction(s) cannot be printed.\r\nTransaction {0} cannot be printed because it is not an Electronic Invoice Document.", invoice.AH_TransactionNum);
				result = new DocumentSupporterDataState(false, errorMessage);
			}
			else if (invoice.IsAwaitingApprovalFromGovt)
			{
				var errorMessage = Res.GetString("c78557bd-fb7f-4c95-bdbc-9a43e3754418", "The Electronic Invoice Document cannot be printed as it has not been approved by the National Tax Service.");
				result = new DocumentSupporterDataState(false, errorMessage);
			}
			else if (invoice.AH_InvoiceAmount.Truncate().ToString().Length > 12)
			{
				var errorMessage = Res.GetString("cf46851c-27d9-4980-9445-106984adc9be", "The Electronic Invoice Document cannot be printed as the total invoice amount exceeded the maximum invoice amount supported by this document layout.");
				result = new DocumentSupporterDataState(false, errorMessage);
			}
			return result;
		}
	}
}
