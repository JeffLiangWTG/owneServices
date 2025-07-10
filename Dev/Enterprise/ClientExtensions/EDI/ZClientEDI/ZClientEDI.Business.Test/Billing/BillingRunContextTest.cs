using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingRunContextTest : TestCaseWithFactory
	{
		public void TestConstructor_NullArgument()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				BillingRunContext context = new BillingRunContext(null, ZDateTime.Today, new ZDateTime());
			});

			AssertExceptionThrown("DateTo is invalid or empty", typeof(ArgumentException), delegate
			{
				BillingRunContext context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, ZDateTime.Invalid);
			});

			AssertExceptionThrown("DateTo is invalid or empty", typeof(ArgumentException), delegate
			{
				BillingRunContext context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, ZDateTime.Empty);
			});

			AssertExceptionThrown("OrganisationPK is invalid", typeof(ArgumentException), delegate
			{
				BillingRunContext context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, ZDateTime.Empty, ZGuid.Invalid);
			});
		}

		public void TestReadonlyProperties()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var dateTo = new ZDateTime(2010, 8, 31);
			ZGuid organisationPK = ZGuid.NewZGuid();

			BillingRunContext context = new BillingRunContext(factory, ZDateTime.Today, dateTo, organisationPK);
			AssertEquals(factory, context.Factory);
			AssertEquals(dateTo, context.DateToInclusive);
			AssertEquals(organisationPK, context.OrganisationPK);
		}

		public void TestBillingGroupPKs()
		{
			EDIOrgHeader randomOrganisation1 = CreateOrganisation(null);
			EDIOrgHeader randomOrganisation2 = CreateOrganisation(randomOrganisation1);
			EDIOrgHeader randomOrganisation3 = CreateOrganisation(null);
			EDIOrgHeader randomOrganisation4 = CreateOrganisation(null);

			EDIOrgHeader parent1 = CreateOrganisation(null);
			EDIOrgHeader child11 = CreateOrganisation(parent1);
			EDIOrgHeader child12 = CreateOrganisation(parent1);
			EDIOrgHeader child13 = CreateOrganisation(parent1);
			var db12 = child12.LicCompany.LicDatabases.AddNew();
			var db13 = child13.LicCompany.LicDatabases.AddNew();

			EDIOrgHeader partner = CreateOrganisation(null);
			partner.LicCompany.SelfBilling.L4_IsPartner = true;

			EDIOrgHeader parent2 = CreateOrganisation(partner);
			EDIOrgHeader child21 = CreateOrganisation(parent2);

			var lic5 = BillingTestHelper.CreateLicence(Factory, "OR4");
			var org5 = lic5.Company.Header;
			var lic6 = BillingTestHelper.CreateAnotherLicence(lic5, "OR5");
			var otherProductLic5 = BillingTestHelper.CreateAnotherDatabase(lic5, "DLV", false);
			otherProductLic5.Database.LD_Product = "DLV";
			var otherProductLic7 = BillingTestHelper.CreateAnotherLicence(otherProductLic5, "DL2", createClientCompany: false);

			var validFrom = new ZDateTime(2010, 1, 1);
			var validTo = new ZDateTime(2010, 12, 31);
			var sharedCommitmentLic1 = BillingTestHelper.CreateLicence(Factory, "SH1");
			var sharedCommitmentLic2 = BillingTestHelper.CreateLicence(Factory, "SH2");
			var nonSharedCommitmentLic1 = BillingTestHelper.CreateLicence(Factory, "NS1");
			BillingTestHelper.CreateCommitment(sharedCommitmentLic1.Database, validFrom, validTo, 1000, "SHARED");
			BillingTestHelper.CreateCommitment(sharedCommitmentLic2.Database, validFrom, validTo, 1000, "SHARED");
			BillingTestHelper.CreateCommitment(nonSharedCommitmentLic1.Database, validFrom, validTo, 1000);

			EDIOrgHeader parent3 = CreateOrganisation(null);
			EDIOrgHeader child31 = CreateOrganisation(null);
			EDIOrgHeader child32 = CreateOrganisation(null);
			var db3 = parent3.LicCompany.LicDatabases.AddNew();
			var setting3 = db3.LicenceSettings.AddNew();
			setting3.LS9_Type = BillingConstants.LicenceSetting.BuyingGroup;
			setting3.LS9_Name = parent3.OH_Code;
			var db31 = child31.LicCompany.LicDatabases.AddNew();
			var setting31 = db31.LicenceSettings.AddNew();
			setting31.LS9_Type = BillingConstants.LicenceSetting.BuyingGroup;
			setting31.LS9_Name = parent3.OH_Code;
			var db32 = child32.LicCompany.LicDatabases.AddNew();
			var setting32 = db32.LicenceSettings.AddNew();
			setting32.LS9_Type = BillingConstants.LicenceSetting.BuyingGroup;
			setting32.LS9_Name = parent3.OH_Code;

			Factory.Save();

			AssertBillingGroupOrgPKs(parent1, new EDIOrgHeader[] { parent1, child11, child12, child13 });
			AssertBillingGroupOrgPKs(child11, new EDIOrgHeader[] { parent1, child11, child12, child13 });
			AssertBillingGroupOrgPKs(child12, new EDIOrgHeader[] { parent1, child11, child12, child13 });
			AssertBillingGroupOrgPKs(child13, new EDIOrgHeader[] { parent1, child11, child12, child13 });

			AssertBillingGroupOrgPKs(parent2, new EDIOrgHeader[] { parent2, child21 });
			AssertBillingGroupOrgPKs(child21, new EDIOrgHeader[] { parent2, child21 });

			AssertBillingGroupOrgPKs(partner, new EDIOrgHeader[] { partner });

			AssertBillingGroupOrgPKs(org5, new EDIOrgHeader[] { org5, lic6.Company.Header });

			AssertBillingGroupDatabasePKs(parent1, new LicenceDatabase[] { db12, db13 });
			AssertBillingGroupDatabasePKs(child11, new LicenceDatabase[] { db12, db13 });
			AssertBillingGroupDatabasePKs(parent2, Array.Empty<LicenceDatabase>());
			AssertBillingGroupDatabasePKs(org5, new LicenceDatabase[] { lic5.Database });
			AssertBillingGroupDatabasePKs(child31, new LicenceDatabase[] { db3, db31, db32 });

			// Commitments
			AssertBillingGroupOrgPKs(sharedCommitmentLic1.Company.Header, new EDIOrgHeader[] { sharedCommitmentLic1.Company.Header, sharedCommitmentLic2.Company.Header });
			AssertBillingGroupOrgPKs(sharedCommitmentLic2.Company.Header, new EDIOrgHeader[] { sharedCommitmentLic1.Company.Header, sharedCommitmentLic2.Company.Header });
			AssertBillingGroupOrgPKs(nonSharedCommitmentLic1.Company.Header, new EDIOrgHeader[] { nonSharedCommitmentLic1.Company.Header });

			AssertBillingGroupDatabasePKs(sharedCommitmentLic1.Company.Header, new LicenceDatabase[] { sharedCommitmentLic1.Database, sharedCommitmentLic2.Database });
			AssertBillingGroupDatabasePKs(sharedCommitmentLic2.Company.Header, new LicenceDatabase[] { sharedCommitmentLic1.Database, sharedCommitmentLic2.Database });
			AssertBillingGroupDatabasePKs(nonSharedCommitmentLic1.Company.Header, new LicenceDatabase[] { nonSharedCommitmentLic1.Database });
		}

		#region Implementation

		ZString BillingGroupPKsAsText(ZGuid[] pks)
		{
			return "'" + string.Join("', '", Array.ConvertAll(pks, x => x.ToString())) + "'";
		}

		void AssertBillingGroupOrgPKs(EDIOrgHeader organisation, EDIOrgHeader[] expectedGroup)
		{
			BillingRunContext context = new BillingRunContext(Factory, new ZDateTime(2010, 8, 31), new ZDateTime(2010, 8, 31), organisation.PK);
			AssertContainsExactElementsInAnyOrder(Array.ConvertAll(expectedGroup, x => x.PK), context.BillingGroupOrgPKs);

			ZString billingGroupPKs = BillingGroupPKsAsText(context.BillingGroupOrgPKs);
			AssertEquals("Not empty", false, billingGroupPKs.IsEmpty);

			foreach (EDIOrgHeader groupOrganisation in expectedGroup)
			{
				if (!billingGroupPKs.Contains(groupOrganisation.PK.ToString()))
				{
					Fail("BillingGroupPKs does not contain expected PK");
				}

				billingGroupPKs = billingGroupPKs.Replace(groupOrganisation.PK.ToString(), "");
			}

			billingGroupPKs = billingGroupPKs.Replace(" ", "");
			billingGroupPKs = billingGroupPKs.Replace(",", "");
			billingGroupPKs = billingGroupPKs.Replace("'", "");

			AssertEquals("BillingGroupPKs does not contain unrelated PK's", 0, billingGroupPKs.Length);
		}

		void AssertBillingGroupDatabasePKs(EDIOrgHeader organisation, LicenceDatabase[] expectedGroup)
		{
			BillingRunContext context = new BillingRunContext(Factory, new ZDateTime(2010, 8, 31), new ZDateTime(2010, 8, 31), organisation.PK);
			AssertContainsExactElementsInAnyOrder(Array.ConvertAll(expectedGroup, x => x.PK), context.BillingGroupDatabasePKs);

			ZString billingGroupPKs = BillingGroupPKsAsText(context.BillingGroupDatabasePKs);
			AssertEquals("Not empty", false, billingGroupPKs.IsEmpty);

			foreach (var bizo in expectedGroup)
			{
				if (!billingGroupPKs.Contains(bizo.PK.ToString()))
				{
					Fail("BillingGroupPKs does not contain expected PK");
				}

				billingGroupPKs = billingGroupPKs.Replace(bizo.PK.ToString(), "");
			}

			billingGroupPKs = billingGroupPKs.Replace(" ", "");
			billingGroupPKs = billingGroupPKs.Replace(",", "");
			billingGroupPKs = billingGroupPKs.Replace("'", "");

			AssertEquals("BillingGroupPKs does not contain unrelated PK's", 0, billingGroupPKs.Length);
		}

		EDIOrgHeader CreateOrganisation(EDIOrgHeader parentOrganisation)
		{
			EDIOrgHeader result = Factory.NewWithValidTestData<EDIOrgHeader>();
			result.CreateAndLoadLicenceForOrg();
			result.LicEnterprise.LE_EnterpriseCode = fountainNumber++.ToString();

			if (parentOrganisation != null)
			{
				result.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = parentOrganisation.PK;
			}

			return result;
		}

		static int fountainNumber = 100;

		#endregion
	}
}
