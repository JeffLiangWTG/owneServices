using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccPayableOrderModule))]
	public class AccPayableOrderModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccPayableOrder;
		}

		public void TestLicenseCheckpoints()
		{
			using (AccPayableOrderModule module = new AccPayableOrderModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (AccPayableOrderModuleForTest module = new AccPayableOrderModuleForTest())
			{
				AssertEquals(Env.Security.PayableOrder, module.SecurityCheckpoint);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (AccPayableOrderModuleForTest module = new AccPayableOrderModuleForTest())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (AccPayableOrderModuleForTest module = new AccPayableOrderModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccPayableOrderFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccPayableOrderModuleForTest module = new AccPayableOrderModuleForTest())
			{
				IBusinessObjectCollection collectionBatchsCollection = module.NewGridCollection;
				Assert("Invalid type", collectionBatchsCollection is AccPayableOrderHeaderCollection);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (AccPayableOrderModuleForTest module = new AccPayableOrderModuleForTest())
			{
				var menuItem = module.FormActionMenu.FindByText("Deactivate") as ZMenuItem;
				AssertNotNull(menuItem);
				AssertNotNull("Deactivates the selected item after viewing its details read-only (shortcut Del)", menuItem.CaptionResourceString.FullDescription);
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		class AccPayableOrderModuleForTest : AccPayableOrderModule
		{
			public AccPayableOrderModuleForTest()
			{ }

			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get
				{
					return GetNewFilterBusinessObject();
				}
			}
		}
	}
}
