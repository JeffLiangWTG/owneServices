using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	[RoutePrefix("api/BorderWiseOrganisations")]
	public class OrganisationController : BorderWiseApiBaseController
	{
		const string BorderWiseSecurityItemName = "BorderWise";

		[HttpPost]
		[Route("{orgPk}")]
		public IHttpActionResult GetOrgHeader(Guid orgPk, [FromBody] string apiKey)
		{
			if (!IsValidApiKey(apiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			return orgPk == Guid.Empty ? StatusCode(HttpStatusCode.BadRequest) : GetOrgHeaderDetailsCore(orgPk);
		}

		[HttpPost]
		[Route("ediProdLicences")]
		public IHttpActionResult CreateOrgHeaderLicence([FromBody] CreateOrgHeaderLicenceRequest request)
		{
			if (!IsValidApiKey(request.ApiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (request.OrgHeaderPks.IsNullOrEmpty() || request.OrgHeaderPks.Any(orgHeaderPk => orgHeaderPk == Guid.Empty))
			{
				return StatusCode(HttpStatusCode.BadRequest);
			}

			return CreateOrgHeaderLicenceCore(request.OrgHeaderPks);
		}

		[HttpGet]
		[Route("wtg-related-companies")]
		public IHttpActionResult GetWtgRelatedCompanies()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var sessionId = Guid.NewGuid().ToString();
				var routingPath = $"wtg-related-companies";

				var orgPksUnderWiseTechGlobal = GetOrganizationPksUnderWiseTechGlobalCompany(sessionId, routingPath);

				return Ok(orgPksUnderWiseTechGlobal ?? Enumerable.Empty<Guid>());
			}
		}

		IHttpActionResult GetOrgHeaderDetailsCore(Guid orgPk)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var orgHeader = DataFactory.Load<EDIOrgHeader>(orgPk);

				if (orgHeader == null)
				{
					return StatusCode(HttpStatusCode.NotFound);
				}

				var orgHeaderDataObject = CreateDataObject(orgHeader);

				// License DB
				var orgLicences = ObjectFactory.Get<IOrgLicencesHelper>().GetOrgLicences(orgPk);
				orgHeaderDataObject.EdiProdLicences = orgLicences.Select(l => new EdiProdLicenceDataObject { CompanyNumber = l.CompanyNumber, DatabaseNumber = l.DatabaseNumber, Product = l.Product, LicenseType = l.LicenseType }).ToList();

				// Org security
				var orgRightQuery = new ZQuery(OrgSecuritySchema.OX_OH, orgPk);
				orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, BorderWiseSecurityItemName);
				var orgRight = DataFactory.LoadTop1<OrgSecurity>(orgRightQuery);

				orgHeaderDataObject.SecurityRightGranted = orgRight?.OX_Granted ?? true;

				// org address
				if (orgHeader.MainAddress != null)
				{
					orgHeaderDataObject.OrgAddresses = new List<OrgAddressDataObject>()
					{
						GetAddressDataObject(orgHeader.MainAddress, orgPk)
					};
				}

				return Ok(orgHeaderDataObject);
			}
		}

		static OrgHeaderDataObject CreateDataObject(EDIOrgHeader orgHeader)
		{
			return new OrgHeaderDataObject
			{
				PK = orgHeader.PK.ToGuid(),
				Code = orgHeader.OH_Code,
				FullName = orgHeader.OH_FullName,
				IsActive = orgHeader.OH_IsActive,
				Language = orgHeader.OH_Language,
			};
		}

		static OrgAddressDataObject GetAddressDataObject(OrgAddress orgAddress, Guid orgPk)
		{
			return new OrgAddressDataObject
			{
				PK = orgAddress.PK.ToGuid(),
				Code = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				Address2 = orgAddress.OA_Address2,
				City = orgAddress.OA_City,
				State = orgAddress.OA_State,
				PostCode = orgAddress.OA_PostCode,
				CountryCode = orgAddress.OA_RN_NKCountryCode,
				Phone = orgAddress.OA_Phone,
				Email = orgAddress.OA_Email,
				Language = orgAddress.OA_Language,
				IsActive = orgAddress.OA_IsActive,
				OrgPk = orgPk
			};
		}

		LicenceDatabase CreateNewLicenceDatabase(EDIOrgHeader orgHeader, string serverCode)
		{
			var licenceDB = orgHeader.LicCompany.LicDatabases.AddNew();
			licenceDB.LD_Product = ProductTypes.Codes.BorderWise;
			licenceDB.LD_ServerCode = serverCode;
			licenceDB.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			licenceDB.LD_OH_WebAccessOrg = orgHeader.PK;
			licenceDB.LD_IsActive = true;
			licenceDB.LD_Status = DatabaseStatusList.Codes.REG;
			licenceDB.LD_LicenceType = DatabaseTypes.Codes.Production;

			DataFactory.Save();
			licenceDB.LD_TenantID = licenceDB.LD_DatabaseNumber.ToString();

			var licenceHeader = licenceDB.ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>()
									.FirstOrDefault(x => x.LA_LD == licenceDB.PK);

			licenceHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			return licenceDB;
		}

		void CreateNewLicenceHeader(EDIOrgHeader orgHeader, LicenceDatabase licenceDB)
		{
			var licenceHeader = licenceDB.ActiveLicHeadersForAllCompanies.AddNew();
			licenceHeader.LA_LC = orgHeader.LicCompany.PK;
			licenceHeader.LA_IsActive = true;
			licenceHeader.LA_LD = licenceDB.PK;
			licenceHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
		}

		IHttpActionResult CreateOrgHeaderLicenceCore(IEnumerable<Guid> orgHeaderPks)
		{
			var licenceResult = new List<OrgHeaderLicenceDetail>();
			var errorBuilder = new StringBuilder();

			using (Db.DisposableActionForDbConnection())
			{
				var query = new ZQuery(OrgHeaderSchema.PK, orgHeaderPks);
				var orgHeaders = DataFactory.Load<EDIOrgHeader>(query);

				var notFound = orgHeaderPks
					.Where(orgHeaderPk => !orgHeaders.Any(x => x.PK.ToGuid() == orgHeaderPk))
					.ToList();

				if (notFound.Count > 0)
				{
					errorBuilder.AppendLine($"EDIOrgHeader(s) not found for OH_PKs [{string.Join("; ", notFound)}]");
				}

				foreach (var orgHeader in orgHeaders)
				{
					try
					{
						if (orgHeader.LicCompany == null)
						{
							orgHeader.CreateAndLoadLicenceForOrg();
						}

						var licenceDB = orgHeader.LicCompany.ActiveOrAllLicDatabases
										.OfType<LicenceDatabase>()
										.FirstOrDefault(x => x.LD_Product == ProductTypes.Codes.BorderWise);

						var isFirstLicenceDatabase = false;

						if (licenceDB == null)
						{
							var bwDatabases = orgHeader.LicEnterprise.Databases.OfType<LicenceDatabase>()
											.Where(x => x.LD_Product == ProductTypes.Codes.BorderWise && x.LD_IsActive && x.LD_LicenceType == DatabaseTypes.Codes.Production).ToList();

							var cwDatabases = orgHeader.LicEnterprise.Databases.OfType<LicenceDatabase>()
											.Where(x => (x.LD_Product == ProductTypes.Codes.CargoWiseNext
													|| x.LD_Product == ProductTypes.Codes.CargoWiseOne
													|| x.LD_Product == ProductTypes.Codes.CargoWise)
													&& x.LD_IsActive && x.LD_LicenceType == DatabaseTypes.Codes.Production).ToList();

							var cwDbCount = cwDatabases.Count;

							licenceDB = orgHeader.LicEnterprise.Databases.OfType<LicenceDatabase>()
								.FirstOrDefault(x => x.LD_Product == ProductTypes.Codes.BorderWise);

							if (licenceDB == null)
							{
								isFirstLicenceDatabase = true; // first licence database for this enterprise
								licenceDB = CreateNewLicenceDatabase(orgHeader, ProductTypes.Codes.BorderWise);
							}
							else if (cwDbCount > 1)
							{
								isFirstLicenceDatabase = true; // first licence database for this org
								licenceDB = CreateNewLicenceDatabase(orgHeader, bwDatabases.Count > 0 ? $"BW{bwDatabases.Count}" : ProductTypes.Codes.BorderWise);
							}
							else if (licenceDB.LD_IsActive)
							{
								CreateNewLicenceHeader(orgHeader, licenceDB);
							}

							DataFactory.Save();
						}

						licenceResult.Add(new OrgHeaderLicenceDetail
						{
							OrgHeaderPk = orgHeader.PK.ToGuid(),
							CompanyNumber = orgHeader.LicCompany.LC_CompanyNumber,
							DatabaseNumber = licenceDB.LD_DatabaseNumber,
							IsFirstLicenceDatabase = isFirstLicenceDatabase,
							HasInactiveLicenceDatabase = !licenceDB.LD_IsActive,
						});
					}
					catch (Exception ex)
					{
						errorBuilder.AppendLine($"Error while creating licence for OH_PK {orgHeader.PK} - {ex.Message}");
					}
				}
			}

			return Ok(new CreateOrgHeaderLicenceResult
			{
				EdiProdLicences = licenceResult,
				ErrorMessage = errorBuilder.ToString(),
			});
		}
	}
}
