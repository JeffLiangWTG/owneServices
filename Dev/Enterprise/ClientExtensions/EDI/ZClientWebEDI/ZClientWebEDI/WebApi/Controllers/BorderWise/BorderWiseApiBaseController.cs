using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;
using NLog;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	public class BorderWiseApiBaseController : ControllerWithEnvironment
	{
		internal const string ApiKey = "b7c25e84-5c14-4467-ac23-8a36034209da";

		protected NLogWrapper Logger { get; private set; }

		public BorderWiseApiBaseController()
		{
			if (Logger == null)
			{
				Logger = new NLogWrapper(GetType());
			}
		}

		public BorderWiseApiBaseController(NLogWrapper logger)
		{
			Logger = logger;
		}

		internal static bool IsValidApiKey(string apiKey)
		{
			return ApiKey.Equals(apiKey, StringComparison.OrdinalIgnoreCase);
		}

		public IEnumerable<Guid> GetOrganizationPksUnderWiseTechGlobalCompany(string sessionId, string routingPath)
		{
			Logger.AddLog(LogLevel.Info, "Get WTG related companies.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			var clientCompanyQuery = new ZDBOnlyQuery(typeof(ClientCompany));

			var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientCompanySchema.LCC_LD);
			licenceDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_DatabaseNumber, 1); // This is for WiseTech global license database
			clientCompanyQuery.AddSubQuery(licenceDatabaseSubQuery, JoinCondition.And);
			clientCompanyQuery.AddToFilter(ClientCompanySchema.LCC_OH, SQLComparisonOperator.NotEqual, null);

			var clientCompanies = DataFactory.Load<ClientCompany>(clientCompanyQuery);

			if (!(clientCompanies.Length > 0))
			{
				Logger.AddLog(LogLevel.Info, "Not found WTG related companies.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return default;
			}

			var infoMessage = $"Get WTG related companies - count: {clientCompanies.Length}";
			Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			return clientCompanies.Select(c => c.LCC_OH.ToGuid());
		}

		BusinessObjectFactory dataFactory;
		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseApiBaseController) });
	}
}
