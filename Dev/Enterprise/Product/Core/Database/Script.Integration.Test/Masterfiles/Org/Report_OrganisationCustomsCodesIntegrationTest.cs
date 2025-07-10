using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org
{
	class Report_OrganisationCustomsCodesIntegrationTest : TransactionedTestCase
	{
		public void TestFilterWithCompanyDataAndWithoutCompanyData()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG1";
			var compData = org.CompanyData; // Create the company data
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			factory.Save();

			var query = "SELECT * FROM Report_OrganisationCustomsCodes(@CompanyPK) WHERE OrganisationPK = @OrganisationPK";
			AssertFilterCorrectWithDifferentCompany(TestDbHelper.DefaultCompanyPK);
			AssertFilterCorrectWithDifferentCompany(Guid.NewGuid());

			void AssertFilterCorrectWithDifferentCompany(Guid companyPK)
			{
				using (var command = TestConnection.Command(query))
				{
					command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
					command.AddParameter("@OrganisationPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
					using (var reader = command.ExecuteReader())
					{
						var totalRow = 0;

						while (reader.Read())
						{
							totalRow++;
							AssertEquals("TESTORG1", reader.GetString(2));
						}

						AssertEquals(1, totalRow);
					}
				}
			}
		}
	}
}

