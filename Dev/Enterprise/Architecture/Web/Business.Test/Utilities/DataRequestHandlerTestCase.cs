using System;
using System.IO;
using System.Threading;
#if NETFRAMEWORK
using System.Web;
#endif
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	[HttpContextEnabledTest]
	[TestsSubclassesOf(typeof(IDataRequestHandler), ExcludePrivate = true)]
	public abstract class DataRequestHandlerTestCase<T> : TestCaseWithFactory
		 where T : DataRequestHelper, new()
	{
		#region Setup

#if NETFRAMEWORK
		protected DummyHttpApplication ApplicationInstance
		{
			get { return HttpContext.Current.ApplicationInstance as DummyHttpApplication; }
		}
#endif

		protected DataRequestHandler<T> RequestHandler
		{
			get
			{
				if (fRequestHandler == null)
				{
					fRequestHandler = GetNewRequestHandler();
				}
				return fRequestHandler;
			}
		}
		DataRequestHandler<T> fRequestHandler;

		protected abstract DataRequestHandler<T> GetNewRequestHandler();

		#endregion

		#region HandlersMethods

		protected void RequestHandlerStoreInCache(byte[] binaryData, params ZGuid[] pKeys)
		{
			RequestHandler.StoreInCache(binaryData, pKeys);
		}

		protected byte[] RequestHandlerRetrieveFromCache(params ZGuid[] pKeys)
		{
			return RequestHandler.RetrieveFromCache(pKeys);
		}

		protected void RequestHandlerRemoveFromCache(params ZGuid[] pKeys)
		{
			RequestHandler.RemoveFromCache(pKeys);
		}

		#endregion

		#region TestCache

		public void TestCache()
		{
			TestCacheCore();
		}

		protected virtual void TestCacheCore()
		{
			byte[] retrievedData = RequestHandler.RetrieveFromCache(RequestHandler.PKs);

			AssertEquals("The cache must be empty initially.", null, retrievedData);

			ZBlob testData = RequestHandler.GetBinaryData();

			AssertNotEquals("The test data must not be empty", ZBlob.Empty, testData);

			RequestHandler.StoreInCache(testData, RequestHandler.PKs);
			retrievedData = RequestHandler.RetrieveFromCache(RequestHandler.PKs);

			AssertEquals("The object retrieved from the cache must equal the object added.", testData, retrievedData);

			RequestHandler.RemoveFromCache(RequestHandler.PKs);
			retrievedData = RequestHandler.RetrieveFromCache(RequestHandler.PKs);

			AssertEquals("The object must be removed from the cache successfully.", null, retrievedData);

			// Prevent leak detection from firing incorrectly
			RequestHandler.Factory.Save();
		}
		#endregion

#if NETFRAMEWORK
		#region TestNoBinaryData

		public void TestNoBinaryData()
		{
			RequestHandler.QueryString.Clear();
			AssertEquals("Querystring should be empty", 0, RequestHandler.QueryString.Count);

			MemoryStream ms = new MemoryStream();
			TestResponseFilter testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
			HttpContext.Current.Response.Filter = testResponseFilter;
			HttpContext.Current.Session["SiteUser"] = new OrgContactWebUser();
			RequestHandler.ProcessRequest(HttpContext.Current);

			AssertEquals("The content type should be text/html.", "text/html", HttpContext.Current.Response.ContentType);

			HttpContext.Current.Response.Flush();
			ms.Position = 0;
			StreamReader reader = new StreamReader(ms);
			string responseData = reader.ReadToEnd();
			reader.Close();

			AssertContains("The response should be the error message.", RequestHandler.NoDataErrorMessage, responseData);
			AssertNull("The Content-Disposition header should not exist.", ApplicationInstance.WorkerRequest.Headers["Content-Disposition"]);
		}

		#endregion

		#region TestWithBinaryData

		public virtual void TestWithBinaryData()
		{
			AssertNotEquals("Querystring should not be empty", 0, RequestHandler.QueryString.Count);

			using (MemoryStream ms = new MemoryStream())
			{
				TestResponseFilter testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
				HttpContext.Current.Response.Filter = testResponseFilter;
				HttpContext.Current.Session["SiteUser"] = new OrgContactWebUser();
				RequestHandler.ProcessRequest(HttpContext.Current);

				HttpContext.Current.Response.Flush();
				HttpContext.Current.Response.End();

				AssertEquals("The content type should be the specified content type.", RequestHandler.ContentType, HttpContext.Current.Response.ContentType);
				AssertNotNull("The Content-Disposition header should exist.", ApplicationInstance.WorkerRequest.Headers["Content-Disposition"]);

				AssertEquals("The Content-Disposition header should specify and attachment and contain the file name.",
					ExpectedContentDispositionForTestWithBinaryData, ApplicationInstance.WorkerRequest.Headers["Content-Disposition"]);

				byte[] binaryData = RequestHandler.GetBinaryData();
				AssertEquals("The binary data length should be the same as the length of the returned data.", ms.Length, binaryData.Length);
			}
		}

		#endregion
#elif NET
		#region TestNoBinaryData

		public void TestNoBinaryData()
		{
			using var memoryStream = new MemoryStream();
			HttpContextEnabledTestAttribute.HttpContext.Response.Body = memoryStream;

			RequestHandler.QueryString.Clear();
			AssertEquals("Querystring should be empty", 0, RequestHandler.QueryString.Count);

			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject<WebUser>("SiteUser", new OrgContactWebUser());
			RequestHandler.ProcessRequest(HttpContextEnabledTestAttribute.HttpContext);

			HttpContextEnabledTestAttribute.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
			var reader = new StreamReader(HttpContextEnabledTestAttribute.HttpContext.Response.Body, System.Text.Encoding.UTF8);
			string responseData = reader.ReadToEndAsync().GetAwaiter().GetResult();

			AssertEquals("The content type should be text/html.", "text/html", HttpContextEnabledTestAttribute.HttpContext.Response.ContentType);

			AssertContains("The response should be the error message.", RequestHandler.NoDataErrorMessage, responseData);
			AssertNull("The Content-Disposition header should not exist.", HttpContextEnabledTestAttribute.HttpContext.Request.Headers["Content-Disposition"]);
		}

		#endregion

		#region TestWithBinaryData

		public virtual void TestWithBinaryData()
		{
			using var memoryStream = new MemoryStream();
			HttpContextEnabledTestAttribute.HttpContext.Response.Body = memoryStream;
			AssertNotEquals("Querystring should not be empty", 0, RequestHandler.QueryString.Count);

			HttpContextEnabledTestAttribute.HttpContext.Session.SetObject<WebUser>("SiteUser", new OrgContactWebUser());
			RequestHandler.ProcessRequest(HttpContextEnabledTestAttribute.HttpContext);

			AssertEquals("The content type should be the specified content type.", RequestHandler.ContentType, HttpContextEnabledTestAttribute.HttpContext.Response.ContentType);
			AssertNotNull("The Content-Disposition header should exist.", HttpContextEnabledTestAttribute.HttpContext.Request.Headers["Content-Disposition"]);

			AssertEquals("The Content-Disposition header should specify and attachment and contain the file name.",
				ExpectedContentDispositionForTestWithBinaryData, HttpContextEnabledTestAttribute.HttpContext.Request.Headers["Content-Disposition"]);

			byte[] binaryData = RequestHandler.GetBinaryData();
			AssertEquals("The binary data length should be the same as the length of the returned data.", memoryStream.Length, binaryData.Length);
		}

		#endregion
#endif
		protected virtual string ExpectedContentDispositionForTestWithBinaryData
		{
			get { return String.Format("attachment; filename=\"{0}\"", RequestHandler.FileName); }
		}

		public virtual object LockingObjectForGetBinaryDataWithLockTests
		{
			get { return RequestHandler.BusinessObjects[0]; }
		}

		public virtual void TestGetBinaryDataWithLock()
		{
			var lockingSleepMilliseconds = 20;
			AssertNotNull("The Locking object object should not be null", LockingObjectForGetBinaryDataWithLockTests);

			var thread = new Thread(() =>
			{
				lock (LockingObjectForGetBinaryDataWithLockTests)
				{
					RequestHandler.StopWatch.Restart();
					Thread.Sleep(lockingSleepMilliseconds);
				}
			});
			thread.Start();
			Thread.Sleep(2);
			AssertNotNull("Should return some data", RequestHandler.GetBinaryData());
			thread.Join();
			Assert("Locking should have caused delay before returning data", RequestHandler.StopWatch.ElapsedMilliseconds > (lockingSleepMilliseconds * 90) / 100);
			Assert("Stopwatch has not been stopped - ensure locking is in place", !RequestHandler.StopWatch.IsRunning);
		}

		#region TestGetContentTypeFromExtension

		public void TestGetContentTypeFromExtension()
		{
			string contentType1 = RequestHandler.GetContentTypeFromExtension(".Pdf");
			AssertEquals("application/pdf", contentType1);

			string contentType2 = RequestHandler.GetContentTypeFromExtension(".xls");
			AssertEquals("application/vnd.ms-excel", contentType2);

			string contentType3 = RequestHandler.GetContentTypeFromExtension(".TIF");
			AssertEquals("image/tiff", contentType3);

			string contentType4 = RequestHandler.GetContentTypeFromExtension(String.Empty);
			AssertEquals(null, contentType4);
		}

		#endregion

		#region TestDefaultContentType

		class EmptyDataRequestHelper : DataRequestHelper
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

		class EmptyDataRequestHandler : DataRequestHandler<EmptyDataRequestHelper>
		{
#if NET
			public EmptyDataRequestHandler()
				: base(HttpContextEnabledTestAttribute.MemoryCache, HttpContextEnabledTestAttribute.HttpContext)
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
		}

		public void TestDefaultContentType()
		{
			EmptyDataRequestHandler emptyHandler = new EmptyDataRequestHandler();
			string defaultContentType = RequestHandler.GetContentTypeFromExtension(".PDF");

			AssertEquals("If no content type is specified, the default content type should be returned.", defaultContentType, emptyHandler.ContentType);
		}

		#endregion

		#region TestIsReusable

		public void TestIsReusable()
		{
			Assert("Should not be reusable - each instance is tied to a specific instance of a business object", !RequestHandler.IsReusable);
		}

		#endregion

		#region TestGetZGuidFromStringSafely

		public void TestGetZGuidFromStringSafely()
		{
			ZGuid validGuid = ZGuid.NewZGuid();
			AssertEquals("valid guid", validGuid, RequestHandler.GetZGuidFromStringSafely(validGuid.ToString()));
			AssertEquals("invalid guid", ZGuid.Invalid, RequestHandler.GetZGuidFromStringSafely("invalid string"));
		}

		#endregion
	}
}
