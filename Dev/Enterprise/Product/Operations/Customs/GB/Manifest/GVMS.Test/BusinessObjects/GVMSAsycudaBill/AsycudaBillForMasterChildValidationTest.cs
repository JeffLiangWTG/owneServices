using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class AsycudaBillForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_E_DEP()
		{
			bill.ABL_E_DEP = ZDateTime.Empty;
			bill.Validation.ValidateABL_E_DEP();
			AssertHasMessageError(bill.ABL_E_DEPInfo, "You have not entered a value.");

			bill.ABL_E_DEP = ZDateTime.Today;
			AssertNoMessageError(bill.ABL_E_DEPInfo, "You have not entered a value.");
		}

		public void TestCheckABL_E_ARV()
		{
			bill.ABL_E_ARV = ZDateTime.Empty;
			bill.Validation.ValidateABL_E_ARV();
			AssertNoNotifications(bill.ABL_E_ARVInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
