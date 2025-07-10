using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusInBondMoveHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMandatoryProperties()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(moveHeader.BM_Calc_PermitNumberInfo, "Please enter a Permit #.");
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(moveHeader.BM_Calc_IssueDateInfo);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(moveHeader.BM_Calc_ValidityDateInfo);
			});
		}

		public void TestBM_Calc_PermitNumberUnique()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CusInBondPermitsHeaders.AddNew().BM_Calc_PermitNumber = "PN0001";
			var moveHeader = instruction.CusInBondPermitsHeaders.AddNew();
			moveHeader.BM_Calc_PermitNumber = "PN0001";
			AssertHasError(moveHeader.BM_Calc_PermitNumberInfo, "Permit # should be unique per Entry Instruction.");
			moveHeader.BM_Calc_PermitNumber = "PN0002";
			AssertNoError(moveHeader.BM_Calc_PermitNumberInfo, "Permit # should be unique per Entry Instruction.");
		}

		public void TestTransitPermitDatesChanged()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "JD00001";
			var instruction1 = dec1.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "ET1";
			var moveHeader1 = instruction1.CusInBondPermitsHeaders.AddNew();
			moveHeader1.BM_Calc_PermitNumber = "PN001";
			moveHeader1.BM_Calc_IssueDate = ZDate.Today.AddDays(1);
			moveHeader1.BM_ArrivalDate = ZDate.Today.AddDays(2);
			moveHeader1.BM_Calc_ValidityDate = ZDate.Today.AddDays(3);
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "JD00002";
			var instruction2 = dec2.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "ET2";
			var moveHeader2 = instruction2.CusInBondPermitsHeaders.AddNew();
			moveHeader2.BM_Calc_PermitNumber = "PN001";

			moveHeader2.BM_Calc_IssueDate = ZDate.Today.AddDays(4);
			var issueDateString = moveHeader1.BM_Calc_IssueDate.ToShortDateString();
			string message = "Entry Instruction ET1 on Declaration JD00001 has a different {0} ({1}) for this Permit #.";
			AssertHasMessageErrorContaining("Check changed value from DB", moveHeader2.BM_Calc_IssueDateInfo, string.Format(message, "Issue Date", issueDateString));
			moveHeader2.BM_Calc_IssueDate = ZDate.Today.AddDays(1);
			AssertNoMessageErrorContaining("Check changed value from DB", moveHeader2.BM_Calc_IssueDateInfo, string.Format(message, "Issue Date", issueDateString));

			moveHeader2.BM_ArrivalDate = ZDate.Today.AddDays(5);
			var arrivalDateString = moveHeader1.BM_ArrivalDate.ToShortDateString();
			AssertHasMessageErrorContaining("Check changed value from DB", moveHeader2.BM_ArrivalDateInfo, string.Format(message, "Arrival Date", arrivalDateString));
			moveHeader2.BM_ArrivalDate = ZDate.Today.AddDays(2);
			AssertNoMessageErrorContaining("Check changed value from DB", moveHeader2.BM_ArrivalDateInfo, string.Format(message, "Arrival Date", arrivalDateString));

			moveHeader2.BM_Calc_ValidityDate = ZDate.Today.AddDays(6);
			var validityDateString = moveHeader1.BM_Calc_ValidityDate.ToShortDateString();
			AssertHasMessageErrorContaining("Check changed value from DB", moveHeader2.BM_Calc_ValidityDateInfo, string.Format(message, "Validity Date", validityDateString));
			moveHeader2.BM_Calc_ValidityDate = ZDate.Today.AddDays(3);
			AssertNoMessageErrorContaining("Check changed value from DB", moveHeader2.BM_Calc_ValidityDateInfo, string.Format(message, "Validity Date", validityDateString));

			var instruction3 = dec2.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Style = "ET3";
			var moveHeader3 = instruction3.CusInBondPermitsHeaders.AddNew();
			moveHeader3.BM_Calc_PermitNumber = "PN002";
			moveHeader3.BM_Calc_IssueDate = ZDateTime.Today.AddDays(-4);
			moveHeader3.BM_ArrivalDate = ZDateTime.Today.AddDays(5);
			moveHeader3.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(6);

			var instruction4 = dec2.CustomsEntryInstructions.AddNew();
			instruction4.CEI_Style = "ET4";
			var moveHeader4 = instruction4.CusInBondPermitsHeaders.AddNew();
			moveHeader4.BM_Calc_PermitNumber = "PN002";
			moveHeader4.BM_Calc_IssueDate = ZDate.Today.AddDays(-7);
			issueDateString = moveHeader3.BM_Calc_IssueDate.ToShortDateString();
			AssertHasMessageErrorContaining("Check changed value from Current Job", moveHeader4.BM_Calc_IssueDateInfo, string.Format("Entry Instruction ET3 on Declaration JD00002 has a different Issue Date ({0}) for this Permit #.", issueDateString));
		}

		public void TestCheckBM_Calc_IssueDate()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var permitNumberBizObj = moveHeader.PermitNumberBizObj;
			permitNumberBizObj.CE_EntryNum = "1234";
			moveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday;
			permitNumberBizObj.CE_IssueDate = ZDateTime.BrettsBirthday;
			permitNumberBizObj.CE_ExpiryDate = ZDateTime.BrettsBirthday;
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Today.");
				AssertNoErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Validity Date.");
				AssertNoErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Arrival Date.");
				moveHeader.BM_ArrivalDate = ZDateTime.Now.AddDays(-2);
				moveHeader.BM_Calc_IssueDate = ZDateTime.Now.AddDays(1);
				moveHeader.BM_Calc_ValidityDate = ZDateTime.Now.AddDays(-2);
				AssertHasErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Today.");
				AssertHasErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Validity Date.");
				AssertHasErrorContaining(moveHeader.BM_Calc_IssueDateInfo, "Issue Date should be before or equal to Arrival Date.");
			});
		}

		public void TestCheckBM_Calc_ValidityDate()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var permitNumberBizObj = moveHeader.PermitNumberBizObj;
			permitNumberBizObj.CE_EntryNum = "1234";
			permitNumberBizObj.CE_IssueDate = ZDateTime.BrettsBirthday;
			permitNumberBizObj.CE_ExpiryDate = ZDateTime.BrettsBirthday;
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(moveHeader.BM_Calc_ValidityDateInfo, "Validity Date should be after or equal to Issue Date.");
				moveHeader.BM_Calc_IssueDate = ZDateTime.Now.AddDays(1);
				moveHeader.BM_Calc_ValidityDate = ZDateTime.Now.AddDays(-2);
				AssertHasErrorContaining(moveHeader.BM_Calc_ValidityDateInfo, "Validity Date should be after or equal to Issue Date.");
			});
		}

		public void TestCheckBM_ArrivalDate()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var permitNumberBizObj = moveHeader.PermitNumberBizObj;
			permitNumberBizObj.CE_EntryNum = "1234";
			moveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday;
			permitNumberBizObj.CE_IssueDate = ZDateTime.BrettsBirthday;
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(moveHeader.BM_ArrivalDateInfo, "Arrival Date should be after or equal to Issue Date.");
				moveHeader.BM_Calc_IssueDate = ZDateTime.Now.AddDays(1);
				moveHeader.BM_ArrivalDate = ZDateTime.Now.AddDays(-2);
				AssertHasErrorContaining(moveHeader.BM_ArrivalDateInfo, "Arrival Date should be after or equal to Issue Date.");
			});
		}

		public void TestValidateAll()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			CombineAssertions(() =>
			{
				moveHeader.BM_Calc_PermitNumber = ZString.Empty;
				moveHeader.BM_Calc_IssueDate = ZDateTime.Empty;
				moveHeader.Validation.ValidateAll();
				AssertHasErrorContaining(moveHeader.BM_Calc_PermitNumberInfo, "Please enter a Permit #.");
				moveHeader.BM_Calc_PermitNumber = "A";
				moveHeader.BM_ArrivalDate = ZDateTime.Now.AddDays(-2);
				moveHeader.BM_Calc_IssueDate = ZDateTime.Now.AddDays(1);
				moveHeader.BM_Calc_ValidityDate = ZDateTime.Now.AddDays(-2);
				moveHeader.Validation.ValidateAll();
				AssertNoErrorContaining(moveHeader.BM_Calc_PermitNumberInfo, "Please enter a Permit #.");
				AssertHasErrorContaining(moveHeader.BM_ArrivalDateInfo, "Arrival Date should be after or equal to Issue Date.");
				AssertHasErrorContaining(moveHeader.BM_Calc_ValidityDateInfo, "Validity Date should be after or equal to Issue Date.");
				moveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday;
				moveHeader.BM_Calc_IssueDate = ZDateTime.BrettsBirthday;
				moveHeader.BM_Calc_ValidityDate = ZDateTime.BrettsBirthday;
				moveHeader.Validation.ValidateAll();
				AssertNoErrorContaining(moveHeader.BM_Calc_ValidityDateInfo, "Validity Date should be after or equal to Issue Date.");
				AssertNoErrorContaining(moveHeader.BM_ArrivalDateInfo, "Arrival Date should be after or equal to Issue Date.");
			});
		}
	}
}
