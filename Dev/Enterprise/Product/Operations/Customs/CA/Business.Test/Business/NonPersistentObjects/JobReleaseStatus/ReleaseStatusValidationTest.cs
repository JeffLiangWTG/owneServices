using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ReleaseStatusValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRL_Bill()
		{
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			releaseStatus.RL_Bill = ZGuid.Empty;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			releaseStatus.Validation.ValidateRL_Bill();
			AssertHasMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			releaseStatus.Validation.ValidateRL_Bill();
			AssertNoMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			releaseStatus.Validation.ValidateRL_Bill();
			AssertHasMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			releaseStatus.Validation.ValidateRL_Bill();
			AssertNoMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			releaseStatus.RL_Bill = bill1.PK;
			releaseStatus.Validation.ValidateRL_Bill();
			AssertNoMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
			releaseStatus.RL_Bill = ZGuid.Empty;
			declaration.Bills.RemoveAndDelete(bill1);
			releaseStatus.Validation.ValidateRL_Bill();
			AssertNoMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);

			var helper = new DeclarationTestHelper(Factory, true);
			var releaseStatus2 = new ReleaseStatus((EDIReleaseMessage)helper.GetEDIReleaseResponseMessage("37132536987", "1"));
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			releaseStatus2.Validation.ValidateRL_Bill();
			AssertNoMessageErrorContaining(releaseStatus.RL_BillInfo, ReleaseStatusValidation.NoAssociatedBill);
		}

		public void TestDuplicatedCCNNumber()
		{
			var ccn1 = declaration.ReleaseStatuses.AddNew();
			ccn1.RL_CargoControlNumber = "CCN0";
			var ccn2 = declaration.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "CCN0";
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertHasMessageErrorContaining(ccn2.RL_CargoControlNumberInfo, "Duplicated CCN numbers on this declaration (CCN Number:'CCN0')");

			var ccn3 = declaration.ReleaseStatuses.AddNew();
			ccn3.RL_CargoControlNumber = "ccn1";
			var ccn4 = declaration.ReleaseStatuses.AddNew();
			ccn4.RL_CargoControlNumber = "CCN1";
			ccn4.Validation.ValidateRL_CargoControlNumber();
			AssertHasMessageErrorContaining(ccn4.RL_CargoControlNumberInfo, "Duplicated CCN numbers on this declaration (CCN Number:'CCN1')");
		}

		[TestDate(2016, 02, 18)]
		public void TestDuplicatedCCNNumberOnDifferentDeclarations()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_DeclarationReference = "B00001000";
			declaration.Company.GC_Name = "GLBCOMP";
			declaration.Branch.GB_BranchName = "GLBBRNCH";

			var ccn0 = declaration.ReleaseStatuses.AddNew();
			ccn0.RL_CargoControlNumber = "CCN001";
			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2014, 02, 19);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "B00001001";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration1.Company.GC_Name = "GLBCOMP";
			declaration1.Branch.GB_BranchName = "GLBBRNCH";

			var ccn1 = declaration1.ReleaseStatuses.AddNew();
			ccn1.RL_CargoControlNumber = "CCN002";
			declaration1.JE_SystemCreateTimeUtc = new ZDateTime(2009, 02, 19);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var ccn2 = declaration2.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "CCN001";
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertHasMessageError(ccn2.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001000', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");

			ccn2.RL_CargoControlNumber = "CCN002";
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertNoMessageErrors(ccn2.RL_CargoControlNumberInfo);

			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2010, 02, 18);
			Factory.Save();

			ccn2.RL_CargoControlNumber = "CCN001";
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertNoErrors(ccn2.RL_CargoControlNumberInfo);

			declaration1.JE_SystemCreateTimeUtc = new ZDateTime(2014, 02, 19);
			Factory.Save();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_DeclarationReference = "B00001003";
			declaration3.Company.GC_Name = "GLBCOMP";
			declaration3.Branch.GB_BranchName = "GLBBRNCH";

			var ccn3 = declaration.ReleaseStatuses.AddNew();
			ccn3.RL_CargoControlNumber = "CCN003";
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration3.JE_SystemCreateTimeUtc = new ZDateTime(2014, 02, 19);
			Factory.Save();

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_DeclarationReference = "B00001004";
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Road;
			Factory.Save();

			var ccn4 = declaration4.ReleaseStatuses.AddNew();
			ccn4.RL_CargoControlNumber = "CCN002";
			ccn4.Validation.ValidateRL_CargoControlNumber();
			AssertHasMessageError(ccn4.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001001', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");

			ccn4.RL_CargoControlNumber = "CCN003";
			ccn4.Validation.ValidateRL_CargoControlNumber();
			AssertNoMessageErrors(ccn4.RL_CargoControlNumberInfo);
		}

		[TestDate(2022, 12, 31)]
		public void TestDuplicatedCCNNumberUsingCreateTime()
		{
			declaration.JE_DeclarationReference = "B00001000";
			declaration.Company.GC_Name = "GLBCOMP";
			declaration.Branch.GB_BranchName = "GLBBRNCH";
			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2017, 02, 18);
			var ccn0 = declaration.ReleaseStatuses.AddNew();
			ccn0.RL_CargoControlNumber = "CCN001";

			var reuseTime = CACustomsDataRegistry.Instance.CCNReuseTimeSetting.Value;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "B00001001";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.Company.GC_Name = "GLBCOMP";
			declaration1.Branch.GB_BranchName = "GLBBRNCH";
			declaration1.JE_SystemCreateTimeUtc = declaration.JE_SystemCreateTimeUtc.AddYears(reuseTime + 1);

			var ccn1 = declaration1.ReleaseStatuses.AddNew();
			ccn1.RL_CargoControlNumber = "CCN001";
			Factory.Save();

			ccn1.Validation.ValidateRL_CargoControlNumber();
			AssertNoMessageError(ccn1.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001000', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");

			declaration.JE_SystemCreateTimeUtc = declaration.JE_SystemCreateTimeUtc.AddYears(reuseTime - 1);
			Factory.Save();
			ccn1.Validation.ValidateRL_CargoControlNumber();
			AssertHasMessageError(ccn1.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001000', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");
		}

		public void TestDuplicatedCCNNumber_NoModeOfTransportCondition()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var ccn1 = declaration.ReleaseStatuses.AddNew();
			ccn1.RL_CargoControlNumber = "CCN001";
			Factory.Save();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "B00001001";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration1.Company.GC_Name = "GLBCOMP";
			declaration1.Branch.GB_BranchName = "GLBBRNCH";
			Factory.Save();

			var ccn2 = declaration1.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "CCN001";
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertEquals("Pre-condition: Transport modes are equal", declaration.JE_TransportMode, declaration1.JE_TransportMode);
			AssertHasMessageError(ccn2.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001000', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");

			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			ccn2.Validation.ValidateRL_CargoControlNumber();
			AssertNotEquals("Pre-condition: Transport modes are not equal", declaration.JE_TransportMode, declaration1.JE_TransportMode);
			AssertHasMessageError(ccn2.RL_CargoControlNumberInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001000', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");
		}

		public void TestCheckRL_CargoControlNumber()
		{
			declaration.ReleaseStatuses.RemoveAndDeleteAll();
			var ccn = declaration.ReleaseStatuses.AddNew();
			ccn.RL_CargoControlNumber = "CCN001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var validator = new ReleaseStatusValidation(ccn);
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, ReleaseStatusValidation.NoAssociatedInvoiceLine);
			var invoice = declaration.Invoices.AddNew();
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, ReleaseStatusValidation.NoAssociatedInvoiceLine);
			var ccn2 = declaration.ReleaseStatuses.AddNew();
			validator.ValidateRL_CargoControlNumber();
			AssertHasMessageErrorContaining(ccn.RL_CargoControlNumberInfo, ReleaseStatusValidation.NoAssociatedInvoiceLine);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, ReleaseStatusValidation.NoAssociatedInvoiceLine);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var ccnOnInvoice = invoice.CargoControlNumbersList.AddNew();
			ccnOnInvoice.J2_ReferenceNumber = "CCN001";
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, ReleaseStatusValidation.NoAssociatedInvoiceLine);
			ccn.RL_CargoControlNumber = "A";
			validator.ValidateRL_CargoControlNumber();
			AssertNoErrorContaining(ccn.RL_CargoControlNumberInfo, MandatoryValidation.MustBeEntered);
			ccn.RL_CargoControlNumber = "";
			validator.ValidateRL_CargoControlNumber();
			AssertHasErrorContaining(ccn.RL_CargoControlNumberInfo, MandatoryValidation.MustBeEntered);

			ccn.RL_CargoControlNumber = "123ABC";
			validator.ValidateRL_CargoControlNumber();
			AssertNoWarning(ccn.RL_CargoControlNumberInfo, "The Cargo Control Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			ccn.RL_CargoControlNumber = "123ABC– ";
			validator.ValidateRL_CargoControlNumber();
			AssertHasWarning(ccn.RL_CargoControlNumberInfo, "The Cargo Control Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			ccn.RL_CargoControlNumber = "A+1";
			validator.ValidateRL_CargoControlNumber();
			AssertHasMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");

			ccn.RL_CargoControlNumber = "A1";
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");

			ccn.RL_CargoControlNumber = "a:1";
			validator.ValidateRL_CargoControlNumber();
			AssertHasMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");

			ccn.RL_CargoControlNumber = "a1";
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");

			ccn.RL_CargoControlNumber = "A 1";
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");

			ccn.RL_CargoControlNumber = "A-1";
			validator.ValidateRL_CargoControlNumber();
			AssertNoMessageErrorContaining(ccn.RL_CargoControlNumberInfo, "Cargo Control Number should only contain letters, numbers, dash(-) and spaces.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseStatus = declaration.ReleaseStatuses.AddNew();
		}

		ReleaseStatus releaseStatus;
		JobDeclaration declaration;

		#endregion

	}
}
