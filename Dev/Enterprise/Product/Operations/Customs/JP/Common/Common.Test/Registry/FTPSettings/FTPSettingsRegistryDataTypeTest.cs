using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemRegistryDataType))]
	sealed class FTPSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FTPSettingsRegistryItemRegistryDataType>
	{
		protected override FTPSettingsRegistryItemRegistryDataType GetNewDataType() => new FTPSettingsRegistryItemRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			var item1 = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item1.UserName = "TestUser";
			item1.Password = "TestPassword";
			item1.InFolder = "TestPath";
			item1.OutFolder = "TestPath";
			item1.Server = "localhost";
			item1.Port = 8888;
			item1.Passive = false;

			var item2 = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item2.UserName = "TestUserName";
			item2.Password = "Testpassword";
			item2.InFolder = "TestPath";
			item2.OutFolder = "TestPath";
			item2.Server = "127.0.0.1";
			item2.Port = 21;
			item2.Passive = false;

			return new[] { new ValidSampleAndBinaryValueInDB(item1, new FTPSettingsRegistryItemRegistryDataType().Serialise(item1)), new ValidSampleAndBinaryValueInDB(item2, new FTPSettingsRegistryItemRegistryDataType().Serialise(item2)) };
		}

		protected override string ExpectedEditorName => "FTPSettingsRegistryItemEditor";
	}
}
