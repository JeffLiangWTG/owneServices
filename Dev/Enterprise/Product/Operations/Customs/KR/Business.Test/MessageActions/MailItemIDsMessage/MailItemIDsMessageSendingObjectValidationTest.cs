using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MailItemIDsMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = CreateInvoiceData();
			var invoice2 = CreateInvoiceData();
			AssertEquals(0, invoice1.Parcels.Count);
			AssertEquals(0, invoice2.Parcels.Count);

			var messageSendingObject = new MailItemIDsMessageSendingObject(entry);
			messageSendingObject.ShouldSend = true;
			AssertHasErrorContaining(messageSendingObject.ShouldSendInfo, "The invoice for this entry requires at least one piece of parcel information.");

			var parcel = invoice1.Parcels.AddNew();
			parcel.CSI_ReferenceNumber = "12345678901234";
			parcel.CSI_ReferenceNumber2 = "Parcel No.1";
			parcel.CSI_Code = "A";
			messageSendingObject = new MailItemIDsMessageSendingObject(entry);
			messageSendingObject.ShouldSend = true;
			AssertNoErrorContaining(messageSendingObject.ShouldSendInfo, "The invoice for this entry requires at least one piece of parcel information.");

			JobComInvoiceHeader CreateInvoiceData()
			{
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				return invoice;
			}
		}
	}
}
