using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "XXX", "DUE");

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "XXX", "RE");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			Assert(previousDocument.CSI_ReferenceNumberInfo.HasMessageErrors());
			previousDocument.CSI_ReferenceNumber = "TEST123";
			Assert(!previousDocument.CSI_ReferenceNumberInfo.HasMessageErrors());
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
		PreviousDocument previousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			previousDocument = invoiceLine.PreviousDocuments.AddNew();
		}
	}
}
