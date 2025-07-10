using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCkeckCSI_Code()
		{
			supportingDocument.CSI_Code = "Z";
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			supportingDocument.CSI_Code = "01";
			AssertNoMessageErrors(supportingDocument.CSI_CodeInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoice = factory.New<JobDeclaration>().Invoices.AddNew();
			var supportingDocumentCollection = new SupportingDocumentCollection(invoice);
			yield return supportingDocumentCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (SupportingDocument)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDoc;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<SupportingDocument>();
			AssertEquals(typeof(SupportingDocumentValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			AssertEquals(invoiceHeader, supportingDocument.Parent);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var supportingDocumentCollection = new SupportingDocumentCollection(invoiceLine);
			supportingDocument = supportingDocumentCollection.AddNew();

			AssertEquals(invoiceLine, supportingDocument.Parent);
		}

		protected override void SetUp()
		{
			invoiceHeader = Factory.New<JobDeclaration>().Invoices.AddNew();
			var supportingDocumentCollection = new SupportingDocumentCollection(invoiceHeader);
			supportingDocument = supportingDocumentCollection.AddNew();
		}
		SupportingDocument supportingDocument;
		JobComInvoiceHeader invoiceHeader;
	}
}
