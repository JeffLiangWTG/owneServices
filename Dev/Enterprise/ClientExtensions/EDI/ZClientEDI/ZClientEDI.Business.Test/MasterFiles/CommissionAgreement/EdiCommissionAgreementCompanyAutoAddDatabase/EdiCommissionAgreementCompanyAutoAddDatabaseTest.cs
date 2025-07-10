using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCompanyAutoAddDatabase))]
	class EdiCommissionAgreementCompanyAutoAddDatabaseTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestPivotNotSaveForUncommittedDraftAgreement()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			database.LD_ServerCode = "SYD";
			Factory.Save();

			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.CompanyAutoAddDatabases.AddNew(database);
			Factory.Save();

			var draft = (EdiCommissionAgreement)agreement.CreateDraft(true);
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			var database = enterprise.Databases.AddNew();
			database.LD_LE = enterprise.PK;

			var commissionAgreement = factory.NewWithValidTestData<EdiCommissionAgreement>();
			commissionAgreement.CA0_OH_Customer = org.PK;
			var customization = commissionAgreement.GetOrCreateCustomization();
			return customization.CompanyAutoAddDatabases.AddNew(database);
		}
	}
}
