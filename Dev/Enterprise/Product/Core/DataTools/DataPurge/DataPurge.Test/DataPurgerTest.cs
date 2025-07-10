using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	[TestedType(typeof(DataPurger))]
	sealed class DataPurgerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertyInitialValues()
		{
			var purger = new DataPurger();

			AssertEquals("Organisation purge should default to false", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Charge Code purge should default to false", ZBool.False, purger.HasNonSystemChageCodesPurgeScript);
			AssertEquals("Product purge should default to false", ZBool.False, purger.HasProductInfoPurgeScript);
			AssertEquals("Rating purge should default to false", ZBool.False, purger.HasRatingInfoPurgeScript);
			AssertEquals("Tariff purge should default to false", ZBool.False, purger.HasTariffInfoPurgeScript);
			AssertEquals("Quotations purge should default to false", ZBool.False, purger.HasQuotationsPurgeScript);
			AssertEquals("CompanySpecificPk should default to CurrentCompany", GlbCompany.CurrentCompany.PK, purger.CompanySpecificPk);
			AssertEquals("Output should default to empty string", ZString.Empty, purger.Output);
			AssertEquals("Output read-only status", true, purger.OutputInfo.ReadOnly);
		}

		public void TestValidation()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("[PRE-CONDITION] No errors", false, purger.HasErrors);

			var controllerUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_IsController, true));

			using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(controllerUser.GS_LoginName))
			{
				AssertEquals("Purger Bizo should have NO errors", false, purger.HasErrors());
				AssertEquals("Company PK should have NO errors", false, purger.CompanySpecificPkInfo.HasErrors());

				purger.CompanySpecificPk = ZGuid.Empty;

				AssertEquals("Purger Bizo should have errors", true, purger.HasErrors());
				AssertEquals("Company PK should have errors", true, purger.CompanySpecificPkInfo.HasErrors());

				purger.CompanySpecificPk = GlbCompany.CurrentCompany.PK;

				AssertEquals("Purger Bizo should have NO errors", false, purger.HasErrors());
				AssertEquals("Company PK should have NO errors", false, purger.CompanySpecificPkInfo.HasErrors());
			}
		}

		public void TestValidationForNonOperationalLogin()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("[PRE-CONDITION] No errors", false, purger.HasErrors);

			var nonOperationalUser = Factory.New<GlbStaff>();
			nonOperationalUser.GS_Code = "XYD";
			nonOperationalUser.GS_LoginName = "XYDLoginName";
			nonOperationalUser.GS_IsOperational = false;

			using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(nonOperationalUser.GS_LoginName))
			{
				AssertEquals("Purger Bizo should have NO errors", false, purger.HasErrors);
				AssertEquals("Company PK should have NO errors", false, purger.CompanySpecificPkInfo.HasErrors());

				purger.CompanySpecificPk = ZGuid.Empty;

				AssertEquals("Purger Bizo should have errors", true, purger.HasErrors);
				AssertEquals("Company PK should have errors", true, purger.CompanySpecificPkInfo.HasErrors());

				purger.CompanySpecificPk = GlbCompany.CurrentCompany.PK;

				AssertEquals("Purger Bizo should have NO errors", false, purger.HasErrors);
				AssertEquals("Company PK should have NO errors", false, purger.CompanySpecificPkInfo.HasErrors());
			}
		}

		public void TestTurningOrganisationScriptOnAlsoTurnsOnRating()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("Organisation purge should be off by default", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge should be off by default", ZBool.False, purger.HasRatingInfoPurgeScript);

			purger.HasRatingInfoPurgeScript = true;

			AssertEquals("Organisation purge should remain off", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge explicitly turned on", ZBool.True, purger.HasRatingInfoPurgeScript);

			purger.HasRatingInfoPurgeScript = false;

			AssertEquals("Organisation purge should remain off", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge explicitly turned off", ZBool.False, purger.HasRatingInfoPurgeScript);

			purger.HasNonProxyOrganisationsPurgeScript = true;

			AssertEquals("Organisation purge explicitly turned on", ZBool.True, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge should be automatically turned on", ZBool.True, purger.HasRatingInfoPurgeScript);
			AssertEquals("Quotations purge should be automatically turned on", ZBool.True, purger.HasQuotationsPurgeScript);

			purger.HasNonProxyOrganisationsPurgeScript = false;

			AssertEquals("Organisation purge explicitly turned off", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge should remain on", ZBool.True, purger.HasRatingInfoPurgeScript);
		}

		public void TestTurningRatingScriptOffAlsoTurnsOffOrganisation()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("Organisation purge should be off by default", false, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge should be off by default", false, purger.HasRatingInfoPurgeScript);

			purger.HasNonProxyOrganisationsPurgeScript = true;

			AssertEquals("Organisation purge explicitly turned on", true, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge should be automatically turned on", true, purger.HasRatingInfoPurgeScript);

			purger.HasRatingInfoPurgeScript = false;

			AssertEquals("Organisation purge should be automatically turned off", false, purger.HasNonProxyOrganisationsPurgeScript);
			AssertEquals("Rating purge explicitly turned off", false, purger.HasRatingInfoPurgeScript);
		}

		public void TestTurningQuotationsScriptOffAlsoTurnsOffRating()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("Quotations purge should be off by default", ZBool.False, purger.HasQuotationsPurgeScript);
			AssertEquals("Rating purge should be off by default", ZBool.False, purger.HasRatingInfoPurgeScript);

			purger.HasQuotationsPurgeScript = true;

			AssertEquals("Quotations purge explicitly turned on", ZBool.True, purger.HasQuotationsPurgeScript);
			AssertEquals("Rating purge remains turend off", ZBool.False, purger.HasRatingInfoPurgeScript);

			purger.HasRatingInfoPurgeScript = true;

			AssertEquals("Quotations purge should remain on", ZBool.True, purger.HasQuotationsPurgeScript);
			AssertEquals("Rating purge explicitly turned on", ZBool.True, purger.HasRatingInfoPurgeScript);

			purger.HasQuotationsPurgeScript = false;

			AssertEquals("Quotations purge explicitly turned off", ZBool.False, purger.HasQuotationsPurgeScript);
			AssertEquals("Rating purge should be automatically turned off", ZBool.False, purger.HasRatingInfoPurgeScript);
			AssertEquals("Organisation purge should be automatically turned off", ZBool.False, purger.HasNonProxyOrganisationsPurgeScript);

			purger.HasQuotationsPurgeScript = true;

			AssertEquals("Quotations purge explicitly turned on", ZBool.True, purger.HasQuotationsPurgeScript);
			AssertEquals("Rating purge should remain off", ZBool.False, purger.HasRatingInfoPurgeScript);
		}

		public void TestTurningRatingScriptOnAlsoTurnsOnQuotations()
		{
			DataPurger purger = new DataPurger();
			AssertEquals("Rating purge should be off by default", false, purger.HasRatingInfoPurgeScript);
			AssertEquals("Quotations purge should be off by default", false, purger.HasQuotationsPurgeScript);

			purger.HasRatingInfoPurgeScript = true;

			AssertEquals("Rating purge explicitly turned on", true, purger.HasRatingInfoPurgeScript);
			AssertEquals("Quotations purge should be automatically turned on", true, purger.HasQuotationsPurgeScript);

			purger.HasRatingInfoPurgeScript = false;

			AssertEquals("Rating purge explicitly turned off", false, purger.HasRatingInfoPurgeScript);
			AssertEquals("Quotations purge should remain on", true, purger.HasQuotationsPurgeScript);
		}
	}
}
