using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusDecHouseBillValidation))]
	sealed class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestCheckBillCount()
		{
			var message = "There can only be at most 1 master bill, 5 house bills. The extra bills may be discarded when sending to customs.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			for (var i = 0; i < 5; i++)
			{
				var bill = declaration.Bills.AddNew();
				bill.CU_BillType = BillTypeList.Codes.HouseBill;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError(bill, message);
			}

			var redundantBill = declaration.Bills.AddNew();
			redundantBill.CU_BillType = BillTypeList.Codes.HouseBill;
			redundantBill.Validation.ValidateAll();
			AssertHasRowMessageError(redundantBill, message);

			redundantBill.CU_BillType = BillTypeList.Codes.MasterBill;
			redundantBill.Validation.ValidateAll();
			AssertNoRowMessageError(redundantBill, message);

			var redundantBill2 = declaration.Bills.AddNew();
			redundantBill2.Validation.ValidateAll();
			AssertHasRowMessageError(redundantBill2, message);

			declaration.Bills.RemoveAll();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.Validation.ValidateAll();
			AssertNoRowMessageError(masterBill, message);

			var redundantMasterBill = declaration.Bills.AddNew();
			redundantMasterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			redundantMasterBill.Validation.ValidateAll();
			AssertHasRowMessageError(redundantMasterBill, message);

			message = "There can only be at most 1 master bill, 1 house bills. The extra bills may be discarded when sending to customs.";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			redundantMasterBill.Validation.ValidateAll();
			AssertHasRowMessageError(redundantMasterBill, message);

			redundantMasterBill.CU_BillType = BillTypeList.Codes.HouseBill;
			redundantMasterBill.Validation.ValidateAll();
			AssertNoRowMessageError(redundantMasterBill, message);

			masterBill.CU_BillType = BillTypeList.Codes.HouseBill;
			redundantMasterBill.Validation.ValidateAll();
			AssertHasRowMessageError(redundantMasterBill, message);
		}

		public void TestCheckCU_BillNum()
		{
			var message = "Bill Number is not of a correct length.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "1234";

			AssertHasMessageError(bill.CU_BillNumInfo, message);

			bill.CU_BillNum = "12345";
			AssertNoMessageError(bill.CU_BillNumInfo, message);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			bill.Validation.ValidateCU_BillNum();
			AssertNoMessageError(bill.CU_BillNumInfo, message);

			bill.CU_BillNum = "123456789012345678901";
			AssertHasMessageError(bill.CU_BillNumInfo, message);
		}

		public void TestCheckCU_BillType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributeTypes.Codes.Type, "ふ中扱");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var bondedWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			bondedWarehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2HDN8", Core.Constants.CountryCodes.Japan);
			var firstHouseBill = declaration.Bills.AddNew();
			firstHouseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			var secondHouseBill = declaration.Bills.AddNew();
			secondHouseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			secondHouseBill.Validation.ValidateCU_BillType();
			AssertNoRowMessageError(secondHouseBill, "Depot type does not allow multiple bills.");

			declaration.DepotDocAddress.OrganisationPK = bondedWarehouse.PK;
			declaration.DepotDocAddress.E2_OA_Address = bondedWarehouse.MainAddress.PK;
			secondHouseBill.CU_BillNum = "123456";
			secondHouseBill.Validation.ValidateCU_BillType();
			AssertHasRowMessageError(secondHouseBill, "Depot type does not allow multiple bills.");
		}
	}
}
