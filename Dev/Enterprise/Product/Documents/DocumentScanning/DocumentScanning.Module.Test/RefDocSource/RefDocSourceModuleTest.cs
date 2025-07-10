using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(RefDocSourceModule))]
	internal sealed class RefDocSourceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefDocSource;
		}

		public void TestCheckpoints()
		{
			using (RefDocSourceModule module = new RefDocSourceModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.DocumentSources, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.DocManager, module.LicenceCheckPoint);
			}
		}

		public void TestBusinessContexts()
		{
			using (RefDocSourceModuleForTest module = new RefDocSourceModuleForTest())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
				AssertEquals("The 'RefDocSource' business context should be returned", BusinessContext.RefDocSource, module.BusinessContexts[0]);
			}
		}

		#region Properties

		public void TestGetNewActionMenuItems()
		{
			using (RefDocSourceModuleForTest module = new RefDocSourceModuleForTest())
			{
				MenuItem[] menus = module.GetNewActionMenuItems();
				foreach (MenuItem item in menus)
				{
					Assert(!item.Text.EndsWith("Delete"));
				}
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefDocSourceModuleForTest module = new RefDocSourceModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefDocSourceFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefDocSourceModuleForTest module = new RefDocSourceModuleForTest())
			{
				IBusinessObjectCollection docsCollection = module.NewGridCollection;
				Assert("Invalid type", docsCollection is RefDocSourceCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefDocSourceModuleForTest module = new RefDocSourceModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefDocSourceFilterBusinessObject);
			}
		}

		#endregion
	}
}
