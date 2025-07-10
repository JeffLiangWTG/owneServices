using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionValidation))]
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_ValueType_SmallValue()
		{
			var expectedErrorMessage = "Small Value is input but there are multiple lines to be declared.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			cusEntryHeader.AllEntryLines.AddNew();
			instruction.CEI_ValueType = ValueTypeList.Codes.S;
			AssertNoMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);

			cusEntryHeader.AllEntryLines.AddNew();
			instruction.Validation.ValidateCEI_ValueType();
			AssertHasMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_ValueType_LargeValue()
		{
			var expectedErrorMessage = "Large Value is input but there are only small value goods.";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var invoiceLine5 = declaration.InvoiceLines.AddNew();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ValueType = ValueTypeList.Codes.L;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_NACCSCode = ExportNACCSCodeList.Codes.X;
			invoiceLine1.JI_Tariff = "1";
			entryLine1.InvoiceLines.Add(invoiceLine1);

			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine2.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
			invoiceLine2.JI_Tariff = "2";
			entryLine2.InvoiceLines.Add(invoiceLine2);

			var entryLine3 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine3.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
			invoiceLine3.JI_Tariff = "2";
			entryLine3.InvoiceLines.Add(invoiceLine3);

			var entryLine4 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine4.JI_NACCSCode = ExportNACCSCodeList.Codes.E;
			invoiceLine4.JI_Tariff = "3";
			entryLine4.InvoiceLines.Add(invoiceLine4);

			var entryLine5 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine5.JI_NACCSCode = ExportNACCSCodeList.Codes.T;
			invoiceLine5.JI_Tariff = "3";
			entryLine5.InvoiceLines.Add(invoiceLine5);

			instruction.Validation.ValidateCEI_ValueType();
			AssertHasMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);

			entryLine1.CL_CustomsValue = 300000;
			instruction.Validation.ValidateCEI_ValueType();
			AssertNoMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);

			entryLine1.CL_CustomsValue = 0;
			entryLine2.CL_CustomsValue = 150000;
			entryLine3.CL_CustomsValue = 150000;
			instruction.Validation.ValidateCEI_ValueType();
			AssertNoMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);

			entryLine2.CL_CustomsValue = 0;
			entryLine3.CL_CustomsValue = 0;
			entryLine4.CL_CustomsValue = 150000;
			entryLine5.CL_CustomsValue = 150000;
			instruction.Validation.ValidateCEI_ValueType();
			AssertNoMessageErrorContaining(instruction.CEI_ValueTypeInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_ValueType_ListValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = instruction.CEI_ValueTypeInfo;
			instruction.CEI_ValueType = "W";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			instruction.CEI_ValueType = ValueTypeList.Codes.S;
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_ValueType_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = instruction.CEI_ValueTypeInfo;

			var expectedMessageError = MandatoryValidation.YouHaveNotEnteredMessage("Value Type");
			instruction.CEI_ValueType = string.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			instruction.Validation.ValidateCEI_ValueType();
			AssertNoMessageError(targetInfo, expectedMessageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
			instruction.Validation.ValidateCEI_ValueType();
			AssertHasMessageError(targetInfo, expectedMessageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.G;
			instruction.Validation.ValidateCEI_ValueType();
			AssertHasMessageError(targetInfo, expectedMessageError);

			instruction.CEI_ValueType = ValueTypeList.Codes.S;
			AssertNoMessageError(targetInfo, expectedMessageError);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			instruction.Validation.ValidateCEI_ValueType();
			AssertHasMessageError(targetInfo, "Value Type must be empty when Declaration Type is Y.");

			instruction.CEI_Style = ZString.Empty;
			instruction.Validation.ValidateCEI_ValueType();
			AssertNoMessageError(targetInfo, "Value Type must be empty when Declaration Type is Y.");
		}

		public void TestCEI_StyleAndCEI_DescriptionPair()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "";
			instruction1.CEI_Description = "";

			instruction1.Validation.ValidateAll();
			var errorMessageBlank = "Please enter a unique Entry Instruction Name or Description.";
			AssertHasRowMessageError(instruction1, errorMessageBlank);

			instruction1.CEI_Style = null;
			instruction1.CEI_Description = null;
			instruction1.Validation.ValidateAll();
			AssertHasRowMessageError(instruction1, errorMessageBlank);

			instruction1.CEI_Style = "";
			instruction1.CEI_Description = "ABC";
			instruction1.Validation.ValidateAll();
			AssertNoRowMessageError(instruction1, errorMessageBlank);

			instruction1.CEI_Style = "Z";
			instruction1.CEI_Description = "";
			instruction1.Validation.ValidateAll();
			AssertNoRowMessageError(instruction1, errorMessageBlank);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "Z";
			instruction2.CEI_Description = "ABC";

			instruction1.Validation.ValidateAll();
			instruction2.Validation.ValidateAll();
			var errorMessageDuplicated = "This Entry Instruction already exists in the instructions list";
			AssertNoRowWarningContaining(instruction1, errorMessageDuplicated);
			AssertNoRowWarningContaining(instruction2, errorMessageDuplicated);

			instruction1.CEI_Description = "ABC";
			instruction2.CEI_Style = "Z";
			instruction2.CEI_Description = "ABC";
			instruction1.Validation.ValidateAll();
			instruction2.Validation.ValidateAll();
			AssertHasRowWarning(instruction1, errorMessageDuplicated);
			AssertHasRowWarning(instruction2, errorMessageDuplicated);
		}

		[TestDate(2024, 8, 15)]
		public void TestCheckCEI_DateForDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;

			var expectedErrorMessage = string.Format("{0} must be today or a future date.", entryInstruction.CEI_DateForDutyInfo.HumanReadableName);

			AssertEntityValidation(entryInstruction)
				.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.WhenProperty(x => x.EntryHeader.EntryNumber, Is.EqualTo(ZString.Empty))
				.ShouldCheckThat(x => x.CEI_DateForDutyInfo, Has.MessageErrorContaining(expectedErrorMessage));

			AssertEntityValidation(entryInstruction)
				.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(1)))
				.WhenProperty(x => x.EntryHeader.EntryNumber, Is.EqualTo(ZString.Empty))
				.ShouldCheckThat(x => x.CEI_DateForDutyInfo, Has.NoErrorContaining(expectedErrorMessage));

			AssertEntityValidation(entryInstruction)
				.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.WhenProperty(x => x.EntryHeader.EntryNumber, Is.EqualTo("12345"))
				.ShouldCheckThat(x => x.CEI_DateForDutyInfo, Has.NoErrorContaining(expectedErrorMessage));

			AssertCEI_DateForDutyOnSendingMessage(declaration, entryInstruction, JPProcedureCodeList.Codes.EDA, expectedErrorMessage);
			AssertCEI_DateForDutyOnSendingMessage(declaration, entryInstruction, JPProcedureCodeList.Codes.EDA01, expectedErrorMessage);
			AssertCEI_DateForDutyOnSendingMessage(declaration, entryInstruction, JPProcedureCodeList.Codes.IDA, expectedErrorMessage);
			AssertCEI_DateForDutyOnSendingMessage(declaration, entryInstruction, JPProcedureCodeList.Codes.IDA01, expectedErrorMessage);
			AssertCEI_DateForDutyOnSendingMessage(declaration, entryInstruction, JPProcedureCodeList.Codes.ECR, expectedErrorMessage, false);

			expectedErrorMessage = "Scheduled Declaration Date must be equal to or earlier than Date of Departure.";

			AssertEntityValidation(entryInstruction)
				.WhenProperty(x => x.JobDeclaration.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today))
				.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(1)))
				.ShouldCheckThat(x => x.CEI_DateForDutyInfo, Has.MessageErrorContaining(expectedErrorMessage));

			AssertEntityValidation(entryInstruction)
				.WhenProperty(x => x.JobDeclaration.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today))
				.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.CEI_DateForDutyInfo, Has.NoMessageErrorContaining(expectedErrorMessage));

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Japan))
			{
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
				currency.SetCustomsRate(new ZDateTime(2024, 8, 18), new ZDateTime(2024, 8, 24), 97.56m);
				Factory.Save();

				expectedErrorMessage = "Please select a date for which the exchange rate has been published.";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 8, 24);
				AssertNoMessageError(entryInstruction.CEI_DateForDutyInfo, expectedErrorMessage);

				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 9, 24);
				AssertHasMessageError(entryInstruction.CEI_DateForDutyInfo, expectedErrorMessage);
			}
		}

		void AssertCEI_DateForDutyOnSendingMessage(JobDeclaration declaration, CusEntryInstruction entryInstruction, string procedureCode, string expectedErrorMessage, bool hasErrorMessage = true)
		{
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = procedureCode };
			using (declaration.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(entryInstruction)
					.WhenProperty(x => x.CEI_DateForDuty, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
					.WhenProperty(x => x.EntryHeader.EntryNumber, Is.EqualTo("12345"))
					.ShouldCheckThat(x => x.CEI_DateForDutyInfo, hasErrorMessage ? Has.MessageErrorContaining(expectedErrorMessage) : Has.NoErrorContaining(expectedErrorMessage));
			}
		}

		public void TestNotesForCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.JP_CustomsNotes = new ZString('a', 139) + "語";
			AssertHasMessageError(entryInstruction.JP_CustomsNotesInfo, "The input text exceeded the max length. It will be truncated in the sent message.");

			entryInstruction.JP_CustomsNotes = new ZString('a', 137) + "\r\n" + "語";
			AssertNoMessageError(entryInstruction.JP_CustomsNotesInfo, "The input text exceeded the max length. It will be truncated in the sent message.");
		}

		public void TestNotesForBrokers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.JP_BrokersNotes = new ZString('a', 69) + "語";
			AssertHasMessageError(entryInstruction.JP_BrokersNotesInfo, "The input text exceeded the max length. It will be truncated in the sent message.");

			entryInstruction.JP_BrokersNotes = "Test";
			AssertNoMessageError(entryInstruction.JP_BrokersNotesInfo, "The input text exceeded the max length. It will be truncated in the sent message.");
		}

		public void TestNotesForCargoOwners()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.JP_OwnersNotes = new ZString('a', 69) + "語";
			AssertHasMessageError(entryInstruction.JP_OwnersNotesInfo, "The input text exceeded the max length. It will be truncated in the sent message.");

			entryInstruction.JP_OwnersNotes = "Test";
			AssertNoErrors(entryInstruction.JP_OwnersNotesInfo);
		}

		public void TestCheckJP_MarksAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = entryInstruction.JP_MarksAndNumbersInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			void AssertMarksAndNumbers()
			{
				entryInstruction.JP_MarksAndNumbers = ZString.Empty;
				foreach (var mailDeclarationCargoType in new[]
				{
					MailedCargoList.Codes.E,
					MailedCargoList.Codes.M,
					MailedCargoList.Codes.U,
					MailedCargoList.Codes.H
				})
				{
					entryInstruction.CEI_DeclarationCargoType = mailDeclarationCargoType;
					entryInstruction.Validation.ValidateJP_MarksAndNumbers();
					if (declaration.IsExport && !entryInstruction.IsExportOrReturnedGoodsDeclarationType)
					{
						AssertNoWarning(targetInfo, warningMessage);
					}
				}

				foreach (var otherDeclarationCargoType in new[]
				{
					DeclarationCargoTypeList.Codes.S,
					DeclarationCargoTypeList.Codes.B,
					DeclarationCargoTypeList.Codes.L,
					DeclarationCargoTypeList.Codes.X,
					DeclarationCargoTypeList.Codes.G,
					DeclarationCargoTypeList.Codes.P,
					DeclarationCargoTypeList.Codes.K,
				})
				{
					entryInstruction.CEI_DeclarationCargoType = otherDeclarationCargoType;
					ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
				}
			}

			foreach (var (messageType, declarationType) in new[]
			{
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, TaxationDeclarationTypeList.Codes.C),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, TaxationDeclarationTypeList.Codes.F),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.S),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.M),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.A),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.G),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.N),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.M),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.T),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.G),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.E),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.R),
			})
			{
				declaration.JE_MessageType = messageType;
				entryInstruction.CEI_Style = declarationType;

				AssertMarksAndNumbers();
			}

			entryInstruction.JP_MarksAndNumbers = new ZString('a', 139) + "語";
			AssertHasMessageError(targetInfo, "The input text exceeded the max length. It will be truncated in the sent message.");

			entryInstruction.JP_MarksAndNumbers = new ZString('a', 137) + "\r\n" + "語";
			AssertNoMessageError(targetInfo, "The input text exceeded the max length. It will be truncated in the sent message.");
		}

		public void TestCheckTradeTypeFirstChar()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.TradeTypeFirstChar = "6";

			var expectedErrorMessage = "The selected value is invalid.";
			AssertHasMessageError(entryInstruction.TradeTypeFirstCharInfo, expectedErrorMessage);

			entryInstruction.TradeTypeFirstChar = TradeTypeFirstChar.Codes.A;
			AssertNoMessageError(entryInstruction.TradeTypeFirstCharInfo, expectedErrorMessage);
		}

		public void TestCheckTradeTypeSecondChar()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.TradeTypeSecondChar = "A";

			var expectedErrorMessage = "The selected value is invalid.";
			AssertHasMessageError(entryInstruction.TradeTypeSecondCharInfo, expectedErrorMessage);

			entryInstruction.TradeTypeSecondChar = TradeTypeSecondChar.Codes.A;
			AssertNoMessageError(entryInstruction.TradeTypeSecondCharInfo, expectedErrorMessage);
		}

		public void TestCheckTradeTypeThirdChar()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.TradeTypeThirdChar = "9";

			var expectedErrorMessage = "The selected value is invalid.";
			AssertHasMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);

			entryInstruction.TradeTypeThirdChar = TradeTypeThirdChar.Codes.A;
			AssertNoMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);

			expectedErrorMessage = "You have not entered third char of Trade Type.";
			entryInstruction.TradeTypeFirstChar = "1";
			entryInstruction.TradeTypeSecondChar = "2";
			entryInstruction.TradeTypeThirdChar = string.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.Validation.ValidateTradeTypeThirdChar();
			AssertHasMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var declarationTypes = new string[] { JPImportDeclarationTypeList.Codes.K, JPImportDeclarationTypeList.Codes.D, JPImportDeclarationTypeList.Codes.U, JPImportDeclarationTypeList.Codes.L,
													JPImportDeclarationTypeList.Codes.B, JPImportDeclarationTypeList.Codes.E, JPImportDeclarationTypeList.Codes.R };
			foreach (var declarationType in declarationTypes)
			{
				entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.K;
				entryInstruction.Validation.ValidateTradeTypeThirdChar();
				AssertNoMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);
			}

			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.C;
			entryInstruction.Validation.ValidateTradeTypeThirdChar();
			AssertHasMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);

			entryInstruction.TradeTypeSecondChar = TradeTypeSecondChar.Codes.G;
			entryInstruction.Validation.ValidateTradeTypeThirdChar();
			AssertNoMessageError(entryInstruction.TradeTypeThirdCharInfo, expectedErrorMessage);
		}

		public void TestAllTradeTypeCharShouldBeEnteredTogether()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertAllTradeTypeCharShouldBeEnteredTogether(declaration, "1", " ", ZString.Empty, false, true, true);

			AssertAllTradeTypeCharShouldBeEnteredTogether(declaration, ZString.Empty, "1", " ", true, false, true);

			AssertAllTradeTypeCharShouldBeEnteredTogether(declaration, " ", ZString.Empty, "1", true, true, false);

			AssertAllTradeTypeCharShouldBeEnteredTogether(declaration, " ", " ", ZString.Empty, false, false, false);

			AssertAllTradeTypeCharShouldBeEnteredTogether(declaration, "1", "1", "1", false, false, false);
		}

		public void TestValidateApprovalCertificateInfos_MaxCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				var approvalCertificateInfo1 = entryInstruction.ApprovalCertificateInfos.AddNew();
				var approvalCertificateInfo2 = entryInstruction.ApprovalCertificateInfos.AddNew();
				var approvalCertificateInfo3 = entryInstruction.ApprovalCertificateInfos.AddNew();
				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

				var expectedErrorMessage = "The maximum row count for ITNO is 1. You have entered 2 rows.";
				approvalCertificateInfo1.CSI_Code = "ITNO";
				approvalCertificateInfo2.CSI_Code = "ITNO";
				AssertHasRowMessageError(entryInstruction, expectedErrorMessage);

				approvalCertificateInfo2.CSI_Code = string.Empty;
				AssertNoRowMessageError(entryInstruction, expectedErrorMessage);

				expectedErrorMessage = "The maximum row count for MOTS is 1. You have entered 3 rows.";
				approvalCertificateInfo1.CSI_Code = "MOTS";
				approvalCertificateInfo2.CSI_Code = "MOTS";
				approvalCertificateInfo3.CSI_Code = "MOTS";
				AssertHasRowMessageError(entryInstruction, expectedErrorMessage);

				approvalCertificateInfo1.CSI_Code = string.Empty;
				approvalCertificateInfo2.CSI_Code = string.Empty;
				AssertNoRowMessageError(entryInstruction, expectedErrorMessage);

				expectedErrorMessage = "The maximum row count for HFNN is 1. You have entered 2 rows.";
				approvalCertificateInfo1.CSI_Code = "HFNN";
				approvalCertificateInfo2.CSI_Code = "HFNN";
				AssertHasRowMessageError(entryInstruction, expectedErrorMessage);

				approvalCertificateInfo1.Delete();
				AssertNoRowMessageError(entryInstruction, expectedErrorMessage);
			});
		}

		void AssertAllTradeTypeCharShouldBeEnteredTogether(JobDeclaration declaration, ZString firstChar, ZString secondChar, ZString thirdChar, bool firstCharShouldHaveMessageError, bool secondCharShouldHaveMessageError, bool thirdCharShouldHaveMessageError)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.TradeTypeFirstChar = firstChar;
			entryInstruction.TradeTypeSecondChar = secondChar;
			entryInstruction.TradeTypeThirdChar = thirdChar;

			var firstCharInfo = entryInstruction.TradeTypeFirstCharInfo;
			var secondCharInfo = entryInstruction.TradeTypeSecondCharInfo;
			var thirdCharInfo = entryInstruction.TradeTypeThirdCharInfo;

			var expectedFirstCharNotEnteredErrorMessage = "You have not entered first char of Trade Type.";
			var expectedSecondCharNotEnteredErrorMessage = "You have not entered second char of Trade Type.";
			var expectedThirdCharNotEnteredErrorMessage = "You have not entered third char of Trade Type.";

			if (firstCharShouldHaveMessageError)
			{
				AssertHasMessageError(firstCharInfo, expectedFirstCharNotEnteredErrorMessage);
			}
			else
			{
				AssertNoMessageError(firstCharInfo, expectedFirstCharNotEnteredErrorMessage);
			}

			if (secondCharShouldHaveMessageError)
			{
				AssertHasMessageError(secondCharInfo, expectedSecondCharNotEnteredErrorMessage);
			}
			else
			{
				AssertNoMessageError(secondCharInfo, expectedSecondCharNotEnteredErrorMessage);
			}

			if (thirdCharShouldHaveMessageError)
			{
				AssertHasMessageError(thirdCharInfo, expectedThirdCharNotEnteredErrorMessage);
			}
			else
			{
				AssertNoMessageError(thirdCharInfo, expectedThirdCharNotEnteredErrorMessage);
			}
		}

		public void TestCheckCEI_PreInspectedCargoType()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			cusEntryInstruction.CEI_PreInspectedCargoType = PreInspectedCargoTypeList.Codes.A;
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_PreInspectedCargoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

			cusEntryInstruction.CEI_PreInspectedCargoType = "W";
			AssertHasMessageErrorContaining(cusEntryInstruction.CEI_PreInspectedCargoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckCEI_CommercialValueType_ListValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			cusEntryInstruction.CEI_CommercialValueType = "X";
			AssertHasMessageErrorContaining("CEI_CommercialValueType IMP", cusEntryInstruction.CEI_CommercialValueTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			cusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
			AssertNoMessageErrors("CEI_CommercialValueType IMP", cusEntryInstruction.CEI_CommercialValueTypeInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			cusEntryInstruction.CEI_CommercialValueType = "X";
			AssertNoMessageErrors("CEI_CommercialValueType EXP", cusEntryInstruction.CEI_CommercialValueTypeInfo);
		}

		public void TestCheckCEI_CommercialValueType_Tariff()
		{
			void AssertCommercialValueType(bool hasMessageError, string messageType, string declarationType)
			{
				declaration.JE_MessageType = messageType;
				cusEntryInstruction.CEI_Style = declarationType;

				declaration.InvoiceLines.RemoveAndDeleteAll();

				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "0000.23";

				cusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;

				var caption = cusEntryInstruction.CEI_CommercialValueTypeInfo.HumanReadableName;
				var message = $"{caption} must be empty when the Declaration Type is H or N, and a 6-character Tariff Code is entered.";

				if (hasMessageError)
				{
					AssertHasMessageError($"Should have the expected message error when the message type is {messageType} and the message sub type is {declarationType}.", cusEntryInstruction.CEI_CommercialValueTypeInfo, message);

					cusEntryInstruction.CEI_CommercialValueType = string.Empty;
					AssertNoMessageError("Should not have the expected message error when the value of 'CEI_CommercialValueType' is empty.", cusEntryInstruction.CEI_CommercialValueTypeInfo, message);

					invoiceLine.JI_Tariff = "0000.2305.01";
					cusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
					AssertNoMessageError("Should not have the expected message error when there is not any 6-character tariff on the invoice line level.", cusEntryInstruction.CEI_CommercialValueTypeInfo, message);
				}
				else
				{
					AssertNoMessageError($"Should not have the expected message error when the message type is {messageType} and the message sub type is {declarationType}.", cusEntryInstruction.CEI_CommercialValueTypeInfo, message);
				}
			}

			AssertCommercialValueType(true, JobMessageTypeList.Codes.Import, JPImportDeclarationTypeList.Codes.H);
			AssertCommercialValueType(true, JobMessageTypeList.Codes.Import, JPImportDeclarationTypeList.Codes.N);

			AssertCommercialValueType(false, JobMessageTypeList.Codes.Export, JPImportDeclarationTypeList.Codes.H);
			AssertCommercialValueType(false, JobMessageTypeList.Codes.Export, JPImportDeclarationTypeList.Codes.N);
			AssertCommercialValueType(false, JobMessageTypeList.Codes.Import, JPImportDeclarationTypeList.Codes.V);
		}

		public void TestCheckCEI_ContentInspectionResult()
		{
			declaration.JE_MessageType = "IMP";
			cusEntryInstruction.CEI_ContentInspectionResult = "X";
			AssertHasMessageErrorContaining("CEI_ContentInspectionResult IMP", cusEntryInstruction.CEI_ContentInspectionResultInfo, ListValidation.InvalidCodeMessageError.ToString());
			cusEntryInstruction.CEI_ContentInspectionResult = "A";
			AssertNoMessageErrors("CEI_ContentInspectionResult IMP", cusEntryInstruction.CEI_ContentInspectionResultInfo);

			declaration.JE_MessageType = "EXP";
			cusEntryInstruction.CEI_ContentInspectionResult = "X";
			AssertNoMessageErrors("CEI_ContentInspectionResult EXP", cusEntryInstruction.CEI_ContentInspectionResultInfo);
		}

		public void TestCheckCEI_DutyDrawback()
		{
			var targetInfo = cusEntryInstruction.CEI_DutyDrawbackInfo;

			var expectedMessageError = ListValidation.InvalidCodeMessageError.ToString();
			cusEntryInstruction.CEI_DutyDrawback = "X";
			AssertHasMessageErrorContaining("CEI_DutyDrawback invalid", targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_DutyDrawback = YesNoList.Codes.Yes;
			AssertNoMessageError(targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_DutyDrawback = YesNoList.Codes.No;
			AssertNoMessageError(targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_DutyDrawback = "";
			AssertNoMessageError(targetInfo, expectedMessageError);

			expectedMessageError = "Cannot claim tax return for this type of declaration.";
			cusEntryInstruction.CEI_DutyDrawback = YesNoList.Codes.Yes;
			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.TakeoverDeclarationTypeList.Codes.H;
			cusEntryInstruction.Validation.ValidateCEI_DutyDrawback();
			AssertNoMessageError(targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.TakeoverDeclarationTypeList.Codes.N;
			cusEntryInstruction.Validation.ValidateCEI_DutyDrawback();
			AssertNoMessageError(targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			cusEntryInstruction.Validation.ValidateCEI_DutyDrawback();
			AssertNoMessageError(targetInfo, expectedMessageError);

			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			cusEntryInstruction.Validation.ValidateCEI_DutyDrawback();
			AssertHasMessageErrorContaining(targetInfo, expectedMessageError);
		}

		public void TestCheckCertificateTypes_ShouldNotEnteredWhenMoreThanOneBill()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();
			var bill3 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;

			AssertCertificateType(cusEntryInstruction.CEI_FoodHygieneCertificateTypeInfo);
			AssertCertificateType(cusEntryInstruction.CEI_AnimalQuarantineCertificateTypeInfo);
			AssertCertificateType(cusEntryInstruction.CEI_PlantProtectionCertificateTypeInfo);

			void AssertCertificateType(ZPropertyInfo testInfo)
			{
				var expectedMessageError = string.Format("{0} should not be entered when there are more than one house bills.", testInfo.HumanReadableName);
				cusEntryInstruction.SetPropertyValue(testInfo.Name, new ZString("X"));
				AssertHasMessageErrorContaining(testInfo, expectedMessageError);

				cusEntryInstruction.SetPropertyValue(testInfo.Name, ZString.Empty);
				AssertNoMessageErrorContaining(testInfo, expectedMessageError);
			}
		}

		public void TestCheckCertificateTypes()
		{
			declaration.JE_MessageType = "IMP";

			cusEntryInstruction.CEI_AnimalQuarantineCertificateType = "X";
			AssertHasMessageErrorContaining("CEI_AnimalQuarantineCertificateType", cusEntryInstruction.CEI_AnimalQuarantineCertificateTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			cusEntryInstruction.CEI_AnimalQuarantineCertificateType = "Y";
			AssertNoMessageErrors(cusEntryInstruction.CEI_AnimalQuarantineCertificateTypeInfo);

			cusEntryInstruction.CEI_FoodHygieneCertificateType = "X";
			AssertHasMessageErrorContaining("CEI_FoodHygieneCertificateType", cusEntryInstruction.CEI_FoodHygieneCertificateTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			cusEntryInstruction.CEI_FoodHygieneCertificateType = "Y";
			AssertNoMessageErrors(cusEntryInstruction.CEI_FoodHygieneCertificateTypeInfo);

			cusEntryInstruction.CEI_PlantProtectionCertificateType = "X";
			AssertHasMessageErrorContaining("CEI_PlantProtectionCertificateType", cusEntryInstruction.CEI_PlantProtectionCertificateTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			cusEntryInstruction.CEI_PlantProtectionCertificateType = "Y";
			AssertNoMessageErrors(cusEntryInstruction.CEI_PlantProtectionCertificateTypeInfo);

			declaration.JE_MessageType = "EXP";
			cusEntryInstruction.Validation.ValidateAll();
			AssertNoMessageErrors(cusEntryInstruction.CEI_AnimalQuarantineCertificateTypeInfo);
			AssertNoMessageErrors(cusEntryInstruction.CEI_FoodHygieneCertificateTypeInfo);
			AssertNoMessageErrors(cusEntryInstruction.CEI_PlantProtectionCertificateTypeInfo);
		}

		public void TestCheckCEI_DeclarationCargoType()
		{
			var info = cusEntryInstruction.CEI_DeclarationCargoTypeInfo;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			cusEntryInstruction.CEI_DeclarationCargoType = "A";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_DeclarationCargoType = "P";
			AssertNoMessageErrors(info);
		}

		public void TestCheckCEI_SubStyle()
		{
			var info = cusEntryInstruction.CEI_SubStyleInfo;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertNoErrors("Field is not mandatory", info);

			cusEntryInstruction.CEI_SubStyle = "P";
			AssertListValidationInvalidCodeMessageError(info, true);

			cusEntryInstruction.CEI_SubStyle = JPAdditionalDeclarationTypeList.Codes.K;
			AssertListValidationInvalidCodeMessageError(info, false);

			cusEntryInstruction.CEI_SubStyle = "7";
			AssertListValidationInvalidCodeMessageError(info, true);
		}

		public void TestCheckCEI_Style_MarineProductsExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingDataGrouping("JP");
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			refCusCodeList.Attributes.DeleteAll();
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Type, "洋上");

			Factory.Save();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_CustomsRegNo = "2HDN8";
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			declaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;

			var expectedMessageError = "Declaration Type must be E when Customs Depot is 洋上.";

			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.E;
			AssertNoMessageError(cusEntryInstruction.CEI_StyleInfo, expectedMessageError);

			cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.F;
			AssertHasMessageError(cusEntryInstruction.CEI_StyleInfo, expectedMessageError);
		}

		public void TestCheckCEI_Style_RequiredCertificateType()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
				var targetInfo = entryInstruction.CEI_StyleInfo;
				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

				var expectedErrorMessage = "Please add AEOU or AEOH as certificate.";
				entryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.N;
				AssertHasMessageError(targetInfo, expectedErrorMessage);
				approvalCertificateInfo.CSI_Code = "AEOH";
				AssertNoMessageError(targetInfo, expectedErrorMessage);
				approvalCertificateInfo.CSI_Code = "AEOU";
				AssertNoMessageError(targetInfo, expectedErrorMessage);
			});
		}

		public void TestCheckCEI_Style_DefualtAddItem()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var approvalCertificateInfo = entryInstruction.ApprovalCertificateInfos.AddNew();
				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
				approvalCertificateInfo.CSI_Code = "MOTS";
				AssertEquals(1, entryInstruction.ApprovalCertificateInfos.Count);

				entryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
				entryInstruction.Validation.ValidateCEI_Style();
				AssertEquals(2, entryInstruction.ApprovalCertificateInfos.Count);
				Assert(entryInstruction.ApprovalCertificateInfos.Where(x => x.CSI_Code.Equals(ApprovalCertificateInfoCodes.ITNO)).Any());

				entryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.M;
				entryInstruction.Validation.ValidateCEI_Style();
				AssertEquals(3, entryInstruction.ApprovalCertificateInfos.Count);
				Assert(entryInstruction.ApprovalCertificateInfos.Where(x => x.CSI_Code.Equals(ApprovalCertificateInfoCodes.AEOM)).Any());
			});
		}

		public void TestCheckCEI_Style()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var info = entryInstruction.CEI_StyleInfo;

			entryInstruction.CEI_Style = "Z";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			AssertNoNotifications(info);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			AssertHasMessageError(info, "Declaration Type \"Y\" must be only selected for AIR freight jobs");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
			AssertNoNotifications(info);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = "Z";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(info);
		}

		public void TestCheckCEI_Style_IsForMarineProductsExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingDataGrouping("JP");
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			refCusCodeList.Attributes.DeleteAll();
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Type, "洋上");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_CustomsRegNo = "2HDN8";
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			declaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var expectedErrorMessage = "Declaration Type must be E when Customs Depot is 洋上.";
			var targetInfo = entryInstruction.CEI_StyleInfo;

			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.E;
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_Style_InvoiceLineCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			var entryInstructionValidation = entryInstruction.Validation;
			var expectedErrorMessage = "It is required to include one and only one invoice line when the Declaration Type is Y. You have included";
			var targetInfo = entryInstruction.CEI_StyleInfo;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			entryInstructionValidation.ValidateCEI_Style();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			entryInstructionValidation.ValidateCEI_Style();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_BeforePermitApplicationReason()
		{
			declaration.JE_MessageType = "IMP";
			cusEntryInstruction.CEI_BeforePermitApplicationReason = "XX";
			AssertHasMessageErrorContaining("CEI_BeforePermitApplicationReason - Import", cusEntryInstruction.CEI_BeforePermitApplicationReasonInfo, ListValidation.InvalidCodeMessageError.ToString());

			cusEntryInstruction.CEI_BeforePermitApplicationReason = BeforePermitApplicationReasonList.Codes.B3;
			AssertNoMessageErrors("CEI_BeforePermitApplicationReason IMP", cusEntryInstruction.CEI_BeforePermitApplicationReasonInfo);

			declaration.JE_MessageType = "EXP";
			cusEntryInstruction.CEI_BeforePermitApplicationReason = "XX";
			AssertNoMessageErrors("CEI_BeforePermitApplicationReason - Export should not validate", cusEntryInstruction.CEI_BeforePermitApplicationReasonInfo);
		}

		public void TestCEI_CommonControlNumber()
		{
			var expectedErrorMessage = string.Format("Please do not enter {0} when there are multiple B/L.", cusEntryInstruction.CEI_CommonControlNumberInfo.HumanReadableName);

			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();
			var bill3 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			cusEntryInstruction.CEI_CommonControlNumber = "001";
			AssertHasMessageError(cusEntryInstruction.CEI_CommonControlNumberInfo, expectedErrorMessage);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			cusEntryInstruction.Validation.ValidateCEI_CommonControlNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_CommonControlNumberInfo, expectedErrorMessage);

			cusEntryInstruction.CEI_CommonControlNumber = string.Empty;
			AssertNoMessageError(cusEntryInstruction.CEI_CommonControlNumberInfo, expectedErrorMessage);

			declaration.Bills.RemoveAndDelete(bill3);
			cusEntryInstruction.CEI_CommonControlNumber = "001";
			AssertNoMessageError(cusEntryInstruction.CEI_CommonControlNumberInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_CustomsOfficeForSpecialDeclarations()
		{
			CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1Y");
			var expectedMessage = "The entered customs office does not exist.";

			cusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarations = "1Y";
			AssertNoMessageError(cusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarationsInfo, expectedMessage);

			cusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarations = "1Z";
			AssertHasMessageError(cusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarationsInfo, expectedMessage);
		}

		public void TestCheckCEI_CustomsOfficeDepartmentForSpecialDeclarations()
		{
			CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1Y");
			CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCustomsOfficeDepartment, "1Y01");
			var expectedMessage = "The entered customs office department does not exist.";

			cusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarations = "1Y";
			cusEntryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarations = "01";
			AssertNoMessageError(cusEntryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarationsInfo, expectedMessage);

			cusEntryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarations = "02";
			AssertHasMessageError(cusEntryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarationsInfo, expectedMessage);
		}

		public void TestCheckCEI_TradeControlOrder()
		{
			AssertNoNotifications(cusEntryInstruction.CEI_TradeControlOrderInfo);
			cusEntryInstruction.CEI_TradeControlOrder = "!";
			AssertHasMessageError(cusEntryInstruction.CEI_TradeControlOrderInfo, "The value entered is invalid. Please select a value from the list.");
			cusEntryInstruction.CEI_TradeControlOrder = ImportTradeControlOrdinanceArticle3CodeList.Codes.W;
			AssertNoNotifications(cusEntryInstruction.CEI_TradeControlOrderInfo);
		}

		public void TestCEI_BondedLocationCode()
		{
			CombineAssertions(() =>
			{
				var refCusCodeList = CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode);

				var bondedWarehouse = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				CodeDescriptionPairList declarationTypes = new InbondDeclarationTypeList();
				for (var i = 0; i < declarationTypes.Count; i++)
				{
					cusEntryInstruction.CEI_Style = declarationTypes[i].Code;
					cusEntryInstruction.Validation.ValidateCEI_BondedLocationCode();
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertHasMessageErrorContaining($"Mandatory when declaration type is {cusEntryInstruction.CEI_Style}", cusEntryInstruction.CEI_BondedLocationCodeInfo, MandatoryValidation.YouHaveNotEntered);
				}

				declarationTypes = new BondedImportDeclarationTypeList();
				declarationTypes.AddPair(JPImportDeclarationTypeList.Codes.R, JPImportDeclarationTypeList.Descriptions.R);
				for (var i = 0; i < declarationTypes.Count; i++)
				{
					cusEntryInstruction.CEI_Style = declarationTypes[i].Code;
					cusEntryInstruction.CEI_BondedLocationCode = "JPBLC";
					cusEntryInstruction.Validation.ValidateCEI_BondedLocationCode();
					AssertHasMessageErrorContaining($"Not required when declaration type is {cusEntryInstruction.CEI_Style}", cusEntryInstruction.CEI_BondedLocationCodeInfo, "is not required");

					cusEntryInstruction.CEI_BondedLocationCode = "";
					cusEntryInstruction.Validation.ValidateCEI_BondedLocationCode();
					AssertNoMessageErrorContaining($"Not required when declaration type is {cusEntryInstruction.CEI_Style}", cusEntryInstruction.CEI_BondedLocationCodeInfo, "is not required");
				}

				cusEntryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.P;
				cusEntryInstruction.CEI_BondedLocationCode = "BLC";
				AssertHasMessageErrorContaining($"Message for invalid value", cusEntryInstruction.CEI_BondedLocationCodeInfo, "invalid");

				cusEntryInstruction.CEI_BondedLocationCode = "99999";
				AssertNoMessageErrorContaining($"No message for placeholder", cusEntryInstruction.CEI_BondedLocationCodeInfo, "invalid");

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.WarehouseDocAddress.OrganisationPK = bondedWarehouse.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = bondedWarehouse.MainAddress.PK;
				AssertNoMessageError("Inconsistent with organization's CCP code", cusEntryInstruction.CEI_BondedLocationCodeInfo, "The Bonded Warehouse organization CCP location code does not match the value entered.");

				bondedWarehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "JPBLC", Core.Constants.CountryCodes.Japan);
				cusEntryInstruction.Validation.ValidateCEI_BondedLocationCode();
				AssertHasMessageError("Inconsistent with organization's CCP code", cusEntryInstruction.CEI_BondedLocationCodeInfo, "The Bonded Warehouse organization CCP location code does not match the value entered.");

				cusEntryInstruction.CEI_BondedLocationCode = refCusCodeList.ZZD_Code;
				AssertNoMessageErrorContaining($"Valid value", cusEntryInstruction.CEI_BondedLocationCodeInfo, "invalid");
			});
		}

		public void TestCEI_BondedLocationName()
		{
			CombineAssertions(() =>
			{
				cusEntryInstruction.CEI_BondedLocationCode = "99999";
				Assert("Bonded location name can be checked only when editable", !cusEntryInstruction.CEI_BondedLocationNameInfo.ReadOnly);
				AssertHasMessageErrorContaining(cusEntryInstruction.CEI_BondedLocationNameInfo, MandatoryValidation.YouHaveNotEntered);

				cusEntryInstruction.CEI_BondedLocationName = "XXX";
				AssertNoNotifications(cusEntryInstruction.CEI_BondedLocationNameInfo);

				cusEntryInstruction.CEI_BondedLocationCode = "9999";
				cusEntryInstruction.CEI_BondedLocationName = "";
				AssertNoNotifications(cusEntryInstruction.CEI_BondedLocationNameInfo);
				Assert("Bonded location name can be checked only when editable", cusEntryInstruction.CEI_BondedLocationNameInfo.ReadOnly);
			});
		}

		public void TestCheckCEI_DeclarationCondition()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			cusEntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.CEI_DeclarationConditionInfo, "X", "I");
		}

		public void TestCheckCEI_ApprovalCertificateCategory()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.CEI_ApprovalCertificateCategoryInfo, "XX", "E1");
		}

		public void TestCheckCEI_GrossWeight()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(cusEntryInstruction.CEI_GrossWeightInfo);

			cusEntryInstruction.Validation.ValidateCEI_GrossWeight();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);

			cusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusEntryInstruction.CEI_GrossWeightInfo);
		}

		public void TestCheckCEI_GrossWeightImportMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var targetInfo = entryInstruction.CEI_GrossWeightInfo;
			void AssertGrossWeight()
			{
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				entryInstruction.CEI_GrossWeight = ZDecimal.Zero;
				foreach (var mailDeclarationCargoType in new[]
				{
					MailedCargoList.Codes.E,
					MailedCargoList.Codes.M,
					MailedCargoList.Codes.U,
					MailedCargoList.Codes.H
				})
				{
					entryInstruction.CEI_DeclarationCargoType = mailDeclarationCargoType;
					entryInstruction.Validation.ValidateCEI_GrossWeight();
					if (!entryInstruction.IsBondedImportDeclarationType)
					{
						AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
					}
					else
					{
						AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
					}
				}

				entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.S;
				entryInstruction.Validation.ValidateCEI_GrossWeight();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedDate = ZDateTime.Now;
				entryInstruction.Validation.ValidateCEI_GrossWeight();
				if (!entryInstruction.IsBondedImportDeclarationType)
				{
					AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				}
				else
				{
					AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				}

				entryInstruction.CEI_GrossWeight = 1m;
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			}

			foreach (var declarationType in new[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.Y,
				JPImportDeclarationTypeList.Codes.H,
				JPImportDeclarationTypeList.Codes.N,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.T,
				JPImportDeclarationTypeList.Codes.V,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
				JPImportDeclarationTypeList.Codes.K,
				JPImportDeclarationTypeList.Codes.D,
				JPImportDeclarationTypeList.Codes.U,
				JPImportDeclarationTypeList.Codes.L,
				JPImportDeclarationTypeList.Codes.B,
				JPImportDeclarationTypeList.Codes.E,
				JPImportDeclarationTypeList.Codes.R,
			})
			{
				entryInstruction.CEI_Style = declarationType;
				AssertGrossWeight();
			}
		}

		public void TestCheckCEI_CargoQuantity()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(cusEntryInstruction.CEI_CargoQuantityInfo);

			cusEntryInstruction.Validation.ValidateCEI_CargoQuantity();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_CargoQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			cusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusEntryInstruction.CEI_CargoQuantityInfo);
		}

		public void TestCheckCEI_CargoQuantityImportMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var targetInfo = entryInstruction.CEI_CargoQuantityInfo;
			void AssertCargoQuantity()
			{
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				entryInstruction.CEI_CargoQuantity = ZDecimal.Zero;
				foreach (var mailDeclarationCargoType in new[]
				{
					MailedCargoList.Codes.E,
					MailedCargoList.Codes.M,
					MailedCargoList.Codes.U,
					MailedCargoList.Codes.H
				})
				{
					entryInstruction.CEI_DeclarationCargoType = mailDeclarationCargoType;
					entryInstruction.Validation.ValidateCEI_CargoQuantity();
					if (!entryInstruction.IsBondedImportDeclarationType)
					{
						AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
					}
					else
					{
						AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
					}
				}

				entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.S;
				entryInstruction.Validation.ValidateCEI_CargoQuantity();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedDate = ZDateTime.Now;
				entryInstruction.Validation.ValidateCEI_CargoQuantity();
				if (!entryInstruction.IsBondedImportDeclarationType)
				{
					AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				}
				else
				{
					AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				}

				entryInstruction.CEI_CargoQuantity = 1m;
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			}

			foreach (var declarationType in new[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.Y,
				JPImportDeclarationTypeList.Codes.H,
				JPImportDeclarationTypeList.Codes.N,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.T,
				JPImportDeclarationTypeList.Codes.V,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
				JPImportDeclarationTypeList.Codes.K,
				JPImportDeclarationTypeList.Codes.D,
				JPImportDeclarationTypeList.Codes.U,
				JPImportDeclarationTypeList.Codes.L,
				JPImportDeclarationTypeList.Codes.B,
				JPImportDeclarationTypeList.Codes.E,
				JPImportDeclarationTypeList.Codes.R,
			})
			{
				entryInstruction.CEI_Style = declarationType;
				AssertCargoQuantity();
			}
		}

		public void TestCheckCEI_GrossWeightUnit()
		{
			cusEntryInstruction.CEI_GrossWeight = 10m;
			cusEntryInstruction.CEI_GrossWeightUnit = "";
			AssertHasMessageError(cusEntryInstruction.CEI_GrossWeightUnitInfo, "You have not entered Gross Weight Unit.");
			cusEntryInstruction.CEI_GrossWeight = 0m;
			cusEntryInstruction.CEI_GrossWeightUnit = "XY";
			AssertHasMessageError(cusEntryInstruction.CEI_GrossWeightUnitInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCEI_CargoQuantityUnit()
		{
			var targetInfo = cusEntryInstruction.CEI_CargoQuantityUnitInfo;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			cusEntryInstruction.CEI_CargoQuantity = 20m;
			cusEntryInstruction.CEI_CargoQuantityUnit = "XY";
			AssertHasMessageError(targetInfo, "The code you have selected is not in the list.");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			cusEntryInstruction.Validation.ValidateCEI_CargoQuantityUnit();
			AssertNoMessageError(targetInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckEntryInstructionCEI_GrossWeightUnit()
		{
			cusEntryInstruction.CEI_GrossWeight = 10;
			cusEntryInstruction.CEI_GrossWeightUnit = "";
			AssertHasMessageError(cusEntryInstruction.CEI_GrossWeightUnitInfo, "You have not entered Gross Weight Unit.");

			cusEntryInstruction.CEI_GrossWeight = 0;
			cusEntryInstruction.CEI_GrossWeightUnit = "XY";
			AssertHasMessageError(cusEntryInstruction.CEI_GrossWeightUnitInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckEntryInstructionCEI_CargoQuantityUnit()
		{
			var targetInfo = cusEntryInstruction.CEI_CargoQuantityUnitInfo;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			cusEntryInstruction.CEI_CargoQuantity = 10;
			cusEntryInstruction.CEI_CargoQuantityUnit = "";
			AssertHasMessageError(targetInfo, "You have not entered Cargo Quantity Unit.");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			cusEntryInstruction.Validation.ValidateCEI_CargoQuantityUnit();
			AssertNoMessageError(targetInfo, "You have not entered Cargo Quantity Unit.");

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			cusEntryInstruction.CEI_CargoQuantity = 0;
			cusEntryInstruction.CEI_CargoQuantityUnit = "XY";
			AssertHasMessageError(targetInfo, "The code you have selected is not in the list.");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			cusEntryInstruction.Validation.ValidateCEI_CargoQuantityUnit();
			AssertNoMessageError(targetInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckSpecialCargoCode()
		{
			CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG");
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.CEI_SpecialCargoCodeInfo, "XXX", "AOG");
		}

		public void TestCheckCEI_Volumn()
		{
			cusEntryInstruction.Validation.ValidateCEI_Volume();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_VolumeInfo, MandatoryValidation.YouHaveNotEntered);

			cusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusEntryInstruction.CEI_VolumeInfo);
		}

		public void TestCheckCEI_VolumeUnit()
		{
			cusEntryInstruction.CEI_Volume = 10m;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(cusEntryInstruction.CEI_VolumeUnitInfo, "XX", "CC");
		}

		public void TestCheckCEI_CustomsWeight()
		{
			var targetInfo = cusEntryInstruction.CEI_CustomsWeightInfo;

			cusEntryInstruction.CEI_GrossWeightUnit = Weight.Kilograms;
			cusEntryInstruction.CEI_GrossWeight = 1000000;
			AssertHasMessageError(targetInfo, "The maximum Customs Weight allowed by NACCS is 999999.999. Please consider using T as the Gross Weight Unit.");

			cusEntryInstruction.CEI_GrossWeightUnit = Weight.Tonnes;
			cusEntryInstruction.CEI_GrossWeight = 1000001;
			AssertHasMessageError(targetInfo, "The maximum value allowed by NACCS is 999999.999.");

			cusEntryInstruction.CEI_GrossWeightUnit = Weight.Tonnes;
			cusEntryInstruction.CEI_GrossWeight = 999999;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckCEI_CustomsVolume()
		{
			var targetInfo = cusEntryInstruction.CEI_CustomsVolumeInfo;
			var validation = cusEntryInstruction.Validation;
			cusEntryInstruction.CEI_Volume = 1000000m;
			cusEntryInstruction.CEI_VolumeUnit = Volume.CubicMetres;
			validation.ValidateAll();
			AssertHasMessageError(targetInfo, "Value exceeds the upper limit.");

			cusEntryInstruction.CEI_Volume = 999999m;
			validation.ValidateAll();
			AssertNoMessageError(targetInfo, "Value exceeds the upper limit.");
		}

		public void TestCheckCEI_ContainerCount()
		{
			var error = "Container Count cannot be negative.";
			var warningMessage = "The entered container count is different from the actual number of containers associated with this entry instruction through Inv. Lines > Containers, which is";

			var targetInfo = cusEntryInstruction.CEI_ContainerCountInfo;
			cusEntryInstruction.CEI_ContainerCount = -1;
			AssertHasError(targetInfo, error);
			cusEntryInstruction.CEI_ContainerCount = 2;
			AssertNoError(targetInfo, error);
			AssertNoWarningContaining("There is no invoice line associated with the entry instruction", targetInfo, warningMessage);

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			cusEntryInstruction.Validation.ValidateCEI_ContainerCount();
			AssertNoWarningContaining("There is one invoice line associated with the entry instruction, no container associated with the entry instruction", targetInfo, warningMessage);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			cusEntryInstruction.Validation.ValidateCEI_ContainerCount();
			AssertHasWarningContaining("There is one invoice line associated with the entry instruction, one container associated with the entry instruction", targetInfo, warningMessage);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			cusEntryInstruction.Validation.ValidateCEI_ContainerCount();
			AssertNoWarningContaining("There is one invoice line associated with the entry instruction, two containers associated with the entry instruction", targetInfo, warningMessage);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine = true;
			cusEntryInstruction.Validation.ValidateCEI_ContainerCount();
			AssertHasWarningContaining("There is one invoice line associated with the entry instruction, one container associated with the entry instruction", targetInfo, warningMessage);
		}

		public void TestCheckCEI_ApprovalCertificateCategoryWithInvoiceLineIsAppendixTable()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "Japan Export Trade Control Ordinance Appendix");

			var refCusCodeList1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "10418", startDate, endDate);
			refCusCodeList1.ZZD_Description = "別表第１*4－(18)";

			var refCusCodeList2 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "23053", startDate, endDate);
			refCusCodeList2.ZZD_Description = "別表第２の３（第２号（汎用品等））*44";

			var refCusCodeList3 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "50010", startDate, endDate);
			refCusCodeList3.ZZD_Description = "別表第５（輸出令第４条第２項第２号関係）*5 － 1";
			Factory.Save();

			var noInvoiceLineIsAppendixTable1 = "There is no invoice line that is for goods listed in trade control order appendix table 1.";
			var invoiceLineIsAppendixTable1 = "There are invoice lines that are for goods listed in trade control order appendix table 1.";
			var noInvoiceLineIsAppendixTable2 = "There is no invoice line that is for goods listed in trade control order appendix table 2.";
			var invoiceLineIsAppendixTable2 = "There are invoice lines that are for goods listed in trade control order appendix table 2.";

			AssertCheckCEI_ApprovalCertificateCategoryWithInvoiceLineIsAppendixTable(noInvoiceLineIsAppendixTable1, invoiceLineIsAppendixTable1, "10418", new string[] { "FE" }, new string[] { "E1", "E2", "N1", "N2", "N3", "NO" });
			AssertCheckCEI_ApprovalCertificateCategoryWithInvoiceLineIsAppendixTable(noInvoiceLineIsAppendixTable2, invoiceLineIsAppendixTable2, "23053", new string[] { "E1", "E2", }, new string[] { "N1", "N2", "N3", "NO" });
		}

		void AssertCheckCEI_ApprovalCertificateCategoryWithInvoiceLineIsAppendixTable(string noInvoiceLineIsAppendixTable, string invoiceLineIsAppendixTable, string appendix, string[] codesForNoInvoiceLineInAppendixTable, string[] codesForInvoiceLineInAppendixTable)
		{
			var targetInfo = cusEntryInstruction.CEI_ApprovalCertificateCategoryInfo;
			var validation = cusEntryInstruction.Validation;
			var invoiceLine1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cusEntryInstruction.PK;
			var invoiceLine2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cusEntryInstruction.PK;

			foreach (var code in codesForNoInvoiceLineInAppendixTable)
			{
				invoiceLine1.JI_TradeControlOrderAppendix = ZString.Empty;
				invoiceLine2.JI_TradeControlOrderAppendix = ZString.Empty;
				cusEntryInstruction.CEI_ApprovalCertificateCategory = ZString.Empty;
				AssertNoMessageError("All JI_TradeControlOrderAppendix are empty, CEI_ApprovalCertificateCategory is Empty.", targetInfo, noInvoiceLineIsAppendixTable);

				cusEntryInstruction.CEI_ApprovalCertificateCategory = code;
				AssertHasMessageError($"All JI_TradeControlOrderAppendix are empty, CEI_ApprovalCertificateCategory is {code}.", targetInfo, noInvoiceLineIsAppendixTable);

				invoiceLine1.JI_TradeControlOrderAppendix = appendix;
				invoiceLine2.JI_TradeControlOrderAppendix = "50010";
				validation.ValidateCEI_ApprovalCertificateCategory();
				AssertNoMessageError($"One of JI_TradeControlOrderAppendix is {appendix}, CEI_ApprovalCertificateCategory is {code}.", targetInfo, noInvoiceLineIsAppendixTable);
			}

			foreach (var code in codesForInvoiceLineInAppendixTable)
			{
				invoiceLine1.JI_TradeControlOrderAppendix = ZString.Empty;
				invoiceLine2.JI_TradeControlOrderAppendix = ZString.Empty;
				cusEntryInstruction.CEI_ApprovalCertificateCategory = code;
				AssertNoMessageError($"All JI_TradeControlOrderAppendix are empty, CEI_ApprovalCertificateCategory is {code}.", targetInfo, invoiceLineIsAppendixTable);

				invoiceLine1.JI_TradeControlOrderAppendix = appendix;
				invoiceLine2.JI_TradeControlOrderAppendix = "50010";
				validation.ValidateCEI_ApprovalCertificateCategory();
				AssertHasMessageError($"One of JI_TradeControlOrderAppendix is {appendix}, CEI_ApprovalCertificateCategory is {code}.", targetInfo, invoiceLineIsAppendixTable);
			}
		}

		public void TestCheckCEI_ApprovalCertificateCategoryEnteredExportNumber()
		{
			var targetInfo = cusEntryInstruction.CEI_ApprovalCertificateCategoryInfo;
			var validation = cusEntryInstruction.Validation;
			var approvalCertificateInfos = cusEntryInstruction.ApprovalCertificateInfos.AddNew();
			AssertCheckCEI_ApprovalCertificateCategoryEnteredExportNumber("You have not entered an export permit number.", new[] { "FE", "FT" }, new[] { "FENJ", "FENO", "FTNO" });
			AssertCheckCEI_ApprovalCertificateCategoryEnteredExportNumber("You have not entered an export approval number.", new[] { "E1", "E2" }, new[] { "ELNJ", "ELNO" });

			void AssertCheckCEI_ApprovalCertificateCategoryEnteredExportNumber(string errorMessage, string[] approvalCertificateCategorys, string[] approvalCertificateCodes)
			{
				foreach (var approvalCertificateCategory in approvalCertificateCategorys)
				{
					cusEntryInstruction.CEI_ApprovalCertificateCategory = approvalCertificateCategory;
					approvalCertificateInfos.CSI_Code = ZString.Empty;
					validation.ValidateCEI_ApprovalCertificateCategory();
					AssertHasMessageError($"CEI_ApprovalCertificateCategory is {approvalCertificateCategory}, CSI_Code is empty.", targetInfo, errorMessage);

					foreach (var code in approvalCertificateCodes)
					{
						approvalCertificateInfos.CSI_Code = code;
						validation.ValidateCEI_ApprovalCertificateCategory();
						AssertNoMessageError($"CEI_ApprovalCertificateCategory is {approvalCertificateCategory}, CSI_Code is {code}.", targetInfo, errorMessage);
					}

					approvalCertificateInfos.CSI_Code = "ITNO";
					validation.ValidateCEI_ApprovalCertificateCategory();
					AssertHasMessageError($"CEI_ApprovalCertificateCategory is {approvalCertificateCategory}, CSI_Code is ITNO.", targetInfo, errorMessage);
				}
			}
		}

		public void TestCheckCEI_GoodsDescription()
		{
			cusEntryInstruction.Validation.ValidateCEI_GoodsDescription();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			cusEntryInstruction.SetupECRTestingContext();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusEntryInstruction.CEI_GoodsDescriptionInfo);
		}

		public void TestValidateAgainstLinkedEntryHeaders()
		{
			var expectedMessage = "The number of entry lines merged from invoice lines exceeds the upper limit of a declaration message. Please confirm Misc. > Merged By, and Tariff, Custom Quantity and Additional Details of invoice lines. Please then Generate Entries (Merge) again to reflect changes.";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			for (var i = 0; i < 99; i++)
			{
				entryHeader.MergedLines.AddNew();
			}
			cusEntryInstruction.Validation.ValidateAgainstLinkedEntryHeaders();
			AssertNoRowMessageError(cusEntryInstruction, expectedMessage);

			entryHeader.MergedLines.AddNew();
			cusEntryInstruction.Validation.ValidateAgainstLinkedEntryHeaders();
			AssertHasRowMessageError(cusEntryInstruction, expectedMessage);
		}

		public void TestCheckRCR_RegisterMandatoryInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var expectedMessageError = "cannot be empty.";
			instruction1.CEI_RCRAction = string.Empty;
			instruction2.CEI_RCRAction = RCRActionList.Codes.Nine;

			instruction1.Validation.ValidateReceiptMode();
			AssertNoMessageError(instruction1.ReceiptModeInfo, expectedMessageError);
			instruction2.Validation.ValidateReceiptMode();
			AssertHasMessageErrorContaining(instruction2.ReceiptModeInfo, expectedMessageError);

			instruction1.Validation.ValidateFinalDestination();
			AssertNoMessageError(instruction1.FinalDestinationInfo, expectedMessageError);
			instruction2.Validation.ValidateFinalDestination();
			AssertHasMessageErrorContaining(instruction2.FinalDestinationInfo, expectedMessageError);

			instruction1.Validation.ValidateCEI_PreviousBillNumber();
			AssertNoMessageError(instruction1.CEI_PreviousBillNumberInfo, expectedMessageError);
			instruction2.Validation.ValidateCEI_PreviousBillNumber();
			AssertHasMessageErrorContaining(instruction2.CEI_PreviousBillNumberInfo, expectedMessageError);
		}

		public void TestCheckRCR_DeleteMandatoryInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var expectedMessageError = "cannot be empty.";
			instruction1.CEI_RCRAction = string.Empty;
			instruction2.CEI_RCRAction = RCRActionList.Codes.One;

			instruction1.Validation.ValidateExportControlNumber();
			AssertNoMessageError(instruction1.ExportControlNumberInfo, expectedMessageError);
			instruction2.Validation.ValidateExportControlNumber();
			AssertHasMessageErrorContaining(instruction2.ExportControlNumberInfo, expectedMessageError);
		}

		public void TestCheckCEI_RCRAction()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			cusEntryInstruction.CEI_RCRAction = string.Empty;
			cusEntryInstruction.Validation.ValidateCEI_RCRAction();
			AssertNoMessageError(cusEntryInstruction.CEI_RCRActionInfo, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_RCRAction = "A";
			cusEntryInstruction.Validation.ValidateCEI_RCRAction();
			AssertHasMessageError(cusEntryInstruction.CEI_RCRActionInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_ViaLocation()
		{
			var validCode = Factory.CreateJapanBondedAreaCode();
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.CEI_ViaLocationInfo, "XXX", validCode);
		}

		public void TestCheckMoveInNotice()
		{
			var expectedMessage = "Only capital letters and digits are allowed with the first 3 characters must be capitalized.";
			var targetInfo = cusEntryInstruction.MoveInNoticeInfo;
			cusEntryInstruction.MoveInNotice = "AB1";
			AssertHasMessageError(targetInfo, expectedMessage);

			cusEntryInstruction.MoveInNotice = "ABc";
			AssertHasMessageError(targetInfo, expectedMessage);

			cusEntryInstruction.MoveInNotice = "ABC";
			AssertNoMessageError(targetInfo, expectedMessage);

			cusEntryInstruction.MoveInNotice = "ABC*";
			AssertHasMessageError(targetInfo, expectedMessage);

			cusEntryInstruction.RequestMoveInNotice = true;
			cusEntryInstruction.MoveInNotice = "AB1";
			AssertNoMessageError(targetInfo, expectedMessage);
		}

		public void TestCheckCEI_BillNumber()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusEntryInstruction.CEI_BillNumberInfo);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			cusEntryInstruction.CEI_BillNumber = ZString.Empty;
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_BillNumber_Format()
		{
			var message = "AWB Number is not in the correct format. It should start with Master Bill or House Bill, and end with an extension number (/NN for export) if applicable.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "M123456789";
			declaration.JE_HouseBill = "H987654321";

			cusEntryInstruction.CEI_BillNumber = "M123456789A";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			cusEntryInstruction.CEI_BillNumber = "H987654321AA";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			cusEntryInstruction.CEI_BillNumber = "M1234567891/11";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			cusEntryInstruction.CEI_BillNumber = "H987654321/11";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			cusEntryInstruction.CEI_BillNumber = "MK123456789";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			cusEntryInstruction.CEI_BillNumber = "HK987654321";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			declaration.JE_MasterBill = "M";
			declaration.JE_HouseBill = "H";
			cusEntryInstruction.CEI_BillNumber = "M";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertHasMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "M123456789";
			declaration.JE_HouseBill = "H987654321";
			cusEntryInstruction.CEI_BillNumber = "M123456789A";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			cusEntryInstruction.CEI_BillNumber = "M123456789A";
			cusEntryInstruction.Validation.ValidateCEI_BillNumber();
			AssertNoMessageError(cusEntryInstruction.CEI_BillNumberInfo, message);
		}

		#region Implementation

		CusEntryInstruction cusEntryInstruction;
		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		}

		RefCusCodeList CreateRefDataForTest(string codeTypeValue, string code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingValue = Core.Constants.CountryCodes.Japan;

			helper.CreateNewOrGetExistingDataGrouping(dataGroupingValue);
			helper.CreateNewOrGetExistingCusCodeType(codeTypeValue, "Testdescription");
			var result = helper.CreateNewOrGetExistingCusCodeList(dataGroupingValue, codeTypeValue, code, "TestDescription", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			Factory.Save();

			return result;
		}

		#endregion
	}
}
