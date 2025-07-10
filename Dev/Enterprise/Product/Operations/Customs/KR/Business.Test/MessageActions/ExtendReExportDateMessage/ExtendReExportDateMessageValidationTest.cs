using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class ExtendReExportDateMessageValidationTest : BusinessObjectValidationTestCase
	{
		public void Test_D72MessageSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var parent = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestWithMessageErrorIfNotEnteredCaseInReasonDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var parent = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
			parent.ReasonDescription = "";
			AssertHasMessageErrorContaining(parent.ReasonDescriptionInfo, "You have not entered a Reason Description.");

			parent.ReasonDescription = "재수출연장기간신청사유";
			AssertNoMessageErrorContaining(parent.ReasonDescriptionInfo, "You have not entered a Reason Description.");
			Assert("Korean characters should be allowed", !parent.HasErrors);
		}

		public void TestWithMessageErrorIfNotEnteredCaseInNewReExportDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var parent = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
			parent.NewReExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(parent.NewReExportDateInfo, "You have not entered a New Re-Export Scheduled Date.");

			parent.NewReExportDate = new ZDateTime(2021, 01, 01);
			AssertNoMessageErrorContaining(parent.NewReExportDateInfo, "You have not entered a New Re-Export Scheduled Date.");
		}

		[TestDate(2024, 1, 1)]
		public void TestNewReExportDateIsValidZDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var parent = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
			parent.NewReExportDate = ZDateTime.Invalid;
			AssertHasErrorContaining(parent.NewReExportDateInfo, "Enter a valid New Re-Export Scheduled Date.");

			parent.NewReExportDate = new ZDateTime(2013, 1, 1);
			AssertHasErrorContaining(parent.NewReExportDateInfo, "The date '01-Jan-2013' is more than 10 years old and thus is not valid");

			parent.NewReExportDate = new ZDateTime(2024, 1, 1);
			AssertNoErrors(parent.NewReExportDateInfo);
		}

		public void TestShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			CreateEntryAndInvoiceLineData(1, 1, ZDateTime.Empty);
			CreateEntryAndInvoiceLineData(2, 1, new ZDateTime(2025, 01, 01));
			CreateEntryAndInvoiceLineData(3, 1, new ZDateTime(2025, 01, 02));
			CreateEntryAndInvoiceLineData(4, 1, new ZDateTime(2025, 01, 02));

			var collection = new ExtendReExportDateMessageSendingObjectCollection(declaration);
			AssertEquals(3, collection.Count);
			SetShouldSendTrue(collection);
			AssertHasErrorContaining(collection[0].ShouldSendInfo, "You cannot send a D72 message as there is no scheduled export date.");
			AssertHasMessageErrorContaining(collection[1].ShouldSendInfo, "There are entry lines (002) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");
			AssertHasMessageErrorContaining(collection[2].ShouldSendInfo, "There are entry lines (003, 004) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");

			var entryNum5FN_EntryLineReference1 = Create5FNEntryNum("1", CustomsMessageStatusTypeList.Codes.OriginalSent);
			var entryNum5FN_EntryLineReference2 = Create5FNEntryNum("2", CustomsMessageStatusTypeList.Codes.OriginalSent);
			var entryNum5FN_EntryLineReference3 = Create5FNEntryNum("3", CustomsMessageStatusTypeList.Codes.OriginalSent);
			var entryNum5FN_EntryLineReference4 = Create5FNEntryNum("4", CustomsMessageStatusTypeList.Codes.OriginalSent);
			collection = new ExtendReExportDateMessageSendingObjectCollection(declaration);
			AssertEquals(3, new ExtendReExportDateMessageSendingObjectCollection(declaration).Count);
			SetShouldSendTrue(collection);
			AssertHasErrorContaining(collection[0].ShouldSendInfo, "You cannot send a D72 message as there is no scheduled export date.");
			AssertHasMessageErrorContaining(collection[1].ShouldSendInfo, "There are entry lines (002) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");
			AssertHasMessageErrorContaining(collection[2].ShouldSendInfo, "There are entry lines (003, 004) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");

			entryNum5FN_EntryLineReference2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum5FN_EntryLineReference3.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			collection = new ExtendReExportDateMessageSendingObjectCollection(declaration);
			AssertEquals(3, new ExtendReExportDateMessageSendingObjectCollection(declaration).Count);
			SetShouldSendTrue(collection);
			AssertHasErrorContaining(collection[0].ShouldSendInfo, "You cannot send a D72 message as there is no scheduled export date.");
			AssertNoMessageErrorContaining(collection[1].ShouldSendInfo, "There are entry lines (002) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");
			AssertHasMessageErrorContaining(collection[2].ShouldSendInfo, "There are entry lines (004) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check.");

			JobComInvoiceLine CreateEntryAndInvoiceLineData(ZShort entryLineNumber, ZShort invoiceLineNumber, ZDateTime scheduledReExportDate)
			{
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = entryLineNumber;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_LineNo = invoiceLineNumber;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_ScheduledReExportDate = scheduledReExportDate;

				return invoiceLine;
			}

			CusEntryNumber Create5FNEntryNum(ZString entryLineReference, ZString entryStatus)
			{
				var entryNum5FN = entry.EntryNumbers.AddNew();
				entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
				entryNum5FN.CE_EntryLineReference = entryLineReference;
				entryNum5FN.CE_EntryStatus = entryStatus;

				return entryNum5FN;
			}

			void SetShouldSendTrue(ExtendReExportDateMessageSendingObjectCollection collection)
			{
				collection[0].ShouldSend = true;
				collection[1].ShouldSend = true;
				collection[2].ShouldSend = true;
			}
		}
	}
}
