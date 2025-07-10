using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	internal class ValidationHelperTest : TestCaseWithDummy
	{
		public void TestValidateRegexField()
		{
			string errorMsg = "This is bad.. man...";
			Dummy.Z0_VarCharMax = "TEST";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateRegexField(Dummy.Z0_VarCharMaxInfo, "^asdf$", errorMsg);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals("JXC: " + errorMsg, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateRegexField(Dummy.Z0_VarCharMaxInfo, "^TEST$", errorMsg);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			Dummy.Z0_VarCharMax = "test";
			AssertEquals("Regex matching should be ignoring case", 0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			Dummy.Z0_Bool = true;
			AssertEquals(0, Dummy.Z0_BoolInfo.GetWarnings().Count());
			ValidationHelper.ValidateRegexField(Dummy.Z0_BoolInfo, "^TEST$", errorMsg);
			AssertEquals(1, Dummy.Z0_BoolInfo.GetWarnings().Count());
			AssertEquals("JXC: " + errorMsg, Dummy.Z0_BoolInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_BoolInfo.ClearAllNotifications();
			ValidationHelper.ValidateRegexField(Dummy.Z0_BoolInfo, "^[YN]$", errorMsg);
			AssertEquals("Bool should be converted to string", 0, Dummy.Z0_BoolInfo.GetWarnings().Count());
		}

		public void TestValidateFreeTextField()
		{
			Dummy.Z0_VarCharMax = "TEST$%_29834";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at most 3 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 0, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at most 3 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at least 1 and at most 3 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "0%K";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			Dummy.Z0_VarCharMax = "";
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at least 1 and at most 3 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 0, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
		}

		public void TestValidateFreeTextFieldWhenMinLengthIsGreaterThanMaxLength()
		{
			Dummy.Z0_VarCharMax = "";
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 6, 1);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at least 1 and at most 6 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "BUGATTI";
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 6, 1);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be at least 1 and at most 6 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "VEYRON";
			ValidationHelper.ValidateFreeTextField(Dummy.Z0_VarCharMaxInfo, 6, 1);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
		}

		public void TestValidateFreeTextFieldWithExactLength()
		{
			Dummy.Z0_VarCharMax = "_9K{";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateFreeTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals("JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must be 3 characters in length", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateFreeTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 4);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
		}

		public void TestValidateNumericTextField()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain numeric characters \"0-9\" and must be at most 3 characters in length";
			Dummy.Z0_VarCharMax = "TEST";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "9921";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "991";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			Dummy.Z0_VarCharMax = "";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain numeric characters \"0-9\" and must be at least 1 and at most 3 characters in length";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		public void TestValidateNumericTextFieldWithExactLength()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain numeric characters \"0-9\" and must be 3 characters in length";
			Dummy.Z0_VarCharMax = "TEST";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "TES";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "123";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			Dummy.Z0_VarCharMax = "1234";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			expectedErrorMessage = "JXC: " + Dummy.Z0_NumberInfo.HumanReadableName + " must only contain numeric characters \"0-9\" and must be 3 characters in length";
			Dummy.Z0_Number = 8891;
			AssertEquals(0, Dummy.Z0_NumberInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_NumberInfo, 3);
			AssertEquals(1, Dummy.Z0_NumberInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_NumberInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_Number = 889;
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_NumberInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Dummy.Z0_NumberInfo, 3);
			AssertEquals(0, Dummy.Z0_NumberInfo.GetWarnings().GetUniqueMessageList().Length);
		}

		public void TestValidateAlphabeticTextField()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphabetic characters \"a-zA-Z\" and must be at most 3 characters in length";
			Dummy.Z0_VarCharMax = "1234";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "asdf";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "asd";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			Dummy.Z0_VarCharMax = "";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphabetic characters \"a-zA-Z\" and must be at least 1 and at most 3 characters in length";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		public void TestValidateAlphabeticTextFieldWithExactLength()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphabetic characters \"a-zA-Z\" and must be 3 characters in length";
			Dummy.Z0_VarCharMax = "1234";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "123";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "tes";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			Dummy.Z0_VarCharMax = "test";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphabeticTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		public void TestValidateAlphanumericTextField()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be at most 3 characters in length";
			Dummy.Z0_VarCharMax = "%ab12";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphanumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "ab12";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphanumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "ab1";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphanumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			Dummy.Z0_VarCharMax = "";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphanumericTextField(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be at least 1 and at most 3 characters in length";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			ValidationHelper.ValidateAlphanumericTextField(Dummy.Z0_VarCharMaxInfo, 1, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		public void TestValidateAlphanumericTextFieldWithExactLength()
		{
			string expectedErrorMessage = "JXC: " + Dummy.Z0_VarCharMaxInfo.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be 3 characters in length";
			Dummy.Z0_VarCharMax = "$12ab";
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "$1a";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMax = "1ab";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			Dummy.Z0_VarCharMax = "1ab2";
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			AssertEquals(0, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			ValidationHelper.ValidateAlphanumericTextFieldWithExactLength(Dummy.Z0_VarCharMaxInfo, 3);
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(expectedErrorMessage, Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		[ExpectNoExceptions]
		public void TestValidateJXCForwarder_NullParamsDoesNotBlowUpMethod()
		{
			ValidationHelper.ValidateJXCForwarder(null, null);
		}

		public void TestValidateJXCForwarder()
		{
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Invoicing);
			JASARInvoice transaction = Factory.New<JASARInvoice>();
			JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
			testOrg.CompanyData.OB_IsDebtor = true;
			testOrg.OH_Code = "TESTORG";
			Assert("Pre-condition", !transaction.AH_OHInfo.HasErrors());
			transaction.AH_OH = testOrg.PK;
			AssertEquals(2, transaction.AH_OHInfo.GetWarnings().Count());
			Assert(transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Netting Code"));
			Assert(transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code"));
			testOrg.OfficeCode = "AUSYD";
			transaction.Validation.ValidateAH_OH();
			AssertEquals(1, transaction.AH_OHInfo.GetWarnings().Count());
			Assert(transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Netting Code"));
			Assert(!transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code"));
			testOrg.NettingCode = "AUCOR";
			transaction.Validation.ValidateAH_OH();
			ValidationHelper.ValidateJXCForwarder(testOrg.OH_CodeInfo, testOrg);
			AssertEquals(0, transaction.AH_OHInfo.GetWarnings().Count());
			Assert(!transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Netting Code"));
			Assert(!transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code"));
		}

		[ExpectNoExceptions]
		public void TestValidateJXCDebtor_NullParamsDoesNotBlowUpMethod()
		{
			ValidationHelper.ValidateJXCDebtor(null, null);
		}

		public void TestValidateJXCDebtor()
		{
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Invoicing);
			JASARInvoice transaction = Factory.New<JASARInvoice>();
			JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
			testOrg.OH_Code = "TESTORG";
			testOrg.CompanyData.OB_IsDebtor = true;
			Assert("Pre-condition", !transaction.AH_OHInfo.HasErrors());
			transaction.AH_OH = testOrg.PK;
			Factory.Save();
			testOrg.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals(2, transaction.AH_OHInfo.GetWarnings().Count());
			Assert(transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Netting Code"));
			Assert(transaction.AH_OHInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code"));
		}

		[ExpectNoExceptions]
		public void TestValidateJXCBranchProxy_NullParamsDoesNotBlowUpMethod()
		{
			ValidationHelper.ValidateJXCBranchProxy(null, null);
		}

		public void TestValidateJXCBranchProxy()
		{
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Invoicing);
			GlbBranch branch = Factory.New<GlbBranch>();
			JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
			branch.GB_OH_OrgProxy = testOrg.PK;
			JASARInvoice transaction = Factory.New<JASARInvoice>();
			Assert("Pre-condition", !transaction.AH_GBInfo.HasErrors());
			transaction.AH_GB = branch.PK;
			AssertEquals(2, transaction.AH_GBInfo.GetWarnings().GetUniqueMessageList().Length);
			Assert(transaction.AH_GBInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Proxy Organisation for the current Branch does not have JAS Netting Code"));
			Assert(transaction.AH_GBInfo.GetWarnings().Contains(JXCConstants.JXCWarningPrefix + "Proxy Organisation for the current Branch does not have JAS Office Code"));
		}

		[ExpectNoExceptions]
		public void TestValidateCurrencyCode_NullParamsDoesNotBlowUpMethod()
		{
			ValidationHelper.ValidateCurrencyCode(null, null, null);
		}

		public void TestValidateCurrencyCode()
		{
			ValidationHelper.ValidateCurrencyCode(Factory, Dummy.Z0_VarCharMaxInfo, "123");
			AssertEquals(1, Dummy.Z0_VarCharMaxInfo.GetWarnings().Count());
			AssertEquals(JXCConstants.JXCWarningPrefix + "Invalid currency code", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
			Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			ValidationHelper.ValidateCurrencyCode(Factory, Dummy.Z0_VarCharMaxInfo, "USD");
			Assert(!Dummy.Z0_VarCharMaxInfo.HasWarnings());
		}

		public void TestAddJXCWarning()
		{
			ValidationHelper.AddJXCWarning(Dummy.Z0_VarCharMaxInfo, "HAHA TEST123");
			AssertEquals("JXC: HAHA TEST123", Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage());
		}

		public void TestAddJXCWarningIfNotEntered()
		{
			Dummy.Z0_VarCharMax = "";
			ValidationHelper.AddJXCWarningIfNotEntered(Dummy.Z0_VarCharMaxInfo);
			Assert(Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage().StartsWith(JXCConstants.JXCWarningPrefix + MandatoryValidation.YouHaveNotEntered));
		}

		public void TestAddJXCWarningIfInvalidCode_CodeDescriptionPairList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("BBB", "");
			Dummy.Z0_Code = "BBB";
			ValidationHelper.AddJXCWarningIfInvalidCode(Dummy.Z0_CodeInfo, list);
			Assert("Should be valid", !Dummy.Z0_CodeInfo.HasWarnings());
			Dummy.Z0_Code = "AAA";
			ValidationHelper.AddJXCWarningIfInvalidCode(Dummy.Z0_CodeInfo, list);
			AssertEquals(1, Dummy.Z0_CodeInfo.GetWarnings().Count());
			Assert(Dummy.Z0_CodeInfo.GetWarnings().GetFirstMessage().StartsWith(JXCConstants.JXCWarningPrefix + ListValidation.InvalidCodeMessage));
		}

		public void TestAddJXCWarningIfInvalidCode_BusinessObjectCollection()
		{
			SuperDummyBusinessObject superDummy = Factory.New<SuperDummyBusinessObject>();
			DummyBusinessObject child1 = Dummy.Lookups.DummyList.AddNew();
			child1.Z0_Code = "CHLD1";
			DummyBusinessObject child2 = Dummy.Lookups.DummyList.AddNew();
			child2.Z0_Code = "CHLD2";
			superDummy.SS_Name = "CHLD2";
			ValidationHelper.AddJXCWarningIfInvalidCode(superDummy.SS_NameInfo, Dummy.Lookups.DummyList);
			Assert("Should be valid", !superDummy.SS_NameInfo.HasWarnings());
			superDummy.SS_Name = "CHLD3";
			ValidationHelper.AddJXCWarningIfInvalidCode(superDummy.SS_NameInfo, Dummy.Lookups.DummyList);
			AssertEquals(1, superDummy.SS_NameInfo.GetWarnings().GetUniqueMessageList().Length);
			Assert(superDummy.SS_NameInfo.GetWarnings().GetFirstMessage().StartsWith(JXCConstants.JXCWarningPrefix + ListValidation.InvalidCodeMessage));
		}

		public void TestHasJXCWarnings()
		{
			DummyBusinessObject dummyWithChildren = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject childDummy = dummyWithChildren.Collection.AddNew();
			using (dummyWithChildren.SuspendValidationTesting())
			using (childDummy.SuspendValidationTesting())
			{
				dummyWithChildren.ClearAllNotifications();
				dummyWithChildren.RegisterEditableChildObject(childDummy);
				Assert("Pre-condition", !ValidationHelper.HasJXCWarnings(dummyWithChildren));
				ValidationHelper.AddJXCWarning(childDummy.Z0_AnotherNumberInfo, "BLAH");
				Assert("Should have jxc warning", ValidationHelper.HasJXCWarnings(dummyWithChildren));
				childDummy.ClearAllNotifications();
				Assert("Should be cleared", !ValidationHelper.HasJXCWarnings(dummyWithChildren));
				dummyWithChildren.Z0_DateInfo.AddMessageError("This is a message error");
				Assert("Warning is not prefixed by JXC: ", !ValidationHelper.HasJXCWarnings(dummyWithChildren));
				ValidationHelper.AddJXCWarning(dummyWithChildren.Z0_DecimalInfo, "WOW");
				Assert("Should have JXC warning", ValidationHelper.HasJXCWarnings(dummyWithChildren));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SuspendValidationTestingDisposable = Dummy.SuspendValidationTesting();
		}

		protected override void TearDown()
		{
			SuspendValidationTestingDisposable.Dispose();
			base.TearDown();
		}

		new DummyEnterpriseBusinessObject Dummy
		{
			get
			{
				return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>());
			}
		}

		DummyEnterpriseBusinessObject dummy;
		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}

				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
		IDisposable SuspendValidationTestingDisposable;
	}
}
