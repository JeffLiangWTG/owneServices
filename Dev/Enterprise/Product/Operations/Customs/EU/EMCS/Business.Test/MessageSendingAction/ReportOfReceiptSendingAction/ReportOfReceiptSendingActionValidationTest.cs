using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class ReportOfReceiptSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateArrivalDate_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendReportOfReceipt.ArrivalDateInfo);
		}

		public void TestValidateArrivalDate_Future()
		{
			const string errorMessage = "Arrival Date must not be in the future.";
			CombineAssertions(() =>
			{
				sendReportOfReceipt.ArrivalDate = ZDateTime.Now.AddDays(3);
				AssertHasError("Future", sendReportOfReceipt.ArrivalDateInfo, errorMessage);
				sendReportOfReceipt.ArrivalDate = ZDateTime.Now;
				AssertNoError("Now", sendReportOfReceipt.ArrivalDateInfo, errorMessage);
			});
		}

		public void TestValidateArrivalDate_BeforeDispatchTime()
		{
			const string errorMessage = "Arrival Date must not be prior to the Dispatch Time.";
			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.Now.AddDays(-2);
				sendReportOfReceipt.ArrivalDate = ZDateTime.Now.AddDays(-3);
				AssertHasError("Later", sendReportOfReceipt.ArrivalDateInfo, errorMessage);
				sendReportOfReceipt.ArrivalDate = ZDateTime.Now.AddDays(-1);
				AssertNoError("Prior", sendReportOfReceipt.ArrivalDateInfo, errorMessage);
			});
		}

		public void TestValidateReceiptResult_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendReportOfReceipt.ReceiptResultInfo);
		}

		public void TestValidateReceiptResult_List()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(sendReportOfReceipt.ReceiptResultInfo, "!", EMCSReceiptResultList.Codes.ReceiptPartiallyRefused);
		}

		public void TestValidateReceiptResult_RefusedQuantity()
		{
			const string messageError = "You have not entered a Refused Quantity on an Invoice Line.";
			CombineAssertions(() =>
			{
				sendReportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptPartiallyRefused;
				AssertHasMessageError("No InvoiceLines", sendReportOfReceipt.ReceiptResultInfo, messageError);
				var goodsItem = declaration.InvoiceLines.AddNew();
				AssertHasMessageError("No Outturn", sendReportOfReceipt.ReceiptResultInfo, messageError);
				goodsItem.Outturn.C5_RejectedQuantity = 1;
				AssertHasMessageError("Rejected Outturn", sendReportOfReceipt.ReceiptResultInfo, messageError);
			});
		}

		public void TestValidateReceiptResult_ReasonCode()
		{
			const string messageError = "Please enter a Reason Code on at least one line.";
			var propertyInfo = sendReportOfReceipt.ReceiptResultInfo;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var reason = invoiceLine.Outturn.ReportOfReceiptReasons.AddNew();

			CombineAssertions(() =>
			{
				AssertNoMessageError("Empty ReceiptResult", propertyInfo, messageError);

				sendReportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptAcceptedAndSatisfactory;
				AssertNoMessageError("ReceiptResult = 1", propertyInfo, messageError);

				AssertReasonCodeExists(EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory);
				AssertReasonCodeExists(EMCSReceiptResultList.Codes.ReceiptRefused);
				AssertReasonCodeExists(EMCSReceiptResultList.Codes.ReceiptPartiallyRefused);
			});

			void AssertReasonCodeExists(string receiptResult)
			{
				reason.CY_Code = ZString.Empty;
				sendReportOfReceipt.ReceiptResult = receiptResult;
				AssertHasMessageError($"ReceiptResult = {receiptResult}, without reason Code", propertyInfo, messageError);

				reason.CY_Code = "0";
				sendReportOfReceipt.Validation.ValidateReceiptResult();
				AssertNoMessageError($"ReceiptResult = {receiptResult}, with reason Code", propertyInfo, messageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			sendReportOfReceipt = new ReportOfReceiptSendingAction(declaration);
		}
		EMCSJobDeclaration declaration;
		ReportOfReceiptSendingAction sendReportOfReceipt;
	}
}
