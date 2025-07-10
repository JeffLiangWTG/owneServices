using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAOrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (var plugin = (CAOrgSupplierPartFormCustomsPlugin)GetNewPlugIn(Part))
			{
				AssertType<CAOrgSupplierPartFormCustomsControl>(plugin.UserControl);
			}
		}

		public void TestIsPromptAuditOnSavedEnabled()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var plugin = new CAOrgSupplierPartFormCustomsPluginForTest(part))
			{
				AssertEquals(false, plugin.IsPromptAuditOnSavedEnabled_Exposed);
				part.PivotsForBinding.AddNew();
				CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
				AssertEquals(true, plugin.IsPromptAuditOnSavedEnabled_Exposed);
				CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.NoAction);
				AssertEquals(false, plugin.IsPromptAuditOnSavedEnabled_Exposed);
				CACustomsDataRegistry.Instance.EntryHighValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
				AssertEquals(true, plugin.IsPromptAuditOnSavedEnabled_Exposed);
				part.PivotsForBinding.RemoveAndDeleteAll();
				AssertEquals(false, plugin.IsPromptAuditOnSavedEnabled_Exposed);
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart() => Factory.New<OrgSupplierPart>();

		protected override OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part) => new CAOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)part);

		class CAOrgSupplierPartFormCustomsPluginForTest : CAOrgSupplierPartFormCustomsPlugin
		{
			public CAOrgSupplierPartFormCustomsPluginForTest(OrgSupplierPart part)
				: base(part)
			{
			}

			internal bool IsPromptAuditOnSavedEnabled_Exposed => base.IsPromptAuditOnSavedEnabled;
		}
	}
}
