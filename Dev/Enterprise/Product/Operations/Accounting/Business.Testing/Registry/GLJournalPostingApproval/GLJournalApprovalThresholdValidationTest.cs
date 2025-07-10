using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	internal class GLJournalApprovalThresholdValidationTest : TestCaseWithFactory
	{
		public void TestValidateRow()
		{
			collection = new GLJournalApprovalThresholdCollection();
			GLJournalApprovalThreshold newElement = collection.AddNew();
			newElement.Type = "";
			AssertHasErrors("Precondition: any error", newElement.TypeInfo);
			newElement.RunPreSaveValidation();
			AssertNoRowErrors(newElement);

			newElement.Type = GLJournalApprovalThreshold.TypeCodes.All;
			AssertNoErrors("Precondition:", newElement.TypeInfo);
			newElement.RunPreSaveValidation();
			AssertHasRowError(newElement, "The corresponding threshold amounts must be defined.");

			newElement.AuthorisationSettings.AddNew();
			newElement.RunPreSaveValidation();
			AssertNoRowErrors(newElement);

			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AssertNoErrors("Precondition:", newElement.TypeInfo);
			newElement.RunPreSaveValidation();
			AssertEquals("There must be no authorization setting when threshold is 'ANY'.", newElement.AuthorisationSettings.Count, 0);
			AssertNoErrors("Should not allow any authorization settings when the threshold type is 'ANY'.", newElement.TypeInfo);
		}

		public void TestValidateReportSection()
		{
			GLJournalApprovalThreshold newElement = collection.AddNew();
			newElement.Type = "RSN";

			newElement.ReportSection = "";
			var error = "Please enter a value.";
			AssertHasError(newElement.ReportSectionInfo, error);

			using (newElement.GetValidationSuspender())
			{
				newElement.ReportSection = "XX";
				AssertHasError("Previous error should be displayed when validation is suspended", newElement.ReportSectionInfo, error);
			}
			newElement.RunPreSaveValidation();
			AssertHasError(newElement.ReportSectionInfo, "Enter a valid selection.");

			newElement.ReportSection = "OV";
			newElement.ReportSectionInfo.HasError("At least one more record already exists with the same Report Section.");

			newElement.ReportSection = "AS";
			AssertNoErrors(newElement.ReportSectionInfo);
		}

		public void TestValidateGLAccount()
		{
			GLJournalApprovalThreshold newElement = collection.AddNew();
			newElement.Type = "GLA";

			newElement.GLAccount = ZGuid.Empty;
			AssertHasError(newElement.GLAccountInfo, "Please enter a value.");

			newElement.GLAccount = ZGuid.Invalid;
			var error = "You can only select a BSH or P&L Account.";
			AssertHasError(newElement.GLAccountInfo, error);

			using (newElement.GetValidationSuspender())
			{
				newElement.GLAccount = glAccount1.PK;
				AssertHasError("Previous error should be displayed when validation is suspended", newElement.GLAccountInfo, error);
			}
			newElement.RunPreSaveValidation();
			AssertHasError(newElement.GLAccountInfo, "At least one more record already exists with the same GL Account.");

			newElement.GLAccount = glAccount2.PK;
			AssertNoErrors(newElement.GLAccountInfo);

			int accNumber = 30;
			foreach (var field in typeof(Constants.AccountType).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var accType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
				var glAccount = TestObjectCreator.CreateAccGLHeader(string.Format("1111.11.{0}", accNumber), "TS", "ACCOUNT", accType, Core.Constants.DebitCredit.Credit);
				newElement.GLAccount = glAccount.PK;
				if (accType == Constants.AccountType.BalanceSheetAccount || accType == Constants.AccountType.ProfitAndLossAccount)
				{
					AssertNoErrors(newElement.GLAccountInfo);
				}
				else
				{
					AssertHasError(newElement.GLAccountInfo, "You can only select a BSH or P&L Account.");
				}
				accNumber++;
			}
		}

		public void TestValidateType()
		{
			GLJournalApprovalThreshold newElement = collection.AddNew();
			newElement.Type = "";
			AssertHasError(newElement.TypeInfo, "Please enter a value.");

			newElement.Type = "XXX";
			var error = "Enter a valid selection.";
			AssertHasError(newElement.TypeInfo, error);

			using (newElement.GetValidationSuspender())
			{
				newElement.Type = "ALL";
				AssertHasError("Previous error should be displayed when validation is suspended", newElement.TypeInfo, error);
			}
			newElement.RunPreSaveValidation();
			AssertHasError(newElement.TypeInfo, "You can't have more than one record with type 'All'.");

			newElement.GLAccount = glAccount1.PK;
			newElement.Type = "GLA";
			AssertHasError(newElement.TypeInfo, "At least one more record already exists with the same GL Account.");

			newElement.ReportSection = "OV";
			newElement.Type = "RSN";
			AssertHasError(newElement.TypeInfo, "At least one more record already exists with the same Report Section.");

			newElement.GLAccount = glAccount2.PK;
			newElement.Type = "GLA";
			AssertNoErrors(newElement.TypeInfo);

			newElement.ReportSection = "AS";
			newElement.Type = "RSN";
			AssertNoErrors(newElement.TypeInfo);

			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			var errorMessage1 = "'ANY' type cannot be used in combination with other types.";
			var errorMessage2 = "You can't have more than one record with type 'ANY'.";
			AssertHasError(newElement.TypeInfo, errorMessage1);
			AssertNoError(newElement.TypeInfo, errorMessage2);
			newElement = collection.AddNew();
			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AssertHasError(newElement.TypeInfo, errorMessage1);
			AssertHasError(newElement.TypeInfo, errorMessage2);
			collection.RemoveAll();
			newElement = collection.AddNew();
			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AssertNoErrors(newElement.TypeInfo);
			newElement = collection.AddNew();
			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AssertNoError(newElement.TypeInfo, errorMessage1);
			AssertHasError(newElement.TypeInfo, errorMessage2);
		}

		protected override void SetUp()
		{
			base.SetUp();

			glAccount1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", "TS", "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			glAccount2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", "TS", "ACCOUNT 2", Constants.AccountType.ProfitAndLossAccount, Constants.DebitCredit.Credit);
			Factory.Save();

			collection = new GLJournalApprovalThresholdCollection();
			GLJournalApprovalThreshold element = collection.AddNew();
			element = collection.AddNew();
			element.Type = "ALL";

			element = collection.AddNew();
			element.Type = "GLA";
			element.GLAccount = glAccount1.PK;

			element = collection.AddNew();
			element.Type = "RSN";
			element.ReportSection = "OV";
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		GLJournalApprovalThresholdCollection collection;
		AccGLHeader glAccount1;
		AccGLHeader glAccount2;
	}
}
