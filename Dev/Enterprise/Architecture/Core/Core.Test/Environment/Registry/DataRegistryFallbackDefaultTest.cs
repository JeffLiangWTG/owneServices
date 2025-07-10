using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[UseSnapshotProtection]
	sealed class DataRegistryFallbackDefaultTest : TestCase
	{
		public void TestGetPasteDebounceMs_AsRegistryValue()
		{
			var registryValue = 200;
			Registry.RawRegistry.PasteDebounceMs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(registryValue, Registry.PasteDebounceMs);
		}

		public void TestGetPasteDebounceMs_LockedOut_ConnectionKilled_SchemaUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: true, killOtherConnections: true, updateSchemaVersion: true);
		}

		public void TestGetPasteDebounceMs_LockedOut_ConnectionKilled_SchemaNotUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: true, killOtherConnections: true, updateSchemaVersion: false);
		}

		public void TestGetPasteDebounceMs_LockedOut_ConnectionNotKilled_SchemaUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: true, killOtherConnections: false, updateSchemaVersion: true);
		}

		public void TestGetPasteDebounceMs_LockedOut_ConnectionNotKilled_SchemaNotUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: true, killOtherConnections: false, updateSchemaVersion: false);
		}

		public void TestGetPasteDebounceMs_NotLockedOut_ConnectionKilled_SchemaUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: false, killOtherConnections: true, updateSchemaVersion: true);
		}

		public void TestGetPasteDebounceMs_NotLockedOut_ConnectionKilled_SchemaNotUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: false, killOtherConnections: true, updateSchemaVersion: false);
		}

		public void TestGetPasteDebounceMs_NotLockedOut_ConnectionNotKilled_SchemaUpdated()
		{
			TestGetPasteDebounceMs(acquireLockout: false, killOtherConnections: false, updateSchemaVersion: true);
		}

		public void TestGetPasteDebounceMs_NoUpgrade()
		{
			TestGetPasteDebounceMs(acquireLockout: false, killOtherConnections: false, updateSchemaVersion: false);
		}

		void TestGetPasteDebounceMs(bool acquireLockout, bool killOtherConnections, bool updateSchemaVersion)
		{
			var defaultValue = Registry.RawRegistry.PasteDebounceMs.DefaultValue;
			using (Db.DisposableUpgrade_ForTest(acquireLockout, killOtherConnections, updateSchemaVersion))
			{
				AssertEquals(defaultValue, Registry.PasteDebounceMs);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}
