using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	public class BorderWiseOrgLicencesHelperTest : TestCaseWithFactory
	{
		public void TestGetOrgLicences_ShouldReturnCorrectLicenses()
		{
			var factory = new BusinessObjectFactory();
			var organisation1 = factory.NewWithValidTestData<EDIOrgHeader>();

			organisation1.CreateAndLoadLicenceForOrg();
			organisation1.LicenceEnterpriseCode = "LE1";
			organisation1.LicCompany.LC_CompanyCode = "LC1";

			var databaseActiveBorProd = organisation1.LicCompany.LicDatabases.AddNew();
			var databaseActiveCW1Prod = organisation1.LicCompany.LicDatabases.AddNew();
			var databaseInactiveCW1Prod = organisation1.LicCompany.LicDatabases.AddNew();
			var databaseActiveOtherProd = organisation1.LicCompany.LicDatabases.AddNew();
			var databaseActiveCWNProd = organisation1.LicCompany.LicDatabases.AddNew();
			var databaseActiveCGWProd = organisation1.LicCompany.LicDatabases.AddNew();

			databaseActiveBorProd.LD_ServerCode = "LD1";
			databaseActiveBorProd.LD_Product = ProductTypes.Codes.BorderWise;
			databaseActiveBorProd.LD_LicenceType = "PRD";
			databaseActiveBorProd.LD_IsActive = true;

			databaseActiveCW1Prod.LD_ServerCode = "LD2";
			databaseActiveCW1Prod.LD_Product = ProductTypes.Codes.CargoWiseOne;
			databaseActiveCW1Prod.LD_LicenceType = "PRD";
			databaseActiveCW1Prod.LD_IsActive = true;

			databaseInactiveCW1Prod.LD_ServerCode = "LD3";
			databaseInactiveCW1Prod.LD_Product = ProductTypes.Codes.CargoWiseOne;
			databaseInactiveCW1Prod.LD_LicenceType = "PRD";
			databaseInactiveCW1Prod.LD_IsActive = false;

			databaseActiveOtherProd.LD_ServerCode = "LD4";
			databaseActiveOtherProd.LD_Product = ProductTypes.Codes.WiseTechAcademy;
			databaseActiveOtherProd.LD_LicenceType = "PRD";
			databaseActiveOtherProd.LD_IsActive = true;

			databaseActiveCWNProd.LD_ServerCode = "LD5";
			databaseActiveCWNProd.LD_Product = ProductTypes.Codes.CargoWiseNext;
			databaseActiveCWNProd.LD_LicenceType = "PRD";
			databaseActiveCWNProd.LD_IsActive = true;

			databaseActiveCGWProd.LD_ServerCode = "LD6";
			databaseActiveCGWProd.LD_Product = ProductTypes.Codes.CargoWise;
			databaseActiveCGWProd.LD_LicenceType = "PRD";
			databaseActiveCGWProd.LD_IsActive = true;

			factory.Save();

			var helper = new BorderWiseOrgLicencesHelper();
			var licences = helper.GetOrgLicences(organisation1);

			Assertion.AssertEquals(4, licences.Count());

			var licence1 = licences.FirstOrDefault(l => l.LicenseType == "PRD" && l.Product == ProductTypes.Codes.BorderWise);
			var licence2 = licences.FirstOrDefault(l => l.LicenseType == "PRD" && l.Product == ProductTypes.Codes.CargoWiseOne);
			var licence3 = licences.FirstOrDefault(l => l.LicenseType == "PRD" && l.Product == ProductTypes.Codes.CargoWiseNext);
			var licence4 = licences.FirstOrDefault(l => l.LicenseType == "PRD" && l.Product == ProductTypes.Codes.CargoWise);

			Assertion.AssertNotNull(licence1);
			Assertion.AssertNotNull(licence2);
			Assertion.AssertNotNull(licence3);
			Assertion.AssertNotNull(licence4);
		}
	}
}
