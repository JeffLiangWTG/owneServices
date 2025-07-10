using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityApplication.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Module.Testing
{
	[TestedType(typeof(EdiIdentityApplicationModule))]
	public class EdiIdentityApplicationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.EdiIdentityApplication;
		}

		public void TestAllowNew()
		{
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestAllowView()
		{
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowView);
			}
		}

		public void TestNewWithSecurityRight()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffNew = Factory.NewWithValidTestData<GlbStaff>();
			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffNew.PK;
			staffSecurity.GU_SecurityRight = EDISecurityCheckpoints.EdiIdentityApplicationNew.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staff.HomeBranch.PK.ToGuid(), Guid.Empty))
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				var exception = AssertExceptionThrown<SecurityAccessDeniedException>(() => newMenuItem.PerformClick());
				AssertEquals(EDISecurityCheckpoints.EdiIdentityApplicationNew.ErrorMessageForNotAllowed, exception.Message);
			}

			using (Env.SetTemporaryUserContext(staffNew.PK.ToGuid(), staffNew.HomeBranch.PK.ToGuid(), Guid.Empty))
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				AssertNoExceptionThrown(() => newMenuItem.PerformClick());
				var openForms = ZApplication.GetOpenForms().OfType<EdiIdentityApplicationForm>();
				AssertEquals(1, openForms.Count());
				openForms.First().ForceClose();
			}
		}

		public void TestNewActionMenuItems()
		{
			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull(module.ToolBarButtons.FindByText("New"));
				AssertEquals(2, module.NewMenuItem.MenuItems.Count);
				AssertEquals("New Azure Application", module.NewMenuItem.MenuItems[0].Text);
				AssertEquals("New Customer Application", module.NewMenuItem.MenuItems[1].Text);
			}
		}

		public void TestNewCustomerApplicationFormShouldBeEditableWhenItIsCreatedAfterModuleGridLoaded()
		{
			_ = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Factory.Save();

			using (var module = (EdiIdentityApplicationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("Load existing data to init the grid collection.",1, module.GridCollection.Count);
				AssertNotNull("Init the menuitems.", module.ToolBarButtons.FindByText("New"));

				var newCustomerApplicationMenuItem = module.NewMenuItem.MenuItems.FindByText("New Customer Application");
				newCustomerApplicationMenuItem.PerformClick();

				var openedForms = Application.OpenForms.OfType<EdiIdentityCustomerApplicationForm>();
				AssertEquals("Should open a new customer application form.",1, openedForms.Count());
				var application = openedForms.First().BusinessEntity as EdiIdentityApplication;
				AssertEquals("Form business entity should not be read only.", false, application.ReadOnly);
				openedForms.First().ForceClose();
			}
		}
	}
}
