using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Integration.Testing
{
	public class OrgCreditLimitAndBalanceDetailsTest : TestCaseWithFactory
	{
		public void TestGetCreditLimitAndBalanceFromSystemDB()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = org.CompanyData.OB_IsCreditor = true;
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsCreditor = org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_ARCreditLimit = 100m;
			org2.CompanyData.OB_APCreditLimit = 150m;
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);

			AccTransactionHeader aRInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			aRInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 0m, false, true, false);

			AccTransactionHeader aRInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
			aRInv2.AH_InvoiceAmount = 80M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 0m, false, true, false);

			AccTransactionHeader aPInv = GetNewInvoice(org, LedgerTypes.AccountsPayable, -45m);
			aPInv.AH_InvoiceAmount = -45M;
			Factory.Save();
			OrgCreditLimitAndBalanceDetails balanceAR = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 45m, 45m, 0m, false, true, false);
			AssertCreditLimitAndBalanceDetails(balanceAR, LedgerTypes.AccountsReceivable, 170m, 170m, 0m, false, true, false);

			AccTransactionHeader aRInv3 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 30m);
			aRInv3.AH_InvoiceAmount = 30M;
			org.CompanyData.OB_ARCreditLimit = 170m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(balanceAR, LedgerTypes.AccountsReceivable, 170m, 170m, 0m, false, true, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 200m, 200m, 170m, false, true, false);

			org.CompanyData.OB_ARCreditLimit = 250m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 200m, 200m, 250m, false, false, false);

			org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 200m, 200m, 250m, true, false, false);

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 150m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 100m, false, false, false);

			AccTransactionHeader aPInv2 = GetNewInvoice(org2, LedgerTypes.AccountsPayable, -50m);
			aPInv2.AH_InvoiceAmount = -50M;
			aPInv2.AH_PostDate = ZDateTime.Now.AddDays(-10);
			aPInv2.AH_DueDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsPayable, 50m, 0m, 150m, false, false, true);
		}

		public void TestARGlobalCreditApprovedDecidedByEitherGlobalCreditGroupOrOrganisationSelf()
		{
			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 90m;
			Factory.Save();
			Factory.Save();
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 90M, creditCurrency: localCurrency);

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			org.MiscServ.OM_ARGlobalCreditLimit = 0M;
			var orgGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgGroup.CompanyData.OB_IsDebtor = true;
			orgGroup.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;
			orgGroup.MiscServ.OM_ARGlobalCreditApproved = true;
			orgGroup.MiscServ.OM_ARGlobalCreditLimit = 190m;
			org.MiscServ.OM_OH_ARGlobalCreditGroup = orgGroup.PK;
			Factory.Save();
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 190M, creditCurrency: localCurrency);
		}

		public void TestOrganisationShouldBeConsideredInGlobalGroupEvenItsGlobalCreditApprovedNoteSet()
		{
			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var orgGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgGroup.CompanyData.OB_IsDebtor = true;
			orgGroup.MiscServ.OM_ARGlobalCreditLimit = 100M;
			orgGroup.MiscServ.OM_ARGlobalCreditApproved = true;
			orgGroup.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;
			org.MiscServ.OM_ARGlobalCreditApproved = false;
			org.MiscServ.OM_OH_ARGlobalCreditGroup = orgGroup.PK;

			Factory.Save();

			var arInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;

			var arInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
			arInv2.AH_InvoiceAmount = 80M;

			Factory.Save();
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);
		}

		public void TestGetGlobalCreditLimitAndBalanceFromSystemDB()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYDCO-WAVE-1/AccountingWebService/CreditLimitService.asmx");

			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditApproved = true;
			org.CompanyData.OB_ARCreditLimit = 90m;

			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			var arInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;    // No need to add special Ex Rates if globsl credit is in the company's currency
			org.MiscServ.OM_ARGlobalCreditLimit = 95m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 90m);

			var arInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
			arInv2.AH_InvoiceAmount = 80M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 90m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);

			var apInv = GetNewInvoice(org, LedgerTypes.AccountsPayable, -45m);
			apInv.AH_InvoiceAmount = -45M;
			Factory.Save();
			var balanceAR = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertCreditLimitAndBalanceDetails(balanceAR, LedgerTypes.AccountsReceivable, 170m, 170m, 90m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(balanceAR, isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);

			var secondAUCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, "AU").AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).FirstOrDefault();
			AssertNotNull(secondAUCompany);
			var otherCompanyBranch = secondAUCompany.Branches.FirstOrDefault(x => x.GB_IsActive);
			AssertNotNull(otherCompanyBranch);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// Needs OrgCompanyData record for other Company
				org.CompanyData.OB_IsDebtor = true;
				org.CompanyData.OB_ARCreditApproved = true;
				org.CompanyData.OB_ARCreditLimit = 5m;

				var arInv3 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 30m);
				arInv3.AH_InvoiceAmount = 30M;
				Factory.Save();
			}

			org.CompanyData.OB_ARCreditLimit = 170m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(balanceAR, LedgerTypes.AccountsReceivable, 170m, 170m, 90m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(balanceAR, isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: true);

			org.MiscServ.OM_ARGlobalCreditLimit = 250m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false);

			org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false, onCreditHold: true);
		}

		public void TestGetGlobalCreditLimitAndBalanceForGlobalCreditGroupFromSystemDB()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYDCO-WAVE-1/AccountingWebService/CreditLimitService.asmx");

			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var orgGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgGroup.CompanyData.OB_IsDebtor = true;
			orgGroup.CompanyData.OB_ARCreditApproved = true;
			orgGroup.CompanyData.OB_ARCreditLimit = 90m;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditApproved = true;

			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			var arInv = GetNewInvoice(orgGroup, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			orgGroup.MiscServ.OM_ARGlobalCreditApproved = true;
			orgGroup.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;    // No need to add special Ex Rates if globsl credit is in the company's currency
			orgGroup.MiscServ.OM_ARGlobalCreditLimit = 95m;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;
			org.MiscServ.OM_OH_ARGlobalCreditGroup = orgGroup.PK;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 90m);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 90m);

			var arInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
			arInv2.AH_InvoiceAmount = 80M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 0m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);

			var apInv = GetNewInvoice(org, LedgerTypes.AccountsPayable, -45m);
			apInv.AH_InvoiceAmount = -45M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 0m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 170m, isOverCreditLimit: true);

			var secondAUCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, "AU").AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).FirstOrDefault();
			AssertNotNull(secondAUCompany);
			var otherCompanyBranch = secondAUCompany.Branches.FirstOrDefault(x => x.GB_IsActive);
			AssertNotNull(otherCompanyBranch);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// Needs OrgCompanyData record for other Company
				org.CompanyData.OB_IsDebtor = true;
				org.CompanyData.OB_ARCreditApproved = true;
				org.CompanyData.OB_ARCreditLimit = 5m;

				var arInv3 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 30m);
				arInv3.AH_InvoiceAmount = 30M;
				Factory.Save();
			}

			org.CompanyData.OB_ARCreditLimit = 170m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: true);

			orgGroup.MiscServ.OM_ARGlobalCreditLimit = 250m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false);

			orgGroup.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false, onCreditHold: true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 170m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 250m, creditCurrency: localCurrency, outstandingBalance: 200m, isOverCreditLimit: false, onCreditHold: true);
		}

		public void TestGetGlobalCreditLimitAndBalanceForSettlementGroupFromSystemDB()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYDCO-WAVE-1/AccountingWebService/CreditLimitService.asmx");

			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var orgGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgGroup.CompanyData.OB_IsDebtor = true;
			orgGroup.CompanyData.OB_ARCreditApproved = true;
			orgGroup.CompanyData.OB_ARCreditLimit = 100m;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsDebtor = true;

			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), isCreditApproved: false);

			var arInv = GetNewInvoice(orgGroup, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), isCreditApproved: false);

			var creator = new TestObjectCreator(Factory);
			creator.CreateARSettlementGroup(org1, orgGroup);
			creator.CreateARSettlementGroup(org2, orgGroup);
			org1.CompanyData.OB_ARCreditApproved = true;
			org1.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			org2.CompanyData.OB_ARCreditApproved = true;
			org2.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), isCreditApproved: false);

			org1.MiscServ.OM_ARGlobalCreditApproved = true;
			org1.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;    // No need to add special Ex Rates if globsl credit is in the company's currency
			org1.MiscServ.OM_ARGlobalCreditLimit = 95m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 0m);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 100m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), isCreditApproved: false);

			var arInv2 = GetNewInvoice(org2, LedgerTypes.AccountsReceivable, 80m);
			arInv2.AH_InvoiceAmount = 80M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 100m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(orgGroup.OH_Code), isCreditApproved: false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 100m, false, true, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org1.OH_Code), isCreditApproved: true, creditLimit: 95m, creditCurrency: localCurrency, outstandingBalance: 0m, isOverCreditLimit: false);
		}

		public void TestGetGlobalCreditLimitAndBalanceFromSystemDB_InvalidCurrencyORMissingExRate()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYDCO-WAVE-1/AccountingWebService/CreditLimitService.asmx");

			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditApproved = true;
			org.CompanyData.OB_ARCreditLimit = 90m;

			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			var arInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: false);

			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = localCurrency;    // No need to add special Ex Rates if globsl credit is in the company's currency
			org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: localCurrency, outstandingBalance: 90m);

			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "***";
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: "***", outstandingBalance: 0m, unableToCalculateBalance: true);

			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: "USD", outstandingBalance: 0m, unableToCalculateBalance: true);

			var buyExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate; // Rate type is not used for Global Credit
			buyExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			buyExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			buyExRate.RE_RX_NKExCurrency = "USD";
			buyExRate.RE_SellRate = 0.5m;
			buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: "USD", outstandingBalance: 0m, unableToCalculateBalance: true);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.PostPeriodsForEntireYear(new ZInt(ZDateTime.Today.Year), GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodHelper.PostPeriodsForEntireYear(new ZInt(ZDateTime.Today.AddYears(1).Year), GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var periodEndExRateForCurrentMonth = Factory.NewWithValidTestData<RefExchangeRate>();
			periodEndExRateForCurrentMonth.RE_ExRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;    // Rate type is used as fall back for Global Credit
			periodEndExRateForCurrentMonth.RE_StartDate = ZDateTime.Today;
			periodEndExRateForCurrentMonth.RE_RX_NKExCurrency = "USD";
			periodEndExRateForCurrentMonth.RE_SellRate = 0.5m;
			periodEndExRateForCurrentMonth.RE_GC = GlbCompany.CurrentCompany.PK;
			var periodEndExRateForNextMonth = Factory.NewWithValidTestData<RefExchangeRate>();
			periodEndExRateForNextMonth.RE_ExRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;    // Rate type is used as fall back for Global Credit
			periodEndExRateForNextMonth.RE_StartDate = ZDateTime.Today.AddMonths(1);
			periodEndExRateForNextMonth.RE_RX_NKExCurrency = "USD";
			periodEndExRateForNextMonth.RE_SellRate = 0.5m;
			periodEndExRateForNextMonth.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: "USD", outstandingBalance: 45m, unableToCalculateBalance: false);

			var globalExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			globalExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;    // Rate type is used for Global Credit
			globalExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			globalExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			globalExRate.RE_RX_NKExCurrency = "USD";
			globalExRate.RE_SellRate = 0.75m;
			globalExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 90m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), isCreditApproved: true, creditLimit: 100m, creditCurrency: "USD", outstandingBalance: 67.50m, unableToCalculateBalance: false);
		}

		public void TestCreditLimitTemporaryIncrease()
		{
			using (new MasterFilesTestHelper().GetUtcPlus8UserContext())
			{
				var creator = new TestObjectCreator(Factory);
				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

				var org = creator.CreateOrgHeader("OG1", false, true);
				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(org.CompanyData, 100m, 99m);
				Assert("The expiry date has been set", !org.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty);

				var org2 = creator.CreateOrgHeader("OG2", false, true);
				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(org2.CompanyData, 200m, 77m);
				Assert("The expiry date has been set", !org2.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty);
				Factory.Save();

				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 199m, false, false, false);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 277m, false, false, false);

				org.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = org.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry.AddDays(-5);
				Factory.Save();

				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 100m, false, false, false);
			}
		}

		public void TestGetCreditLimitAndBalanceFromWebServiceAndSystemDB()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;

			OrgHeader org2 = creator.ABIGAS;
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_ARCreditLimit = 100m;
			org2.CompanyData.OB_APCreditLimit = 150m;
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 1000.00m, true, true, true);

			AccTransactionHeader aRInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			aRInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 1000.00m, true, true, true);

			AccTransactionHeader aRInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
			aRInv2.AH_InvoiceAmount = 80M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 1000.00m, true, true, true);

			AccTransactionHeader aPInv = GetNewInvoice(org, LedgerTypes.AccountsPayable, -45m);
			aPInv.AH_InvoiceAmount = -45M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 45m, 45m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 170m, 170m, 1000.00m, true, true, true);

			AccTransactionHeader aRInv3 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 30m);
			aRInv3.AH_InvoiceAmount = 30M;
			org.CompanyData.OB_ARCreditLimit = 170m;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 45m, 45m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 200m, 200m, 1000.00m, true, true, true);

			org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 45m, 45m, -1000.00m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 200m, 200m, 1000.00m, true, true, true);

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, -1000.00m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 1000.00m, false, false, false);

			AccTransactionHeader aPInv2 = GetNewInvoice(org2, LedgerTypes.AccountsPayable, -50m);
			aPInv2.AH_InvoiceAmount = -50M;
			aPInv2.AH_PostDate = ZDateTime.Now.AddDays(-10);
			aPInv2.AH_DueDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsPayable, 50m, 0m, -1000.00m, false, false, false);
		}

		public void TestGetCreditLimitAndBalanceFromWebService()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;
			OrgHeader org2 = creator.ABIGAS;
			org2.CompanyData.OB_IsDebtor = org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_ARCreditLimit = 100m;
			org2.CompanyData.OB_APCreditLimit = 150m;
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, true, true, true);

			AccTransactionHeader aRInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			aRInv.AH_InvoiceAmount = 90M;
			Factory.Save();
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, true, true, true);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, true, true, true);

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org2.OH_Code), LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, false, false, false);
		}

		public void TestOrgGlobalValidationIsNotFailingWhenGlobalFieldsFromWebServiceAreNotProvided()
		{
			var creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var org = creator.ABIGAS;
			org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;
			Factory.Save();

			var testTransaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(typeof(ARInvoice));
			testTransaction.AH_OH = org.PK;

			AssertNoErrors(testTransaction.AH_OHInfo);

			testTransaction.Lines.AddNew();
			var line1 = testTransaction.Lines[0];
			line1.AL_OSAmount = 10M;
			line1.AL_OSExTaxAmount = 10M;
			line1.AL_RX_NKTransactionCurrency = "EUR";
			line1.AL_AG = creator.GLHeader1.PK;
			testTransaction.AH_OSTotalAmount = 10M;

			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, false, false, false);
			AssertARGlobalCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), false);
		}

		public void TestFetchCreditLimitAndBalanceFromWebService()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;
			Factory.Save();

			var details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertEquals(0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			AssertEquals("Web Service was invoked for provided option", 1, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable);
			AssertEquals("Web Service was invoked for all 3 default options", 3, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Overdue);
			AssertEquals("All data is already fetched from Web Service", 3, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			AssertEquals("All data is already fetched from Web Service", 3, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			AssertCreditLimitAndBalanceDetails(details, LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, true, true, true);
			AssertEquals("All data is already fetched from Web Service", 3, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			AssertCreditLimitAndBalanceDetails(details, LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, true, true, true);
			AssertEquals("Invoked data for AP", 5, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			MockCreditLimitServiceClientProvider.Reset();
			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertEquals(0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			AssertCreditLimitAndBalanceDetails(details, LedgerTypes.AccountsReceivable, 1500.00m, 100.00m, 1000.0m, true, true, true);
			AssertEquals("Invoked data for AR", 2, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			AssertCreditLimitAndBalanceDetails(details, LedgerTypes.AccountsPayable, 1500.00m, -100.00m, -1000.0m, true, true, true);
			AssertEquals("Invoked data for AP", 4, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
		}

		[TestDate(2020, 12, 2, 17, 0, 0)]
		public void TestFetchCreditLimitAndBalanceFromWebServiceSuspendsAfterANumberOfTimeouts()
		{
			OrgCreditLimitAndBalanceDetails.ClearTimeoutRetrySuspensionControl_ForTestOnly();
			MockCreditLimitServiceClientProvider.Reset();
			try
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);
				MockCreditLimitServiceClientProvider.Reset();

				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

				OrgHeader org = creator.AALSHI;
				org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
				org.CompanyData.OB_ARCreditLimit = 100m;
				org.CompanyData.OB_APCreditLimit = 150m;
				Factory.Save();

				var details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
				AssertEquals(0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				assertCreditLimitWebServiceInvokesSucessfully();

				MockCreditLimitServiceClientProvider.Reset();
				MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount = 5;
				for (int i = 0; i < 5; i++)
				{
					assertCreditLimitWebServiceInvokesWithTestTimeout();
				}
				AssertEquals("Web Service should be invoked up to 5 times when there are timeouts", 5, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				AssertEquals(0, MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount);

				assertCreditLimitWebServiceIsNotInvoked("Web Service should not be invoked after 5 timeouts");

				TestDateAttribute.AddMinutes(15);
				TestDateAttribute.AddSeconds(1);

				using (AccountingConfigurationRegistry.Instance.WebServiceCallSuspendingPeriodInMinutes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 10))
				{
					MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount = 1;
					assertCreditLimitWebServiceInvokesWithTestTimeout("After 15 minutes, Web Service should be invoked again");

					assertCreditLimitWebServiceIsNotInvoked("Web Service should not be invoked after a new timeout as suspension was extended by 10 minutes");

					using (AccountingConfigurationRegistry.Instance.MaxTimeoutCountBeforeSuspendingWebService.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 10))
					{
						TestDateAttribute.AddMinutes(10);
						TestDateAttribute.AddSeconds(1);

						assertCreditLimitWebServiceInvokesSucessfully("After 10 minutes, Web Service should be invoked again");

						MockCreditLimitServiceClientProvider.Reset();
						MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount = 9;
						for (int i = 0; i < 9; i++)
						{
							assertCreditLimitWebServiceInvokesWithTestTimeout();
						}
						AssertEquals("Web Service should be invoked 9 times when there are timeouts", 9, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
						AssertEquals(0, MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount);

						assertCreditLimitWebServiceInvokesSucessfully("Web Service should be invoked when timeouts end before 10 attempts");
					}
				}

				#region Helper methods

				void assertCreditLimitWebServiceInvokesSucessfully(string assertionMessage = null)
				{
					var expectedInvokeCount = MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked + 1;
					var message = assertionMessage ?? $"Web Service should be invoked {expectedInvokeCount} time(s)";

					details.ClearCachedValues_ForTestOnly();
					details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
					AssertEquals(message, expectedInvokeCount, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				}

				void assertCreditLimitWebServiceInvokesWithTestTimeout(string assertionMessage = null)
				{
					AssertGreaterThan("Expect CreditLimitReturnTimeoutForCount to be set to throw Test Timeout Exception", MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount, 0);

					var expectedTimeotCount = MockCreditLimitServiceClientProvider.CreditLimitReturnTimeoutForCount - 1;
					var expectedInvokeCount = MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked + 1;
					var message = assertionMessage ?? $"Web Service should be invoked {expectedInvokeCount} time(s)";

					details.ClearCachedValues_ForTestOnly();
					details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
					AssertEquals(message, expectedInvokeCount, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

					var webException = details.GetLastWebServiceException_ForTestOnly();
					AssertNotNull("Exception was thrown", webException?.InnerException);
					Assert("TimeoutException", webException.InnerException is TimeoutException);
					AssertEquals("It is a test timeout", "Timeout for Test", webException.InnerException.Message);
				}

				void assertCreditLimitWebServiceIsNotInvoked(string assertionMessage = null)
				{
					var expectedInvokeCount = MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked;
					var message = assertionMessage ?? $"Web Service should not be invoked";

					details.ClearCachedValues_ForTestOnly();
					details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
					AssertEquals(message, expectedInvokeCount, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				}

				#endregion
			}
			finally
			{
				OrgCreditLimitAndBalanceDetails.ClearTimeoutRetrySuspensionControl_ForTestOnly();
				MockCreditLimitServiceClientProvider.Reset();
			}
		}

		public void TestGetCreditLimitAndBalanceLegacySystemCode()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			GlbCompany aUCompany = testObjectCreator.CreateNewCompany("AUC", Constants.CountryCodes.Australia);
			GlbBranch aUBranch = testObjectCreator.CreateNewBranch(aUCompany, "AB1");
			GlbCompany nZCompany = testObjectCreator.CreateNewCompany("NZC", Constants.CountryCodes.NewZealand);
			GlbBranch nZBranch = testObjectCreator.CreateNewBranch(nZCompany, "NB1");

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode lSCAU = org.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "AU123", aUCompany.GC_RN_NKCountryCode);
			OrgCusCode lSCNZ = org.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "NZ456", nZCompany.GC_RN_NKCountryCode);

			Factory.Save();

			using (new TemporaryUserContext() { BranchPK = aUBranch.PK.ToGuid() }.Set())
			{
				org.CompanyData.OB_IsCreditor = org.CompanyData.OB_IsCreditor = true;
				Factory.Save();
				AccTransactionHeader aRInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
				aRInv.AH_InvoiceAmount = 90M;
				AccTransactionHeader aPInv = GetNewInvoice(org, LedgerTypes.AccountsPayable, -45m);
				aPInv.AH_InvoiceAmount = -45M;
				Factory.Save();
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCAU.OK_CustomsRegNo), LedgerTypes.AccountsPayable, 45m, 45m, 0m, false, true, false);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCAU.OK_CustomsRegNo), LedgerTypes.AccountsReceivable, 90m, 90m, 0m, false, true, false);
				AssertExceptionThrown("AU company should not match org because NZ Legacy System Code was passed in", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCNZ.OK_CustomsRegNo), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false));
				AssertExceptionThrown("AU company should not match org because NZ Legacy System Code was passed in", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCNZ.OK_CustomsRegNo), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false));
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 45m, 45m, 0m, false, true, false);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 0m, false, true, false);
				OrgHeader aUOrgWithSameCodeAsLecacy = Factory.NewWithValidTestData<OrgHeader>();
				aUOrgWithSameCodeAsLecacy.OH_Code = lSCAU.OK_CustomsRegNo;
				Factory.Save();
				AssertExceptionThrown("AU company should match org code before Legacy Code", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(aUOrgWithSameCodeAsLecacy.OH_Code), LedgerTypes.AccountsPayable, 45m, 45, 0m, false, true, false));
				AssertExceptionThrown("AU company should match org code before Legacy Code", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(aUOrgWithSameCodeAsLecacy.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 0m, false, true, false));
			}

			using (new TemporaryUserContext() { BranchPK = nZBranch.PK.ToGuid() }.Set())
			{
				org.CompanyData.OB_IsCreditor = org.CompanyData.OB_IsCreditor = true;
				Factory.Save();
				AccTransactionHeader aRInv2 = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 80m);
				aRInv2.AH_InvoiceAmount = 80M;
				AccTransactionHeader aPInv2 = GetNewInvoice(org, LedgerTypes.AccountsPayable, -50m);
				aPInv2.AH_InvoiceAmount = -50M;
				Factory.Save();
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCNZ.OK_CustomsRegNo), LedgerTypes.AccountsPayable, 50m, 50m, 0m, false, true, false);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCNZ.OK_CustomsRegNo), LedgerTypes.AccountsReceivable, 80m, 80m, 0m, false, true, false);
				AssertExceptionThrown("NZ company should not match org because AU Legacy System Code was passed in", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCAU.OK_CustomsRegNo), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false));
				AssertExceptionThrown("NZ company should not match org because AU Legacy System Code was passed in", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(lSCAU.OK_CustomsRegNo), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false));
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 50m, 50m, 0m, false, true, false);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 0m, false, true, false);
				OrgHeader nZOrgWithSameCodeAsLecacy = Factory.NewWithValidTestData<OrgHeader>();
				nZOrgWithSameCodeAsLecacy.OH_Code = lSCNZ.OK_CustomsRegNo;
				Factory.Save();
				AssertExceptionThrown("NZ company should match org code before Legacy Code", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(nZOrgWithSameCodeAsLecacy.OH_Code), LedgerTypes.AccountsPayable, 50m, 50m, 0m, false, true, false));
				AssertExceptionThrown("NZ company should match org code before Legacy Code", typeof(InvalidOperationException),
					() => AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(nZOrgWithSameCodeAsLecacy.OH_Code), LedgerTypes.AccountsReceivable, 80m, 80m, 0m, false, true, false));
			}
		}

		public void TestGetCreditLimitAndBalanceFromWebServiceWithoutNullReferenceException()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Invalid data was retrieved from an external system. (The CreditLimitAndBalanceDetails collection should contain one element but the collection was null)'.";

			AssertNoExceptionThrown("Null returned instead of Balance Infos",
				() => AssertAllPropertiesErrorMessage(new OrgCreditLimitAndBalanceDetails("NULLD"), expectedErrorMessageWebService));

			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'External system returned null response.'.";
			AssertNoExceptionThrown("Null Web Response returned",
				() => AssertAllPropertiesErrorMessage(new OrgCreditLimitAndBalanceDetails("NULLRESP"), expectedErrorMessageWebService));
		}

		public void TestUnpostedRevenueFromSystemDB()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var creator = new TestObjectCreator(Factory);

			var orgHeader = creator.ABIGAS;
			orgHeader.CompanyData.OB_IsDebtor = true;
			orgHeader.CompanyData.OB_IsCreditor = true;

			var orgHeader2 = creator.AALSHI;
			orgHeader2.CompanyData.OB_IsDebtor = true;
			orgHeader2.CompanyData.OB_IsCreditor = true;

			var chargeCode = creator.CC1;
			var job = creator.CreateJobHeader();
			job.JH_GB = creator.NonCurrentBranch.PK;
			job.JH_GE = creator.FESDepartment.PK;

			CreateJobCharge(job, chargeCode, orgHeader, 55m, false);
			CreateJobCharge(job, chargeCode, orgHeader, 44m, false);
			CreateJobCharge(job, chargeCode, orgHeader2, 61m, false);

			CreateJobCharge(job, chargeCode, orgHeader, 22m, true);
			CreateJobCharge(job, chargeCode, orgHeader, 11m, true);
			CreateJobCharge(job, chargeCode, orgHeader2, 78m, true);

			Factory.Save();

			IOrgCreditLimitAndBalanceDetails details = new OrgCreditLimitAndBalanceDetails(orgHeader.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 99m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 33m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));

			details = new OrgCreditLimitAndBalanceDetails(orgHeader2.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 61m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 78m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));
		}

		public void TestUnpostedRevenueFromWebServiceAndSystemDB()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var orgHeader = creator.AALSHI;
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsCreditor = true;

			var orgHeader2 = creator.ABIGAS;
			orgHeader2.OH_IsDebtor = true;
			orgHeader2.OH_IsCreditor = true;

			var chargeCode = creator.CC1;

			var job = creator.CreateJobHeader();
			job.JH_GB = creator.NonCurrentBranch.PK;
			job.JH_GE = creator.FESDepartment.PK;

			CreateJobCharge(job, chargeCode, orgHeader, 55m, false);
			CreateJobCharge(job, chargeCode, orgHeader, 44m, false);
			CreateJobCharge(job, chargeCode, orgHeader2, 61m, false);

			CreateJobCharge(job, chargeCode, orgHeader, 22m, true);
			CreateJobCharge(job, chargeCode, orgHeader, 11m, true);
			CreateJobCharge(job, chargeCode, orgHeader2, 78m, true);

			Factory.Save();

			IOrgCreditLimitAndBalanceDetails details = new OrgCreditLimitAndBalanceDetails(orgHeader.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 99m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 33m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));

			details = new OrgCreditLimitAndBalanceDetails(orgHeader2.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 61m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 78m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));

			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			details = new OrgCreditLimitAndBalanceDetails(orgHeader.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 100m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 100m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));

			details = new OrgCreditLimitAndBalanceDetails(orgHeader2.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 200m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 200m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));
		}

		public void TestUnpostedRevenueFromWebService()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			using var userContext = creator.SwitchEnvToBranch(branch, department: department);

			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			OrgHeader orgHeader = creator.AALSHI;
			OrgHeader orgHeader2 = creator.ABIGAS;
			orgHeader.CompanyData.OB_IsCreditor = orgHeader2.CompanyData.OB_IsCreditor = true;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			JobCharge jobChargeUnrecognised1 = Factory.New<JobCharge>();
			JobCharge jobChargeUnrecognised2 = Factory.New<JobCharge>();
			JobCharge jobChargeUnrecognised3 = Factory.New<JobCharge>();

			jobChargeUnrecognised1.JR_OH_SellAccount = orgHeader.PK;
			jobChargeUnrecognised1.JR_LocalSellAmt = 55m;
			jobChargeUnrecognised1.JR_OSSellAmt = 55m;
			jobChargeUnrecognised1.JR_AC = chargeCode.PK;
			jobChargeUnrecognised1.JR_JH = jobHeader.PK;

			jobChargeUnrecognised2.JR_OH_SellAccount = orgHeader.PK;
			jobChargeUnrecognised2.JR_LocalSellAmt = 44m;
			jobChargeUnrecognised2.JR_OSSellAmt = 44m;
			jobChargeUnrecognised2.JR_AC = chargeCode.PK;
			jobChargeUnrecognised2.JR_JH = jobHeader.PK;

			jobChargeUnrecognised3.JR_OH_SellAccount = orgHeader2.PK;
			jobChargeUnrecognised3.JR_LocalSellAmt = 61m;
			jobChargeUnrecognised3.JR_OSSellAmt = 61m;
			jobChargeUnrecognised3.JR_AC = chargeCode.PK;
			jobChargeUnrecognised3.JR_JH = jobHeader.PK;

			Factory.Save();

			AccTransactionLines transactionLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionLines transactionLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionLines transactionLine3 = Factory.NewWithValidTestData<AccTransactionLines>();

			transactionLine1.AL_LineType = "WIP";
			transactionLine2.AL_LineType = "WIP";
			transactionLine3.AL_LineType = "WIP";
			transactionLine1.AL_AG = creator.GLHeader1.PK;
			transactionLine2.AL_AG = creator.GLHeader1.PK;
			transactionLine3.AL_AG = creator.GLHeader1.PK;

			JobCharge jobChargeRecognised1 = Factory.New<JobCharge>();
			JobCharge jobChargeRecognised2 = Factory.New<JobCharge>();
			JobCharge jobChargeRecognised3 = Factory.New<JobCharge>();

			jobChargeRecognised1.JR_OH_SellAccount = orgHeader.PK;
			jobChargeRecognised1.JR_LocalSellAmt = 22m;
			jobChargeRecognised1.JR_OSSellAmt = 22m;
			jobChargeRecognised1.JR_AC = chargeCode.PK;
			jobChargeRecognised1.JR_JH = jobHeader.PK;
			jobChargeRecognised1.JR_AL_ARLine = transactionLine1.PK;
			jobChargeRecognised1.SetAmountsToLinkedLinesForTests();

			jobChargeRecognised2.JR_OH_SellAccount = orgHeader.PK;
			jobChargeRecognised2.JR_LocalSellAmt = 11m;
			jobChargeRecognised2.JR_OSSellAmt = 11m;
			jobChargeRecognised2.JR_AC = chargeCode.PK;
			jobChargeRecognised2.JR_JH = jobHeader.PK;
			jobChargeRecognised2.JR_AL_ARLine = transactionLine2.PK;
			jobChargeRecognised2.SetAmountsToLinkedLinesForTests();

			jobChargeRecognised3.JR_OH_SellAccount = orgHeader2.PK;
			jobChargeRecognised3.JR_LocalSellAmt = 78m;
			jobChargeRecognised3.JR_OSSellAmt = 78m;
			jobChargeRecognised3.JR_AC = chargeCode.PK;
			jobChargeRecognised3.JR_JH = jobHeader.PK;
			jobChargeRecognised3.JR_AL_ARLine = transactionLine3.PK;
			jobChargeRecognised3.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			IOrgCreditLimitAndBalanceDetails details = new OrgCreditLimitAndBalanceDetails(orgHeader.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 100.00m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 100.00m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));

			details = new OrgCreditLimitAndBalanceDetails(orgHeader2.OH_Code);

			AssertEquals("Unposted Unrecognized Revenue", 200.00m, details.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable));
			AssertEquals("Unposted Recognized Revenue", 200.00m, details.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable));
		}

		public void TestAllGlobalPropertiesAreGuardedByIsARGlobalCreditApproved()
		{
			var creator = new TestObjectCreator(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = creator.USD.RX_Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 40m;
			org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			var arInv = GetNewInvoice(org, LedgerTypes.AccountsReceivable, 90m);
			arInv.AH_InvoiceAmount = 90M;
			Factory.Save();

			var details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertARGlobalCreditLimitAndBalanceDetails(details, true, 40m, creator.USD.RX_Code, 0m, true, false, true);
			AssertGlobalUnpostedRevenueDetails(details, 0m, 0m, true);

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			Factory.Save();

			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertARGlobalCreditLimitAndBalanceDetails(details, false, 0m, string.Empty, 0m, false, false, false);
			AssertGlobalUnpostedRevenueDetails(details, 0m, 0m, false);

			org.MiscServ.OM_ARGlobalCreditApproved = true;

			var buyExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			buyExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			buyExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			buyExRate.RE_RX_NKExCurrency = creator.USD.RX_Code;
			buyExRate.RE_SellRate = 0.5m;
			buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertARGlobalCreditLimitAndBalanceDetails(details, true, 40m, creator.USD.RX_Code, 45m, true, true, false);

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			Factory.Save();

			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertARGlobalCreditLimitAndBalanceDetails(details, false, 0m, string.Empty, 0m, false, false, false);

			org.MiscServ.OM_ARGlobalCreditApproved = true;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			var chargeCode = creator.CC1;
			var job = creator.CreateJobHeader();
			job.JH_GB = creator.NonCurrentBranch.PK;
			job.JH_GE = creator.FESDepartment.PK;

			CreateJobCharge(job, chargeCode, org, 61m, false);
			CreateJobCharge(job, chargeCode, org, 78m, true);
			Factory.Save();

			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertGlobalUnpostedRevenueDetails(details, 39m, 30.5m, false);

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			Factory.Save();

			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			AssertGlobalUnpostedRevenueDetails(details, 0m, 0m, false);
		}

		#region TestGetCreditLimitAndBalance Error Processing

		public void TestGetCreditLimitAndBalanceShowOnlyRelatedMessages()
		{
			var details = new OrgCreditLimitAndBalanceDetails("XXXXXX");

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var expectedErrorMessageDb = "An error occurred when retrieving data from a database. Data was not found.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageDb);

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageDb, expectedErrorMessageWebService, expectedErrorMessageWebService);

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService, expectedErrorMessageDb, expectedErrorMessageWebService);

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService, expectedErrorMessageWebService, expectedErrorMessageDb);
		}

		public void TestGetCreditLimitAndBalanceShowHighTotalBalanceAmountErrorMessage()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var factory = new BusinessObjectFactory();
				var testCreator = new TestObjectCreator(factory);
				for (int i = 0; i < 10; i++)
				{
					var arInvoice = testCreator.CreateARInvoice<ARInvoice>("001", testCreator.AUD, 1, testCreator.AALSHI);
					testCreator.CreateInvoiceLine(arInvoice, testCreator.AUD, 1, 92233720368567M);

					testCreator.CreateAPInvoice<APInvoice>($"002{i}", testCreator.AUD, 1, 92233720368567M, 0M, 0M, 99999999999999M, 0M, 0M, testCreator.AALSHI);
					factory.Save();
				}

				var details = new OrgCreditLimitAndBalanceDetails(testCreator.AALSHI.OH_Code);
				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var expectedErrorMessageDb = "An error occurred when retrieving data from a database. Data is invalid.";
				AssertAllPropertiesErrorMessage(details, expectedErrorMessageDb);
			}
		}

		public void TestGetCreditLimitAndBalanceWebServiceErrors()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var details = new OrgCreditLimitAndBalanceDetails("XXXXXX");
			var expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebService/Link");
			//error should be cached system should ask for data again and again if error heppened.
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Data was not retrieved.'.";
			var expectedErrorMessageDb = "An error occurred when retrieving data from a database. Data was not found.";
			//cached should be cleared if sources were changed.
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService, expectedErrorMessageWebService, expectedErrorMessageDb);

			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				//error should be cached system should ask for data again and again if error heppened.
				AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

				AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebService/Link");
				details = new OrgCreditLimitAndBalanceDetails("XXXXXX");
				expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'External system can't be initialized. Invalid URI: The format of the URI could not be determined.'.";
				AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);
			}

			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");
			details = new OrgCreditLimitAndBalanceDetails("XXFAIL");
			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'External system failed to process a request: Some fail error message.'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			details = new OrgCreditLimitAndBalanceDetails("EXCEPT");
			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Request error: Some exception error message.'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			details = new OrgCreditLimitAndBalanceDetails("EMPTYD");
			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'No data was retrieved from an external system. (The CreditLimitAndBalanceDetails collection should contain one element but the collection contained 0 elements)'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			details = new OrgCreditLimitAndBalanceDetails("NULL1ST");
			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'No data was retrieved from an external system. The first CreditLimitAndBalanceInfo element in the CreditLimitAndBalanceDetails collection was empty'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);

			details = new OrgCreditLimitAndBalanceDetails("INVALD");
			expectedErrorMessageWebService = @$"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Invalid data was retrieved from an external system. (The CreditLimitAndBalanceDetails collection should contain one element but the collection contained 2 elements)'.";
			AssertAllPropertiesErrorMessage(details, expectedErrorMessageWebService);
		}

		void AssertAllPropertiesErrorMessage(OrgCreditLimitAndBalanceDetails details, string errorMessage)
		{
			AssertAllPropertiesErrorMessage(details, errorMessage, errorMessage, errorMessage);
		}

		void AssertAllPropertiesErrorMessage(OrgCreditLimitAndBalanceDetails details, string errorMessageCreditLimit, string errorMessageOutstandingBalance, string errorMessageUnpostedRevenue)
		{
			AssertErrorMessage(details, "OnCreditHold", errorMessageCreditLimit, x => details.OnCreditHold(x));
			AssertErrorMessage(details, "CreditLimit", errorMessageCreditLimit, x => details.CreditLimit(x));
			AssertErrorMessage(details, "IsOverCreditLimit", errorMessageCreditLimit, x => details.IsOverCreditLimit(x));
			AssertErrorMessage(details, "IsOverCreditTerms", errorMessageCreditLimit, x => details.IsOverCreditTerms(x));
			AssertErrorMessage(details, "OutstandingBalance", errorMessageOutstandingBalance, x => details.OutstandingBalance(x));
			AssertErrorMessage(details, "Claim", errorMessageOutstandingBalance, x => details.Claim(x));
			AssertErrorMessage(details, "OutstandingBalanceOverdue", errorMessageOutstandingBalance, x => details.OutstandingBalanceOverdue(x));
			AssertErrorMessage(details, "OutstandingBalanceNotOverdue", errorMessageOutstandingBalance, x => details.OutstandingBalanceNotOverdue(x));
			AssertErrorMessage(details, "UnpostedRevenue", errorMessageUnpostedRevenue, x => details.UnpostedRevenue(x));
			AssertErrorMessage(details, "UnpostedRevenueRecognised", errorMessageUnpostedRevenue, x => details.UnpostedRevenueRecognised(x));
			AssertErrorMessage(details, "UnpostedRevenueUnrecognised", errorMessageUnpostedRevenue, x => details.UnpostedRevenueUnrecognised(x));
		}

		void AssertErrorMessage(OrgCreditLimitAndBalanceDetails details, string errorPrefix, string errorMessage, Action<string> action)
		{
			try
			{
				action(LedgerTypes.AccountsReceivable);
			}
			catch (InvalidOperationException ex)
			{
				var errorPrefixAR = string.Format("AR {0}.", errorPrefix);
				var errorMessageActual = details.GetErrorMessage(errorPrefixAR, ex);
				AssertEquals(errorPrefixAR + " " + errorMessage, errorMessageActual);
			}

			try
			{
				action(LedgerTypes.AccountsPayable);
			}
			catch (InvalidOperationException ex)
			{
				var errorPrefixAP = string.Format("AP {0}.", errorPrefix);
				var errorMessageActual = details.GetErrorMessage(errorPrefixAP, ex);
				AssertEquals(errorPrefixAP + " " + errorMessage, errorMessageActual);
			}
		}

		public void TestGetClaimFromSystemDB()
		{
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var org = creator.CreateOrgHeader("CLAIM", true, true);
			org.CompanyData.OB_ARCreditLimit = 150m;
			org.CompanyData.OB_APCreditLimit = 150m;
			var orgContact = creator.CreateContact(org);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 150m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 150m, false, false, false);

			var arInv = creator.CreateARInvoice<ARInvoice>("22222", creator.AUD, 1m, org);
			arInv.AH_DueDate = ZDateTime.Now.AddDays(7);
			creator.CreateInvoiceLine(arInv, creator.AUD, 1m, 90m, 0m, creator.GLHeader1.PK);
			var apInv = creator.CreateAPInvoice<APInvoice>("33333", creator.AUD, 1m, 90m, 0m, 0m, 90m, 0m, 0m, org);
			apInv.AH_DueDate = ZDateTime.Now.AddDays(7);
			Factory.Save();

			creator.CreateClaim(typeof(ARAccQueryClaim), 20m, arInv.PK, org.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			creator.CreateClaim(typeof(APAccQueryClaim), 10m, apInv.PK, org.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 90m, 90, 150m, false, false, false);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 150m, false, false, false);

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 90m, 90, 150m, false, false, false, 10m);
				AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 90m, 90m, 150m, false, false, false, 20m);
			}
		}

		public void TestGetClaimFromWebService()
		{
			var creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var org = creator.CreateOrgHeader("CLAIM", true, true);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false, 100m);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false, 100m);
		}

		public void TestWebServiceResponseCanbeProcessedWithoutClaimInfo()
		{
			var creator = new TestObjectCreator(Factory);
			MockCreditLimitServiceClientProvider.Reset();

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var org = creator.CreateOrgHeader("WOCLAIMAMNT", true, true);
			Factory.Save();

			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsPayable, 0m, 0m, 0m, false, false, false, 0m);
			AssertCreditLimitAndBalanceDetails(new OrgCreditLimitAndBalanceDetails(org.OH_Code), LedgerTypes.AccountsReceivable, 0m, 0m, 0m, false, false, false, 0m);
		}

		#endregion

		public void TestGenerateDetailsFieldStrategiesHasUsedCache()
		{
			var creator = new TestObjectCreator(Factory);

			using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx"))
			{
				var details = new OrgCreditLimitAndBalanceDetails(creator.AALSHI.OH_Code);
				var details2 = new OrgCreditLimitAndBalanceDetails(creator.ABIGAS.OH_Code);

				details.ClearDetailsFieldStrategyCacheDictionary_ForTestOnly();

				var cacheKey = GlbCompany.CurrentCompany.GC_Code + "_TTT";
				AssertEquals($"The details field strategy cache for organization {creator.AALSHI.OH_Code} don't contain key {cacheKey}", false, details.IsInDetailsFieldStrategyCacheDictionary_ForTestOnly(cacheKey));
				AssertEquals($"The details field strategy cache for organization {creator.ABIGAS.OH_Code} don't contain key {cacheKey}", false, details2.IsInDetailsFieldStrategyCacheDictionary_ForTestOnly(cacheKey));

				details.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable);

				AssertEquals($"The details field strategy cache for organization {creator.AALSHI.OH_Code} contains key {cacheKey}", true, details.IsInDetailsFieldStrategyCacheDictionary_ForTestOnly(cacheKey));
				AssertEquals($"The cache is global so details field strategy cache for organization {creator.ABIGAS.OH_Code} should contains key {cacheKey} before execute FetchCreditLimitAndBalanceFromWebService", true, details2.IsInDetailsFieldStrategyCacheDictionary_ForTestOnly(cacheKey));
			}
		}

		public void TestReportErrorForCachedValuesFromWebServiceWhenKeyExistsWithNewContent()
		{
			MockCreditLimitServiceClientProvider.Reset();
			var creator = new TestObjectCreator(Factory);

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;

			Factory.Save();

			var orgCreditLimitAndBalanceDetails = new OrgCreditLimitAndBalanceDetails(org.OH_Code);

			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			ErrorReporter.Clear();

			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			AssertEquals("OrgCreditLimitAndBalanceDetails|AnItemWithTheSameKeyHasAlreadyBeenAdded", ErrorReporter.LastKeyReported);
			AssertEqualsIgnoreLineBreaks("Expecting the ErrorReporter content should be displayed", @"An item with the same key has already been added to cachedValuesFromWebService, registry info:
.Use Web Service for Credit Limit: No
.Use Web Service for Outstanding Balance: No
.Use Web Service for Transaction Payment Status: No
.Use Web Service for Unposted Revenue: No
.Enable Background Validation on Billing Tab: Yes
Key details: AR BAL
Existing Values: OnCreditHold:True IsOverCreditLimit:True IsOverCreditTerms:True CreditLimit:1000.00 OutstandingBalance:1500.00 Claim:0 OutstandingBalanceOverdue:500.00 OutstandingBalanceNotOverdue:100.00 UnpostedRevenueRecognised:100.00 UnpostedRevenueUnrecognised:100.00 UnpostedRevenue: IsGlobalCreditApproved:False OnGlobalCreditHold: IsOverGlobalCreditLimit: GlobalCreditCurrencyCode: GlobalCreditGroupOrgCode:tst GlobalCreditLimit: GlobalOutstandingBalance: GlobalClaim: UnableToCalculateGlobalOutstandingBalance: GlobalUnpostedRevenueRecognised: GlobalUnpostedRevenueUnrecognised: UnableToCalculateGlobalUnpostedRevenue: TimeStamp:1/01/0001 12:00:00 AM
New Values: OnCreditHold:True IsOverCreditLimit:True IsOverCreditTerms:True CreditLimit:1000.00 OutstandingBalance:1500.00 Claim:0 OutstandingBalanceOverdue:500.00 OutstandingBalanceNotOverdue:100.00 UnpostedRevenueRecognised:100.00 UnpostedRevenueUnrecognised:100.00 UnpostedRevenue: IsGlobalCreditApproved:False OnGlobalCreditHold: IsOverGlobalCreditLimit: GlobalCreditCurrencyCode: GlobalCreditGroupOrgCode:new GlobalCreditLimit: GlobalOutstandingBalance: GlobalClaim: UnableToCalculateGlobalOutstandingBalance: GlobalUnpostedRevenueRecognised: GlobalUnpostedRevenueUnrecognised: UnableToCalculateGlobalUnpostedRevenue: TimeStamp:1/01/0001 12:00:00 AM", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportErrorForCachedValuesFromWebServiceWhenKeyExistsWithExistingContent()
		{
			MockCreditLimitServiceClientProvider.Reset();
			var creator = new TestObjectCreator(Factory);

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsDebtor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;

			Factory.Save();

			var orgCreditLimitAndBalanceDetails = new OrgCreditLimitAndBalanceDetails(org.OH_Code);

			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			ErrorReporter.Clear();
			MockCreditLimitServiceClientProvider.Reset();

			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			AssertContains(string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportErrorForCachedValuesFromWebServiceWhenNoKeyExists()
		{
			var creator = new TestObjectCreator(Factory);
			var orgCreditLimitAndBalanceDetails = new OrgCreditLimitAndBalanceDetails(creator.GetOrganisation().OH_Code);

			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsReceivable, Constants.BalanceOverdueAgingOption.Balance);
			orgCreditLimitAndBalanceDetails.GetCreditLimitAndBalanceFromWebService_ForTestOnly(LedgerTypes.AccountsPayable, Constants.BalanceOverdueAgingOption.Balance);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			AssertContains(string.Empty, ErrorReporter.LastMessageReported);
		}

		#region Implementation

		void AssertCreditLimitAndBalanceDetails(IOrgCreditLimitAndBalanceDetails details, string ledger,
			decimal outstandingBalance, decimal oustandingBalanceNotOverdue, decimal creditLimit,
			bool onCreditHold, bool isOverCreditLimit, bool isOverCreditTerms, decimal claim = 0m)
		{
			AssertNotNull(details);
			AssertEquals(ledger + " OutstandingBalance", outstandingBalance, details.OutstandingBalance(ledger));
			AssertEquals(ledger + " Claim", claim, details.Claim(ledger));
			AssertEquals(ledger + " OutstandingBalanceNotOverdue", oustandingBalanceNotOverdue, details.OutstandingBalanceNotOverdue(ledger));
			AssertEquals(ledger + " CreditLimit", creditLimit, details.CreditLimit(ledger));
			AssertEquals(ledger + " OnCreditHold", onCreditHold, details.OnCreditHold(ledger));
			AssertEquals(ledger + " IsOverCreditLimit", isOverCreditLimit, details.IsOverCreditLimit(ledger));
			AssertEquals(ledger + " IsOverCreditTerms", isOverCreditTerms, details.IsOverCreditTerms(ledger));
		}

		void AssertARGlobalCreditLimitAndBalanceDetails(IOrgCreditLimitAndBalanceDetails details, bool isCreditApproved,
			decimal creditLimit = 0, string creditCurrency = "", decimal outstandingBalance = 0, bool onCreditHold = false, bool isOverCreditLimit = false,
			bool unableToCalculateBalance = false)
		{
			AssertNotNull(details);
			AssertEquals("Store procedure OrgCreditLimitAndBalanceDetails cannot calculate because missing exchange rate", unableToCalculateBalance, details.UnableToCalculateARGlobalOutstandingBalance);
			AssertEquals("IsARGlobalCreditApproved", isCreditApproved, details.IsARGlobalCreditApproved);
			AssertEquals("ARGlobalCreditLimit", creditLimit, details.ARGlobalCreditLimit);
			AssertEquals("ARGlobalOutstandingBalance", outstandingBalance, details.ARGlobalOutstandingBalance);
			AssertEquals("OnARGlobalCreditHold", onCreditHold, details.OnARGlobalCreditHold);
			AssertEquals("IsOverARGlobalCreditLimit", isOverCreditLimit, details.IsOverARGlobalCreditLimit);
			AssertEquals("ARGlobalCreditCurrencyCode", creditCurrency, details.ARGlobalCreditCurrencyCode);
		}

		void AssertGlobalUnpostedRevenueDetails(IOrgCreditLimitAndBalanceDetails details, decimal unpostedRevenueRecognised, decimal unpostedRevenueUnrecognised, bool unableToCalculateUnpostedRevenue)
		{
			AssertNotNull(details);
			AssertEquals("ARGlobalUnpostedRevenueRecognised", unpostedRevenueRecognised, details.ARGlobalUnpostedRevenueRecognised);
			AssertEquals("ARGlobalUnpostedRevenueUnrecognised", unpostedRevenueUnrecognised, details.ARGlobalUnpostedRevenueUnrecognised);
			AssertEquals("UnableToCalculateARGlobalUnpostedRevenue", unableToCalculateUnpostedRevenue, details.UnableToCalculateARGlobalUnpostedRevenue);
		}

		AccTransactionHeader GetNewInvoice(OrgHeader org, string ledger, decimal outstandingAmount)
		{
			return GetNewTransactionHeader(org, ZArchitecture.Core.TransactionTypes.Invoice, ledger, outstandingAmount);
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = transactionType;
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			return invoice;
		}

		JobCharge CreateJobCharge(JobHeader jobHeader, AccChargeCode chargeCode, OrgHeader sellAccount, decimal sellAmount, bool revenueIsRecognized)
		{
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = jobHeader.JH_GB;
			jobCharge.JR_GE = jobHeader.JH_GE;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OH_SellAccount = sellAccount.PK;
			jobCharge.JR_OSSellAmt = sellAmount;
			jobCharge.JR_LocalSellAmt = sellAmount;

			if (revenueIsRecognized)
			{
				var line = Factory.New<AccTransactionLines>();
				line.AL_LineType = TransactionLineTypes.WIP;
				line.AL_RX_NKTransactionCurrency = jobCharge.JR_RX_NKSellCurrency;
				line.AL_RevRecognitionType = "IMM";
				line.AL_JH = jobHeader.PK;
				line.AL_GB = jobHeader.JH_GB;
				line.AL_GE = jobHeader.JH_GE;
				line.AL_AC = chargeCode.PK;
				line.AL_OH = sellAccount.PK;
				line.AL_PostDate = ZDateTime.Today;
				line.AL_ReverseDate = ZDateTime.Empty;
				jobCharge.JR_AL_ARLine = line.PK;
				jobCharge.SetAmountsToLinkedLinesForTests();
			}

			return jobCharge;
		}

		#endregion

	}
}
