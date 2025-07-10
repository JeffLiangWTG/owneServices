using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgCompanyData))]
	public class EDIOrgCompanyDataTest : EnterpriseBusinessObjectTestCase
	{
		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return HeaderForTest.CompanyData;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return HeaderForTest.CompanyData;
		}

		#endregion

		#region WHT Applicable

		public void TestOB_ARWHTApplicable()
		{
			HeaderForTest.CreateAndLoadLicenceForOrg();
			Assert("WHT Not Registered", !HeaderForTest.LicCompany.LC_IsWHTRegistered);

			HeaderForTest.CompanyData.OB_ARWHTApplicable = ZBool.True;
			Assert("WHT Registered", HeaderForTest.LicCompany.LC_IsWHTRegistered);
		}

		#endregion

		#region Set Up

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (fHeaderForTest == null)
				{
					fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
					fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
					fHeaderForTest.OH_FullName = "My Organisation";
					fHeaderForTest.OH_Code = "TGBLOG";
					fHeaderForTest.CreateAndLoadLicenceForOrg();
					fHeaderForTest.GenerateNewLicenceCode();

					OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
					newAddress.OA_Address1 = "666 Test Address";
				}

				return fHeaderForTest;
			}
		}
		EDIOrgHeader fHeaderForTest;

		#endregion
	}
}
