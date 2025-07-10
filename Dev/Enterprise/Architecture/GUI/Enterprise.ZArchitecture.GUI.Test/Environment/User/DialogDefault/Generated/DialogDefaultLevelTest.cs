using System.Linq;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Core.DialogDefault.Testing
{
	sealed class DialogDefaultLevelTest : TestCase
	{
		ISecurityCheckpoint CanCreateAndModifyGlobalDialogDefaults
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}

		public void TestLevelsForCurrentUser_NoPermissions()
		{
			CanCreateAndModifyGlobalDialogDefaults.IsAllowed = false;

			var codes = DialogDefaultLevel.DialogDefaultLevelsForCurrentUser.Cast<CodeDescriptionPair>().Select(pair => pair.Code).ToArray();

			AssertCollectionNotContains(DialogDefaultLevel.Codes.Company, codes);
			AssertCollectionNotContains(DialogDefaultLevel.Codes.Global, codes);
			AssertCollectionContains(DialogDefaultLevel.Codes.User, codes);
		}

		public void TestLevelsForCurrentUser_WithPermission()
		{
			CanCreateAndModifyGlobalDialogDefaults.IsAllowed = true;

			var codes = DialogDefaultLevel.DialogDefaultLevelsForCurrentUser;
			var allCodes = new DialogDefaultLevel();

			AssertEquals(allCodes.Count, codes.Count);
			AssertContainsExactElementsInAnyOrder(allCodes, codes);
		}
	}
}
