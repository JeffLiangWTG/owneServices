using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
{
	public void TestCheckABL_E_ARV_IsNotMandatory()
	{
		var bill = Header.MasterBill;
		ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_ARVInfo);
	}

	public void TestCheckABL_E_DEP_IsNotMandatory()
	{
		var bill = Header.MasterBill;
		ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo);
	}
	public void TestCheckABL_BillIssueDate() => CombineAssertions(() =>
	{
		NUnit.Framework.Assert.That(Header.AMA_MasterBillIssueDate.IsEmpty, NUnit.Framework.Is.EqualTo(true), "Pre-Condition: Bill Issue Date is Empty.");
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.AMA_MasterBillIssueDateInfo);
	});

	public void TestCheckABL_BillIssueDate_InvalidValue()
	{
		Header.AMA_MasterBillIssueDate = ZDate.Today;
		AssertNoMessageErrors(Header.AMA_MasterBillIssueDateInfo);

		Header.AMA_MasterBillIssueDate = ZDate.Today.AddDays(1);
		AssertHasWarningContaining(Header.AMA_MasterBillIssueDateInfo, "The Issue Date cannot be in the future.");
	}

	AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
	AsycudaManifestHeader header;
}
