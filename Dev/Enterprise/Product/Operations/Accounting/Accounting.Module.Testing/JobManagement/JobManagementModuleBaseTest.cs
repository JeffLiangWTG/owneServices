using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementModuleBase))]
	public class JobManagementModuleBaseTest : ZModuleBasherTest
	{
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			collection.Add(Factory.NewWithValidTestData<JobManagement>(TestBusinessObjectKind.MinimumRequiredToSave));
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobHeader;
		}

		public void TestGetNewStandardMenuItems()
		{
			using (JobManagementModuleBase module = new JobManagementModuleBase())
			{
				MenuItem[] items = module.GetNewStandardMenuItems_ForTestOnly();
				AssertNull("This Item must be deleted", items.FindByText("&New"));
				AssertNull("This Item must be deleted", items.FindByText("&Edit"));
				AssertNull("This Item must be deleted", items.FindByText("&Delete"));
			}
		}

		public void TestShowNewFormDoesntThrowException()
		{
			using (JobManagementModuleBase module = new JobManagementModuleBase())
			{
				Assert("Should not allow new - pressing F3 will blow up if allownew = true", !module.AllowNew);
			}
		}
	}
}
