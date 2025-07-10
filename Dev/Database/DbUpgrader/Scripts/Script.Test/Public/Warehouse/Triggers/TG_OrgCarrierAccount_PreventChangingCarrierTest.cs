using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgCarrierAccount_PreventChangingCarrier))]
	class TG_OrgCarrierAccount_PreventChangingCarrierTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_AttemptToChangeCarrier

		public void TestTrigger_DontAttemptToChangeCarrier()
		{
			var carrier = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var changeCarrierSQL = new SqlQueryBuilder();

			AssertNoExceptionThrown(() => OrgCarrierAccount.UpdateWhere(carrierAccount.PK).Set(c => c.OAN_OH_Carrier, carrier.PK).Post(TestConnection));
		}

		public void TestTrigger_AttemptToChangeCarrier()
		{
			var carrier1 = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			var carrier2 = new OrgHeader("C2").InsertAndReturnObject(TestConnection);

			var billToParty = new OrgHeader("B1").InsertAndReturnObject(TestConnection);
			var carrierAccount = new OrgCarrierAccount(carrier1.PK, billToParty.PK, true, "123", "ABC", "123").InsertAndReturnObject(TestConnection);

			var changeCarrierSQL = new SqlQueryBuilder();

			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete Carrier for Carrier Account.", () => OrgCarrierAccount.UpdateWhere(carrierAccount.PK).Set(c => c.OAN_OH_Carrier, carrier2.PK).Post(TestConnection), true);
		}

		#endregion
	}
}

