using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgWhsClientAccountAssociation_PreventDuplicateAssociations))]
	class TG_OrgWhsClientAccountAssociation_PreventDuplicateAssociationsTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_AttemptToCreateDuplicateAssociation

		public void TestTrigger_AttemptToChangeClientToDuplicateAssociation()
		{
			var org1 = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var org2 = new OrgHeader("O2").InsertAndReturnObject(TestConnection);

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org1.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);
			new OrgWhsClientAccountAssociation(org2.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => OrgWhsClientAccountAssociation.UpdateWhere(association.PK).Set(owca => owca.OWC_OH_Client, org2.PK).Post(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToChangeWarehouseToDuplicateAssociation()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);

			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs1.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);
			new OrgWhsClientAccountAssociation(org.PK, whs2.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => OrgWhsClientAccountAssociation.UpdateWhere(association.PK).Set(owca => owca.OWC_WW_Warehouse, whs2.PK).Post(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToChangeSalesChannelToDuplicateAssociation()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var salesChannel1 = new WhsSalesChannel("SC1", "Channel1").InsertAndReturnObject(TestConnection);
			var salesChannel2 = new WhsSalesChannel("SC2", "Channel2").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);
			new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel2.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => OrgWhsClientAccountAssociation.UpdateWhere(association.PK).Set(owca => owca.OWC_WSH_SalesChannel, salesChannel2.PK).Post(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToChangeCarrierToDuplicateAssociation()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier1 = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty1 = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount1 = new OrgCarrierAccount(carrier1.PK, billToParty1.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var carrier2 = new OrgHeader("C2").InsertAndReturnObject(TestConnection);
			var billToParty2 = new OrgHeader("B2").InsertAndReturnObject(TestConnection);
			var carrierAccount2 = new OrgCarrierAccount(carrier2.PK, billToParty2.PK, true, "456", "DEF", "456").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);
			new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount2.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => OrgWhsClientAccountAssociation.UpdateWhere(association.PK).Set(owca => owca.OWC_OAN_CarrierAccount, carrierAccount2.PK).Post(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToChangeCarrierToDuplicateAssociation_ChecksAttachedCarrier()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier1 = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty1 = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount1 = new OrgCarrierAccount(carrier1.PK, billToParty1.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var carrier2 = new OrgHeader("C2").InsertAndReturnObject(TestConnection);
			var billToParty2 = new OrgHeader("B2").InsertAndReturnObject(TestConnection);
			var carrierAccount2 = new OrgCarrierAccount(carrier2.PK, billToParty2.PK, true, "456", "DEF", "456").InsertAndReturnObject(TestConnection);

			var billToParty3 = new OrgHeader("B3").InsertAndReturnObject(TestConnection);
			var carrierAccount3 = new OrgCarrierAccount(carrier2.PK, billToParty3.PK, true, "789", "GHI", "789").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);
			new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount2.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => OrgWhsClientAccountAssociation.UpdateWhere(association.PK).Set(owca => owca.OWC_OAN_CarrierAccount, carrierAccount3.PK).Post(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToInsertDuplicateAssociation()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToInsertDuplicateAssociation_NullWarehouse()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);

			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var salesChannel = new WhsSalesChannel("SC1", "Channel").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, null, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => new OrgWhsClientAccountAssociation(org.PK, null, carrierAccount.PK) { OWC_WSH_SalesChannel = salesChannel.PK }.InsertAndReturnObject(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToInsertDuplicateAssociation_NullSalesChannel()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var association = new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK).InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Association between Client, Warehouse, Carrier and Sales Channel must be unique.",
				() => new OrgWhsClientAccountAssociation(org.PK, whs.PK, carrierAccount.PK).InsertAndReturnObject(TestConnection),
				true);
		}

		public void TestTrigger_AttemptToInsertDifferentAssociation_NoExceptions()
		{
			var org1 = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			var org2 = new OrgHeader("O2").InsertAndReturnObject(TestConnection);

			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);

			var carrier1 = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty1 = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount1 = new OrgCarrierAccount(carrier1.PK, billToParty1.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var carrier2 = new OrgHeader("C2").InsertAndReturnObject(TestConnection);
			var billToParty2 = new OrgHeader("B2").InsertAndReturnObject(TestConnection);
			var carrierAccount2 = new OrgCarrierAccount(carrier2.PK, billToParty2.PK, true, "456", "DEF", "456").InsertAndReturnObject(TestConnection);

			var salesChannel1 = new WhsSalesChannel("SC1", "Channel1").InsertAndReturnObject(TestConnection);
			var salesChannel2 = new WhsSalesChannel("SC2", "Channel2").InsertAndReturnObject(TestConnection);

			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, whs1.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org2.PK, whs1.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, whs2.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, whs1.PK, carrierAccount2.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, whs1.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel2.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, null, carrierAccount1.PK) { OWC_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection));
			AssertNoExceptionThrown(() => new OrgWhsClientAccountAssociation(org1.PK, whs1.PK, carrierAccount1.PK) { OWC_WSH_SalesChannel = null }.InsertAndReturnObject(TestConnection));
		}

		#endregion
	}
}

