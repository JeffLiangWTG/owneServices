using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TransactionNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAccountSecurityCode()
		{
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var messageError = "Account Security Number is not specified properly.\r\nPlease follow Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> ASEC Number and specify 5 digit code.";
			tranNumber.SetAccountSecurityNo();
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
			AssertHasError(messageError, tranNumber.AccountSecurityCodeInfo, "123", "00000", ZString.Empty);
			AssertNoError(messageError, tranNumber.AccountSecurityCodeInfo, "54321", "12005", "00125", "12340");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			tranNumber.Validation.ValidateAccountSecurityCode();
			AssertNoError(messageError, tranNumber.AccountSecurityCodeInfo, "123", "00000", ZString.Empty);
		}

		[UseSnapshotProtection]
		public void TestCheckSequentialNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var formatMessageError = "Sequential Number should be composed of 8 digits.";
				AssertHasError(formatMessageError, tranNumber.SequentialNumberInfo, "123AB678");
				AssertNoError(formatMessageError, tranNumber.SequentialNumberInfo, "12345678", "00001234", "12340012");

				declaration.Importer.CustomsCodes.RemoveAndDeleteAll();
				tranNumber.SetAccountSecurityNo();
				AssertNoError(formatMessageError, tranNumber.SequentialNumberInfo, "00000000", ZString.Empty);

				var uniqueMessageError = "The Transaction Number is already used in B00001000. Please use a different number.";
				tranNumber.AccountSecurityCode = "12345";
				tranNumber.SequentialNumber = "10012345";
				AssertNoError(tranNumber.SequentialNumberInfo, uniqueMessageError);
				Factory.Save();

				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration1.TransactionNumber.AccountSecurityCode = tranNumber.AccountSecurityCode;
				declaration1.TransactionNumber.SequentialNumber = tranNumber.SequentialNumber;

				AssertHasError(declaration1.TransactionNumber.SequentialNumberInfo, uniqueMessageError);

				var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey("12345" + declaration.Branch.PK + "IMP"));
				fountain.SetValues(Factory, minValue: 1, nextValue: 10000, maxValue: 10000);

				declaration1.TransactionNumber.SequentialNumber = ZString.Empty;
				AssertNoErrors(declaration1.TransactionNumber.SequentialNumberInfo);

				Factory.Save();

				AssertEquals("10012345", tranNumber.SequentialNumber);
				AssertEquals("00010000", declaration1.TransactionNumber.SequentialNumber);
			}
		}

		public void TestCheckSequentialNumber_RangeValidation_SeparateTransactionNumberFields()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				tranNumber.AccountSecurityCode = "12345";
				Factory.Save();

				AssertEquals(tranNumber.SequentialNumber, "00000001");
				AssertNoError(tranNumber.SequentialNumberInfo, "Sequential Number should be between 10000000 and 19999999.");
				Factory.Save();

				// range validation should not be applied to already saved declarations
				tranNumber.RunPreSaveValidation();
				AssertEquals(tranNumber.SequentialNumber, "00000001");
				AssertNoError(tranNumber.SequentialNumberInfo, "Sequential Number should be between 10000000 and 19999999.");

				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345", 10000000, 19999999);
				tranNumber.SequentialNumber = "9999";
				AssertHasError(tranNumber.SequentialNumberInfo, "Sequential Number should be between 10000000 and 19999999.");

				tranNumber.SequentialNumber = "10000000";
				AssertNoErrors(tranNumber.SequentialNumberInfo);

				tranNumber.SequentialNumber = "14567000";
				AssertNoErrors(tranNumber.SequentialNumberInfo);

				tranNumber.SequentialNumber = "19999999";
				AssertNoErrors(tranNumber.SequentialNumberInfo);

				tranNumber.SequentialNumber = "20000000";
				AssertHasError(tranNumber.SequentialNumberInfo, "Sequential Number should be between 10000000 and 19999999.");
			}
		}

		public void TestCheckSequentialNumber_RangeValidation_CombinedTransactionNumberFields()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				tranNumber.AccountSecurityCode = "12345";
				Factory.Save();

				AssertEquals(tranNumber.FormattedTransactionNumber, "12345000000012");
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Sequential Number should be between 10000000 and 19999999.");

				Factory.Save();

				// range validation should not be applied to already saved declarations
				tranNumber.RunPreSaveValidation();
				AssertEquals(tranNumber.FormattedTransactionNumber, "12345000000012");
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Sequential Number should be between 10000000 and 19999999.");

				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345", 10000000, 19999999);
				tranNumber.FormattedTransactionNumber = "12345000099997";
				AssertHasError(tranNumber.FormattedTransactionNumberInfo, "Sequential Number should be between 10000000 and 19999999.");

				tranNumber.FormattedTransactionNumber = "12345100100004";
				AssertNoErrors(tranNumber.FormattedTransactionNumberInfo);

				tranNumber.FormattedTransactionNumber = "12345100145677";
				AssertNoErrors(tranNumber.FormattedTransactionNumberInfo);

				tranNumber.FormattedTransactionNumber = "12345100199990";
				AssertNoErrors(tranNumber.FormattedTransactionNumberInfo);

				tranNumber.FormattedTransactionNumber = "12345299999999";
				AssertHasError(tranNumber.FormattedTransactionNumberInfo, "Sequential Number should be between 10000000 and 19999999.");
			}
		}

		public void TestCheckSequentialNumber_WHSTransactionExists_10()
		{
			CheckSequentialNumber_WHSTransactionExists(B3EntryTypeList.Codes.Warehouse10, false);
		}

		public void TestCheckSequentialNumber_WHSTransactionExists_101()
		{
			CheckSequentialNumber_WHSTransactionExists(CADEntryTypeList.Codes.Warehouse101, true);
		}

		public void TestCheckSequentialNumber_WHSTransactionExists_102()
		{
			CheckSequentialNumber_WHSTransactionExists(CADEntryTypeList.Codes.Warehouse102, true);
		}

		void CheckSequentialNumber_WHSTransactionExists(string messageSubType, bool isCADEnabled)
		{
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var helper = new WhsDataTestHelper(Factory);
			var declaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B00000001", "00000001", messageSubType, helper.Importer, helper.Warehouse);
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = isCADEnabled ? MessageTypeList.Codes.CommercialAccountingDeclaration : MessageTypeList.Codes.B3CUSDEC;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
			Factory.Save();
			declaration.TransactionNumber.SequentialNumber = "11111111";
			AssertNoError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
			Factory.Save();
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			declaration.TransactionNumber.SequentialNumber = "99999999";
			AssertHasError(declaration.TransactionNumber.SequentialNumberInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
			declaration.TransactionNumber.SequentialNumber = "11111111";
			AssertNoError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckFormattedTransactionNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				tranNumber.FormattedTransactionNumber = "XXXX";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertHasError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number should be composed of 14 digits numbers.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number should be composed of 14 digits numbers.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				tranNumber.FormattedTransactionNumber = "54321000000001";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number should be composed of 14 digits numbers.");
				AssertHasError(tranNumber.FormattedTransactionNumberInfo, "The first 5 digits should be the Account Security Code '12345'.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "The first 5 digits should be the Account Security Code '12345'.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				tranNumber.FormattedTransactionNumber = "12345000000001";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "The first 5 digits should be the Account Security Code '12345'.");
				AssertHasError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number cannot end with all zeros.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number cannot end with all zeros.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				tranNumber.FormattedTransactionNumber = "12345100000011";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number cannot end with all zeros.");
				AssertHasErrorContaining(tranNumber.FormattedTransactionNumberInfo, "Transaction Number does not have a valid check (last) digit. The check digit should be 4.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number does not have a valid check (last) digit. The check digit should be 4.");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				tranNumber.FormattedTransactionNumber = "12345100000014";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoErrorContaining(tranNumber.FormattedTransactionNumberInfo, "Transaction Number does not have a valid check (last) digit. The check digit should be 4.");

				var newFactory = new BusinessObjectFactory();
				var newJob = newFactory.New<JobDeclaration>();
				newJob.JE_DeclarationReference = "B12345678";
				newJob.JE_MessageType = JobMessageTypeList.Codes.Import;
				CusEntryNumber.New(newJob, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada).CE_EntryNum = "12345100000014";
				newFactory.Save();

				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertHasErrorContaining(tranNumber.FormattedTransactionNumberInfo, "Transaction Number is already used in");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoError(tranNumber.FormattedTransactionNumberInfo, "Transaction Number is already used in");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				tranNumber.FormattedTransactionNumber = "12345100000023";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoErrorContaining(tranNumber.FormattedTransactionNumberInfo, "Transaction Number is already used in");
			}
		}

		public void TestCheckFormattedTransactionNumber_MessagesHaveBeenSentByOtherFactory()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = "00001796";
				Factory.Save();
				Assert(!declaration.TransactionNumber.EntryNumberChanged);

				declaration.TransactionNumber.SequentialNumber = "00001797";
				Assert(declaration.TransactionNumber.EntryNumberChanged);
			}

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = "00000023";

				var error = "Transaction Number can not be changed as message(s) has been sent.";
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				declaration.TransactionNumber.FormattedTransactionNumber = "32450000000023";
				declaration.TransactionNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoErrorContaining(declaration.TransactionNumber.FormattedTransactionNumberInfo, error);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedDeclaration = Factory.Load<JobDeclaration>(declaration.PK);
				var entry = reloadedDeclaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entry.Messages.AddNew();
				newFactory.Save();

				reloadedDeclaration.TransactionNumber.FormattedTransactionNumber = "32450000000013";
				newFactory.Save();
				AssertHasErrorContaining(reloadedDeclaration.TransactionNumber.FormattedTransactionNumberInfo, error);

				entry.Messages.RemoveAndDeleteAll();
				newFactory.Save();

				reloadedDeclaration.TransactionNumber.Validation.ValidateFormattedTransactionNumber();
				AssertNoErrorContaining(reloadedDeclaration.TransactionNumber.FormattedTransactionNumberInfo, error);
			}
		}

		public void TestNoExceptionThrownWhenDeclarationDoNotExists()
		{
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cusEntryNumber.CE_EntryNum = "12345000000012";
			cusEntryNumber.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber.CE_ParentID = Guid.NewGuid();
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				tranNumber.FormattedTransactionNumber = "12345000000012";
				tranNumber.Validation.ValidateFormattedTransactionNumber();
			});
		}

		#region Implementation

		void AssertHasError(string messageError, ZPropertyInfo info, params ZString[] values)
		{
			foreach (var value in values)
			{
				info.Value = value;
				AssertHasError(info, messageError);
			}
		}

		void AssertNoError(string messageError, ZPropertyInfo info, params ZString[] values)
		{
			foreach (var value in values)
			{
				info.Value = value;
				AssertNoError(info, messageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-8E86F7C5", Guid.Empty);

			var helper = new DeclarationTestHelper(Factory, true);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = helper.CreateOrganisation("IMPORTER NAME", "CATOR");
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "30215";
			addInfo.ZO_AccountSecirityPassword = "1234567";
			declaration.JE_OH_Importer = importer.PK;
			tranNumber = declaration.TransactionNumber;
		}

		TransactionNumber tranNumber;
		JobDeclaration declaration;

		#endregion
	}
}
