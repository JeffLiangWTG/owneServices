namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using CargoWise.Data;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class AuditChangesProcessorTest : TestCase
	{
		public void TestReadNextChange()
		{
			using (var connection = AuditTestHelper.GetAuditConnection())
			using (var cmd = connection.Command($"SELECT 0x AS [{AuditFieldNames.StartLsnFieldName}], NULL AS [{AuditFieldNames.SeqValFieldName}], 1 AS [__$maxChangeRank], NULL AS [__$operation]"))
			{
				try
				{
					var queryResult = DataUtils.GetDataTableFromCommand(cmd);
					int operation;

					var testLogger = new LoggerForTest();
					SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
					var rawSub = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols);
					var testSubscriber = (ActualDataChangesAuditSubscriberWrapper)rawSub.GetWrapper(connection, testLogger);

					string[] columns = new string[] { AuditFieldNames.StartLsnFieldName, AuditFieldNames.SeqValFieldName, "__$maxChangeRank", "__$operation" };
					testSubscriber.ReadNextChange(out operation, queryResult, queryResult.Rows[0]);
					Fail("Should throw exception");
				}
				catch (InvalidCastException ex)
				{
					AssertEquals($"Failure during execution of !RX. The change table yielded the following row where it could not parse {AuditFieldNames.StartLsnFieldName} or {AuditFieldNames.SeqValFieldName} or {AuditFieldNames.LsnPeriodFieldName}:\r\n > " + AuditFieldNames.StartLsnFieldName + " = System.Byte[]\r\n > " + AuditFieldNames.SeqValFieldName + " = NULL\r\n > __$maxChangeRank = 1\r\n > __$operation = NULL", ex.Message);
				}
			}
		}
	}
}
