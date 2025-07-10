using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Core.DialogDefault.Testing
{
	[TestedType(typeof(DialogDefaultSaveOptions))]
	class DialogDefaultSaveOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCloning()
		{
			var original = new DialogDefaultSaveOptions
			{
				Level = DialogDefaultLevel.Codes.User,
				KeepShowingDialog = true,
				SaveNewDefaults = true,
				SaveForAllContexts = true,
			};

			var clone = original.Clone();

			CombineAssertions(() =>
			{
				//Order matters when cloning an object. It appears if level is not set before items that depend on it then all hell breaks loose

				AssertEquals("Level", original.Level, clone.Level);
				AssertEquals("SaveNewDefaults", original.SaveNewDefaults, clone.SaveNewDefaults);
				AssertEquals("SaveForAllContexts", original.SaveForAllContexts, clone.SaveForAllContexts);
				AssertEquals("SaveForMeAsWell", original.SaveForMeAsWell, clone.SaveForMeAsWell);
				AssertEquals("KeepShowingDialog", original.KeepShowingDialog, clone.KeepShowingDialog);
			});
		}

		public void TestCloning_UsesFields()
		{
			var options = new DialogDefaultSaveOptions();
			options.Level = DialogDefaultLevel.Codes.Global;
			options.SaveForMeAsWell = false;
			options.KeepShowingDialog = false;

			Assert("PRE: Since we are saving globally KeepShowingDialog must be true", options.KeepShowingDialog);

			var clone = options.Clone();
			clone.Level = DialogDefaultLevel.Codes.User;

			Assert("Should have the previously set value, since now we are not being blocked by being a GLB level", !clone.KeepShowingDialog);
		}

		public void TestUserGivenCorrectSaveOptions()
		{
			var dialogOptions = new DialogDefaultSaveOptions();
			var securityCheck = EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults");

			securityCheck.IsAllowed = false;
			var levels = dialogOptions.LevelList;

			//Should be 1 because user only has privilege to save for his/her self
			AssertEquals(1, levels.Count);
			Assert("User should be able to save for self", levels.ContainsCode(DialogDefaultLevel.Codes.User));

			securityCheck.IsAllowed = true;
			levels = dialogOptions.LevelList;

			//3 because user can now save for self, company and global
			AssertEquals(3, levels.Count);

			Assert("User should still be able to save for self", levels.ContainsCode(DialogDefaultLevel.Codes.User));
			Assert("User should be able to save for company", levels.ContainsCode(DialogDefaultLevel.Codes.Company));
			Assert("User should be able to save globally", levels.ContainsCode(DialogDefaultLevel.Codes.Global));
		}
	}
}
