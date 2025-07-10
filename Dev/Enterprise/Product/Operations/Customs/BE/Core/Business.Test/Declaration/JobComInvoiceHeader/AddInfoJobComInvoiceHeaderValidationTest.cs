using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_TransportChargesMethodOfPayment_ListValidation_Export()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(GetInvoiceHeader(JobMessageTypeList.Codes.Export).ZG_TransportChargesMethodOfPaymentInfo, "X", ExportMethodOfPaymentList.Codes.A);
	}

	JobComInvoiceHeader GetInvoiceHeader(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		return declaration.Invoices.AddNew();
	}
}
