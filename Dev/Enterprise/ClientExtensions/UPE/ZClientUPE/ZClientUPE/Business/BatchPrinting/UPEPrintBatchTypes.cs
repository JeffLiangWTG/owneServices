using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatchTypes : AutoUPEPrintBatchTypes
	{
		public ZString GetDocumentEmailSubjectContainsString(ZString batchType)
		{
			ZString result = GetDescriptionFromCode(batchType);
			if (batchType == UPEPrintBatchTypes.Codes.TaxInvoice)
			{
				result = "Tax Invoice";
			}
			else if (batchType == UPEPrintBatchTypes.Codes.ShipmentHeldLetter)
			{
				result = "Customer Notification";
			}
			else if (batchType == UPEPrintBatchTypes.Codes.AlternateBrokerSplitNotification)
			{
				result = "Alternate Broker Split Notification";
			}
			else if (batchType == UPEPrintBatchTypes.Codes.UPSLetterOfAuthority)
			{
				result = "Letter of Authority";
			}
			return result;
		}
	}
}
