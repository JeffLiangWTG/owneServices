using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	public class MalaysiaEInvoiceAPICommandListTest : TestCase
	{
		public void TestGetMessageTypeAccordingPivotStatus()
		{
			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, MalaysiaEInvoiceAPICommandList.GetMessageType(Constants.EInvoicingPivotActionType.Submit));
			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, MalaysiaEInvoiceAPICommandList.GetMessageType(Constants.EInvoicingPivotActionType.StatusCheck));
			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.GetDocument, MalaysiaEInvoiceAPICommandList.GetMessageType(Constants.EInvoicingPivotActionType.DocumentAction));
			AssertEquals(MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, MalaysiaEInvoiceAPICommandList.GetMessageType(Constants.EInvoicingPivotActionType.DocumentDetail));
		}
	}
}
