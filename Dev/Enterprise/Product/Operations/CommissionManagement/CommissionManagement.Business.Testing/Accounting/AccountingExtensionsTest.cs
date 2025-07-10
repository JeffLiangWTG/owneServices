using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class AccountingExtensionsTest : CommissionCreatorTestCase
	{
		#region Tests
		public void TestIsFullAmendingCreditNote()
		{
			ErrorReporter.Clear();
			var objectCreator = new TestObjectCreator(Factory);
			var shipment1 = objectCreator.CreateShipment("S001");
			var job1 = objectCreator.CreateJob(shipment1);

			var shipment2 = objectCreator.CreateShipment("S002");
			var job2 = objectCreator.CreateJob(shipment2);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job1.PK;
			invoice.AH_PostDate = new ZDateTime(2003, 1, 1);
			objectCreator.CreateARInvoiceLineWithJobCharge(invoice, job1, TestObjectCreator.CC2, TestObjectCreator.AUD, 1.0m, "AR Line", 100m, TestObjectCreator.GST1.PK);

			Factory.Save();

			var amendingCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			amendingCreditNote.AH_JH = job1.PK;
			amendingCreditNote.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingCreditNote.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingCreditNoteLine = objectCreator.CreateARCreditNoteLine(amendingCreditNote, job1, TestObjectCreator.CC2, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line");
			amendingCreditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
			Assert("Full Amending Credit Note", amendingCreditNote.IsFullAmendingCreditNote());

			var amendingPartialCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote1.AH_JH = job1.PK;
			amendingPartialCreditNote1.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingPartialCreditNote1.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote1Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote1, job1, TestObjectCreator.CC2, 80m, TestObjectCreator.AUD, 1.0m, "Credit Line2");
			amendingPartialCreditNote1Line.AL_AT = TestObjectCreator.GST1.PK;
			Assert("First Partial Amending Credit Note", !amendingPartialCreditNote1.IsFullAmendingCreditNote());

			var amendingPartialCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote2.AH_JH = job1.PK;
			amendingPartialCreditNote2.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingPartialCreditNote2.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote2Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote2, job1, TestObjectCreator.CC2, 100m, TestObjectCreator.CNY, 1.0m, "Credit Line3");
			amendingPartialCreditNote2Line.AL_AT = TestObjectCreator.GST1.PK;
			Assert("Second Partial Amending Credit Note", !amendingPartialCreditNote2.IsFullAmendingCreditNote());

			var amendingPartialCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote3.AH_JH = job1.PK;
			amendingPartialCreditNote3.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingPartialCreditNote3.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote3Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote3, job1, TestObjectCreator.CC3, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line4");
			amendingPartialCreditNote3Line.AL_AT = TestObjectCreator.GST1.PK;
			Assert("Third Partial Amending Credit Note", !amendingPartialCreditNote3.IsFullAmendingCreditNote());

			var amendingPartialCreditNote4 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote4.AH_JH = job1.PK;
			amendingPartialCreditNote4.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingPartialCreditNote4.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote4Line = objectCreator.CreateARCreditNoteLine(amendingCreditNote, job1, TestObjectCreator.CC2, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line5");
			amendingPartialCreditNote4Line.AL_AT = TestObjectCreator.GST1.PK;
			amendingPartialCreditNote4Line.AL_OH = TestObjectCreator.ABIGAS.PK;
			Assert("Fourth Amending Credit Note", !amendingPartialCreditNote4.IsFullAmendingCreditNote());

			var amendingPartialCreditNote5 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote5.AH_JH = job2.PK;
			amendingPartialCreditNote5.AH_PostDate = new ZDateTime(2002, 1, 1);
			amendingPartialCreditNote5.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote5Line = objectCreator.CreateARCreditNoteLine(amendingCreditNote, job1, TestObjectCreator.CC2, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line6");
			amendingPartialCreditNote5Line.AL_AT = TestObjectCreator.GST1.PK;
			amendingPartialCreditNote5Line.AL_JH = job2.PK;
			Assert("Fifth Amending Credit Note", !amendingPartialCreditNote5.IsFullAmendingCreditNote());

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_JH = job1.PK;
			creditNote.AH_PostDate = new ZDateTime(2003, 1, 1);
			Assert("Not an Amending Credit Note", !creditNote.IsFullAmendingCreditNote());

			var creditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote2.AH_JH = job1.PK;
			creditNote2.AH_PostDate = new ZDateTime(2003, 1, 1);
			creditNote2.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			creditNote2.AH_OriginalInvoiceDate = new ZDate(2002, 12, 15);
			Assert("Bad AH_TransactionBelongsToGroup Guid", !creditNote2.IsFullAmendingCreditNote());
			AssertEquals("Total Error Count", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		#endregion
	}
}
