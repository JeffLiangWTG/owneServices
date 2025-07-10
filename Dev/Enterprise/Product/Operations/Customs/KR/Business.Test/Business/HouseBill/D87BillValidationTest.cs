using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class D87BillValidationTest : CusDecHouseBillValidationTest
	{
		public void TestNoMessageErrorsForUnusedFieldsOfD87()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;

			var bill = declaration.Bills.AddNew();
			bill.CU_HBSplitDecInd = "Y";

			bill.RunPreSaveValidation();
			Assert(!bill.HasMessageErrors);
		}
	}
}
