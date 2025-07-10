using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Caching;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.Utilities
{
	public class UserActivityLogger
	{
		#region Constants

		public const int NumberOfLogs = 3;

		public static string CookieKey { get { return "ediWebTrackerActivity"; } }

		#endregion

		#region Static

		public static List<UserActivityLog> ActivityLogs
		{
			get
			{
				lock (staticLock)
				{
					List<UserActivityLog> result = new List<UserActivityLog>();
					UserActivityLogger logger = Current;
					foreach (UserActivityLog log in Current.Logs)
					{
						result.Add(log);
					}
					return result;
				}
			}
		}

		public static void SetLogActionTime(string actionKey)
		{
			lock (staticLock)
			{
				UserActivityLogger logger = Current;
				logger.LogActionTime(actionKey);
				Cache(logger);
			}
		}

		public static void AddNewLog()
		{
			var log = new UserActivityLog();
			lock (staticLock)
			{
				UserActivityLogger logger = Current;
				logger.AddNewLog(log);
				Cache(logger);
			}
		}

		static void Cache(UserActivityLogger logger)
		{
			Guid key = LoggerKey;
			if (key != Guid.Empty)
			{
				try
				{
					if (HttpRuntime.Cache[key.ToString()] == null)
					{
						HttpRuntime.Cache.Add(key.ToString(), logger, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(24, 0, 0), CacheItemPriority.AboveNormal, null);
					}
					else
					{
						HttpRuntime.Cache[key.ToString()] = logger;
					}
				}
				catch (Exception e) when (!e.IsCriticalException()) { } //HttpRuntime.Cache can throw internal exceptions. We don't care about any non-critical exceptions.
			}
		}

		static UserActivityLogger Current
		{
			get
			{
				UserActivityLogger logger = null;
				Guid key = LoggerKey;
				if (key != Guid.Empty && HttpRuntime.Cache[key.ToString()] != null)
				{
					logger = HttpRuntime.Cache[key.ToString()] as UserActivityLogger;
				}
				if (logger == null)
				{
					logger = new UserActivityLogger();
				}
				return logger;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "IIS time, not PC")]
		static Guid LoggerKey
		{
			get
			{
				var result = Guid.Empty;
				if (HttpContext.Current?.Request == null || HttpContext.Current.Response == null)
				{
					return result;
				}

				if (Guid.TryParse(HttpContext.Current.Request.Cookies[CookieKey]?.Value, out result))
				{
					return result;
				}

				if (Guid.TryParse(HttpContext.Current.Response.Cookies[CookieKey]?.Value, out result))
				{
					return result;
				}

				if (HttpContext.Current.Request.IsAuthenticated)
				{
					HttpContext.Current.Response.Cookies.Remove(CookieKey);

					result = Guid.NewGuid();
					var loggerKeyCookie = new HttpCookie(CookieKey, result.ToString());
					loggerKeyCookie.Expires = DateTime.Now.AddMonths(12); // IIS time, not PC
					HttpContext.Current.Response.Cookies.Add(loggerKeyCookie);
				}

				return result;
			}
		}

		readonly static object staticLock = new object();

		#endregion

		#region Constructors

		public UserActivityLogger()
		{
			logs = new LinkedList<UserActivityLog>();
		}

		#endregion

		#region Properties

		public LinkedList<UserActivityLog> Logs
		{
			get
			{
				return logs;
			}
		}

		#endregion

		#region Methods

		public void AddNewLog(UserActivityLog log)
		{
			lock (instanceLock)
			{
				logs.AddFirst(log);
				if (logs.Count > NumberOfLogs)
				{
					logs.RemoveLast();
				}
			}
		}

		public void LogActionTime(string actionKey)
		{
			lock (instanceLock)
			{
				if (logs.First != null)
				{
					logs.First.Value.LogActionTime(actionKey);
				}
			}
		}

		#endregion

		#region Implementation

		readonly LinkedList<UserActivityLog> logs;
		readonly object instanceLock = new object();

		#endregion
	}
}
