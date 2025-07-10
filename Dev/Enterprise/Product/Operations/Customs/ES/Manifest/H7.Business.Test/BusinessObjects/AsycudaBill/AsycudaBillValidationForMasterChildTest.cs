using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForMasterChild))]
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_BillNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			masterBill.Validation.ValidateABL_BillNumber();

			AssertHasMessageError(masterBill.ABL_BillNumberInfo, "You have not entered a Bill Number or a Entry Line Number.");

			header.EntryLineNumber = "12345";
			masterBill.ABL_BillNumber = "12345";
			AssertHasMessageError(masterBill.ABL_BillNumberInfo, "You have entered both a Master Bill Number and a Entry Line Number. Only one must be entered.");

			header.EntryLineNumber = string.Empty;
			masterBill.Validation.ValidateABL_BillNumber();
			AssertNoMessageErrors(masterBill.ABL_BillNumberInfo);
		}
	}
}
