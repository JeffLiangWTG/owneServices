using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestHouseBill()
		{
			var parent = Factory.New<Bill>();
			AssertEquals(parent.Validation.Bill, parent);
		}

		public void TestCU_HBSplitDecInd()
		{
			var declaration = CreateDeclaration();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.Validation.ValidateCU_HBSplitDecInd();
			AssertHasMessageErrorContaining(masterBill.CU_HBSplitDecIndInfo, MandatoryValidation.YouHaveNotEntered);

			masterBill.CU_HBSplitDecInd = "X";
			AssertHasMessageErrorContaining(masterBill.CU_HBSplitDecIndInfo, ListValidation.InvalidCodeMessageError);

			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			AssertNoMessageErrors(masterBill.CU_HBSplitDecIndInfo);

			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			AssertNoMessageErrors(masterBill.CU_HBSplitDecIndInfo);
		}

		public void TestCheckCU_HBSplitDecReasonCode()
		{
			var declaration = CreateDeclaration();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			masterBill.Validation.ValidateCU_HBSplitDecReasonCode();
			AssertHasMessageErrorContaining(masterBill.CU_HBSplitDecReasonCodeInfo, "If House Bill Split Declaration Indicator is 'Y', you must enter House Bill Split Declaration Reason Code");

			masterBill.CU_HBSplitDecReasonCode = HouseBillSplitDeclarationReasonCodeList.Codes.A;
			AssertNoMessageErrors(masterBill.CU_HBSplitDecReasonCodeInfo);

			masterBill.CU_HBSplitDecReasonCode = "X";
			AssertHasMessageErrorContaining(masterBill.CU_HBSplitDecReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_HBSplitDecReasonCode = HouseBillSplitDeclarationReasonCodeList.Codes.A;
			AssertHasMessageErrorContaining(masterBill.CU_HBSplitDecReasonCodeInfo, "If House Bill Split Declaration Indicator is 'N', don't enter House Bill Split Declaration Reason Code");
		}

		public void TestCheckHBSplitDecReasonRemark()
		{
			var declaration = CreateDeclaration();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;

			masterBill.CU_HBSplitDecReasonCode = HouseBillSplitDeclarationReasonCodeList.Codes.A;
			masterBill.Validation.ValidateHBSplitDecReasonRemark();
			AssertNoMessageErrors(masterBill.HBSplitDecReasonRemarkInfo);

			masterBill.CU_HBSplitDecReasonCode = HouseBillSplitDeclarationReasonCodeList.Codes.Z;
			masterBill.Validation.ValidateHBSplitDecReasonRemark();
			AssertHasMessageErrorContaining(masterBill.HBSplitDecReasonRemarkInfo, "If House Bill Split Declaration Reason Code is 'Z', you must enter House Bill Split Declaration Reason Description");

			masterBill.HBSplitDecReasonRemark = "테스트";
			AssertNoMessageErrors(masterBill.HBSplitDecReasonRemarkInfo);
			AssertNoErrors(masterBill.HBSplitDecReasonRemarkInfo);

			masterBill.CU_HBSplitDecReasonCode = HouseBillSplitDeclarationReasonCodeList.Codes.A;
			masterBill.Validation.ValidateHBSplitDecReasonRemark();
			AssertHasMessageErrorContaining(masterBill.HBSplitDecReasonRemarkInfo, "If House Bill Split Declaration Reason Code is  not 'Z', don't enter House Bill Split Declaration Reason Description");
		}

		JobDeclaration CreateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			return declaration;
		}
	}
}
