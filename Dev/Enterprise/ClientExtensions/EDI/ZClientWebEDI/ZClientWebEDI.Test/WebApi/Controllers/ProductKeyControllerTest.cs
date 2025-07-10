using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class ProductKeyControllerTest : TestCaseWithFactory
	{
		public void TestGetInternalTestKeyFirstKey()
		{
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "oddisee" });
			var key = ReadResponse(response);
			var licDb = LoadProductKey(key);
			AssertNotNull(key);
			AssertEquals("WUT", licDb.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(Dns.GetHostName(), licDb.LD_HostServerName);
			AssertEquals("oddisee", licDb.LD_HostDBName);
			AssertEquals(DatabaseStatusList.Codes.Preregistered, licDb.LD_Status);
			AssertEquals(DatabaseTypes.Codes.Test, licDb.LD_LicenceType);
			AssertEquals("CWN", licDb.LD_Product);
		}

		public void TestGetInternalTestKeyServerWithInstance()
		{
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName() + @"\INSTANCE1", DatabaseName = "oddisee" });
			var key = ReadResponse(response);
			var licDb = LoadProductKey(key);
			AssertNotNull(key);
			AssertEquals("WUT", licDb.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(Dns.GetHostName() + @"\INSTANCE1", licDb.LD_HostServerName);
			AssertEquals("oddisee", licDb.LD_HostDBName);
			AssertEquals(DatabaseStatusList.Codes.Preregistered, licDb.LD_Status);
			AssertEquals(DatabaseTypes.Codes.Test, licDb.LD_LicenceType);
			AssertEquals("CWN", licDb.LD_Product);
		}

		public void TestGetInternalTestMultipleKeys()
		{
			new NonFormattedNumberFountainFactory("InternalTestKeyServerIDWUT").New().SetNext(Db.Connection, nextValue: 1);
			var key1 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "mydbone" }));
			var key2 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "mydbtwo" }));
			var key3 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "mydbthree" }));
			AssertEquals(3, new[] { key1, key2, key3 }.Distinct().Count());
		}

		public void TestGetInternalTestRecycleNonKey()
		{
			var key1 = Factory.NewWithValidTestData<LicenceDatabase>();
			key1.LD_LE = wutLic.PK;
			key1.LD_Status = DatabaseStatusList.Codes.NON;
			Factory.Save();
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "oddisee" });
			var key2 = LoadProductKey(ReadResponse(response));
			AssertEquals(key2.PK, key1.PK);
			AssertEquals("WUT", key2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(Dns.GetHostName(), key2.LD_HostServerName);
			AssertEquals("PRE", key2.LD_Status);
			AssertEquals("CWN", key2.LD_Product);
		}

		public void TestGetInternalTestKeyForDisallowedEnterpriseCode()
		{
			var lic = Factory.NewWithValidTestData<LicenceEnterprise>();
			lic.LE_EnterpriseCode = "AAA";
			Factory.Save();
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "AAA", ServerName = Dns.GetHostName(), DatabaseName = "oddisee" });
			AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			AssertEquals(false, lic.Databases.Any());
		}

		public void TestGetInternalTestKeyForNonExistantEnterpriseCode()
		{
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "XXX", ServerName = Dns.GetHostName(), DatabaseName = "oddisee" });
			AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
		}

		public void TestGetInternalTestKeyForDisallowedIpAddress()
		{
			SetRequestContext("122.106.28.55");
			var response = GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "oddisee" });
			AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			AssertEquals(false, wutLic.Databases.Any());
		}

		[TestDate(2018, 3, 20)]
		public void TestGetInternalTestKeyForAllKeysInUse()
		{
			new NonFormattedNumberFountainFactory("InternalTestKeyServerIDWUT").New().SetNext(Db.Connection, nextValue: 0xFFF);
			var db1 = Factory.New<LicenceDatabase>();
			db1.LD_LE = wutLic.PK;
			db1.LD_ServerCode = "001";
			db1.LD_Status = "PRE";
			db1.LD_LicenceExpiry = TestDateAttribute.Date;
			var db2 = Factory.New<LicenceDatabase>();
			db2.LD_LE = wutLic.PK;
			db2.LD_ServerCode = "002";
			db2.LD_Status = "REG";
			db2.LD_LicenceExpiry = TestDateAttribute.Date.AddDays(1);
			Factory.Save();
			var response1 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r1" }));
			var response2 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r2" }));
			var response3 = ReadResponse(GetProductKeyController().GetInternalTestKey(new InternalTestKeyRequest()
			{ EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r3" }));
			var key1 = LoadProductKey(response1);
			var key2 = LoadProductKey(response2);
			var key3 = LoadProductKey(response3);
			AssertEquals("FFF", key1.LD_ServerCode);
			AssertEquals(TestDateAttribute.Date.AddDays(60), key1.LD_LicenceExpiry);
			AssertEquals(db1.PK, key2.PK);
			AssertEquals(TestDateAttribute.Date.AddDays(60), key2.LD_LicenceExpiry);
			AssertEquals(db2.PK, key3.PK);
			AssertEquals(TestDateAttribute.Date.AddDays(60), key3.LD_LicenceExpiry);
		}

		public void TestGetNextKey_ConcurrencyError()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = wutLic.PK;
			database.LD_Status = DatabaseStatusList.Codes.NON;
			Factory.Save();

			var method = typeof(ProductKeyController).GetMethod("GetNextKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var savingMethod = typeof(ProductKeyController).GetMethod("PreregisterAndSaveWithRetry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(method);
			AssertNotNull(savingMethod);

			var controller = GetProductKeyController();

			var request1 = new InternalTestKeyRequest() { EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r1" };
			var request2 = new InternalTestKeyRequest() { EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r2" };

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var key1 = method.Invoke(null, new object[] { factory1, wutLic }) as LicenceDatabase;
			var key2 = method.Invoke(null, new object[] { factory2, wutLic }) as LicenceDatabase;
			AssertEquals(key1.PK, key2.PK);

			var key1Args = new object[] { key1, request1, true };
			savingMethod.Invoke(controller, key1Args);
			AssertEquals("The licenceDatabase should not be changed because no concurrency error happened", key1.PK, (key1Args[0] as LicenceDatabase).PK);

			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = wutLic.PK;
			database2.LD_Status = DatabaseStatusList.Codes.NON;
			Factory.Save();

			var key2Args = new object[] { key2, request2, true };
			AssertNoExceptionThrown(() =>
			{
				savingMethod.Invoke(controller, key2Args);
			});

			AssertNotEquals("API should return another key or create new one when concurrency error happend", key1.PK, (key2Args[0] as LicenceDatabase).PK);
		}

		public void TestGetOldestKey_ConcurrencyError()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = wutLic.PK;
			database.LD_Status = DatabaseStatusList.Codes.Preregistered;
			Factory.Save();

			var method = typeof(ProductKeyController).GetMethod("GetOldestKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var savingMethod = typeof(ProductKeyController).GetMethod("PreregisterAndSaveWithRetry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(method);
			AssertNotNull(savingMethod);

			var controller = GetProductKeyController();

			var request1 = new InternalTestKeyRequest() { EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r1" };
			var request2 = new InternalTestKeyRequest() { EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r2" };

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var key1 = method.Invoke(null, new object[] { factory1, wutLic }) as LicenceDatabase;
			var key2 = method.Invoke(null, new object[] { factory2, wutLic }) as LicenceDatabase;
			AssertEquals(key1.PK, key2.PK);

			var key1Args = new object[] { key1, request1, true };
			savingMethod.Invoke(controller, key1Args);
			AssertEquals("The licenceDatabase should not be changed because no concurrency error happened", key1.PK, (key1Args[0] as LicenceDatabase).PK);

			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = wutLic.PK;
			database2.LD_Status = DatabaseStatusList.Codes.Preregistered;
			Factory.Save();

			var key2Args = new object[] { key2, request2, true };
			AssertNoExceptionThrown(() =>
			{
				savingMethod.Invoke(controller, key2Args);
			});

			AssertNotEquals("API should return another key or create new one when concurrency error happend", key1.PK, (key2Args[0] as LicenceDatabase).PK);
		}

		public void TestGetConcurrencyErrorLog()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var database = Factory.NewWithValidTestData<LicenceDatabase>();
				database.LD_LE = wutLic.PK;
				database.LD_Status = DatabaseStatusList.Codes.Preregistered;
				Factory.Save();

				var method = typeof(ProductKeyController).GetMethod("GetOldestKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var savingMethod = typeof(ProductKeyController).GetMethod("PreregisterAndSaveWithRetry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				AssertNotNull(method);
				AssertNotNull(savingMethod);

				var controller = GetProductKeyController();
				var request = new InternalTestKeyRequest() { EnterpriseCode = "WUT", ServerName = Dns.GetHostName(), DatabaseName = "r2" };
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var key = method.Invoke(null, new object[] { newFactory, wutLic }) as LicenceDatabase;
				var keyRequestArgs = new object[] { key, request, true };
				AssertNoExceptionThrown(() =>
				{
					BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
					{
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~"), null, ((IDbConnected)factory).Connection), factory);
					});
					savingMethod.Invoke(controller, keyRequestArgs);
				});
				AssertEquals("ProductKeyController cannot save the changes of LicenceDatabase because of concurrency error", ErrorReporter.LastKeyReported);
			}
		}

		ProductKeyController GetProductKeyController()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "https://myaccount-portal.cargowise.com/myaccount/api/ProductKey/Mocked");
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
			return new ProductKeyController()
			{ Request = request };
		}

		string ReadResponse(HttpResponseMessage response)
		{
			var readTask = response.Content.ReadAsStringAsync();
			readTask.Wait();
			return readTask.Result.Trim('\"');
		}

		LicenceDatabase LoadProductKey(string registrationKey)
		{
			return LicenceDatabase.LoadFromEnterpriseAndServerCode(new BusinessObjectFactory()
			{ RefreshEnabled = false }, registrationKey.Substring(0, 3), registrationKey.Substring(3));
		}

		void SetRequestContext(string ip)
		{
			var mockRequest = new Mock<HttpWorkerRequest>();
			mockRequest.Setup(o => o.GetRemoteAddress()).Returns(ip);
			mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/ProductKey/Mocked");
			HttpContext.Current = new HttpContext(mockRequest.Object);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wutLic = Factory.NewWithValidTestData<LicenceEnterprise>();
			wutLic.LE_EnterpriseCode = "WUT";
			Factory.Save();
			SetRequestContext("10.61.165.176");
		}

		LicenceEnterprise wutLic;
	}
}
