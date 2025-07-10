using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	public class CreditLimitServiceTest : TestCaseWithFactory
	{
		public void TestCreditLimitCheckLegacyCode()
		{
			OrgHeader org = CreateTestOrg();
			var checker = new CreditLimitAndBalanceChecker(DbAccess);

			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";

			CreditLimitAndBalanceResponse response = checker.GetCreditLimitAndBalanceDetails(request);

			Assert("Succeeded", response.Succeeded);
			AssertEquals("OriginalRequest.CompanyCode", GlbCompany.CurrentCompany.GC_Code, response.CreditLimitAndBalanceDetails[0].CompanyCode);
			AssertEquals("OriginalRequest.ExternalDebtorCode", externalDebtorCode, response.CreditLimitAndBalanceDetails[0].ExternalDebtorCode);
			AssertEquals("OriginalRequest.ExternalCreditorCode", externalCreditorCode, response.CreditLimitAndBalanceDetails[0].ExternalCreditorCode);
			AssertEquals("OriginalRequest.LegacyCode", legacyCode, response.CreditLimitAndBalanceDetails[0].LegacySystemCode);
			AssertEquals("Credit on Hold", org.CompanyData.OB_AROnCreditHold, response.CreditLimitAndBalanceDetails[0].OnCreditHold);
		}

		public void TestSettlementGroupInfoWhenSettlementGroupIsADifferentOrg()
		{
			OrgHeader org = CreateTestOrg();

			OrgHeader settlementOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			settlementOrg1.SetLocalCustomsCode(OrgCusCode.CodeTypes.LegacySystemCode, settlementGrouplegacyCode);
			settlementOrg1.CompanyData.OB_ARExternalDebtorCode = settlementGroupExternalDebtorCode;
			settlementOrg1.CompanyData.OB_APExternalCreditorCode = settlementGroupExternalCreditorCode;

			org.SetRelatedParty(settlementOrg1, RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);
			org.SetRelatedParty(settlementOrg1, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);

			Factory.Save();

			var checker = new CreditLimitAndBalanceChecker(DbAccess);

			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";

			CreditLimitAndBalanceResponse response = checker.GetCreditLimitAndBalanceDetails(request);

			Assert("Succeeded", response.Succeeded);
			AssertEquals("OriginalRequest.CompanyCode", GlbCompany.CurrentCompany.GC_Code, response.CreditLimitAndBalanceDetails[0].CompanyCode);
			AssertEquals("OriginalRequest.ExternalDebtorCode", externalDebtorCode, response.CreditLimitAndBalanceDetails[0].ExternalDebtorCode);
			AssertEquals("OriginalRequest.ExternalCreditorCode", externalCreditorCode, response.CreditLimitAndBalanceDetails[0].ExternalCreditorCode);
			AssertEquals("OriginalRequest.LegacyCode", legacyCode, response.CreditLimitAndBalanceDetails[0].LegacySystemCode);
			AssertEquals("Credit on Hold", org.CompanyData.OB_AROnCreditHold, response.CreditLimitAndBalanceDetails[0].OnCreditHold);

			AssertEquals("OriginalRequest.SettlementGroupCode", settlementOrg1.OH_Code, response.CreditLimitAndBalanceDetails[0].SettlementGroupOrgCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalDebtorCode", settlementGroupExternalDebtorCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalDebtorCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalCreditorCode", settlementGroupExternalCreditorCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalCreditorCode);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", settlementGrouplegacyCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupLegacySystemCode);
		}

		public void TestSettlementGroupInfoWhenSettlementGroupIstOrgItself()
		{
			OrgHeader org = CreateTestOrg();
			org.SetRelatedParty(org, RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);
			org.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			org.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var checker = new CreditLimitAndBalanceChecker(DbAccess);
			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";
			var response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("OriginalRequest.SettlementGroupCode", org.OH_Code, response.CreditLimitAndBalanceDetails[0].SettlementGroupOrgCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalDebtorCode", externalDebtorCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalDebtorCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalCreditorCode", externalCreditorCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalCreditorCode);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", legacyCode, response.CreditLimitAndBalanceDetails[0].SettlementGroupLegacySystemCode);
		}

		public void TestSettlementGroupInfoWhenThereIsNoSettlementGroup()
		{
			OrgHeader org = CreateTestOrg(false);
			var checker = new CreditLimitAndBalanceChecker(DbAccess);

			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";

			var response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("OriginalRequest.SettlementGroupCode", null, response.CreditLimitAndBalanceDetails[0].SettlementGroupOrgCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalDebtorCode", null, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalDebtorCode);
			AssertEquals("OriginalRequest.SettlementGroupExternalCreditorCode", null, response.CreditLimitAndBalanceDetails[0].SettlementGroupExternalCreditorCode);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", null, response.CreditLimitAndBalanceDetails[0].SettlementGroupLegacySystemCode);
		}

		[ExpectNoExceptions]
		public void TestCreditLimitCheckLegacyCodeDoesNotThrowUnhandledExceptions()
		{
			var organisationCode = GetOrganisationCodeFromDatabase();

			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = organisationCode;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";

			var service = new CreditLimitServiceForTest(Connection, Transaction, DbAccess);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
			var response = service.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("OriginalRequest.CompanyCode", GlbCompany.CurrentCompany.GC_Code, response.CreditLimitAndBalanceDetails[0].CompanyCode);
			AssertEquals("OriginalRequest.OrgCode", organisationCode.Trim(), response.CreditLimitAndBalanceDetails[0].OrgCode);
		}

		[ExpectNoExceptions]
		public void TestCheckTransactionPaymentStatusDoesNotThrowUnhandledExceptions()
		{
			var organisationCode = GetOrganisationCodeFromDatabase();

			var request = new TransactionPaymentStatusRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = organisationCode;
			request.AccLedger = "AR";
			request.TransactionType = "";
			request.TransactionNumber = "";

			var service = new CreditLimitServiceForTest(Connection, Transaction, DbAccess);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
			var response = service.CheckTransactionPaymentStatus(request);

			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("ErrorMessage", "Transaction not found", response.ErrorMessage);
		}

		public void TestGlobalCreditLimitDetailsNoCreditLimitGroup()
		{
			OrgHeader org = CreateTestOrg();
			org.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			org.MiscServ.OM_ARGlobalCreditLimit = 5000m;
			Factory.Save();

			var checker = new CreditLimitAndBalanceChecker(DbAccess);
			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";
			var response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApproved", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHoldHasValue", true, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHoldHasValue);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", false, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHold);
			AssertEquals("OriginalRequest.GlobalCreditLimitCurrencyCode", "USD", response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", 5000m, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimit);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimitHasValue", true, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimit);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRate", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRate);

			org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApproved", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", true, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHold);
			AssertEquals("OriginalRequest.GlobalCreditLimitCurrencyCode", "USD", response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", 5000m, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimit);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimitHasValue", true, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimit);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRate", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRate);

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			Factory.Save();

			response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApprovedHasValue", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApprovedHasValue);
			AssertEquals("OriginalRequest.IsGlobalCreditApproved", false, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", false, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHoldHasValue);
			AssertEquals("OriginalRequest.GlobalCreditLimitCurrencyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", false, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
		}

		public void TestGlobalCreditLimitDetailsWithCreditLimitGroup()
		{
			OrgHeader org = CreateTestOrg();

			OrgHeader globalCreditGroupOrg = Factory.NewWithValidTestData<OrgHeader>();
			globalCreditGroupOrg.MiscServ.OM_ARGlobalCreditApproved = true;
			globalCreditGroupOrg.MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			globalCreditGroupOrg.MiscServ.OM_ARGlobalCreditLimit = 10000m;

			org.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_OH_ARGlobalCreditGroup = globalCreditGroupOrg.PK;
			org.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			var checker = new CreditLimitAndBalanceChecker(DbAccess);
			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = org.OH_Code;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";
			var response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApproved", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHoldHasValue", true, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHoldHasValue);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", false, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHold);
			AssertEquals("OriginalRequest.GlobalCreditCurrencyCode", "USD", response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", 10000m, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimit);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", globalCreditGroupOrg.OH_Code, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimitHasValue", true, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimit);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRate", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRate);

			org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApproved", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", true, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHold);
			AssertEquals("OriginalRequest.GlobalCreditCurrencyCode", "USD", response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", 10000m, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimit);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", globalCreditGroupOrg.OH_Code, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimitHasValue", true, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimit);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRate", true, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRate);

			globalCreditGroupOrg.MiscServ.OM_ARGlobalCreditApproved = false;
			Factory.Save();

			response = checker.GetCreditLimitAndBalanceDetails(request);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertEquals("ErrorMessage", null, response.ErrorMessage);

			AssertEquals("OriginalRequest.IsGlobalCreditApprovedHasValue", true, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApprovedHasValue);
			AssertEquals("OriginalRequest.IsGlobalCreditApproved", false, response.CreditLimitAndBalanceDetails[0].IsGlobalCreditApproved);
			AssertEquals("OriginalRequest.OnGlobalCreditHold", false, response.CreditLimitAndBalanceDetails[0].OnGlobalCreditHoldHasValue);
			AssertEquals("OriginalRequest.GlobalCreditCurrencyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditCurrencyCode);
			AssertEquals("OriginalRequest.GlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].GlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.SettlementGroupLegacyCode", null, response.CreditLimitAndBalanceDetails[0].GlobalCreditGroupOrgCode);
			AssertEquals("OriginalRequest.IsOverGlobalCreditLimit", false, response.CreditLimitAndBalanceDetails[0].IsOverGlobalCreditLimitHasValue);
			AssertEquals("OriginalRequest.GlobalBalanceTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalBalanceTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueRecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueRecognisedTotalHasValue);
			AssertEquals("OriginalRequest.GlobalUnpostedRevenueUnrecognisedTotalHasValue", false, response.CreditLimitAndBalanceDetails[0].GlobalUnpostedRevenueUnrecognisedTotalHasValue);
			AssertEquals("OriginalRequest.InvalidGlobalCreditCurrencyOrMissingExRateHasValue", false, response.CreditLimitAndBalanceDetails[0].InvalidGlobalCreditCurrencyOrMissingExRateHasValue);
		}

		public void TestGetCreditLimitAndBalanceDetailsExceptionErrorReporter()
		{
			// Arrange
			var checker = new CreditLimitAndBalanceChecker(DbAccess);

			// Act
			var response = checker.GetCreditLimitAndBalanceDetails(null);

			// Assert
			Assert("Fail", !response.Succeeded);
			AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

			var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
			AssertNotNull(webException);
			AssertEquals(webException.Message, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastMessageReported, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_CreditLimitAndBalanceChecker.GetCreditLimitAndBalanceDetails_NullReferenceException");

			var innerException = webException.InnerException;
			AssertNotNull(innerException);
			AssertNotNullOrEmpty(innerException.StackTrace);
			AssertEquals(webException.Source, innerException.Source);
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestGetTransactionPaymentStatusExceptionErrorReporter()
		{
			// Arrange
			var checker = new CreditLimitAndBalanceChecker(DbAccess);

			// Act
			var response = checker.GetTransactionPaymentStatus(null);

			// Assert
			Assert("Fail", !response.Succeeded);
			AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

			var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
			AssertNotNull(webException);
			AssertEquals(webException.Message, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastMessageReported, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_CreditLimitAndBalanceChecker.GetTransactionPaymentStatus_NullReferenceException");

			var innerException = webException.InnerException;
			AssertNotNull(innerException);
			AssertNotNullOrEmpty(innerException.StackTrace);
			AssertEquals(webException.Source, innerException.Source);
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestGetCreditLimitAndBalanceDetailsWebMethodExceptionErrorReporter()
		{
			// Arrange
			var organisationCode = GetOrganisationCodeFromDatabase();

			var request = new CreditLimitAndBalanceRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = organisationCode;
			request.AccLedger = "AR";
			request.OverdueAgingPeriod = 30;
			request.BalanceOverdueAgingOption = "BAL";

			var service = new CreditLimitServiceForTest(null, null, null);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };

			// Act
			var response = service.GetCreditLimitAndBalanceDetails(request);

			// Assert
			Assert("Fail", !response.Succeeded);
			Assert(response.CreditLimitAndBalanceDetails.Count == 0);
			AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

			var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
			AssertNotNull(webException);
			AssertEquals(webException.Message, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastMessageReported, "Object reference not set to an instance of an object.");
			AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_CreditLimitService.GetCreditLimitAndBalanceDetails_NullReferenceException");

			var innerException = webException.InnerException as NullReferenceException;
			AssertNotNull(innerException);
			AssertNotNullOrEmpty(innerException.StackTrace);
			AssertEquals(webException.Source, innerException.Source);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestCheckTransactionPaymentStatusWebMethodExceptionErrorReporter()
		{
			// Arrange
			var organisationCode = GetOrganisationCodeFromDatabase();

			var request = new TransactionPaymentStatusRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.OrgCode = organisationCode;
			request.AccLedger = "AR";
			request.TransactionType = "";
			request.TransactionNumber = "";

			var service = new CreditLimitServiceForTest(null, null, null);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };

			// Act
			var response = service.CheckTransactionPaymentStatus(request);

			// Assert
			Assert("Fail", !response.Succeeded);
			AssertNull(response.TransactionPaymentStatus);
			AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

			var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
			AssertNotNull(webException);
			AssertEquals(webException.Message, "BaseDataAccess: Connection property has not been initialized.");
			AssertEquals(ErrorReporter.LastMessageReported, "BaseDataAccess: Connection property has not been initialized.");
			AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_CreditLimitService.CheckTransactionPaymentStatus_InvalidOperationException");

			var innerException = webException.InnerException;
			AssertNotNull(innerException);
			AssertNotNullOrEmpty(innerException.StackTrace);
			AssertEquals(webException.Source, innerException.Source);
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.AccountingWebServiceUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "username");
			AccountingConfigurationRegistry.Instance.AccountingWebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");

			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DbAccess = new BaseDataAccess(Connection, Transaction);
		}

		OrgHeader CreateTestOrg(bool assignLegacyCode = true)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			if (assignLegacyCode)
			{
				result.SetLocalCustomsCode(OrgCusCode.CodeTypes.LegacySystemCode, legacyCode);
			}
			result.CompanyData.OB_ARExternalDebtorCode = externalDebtorCode;
			result.CompanyData.OB_APExternalCreditorCode = externalCreditorCode;
			result.CompanyData.OB_AROnCreditHold = ZBool.True;

			Factory.Save();

			return result;
		}

		string GetOrganisationCodeFromDatabase()
		{
			string sqlCommandText = string.Format(@"SELECT TOP 1 OH_Code 
													FROM dbo.OrgHeader
													JOIN dbo.OrgCompanyData	ON OH_PK = OB_OH
													JOIN dbo.GlbCompany ON OB_GC = GC_PK
													WHERE GC_Code = '{0}'
													ORDER BY OH_Code", GlbCompany.CurrentCompany.GC_Code);
			return (string)Db.Connection.ExecuteScalar(sqlCommandText);
		}

		class CreditLimitServiceForTest : CreditLimitService
		{
			readonly System.Data.Common.DbConnection TestSQLConnection;
			readonly System.Data.Common.DbTransaction TestTransaction;
			readonly BaseDataAccess TestBaseDataAccess;

			public CreditLimitServiceForTest(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, BaseDataAccess baseDataAccess)
			{
				TestSQLConnection = connection;
				TestTransaction = transaction;
				TestBaseDataAccess = baseDataAccess;
			}

			protected override BaseDataAccess CreateBaseDataAccess(System.Data.Common.DbConnection conn)
			{
				return TestBaseDataAccess;
			}

			protected override TransactionPaymentDataAccess CreateTransactionPaymentDataAccess(System.Data.Common.DbConnection conn)
			{
				return new TransactionPaymentDataAccess(TestSQLConnection, TestTransaction);
			}
		}

		BaseDataAccess DbAccess { get; set; }
		System.Data.Common.DbConnection Connection { get; set; }
		System.Data.Common.DbTransaction Transaction { get; set; }

		const string legacyCode = "LEGACY123";
		const string settlementGrouplegacyCode = "SETTLELEGACY123";
		const string externalDebtorCode = "DEBT123";
		const string externalCreditorCode = "CREDIT123";
		const string settlementGroupExternalDebtorCode = "STGDEBT123";
		const string settlementGroupExternalCreditorCode = "STGCREDIT123";

		#endregion
	}
}
