using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItem))]
	sealed class FTPSettingsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FTPSettings>
	{
		protected override StronglyTypedRegistryItem<FTPSettings, FTPSettings> GetNewRegistryItem()
		{
			return new FTPSettingsRegistryItem("", null, null, null, RegistryStorageFlags.BranchDepartment, RegistryOptions.IsOnlyForSupport);
		}

		protected override FTPSettings ValidValue
		{
			get
			{
				var result = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.UserName = "TestUser";
				result.Password = "TestPassword";
				result.InFolder = "TestPath";
				result.OutFolder = "TestPath";
				result.Server = "localhost";
				result.Port = 8888;
				result.Passive = false;

				return result;
			}
		}
	}
}
