using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonProcedureWrapperTest : WrapperHelperTest<ImportH1CommonProcedureWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown("Constructor Throws Exception if invoiceLine is null", typeof(ArgumentNullException),
		"Value cannot be null.\r\nParameter name: invoiceLine", () => GetWrapper(null));
	}

	public void TestRequestedCPC()
	{
		invoiceLine.JI_Procedure = "1049123";
		AssertEquals("Expected filled RequestedCPC", "10", wrapper.RequestedCPC);
	}

	public void TestPreviousCPC()
	{
		invoiceLine.JI_Procedure = "1049123";
		AssertEquals("Expected filled PreviousCPC", "49", wrapper.PreviousCPC);
	}

	protected override void SetUp()
	{
		base.SetUp();

		invoiceLine = Factory.New<JobComInvoiceLine>();
		wrapper = GetWrapper(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	ImportH1CommonProcedureWrapper wrapper;

	ImportH1CommonProcedureWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new ImportH1CommonProcedureWrapper(invoiceLine);

	protected override ImportH1CommonProcedureWrapper GetProvider() => wrapper;
}
