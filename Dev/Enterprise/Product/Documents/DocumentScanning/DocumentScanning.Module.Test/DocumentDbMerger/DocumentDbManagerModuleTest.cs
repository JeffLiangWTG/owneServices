using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(DocumentDbManagerModule))]
	internal sealed class DocumentDbManagerModuleTest : ZPopupModuleBasherTest
	{
		[UseSnapshotProtection]
		public void TestIsAllowedToShow()
		{
			using (DocumentDbManagerModuleForTesting module = new DocumentDbManagerModuleForTesting())
			{
				using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
				{
					Assert("Module supposed to be shown", module.IsAllowedToShow_Exposed());
				}

				using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
				{
					Assert("Module not supposed to be shown", !module.IsAllowedToShow_Exposed());
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocumentDbManager;
		}
	}
}
