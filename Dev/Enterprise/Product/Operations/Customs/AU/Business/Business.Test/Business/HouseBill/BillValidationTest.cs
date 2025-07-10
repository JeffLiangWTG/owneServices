using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestCusDecHouseBillValidationIsNotRunForExWarhouse()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_TransportMode = "";
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoNotifications("House bill is not mandatory", declaration.JE_HouseBillInfo);
		}

		public void TestConsignRefNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_fPartShipConsignmentReference = "B001";
			AssertHasWarning(bill.CU_fPartShipConsignmentReferenceInfo, BillValidation.ConsignReferenceEntered);

			bill.CU_fPartShipConsignmentReference = "";
			AssertNoWarning(bill.CU_fPartShipConsignmentReferenceInfo, BillValidation.ConsignReferenceEntered);
		}

		public void TestMasterBillValidationIsNotRunForExWarehouse()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_TransportMode = "";
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoNotifications("Master bill is not mandatory", declaration.JE_MasterBillInfo);
		}

		public void TestMasterBillValidation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors("Master bill is mandatory", declaration.JE_MasterBillInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageErrors("Master bill is not mandatory", declaration.JE_MasterBillInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors("Master bill is mandatory", declaration.JE_MasterBillInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageErrors("Master bill is not mandatory", declaration.JE_MasterBillInfo);
		}

		public void TestMasterBillIsMandatoryForSAC()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, declaration.IsSACWithoutLines);

			declaration.JE_MasterBill = "";
			AssertEquals("Master bill is mandatory", true, declaration.JE_MasterBillInfo.HasMessageErrors());

			declaration.JE_MasterBill = "M1";
			AssertEquals("Masterbill is mandatory", false, declaration.JE_MasterBillInfo.HasMessageErrors());

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_ParentBillUniqueCode = "";
			AssertEquals("Master bill is mandatory", true, bill2.CU_ParentBillUniqueCodeInfo.HasMessageErrors());

			bill2.CU_ParentBillUniqueCode = declaration.PrimaryMasterBill.CU_BillUniqueCode;
			AssertEquals("Master bill is mandatory and entered", false, bill2.CU_ParentBillUniqueCodeInfo.HasMessageErrors());
		}

		public void TestNoParcelPostErrorForExports()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = ZString.Empty;
			Assert(!bill.CU_BillNumInfo.HasMessageErrors());
		}

		public void TestParcelPostValidationForEdifice()
		{
			string[] validParcelPostNumbers = { "1N123", "2V293", "2NT12345", "1N123", "2NT12345" };
			string[] invalidParcelPostNumbers = { "1X123", "4N123", "1N123, 4N123", "1N1,1N2,1N3,1N4,1N5,1N6", "1N123T" };
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;

			AssertValidationForParcelPostNumbersForEdifice(bill, validParcelPostNumbers, invalidParcelPostNumbers);
		}

		public void TestParcelPostValidationForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;

			string[] validParcelPostNumbers = { "N123", "V293", "Q12345", "W123", "S12345" };
			string[] invalidParcelPostNumbers = { "X123", "4N123", "1O123, 1234", "1N1,1N2,1N3,1N4,1N5,1N6", "1N123T" };

			AssertValidationForParcelPostNumbersForCMR(bill, validParcelPostNumbers, invalidParcelPostNumbers);
		}

		public void TestParcelsPostNumberRequired()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "";
			AssertHasMessageError(bill.CU_BillNumInfo, BillValidation.ParcelPostNumberIsRequired);

			bill.CU_HouseBill = "N123";
			AssertNoMessageError(bill.CU_BillNumInfo, BillValidation.ParcelPostNumberIsRequired);
		}

		public void TestWarningIfNotPacked()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "foo";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "spam";
			PackingGroup pivot = declaration.PackingGroups.AddNew();
			pivot.CR_CU_HouseBill = bill2.PK;
			pivot.Packages.AddNew();

			bill.Validation.ValidateCU_BillNum();
			bill2.Validation.ValidateCU_BillNum();

			AssertHasMessageError(bill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			AssertNoMessageError(bill2.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			bill.Validation.ValidateCU_BillNum();
			bill2.Validation.ValidateCU_BillNum();
			AssertNoMessageError(bill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			AssertNoMessageError(bill2.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			bill.Validation.ValidateCU_BillNum();
			bill2.Validation.ValidateCU_BillNum();
			AssertNoMessageError(bill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			AssertNoMessageError(bill2.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			bill.Validation.ValidateCU_BillNum();
			bill2.Validation.ValidateCU_BillNum();
			AssertHasMessageError(bill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			AssertNoMessageError(bill2.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
		}

		public void TestNoValidationOnHouseBillIfEmpty()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			declaration.JE_HouseBill = ZString.Empty;
			AssertNoNotifications(declaration.JE_HouseBillInfo);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_HouseBill = ZString.Empty;
			AssertNoNotifications(declaration.JE_HouseBillInfo);
		}

		void AssertValidationForParcelPostNumbersForEdifice(Bill bill, string[] validParcelPostNumbers, string[] invalidParcelPostNumbers)
		{
			foreach (string validNumber in validParcelPostNumbers)
			{
				bill.CU_BillNum = validNumber;
				AssertNoMessageErrors(bill.CU_BillNumInfo);
			}

			foreach (string invalidNumber in invalidParcelPostNumbers)
			{
				bill.CU_BillNum = invalidNumber;
				AssertHasMessageErrors(bill.CU_BillNumInfo);
			}
		}

		void AssertValidationForParcelPostNumbersForCMR(Bill bill, string[] validParcelPostNumbers, string[] invalidParcelPostNumbers)
		{
			foreach (string validNumber in validParcelPostNumbers)
			{
				bill.CU_BillNum = validNumber;
				AssertNoMessageError(bill.CU_BillNumInfo, BillValidation.InvalidParcelPostNumber);
			}

			foreach (string invalidNumber in invalidParcelPostNumbers)
			{
				bill.CU_BillNum = invalidNumber;
				AssertHasMessageError(bill.CU_BillNumInfo, BillValidation.InvalidParcelPostNumber);
			}
		}
	}
}
