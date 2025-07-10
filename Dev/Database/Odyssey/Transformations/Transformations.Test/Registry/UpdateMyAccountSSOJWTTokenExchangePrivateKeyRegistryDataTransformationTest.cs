using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(UpdateMyAccountSSOJWTTokenExchangePrivateKeyRegistryDataTransformation))]
	public class UpdateMyAccountSSOJWTTokenExchangePrivateKeyUnicodeRegistryDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateMyAccountSSOJWTTokenExchangePrivateKeyRegistryDataTransformation();
		}

		protected override void PrepareTestData()
		{
			var sql = @"INSERT dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled) VALUES
	(NEWID(), 'MyAccountSSOJWTTokenExchangePrivateKey', 'STR', 1, @utf16Value,
	'00000000-0000-0000-0000-000000000000', 0);";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@utf16Value", Encoding.Unicode.GetBytes(privateKey), StmDataSchema.SD_BinaryValue);
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			var data = Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM StmData WHERE SD_Name = 'MyAccountSSOJWTTokenExchangePrivateKey'");
			AssertEquals(privateKey, Encoding.UTF8.GetString((byte[])data));
		}

		[ExpectNoExceptions]
		public void TestRunAndAssertResultsWhenPrivateKeyIsNull()
		{
			var sql = @"INSERT dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled) VALUES
				(NEWID(), 'MyAccountSSOJWTTokenExchangePrivateKey', 'STR', 1, NULL,
				'00000000-0000-0000-0000-000000000000', 0);";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			RunTransformation();
			var data = Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM StmData WHERE SD_Name = 'MyAccountSSOJWTTokenExchangePrivateKey'");
			Assert(Convert.IsDBNull(data));
		}

		const string privateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAzC3PmqAVO8x+AcloQ5O9RlA+pjAdUxERZTDt/qgPI64uTPXG
PADAnD8+fLHZvrV0zV8O6LWJDuayYvaCxTG4B8gFfT0X6X8DfxJIGOwS2MitcKWU
B3umrk7qgTOCcAWAzWGhBUta5CZTgEGXi3hawX+pRTY+pzBP9ZMqRLdMiZUVWEQr
kWpLhu15CWAYg1JWazuSPXbvX990JYGRoXej4SxRQe13ArcyPs2zmYbZ9xreCpdx
Y/0oHSna51EfxC57Zn0LET5GNAra9K83F3cg+6yiJxwLGm4sanlKSp5Aqd8vQoWU
HbZLDvA4tMRFeg0m77xjSzkTnDEGIyQpoqZ0jQIDAQABAoIBAQCHfF6mXFO6updi
2CM3tHGElvr7jDHpTQod+7nxodNp+cr/hpdkeZtWEyGD3QCAbCh1nv5lrRClsq/s
u2dLMxLLFw+Na1zStFW9nIP7Bav77i4o8baowIR6ZiN2WJfVfdFad85BlR9bBZOj
J+NHyTVv8SaBpt0sVAK7EkyaDIfdQsDSP2qJJVJS0/Zwm55Y/ptw/YxBHOFy6HX9
iipT1+LQa0d2dYDKxLbqpnZQzc4YCrQT5rPP44M5a79vMQMAKMVCPu6lzAVThEfx
fiYzuyvVz4JVJ1tSIf1WLf/F3ypC2K/2MhaRSzM/Nmy1Ib5tpdXwwf40+Z8Mwvbp
QslzksgBAoGBAOgwbW/jnwTmNKHbovc7aS1TzTMV9RyE7Uajx7JOaztZOso2F+kQ
kuICw0OmtQXmWHwXLJ10TXyYuRk3CUum9HlhB4SpAeieLDAKgTXba44V7qtN7F0N
8G3YCIGjKU92vrB4SqcoLeSTlj5R2HPAxSz4oedQ0xLNWjtO14YirlU9AoGBAOEe
Cr5EBeMGN/A0mjFcFa9Pa1+6MlCYlIDrx5PMWSGnUaVdOsNBCb4nhgE8cxU1WKIx
zLyiIJTuN+jmb3fl9rn/aA0RDNRzt7NzqB60ODiRAFFtwZKVpKWQfXbIbbBjJRgJ
43HuXTxpxpl+huySEwWzJeMSXrohVbwQbX5ybbGRAoGAYuGs2Yewgx+eroehAXUV
t64Gp4jkV/7sJbc+JltrI10+wjsDN8hNJV9T1Q277gVJDZ+46l1LWpKX0Xs0xDkX
yFFgKEjpfS1PWC5BFLSbO2lvuRh4XrC/AaiNBth7kVHap8Cy2jksQjnwNB4a9kDU
N/Cy0pYDLfCySquq8X73i2kCgYAVKM25tIsZG6yGV2tm2FDxeXWOOeIg0TakJ4VK
zxpRn3h9IpYzZBmWVgCyfQwUIj+Cf0vPLy4A0aNPsNkpW+Qk92zATan3DilmJKjY
uffO2VI+VSKstIQVS89/KrekrKz/5W4Ld2wsEYUpSEtGUTSYhI47Ga7tr9RvKNwh
1n+ZAQKBgQDdHtmBMpwMcNZwdCP4WfRORPuGjncWoRdGjCgazUmLL+zsBAbsO6VL
7wHq1Y/DGTLwg2thSdxJpCEr5hCq5kyyV+BcTXQJ0N/P/LD/ZHL6AVaSOnxjcd5c
Dx7PPxXUzdGioltaSQyawR5qIaj5E17PxJQesFpXEpmU1g2EnAVP3g==
-----END RSA PRIVATE KEY-----";
	}
}
