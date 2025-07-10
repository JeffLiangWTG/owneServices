using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/ProductKey")]
	public class ProductKeyController : TrustedController
	{
		[HttpPost]
		[Route("InternalTestKey")]
		public HttpResponseMessage GetInternalTestKey([FromBody] InternalTestKeyRequest request)
		{
			if (!RequestAuthorisationHelper.IsRequestPermitted(HttpContext.Current.Request))
			{
				return Request.CreateResponse(HttpStatusCode.Forbidden);
			}

			if (!InternalTestAllowedEnterpriseCodes.Contains(request.EnterpriseCode, StringComparer.OrdinalIgnoreCase))
			{
				return Request.CreateResponse(HttpStatusCode.Forbidden);
			}

			string key = null;

			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var licEnt = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, request.EnterpriseCode));
				LicenceDatabase licDb = null;

				using (var transactionMgr = Db.Connection.BeginTransactionWithManager())
				{
					licDb = GetNextKey(factory, licEnt);
					if (licDb != null)
					{
						PreregisterAndSaveWithRetry(ref licDb, request, isNextKey: true);
						transactionMgr.CommitTransaction();
					}
				}

				if (licDb == null)
				{
					licDb = GetOldestKey(factory, licEnt);
					PreregisterAndSaveWithRetry(ref licDb, request, isNextKey: false);
				}
				key = licEnt.LE_EnterpriseCode + licDb.LD_ServerCode;
			}

			return Request.CreateResponse(key);
		}

		static LicenceDatabase GetNextKey(BusinessObjectFactory factory, LicenceEnterprise licEnterprise)
		{
			var query = new ZQuery(LicenceDatabaseSchema.LD_LE, licEnterprise.PK).AddToFilter(LicenceDatabaseSchema.LD_Status, DatabaseStatusList.Codes.NON);
			var licDb = factory.LoadTop1<LicenceDatabase>(query);
			if (licDb == null)
			{
				var numberFactory = new NonFormattedNumberFountainFactory("InternalTestKeyServerID" + licEnterprise.LE_EnterpriseCode).New();
				var nextNumber = numberFactory.GetNext(Db.Connection);
				if (nextNumber <= 0xFFF)
				{
					licDb = factory.New<LicenceDatabase>();
					licDb.LD_LE = licEnterprise.PK;
					licDb.LD_ServerCode = nextNumber.ToString("X", CultureInfo.InvariantCulture).PadLeft(3, '0');
					licDb.LD_LicenceType = DatabaseTypes.Codes.Test;
				}
			}

			return licDb;
		}

		static void PreregisterAndSave(LicenceDatabase licDb, InternalTestKeyRequest request)
		{
			licDb.LD_Status = DatabaseStatusList.Codes.Preregistered;
			licDb.LD_HostServerName = request.ServerName;
			licDb.LD_HostDBName = request.DatabaseName;
			licDb.LD_LicenceExpiry = ZDateTime.UtcNow.AddDays(60);
			licDb.LD_Product = "CWN";
			licDb.Factory.Save();
		}

		static readonly int MaxRetryCounts = 3;

		void PreregisterAndSaveWithRetry(ref LicenceDatabase licDb, InternalTestKeyRequest request, bool isNextKey)
		{
			try
			{
				PreregisterAndSave(licDb, request);
			}
			catch (ZSaveConcurrencyException)
			{
				for (var i = 1; i <= MaxRetryCounts; i++)
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					try
					{
						if (isNextKey)
						{
							licDb = GetNextKey(factory, licDb.LicEnterprise);
						}
						else
						{
							licDb = GetOldestKey(factory, licDb.LicEnterprise);
						}

						if (licDb != null)
						{
							PreregisterAndSave(licDb, request);
						}

						break;
					}
					catch (ZSaveConcurrencyException ex)
					{
						var logMessage = $"The concurrency error occurred three times during the save period | RequestInfo{{DataBaseName:{request.DatabaseName},EnterpriseCode:{request.EnterpriseCode},ServerName:{request.ServerName}}}";
						AddWarnLog(logMessage, ((int)HttpStatusCode.BadRequest), routingPath: RoutingPath, ex: ex);
						if (i == MaxRetryCounts)
						{
							ErrorReporter.ReportOnce("ProductKeyController cannot save the changes of LicenceDatabase because of concurrency error", logMessage, ex);
						}

						continue;
					}
				}
			}
		}

		static LicenceDatabase GetOldestKey(BusinessObjectFactory factory, LicenceEnterprise licEnterprise)
		{
			var query = new ZQuery(LicenceDatabaseSchema.LD_LE, licEnterprise.PK)
				.AddToFilter(LicenceDatabaseSchema.LD_Status, SQLComparisonOperator.NotEqual, DatabaseStatusList.Codes.NON);
			query.OrderBy = LicenceDatabaseSchema.Constants.LD_LicenceExpiry;
			return factory.LoadTop1<LicenceDatabase>(query);
		}

		static string[] InternalTestAllowedEnterpriseCodes => new[] { "WUT" };
	}
}
