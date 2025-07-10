using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCountryWrapper))]
	class EdiCommissionAgreementCompanyCountryWrapperTest : EdiCommissionAgreementTreeBizObjWrapperTestCase<EdiCommissionAgreementCountryWrapper>
	{
		public void TestCode()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var countryWrapper = new EdiCommissionAgreementCountryWrapper(customization, null, "AU", null);
			AssertEquals("Australia (AU)", countryWrapper.Code);
			var unknownCountryWrapper = new EdiCommissionAgreementCountryWrapper(customization, null, "", null);
			AssertEquals("Unknown", unknownCountryWrapper.Code);
		}

		public void TestDescription()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var wrapper = new EdiCommissionAgreementCountryWrapper(customization, null, "AU", null);
			AssertEquals("", wrapper.Description);
		}

		public void TestShouldAutoAdd()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllCompanies = false;
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var countryWrapper = new EdiCommissionAgreementCountryWrapper(customization, licenceDatabase, "AU", null);
			countryWrapper.ShouldAutoAdd = true;
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(licenceDatabase.PK, (ZString)"AU") }, customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry)));
			countryWrapper.ShouldAutoAdd = false;
			AssertContainsExactElementsInAnyOrder(Array.Empty<Tuple<ZGuid, ZString>>(), customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry)));
			AssertEquals(false, licenceDatabase.IsDeleted);
		}

		public void TestShouldAutoAdd_ForNewDatabase()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.EZN_IsAllCompanies = false;
			var countryWrapper = new EdiCommissionAgreementCountryWrapper(customization, null, "AU", null);
			countryWrapper.ShouldAutoAdd = true;
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(ZGuid.Empty, (ZString)"AU") }, customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry)));
			countryWrapper.ShouldAutoAdd = false;
			AssertContainsExactElementsInAnyOrder(Array.Empty<Tuple<ZGuid, ZString>>(), customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry)));
		}

		#region Overrides
		protected override EdiCommissionAgreementCountryWrapper GetNewWrapper(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> children)
		{
			return new EdiCommissionAgreementCountryWrapper(customization, Factory.New<LicenceDatabase>(), "AU", children != null ? children.Cast<EdiCommissionAgreementCompanyWrapper>() : null);
		}

		protected override EdiCommissionAgreementTreeBizObjWrapper GetNewChildWrapper(EdiCommissionAgreementCustomization customization)
		{
			return new EdiCommissionAgreementCompanyWrapper(customization, Factory.New<ClientCompany>());
		}
		#endregion
	}
}
