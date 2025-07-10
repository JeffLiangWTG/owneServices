using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(AuditServerValidator))]
	sealed class AuditServerValidatorTest : TransactionedTestCase
	{
		public void TestAuditHasValidServerHostWhenServerDoesNotExist()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				BiServers.ClearBiServersCache();
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				AssertEquals("Audit server is invalid", expected: false, AuditServerValidator.AuditHasValidServerHost(auditServer));
			}
		}

		public void TestAuditHasValidServerHostWhenServerNameWithComma()
		{
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "localhost,1433"))
			{
				BiServers.ClearBiServersCache();
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				AssertEquals("Audit server is valid", expected: true, AuditServerValidator.AuditHasValidServerHost(auditServer));
			}
		}

		public void TestAuditHasValidServerHostWhenServerNameNotValid()
		{
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NOT VALID"))
			{
				BiServers.ClearBiServersCache();
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				AssertEquals("Audit server is valid", expected: false, AuditServerValidator.AuditHasValidServerHost(auditServer));
			}
		}

		public void TestAuditHasValidServerHostWhenServerNameValid()
		{
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			{
				BiServers.ClearBiServersCache();
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				AssertEquals("Audit server is valid", expected: true, AuditServerValidator.AuditHasValidServerHost(auditServer));
			}
		}
	}
}
