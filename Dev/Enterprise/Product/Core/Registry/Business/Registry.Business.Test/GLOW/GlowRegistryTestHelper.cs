using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Registry.Business.Testing
{
	public static class GlowRegistryTestHelper
	{
		const string GlowRestrictedModulesOverrideRegistryName = "GlowRestrictedModulesOverride";

		public static void SetRestrictedModulesOverride(params string[] moduleNames)
		{
			Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name=@name", p => p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name));

			if (moduleNames == null || moduleNames.Length == 0)
			{
				return;
			}

			// GLOW doesn't use compression here, we need UTF-8 stored as binary blob
			var serialisedValue = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfstring xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{string.Concat(moduleNames.Select(x => $"<string>{x}</string>"))}</ArrayOfstring>";

			Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmData (SD_Name, SD_BinaryValue, SD_PK) VALUES (@name, CONVERT(varbinary(max), @value), NEWID())", p =>
			{
				p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name);
				p.AddParameter("@value", System.Data.SqlDbType.VarChar, serialisedValue);
			});
		}

		public static IDisposable SetFeatureFlagNeo()
		{
			var featureDataMock = new Mock<IFeatureData>();

			var featureControlManagerMock = new Mock<IFeatureControlManager>();
			featureControlManagerMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.NeoFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			return ObjectFactory.Substitute(featureControlManagerMock.Object);
		}
	}
}
