using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(DocumentDbMergerModule))]
	internal sealed class DocumentDbMergerModuleTest : ZPopupModuleBasherTest
	{
		[UseSnapshotProtection]
		public void TestCheckIsAllowedToShow()
		{
			using (DocumentDbMergerModuleForTesting module = new DocumentDbMergerModuleForTesting())
			{
				module.sqlServerEditionOverride = DbConnection.SqlServerEdition.EnterpriseDeveloper;

				using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily("sysadmin"))
				{
					Assert("Module not supposed to be shown", !module.CheckIsAllowedToOpen_Exposed());
				}

				using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
				{
					Assert("Module not supposed to be shown", !module.CheckIsAllowedToOpen_Exposed());
				}

				BusinessObjectFactory factory = new BusinessObjectFactory();
				var controller = factory.New<GlbStaff>();
				controller.GS_LoginName = "AnotherNewStaff";
				controller.GS_FullName = "Another New Staff";
				controller.GS_IsController = true;
				factory.Save();
				using (Enterprise.ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(controller.GS_LoginName))
				{
					Assert("Module supposed to be shown", module.CheckIsAllowedToOpen_Exposed());
				}

				module.sqlServerEditionOverride = DbConnection.SqlServerEdition.Express;
				Assert("Module not supposed to be shown", !module.CheckIsAllowedToOpen_Exposed());

				module.sqlServerEditionOverride = DbConnection.SqlServerEdition.EnterpriseDeveloper;
				Assert("Module supposed to be shown", module.CheckIsAllowedToOpen_Exposed());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocumentDbMerger;
		}
	}
}
