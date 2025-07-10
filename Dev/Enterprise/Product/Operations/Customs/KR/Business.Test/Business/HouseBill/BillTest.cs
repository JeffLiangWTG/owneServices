using CargoWise.Types;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class BillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var bill = Factory.New<Bill>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(bill, "KRHouseBill");
		}

		public void TestCU_HBSplitDecInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();

			AssertEquals(ZString.Empty, bill.CU_HBSplitDecInd);
			AssertEquals(false, bill.CU_HBSplitDecIndInfo.ReadOnly);

			var cargoManagementNum1 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum1.CY_Data = "TEST1";

			AssertEquals(ZString.Empty, bill.CU_HBSplitDecInd);
			AssertEquals(false, bill.CU_HBSplitDecIndInfo.ReadOnly);

			var cargoManagementNum2 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum2.CY_Data = "TEST2";

			AssertEquals("Y", bill.CU_HBSplitDecInd);
			AssertEquals(true, bill.CU_HBSplitDecIndInfo.ReadOnly);
		}

		public void TestCargoManagementNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CargoManagementNumber = "TEST1";

			AssertEquals(1, bill.CargoManagementNumbers.Count);
			AssertEquals("TEST1", bill.CargoManagementNumber);

			var cargoManagementNum2 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum2.CY_Data = "TEST2";

			AssertEquals(2, bill.CargoManagementNumbers.Count);
			AssertEquals("TEST1", bill.CargoManagementNumber);

			bill.CargoManagementNumbers.Remove(bill.CargoManagementNumbers.First());
			var cargoManagementNum3 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum3.CY_Data = "TEST3";

			AssertEquals(2, bill.CargoManagementNumbers.Count);
			AssertEquals("TEST2", bill.CargoManagementNumber);
		}

		public void TestHBSplitDecReasonRemark()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.HBSplitDecReasonRemark = "테스트";
			var notes = bill.Notes.FindByDescription(PredefinedNoteTypes.Instance.AdditionalInformation.Description);
			AssertEquals("Note exists", 1, notes.Length);
			AssertEquals("Note text is not empty", "테스트", notes[0].ST_NoteText);
			notes[0].ST_NoteText = "테스트2";
			AssertEquals("Updated", "테스트2", bill.HBSplitDecReasonRemark);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseHouseBill to include a decider for this class", Factory.New(typeof(Customs.Business.Bill)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
