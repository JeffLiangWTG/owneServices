using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using BorderWise.Sync;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class OrganisationControllerTest : TestCaseWithFactory
	{
		#region GetOrganisation
		public void TestGetOrganisation_ShouldBeForbidden_WhenWithWrongApiKey()
		{
			var controller = new OrganisationController();
			AssertStatusCode(HttpStatusCode.Forbidden, controller.GetOrgHeader(Guid.NewGuid(), "Mah Key"));
		}

		public void TestGetOrganisation_ShouldBeNotFound_WhenWithWrongPk()
		{
			var controller = new OrganisationController();
			AssertStatusCode(HttpStatusCode.NotFound, controller.GetOrgHeader(Guid.NewGuid(), BorderWiseApiBaseController.ApiKey));
		}

		public void TestGetOrganisation_ShouldBeSuccess()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.MainAddress.OA_Address1 = "Address1";
			org.MainAddress.OA_Address2 = "Address2";
			org.MainAddress.OA_Code = "AddrCode";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.MainAddress.OA_Phone = "12345";
			org.MainAddress.OA_Email = "Test@email.com";
			org.MainAddress.OA_Language = "EN";
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = org.PK;
			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyNumber = 200;
			licenceCompany.LC_CompanyCode = "T01";
			licenceCompany.LC_OH = org.PK;
			licenceCompany.LC_LE = licenceEnterprise.PK;
			var licenceDbBor = Factory.New<LicenceDatabase>();
			licenceDbBor.LD_Product = ProductTypes.Codes.BorderWise;
			licenceDbBor.LD_LicenceType = "PRD";
			licenceDbBor.LD_DatabaseNumber = 201;
			licenceDbBor.LD_ServerCode = "B01";
			licenceDbBor.LD_LE = licenceEnterprise.PK;
			licenceCompany.LicDatabases.Add(licenceDbBor);
			var licenceDbCw1 = Factory.New<LicenceDatabase>();
			licenceDbCw1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			licenceDbCw1.LD_LicenceType = "TRN";
			licenceDbCw1.LD_DatabaseNumber = 202;
			licenceDbCw1.LD_LE = licenceEnterprise.PK;
			licenceDbCw1.LD_ServerCode = "C01";
			licenceCompany.LicDatabases.Add(licenceDbCw1);
			Factory.Save();
			var controller = new OrganisationController();
			var result = controller.GetOrgHeader(org.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			AssertType<OkNegotiatedContentResult<OrgHeaderDataObject>>(result);
			var orgHeaderDataObject = ((OkNegotiatedContentResult<OrgHeaderDataObject>)result).Content;
			AssertEquals(org.OH_Code, orgHeaderDataObject.Code);
			AssertEquals(org.OH_FullName, orgHeaderDataObject.FullName);
			AssertEquals(org.OH_IsActive, orgHeaderDataObject.IsActive);
			AssertEquals(org.OH_Language, orgHeaderDataObject.Language);
			var addressList = orgHeaderDataObject.OrgAddresses.ToList();
			AssertEquals(1, addressList.Count);
			var address = addressList.First();
			AssertEquals(org.MainAddress.OA_Address1, address.Address1);
			AssertEquals(org.MainAddress.OA_Address2, address.Address2);
			AssertEquals(org.MainAddress.OA_Code, address.Code);
			AssertEquals(org.MainAddress.OA_City, address.City);
			AssertEquals(org.MainAddress.OA_State, address.State);
			AssertEquals(org.MainAddress.OA_PostCode, address.PostCode);
			AssertEquals(org.MainAddress.OA_RN_NKCountryCode, address.CountryCode);
			AssertEquals(org.MainAddress.OA_Phone, address.Phone);
			AssertEquals(org.MainAddress.OA_Email, address.Email);
			AssertEquals(org.MainAddress.OA_Language, address.Language);
			var ediProdLicences = orgHeaderDataObject.EdiProdLicences.ToList();

			AssertEquals(2, ediProdLicences.Count);
			var ediProdLicenceCw1 = ediProdLicences.Single(l => l.Product == ProductTypes.Codes.CargoWiseOne);
			AssertEquals(org.LicCompany.LC_CompanyNumber, ediProdLicenceCw1.CompanyNumber);
			AssertEquals("TRN", ediProdLicenceCw1.LicenseType);
			var ediProdLicenceBor = ediProdLicences.Single(l => l.Product == ProductTypes.Codes.BorderWise);
			AssertEquals(org.LicCompany.LC_CompanyNumber, ediProdLicenceBor.CompanyNumber);
			AssertEquals("PRD", ediProdLicenceBor.LicenseType);

			var expectedDatabaseNumberBor = org.LicCompany.LicDatabases.OfType<LicenceDatabase>().Single(ld => ld.LD_Product == ProductTypes.Codes.BorderWise).LD_DatabaseNumber;
			AssertEquals(expectedDatabaseNumberBor, ediProdLicenceBor.DatabaseNumber);
			var expectedDatabaseNumberCw1 = org.LicCompany.LicDatabases.OfType<LicenceDatabase>().Single(ld => ld.LD_Product == ProductTypes.Codes.CargoWiseOne).LD_DatabaseNumber;
			AssertEquals(expectedDatabaseNumberCw1, ediProdLicenceCw1.DatabaseNumber);
		}

		#region Assertions
		static void AssertStatusCode(HttpStatusCode expectedStatusCode, IHttpActionResult actionResult)
		{
			AssertType<StatusCodeResult>(actionResult);
			AssertEquals(expectedStatusCode, ((StatusCodeResult)actionResult).StatusCode);
		}
		#endregion
		#endregion

		#region GetWTGRelatedCompanies
		public void TestGetWtgRelatedCompanies_ShouldBeSuccessAndReturnNonEmptyList()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech global related Org";
			orgHeader.OH_Code = "ABC";

			var orgHeaderNotLinked = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotLinked.OH_FullName = "Not linked Org";
			orgHeaderNotLinked.OH_Code = "XYZ";

			var orgHeaderNotRelated = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotRelated.OH_FullName = "Not related Org";
			orgHeaderNotRelated.OH_Code = "ABCD";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeader.PK;
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "ABC";

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = licenceDatabase.PK;
			clientCompany1.LCC_Code = "XYZ";

			Factory.Save();

			var result = new OrganisationController().GetWtgRelatedCompanies();

			AssertType<OkNegotiatedContentResult<IEnumerable<Guid>>>(result);
			var okResult = (OkNegotiatedContentResult<IEnumerable<Guid>>)result;
			AssertNotNull(okResult.Content);

			var guids = okResult.Content;
			AssertEquals(1, guids.Count());
			AssertEquals(orgHeader.PK.ToGuid(), guids.First());
		}

		public void TestGetWtgRelatedCompanies_ShouldReturnEmptyList_WhenNoCompanyUnderWtgDatabase()
		{
			var orgHeaderNotLinked = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotLinked.OH_FullName = "Not linked Org";
			orgHeaderNotLinked.OH_Code = "ABC";

			var orgHeaderNotRelated = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotRelated.OH_FullName = "Not related Org";
			orgHeaderNotRelated.OH_Code = "ABCD";

			var wtgLicenceEnterprise = Factory.New<LicenceEnterprise>();
			wtgLicenceEnterprise.LE_OH = orgHeaderNotLinked.PK;
			wtgLicenceEnterprise.LE_EnterpriseCode = "DEF";

			var otherLicenceEnterprise = Factory.New<LicenceEnterprise>();
			otherLicenceEnterprise.LE_OH = orgHeaderNotRelated.PK;
			otherLicenceEnterprise.LE_EnterpriseCode = "GHI";

			var wtgLicenceDatabase = Factory.New<LicenceDatabase>();
			wtgLicenceDatabase.LD_DatabaseNumber = 1;
			wtgLicenceDatabase.LD_LE = wtgLicenceEnterprise.PK;
			wtgLicenceDatabase.LD_ServerCode = "XXX";

			var otherLicenceDatabase = Factory.New<LicenceDatabase>();
			otherLicenceDatabase.LD_DatabaseNumber = 123;
			otherLicenceDatabase.LD_LE = otherLicenceEnterprise.PK;
			otherLicenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeaderNotRelated.PK;
			clientCompany.LCC_LD = otherLicenceDatabase.PK;
			clientCompany.LCC_Code = "ABC";

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = otherLicenceDatabase.PK;
			clientCompany1.LCC_Code = "XYZ";

			Factory.Save();

			var result = new OrganisationController().GetWtgRelatedCompanies();

			AssertType<OkNegotiatedContentResult<IEnumerable<Guid>>>(result);
			var okResult = (OkNegotiatedContentResult<IEnumerable<Guid>>)result;
			AssertNotNull(okResult.Content);

			var guids = okResult.Content;
			AssertEquals(0, guids.Count());
		}

		#endregion

		#region CreateOrgHeaderLicence
		public void TestCreateOrgHeaderLicence_ShouldBeForbidden_WhenWithWrongApiKey()
		{
			var controller = new OrganisationController();
			AssertStatusCode(HttpStatusCode.Forbidden, controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = "You shall not pass",
			}));
		}

		public void TestCreateOrgHeaderLicence_ShouldBeBadRequest_WhenWithEmptyPks()
		{
			var controller = new OrganisationController();
			AssertStatusCode(HttpStatusCode.BadRequest, controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
			}));
			AssertStatusCode(HttpStatusCode.BadRequest, controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { Guid.Empty },
			}));
		}

		public void TestCreateOrgHeaderLicence_ShouldBeSuccessAndReturnMessage_WhenFailedToCreateNewLicence()
		{
			var invalidPK = Guid.NewGuid();
			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { invalidPK },
			});

			AssertCreateLicenceResult(result, invalidPK, expectedErrorMessage: $"EDIOrgHeader(s) not found for OH_PKs [{invalidPK}]", expectedLicenceRows: 0);
		}

		public void TestCreateOrgHeaderLicence_ShouldCreateLicCompany_WhenNoLicCompanyExists()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader.PK);
		}

		public void TestCreateOrgHeaderLicence_ShouldCreateLicDatabase_WhenLicDatabasesEmpty()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = orgHeader.PK;
			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyNumber = 200;
			licenceCompany.LC_CompanyCode = "T01";
			licenceCompany.LC_OH = orgHeader.PK;
			licenceCompany.LC_LE = licenceEnterprise.PK;
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader.PK);
		}

		public void TestCreateOrgHeaderLicence_ShouldCreateLicHeader_WhenLicEnterpriseNotContainsBorLicenseDb()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.CreateAndLoadLicenceForOrg();
			var licenceDB = orgHeader.LicEnterprise.Databases.AddNew();
			licenceDB.LD_Product = ProductTypes.Codes.BorderWise;
			licenceDB.LD_ServerCode = ProductTypes.Codes.BorderWise;
			licenceDB.LD_DatabaseNumber = 201;
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader.PK, expectedIsFirstLicenceDatabase: false);
		}

		public void TestCreateOrgHeaderLicence_ShouldCreateBorLicDatabase_WhenBorLicDatabaseNotExists()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = orgHeader.PK;
			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyNumber = 200;
			licenceCompany.LC_CompanyCode = "T01";
			licenceCompany.LC_OH = orgHeader.PK;
			licenceCompany.LC_LE = licenceEnterprise.PK;
			var licenceDB = Factory.New<LicenceDatabase>();
			licenceDB.LD_Product = ProductTypes.Codes.Enterprise; // Not Borderwise
			licenceDB.LD_DatabaseNumber = 201;
			licenceDB.LD_LE = licenceEnterprise.PK;
			licenceCompany.LicDatabases.Add(licenceDB);
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader.PK, 2);
		}

		public void TestCreateOrgHeaderLicence_ShouldAssignProperServerCode_WhenMultipleCW1LicencesForSameEnterpriseCode()
		{
			var orgHeader1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "ENT";
			licenceEnterprise.LE_OH = orgHeader1.PK;

			var licenceCompany1 = licenceEnterprise.Companies.AddNew();
			licenceCompany1.LC_CompanyNumber = 1;
			licenceCompany1.LC_CompanyCode = "C01";
			licenceCompany1.LC_OH = orgHeader1.PK;
			licenceCompany1.LC_LE = licenceEnterprise.PK;

			var licenceDB1 = orgHeader1.LicCompany.LicDatabases.AddNew();
			licenceDB1.LD_ServerCode = "CW1";
			licenceDB1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			licenceDB1.LD_OH_WebAccessOrg = orgHeader1.PK;
			licenceDB1.LD_LE = licenceEnterprise.PK;

			var orgHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			var licenceCompany2 = licenceEnterprise.Companies.AddNew();
			licenceCompany1.LC_CompanyNumber = 2;
			licenceCompany2.LC_CompanyCode = "C02";
			licenceCompany2.LC_OH = orgHeader2.PK;
			licenceCompany2.LC_LE = licenceEnterprise.PK;

			var licenceDB2 = orgHeader2.LicCompany.LicDatabases.AddNew();
			licenceDB2.LD_ServerCode = "CW2";
			licenceDB2.LD_Product = ProductTypes.Codes.CargoWiseNext;
			licenceDB2.LD_OH_WebAccessOrg = orgHeader2.PK;
			licenceDB2.LD_LE = licenceEnterprise.PK;

			var licenceDB3 = orgHeader2.LicCompany.LicDatabases.AddNew();
			licenceDB3.LD_ServerCode = "CW3";
			licenceDB3.LD_Product = ProductTypes.Codes.CargoWise;
			licenceDB3.LD_OH_WebAccessOrg = orgHeader2.PK;
			licenceDB3.LD_LE = licenceEnterprise.PK;

			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader1.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader1.PK, 2, expectedIsFirstLicenceDatabase: true, expectedServerCode: ProductTypes.Codes.BorderWise);

			result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader2.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader2.PK, 3, expectedIsFirstLicenceDatabase: true, expectedServerCode: "BW1");

			result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader2.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader2.PK, 3, expectedIsFirstLicenceDatabase: false, expectedServerCode: "BW1");
		}

		public void TestCreateOrgHeaderLicence_ShouldIgnoreCW1LicenceForSameEnterpriseCode_WhenItIsInactive()
		{
			var orgHeader1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "ENT";
			licenceEnterprise.LE_OH = orgHeader1.PK;

			var licenceCompany1 = licenceEnterprise.Companies.AddNew();
			licenceCompany1.LC_CompanyNumber = 1;
			licenceCompany1.LC_CompanyCode = "C01";
			licenceCompany1.LC_OH = orgHeader1.PK;
			licenceCompany1.LC_LE = licenceEnterprise.PK;

			var licenceDB1 = orgHeader1.LicCompany.LicDatabases.AddNew();
			licenceDB1.LD_ServerCode = "CW1";
			licenceDB1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			licenceDB1.LD_OH_WebAccessOrg = orgHeader1.PK;
			licenceDB1.LD_LE = licenceEnterprise.PK;
			licenceDB1.LD_IsActive = false;

			var orgHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			var licenceCompany2 = licenceEnterprise.Companies.AddNew();
			licenceCompany1.LC_CompanyNumber = 2;
			licenceCompany2.LC_CompanyCode = "C02";
			licenceCompany2.LC_OH = orgHeader2.PK;
			licenceCompany2.LC_LE = licenceEnterprise.PK;

			var licenceDB2 = orgHeader2.LicCompany.LicDatabases.AddNew();
			licenceDB2.LD_ServerCode = "CW2";
			licenceDB2.LD_Product = ProductTypes.Codes.CargoWise;
			licenceDB2.LD_OH_WebAccessOrg = orgHeader2.PK;
			licenceDB2.LD_LE = licenceEnterprise.PK;

			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader1.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader1.PK, 2, expectedIsFirstLicenceDatabase: true, expectedServerCode: ProductTypes.Codes.BorderWise);

			result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader2.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader2.PK, 2, expectedIsFirstLicenceDatabase: false, expectedServerCode: ProductTypes.Codes.BorderWise);
		}

		public void TestCreateOrgHeaderLicence_ShouldSetHasInactiveLicenceDatabase_WhenInactiveBorLicDatabaseExists()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = orgHeader.PK;
			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyNumber = 200;
			licenceCompany.LC_CompanyCode = "T01";
			licenceCompany.LC_OH = orgHeader.PK;
			licenceCompany.LC_LE = licenceEnterprise.PK;
			var licenceDB = Factory.New<LicenceDatabase>();
			licenceDB.LD_Product = ProductTypes.Codes.BorderWise;
			licenceDB.LD_ServerCode = ProductTypes.Codes.BorderWise;
			licenceDB.LD_DatabaseNumber = 201;
			licenceDB.LD_LE = licenceEnterprise.PK;
			licenceDB.LD_IsActive = false;
			licenceCompany.LicDatabases.Add(licenceDB);
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader.PK.ToGuid() },
			});

			AssertCreateLicenceResult(result, orgHeader.PK, 1, expectedIsFirstLicenceDatabase: false, expectedHasInactiveLicenceDatabase: true);
		}

		public void TestCreateOrgHeaderLicence_ShouldCreateMultipleLicences_WhenMultiplePks()
		{
			var orgHeader1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var invalidPk = Guid.NewGuid();
			Factory.Save();

			var controller = new OrganisationController();
			var result = controller.CreateOrgHeaderLicence(new CreateOrgHeaderLicenceRequest
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				OrgHeaderPks = new[] { orgHeader1.PK.ToGuid(), orgHeader2.PK.ToGuid(), orgHeader3.PK.ToGuid(), invalidPk },
			});

			AssertType<OkNegotiatedContentResult<CreateOrgHeaderLicenceResult>>(result);
			var resultObject = ((OkNegotiatedContentResult<CreateOrgHeaderLicenceResult>)result).Content;

			var expectedMessage = $"EDIOrgHeader(s) not found for OH_PKs [{invalidPk}]";
			AssertStringRemovedWhitespaceEquals(expectedMessage, resultObject.ErrorMessage);
			AssertNotNull(resultObject.EdiProdLicences);
			AssertEquals(resultObject.EdiProdLicences.Count(), 3);

			AssertLicenceCreated(resultObject.EdiProdLicences.FirstOrDefault(x => x.OrgHeaderPk == orgHeader1.PK), orgHeader1.PK);
			AssertLicenceCreated(resultObject.EdiProdLicences.FirstOrDefault(x => x.OrgHeaderPk == orgHeader2.PK), orgHeader2.PK);
			AssertLicenceCreated(resultObject.EdiProdLicences.FirstOrDefault(x => x.OrgHeaderPk == orgHeader3.PK), orgHeader3.PK);
		}

		void AssertStringRemovedWhitespaceEquals(string expected, string value)
		{
			expected = Regex.Replace(expected, @"\p{Z}", "");
			value = Regex.Replace(value, @"\p{Z}", "");
			AssertEqualsIgnoreLineBreaks("Expect string to be the same after whitespace has been removed", expected, value);
		}

		void AssertCreateLicenceResult(IHttpActionResult httpActionResult, ZGuid expectedPk, int expectedLicenceRows = 1, string expectedErrorMessage = "", bool expectedIsFirstLicenceDatabase = true, bool expectedHasInactiveLicenceDatabase = false, string expectedServerCode = ProductTypes.Codes.BorderWise)
		{
			AssertType<OkNegotiatedContentResult<CreateOrgHeaderLicenceResult>>(httpActionResult);
			var resultObject = ((OkNegotiatedContentResult<CreateOrgHeaderLicenceResult>)httpActionResult).Content;

			AssertStringRemovedWhitespaceEquals(expectedErrorMessage, resultObject.ErrorMessage);
			if (expectedLicenceRows > 0)
			{
				AssertLicenceCreated(resultObject.EdiProdLicences.FirstOrDefault(), expectedPk, expectedLicenceRows, expectedIsFirstLicenceDatabase, expectedHasInactiveLicenceDatabase, expectedServerCode);
			}
		}

		void AssertLicenceCreated(OrgHeaderLicenceDetail ediProdLicence, ZGuid expectedPk, int expectedLicenceRows = 1, bool expectedIsFirstLicenceDatabase = true, bool expectedHasInactiveLicenceDatabase = false, string expectedServerCode = ProductTypes.Codes.BorderWise)
		{
			AssertNotNull(ediProdLicence);
			AssertEquals(expectedPk.ToGuid(), ediProdLicence.OrgHeaderPk);
			AssertLessThan(0, ediProdLicence.CompanyNumber);
			AssertLessThan(0, ediProdLicence.DatabaseNumber);
			AssertEquals(expectedIsFirstLicenceDatabase, ediProdLicence.IsFirstLicenceDatabase);
			AssertEquals(expectedHasInactiveLicenceDatabase, ediProdLicence.HasInactiveLicenceDatabase);

			var orgHeader = Factory.Load<EDIOrgHeader>(expectedPk);
			var orgLicences = orgHeader.LicCompany.LicDatabases.OfType<LicenceDatabase>().ToArray();
			AssertEquals(expectedLicenceRows, orgLicences.Length);

			var bwLicences = orgLicences.Where(x => x.LD_Product == "BOR").ToList();
			AssertLessThan(0, bwLicences.Count);

			bwLicences.ForEach(licenceDB =>
			{
				AssertEquals(expectedServerCode, licenceDB.LD_ServerCode);
				var licHeader = licenceDB.ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>()
					.FirstOrDefault(x => x.LA_LD == licenceDB.PK);

				AssertNotNull(licHeader);
				AssertEquals(licHeader.LA_LicenceAdvStdOth, "STL");
			});
		}
		#endregion
	}
}
