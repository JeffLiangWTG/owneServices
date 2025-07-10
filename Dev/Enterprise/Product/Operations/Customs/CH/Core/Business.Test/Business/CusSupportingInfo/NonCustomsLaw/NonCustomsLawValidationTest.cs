using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class NonCustomsLawValidationTest : BusinessObjectValidationTestCase
{
	public void TestTypeCode()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawTypeCodesList(Factory);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(NonCustomsLaw.CSI_CodeInfo, RefCusCodeTestHelper.InvalidNonCustomsLawTypeCode, RefCusCodeTestHelper.ValidNonCustomsLawTypeCode);
	}

	NonCustomsLaw NonCustomsLaw => nonCustomsLaw ?? (nonCustomsLaw = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().NonCustomsLaws.AddNew());
	NonCustomsLaw nonCustomsLaw;
}
