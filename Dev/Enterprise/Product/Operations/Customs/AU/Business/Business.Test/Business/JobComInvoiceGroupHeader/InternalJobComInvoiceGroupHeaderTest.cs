using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing;

public class InternalJobComInvoiceGroupHeaderTest : TestCaseWithFactory
{
	public void TestGetEXDValidationObject()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
		Assert("EXD JobValidation object", groupHeader.JobValidation is EXDJobComInvoiceGroupHeaderValidation);
	}
}
