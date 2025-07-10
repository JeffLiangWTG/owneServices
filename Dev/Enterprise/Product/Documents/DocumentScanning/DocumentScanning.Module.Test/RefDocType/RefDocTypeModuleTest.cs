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
	[TestedType(typeof(RefDocTypeModule))]
	internal sealed class RefDocTypeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefDocType;
		}

		public void TestCheckpoints()
		{
			using (RefDocTypeModule module = new RefDocTypeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.DocumentTypes, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.DocManager, module.LicenceCheckPoint);
			}
		}

		public void TestBusinessContexts()
		{
			using (RefDocTypeModuleForTest module = new RefDocTypeModuleForTest())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
				AssertEquals("The 'RefDocType' business context should be returned", BusinessContext.RefDocType, module.BusinessContexts[0]);
			}
		}

		#region Properties

		public void TestGetNewActionMenuItems()
		{
			using (RefDocTypeModuleForTest module = new RefDocTypeModuleForTest())
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
			using (RefDocTypeModuleForTest module = new RefDocTypeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefDocTypeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefDocTypeModuleForTest module = new RefDocTypeModuleForTest())
			{
				IBusinessObjectCollection docsCollection = module.NewGridCollection;
				Assert("Invalid type", docsCollection is RefDocTypeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefDocTypeModuleForTest module = new RefDocTypeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefDocTypeFilterBusinessObject);
			}
		}

		#endregion
	}
}
