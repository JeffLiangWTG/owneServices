using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Threading;
#if NETFRAMEWORK
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
#endif
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

#if DEBUG
using CargoWise.Common.Testing;
#endif

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public abstract class DataRequestHandler<T> :
#if NETFRAMEWORK
		IHttpHandler, IReadOnlySessionState
#endif
#if DEBUG && NETFRAMEWORK
,IDataRequestHandler
#elif NET
IDataRequestHandler
#endif
 where T : DataRequestHelper, new()
	{
		#region IHttpHandler Members

		void CleanUp()
		{
#if DEBUG
			if (!Globals.IsTest)
#endif
			{
				fPKs = null;
				fQueryString = null;
				fBusinessObjects = null;
			}
		}

#if NETFRAMEWORK
		public void ProcessRequest(HttpContext context)
		{
			object user = context.Session != null ? context.Session[ZEnterpriseGlobalBase.SiteUserSessionKey] : null;

			using (Db.DisposableActionForDbConnection())
			{
				var lockTaken = false;

				try
				{
					if (user != null)
					{
						Monitor.Enter(user, ref lockTaken);
					}

					if (QueryString != null && QueryString.Count > 0)
					{
						PreProcessRequest();

						byte[] binaryData = RetrieveFromCache(PKs)
							?? GetBinaryData();

						if (binaryData != null && binaryData.Length > 0)
						{
							if (RequestHelper.EnableCache)
							{
								StoreInCache(binaryData, PKs);
							}

							WriteResponse(binaryData, ContentType, FileName);
						}
						else
						{
							WriteError(context, NoDataErrorMessage);
						}
					}
					else
					{
						WriteError(context, NoDataErrorMessage);
					}
				}
				catch (InvalidQueryStringException ex)
				{
					HttpContext.Current.Response.StatusCode = 400;
					HttpContext.Current.Response.StatusDescription = ex.Message;
					HttpContext.Current.Response.End();
				}
				catch (Exception ex1) when (ex1 is ThreadAbortException)
				{
					// Ignore - this is thrown when calling HttpContext.Current.Response.End() from the WriteResponse() method
				}
				finally
				{
					try
					{
						CleanUp();
						PostProcessRequest();
					}
					finally
					{
						if (lockTaken && user != null)
						{
							Monitor.Exit(user);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected void WriteResponse(byte[] img, string contentType, string filename)
		{
			if (contentType != null)
			{
				HttpContext.Current.Response.ContentType = contentType;
			}

			var contentDispositionType = ContentDispositionType.ToString().ToLowerInvariant();
			HttpContext.Current.Response.AddHeader("Content-Disposition", String.Format("{0}; filename=\"{1}\"", contentDispositionType, filename));
			HttpContext.Current.Response.OutputStream.Write(img, 0, img.Length);
			HttpContext.Current.Response.End();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Java Script code")]
		protected void WriteError(HttpContext context, string message)
		{
			context.Response.Write(String.Format(CultureInfo.InvariantCulture, "<script language=\"javascript\">\nalert(\"{0}\");\njavascript:history.go(-1);\n</script>\n", message));
		}

		#region QueryString

		public virtual NameValueCollection QueryString
		{
			get
			{
				if (fQueryString == null && HttpContext.Current.Request.QueryString != null)
				{
					fQueryString = GetQueryString();
				}
				return fQueryString;
			}
		}
		NameValueCollection fQueryString;

		NameValueCollection GetQueryString()
		{
			if (RequestHelper.UseSecureQueryString)
			{
				return new SecureQueryString(HttpContext.Current.Request[SecureQueryString.QueryStringKey]);
			}
			else
			{
				return HttpContext.Current.Request.QueryString;
			}
		}

		#endregion

		#region AppInstance

		public ZEnterpriseGlobalBase AppInstance
		{
			get
			{
				if (fAppInstance == null)
				{
					fAppInstance = HttpContext.Current.ApplicationInstance as ZEnterpriseGlobalBase;
				}
				return fAppInstance;
			}
		}
		ZEnterpriseGlobalBase fAppInstance;

		#endregion

		#region Cache

		const int DefaultTimeout = 5;

		protected internal void StoreInCache(byte[] binaryData, params ZGuid[] pKeys)
		{
			if (binaryData == null)
			{
				throw new ArgumentNullException(nameof(binaryData), "Null image data");
			}

			try
			{
				HttpContext.Current.Cache.Insert(GetCacheKey(pKeys), binaryData, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(DefaultTimeout), CacheItemPriority.High, null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("CACHEDIMAGESERVICE_STOREIMAGEFAILURE", "Failed to save image data to the cache", ex);
			}
		}

		internal byte[] RetrieveFromCache(params ZGuid[] pKeys)
		{
			return (byte[])HttpContext.Current.Cache[GetCacheKey(pKeys)];
		}

		public void RemoveFromCache(params ZGuid[] pKeys)
		{
			foreach (ZGuid pK in pKeys)
			{
				if (!pK.IsValid)
				{
					throw new InvalidOperationException("Invalid key: ImageKey");
				}
			}

			HttpContext.Current.Cache.Remove(GetCacheKey(pKeys));
		}

		protected virtual string GetCacheKey(params ZGuid[] pKeys)
		{
			return RequestHelper.GetCacheKey(pKeys);
		}

		#endregion

#elif NET
		public DataRequestHandler(IMemoryCache memoryCache, HttpContext context)
		{
			this.memoryCache = memoryCache;
			this.context = context;
		}

		readonly IMemoryCache memoryCache;
		readonly HttpContext context;

		public void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				object user = context.Session != null ? context.Session.GetObject<WebUser>("SiteUser") : null;
				var lockTaken = false;

				try
				{
					if (user != null)
					{
						Monitor.Enter(context, ref lockTaken);
					}

					if (QueryString != null && QueryString.Count > 0)
					{
						PreProcessRequest();

						byte[] binaryData = RetrieveFromCache(PKs)
							?? GetBinaryData();

						if (binaryData != null && binaryData.Length > 0)
						{
							if (RequestHelper.EnableCache)
							{
								StoreInCache(binaryData, PKs);
							}

							WriteResponse(binaryData, ContentType, FileName);
						}
						else
						{
							WriteError(context, NoDataErrorMessage);
						}
					}
					else
					{
						WriteError(context, NoDataErrorMessage);
					}
				}
				catch (InvalidQueryStringException ex)
				{
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					context.Response.ContentType = "text/plain";
					context.Response.WriteAsync(ex.Message).GetAwaiter().GetResult();
				}
				catch (Exception ex1) when (ex1 is ThreadAbortException)
				{
					// Ignore - this is thrown when calling HttpContext.Current.Response.End() from the WriteResponse() method
				}
				finally
				{
					try
					{
						CleanUp();
						PostProcessRequest();
					}
					finally
					{
						if (lockTaken && user != null)
						{
							Monitor.Exit(context);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected void WriteResponse(byte[] img, string contentType, string filename)
		{
			if (contentType != null)
			{
				context.Response.ContentType = contentType;
			}

			var contentDispositionType = ContentDispositionType.ToString().ToLowerInvariant();
			context.Response.Headers.Append("Content-Disposition", String.Format("{0}; filename=\"{1}\"", contentDispositionType, filename));
			context.Response.Body.WriteAsync(img, 0, img.Length).GetAwaiter().GetResult();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Java Script code")]
		protected void WriteError(HttpContext context, string message)
		{
			context.Response.WriteAsync(String.Format(CultureInfo.InvariantCulture, "<script language=\"javascript\">\nalert(\"{0}\");\njavascript:history.go(-1);\n</script>\n", message)).GetAwaiter().GetResult();
		}

		#region QueryString

		public virtual NameValueCollection QueryString
		{
			get
			{
				if (fQueryString == null)
				{
					fQueryString = GetQueryString();
				}
				return fQueryString;
			}
		}
		NameValueCollection fQueryString;

		NameValueCollection GetQueryString()
		{
			if (RequestHelper.UseSecureQueryString)
			{
				return new SecureQueryString(context.Request.Query[SecureQueryString.QueryStringKey]);
			}
			else
			{
				var collection = new NameValueCollection();
				foreach (var kvp in context.Request.Query)
				{
					collection.Add(kvp.Key, kvp.Value);
				}
				return collection;
			}
		}

		#endregion

		#region Cache

		const int DefaultTimeout = 5;

		protected internal void StoreInCache(byte[] binaryData, params ZGuid[] pKeys)
		{
			if (binaryData == null)
			{
				throw new ArgumentNullException(nameof(binaryData), "Null image data");
			}

			try
			{
				var cacheKey = GetCacheKey(pKeys);

				var cacheEntryOptions = new MemoryCacheEntryOptions()
					.SetSlidingExpiration(TimeSpan.FromMinutes(DefaultTimeout))
					.SetPriority(CacheItemPriority.High);

				memoryCache.Set(cacheKey, binaryData, cacheEntryOptions);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("CACHEDIMAGESERVICE_STOREIMAGEFAILURE", "Failed to save image data to the cache", ex);
			}
		}

		internal byte[] RetrieveFromCache(params ZGuid[] pKeys)
		{
			if (memoryCache.TryGetValue(GetCacheKey(pKeys), out byte[] binaryData))
			{
				return binaryData;
			}
			return null;
		}

		public void RemoveFromCache(params ZGuid[] pKeys)
		{
			foreach (ZGuid pK in pKeys)
			{
				if (!pK.IsValid)
				{
					throw new InvalidOperationException("Invalid key: ImageKey");
				}
			}

			var cacheKey = GetCacheKey(pKeys);
			if (memoryCache.TryGetValue(cacheKey, out _))
			{
				memoryCache.Remove(cacheKey);
			}
		}

		protected virtual string GetCacheKey(params ZGuid[] pKeys)
		{
			return RequestHelper.GetCacheKey(context, pKeys);
		}

		#endregion
#endif

		protected virtual ContentDispositionType ContentDispositionType
		{
			get { return ContentDispositionType.Attachment; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May only be required by developers")]
		public virtual string NoDataErrorMessage
		{
			get { return "No data found."; }
		}

		public bool IsReusable
		{
			get { return false; }
		}

		protected virtual void PreProcessRequest()
		{
		}

		protected virtual void PostProcessRequest()
		{
		}
		#endregion

		#region DataRequestHelper

		public static T RequestHelper
		{
			get
			{
				if (fRequestHelper == null)
				{
					fRequestHelper = new T();
				}

				return fRequestHelper;
			}
		}
#if DEBUG
		[SuppressThreadStaticFieldMessage]
#endif
		static T fRequestHelper;

		#endregion

		#region BusinessObjects

		protected abstract BusinessObject[] GetNewBusinessObjects();

		public BusinessObject[] BusinessObjects
		{
			get
			{
				if (fBusinessObjects == null)
				{
					fBusinessObjects = GetNewBusinessObjects();
				}
				return fBusinessObjects;
			}
		}
		BusinessObject[] fBusinessObjects;

		protected internal ZGuid GetZGuidFromStringSafely(string str)
		{
			ZGuid result;

			try
			{
				result = new ZGuid(str);
			}
			catch (Exception e) when (e is ZTypeValueException || e is FormatException)
			{
				result = ZGuid.Invalid;
			}

			return result;
		}

		#endregion

		#region PKs

		public ZGuid[] PKs
		{
			get
			{
				if (fPKs == null)
				{
					fPKs = GetNewPKs();
				}
				return fPKs;
			}
		}
		ZGuid[] fPKs;

		protected virtual ZGuid[] GetNewPKs()
		{
			string[] stringPKs = QueryString[DataRequestHelper.DataKey].Split(',');
			ZGuid[] result = new ZGuid[stringPKs.Length];
			for (int i = 0; i < stringPKs.Length; i++)
			{
				result[i] = new ZGuid(stringPKs[i]);
			}
			return result;
		}

		#endregion

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = GetNewFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		#endregion

		#region FileName

		public abstract string FileName { get; }

		#endregion

		#region Data

		public abstract ZBlob GetBinaryData();
#if DEBUG
		public System.Diagnostics.Stopwatch StopWatch = new System.Diagnostics.Stopwatch();
		public virtual void StopStopwatchForTesting()
		{
			StopWatch.Stop();
		}
#endif
#endregion

		#region ContentType

		public virtual string ContentType
		{
			get
			{
				string contentType = null;

				if (!string.IsNullOrEmpty(FileName))
				{
					var extension = Path.GetExtension(PathValidation.GetSafeFilename(FileName));
					contentType = GetContentTypeFromExtension(extension);
				}

				return contentType;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected internal string GetContentTypeFromExtension(string extension)
		{
			if (!String.IsNullOrEmpty(extension))
			{
				string lowerExtension = extension.ToLower();
				string contentType = null;

				switch (lowerExtension)
				{
					case ".tiff":
					case ".tif":
						{
							contentType = DataContentTypes.Tiff;
							break;
						}

					case ".xls":
						{
							contentType = DataContentTypes.Excel;
							break;
						}

					case ".pdf":
						{
							contentType = DataContentTypes.Pdf;
							break;
						}

					default:
						{
							try
							{
								Microsoft.Win32.RegistryKey classesRoot = Microsoft.Win32.Registry.ClassesRoot;
								string dotExt = extension.ToLower();
								Microsoft.Win32.RegistryKey typeKey = classesRoot.OpenSubKey(@"MIME\Database\Content Type");
								foreach (string keyname in typeKey.GetSubKeyNames())
								{
									Microsoft.Win32.RegistryKey curKey = classesRoot.OpenSubKey(@"MIME\Database\Content Type\" + keyname);
									object registryExtValue = curKey.GetValue("Extension");

									if (registryExtValue != null && (registryExtValue.ToString().ToLower() == dotExt))
									{
										contentType = keyname;
									}
								}
							}
							catch (Exception e) when (!e.IsCriticalException())
							{
								// Ignore all exceptions - default type will be returned by the browser
							}

							break;
						}
				}

				return contentType;
			}

			return null;
		}

		#endregion
	}

	#region IDataRequestHandler
#if DEBUG || NET

	public interface IDataRequestHandler
	{
	}

#endif
#endregion
}
