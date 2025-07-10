using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStorageRegisterModule))]
	class TempStorageRegisterModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertType<TempStorageRegisterController>(module.GetNewController());
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertType<TempStorageRegisterFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				using (var filterControl = module.GetNewFilterControlForGrid())
				{
					AssertType<TempStorageRegisterFilterStripControl>(filterControl);
				}
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				var collection = module.GridCollection;
				AssertType<CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>>("Type", collection);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertEquals(Env.Security.EUTempStorageRegister, module.SecurityCheckpoint);
			}
		}

		public void TestAllowNew()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				AssertEquals("User should not be able to create new register, the system does instead", false, module.AllowNew);
			}
		}

		public void TestImportDataWizardMenuItem()
		{
			using (var module = (TempStorageRegisterModule)GetModule())
			{
				var dataTransferActionsMenuItem = module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.OfType<ZMenuItem>().Single(i => i.Text == "D&ata Transfer");
				var menuItem = dataTransferActionsMenuItem.MenuItems.OfType<ZMenuItem>().Single(m => m.Text == "&Import By Data Wizard");
				AssertNotNull("Import By Data Wizard menu item should be present", menuItem);

				module.Dispose();
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_Reference = "Reference";
			header.SRH_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}
	}
}
