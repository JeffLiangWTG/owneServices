using System;
using System.Collections.Concurrent;
using CargoWise.Data;
using CargoWise.EntityFramework;
using NLog;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class TrustedController : ControllerWithEnvironment
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Access to ConcurrentDictionary controlled with a lock")]
		public TrustedController()
		{
			using (Db.DisposableActionForDbConnection())
			{
				lock (Loggers)
				{
					if (Logger == null)
					{
						var type = GetType();
						if (Loggers.TryGetValue(type.FullName, out var logger))
						{
							Logger = logger;
						}
						else
						{
							Loggers[type.FullName] = Logger = new NLogWrapper(GetType());
						}
					}
				}
				Init();
			}
		}

		public TrustedController(NLogWrapper logger)
		{
			using (Db.DisposableActionForDbConnection())
			{
				Logger = logger;
				Init();
			}
		}

		void Init()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Factory = new BusinessObjectFactory() { RefreshEnabled = false };
				LazyRoutingPath = new Lazy<string>(() =>
				{
					var uriAsString = Request.RequestUri.GetLeftPart(UriPartial.Path);
					var pathIdx = uriAsString.IndexOf("/api/");
					var path = pathIdx > 0 ? uriAsString.Substring(pathIdx + 1) : uriAsString;
					return $"{Guid.NewGuid()} {path}";
				});
			}
		}

		protected BadRequestWithErrorMessages BadRequest(string code, string message) =>
			new BadRequestWithErrorMessages(new ErrorMessages(code, message), this);

		protected BadRequestWithErrorMessages InternalServerError(string message) =>
			new BadRequestWithErrorMessages(new ErrorMessages(StatusCodes.InternalServerError, message), this);

		class StatusCodes
		{
			public const string InternalServerError = "500";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Share logger across requests to avoid overhead")]
		static readonly ConcurrentDictionary<string, NLogWrapper> Loggers = new ConcurrentDictionary<string, NLogWrapper>();
		
		public BusinessObjectFactory Factory { get; private set; }
		public string RoutingPath => LazyRoutingPath.Value;
		Lazy<string> LazyRoutingPath { get; set; }
		#region Logger

		public NLogWrapper Logger { get; private set; }

		public void AddInfoLog(string message, int statusCode, string sessionId = "", string userId = "", string routingPath = "", string product = "", string systemId = "", string tenantId = "")
		{
			Logger.AddLog(LogLevel.Info, message, statusCode, sessionId, userId, routingPath, product, systemId, tenantId);
		}

		public void AddWarnLog(string message, int statusCode, string sessionId = "", string userId = "", string routingPath = "", string product = "", string systemId = "", string tenantId = "", Exception ex = null)
		{
			Logger.AddLog(LogLevel.Warn, message, statusCode, sessionId, userId, routingPath, product, systemId, tenantId, ex);
		}

		public void AddErrorLog(string message, int statusCode, string sessionId = "", string userId = "", string routingPath = "", string product = "", string systemId = "", string tenantId = "", Exception ex = null)
		{
			Logger.AddLog(LogLevel.Error, message, statusCode, sessionId, userId, routingPath, product, systemId, tenantId, ex);
		}

		#endregion
	}
}
