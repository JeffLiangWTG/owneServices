using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementDatabaseWrapper))]
	class EdiCommissionAgreementDatabaseWrapperTest : EdiCommissionAgreementTreeBizObjWrapperTestCase<EdiCommissionAgreementDatabaseWrapper>
	{
		public void TestSelected()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllDatabases = false;
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var databaseWrapper = new EdiCommissionAgreementDatabaseWrapper(customization, licenceDatabase, null);
			databaseWrapper.Selected = true;
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase.PK }, customization.DatabasePivots.Select(x => x.EZD_LD));
			databaseWrapper.Selected = false;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), customization.DatabasePivots.Select(x => x.EZD_LD));
		}

		public void TestCode()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_ServerCode = "AAA";
			licenceDatabase.LD_LicenceType = "PRD";
			var databaseWrapper = new EdiCommissionAgreementDatabaseWrapper(customization, licenceDatabase, null);
			AssertEquals("AAA (PRD)", databaseWrapper.Code);
			var newDatabaseWrapper = new EdiCommissionAgreementDatabaseWrapper(customization, null, null);
			AssertEquals("[New Databases]", newDatabaseWrapper.Code);
		}

		public void TestDescription()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var wrapper = new EdiCommissionAgreementDatabaseWrapper(customization, null, null);
			AssertEquals("", wrapper.Description);
		}

		public void TestShouldAutoAdd()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllCompanies = false;
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var databaseWrapper = new EdiCommissionAgreementDatabaseWrapper(customization, licenceDatabase, null);
			databaseWrapper.ShouldAutoAdd = true;
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase.PK }, customization.CompanyAutoAddDatabases.Select(x => x.EPD_LD));
			databaseWrapper.ShouldAutoAdd = false;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), customization.CompanyAutoAddDatabases.Select(x => x.EPD_LD));
			AssertEquals(false, licenceDatabase.IsDeleted);
		}

		public void TestIncludeDatabaseUsage()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllDatabases = false;
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var databaseWrapper = new EdiCommissionAgreementDatabaseWrapper(customization, licenceDatabase, null);
			databaseWrapper.IncludeDatabaseUsage = true;
			AssertContainsExactElementsInAnyOrder(new[] { licenceDatabase.PK }, customization.DatabasePivots.Select(x => x.EZD_LD));
			databaseWrapper.IncludeDatabaseUsage = false;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), customization.DatabasePivots.Select(x => x.EZD_LD));
			AssertEquals(false, licenceDatabase.IsDeleted);
		}

		#region Overrides
		protected override EdiCommissionAgreementDatabaseWrapper GetNewWrapper(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> children)
		{
			return new EdiCommissionAgreementDatabaseWrapper(customization, Factory.New<LicenceDatabase>(), children != null ? children.Cast<EdiCommissionAgreementCountryWrapper>() : null);
		}

		protected override EdiCommissionAgreementTreeBizObjWrapper GetNewChildWrapper(EdiCommissionAgreementCustomization customization)
		{
			return new EdiCommissionAgreementCountryWrapper(customization, null, "AU", null);
		}
		#endregion
	}
}
