using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(EdiLicenceDatabaseOrgSuggestionCollection))]
	public class EdiLicenceDatabaseOrgSuggestionCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiLicenceDatabaseOrgSuggestionCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiLicenceDatabaseOrgSuggestionCollection);
		}

		protected override EdiLicenceDatabaseOrgSuggestionCollection GetCollectionToTest()
		{
			return new EdiLicenceDatabaseOrgSuggestionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var suggestion = Factory.New<EdiLicenceDatabaseOrgSuggestion>();
			suggestion.LDS_LD = Database.PK;
			suggestion.LDS_OH = Org.PK;
			return suggestion;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Database = Factory.NewWithValidTestData<LicenceDatabase>();
			Org = Factory.NewWithValidTestData<OrgHeader>();
		}

		LicenceDatabase Database;
		OrgHeader Org;
	}
}
