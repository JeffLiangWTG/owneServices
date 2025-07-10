using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Billing.TestingEnvironment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[UseSnapshotProtection]
	public class BillingTestingEnvironmentSynchronizerTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		[TestDate(2023, 11, 1)]
		[SnailTest]
		public void TestSynchronise()
		{
			new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);
			EDIDataRegistry.Instance.EDIBillingTestingDatabaseServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Server");
			EDIDataRegistry.Instance.EDIBillingTestingDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Database");
			var login = new ServerUsernamePasswordConfiguration();
			login.UserName = "user";
			login.Password = "password";
			login.ConfirmPassword = "password";
			EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, login);

			var factory1 = new BusinessObjectFactory();
			var lic = BillingTestHelper.CreateLicence(factory1, "AAA");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic.Company.Header.OH_FullName = "OH_FullName";
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");

			var prices = lic.Company.PriceHeaders.AddNew();
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			var rate1 = item1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 17.5;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2023, 1, 1);

			BillingTestHelper.CreateChargeableUsage(factory1, "STL", "USR", new ZDateTime(2023, 10, 1), lic, 100);

			BillingTestHelper.CreateChargeCodeForBranch(factory1, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value)
				.AC_ChargeType = Core.Constants.ChargeType.Comment;
			factory1.Save();

			BillingTestHelper.LoadClientSpecificDocuments();
			var billing = new StlBilling(factory1);
			billing.DateTo = new ZDateTime(2023, 10, 31);
			billing.GenerateReport(null);

			var bill = billing.Bills.Cast<StlBill>().Single();
			AssertEquals(1750m, bill.InvoicePostDiscountTotal);
			var invoice = billing.CreateInvoices(bill).Single();
			invoice.Factory.Save();

			var logger = new SimpleLogger();
			new TestingEnvironmentSynchronizerForTest(logger).Synchronise();
			var logs = logger.ToString();

			AssertStartsWith("Ensure sensitive information is masked for new tables and review the list of deleted tables",
				@"Generating Staging Table AccBankAccount ...
Generating Staging Table AccChargeCode ...
Generating Staging Table AccChargeTaxOverride ...
Generating Staging Table AccGLHeader ...
Generating Staging Table AccGroups ...
Generating Staging Table AccInvMsg ...
Generating Staging Table AccOrgTaxConfigurationTemplate ...
Generating Staging Table AccPeriodManagement ...
Generating Staging Table AccTaxOverrideGroup ...
Generating Staging Table AccTaxRate ...
Generating Staging Table AccTransactionHeader ...
Generating Staging Table AccTransactionLines ...
Generating Staging Table ClientChargeableUsage ...
Generating Staging Table ClientCompany ...
Generating Staging Table ClientInvoiceDelivery ...
Generating Staging Table ClientLicenceBilling ...
Generating Staging Table ClientLicenceFee ...
Generating Staging Table ClientLicencePriceHeader ...
Generating Staging Table ClientLicencePriceItem ...
Generating Staging Table ClientPremiumService ...
Generating Staging Table EdiDepositBalance ...
Generating Staging Table EdiDepositBalanceChanges ...
Generating Staging Table EdiLicenceDatabaseConsolidationHistory ...
Generating Staging Table EdiBilledUsage ...
Generating Staging Table EdiBilledDiscount ...
Generating Staging Table EdiLicenceSetting ...
Generating Staging Table EdiOrgMembership ...
Generating Staging Table EdiPriceDiscountGroupMember ...
Generating Staging Table EdiPriceHeaderDiscount ...
Generating Staging Table EdiPriceHeaderExchangeRate ...
Generating Staging Table EdiPriceHeaderLink ...
Generating Staging Table EdiPriceItemRate ...
Generating Staging Table EdiPriceUsageMapping ...
Generating Staging Table EdiUsageInvoice ...
Generating Staging Table GlbBranch ...
Generating Staging Table GlbCompany ...
Generating Staging Table GlbDepartment ...
Generating Staging Table LicenceCompany ...
Generating Staging Table LicenceDatabase ...
Generating Staging Table LicenceEnterprise ...
Generating Staging Table LicenceHeader ...
Generating Staging Table LicenceModules ...
Generating Staging Table OrgCompanyData ...
Generating Staging Table OrgCreditorGroup ...
Generating Staging Table OrgDebtorGroup ...
Generating Staging Table OrgHeader ...
Generating Staging Table RefShippingLine ...
Generating Staging Table ReleaseBuild ...
Generating Staging Table StmALog ...
Generating Staging Table StmData ...
Generating Staging Table ZZRefExchangeRate ...
Generating Table Dependencies ...
Deleting ", logs);

			var newFactory = new BusinessObjectFactory();
			var branch = newFactory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			AssertNotNull(branch);
			var org = newFactory.Load<EDIOrgHeader>(lic.Company.Header.PK);
			AssertNotEquals(org.OH_FullName, "OH_FullName");
			AssertStartsWith("The OH_FullName should be masked", "S", org.OH_FullName);

			var billing2 = new StlBilling(newFactory);
			billing2.DateTo = new ZDateTime(2023, 10, 31);
			billing2.GenerateReport(null);

			var bill2 = billing2.Bills.Cast<StlBill>().Single();
			AssertEquals(1750m, bill2.InvoicePostDiscountTotal);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		class TestingEnvironmentSynchronizerForTest : TestingEnvironmentSynchronizer
		{
			public TestingEnvironmentSynchronizerForTest(ILogger logger) : base(logger)
			{
			}

			protected override DbConnection NewTargetServerConnection()
			{
				return Db.NewAdminConnection();
			}

			protected override bool Validate() => true;
		}
	}
}
