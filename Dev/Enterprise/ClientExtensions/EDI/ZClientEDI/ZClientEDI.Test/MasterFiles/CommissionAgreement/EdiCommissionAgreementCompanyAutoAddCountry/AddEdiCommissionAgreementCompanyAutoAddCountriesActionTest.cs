using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(AddEdiCommissionAgreementCompanyAutoAddCountriesAction))]
	class AddEdiCommissionAgreementCompanyAutoAddCountriesActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExecute()
		{
			var database1 = Factory.New<LicenceDatabase>();
			var database2 = Factory.New<LicenceDatabase>();
			var database3 = Factory.New<LicenceDatabase>();
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			customization.CompanyAutoAddCountries.AddNew(database1.PK, "AU");
			customization.CompanyAutoAddCountries.AddNew(database2.PK, "US");
			customization.CompanyAutoAddCountries.AddNew(ZGuid.Empty, "AU");
			var action = new AddEdiCommissionAgreementCompanyAutoAddCountriesAction(customization, ZGuid.Empty, database1.PK, database2.PK);
			action.Items.AddNew("AU");
			action.Items.AddNew("GB");
			action.Execute();
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(database1.PK, "AU"), Tuple.Create(database1.PK, "GB"), Tuple.Create(database2.PK, "AU"), Tuple.Create(database2.PK, "GB"), Tuple.Create(database2.PK, "US"), Tuple.Create(ZGuid.Empty, "AU"), Tuple.Create(ZGuid.Empty, "GB"), }, customization.CompanyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry.ToString())));
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			return new AddEdiCommissionAgreementCompanyAutoAddCountriesAction(customization, ZGuid.Empty);
		}
		#endregion
	}
}
