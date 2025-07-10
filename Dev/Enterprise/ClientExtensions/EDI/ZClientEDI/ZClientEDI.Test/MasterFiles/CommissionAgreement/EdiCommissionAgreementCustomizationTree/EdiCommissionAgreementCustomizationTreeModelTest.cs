using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCustomizationTreeModel))]
	public class EdiCommissionAgreementCustomizationTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRebuild()
		{
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			var licenceDatabase1 = licenceEnterprise.Databases.AddNew();
			var licenceDatabase2 = licenceEnterprise.Databases.AddNew();
			var clientCompany1AUa = Factory.New<ClientCompany>();
			clientCompany1AUa.LCC_LD = licenceDatabase1.PK;
			clientCompany1AUa.LCC_RN_NKCountryCode = "AU";
			var clientCompany1AUb = Factory.New<ClientCompany>();
			clientCompany1AUb.LCC_LD = licenceDatabase1.PK;
			clientCompany1AUb.LCC_RN_NKCountryCode = "AU";
			var clientCompany1NZ = Factory.New<ClientCompany>();
			clientCompany1NZ.LCC_LD = licenceDatabase1.PK;
			clientCompany1NZ.LCC_RN_NKCountryCode = "NZ";
			var clientCompany2AU = Factory.New<ClientCompany>();
			clientCompany2AU.LCC_LD = licenceDatabase2.PK;
			clientCompany2AU.LCC_RN_NKCountryCode = "AU";
			var clientCompany2US = Factory.New<ClientCompany>();
			clientCompany2US.LCC_LD = licenceDatabase2.PK;
			clientCompany2US.LCC_RN_NKCountryCode = "US";
			var company = licenceEnterprise.Companies.AddNew();
			company.LC_LE = licenceEnterprise.PK;
			company.LC_OH = Factory.New<OrgHeader>().PK;
			var org = company.Header;
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllCompanies = false;
			customization.CompanyPivots.AddNew(clientCompany1AUa);
			var newCompaniesForDb1 = customization.CompanyAutoAddDatabases.AddNew();
			newCompaniesForDb1.EPD_LD = licenceDatabase1.PK;
			var newUsCompaniesForDb2 = customization.CompanyAutoAddCountries.AddNew();
			newUsCompaniesForDb2.EPC_LD = licenceDatabase2.PK;
			newUsCompaniesForDb2.EPC_RN_NKCountry = "US";
			var newGbCompaniesForDb2 = customization.CompanyAutoAddCountries.AddNew();
			newGbCompaniesForDb2.EPC_LD = licenceDatabase2.PK;
			newGbCompaniesForDb2.EPC_RN_NKCountry = "GB";
			var newAuCompaniesForNewDb = customization.CompanyAutoAddCountries.AddNew();
			newAuCompaniesForNewDb.EPC_RN_NKCountry = "AU";
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			model.Rebuild();
			var dbWrappers = model.RootNodes.Select(x => x.BizObjForBinding).Cast<EdiCommissionAgreementDatabaseWrapper>();
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(licenceDatabase1, ZBool.True), Tuple.Create(licenceDatabase2, ZBool.False), Tuple.Create((LicenceDatabase)null, ZBool.False), }, dbWrappers.Select(x => Tuple.Create(x.LicenceDatabase, x.ShouldAutoAdd)));
			{
				var db1Wrapper = dbWrappers.Single(x => x.LicenceDatabase == licenceDatabase1);
				var db1CountryWrappers = db1Wrapper.Children.Cast<EdiCommissionAgreementCountryWrapper>();
				AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create("AU", ZBool.False), Tuple.Create("NZ", ZBool.False), }, db1CountryWrappers.Select(x => Tuple.Create(x.CountryCode.ToString(), x.ShouldAutoAdd)));
				{
					var db1AuWrapper = db1CountryWrappers.Single(x => x.CountryCode == "AU");
					var db1AuCompanyWrappers = db1AuWrapper.Children.Cast<EdiCommissionAgreementCompanyWrapper>();
					AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(ZBool.True, clientCompany1AUa), Tuple.Create(ZBool.False, clientCompany1AUb), }, db1AuCompanyWrappers.Select(x => Tuple.Create(x.Selected, x.ClientCompany)));
				}

				{
					var db1NzWrapper = db1CountryWrappers.Single(x => x.CountryCode == "NZ");
					var db1NzCompanyWrappers = db1NzWrapper.Children.Cast<EdiCommissionAgreementCompanyWrapper>();
					AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(ZBool.False, clientCompany1NZ), }, db1NzCompanyWrappers.Select(x => Tuple.Create(x.Selected, x.ClientCompany)));
				}
			}

			{
				var db2Wrapper = dbWrappers.Single(x => x.LicenceDatabase == licenceDatabase2);
				var db2CountryWrappers = db2Wrapper.Children.Cast<EdiCommissionAgreementCountryWrapper>();
				AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create("AU", ZBool.False), Tuple.Create("US", ZBool.True), Tuple.Create("GB", ZBool.True), }, db2CountryWrappers.Select(x => Tuple.Create(x.CountryCode.ToString(), x.ShouldAutoAdd)));
				{
					var db2AuWrapper = db2CountryWrappers.Single(x => x.CountryCode == "AU");
					var db2AuCompanyWrappers = db2AuWrapper.Children.Cast<EdiCommissionAgreementCompanyWrapper>();
					AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(ZBool.False, clientCompany2AU), }, db2AuCompanyWrappers.Select(x => Tuple.Create(x.Selected, x.ClientCompany)));
				}

				{
					var db2UsWrapper = db2CountryWrappers.Single(x => x.CountryCode == "US");
					var db2UsCompanyWrappers = db2UsWrapper.Children.Cast<EdiCommissionAgreementCompanyWrapper>();
					AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(ZBool.False, clientCompany2US), }, db2UsCompanyWrappers.Select(x => Tuple.Create(x.Selected, x.ClientCompany)));
				}

				{
					var db2GbWrapper = db2CountryWrappers.Single(x => x.CountryCode == "GB");
					AssertNull(db2GbWrapper.Children);
				}
			}

			{
				var newDbWrapper = dbWrappers.Single(x => x.LicenceDatabase == null);
				var newDbCountryWrappers = newDbWrapper.Children.Cast<EdiCommissionAgreementCountryWrapper>();
				AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create("AU", ZBool.True), }, newDbCountryWrappers.Select(x => Tuple.Create(x.CountryCode.ToString(), x.ShouldAutoAdd)));
				{
					var newDbAuWrapper = newDbCountryWrappers.Single(x => x.CountryCode == "AU");
					AssertNull(newDbAuWrapper.Children);
				}
			}
		}

		public void TestRebuild_WithoutEnterpriseLicence()
		{
			var org = Factory.New<OrgHeader>();
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllCompanies = false;
			var newUsCompaniesForNewDb = customization.CompanyAutoAddCountries.AddNew();
			newUsCompaniesForNewDb.EPC_LD = ZGuid.Empty;
			newUsCompaniesForNewDb.EPC_RN_NKCountry = "US";
			var newGbCompaniesForNewDb = customization.CompanyAutoAddCountries.AddNew();
			newGbCompaniesForNewDb.EPC_LD = ZGuid.Empty;
			newGbCompaniesForNewDb.EPC_RN_NKCountry = "GB";
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			model.Rebuild();
			var dbWrappers = model.RootNodes.Select(x => x.BizObjForBinding).Cast<EdiCommissionAgreementDatabaseWrapper>();
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create((LicenceDatabase)null, ZBool.False), }, dbWrappers.Select(x => Tuple.Create(x.LicenceDatabase, x.ShouldAutoAdd)));
			{
				var newDbWrapper = dbWrappers.Single(x => x.LicenceDatabase == null);
				var newDbCountryWrappers = newDbWrapper.Children.Cast<EdiCommissionAgreementCountryWrapper>();
				AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create("US", ZBool.True), Tuple.Create("GB", ZBool.True), }, newDbCountryWrappers.Select(x => Tuple.Create(x.CountryCode.ToString(), x.ShouldAutoAdd)));
			}
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			var agreement = Factory.New<EdiCommissionAgreement>();
			var customization = agreement.GetOrCreateCustomization();
			return new EdiCommissionAgreementCustomizationTreeModel(customization);
		}
		#endregion
	}
}
