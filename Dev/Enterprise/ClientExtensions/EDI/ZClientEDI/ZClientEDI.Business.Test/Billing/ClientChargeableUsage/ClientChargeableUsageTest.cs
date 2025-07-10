using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientChargeableUsage))]
	internal class ClientChargeableUsageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			ClientChargeableUsage chargeableUsage = Factory.New<ClientChargeableUsage>();
			AssertEquals(null, chargeableUsage.LicenceCompany);
			AssertEquals(null, chargeableUsage.ClientCompany);
			AssertEquals(null, chargeableUsage.Database);
			AssertEquals(ZGuid.Empty, chargeableUsage.OrganisationPK);
		}

		public void TestRelatedObjects()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var chargeableUsage = Factory.New<ClientChargeableUsage>();
			chargeableUsage.U1_LC = organisation.LicCompany.PK;

			AssertEquals(organisation.LicCompany, chargeableUsage.LicenceCompany);
			AssertEquals(organisation.PK, chargeableUsage.OrganisationPK);
			AssertEquals(null, chargeableUsage.ClientCompany);
			AssertEquals(null, chargeableUsage.Database);

			var licenceHeader1 = organisation.LicCompany.LicHeadersForAllDatabases[0];
			var licenceHeader2 = BillingTestHelper.CreateAnotherDatabase(licenceHeader1, "BBB");
			licenceHeader2.Database.LD_OH_BillingParty = licenceHeader2.Company.LC_OH;

			var clientCompanyOnDb1 = licenceHeader1.ClientCompany;
			var clientCompanyOnDb2 = licenceHeader2.ClientCompany;

			chargeableUsage.U1_LD = licenceHeader1.Database.PK;
			AssertEquals(licenceHeader1.Database, chargeableUsage.Database);
			AssertEquals(null, chargeableUsage.ClientCompany);

			chargeableUsage.U1_LCC = clientCompanyOnDb2.PK;
			AssertEquals(licenceHeader1.Database, chargeableUsage.Database);
			AssertEquals(clientCompanyOnDb2, chargeableUsage.ClientCompany);

			var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
			chargeableUsage2.U1_LCC = clientCompanyOnDb1.PK;
			AssertEquals(organisation.PK, chargeableUsage.OrganisationPK);

			var chargeableUsage3 = Factory.New<ClientChargeableUsage>();
			chargeableUsage2.U1_LD = licenceHeader2.LA_LD;
			AssertEquals(organisation.PK, chargeableUsage.OrganisationPK);
		}

		public void TestLastGroupUsageInDateRange()
		{
			EDIOrgHeader orgWithNoLicence = Factory.NewWithValidTestData<EDIOrgHeader>();

			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			LicenceHeader anotherLic = BillingTestHelper.CreateLicence(Factory, "XYZ");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(anotherLic, lic.Company);

			Factory.Save();

			ZDateTime currentPeriod = new ZDateTime(2010, 11, 1);
			AssertNull("no usage", ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod, null));
			AssertNull("no usage", ClientChargeableUsage.LastGroupUsageInDateRange(Factory, orgWithNoLicence, ZDateTime.Empty, currentPeriod, null));

			ClientChargeableUsage chargeableUsage1 = Factory.New<ClientChargeableUsage>();
			chargeableUsage1.U1_LC = lic.LA_LC;
			chargeableUsage1.U1_PeriodStart = currentPeriod;
			chargeableUsage1.U1_Code = BillingConstants.BillingSystem.ODM;
			Factory.Save();

			AssertNull("only later usage", ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod, null));
			AssertNull("only earlier usage", ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, currentPeriod.AddDays(1), ZDateTime.Empty, null));

			ClientChargeableUsage chargeableUsage2 = Factory.New<ClientChargeableUsage>();
			chargeableUsage2.U1_LC = lic.LA_LC;
			chargeableUsage2.U1_PeriodStart = currentPeriod.AddMonths(-2);
			chargeableUsage2.U1_Code = BillingConstants.BillingSystem.Fax;
			ClientChargeableUsage chargeableUsage3 = Factory.New<ClientChargeableUsage>();
			chargeableUsage3.U1_LC = lic.LA_LC;
			chargeableUsage3.U1_PeriodStart = currentPeriod.AddMonths(-3);
			chargeableUsage3.U1_Code = BillingConstants.BillingSystem.eBACCA;
			Factory.Save();
			AssertEquals("last usage found", chargeableUsage2.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod, null).PK);
			AssertEquals("last usage found", chargeableUsage3.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod.AddMonths(-2), null).PK);
			AssertNull("no ISF usage", ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod,
				new string[] { BillingConstants.BillingSystem.ImporterSecurityFiling }));
			AssertEquals("last FAX/eBacca usage found", chargeableUsage2.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod,
				new string[] { BillingConstants.BillingSystem.Fax, BillingConstants.BillingSystem.eBACCA }).PK);
			AssertEquals("last FAX usage found", chargeableUsage2.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod,
				new string[] { BillingConstants.BillingSystem.Fax, BillingConstants.BillingSystem.eBACCA }).PK);
			AssertEquals("last eBACCa usage found", chargeableUsage3.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod,
				new string[] { BillingConstants.BillingSystem.eBACCA }).PK);

			ClientChargeableUsage chargeableUsage4 = Factory.New<ClientChargeableUsage>();
			chargeableUsage4.U1_LC = anotherLic.LA_LC;
			chargeableUsage4.U1_PeriodStart = currentPeriod.AddMonths(-1);
			chargeableUsage4.U1_Code = BillingConstants.BillingSystem.ImporterSecurityFiling;
			Factory.Save();
			AssertEquals("last child usage found", chargeableUsage4.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod, null).PK);
			AssertEquals("last ISF usage found", chargeableUsage4.PK, ClientChargeableUsage.LastGroupUsageInDateRange(Factory, lic.Company.Header, ZDateTime.Empty, currentPeriod,
				new string[] { BillingConstants.BillingSystem.ImporterSecurityFiling }).PK);
		}

		[TestDate(2015, 12, 11, 9, 15, 0)]
		public void TestSystemCreateTimeUtc()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			Factory.Save();
			var usage = Factory.New<ClientChargeableUsage>();
			usage.U1_LCC = lic.ClientCompany.PK;
			usage.U1_LD = lic.LA_LD;
			usage.U1_Code = "ISF";
			usage.U1_PeriodStart = new ZDateTime(2015, 12, 1);
			Factory.Save();

			var usageFromDb = new BusinessObjectFactory().Load<ClientChargeableUsage>(usage.PK);
			AssertEquals(TestDateAttribute.Date, usageFromDb.U1_SystemCreateTimeUtc);
		}

		public void TestGetInAnotherFactory()
		{
			Factory.RefreshEnabled = false;
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2018, 8, 1), lic, 11);
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var usageInFactory2 = usage.GetInAnotherFactory(factory2);
			var usageInFactory2again = usage.GetInAnotherFactory(factory2);
			var usageInSameFactory = usageInFactory2.GetInAnotherFactory(factory2);

			AssertEquals(usage.PK, usageInFactory2.PK);
			AssertEquals(usageInFactory2, usageInFactory2again);
			AssertEquals(usageInFactory2, usageInSameFactory);
		}
	}
}
