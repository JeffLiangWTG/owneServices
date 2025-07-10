using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionValidation))]
class RestrictionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code()
	{
		RefCusCodeTestHelper.CreateRestrictionCodeLists(Factory);

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Restriction.CSI_CodeInfo, RefCusCodeTestHelper.InvalidRestrictionCode, RefCusCodeTestHelper.RestrictionCode1);
	}

	public void TestCheckCSI_Description() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRestrictionCodeLists(Factory);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCode2;
		ValidationTestHelper.AssertFieldIsNotMandatory(Restriction.CSI_DescriptionInfo);
		ValidationTestHelper.AssertInvalidCodeMessageError(Restriction.CSI_DescriptionInfo, RefCusCodeTestHelper.RestrictionExceptionCodeFor1, RefCusCodeTestHelper.RestrictionExceptionCodeFor1And2);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithoutException;
		Restriction.CSI_Description = ZString.Empty;
		AssertNoNotifications("Restriction without exceptions", Restriction.CSI_DescriptionInfo);
		Restriction.CSI_Description = "X";
		AssertHasMessageError("Restriction without exceptions", Restriction.CSI_DescriptionInfo, ListValidation.InvalidCodeMessageError);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= InvoiceHeader.InvoiceLines.AddNew();
	JobComInvoiceLine jobComInvoiceLine;

	Restriction Restriction => restriction ??= JobComInvoiceLine.Restrictions.AddNew();
	Restriction restriction;
}
