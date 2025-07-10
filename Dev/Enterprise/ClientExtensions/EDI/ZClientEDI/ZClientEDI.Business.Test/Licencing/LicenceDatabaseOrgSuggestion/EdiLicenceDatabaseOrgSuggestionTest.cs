using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(EdiLicenceDatabaseOrgSuggestion))]
	public class EdiLicenceDatabaseOrgSuggestionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var suggestion = Factory.New<EdiLicenceDatabaseOrgSuggestion>();
			suggestion.LDS_LD = Database.PK;
			suggestion.LDS_OH = Org.PK;
			return suggestion;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var suggestion = factory.New<EdiLicenceDatabaseOrgSuggestion>();
			suggestion.LDS_LD = Database.PK;
			suggestion.LDS_OH = Org.PK;
			return suggestion;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Database = Factory.NewWithValidTestData<LicenceDatabase>();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
		}

		LicenceDatabase Database;
		OrgHeader Org;
	}
}
