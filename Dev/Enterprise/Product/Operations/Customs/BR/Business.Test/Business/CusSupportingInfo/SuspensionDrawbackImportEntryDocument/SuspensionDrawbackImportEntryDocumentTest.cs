using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawbackImportEntryDocument))]
	public class SuspensionDrawbackImportEntryDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SuspensionDrawbackImportEntryDocument>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var suspensionDrawback = invoiceLine.SuspensionDrawbackCollection.AddNew();
			var suspensionDrawbackImportEntryDocument = suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection.AddNew();

			return suspensionDrawbackImportEntryDocument;
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<SuspensionDrawbackImportEntryDocument>();
			AssertEquals(CusSupportingInfoTypeList.Codes.SuspensionDrawbackImportEntryDocument, supporting.CSI_Type);
		}

		protected override IEnumerable<SuspensionDrawbackImportEntryDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var drawbackCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew().SuspensionDrawbackImportEntryDocumentCollection.AddNew();
			drawbackCusSupporting.CSI_ReferenceNumber = "1";
			drawbackCusSupporting.CSI_LineNo = 1;
			yield return drawbackCusSupporting;
		}
	}
}
