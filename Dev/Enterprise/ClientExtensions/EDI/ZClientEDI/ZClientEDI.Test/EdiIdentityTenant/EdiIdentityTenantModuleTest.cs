using System;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IdentityTenant.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Module.Testing
{
	[TestedType(typeof(EdiIdentityTenantModule))]
	internal class EdiIdentityTenantModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.EdiIdentityTenant;
		}

		public void TestAllowNew()
		{
			using var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID());
			AssertEquals(true, module.AllowNew);
		}

		public void TestAllowEdit()
		{
			using var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID());
			AssertEquals(true, module.AllowEdit);
		}

		public void TestAllowDelete()
		{
			using var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID());
			AssertEquals(false, module.AllowDelete);
		}

		public void TestAllowView()
		{
			using var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID());
			AssertEquals(true, module.AllowView);
		}

		public void TestNewWithSecurityRight()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffNew = Factory.NewWithValidTestData<GlbStaff>();
			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffNew.PK;
			staffSecurity.GU_SecurityRight = EDISecurityCheckpoints.EdiIdentityTenantNew.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staff.HomeBranch.PK.ToGuid(), Guid.Empty))
			using (var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				var exception = AssertExceptionThrown<SecurityAccessDeniedException>(() => newMenuItem.PerformClick());
				AssertEquals(EDISecurityCheckpoints.EdiIdentityTenantNew.ErrorMessageForNotAllowed, exception.Message);
			}

			using (Env.SetTemporaryUserContext(staffNew.PK.ToGuid(), staffNew.HomeBranch.PK.ToGuid(), Guid.Empty))
			using (var module = (EdiIdentityTenantModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var newMenuItem = module.FormActionMenu.Single(m => m.Text == "&New");
				AssertNoExceptionThrown(() => newMenuItem.PerformClick());
				var openForms = ZApplication.GetOpenForms().OfType<EdiIdentityTenantForm>();
				AssertEquals(1, openForms.Count());
				openForms.First().ForceClose();
			}
		}
	}
}
