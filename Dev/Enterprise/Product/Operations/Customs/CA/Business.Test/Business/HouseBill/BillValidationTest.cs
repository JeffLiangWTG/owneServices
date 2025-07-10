using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class BillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestCheckCU_BillNum()
		{
			var parent = Factory.New<Bill>();
			parent.CU_BillNum = "123ABC";
			AssertNoWarning(parent.CU_BillNumInfo, "The Bill Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			parent.CU_BillNum = "123ABC– ";
			AssertHasWarning(parent.CU_BillNumInfo, "The Bill Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Validation.Bill, parent);
		}

		public void TestCheckCU_PackTypeIsAValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			string message = "Please enter a valid Manifest UQ code. The code you have selected is not in the Manifest UQ codes List.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var bill = declaration.Bills.AddNew();
			bill.CU_PackType = "XXX";

			AssertHasWarning(bill.CU_PackTypeInfo, message);
			AssertNoMessageError(bill.CU_PackTypeInfo, message);

			bill.CU_PackType = "AAA";
			AssertNoWarnings(bill.CU_PackTypeInfo);
		}
	}
}
