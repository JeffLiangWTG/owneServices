using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.GUI;
using Enterprise.MasterData.GUI.Tests;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	class DeDuplicationTest : TestCaseWithFactory
	{
		public void TestDataSourceEmptyValues()
		{
			var model = TestLicenceValuesSetUp(false);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, model.EnterpriseId);
				AssertEquals(ZString.Empty, model.EnterpriseCode);
				AssertEquals(ZString.Empty, model.CompanyCode);
				AssertEquals(ZString.Empty, model.ProductId);
			});
		}

		public void TestDataSourceLicenceValues()
		{
			var model = TestLicenceValuesSetUp(true);

			CombineAssertions(() =>
			{
				AssertEquals("AA3", model.EnterpriseId);
				AssertEquals("AA1", model.EnterpriseCode);
				AssertEquals("AA2", model.CompanyCode);
				Assert("AA1, AA2" == model.ProductId || "AA2, AA1" == model.ProductId);
			});
		}

		public void TestOrganisationEmptyValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			var dedupOrg = Factory.Load<EDIDeduplicationOrganisation>(org.PK);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, dedupOrg.DOH_EnterpriseId);
				AssertEquals(ZString.Empty, dedupOrg.DOH_EnterpriseCode);
				AssertEquals(ZString.Empty, dedupOrg.DOH_CompanyCode);
				AssertEquals(ZString.Empty, dedupOrg.DOH_ProductId);
			});
		}

		public void TestOrganisationLicenceValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			SupplyLicenceValues(org.PK);

			Factory.Save();
			var dedupOrg = Factory.Load<EDIDeduplicationOrganisation>(org.PK);

			CombineAssertions(() =>
			{
				AssertEquals("AA3", dedupOrg.DOH_EnterpriseId);
				AssertEquals("AA1", dedupOrg.DOH_EnterpriseCode);
				AssertEquals("AA2", dedupOrg.DOH_CompanyCode);
				Assert("AA1, AA2" == dedupOrg.DOH_ProductId || "AA2, AA1" == dedupOrg.DOH_ProductId);
			});
		}

		#region Implementation

		PotentialDuplicationModel TestLicenceValuesSetUp(bool supplyValues)
		{
			var testEDIorgHeader = Factory.New<OrgHeader>();
			testEDIorgHeader.OH_Code = "ABC";
			var dedupOrgHeader = new DeduplicationOrgHeader(testEDIorgHeader);

			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(dedupOrgHeader,
				new List<DeduplicationOrgHeader>() { dedupOrgHeader },
				new List<DeduplicationPresenterModel>()
			);

			var populatedTarget = new List<DeduplicationPresenterModel> { new DeduplicationPresenterModel() }.GroupBy(x => x.TargetID).FirstOrDefault();

			if (supplyValues)
			{
				SupplyLicenceValues(dedupOrgHeader.PK);
			}

			Factory.Save();
			return duplicationDataSource.GetPotentialDuplicationModelForTest(dedupOrgHeader, populatedTarget);
		}

		void SupplyLicenceValues(ZGuid pk)
		{
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "AA1";
			enterprise.LE_EnterpriseID = "AA3";
			enterprise.LE_OH = pk;

			var company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = "AA2";
			company.LC_OH = pk;
			company.LC_LE = enterprise.PK;

			int serverCodeNum = 10;
			foreach (var prod in new string[] { "AA1", "AA2", "AA1" })
			{
				LicenceDatabase database = company.LicDatabases.AddNew();
				database.LD_Product = prod;
				database.LD_ServerCode = ZString.Format("T{0}", serverCodeNum++);
			}
		}

		#endregion
	}
}
