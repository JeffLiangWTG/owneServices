using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AVSQueryResultCollection))]
	sealed class AVSQueryResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AVSQueryResultCollection>
	{
		public void TestLoad()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals("AVSQueryResults.Count", 0, invoiceLine.AVSQueryResults.Count);

			invoiceLine.Notes.AddNew(false, PredefinedNoteTypes.Instance.AIRSValidationResults.Description, "text");
			invoiceLine.Factory.Save();
			AssertEquals("AVSQueryResults.Count", 1, invoiceLine.AVSQueryResults.Count);

			invoiceLine.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsManualStatus.Description, "text");
			invoiceLine.Factory.Save();
			AssertEquals("AVSQueryResults.Count", 1, invoiceLine.AVSQueryResults.Count);
		}

		protected override AVSQueryResultCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			return new AVSQueryResultCollection(invoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var note = invoiceLine.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AIRSValidationResults.Description;
			return new AVSQueryResult(note);
		}
	}
}
