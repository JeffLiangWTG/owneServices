using System.Data;
using System.Data.SqlClient;
using CargoWise.Data.HttpClient;
using CargoWise.Data.SqlProxy.Interface.Converters;
using CargoWise.Data.SqlProxy.Interface.Models;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Resource.Version;
using Microsoft.SqlServer.Types;
using Moq;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Test.EndToEndTest;

class SqlParameterDTOTest
{
	[TestCaseSource(nameof(SqlParameterTestData))]
	public void SerializationAndDeserialization(SqlParameter sqlParameter)
	{
		var dto = SqlParameterDTO.FromSqlParameter(sqlParameter);
		var request = new SqlProxyRequest(MockOfDatabaseConnection)
		{
			SqlStatement = "sql",
			CommandType = CommandType.Text,
			Parameters = [dto],
		};

		// Act
		var json = JsonConvert.SerializeObject(request);
		var deserialized = JsonConvert.DeserializeObject<SqlProxyRequest>(json);

		Assert.That(deserialized, Is.Not.Null);
		Assert.That(deserialized!.Parameters, Is.Not.Null);

		var resultParameter = deserialized!.Parameters![0];
		Assert.That(resultParameter, Is.Not.Null);

		// Assert
		var resultSqlParameter = resultParameter.ToSqlParameter();
		Assert.That(resultSqlParameter, Is.Not.Null);
		Assert.That(JsonConvert.SerializeObject(resultSqlParameter), Is.EqualTo(JsonConvert.SerializeObject(sqlParameter)));
	}

	[OneTimeSetUp]
	public void OneTimeSetup()
	{
		HttpLoaderFactory.SetGlowLoaderServerListeningPort("123");
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

	static IEnumerable<SqlParameter> SqlParameterTestData()
	{
		yield return new SqlParameter("@bigInt", SqlDbType.BigInt) { Value = long.MaxValue };
		yield return new SqlParameter("@binary", SqlDbType.Binary, 16) { Value = new byte[] { 0x01, 0x02, 0x03, 0x04 } };
		yield return new SqlParameter("@bit", SqlDbType.Bit) { Value = true };
		yield return new SqlParameter("@char", SqlDbType.Char, 10) { Value = "CharTest" };
		yield return new SqlParameter("@date", SqlDbType.Date) { Value = DateTime.UtcNow.Date };
		yield return new SqlParameter("@dateTime", SqlDbType.DateTime) { Value = DateTime.UtcNow };
		yield return new SqlParameter("@dateTime2", SqlDbType.DateTime2) { Value = DateTime.UtcNow };
		yield return new SqlParameter("@dateTimeOffset", SqlDbType.DateTimeOffset) { Value = DateTimeOffset.UtcNow };
		yield return new SqlParameter("@decimal", SqlDbType.Decimal) { Value = 12345.6789m };
		yield return new SqlParameter("@float", SqlDbType.Float) { Value = 12345.6789 };
		yield return new SqlParameter("@image", SqlDbType.Image) { Value = new byte[] { 0x01, 0x02, 0x03 } };
		yield return new SqlParameter("@int", SqlDbType.Int) { Value = int.MaxValue };
		yield return new SqlParameter("@money", SqlDbType.Money) { Value = 12345.67m };
		yield return new SqlParameter("@nChar", SqlDbType.NChar, 10) { Value = "NCharTest" };
		yield return new SqlParameter("@nText", SqlDbType.NText) { Value = "This is a test for NText" };
		yield return new SqlParameter("@nVarChar", SqlDbType.NVarChar, 128) { Value = "This is a test for NVarChar" };
		yield return new SqlParameter("@real", SqlDbType.Real) { Value = 12345.67f };
		yield return new SqlParameter("@smallDateTime", SqlDbType.SmallDateTime) { Value = DateTime.UtcNow };
		yield return new SqlParameter("@smallInt", SqlDbType.SmallInt) { Value = short.MaxValue };
		yield return new SqlParameter("@smallMoney", SqlDbType.SmallMoney) { Value = 123.45m };
		yield return SqlParameterHelper.CreateTableValuedParameter(
			"@structured",
			"dbo.TVP_Sample",
			["Value1", "Value2"]
		);
		yield return SqlParameterHelper.CreateTableValuedParameter("@guids", TVPHelper.TVP_uniqueidentifier, [$"{Guid.NewGuid()}"]);
		yield return new SqlParameter("@text", SqlDbType.Text) { Value = "This is a test for Text" };
		yield return new SqlParameter("@time", SqlDbType.Time) { Value = TimeSpan.FromHours(12) };
		yield return new SqlParameter("@timestamp", SqlDbType.Timestamp) { Value = new byte[] { 0x01, 0x02, 0x03, 0x04 } };
		yield return new SqlParameter("@tinyInt", SqlDbType.TinyInt) { Value = byte.MaxValue };
		yield return new SqlParameter("@uniqueIdentifier", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() };
		yield return new SqlParameter("@varBinary", SqlDbType.VarBinary, 256) { Value = new byte[] { 0x01, 0x02, 0x03 } };
		yield return new SqlParameter("@varChar", SqlDbType.VarChar, 128) { Value = "This is a test for VarChar" };
		yield return new SqlParameter("@variant", SqlDbType.Variant) { Value = 12345 };
		yield return new SqlParameter("@xml", SqlDbType.Xml) { Value = "<root><element>Test</element></root>" };
		yield return new SqlParameter("@LockTimeout", SqlDbType.Int) { Value = 0.123D };
		yield return new SqlParameter
		{
			ParameterName = "@Geography",
			SqlDbType = SqlDbType.Udt,
			UdtTypeName = "GEOGRAPHY",
			Value = SqlGeography.Point(47.6, -122.3, 4326)
		};
		yield return new SqlParameter
		{
			ParameterName = "@Geometry",
			SqlDbType = SqlDbType.Udt,
			UdtTypeName = "GEOMETRY",
			Value = SqlGeometry.Parse("POINT(1 1)")
		};
		yield return new SqlParameter
		{
			ParameterName = "@HierarchyId",
			SqlDbType = SqlDbType.Udt,
			UdtTypeName = "HIERARCHYID",
			Value = SqlHierarchyId.Parse("/1/2/3/")
		};
	}
}
