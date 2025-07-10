using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/LicenceInformation")]
	public class LicenceInformationController : ControllerWithEnvironment
	{
		[Route("GetLicenceInformation")]
		public HttpResponseMessage GetLicenceInformation()
		{
			var accessAllowed = IsValidClientRequest();

			return accessAllowed
				? GetLicenceInformationFromDatabase(new ZQuery())
				: Request.CreateResponse(HttpStatusCode.Forbidden);
		}

		[Route("GetLicenceInformationInactive")]
		public HttpResponseMessage GetLicenceInformationInactive()
		{
			var accessAllowed = IsValidClientRequest();

			return accessAllowed
				? GetLicenceInformationFromDatabase(new ZQuery(LicenceDatabaseSchema.LD_IsActive, 0))
				: Request.CreateResponse(HttpStatusCode.Forbidden);
		}

		[Route("GetLicenceInformationByDatabaseIds")]
		public HttpResponseMessage GetLicenceInformationByDatabaseIds([FromUri]string[] databaseIds)
		{
			var accessAllowed = IsValidClientRequest();
			if (!accessAllowed)
			{
				return Request.CreateResponse(HttpStatusCode.Forbidden);
			}

			if (databaseIds == null)
			{
				return Request.CreateResponse(new { LicenceInformationCollection = Array.Empty<LicenceInformation>() });
			}

			var decodedDatabaseIds = new List<ZInt>();

			foreach (var databaseId in databaseIds)
			{
				if (string.IsNullOrEmpty(databaseId))
				{
					continue;
				}

				if (Base27Encoding.TryDecode(databaseId, out int id))
				{
					decodedDatabaseIds.Add(id);
				}
			}

			return GetLicenceInformationFromDatabase(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, decodedDatabaseIds));
		}

		[Route("GetLicenceInformationByCountryCode")]
		public HttpResponseMessage GetLicenceInformationByCountryCode(string countryCode)
		{
			var accessAllowed = IsValidClientRequest();
			if (!accessAllowed)
			{
				return Request.CreateResponse(HttpStatusCode.Forbidden);
			}

			if (string.IsNullOrEmpty(countryCode))
			{
				return Request.CreateResponse(new { LicenceInformationCollection = Array.Empty<LicenceInformation>() });
			}

			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			var clientCompanySubQuery = new ZDBOnlySubQuery(typeof(ClientCompany), ClientCompanySchema.LCC_LD);
			clientCompanySubQuery.AddToFilter(ClientCompanySchema.LCC_RN_NKCountryCode, countryCode);
			clientCompanySubQuery.AddToFilter(ClientCompanySchema.LCC_DeactivateTimeUtc, null);

			query.AddSubQuery(LicenceDatabaseSchema.PK, clientCompanySubQuery, JoinCondition.And);

			return GetLicenceInformationFromDatabase(query);
		}

		HttpResponseMessage GetLicenceInformationFromDatabase(ZQuery query)
		{
			if (query.IsNoResultQuery)
			{
				return Request.CreateResponse(new { LicenceInformationCollection = Array.Empty<LicenceInformation>() });
			}

			List<LicenceInformation> licences = new List<LicenceInformation>();
			using (Db.DisposableActionForDbConnection())
			{
				var sqlBuilder = new StringBuilder();
				sqlBuilder.AppendLine(FormattableString.Invariant($@"
SELECT
	{LicenceDatabaseSchema.Constants.LD_DatabaseNumber},
	{LicenceDatabaseSchema.Constants.LD_LicenceType},
	{LicenceDatabaseSchema.Constants.LD_IsActive},
	CONCAT({LicenceEnterpriseSchema.Constants.LE_EnterpriseCode}, {ClientCompanySchema.Constants.LCC_Code}, {LicenceDatabaseSchema.Constants.LD_ServerCode})
FROM
{LicenceDatabaseSchema.Constants.TableName}
JOIN {LicenceEnterpriseSchema.Constants.TableName} ON {LicenceDatabaseSchema.Constants.LD_LE} = {LicenceEnterpriseSchema.Constants.PK}
OUTER APPLY
(
	SELECT TOP 1 {ClientCompanySchema.Constants.LCC_Code}, {ClientCompanySchema.Constants.LCC_DeactivateTimeUtc}, {ClientCompanySchema.Constants.LCC_RN_NKCountryCode} FROM {ClientCompanySchema.Constants.TableName} WHERE {ClientCompanySchema.Constants.LCC_LD} = {LicenceDatabaseSchema.Constants.PK} AND {ClientCompanySchema.Constants.LCC_DeactivateTimeUtc} IS NULL ORDER BY {ClientCompanySchema.Constants.LCC_CreateTimeUtc}
) AS ClientCompany"));
				if (!query.IsEmpty && !string.IsNullOrEmpty(query.LiteralTextSqlFormatted))
				{
					sqlBuilder.AppendLine(FormattableString.Invariant($"WHERE {query.LiteralTextSqlFormatted}"));
				}

				var command = Db.Connection.Command(sqlBuilder.ToString());
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						licences.Add(new LicenceInformation
						{
							DatabaseId = Base27Encoding.Encode(reader.GetInt32(0)),
							LicenceType = reader.GetString(1),
							IsActive = reader.GetBoolean(2),
							LicenceCode = reader.GetString(3)
						});
					}
				}
			}

			return Request.CreateResponse(new
			{
				LicenceInformationCollection = licences
			});
		}

		#region Data Structure

		public class LicenceInformation
		{
			public string DatabaseId { get; set; }
			public string LicenceCode { get; set; }
			public string LicenceType { get; set; }
			public bool IsActive { get; set; }
		}

		#endregion

		#region Helpers

		static bool IsValidClientRequest()
		{
			var request = HttpContext.Current.Request;
			if (request.UserHostAddress == "127.0.0.1" || request.UserHostAddress == "::1")
			{
				return true;
			}

			return RequestAuthorisationHelper.IsRequestPermitted(request);
		}

		#endregion
	}
}
