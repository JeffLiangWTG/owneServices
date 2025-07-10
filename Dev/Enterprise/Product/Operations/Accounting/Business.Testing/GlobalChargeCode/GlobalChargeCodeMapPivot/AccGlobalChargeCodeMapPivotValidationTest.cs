namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;

	internal abstract class AccGlobalChargeCodeMapPivotValidationTest : BusinessObjectValidationTestCase
	{
		protected abstract BusinessObjectCollection GetGlobalChargeCodePivotCollection
		{
			get;
		}

		public void TestCheckYP_TYPE()
		{
			BusinessObjectCollection globalChargeCodePivotCollection = GetGlobalChargeCodePivotCollection;
			GlobalChargeCodeMapPivot pivot1 = (GlobalChargeCodeMapPivot)globalChargeCodePivotCollection.AddNew();
			pivot1.Validation.ValidateAll();
			AssertHasError(pivot1.YP_TYPEInfo, "Please enter a Ledger.");
			pivot1.YP_TYPE = "XX";
			AssertHasError(pivot1.YP_TYPEInfo, "Enter a valid Ledger.");
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertNoErrors(pivot1.YP_TYPEInfo);
			GlobalChargeCodeMapPivot pivot2 = (GlobalChargeCodeMapPivot)globalChargeCodePivotCollection.AddNew();
			pivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertHasError(pivot2.YP_TYPEInfo, "You can only create a single AP Charge Code per mapping.");
			pivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AssertNoErrors(pivot2.YP_TYPEInfo);
		}

		public void TestCheckYP_AC()
		{
			BusinessObjectCollection globalChargeCodePivotCollection = GetGlobalChargeCodePivotCollection;
			GlobalChargeCodeMapPivot pivot1 = (GlobalChargeCodeMapPivot)globalChargeCodePivotCollection.AddNew();
			pivot1.YP_AC = ZGuid.NewZGuid();
			pivot1.YP_YG = ZGuid.NewZGuid();
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			GlobalChargeCodeMapPivot pivot2 = (GlobalChargeCodeMapPivot)globalChargeCodePivotCollection.AddNew();
			pivot2.YP_YG = pivot1.YP_YG;
			pivot2.YP_TYPE = pivot1.YP_TYPE;
			pivot2.YP_AC = pivot1.YP_AC;
			AssertHasError(pivot2.YP_ACInfo, "This Charge Code and Type is already mapped to this Global Code");
			pivot2.YP_AC = ZGuid.NewZGuid();
			AssertNoError(pivot2.YP_ACInfo, "This Charge Code and Type is already mapped to this Global Code");
		}
	}
}