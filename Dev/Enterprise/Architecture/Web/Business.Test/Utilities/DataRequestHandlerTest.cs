using System;
using System.Collections.Specialized;
using System.Threading;
#if NETFRAMEWORK
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities.Testing
{
	sealed class DataRequestHandlerTest : TestCase
	{
		class DataRequestHelperConcrete : DataRequestHelper
		{
			public override string BaseUrl
			{
				get { return null; }
			}

			public override bool EnableCache
			{
				get { return false; }
			}

			public override bool UseSecureQueryString
			{
				get { return false; }
			}
		}

		class DataRequestHandlerConcrete : DataRequestHandler<DataRequestHelperConcrete>
		{
#if NET
			public DataRequestHandlerConcrete(IMemoryCache memoryCache, HttpContext httpContext)
				: base(memoryCache, httpContext)
			{
			}
#endif
			protected override BusinessObject[] GetNewBusinessObjects()
			{
				return null;
			}

			public override string FileName
			{
				get { return "TestRequest.pdf"; }
			}

			public override ZBlob GetBinaryData()
			{
				return null;
			}

			public override NameValueCollection QueryString
			{
				get
				{
					var query = base.QueryString;
					query.Add("Blah", "More Blah");
					return query;
				}
			}

			protected override ZGuid[] GetNewPKs()
			{
				return new ZGuid[] { ZGuid.NewZGuid() };
			}

			public int MaxCount;

			public int TotalCount;
			int count;

			protected override void PreProcessRequest()
			{
				base.PreProcessRequest();
				Interlocked.Increment(ref TotalCount);
				Interlocked.Increment(ref count);
				MaxCount = Math.Max(count, MaxCount);
			}

			protected override void PostProcessRequest()
			{
				base.PostProcessRequest();
				Interlocked.Decrement(ref count);
			}
		}

		class DataRequestHandlerWithInvalidFileName : DataRequestHandlerConcrete
		{
#if NET
			public DataRequestHandlerWithInvalidFileName(IMemoryCache memoryCache, HttpContext httpContext)
				: base(memoryCache, httpContext)
			{
			}
#endif
			public override string FileName
			{
				get
				{
					return "abc1234\\/*? dsaf2342cc\\//?*|34.pdf";
				}
			}
		}

#if NETFRAMEWORK
		[HttpContextEnabledTest]
		public void TestProcessRequest_MultiThreadsHits()
		{
			var requestHandler = new DataRequestHandlerConcrete();

			HttpContext.Current.Session["SiteUser"] = new OrgContactWebUser();

			Thread[] ths = new Thread[10];

			for (int i = 0; i < ths.Length; i++)
			{
				ths[i] = new Thread(new ParameterizedThreadStart(o =>
				{
					var context = (HttpContext)o;
					HttpContext.Current = context;
					AssertNoExceptionThrown(() => requestHandler.ProcessRequest(context));
				}));
			}

			foreach (var thread in ths)
			{
				thread.Start(HttpContext.Current);
			}

			foreach (var thread in ths)
			{
				thread.Join();
			}

			AssertEquals("Only report should be processed at a time", 1, requestHandler.MaxCount);
			AssertEquals("All threads should run", 10, requestHandler.TotalCount);
		}

		[HttpContextEnabledTest]
		public void TestProcessRequest_NoSiteUser()
		{
			var requestHandler = new DataRequestHandlerConcrete();
			HttpContext.Current.Session["SiteUser"] = null;
			requestHandler.ProcessRequest(HttpContext.Current);
			AssertEquals("Request should be processed when site user is null", 1, requestHandler.TotalCount);
			AssertEquals("Request should be processed when site user is null", 200, HttpContext.Current.Response.StatusCode);
		}

		[HttpContextEnabledTest]
		public void TestProcessRequestWithInvalidFileName()
		{
			var requestHandler = new DataRequestHandlerWithInvalidFileName();
			HttpContext.Current.Session["SiteUser"] = null;
			requestHandler.ProcessRequest(HttpContext.Current);
			AssertEquals("Request should be processed when site user is null", 1, requestHandler.TotalCount);
			AssertEquals("Request should be processed when site user is null", 200, HttpContext.Current.Response.StatusCode);
			AssertEquals(DataContentTypes.Pdf, requestHandler.ContentType);
		}
#elif NET
		[HttpContextEnabledTest]
		public void TestProcessRequest_MultiThreadsHits()
		{
			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject("SiteUser", (WebUser)new OrgContactWebUser());
			var requestHandler = new DataRequestHandlerConcrete(HttpContextEnabledTestAttribute.MemoryCache, HttpContextEnabledTestAttribute.HttpContext);

			Thread[] ths = new Thread[10];

			for (int i = 0; i < ths.Length; i++)
			{
				ths[i] = new Thread(new ParameterizedThreadStart(o =>
				{
					var context = (HttpContext)o;
					AssertNoExceptionThrown(() => requestHandler.ProcessRequest(context));
				}));
			}

			foreach (var thread in ths)
			{
				thread.Start(HttpContextEnabledTestAttribute.HttpContext);
			}

			foreach (var thread in ths)
			{
				thread.Join();
			}

			AssertEquals("Only report should be processed at a time", 1, requestHandler.MaxCount);
			AssertEquals("All threads should run", 10, requestHandler.TotalCount);
		}

		[HttpContextEnabledTest]
		public void TestProcessRequest_NoSiteUser()
		{
			var requestHandler = new DataRequestHandlerConcrete(HttpContextEnabledTestAttribute.MemoryCache, HttpContextEnabledTestAttribute.HttpContext);
			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject<WebUser>("SiteUser", null);
			requestHandler.ProcessRequest(HttpContextEnabledTestAttribute.HttpContext);
			AssertEquals("Request should be processed when site user is null", 1, requestHandler.TotalCount);
			AssertEquals("Request should be processed when site user is null", 200, HttpContextEnabledTestAttribute.HttpContext.Response.StatusCode);
		}

		[HttpContextEnabledTest]
		public void TestProcessRequestWithInvalidFileName()
		{
			var requestHandler = new DataRequestHandlerConcrete(HttpContextEnabledTestAttribute.MemoryCache, HttpContextEnabledTestAttribute.HttpContext);
			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject<WebUser>("SiteUser", null);
			requestHandler.ProcessRequest(HttpContextEnabledTestAttribute.HttpContext);
			AssertEquals("Request should be processed when site user is null", 1, requestHandler.TotalCount);
			AssertEquals("Request should be processed when site user is null", 200, HttpContextEnabledTestAttribute.HttpContext.Response.StatusCode);
			AssertEquals(DataContentTypes.Pdf, requestHandler.ContentType);
		}
#endif
	}
}
