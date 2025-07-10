using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatchTypesTest : TestCase
	{
		public void TestGetDocumentEmailSubjectContainsString()
		{
			UPEPrintBatchTypes batchTypes = new UPEPrintBatchTypes();
			Assert("Tax Invoice" == batchTypes.GetDocumentEmailSubjectContainsString(UPEPrintBatchTypes.Codes.TaxInvoice));
			Assert("Alternate Broker Split Notification" == batchTypes.GetDocumentEmailSubjectContainsString(UPEPrintBatchTypes.Codes.AlternateBrokerSplitNotification));
			Assert("Letter of Authority" == batchTypes.GetDocumentEmailSubjectContainsString(UPEPrintBatchTypes.Codes.UPSLetterOfAuthority));
			Assert("Customer Notification" == batchTypes.GetDocumentEmailSubjectContainsString(UPEPrintBatchTypes.Codes.ShipmentHeldLetter));
		}
	}
}
