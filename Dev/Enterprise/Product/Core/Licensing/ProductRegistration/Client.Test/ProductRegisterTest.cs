using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Data;
using CargoWise.Licensing;
using CargoWise.Licensing.Registration;
using CargoWise.ProductRegistration.Service;
using Enterprise.Integration.Licensing;
using Enterprise.ProductRegistration.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.ProductRegistration.Client.ProductRegister;

namespace Enterprise.ProductRegistration.Client.Test
{
	public class ProductRegisterTest : TransactionedTestCase
	{
		public void TestClient()
		{
			var reg = new ProductRegister();
			AssertType<RegistrationServiceClientForTests>("client for unit tests", reg.Client);
		}

		public void TestIsMatch()
		{
			DateTime t1 = new DateTime(2014, 10, 17);
			DateTime t2 = new DateTime(2014, 10, 18);
			Guid group1 = Guid.NewGuid();
			Guid group2 = Guid.NewGuid();

			Assert(RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1), new DatabaseUniqueKey("server", "db", t1)));
			Assert(RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1), new DatabaseUniqueKey("server", "db", t1, group1)));
			Assert(RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1, group1), new DatabaseUniqueKey("server2", "db", t2, group1)));
			Assert(RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1, group1), new DatabaseUniqueKey("server", "db", t1, group2)));

			Assert(!RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1, group1), new DatabaseUniqueKey("server2", "db", t1, group2)));
			Assert(!RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1, group1), new DatabaseUniqueKey("server", "db2", t1, group1)));
			Assert(!RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1), new DatabaseUniqueKey("server2", "db", t1)));
			Assert(!RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1), new DatabaseUniqueKey("server", "db2", t1)));
			Assert(!RegistrationKeyLocalVerifier.IsMatch(new DatabaseUniqueKey("server", "db", t1), new DatabaseUniqueKey("server", "db2", t2)));
		}

		public void TestLocalVerify()
		{
			var reg = new ProductRegister();
			reg.Client = null;
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3), "SomeRandomDNSName", null));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
		}

		public void TestLocalVerify_Hosted()
		{
			var reg = new ProductRegister();
			reg.Client = null;
			var testRegKey = CreateTestRegistrationKey();
			testRegKey.HostedLocation = "SYD";
			testRegKey.DbUniqueKey.ConnectionServerName = "SomeRandomDNSName";
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2015, 5, 26), "SomeRandomDNSName", null));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
		}

		public void TestLocalVerify_NotHosted()
		{
			AssertEquals(RegistrationKeyLocalVerifier.NotHostedWithCargoWise, Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise);
			var reg = new ProductRegister();
			reg.Client = null;
			var testRegKey = CreateTestRegistrationKey();
			testRegKey.HostedLocation = Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			testRegKey.DbUniqueKey.ConnectionServerName = "SomeRandomDNSName";
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2015, 5, 26), "SomeRandomDNSName", null));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.Fail, actualResult);
		}

		public void TestLocalVerify_CanVerifyByNameOnly()
		{
			var reg = new ProductRegister();
			reg.Client = null;
			var testRegKey = CreateTestRegistrationKey();
			testRegKey.HostedLocation = "";
			testRegKey.CanVerifyByNameOnly = true;
			testRegKey.DbUniqueKey.ConnectionServerName = "SomeRandomDNSName";
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2015, 5, 26), "SomeRandomDNSName", null));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			AssertEquals(ProductRegistrationVerifyResult.OK, reg.LocalVerify());

			testRegKey.CanVerifyByNameOnly = false;
			AssertEquals(ProductRegistrationVerifyResult.Fail, reg.LocalVerify());

			testRegKey.CanVerifyByNameOnly = null;
			AssertEquals(ProductRegistrationVerifyResult.Fail, reg.LocalVerify());
		}

		public void TestLocalVerify_Unregistered()
		{
			var reg = new ProductRegister();
			reg.Client = null;
			var regKeyProvider = new Mock<IRegistrationKeyProvider>();
			reg.RegKeyProvider = regKeyProvider.Object;
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(new RegistrationKeyXmlPair(new RegistrationKey(), ""));

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.Unregistered, actualResult);
		}

		public void TestLocalVerify_DbUniqueKeyNotMatched()
		{
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			var reg = new ProductRegister();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = null;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("AnotherServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.Fail, actualResult);
		}

		public void TestLocalVerify_UsesPreviousRemoteFailureRegistry()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

			var reg = new ProductRegister();
			reg.Client = null;
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.LocalVerify();

			AssertEquals(ProductRegistrationVerifyResult.Fail, actualResult);
		}

		delegate void SubmitCallback<T>(T request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000);

		public void TestFullVerify()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			response.Status = (int)RegisterStatus.Success;
			var outputStatusCode = HttpStatusCode.OK;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>()
				))
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.FullVerify(cancelToken);
			regKeyProvider.Verify(x => x.SetKey(It.IsAny<string>()), Times.Never());

			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("failure registry is updated", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestFullVerify_LocallyInvalidRemotelyUnregistered()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("AnotherServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)RegisterStatus.Unregistered;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var localResult = reg.LocalVerify();
			var actualResult = reg.FullVerify(cancelToken);

			AssertEquals(ProductRegistrationVerifyResult.Fail, localResult);
			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.Unregistered, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("fail set", (int)ProductRegistrationVerifyResult.Unregistered, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			var outputStatusCode = HttpStatusCode.OK;

			response.Status = (int)RegisterStatus.Success;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Verify(cancelToken);
			regKeyProvider.Verify(x => x.SetKey(It.IsAny<string>()), Times.Never);

			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("failure registry is updated", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerifyDataContracts()
		{
			HttpListener listener;
			string prefix;
			var port = 29000;
			do
			{
				prefix = "http://localhost:" + port;
				listener = new HttpListener();
				listener.Prefixes.Add(prefix + "/api/Registration/Verify/");

				try
				{
					listener.Start();
				}
				catch (HttpListenerException) when (port <= 29050)
				{
					port++;
				}
			}
			while (!listener.IsListening);

			SystemDataRegistry.Instance.ProductRegistrationServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, prefix);
			try
			{
				RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

				var reg = new ProductRegister();
				var testRegKey = CreateTestRegistrationKey();
				var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

				var repository = new MockRepository(MockBehavior.Default);
				var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
				var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
				reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
				reg.RegKeyProvider = regKeyProvider.Object;
				reg.Client = new RegistrationServiceClient();

				dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
				regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

				CancellationToken cancelToken = new CancellationTokenSource().Token;

				var actualResult = reg.Verify(cancelToken);
				HttpListenerContext context = listener.GetContext();

				string requestContent;
				using (var body = context.Request.InputStream)
				{
					using (var reader = new StreamReader(body, context.Request.ContentEncoding))
					{
						requestContent = reader.ReadToEnd();
					}
				}

				AssertEquals(requestContent, $@"<VerifyRequest xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/Enterprise.ProductRegistration.Common""><Key>{{my reg key XML}}</Key><ProductVersion>{ReleaseInfo.Instance.VersionNumber}</ProductVersion><UniqueKey><ConnectionServerName>MyServer</ConnectionServerName><DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated><DatabaseName>MyDb</DatabaseName><GroupId i:nil=""true"" /><ServerName>MyServer</ServerName></UniqueKey></VerifyRequest>");
			}
			finally
			{
				listener.Stop();
			}
		}

		public void TestVerify_ExpiryDateRenewed()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			response.Status = (int)RegisterStatus.Success;
			var outputStatusCode = HttpStatusCode.OK;
			response.Key = MessageSigner.SignXml(RegistrationKeyProviderTest.TestUnsignedKeyXml.Replace(@"<GroupId xsi:nil=""true"" />", "<GroupId>531D0F6F-472F-4FAE-8B0C-C9C1E4467705</GroupId>"));
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Verify(cancelToken);
			regKeyProvider.Verify(x => x.SetKey(response.Key));

			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("failure registry is updated", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_KeyUpdated()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)RegisterStatus.UniqueKeyUpdated;
			response.Key = MessageSigner.SignXml(RegistrationKeyProviderTest.TestUnsignedKeyXml.Replace(@"<GroupId xsi:nil=""true"" />", "<GroupId>531D0F6F-472F-4FAE-8B0C-C9C1E4467705</GroupId>"));

			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Verify(cancelToken);
			regKeyProvider.Verify(x => x.SetKey(response.Key));

			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.OK, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("registry is updated", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_LocallyUnregistered()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);

			var reg = new ProductRegister();
			var regKeyProvider = new Mock<IRegistrationKeyProvider>();
			reg.RegKeyProvider = regKeyProvider.Object;
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(new RegistrationKeyXmlPair(new RegistrationKey(), ""));

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			var actualResult = reg.Verify(cancelToken);

			AssertEquals(ProductRegistrationVerifyResult.Unregistered, actualResult);
			AssertEquals("failure registry not changed", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_LocallyRegisteredRemotelyUnregistered()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyRequest actualRequest = null;
			VerifyResponse response = new VerifyResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)RegisterStatus.Unregistered;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<VerifyRequest>((VerifyRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Verify(cancelToken);

			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(ProductRegistrationVerifyResult.Unregistered, actualResult);
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals("fail set", (int)ProductRegistrationVerifyResult.Unregistered, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_DbUniqueKeyNotMatchedLocally()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			var reg = new ProductRegister();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = null;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("AnotherServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			CancellationToken cancelToken = new CancellationTokenSource().Token;

			var actualResult = reg.Verify(cancelToken);

			AssertEquals(ProductRegistrationVerifyResult.Fail, actualResult);
			AssertEquals("remote failure registry not set", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_Status()
		{
			AssertVerify(ProductRegistrationVerifyResult.OK, RegisterStatus.Success);
			AssertVerify(ProductRegistrationVerifyResult.OK, RegisterStatus.UniqueKeyUpdated);
			AssertVerify(ProductRegistrationVerifyResult.Fail, RegisterStatus.UniqueKeyNotMatched);
			AssertVerify(ProductRegistrationVerifyResult.Error, RegisterStatus.InternalError);
			AssertVerify(ProductRegistrationVerifyResult.Error, RegisterStatus.RequestInvalid);
			AssertVerify(ProductRegistrationVerifyResult.NotFound, RegisterStatus.ProductKeyNotFound);
		}

		void AssertVerify(ProductRegistrationVerifyResult expectedResult, RegisterStatus verifyStatus)
		{
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			VerifyResponse response = new VerifyResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)verifyStatus;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000)
				))
				.Returns(response);

			AssertEquals(expectedResult, reg.Verify(cancelToken));
		}

		public void TestVerify_ServiceError()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;
			var outputStatusCode = HttpStatusCode.InternalServerError;

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000)
				))
				.Returns<IEnumerable<VerifyRequest>>(null);

			AssertEquals(ProductRegistrationVerifyResult.Error, reg.Verify(cancelToken));
			AssertEquals("failure registry not changed", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestVerify_ServiceTimeout()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);

			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;
			var outputStatusCode = HttpStatusCode.RequestTimeout;
			CancellationToken cancelToken = new CancellationTokenSource().Token;
			mockClient.Setup(x =>
				x.Verify
				(
					It.IsAny<VerifyRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000)
				))
				.Returns<IEnumerable<VerifyRequest>>(null);

			AssertEquals(ProductRegistrationVerifyResult.Timeout, reg.Verify(cancelToken));
			AssertEquals("failure registry not changed", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);
		}

		public void TestUnregister()
		{
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			UnregisterRequest actualRequest = null;
			UnregisterResponse response = new UnregisterResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)RegisterStatus.Success;
			mockClient.Setup(x =>
				x.Unregister
				(
					It.IsAny<UnregisterRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>())
				)
				.Returns(response)
				.Callback(new SubmitCallback<UnregisterRequest>((UnregisterRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Unregister(cancelToken);
			Mock.Get(reg.RegKeyProvider).Verify(x => x.SetKey(""));
			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals(ProductRegistrationUnregisterResult.OK, actualResult);
		}

		public void TestUnregister_NotRegistered()
		{
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;

			CancellationToken cancelToken = new CancellationTokenSource().Token;
			UnregisterRequest actualRequest = null;
			UnregisterResponse response = new UnregisterResponse();
			var outputStatusCode = HttpStatusCode.OK;
			response.Status = (int)RegisterStatus.ProductKeyUnavailable;
			mockClient.Setup(x =>
				x.Unregister
				(
					It.IsAny<UnregisterRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.IsAny<int>()
				))
				.Returns(response)
				.Callback(new SubmitCallback<UnregisterRequest>((UnregisterRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			var actualResult = reg.Unregister(cancelToken);

			AssertEquals("{my reg key XML}", actualRequest.Key);
			AssertEquals(ProductRegistrationUnregisterResult.NotRegistered, actualResult);
		}

		public void TestUnregister_ServiceTimeout()
		{
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;
			var outputStatusCode = HttpStatusCode.RequestTimeout;
			CancellationToken cancelToken = new CancellationTokenSource().Token;
			mockClient.Setup(x =>
				x.Unregister
				(
					It.IsAny<UnregisterRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000))
				)
				.Returns<IEnumerable<UnregisterRequest>>(null);

			AssertEquals(ProductRegistrationUnregisterResult.Timeout, reg.Unregister(cancelToken));
		}

		public void TestUnregister_Error()
		{
			var reg = new ProductRegister();
			GenerateStubKeys(reg);

			var mockClient = new Mock<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;
			var outputStatusCode = HttpStatusCode.InternalServerError;
			CancellationToken cancelToken = new CancellationTokenSource().Token;
			mockClient.Setup(x =>
				x.Unregister
				(
					It.IsAny<UnregisterRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000))
				)
				.Returns<IEnumerable<UnregisterRequest>>(null);

			AssertEquals(ProductRegistrationUnregisterResult.Error, reg.Unregister(cancelToken));
		}

		public void TestRegister()
		{
			TestHelper.KeyForTest = null;

			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.Fail);
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZXY");
			DataRegistry.Instance.PhysicalServerID = "STU";
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "{some old key}");

			var reg = new ProductRegister();
			CancellationToken cancelToken = new CancellationTokenSource().Token;
			RegisterRequest actualRequest = null;
			var response = new RegisterResponse();
			response.Key = "{key XML}";
			response.Status = (int)RegisterStatus.Success;
			var mockClient = new Mock<IRegistrationServiceClient>();
			var outputStatusCode = HttpStatusCode.OK;
			reg.Client = mockClient.Object;
			GenerateStubKeys(reg);

			mockClient.Setup(x =>
				x.Register
				(
					It.IsAny<RegisterRequest>(),
					out outputStatusCode,
					It.Is<CancellationToken>(c => c.Equals(cancelToken)),
					It.Is<int>(i => i == 20000))
				)
				.Returns(response)
				.Callback(new SubmitCallback<RegisterRequest>((RegisterRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			AssertEquals(ProductRegistrationRegisterResult.OK, reg.Register("ABCDEF", cancelToken));

			AssertEquals("ABCDEF", actualRequest.ProductKey);
			AssertEquals("MyServer", actualRequest.UniqueKey.ServerName);
			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(new DateTime(2014, 3, 3), actualRequest.UniqueKey.DatabaseCreated);
			AssertEquals("{key XML}", TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(RawDataRegistry.Instance.EncryptedRegistrationKey.Value));
			AssertEquals("failure registry is cleared", (int)ProductRegistrationVerifyResult.OK, RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value);

			AssertEquals("SystemEnterpriseCode registry updated", "ENT", RawDataRegistry.Instance.SystemEnterpriseCode.Value);
			AssertEquals("PhysicalServerID registry updated", "SYD", DataRegistry.Instance.PhysicalServerID);
			AssertEquals("LegacyEncryptedSystemRegistrationKey registry updated", "", RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.Value);
			AssertEquals($"ENT{EnvProxy.Instance.CurrentCompany.Code}SYD", EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier);
		}

		public void TestTryAutoRegisterAfterUpgrade()
		{
			// Install a valid legacy registration key
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(
					GetLegacyXMLKey(Today.AddDays(30))));

			var reg = new ProductRegister();
			RegisterRequest actualRequest = null;
			var response = new RegisterResponse();
			response.Key = "{key XML}";
			response.Status = (int)RegisterStatus.Success;

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";
			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			reg.Client = mockClient.Object;

			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));

			var testKeyPair = new RegistrationKeyXmlPair(new RegistrationKey(), "");
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.RegKeyProvider = regKeyProvider.Object;
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);
			var outputStatusCode = HttpStatusCode.OK;

			mockClient.Setup(x =>
				x.Register
				(
					It.IsAny<RegisterRequest>(),
					out outputStatusCode,
					It.IsAny<CancellationToken>(),
					It.Is<int>(i => i == 20000))
				)
				.Returns(response)
				.Callback(new SubmitCallback<RegisterRequest>((RegisterRequest v, out HttpStatusCode code, CancellationToken c, int t) =>
				{
					code = HttpStatusCode.OK;
					actualRequest = v;
				}));

			AssertEquals(ProductRegistrationRegisterResult.OK, reg.TryAutoRegisterAfterUpgrade());

			AssertEquals("ABCDEF", actualRequest.ProductKey);
			AssertEquals("MyServer", actualRequest.UniqueKey.ServerName);
			AssertEquals("MyDb", actualRequest.UniqueKey.DatabaseName);
			AssertEquals(new DateTime(2014, 3, 3), actualRequest.UniqueKey.DatabaseCreated);
			AssertEquals("{key XML}", TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(RawDataRegistry.Instance.EncryptedRegistrationKey.Value));
		}

		public void TestTryAutoRegisterAfterUpgrade_LegacyKeyExpired()
		{
			// Install expired legacy registration key
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(GetLegacyXMLKey(Today.AddDays(-2))));

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";

			AssertEquals(ProductRegistrationRegisterResult.Fail, reg.TryAutoRegisterAfterUpgrade());
		}

		public void TestTryAutoRegisterAfterUpgrade_LegacyKeyBlank()
		{
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";

			AssertEquals(ProductRegistrationRegisterResult.Fail, reg.TryAutoRegisterAfterUpgrade());
		}

		public void TestTryAutoRegisterAfterUpgrade_LegacyKeyWithNoSystemId()
		{
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(GetLegacyXMLKey(Today.AddDays(30),
					AdminConnection.ServerSid, Db.DatabaseName, Db.Connection.ServerInstanceName, "")));

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";

			AssertEquals(ProductRegistrationRegisterResult.Fail, reg.TryAutoRegisterAfterUpgrade());
		}

		public void TestTryAutoRegisterAfterUpgrade_LegacyKeyMismatchSid()
		{
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(GetLegacyXMLKey(Today.AddDays(30),
					Guid.NewGuid(), Db.DatabaseName, Db.Connection.ServerInstanceName, "AAA")));

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";

			AssertEquals(ProductRegistrationRegisterResult.Fail, reg.TryAutoRegisterAfterUpgrade());
		}

		public void TestTryAutoRegisterAfterUpgrade_LegacyKeyMismatchDatabase()
		{
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(GetLegacyXMLKey(Today.AddDays(30),
					AdminConnection.ServerSid, "TheWrongTrousers", Db.Connection.ServerInstanceName, "AAA")));

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "DEF";

			AssertEquals(ProductRegistrationRegisterResult.Fail, reg.TryAutoRegisterAfterUpgrade());
		}

		public void TestTryAutoRegisterAfterUpgrade_UpdatesEnterpriseCode()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DDD");

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.TryAutoRegisterAfterUpgrade();

			AssertEquals(ProductRegistrationRegisterResult.OK, actualResult);
			AssertEquals(testRegKey.EnterpriseCode, RawDataRegistry.Instance.SystemEnterpriseCode.Value);

			AssertEquals("No need for exception report", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestTryAutoRegisterAfterUpgrade_UpdatesServerCode()
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)ProductRegistrationVerifyResult.OK);
			DataRegistry.Instance.PhysicalServerID = "DDD";

			var reg = new ProductRegister();
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");

			var repository = new MockRepository(MockBehavior.Default);
			var mockClient = repository.Create<IRegistrationServiceClient>();
			var dbUniqueKeyProvider = repository.Create<IDatabaseUniqueKeyProvider>();
			var regKeyProvider = repository.Create<IRegistrationKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			reg.RegKeyProvider = regKeyProvider.Object;
			reg.Client = mockClient.Object;

			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);

			var actualResult = reg.TryAutoRegisterAfterUpgrade();

			AssertEquals(ProductRegistrationRegisterResult.OK, actualResult);
			AssertEquals(testRegKey.ServerCode, DataRegistry.Instance.PhysicalServerID);

			AssertEquals("No need for exception report", 0, ExceptionReporterTestListener.Instance.Count);
		}

		static DateTime Today
		{
			get { return DateTime.Today; }
		}

		public void TestTryAutoRegisterAfterUpgrade_InvalidCodes()
		{
			// Install a valid legacy registration key
			RawDataRegistry.Instance.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(GetLegacyXMLKey(Today.AddDays(30))));

			var reg = new ProductRegister();

			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			DataRegistry.Instance.PhysicalServerID = "";

			AssertEquals(ProductRegistrationRegisterResult.ProductKeyNotFound, reg.TryAutoRegisterAfterUpgrade());

			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			DataRegistry.Instance.PhysicalServerID = "DEF";
			AssertEquals(ProductRegistrationRegisterResult.ProductKeyNotFound, reg.TryAutoRegisterAfterUpgrade());
		}

		void GenerateStubKeys(ProductRegister reg)
		{
			var dbUniqueKeyProvider = new Mock<IDatabaseUniqueKeyProvider>();
			reg.DbUniqueKeyProvider = dbUniqueKeyProvider.Object;
			dbUniqueKeyProvider.Setup(x => x.UniqueKey).Returns(new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3)));

			GenerateStubRegistrationKey(reg);
		}

		void GenerateStubRegistrationKey(ProductRegister reg)
		{
			var testRegKey = CreateTestRegistrationKey();
			var testKeyPair = new RegistrationKeyXmlPair(testRegKey, "{my reg key XML}");
			var regKeyProvider = new Mock<IRegistrationKeyProvider>();
			reg.RegKeyProvider = regKeyProvider.Object;
			regKeyProvider.Setup(x => x.KeyXmlPair).Returns(testKeyPair);
		}

		static RegistrationKey CreateTestRegistrationKey()
		{
			var key = new RegistrationKey();
			key.DatabaseNumber = 123;
			key.DbUniqueKey = new DatabaseUniqueKey("MyServer", "MyDb", new DateTime(2014, 3, 3));
			key.EnterpriseCode = "ENT";
			key.ServerCode = "SYD";
			return key;
		}

		string GetLegacyXMLKey(DateTime expiryDate)
		{
			return GetLegacyXMLKey(expiryDate, AdminConnection.ServerSid, Db.DatabaseName, Db.Connection.ServerInstanceName, "AAA");
		}

		string GetLegacyXMLKey(DateTime expiryDate, Guid serverSid, string dbName, string dbInstance, string systemId)
		{
			return TestLegacyXMLKey
				.Replace("{ExpiryDate}", expiryDate.Ticks.ToString("d"))
				.Replace("{ExpiryDate}", expiryDate.Ticks.ToString("d"))
				.Replace("{ServerSID}", serverSid.ToString())
				.Replace("{DBName}", dbName)
				.Replace("{DBInstanceName}", dbInstance)
				.Replace("{SystemId}", systemId);
		}

		readonly string TestLegacyXMLKey = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>{ExpiryDate}</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>{ServerSID}</ServerSID>" + System.Environment.NewLine +
										"		<DBName>{DBName}</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>{DBInstanceName}</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<SystemId>{SystemId}</SystemId>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		protected override void SetUp()
		{
			ProductRegister.ForceValidRegistrationForTest = false;
		}

		protected override void TearDown()
		{
			ProductRegister.ForceValidRegistrationForTest = true;
		}
	}
}
