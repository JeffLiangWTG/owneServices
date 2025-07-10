using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class FTPSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAllShouldBeEnteredWhenNotReadOnly()
		{
			var fTPSettings = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);

			fTPSettings.ReadOnly = true;
			AssertEntered(AssertNoError);

			fTPSettings.ReadOnly = false;
			AssertEntered(AssertHasError);

			fTPSettings.UserName = "TestUser";
			fTPSettings.Password = "TestPassword";
			fTPSettings.InFolder = "TestPath";
			fTPSettings.OutFolder = "TestPath";
			fTPSettings.Server = "localhost";
			AssertEntered(AssertNoError);

			void AssertEntered(Action<ZPropertyInfo, string> action)
			{
				fTPSettings.Validation.ValidateAll();
				AssertWithPropertyInfo(fTPSettings.InFolderInfo);
				AssertWithPropertyInfo(fTPSettings.OutFolderInfo);
				AssertWithPropertyInfo(fTPSettings.UserNameInfo);
				AssertWithPropertyInfo(fTPSettings.PasswordInfo);
				AssertWithPropertyInfo(fTPSettings.ServerInfo);

				void AssertWithPropertyInfo(ZPropertyInfo targetInfo)
				{
					action(targetInfo, string.Format("You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
				}
			}
		}
	}
}
