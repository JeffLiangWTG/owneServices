using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class PreviousDocumentMasterValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_Procedure_PrematureInput()
		{
			var info = instruction.PreviousDocumentMaster.CSI_ProcedureInfo;

			var validProcedureCodes = new string[] { PreviousProcedureList.Codes._ATNEU, PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Codes._ATZL, PreviousProcedureList.Codes._ESUMA, PreviousProcedureList.Codes._T1,
				PreviousProcedureList.Codes._T2, PreviousProcedureList.Codes._ATA, PreviousProcedureList.Codes._VO, PreviousProcedureList.Codes._TIR, PreviousProcedureList.Codes._OHNE };
			CombineAssertions(() =>
			{
				foreach (var procedureCode in validProcedureCodes)
				{
					instruction.PreviousDocumentMaster.CSI_Procedure = procedureCode;
					AssertNoMessageError($"{procedureCode} is valid", info, prematureInputFlagInvalidForPreviousDocument);
				}

				foreach (var procedureCode in new string[] { PreviousProcedureList.Codes._GB, PreviousProcedureList.Codes._POST, PreviousProcedureList.Codes._PUEB })
				{
					instruction.PreviousDocumentMaster.CSI_Procedure = procedureCode;
					AssertHasMessageError($"{procedureCode} is invalid", info, prematureInputFlagInvalidForPreviousDocument);
				}
			});
		}

		public void TestCheckCSI_Procedure()
		{
			var info = instruction.PreviousDocumentMaster.CSI_ProcedureInfo;
			instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(info, ListValidation.InvalidCodeMessageError);

			instruction.PreviousDocumentMaster.CSI_Procedure = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(info, ListValidation.InvalidCodeMessageError);

			instruction.PreviousDocumentMaster.CSI_Procedure = "@@";
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_Procedure_ExportInvoiceLine()
		{
			var message = "You have not entered a Previous Procedure.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var previousProcedureMaster = invoiceLine.PreviousProcedureMaster;
			var info = previousProcedureMaster.CSI_ProcedureInfo;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(info, "##", PreviousProcedureList.Codes._ATAV);

				entryInstruction.CEI_SubStyle = "00";
				invoiceLine.JI_Procedure = "3151";
				ValidationTestHelper.AssertWarningIfNotEntered(info, message, "PreviousProcedure is required for InvoiceLine");

				entryInstruction.CEI_SubStyle = "10";
				ValidationTestHelper.AssertNoWarningIfNotEntered(info, message, "PreviousProcedure isn't required for InvoiceLine");
			});
		}

		public void TestCheckCSI_Procedure_ImportEntryInstruction()
		{
			var message = "You have not entered a Previous Procedure.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var previousProcedureMaster = entryInstruction.PreviousDocumentMaster;
			var info = previousProcedureMaster.CSI_ProcedureInfo;

			CombineAssertions(() =>
			{
				previousProcedureMaster.Validation.ValidateCSI_Procedure();
				AssertHasMessageError("PreviousProcedure is required for EntryInstruction", info, message);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
				previousProcedureMaster.Validation.ValidateCSI_Procedure();
				AssertNoMessageError("PreviousProcedure isn't required for EntryInstruction", info, message);
			});
		}

		public void TestCheckAuthorizationNumber_ATZL()
		{
			const string warningMessage = "The entered Authorization is not linked to an Organization of this Declaration.";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var orgHeader = Factory.New<OrgHeader>();
			var declarantAddress = orgHeader.MainAddress;
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DECW123");
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			var master = dec.CustomsEntryInstructions.AddNew().PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var info = master.AuthorizationNumberInfo;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(info);

				master.AuthorizationNumber = "invalid";
				AssertHasWarning("Invalid value", info, warningMessage);
				master.AuthorizationNumber = "DECW123";
				AssertNoWarning("Valid value", info, warningMessage);
			});
		}

		public void TestCheckAuthorizationNumber_ATAV()
		{
			const string warningMessage = "The entered Authorization is not linked to an Organization of this Declaration.";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEIPO123");
			dec.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var master = dec.CustomsEntryInstructions.AddNew().PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			master.SimplifiedGrantAuthorizationFlag = true;
			var info = master.AuthorizationNumberInfo;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(info);

				master.SimplifiedGrantAuthorizationFlag = false;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(info);

				master.AuthorizationNumber = "invalid";
				AssertHasWarning("Invalid value", info, warningMessage);
				master.AuthorizationNumber = "DEIPO123";
				AssertNoWarning("Valid value", info, warningMessage);
			});
		}

		public void TestCheckAuthorizationNumber_ValidateAll()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var master = dec.CustomsEntryInstructions.AddNew().PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var info = master.AuthorizationNumberInfo;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("Before ValidateAll", info, MandatoryValidation.YouHaveNotEntered);
				master.Validation.ValidateAll();
				AssertHasMessageErrorContaining("After ValidateAll", info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_CustomsOffice_ATAV()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var master = instruction.PreviousDocumentMaster;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var zzd = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1", "Valid", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			helper.CreateCusCodeListAttribute(zzd.PK, Universal.RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");

			Factory.Save();

			var info = master.CSI_CustomsOfficeInfo;
			master.CSI_CustomsOffice = "0";
			AssertHasMessageError(info, "The entered Customs Office is not a Main Office in Germany.");

			master.CSI_CustomsOffice = "1";
			AssertNoMessageError(info, "The entered Customs Office is not a Main Office in Germany.");

			master.SimplifiedGrantAuthorizationFlag = true;
			master.CSI_CustomsOffice = ZString.Empty;
			master.Validation.ValidateCSI_CustomsOffice();
			AssertHasMessageError(info, "You have not entered a Monitoring Customs Office.");

			master.CSI_CustomsOffice = "SomeValue";
			AssertNoMessageError(info, "You have not entered a Monitoring Customs Office.");

			master.SimplifiedGrantAuthorizationFlag = false;
			master.CSI_CustomsOffice = ZString.Empty;
			AssertNoMessageErrors(info);
		}

		public void TestCheckCSI_ReferenceNumber2_ATZL()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var master = instruction.PreviousDocumentMaster;
			master.CSI_ReferenceNumber2 = ZString.Empty;
			master.Validation.ValidateAll(); //As there is no current ValidateCSI_ReferenceNumber2() method
			AssertNoMessageErrors(master.CSI_ReferenceNumber2Info);
		}

		public void TestExportCusEntryInstructionHasNoPreviousDocumentValidation()
		{
			_ = instruction.PreviousDocumentMaster;
			instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.RunPreSaveValidation();
			Assert(!instruction.NotificationsIncludingChildren.Any(x => x.Message.Contains("You have not entered a Previous Procedure")));
		}

		public void TestImportCusEntryInstructionHasPreviousDocumentValidation()
		{
			_ = instruction.PreviousDocumentMaster;
			instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.RunPreSaveValidation();
			Assert(instruction.NotificationsIncludingChildren.Any(x => x.Message.Contains("You have not entered a Previous Procedure")));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PrematureInputFlag = true;
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}
		CusEntryInstruction instruction;
		const string prematureInputFlagInvalidForPreviousDocument = "Premature Input flag invalid for Previous Document.";
	}
}
