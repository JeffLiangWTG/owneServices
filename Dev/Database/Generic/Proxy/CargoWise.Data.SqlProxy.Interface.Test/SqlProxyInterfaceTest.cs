using System.Net;
using System.Net.Http;
using CargoWise.Data.SqlProxy.Interface.Models;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Test;

public class SqlProxyInterfaceTest
{
	[Test]
	public async Task ExecuteScalarAsync()
	{
		// Arrange
		var expectedResult = new ExecuteScalarResult(type: "int", value: "42");
		var httpClient = new System.Net.Http.HttpClient(MockOfHttpMessageHandler(expectedResult));
		var client = new SqlProxyClient(httpClient);
		var request = new SqlProxyRequest(MockOfDatabaseConnection);

		// Act
		var result = await client.ExecuteScalarAsync(request);

		// Assert
		Assert.That(result.Type, Is.EqualTo(expectedResult.Type));
		Assert.That(result.Value, Is.EqualTo(expectedResult.Value));
	}

	[Test]
	public async Task ExecuteNonQueryAsync()
	{
		// Arrange
		var expectedResult = new ExecuteNonQueryResult(1);
		var httpClient = new System.Net.Http.HttpClient(MockOfHttpMessageHandler(expectedResult));
		var client = new SqlProxyClient(httpClient);
		var request = new SqlProxyRequest(MockOfDatabaseConnection);

		// Act
		var result = await client.ExecuteNonQueryAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.RowsAffected, Is.EqualTo(expectedResult.RowsAffected));
	}

	[Test]
	public async Task BeginTransactionAsync()
	{
		// Arrange
		var expectedResult = new BeginTransactionResult(Guid.NewGuid());
		var httpClient = new System.Net.Http.HttpClient(MockOfHttpMessageHandler(expectedResult));
		var client = new SqlProxyClient(httpClient);
		var request = new SqlProxyRequest(MockOfDatabaseConnection);

		// Act
		var result = await client.BeginTransactionAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.TransactionId, Is.EqualTo(expectedResult.TransactionId));
	}

	[Test]
	public async Task RollbackTransactionAsync()
	{
		// Arrange
		var expectedResult = new VoidResult();
		var httpClient = new System.Net.Http.HttpClient(MockOfHttpMessageHandler(expectedResult));
		var client = new SqlProxyClient(httpClient);

		// Act
		var result = await client.RollbackTransactionAsync(Guid.NewGuid());

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.EqualTo(expectedResult));
	}

	[Test]
	public async Task CommitTransactionAsync()
	{
		// Arrange
		var expectedResult = new VoidResult();
		var httpClient = new System.Net.Http.HttpClient(MockOfHttpMessageHandler(expectedResult));
		var client = new SqlProxyClient(httpClient);

		// Act
		var result = await client.CommitTransactionAsync(Guid.NewGuid());

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.EqualTo(expectedResult));
	}

	[SetUp]
	public void SetUp()
	{
		var versionMock = Mock.Of<IDatabaseAspectVersions>(x => x.SchemaVersion == new VersionLabel(1, 0));
		var serviceProviderMock = new Mock<IServiceProvider>();
		serviceProviderMock.Setup(x => x.GetService(typeof(IDatabaseAspectVersions))).Returns(versionMock);

		globalServiceProviderDisposable = GlobalServiceProvider.Configure(serviceProviderMock.Object);
	}

	[TearDown]
	public void TearDown()
	{
		globalServiceProviderDisposable?.Dispose();
	}

	IDisposable? globalServiceProviderDisposable;

	static IDbConnectionInfo MockOfDatabaseConnection =>
		Mock.Of<IDbConnectionInfo>(x =>
			x.ServerName == "serverName"
			&& x.Database == "database"
			&& x.UserName == "userName"
			&& x.Password == "password"
			&& x.SuffixedApplicationName == ""
			&& x.ConnectTimeout == 30);

	static HttpMessageHandler MockOfHttpMessageHandler<T>(T expectedResult, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var responseContent = new StringContent(JsonConvert.SerializeObject(new SqlProxyResponse<T>(expectedResult)));
		var httpResponseMessage = new HttpResponseMessage(statusCode) { Content = responseContent };
		var handlerMock = new Mock<HttpMessageHandler>();

		handlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(httpResponseMessage);

		return handlerMock.Object;
	}
}
