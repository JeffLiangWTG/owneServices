using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.Testing
{
	[TestedType(typeof(AuditController))]
	sealed class AuditPopupControllerTest : ZPopupControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Audit;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(AuditController); }
		}

		public void TestAuditServerSetInRegistryShouldNotThrowException()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			{
				try
				{
					BiServers.ClearBiServersCache();
					AuditController.ResetIsAuditServerSetInRegistry();
					AssertEquals("Audit should be valid if registry value is set", true, AuditController.IsAuditServerSetInRegistry);
				}
				finally
				{
					AuditController.ResetIsAuditServerSetInRegistry();
				}
			}
		}

		public void TestAuditServerNotSetInRegistryShouldThrowExceptionAndDisplayError()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				try
				{
					BiServers.ClearBiServersCache();
					AuditController.ResetIsAuditServerSetInRegistry();
					AssertEquals("Audit should be invalid if the registry value not set", false, AuditController.IsAuditServerSetInRegistry);
				}
				finally
				{
					AuditController.ResetIsAuditServerSetInRegistry();
				}
			}
		}

		public void TestAuditPluginDisabledWhenServerDoesNotExist()
		{
			BiServers.ClearBiServersCache();

			using (BiServers.TemporarilySetAuditServerToNull())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				AssertEquals("Audit server is invalid", false, AuditServerValidator.AuditHasValidServerHost(auditServer));
			}
		}
	}
}
