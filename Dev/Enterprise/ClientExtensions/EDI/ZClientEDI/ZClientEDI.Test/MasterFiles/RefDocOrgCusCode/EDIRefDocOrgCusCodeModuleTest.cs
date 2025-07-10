using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Test
{
	[TestedType(typeof(EDIRefDocOrgCusCodeModule))]
	class EDIRefDocOrgCusCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.RefDocOrgCusCode;
		public void TestAllowNew()
		{
			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module does not support adding new when the registry setting is disabled", !module.AllowNew);
			}

			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module supports adding new when the registry setting is enabled", module.AllowNew);
			}
		}

		public void TestAllowEdit()
		{
			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module does not support editing when the registry setting is disabled", !module.AllowEdit);
			}

			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module supports editting when the registry setting is enabled", module.AllowEdit);
			}
		}

		public void TestAllowDelete()
		{
			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module does not support deleting when the registry setting is disabled", !module.AllowDelete);
			}

			using (EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new EDIRefDocOrgCusCodeModule())
			{
				Assert("Document Organisation Mapping Module supports deleting when the registry setting is enabled", module.AllowDelete);
			}
		}
	}
}
