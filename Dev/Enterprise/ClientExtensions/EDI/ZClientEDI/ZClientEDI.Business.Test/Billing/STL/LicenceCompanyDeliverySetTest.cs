using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class LicenceCompanyDeliverySetTest : TestCaseWithFactory
	{
		public void TestGetCompany()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var companySet = new LicenceCompanyDeliverySet(factory2);
			companySet.AddOwnerCompanyPks(new Guid[] { lic1.LA_LC.ToGuid(), lic2.LA_LC.ToGuid() });
			companySet.AddOwnerOrgPks(new Guid[] { lic3.Company.LC_OH.ToGuid() });

			AssertEquals(lic1.Company.PK, companySet.GetCompany(lic1.LA_LC.ToGuid()).PK);
			AssertEquals(lic2.Company.PK, companySet.GetCompany(lic2.LA_LC.ToGuid()).PK);
			AssertEquals(lic3.Company.PK, companySet.GetCompany(lic3.LA_LC.ToGuid()).PK);
			AssertEquals(1, factory2.DatabaseLoadCount);
		}

		public void TestGetDeliveries()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			var lic4 = BillingTestHelper.CreateLicence(Factory, "DDD");

			var delivery1a = lic1.Company.InvoiceDeliveries.AddNew();
			delivery1a.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery1a.L9_RX_NKInvoiceCurrency = "AUD";
			delivery1a.L9_SystemCode = BillingConstants.BillingSystem.Fee;

			var delivery1b = lic1.Company.InvoiceDeliveries.AddNew();
			delivery1b.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery1b.L9_RX_NKInvoiceCurrency = "AUD";
			delivery1b.L9_SystemCode = BillingConstants.BillingSystem.All;

			var delivery3 = lic3.Company.InvoiceDeliveries.AddNew();
			delivery3.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery3.L9_RX_NKInvoiceCurrency = "AUD";
			delivery3.L9_SystemCode = BillingConstants.BillingSystem.All;

			BillingTestHelper.SetInvoicing(lic4, Env.CurrentBranch.PK);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var companySet = new LicenceCompanyDeliverySet(factory2);
			companySet.AddOwnerCompanyPks(new Guid[] { lic1.LA_LC.ToGuid(), lic2.LA_LC.ToGuid() });
			companySet.AddOwnerOrgPks(new Guid[] { lic3.Company.LC_OH.ToGuid() });

			var deliveries1 = companySet.GetDeliveries(lic1.LA_LC.ToGuid());
			var deliveries2 = companySet.GetDeliveries(lic2.LA_LC.ToGuid());

			AssertEquals(true, deliveries1.Any(x => x.PK == delivery1a.PK));
			AssertEquals(true, deliveries1.Any(x => x.PK == delivery1b.PK));
			AssertEquals(2, deliveries1.Count());

			AssertNull(deliveries2);

			AssertEquals(1, factory2.DatabaseLoadCount);
			AssertEquals(2, ((IBusinessObjectFactoryInternals)factory2).NumberOfBusinessObjects);
		}
	}
}