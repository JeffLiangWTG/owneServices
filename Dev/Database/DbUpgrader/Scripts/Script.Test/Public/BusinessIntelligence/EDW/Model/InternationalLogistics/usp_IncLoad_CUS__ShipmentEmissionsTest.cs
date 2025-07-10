using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__ShipmentEmissions))]
	internal class usp_IncLoad_CUS__ShipmentEmissionsTest : BiCreateScriptTest
	{
		#region RefValueTests

		[ExpectNoExceptions]
		public void TestUpdateShipmentCO2e_RefValue()
		{
			var updateSql = @"
                UPDATE [InternationalLogistics].[BAS__ShipmentCO2e]
                SET CO2eStatus = 'NEW', Distance = 6000, TotalCO2e = 888.8
                WHERE ShipmentCO2eKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2, RefValue3, RefValue4)
                VALUES ('InternationalLogistics', 'BAS__ShipmentCO2e', 1, newid(), '{0}', '{1}', '{2}', '{3}')
            ", "NEW", "6000", "888.8", "1");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2, RefValue3, RefValue4)
                VALUES ('InternationalLogistics', 'BAS__ShipmentCO2e', 1, NULL, '{0}', '{1}', '{2}', '{3}')
            ", "NEW", "6000", "888.8", "1");
			RefValueTest(
				"InternationalLogistics",
				"BAS__ShipmentCO2e",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"ShipmentCO2eKey = 1",
				rows =>
				{
					AssertEquals("Updated CO2eStatus", "NEW", rows[0]["CO2eStatus"].ToString());
					AssertEquals("Updated DistanceInKM", decimal.Parse("6000"), decimal.Parse(rows[0]["DistanceInKM"].ToString()));
					AssertEquals("Updated TotalCO2e", decimal.Parse("888.8"), decimal.Parse(rows[0]["TotalCO2e"].ToString()));
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateControllingCustomerAddress_RefValues()
		{
			var orgAddrSql = @"
                IF NOT EXISTS (SELECT 1 FROM [Organization].[BAS__OrganizationAddress] WHERE OrganizationAddressKey = 22)
                BEGIN
                    INSERT INTO [Organization].[BAS__OrganizationAddress]
                    (OrganizationAddressKey, OrganizationAddressID, OrganizationKey)
                    VALUES (22, newid(), 99)
                END
            ";
			TestConnection.ExecuteNonQuery(orgAddrSql);
			var orgSql = @"
                IF NOT EXISTS (SELECT 1 FROM [Organization].[BAS__Organization] WHERE OrganizationKey = 99)
                BEGIN
                    INSERT INTO [Organization].[BAS__Organization]
                    (OrganizationKey, OrganizationID, Code)
                    VALUES (99, newid(), 'NEWORG')
                END
                ELSE
                BEGIN
                    UPDATE [Organization].[BAS__Organization]
                    SET Code = 'NEWORG'
                    WHERE OrganizationKey = 99
                END
            ";
			TestConnection.ExecuteNonQuery(orgSql);
			var updateSql = @"
                UPDATE [InternationalLogistics].[BAS__ControllingCustomerAddress]
                SET AddressKey = 22
                WHERE ControllingCustomerAddressKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ControllingCustomerAddress', 1, newid(), '1', '2')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ControllingCustomerAddress', 1, NULL, '1', '22')
            ");
			RefValueTest(
				"InternationalLogistics",
				"BAS__ControllingCustomerAddress",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"ShipmentKey = 1",
				rows =>
				{
					AssertEquals("ControllingCustomerAddressKey", "1", rows[0]["ControllingCustomerAddressKey"].ToString());
					AssertEquals("ControllingCustomerOrgAddressKey", "22", rows[0]["ControllingCustomerOrgAddressKey"].ToString());
					AssertEquals("ControllingCustomerOrgCode", "NEWORG", rows[0]["ControllingCustomerOrgCode"].ToString());
					Assert(!string.IsNullOrEmpty(rows[0]["ControllingCustomerOrgID"].ToString()));
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateConsignorAddress_RefValues()
		{
			var orgSql = @"
                IF NOT EXISTS (SELECT 1 FROM [Organization].[BAS__Organization] WHERE OrganizationKey = 1)
                BEGIN
                    INSERT INTO [Organization].[BAS__Organization]
                    (OrganizationKey, OrganizationID, Code)
                    VALUES (1, newid(), 'ORG1NEW')
                END
            ";
			TestConnection.ExecuteNonQuery(orgSql);
			var orgAddrSql = @"
                IF NOT EXISTS (SELECT 1 FROM [Organization].[BAS__OrganizationAddress] WHERE OrganizationAddressKey = 1)
                BEGIN
                    INSERT INTO [Organization].[BAS__OrganizationAddress]
                    (OrganizationAddressKey, OrganizationAddressID, OrganizationKey)
                    VALUES (1, newid(), 1)
                END
            ";
			TestConnection.ExecuteNonQuery(orgAddrSql);
			var updateSql = @"
                UPDATE [InternationalLogistics].[BAS__ConsignorAddress]
                SET ShipmentKey = 1, AddressKey = 1
                WHERE ConsignorAddressKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ConsignorAddress', 1, newid(), '1', '4')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ConsignorAddress', 1, NULL, '1', '1')
            ");
			RefValueTest(
				"InternationalLogistics",
				"BAS__ConsignorAddress",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"ShipmentKey = 1",
				rows =>
				{
					AssertEquals("ConsignorAddressKey", "1", rows[0]["ConsignorAddressKey"].ToString());
					AssertEquals("ConsignorOrgAddressKey", "1", rows[0]["ConsignorOrgAddressKey"].ToString());
					AssertEquals("ConsignorOrgCode", "ORG1NEW", rows[0]["ConsignorOrgCode"].ToString());
					Assert(!string.IsNullOrEmpty(rows[0]["ConsignorOrgID"].ToString()));
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateConsigneeAddress_RefValues()
		{
			var updateSql = @"
                UPDATE [InternationalLogistics].[BAS__ConsigneeAddress]
                SET AddressKey = 34
                WHERE ConsigneeAddressKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ConsigneeAddress', 1, newid(), '1', '3')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('InternationalLogistics', 'BAS__ConsigneeAddress', 1, NULL, '1', '34')
            ");
			RefValueTest(
				"InternationalLogistics",
				"BAS__ConsigneeAddress",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"ConsigneeAddressKey = 1",
				rows =>
				{
					AssertEquals("ConsigneeAddressKey remains", "1", rows[0]["ConsigneeAddressKey"].ToString());
					Assert(rows[0]["ConsigneeOrgAddressKey"] == DBNull.Value || string.IsNullOrEmpty(rows[0]["ConsigneeOrgAddressKey"].ToString()));
					Assert(rows[0]["ConsigneeOrgCode"] == DBNull.Value || string.IsNullOrEmpty(rows[0]["ConsigneeOrgCode"].ToString()));
					Assert(rows[0]["ConsigneeOrgID"] == DBNull.Value || string.IsNullOrEmpty(rows[0]["ConsigneeOrgID"].ToString()));
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateJobHeader_RefValues()
		{
			var updateSql = @"
                UPDATE [Finance].[BAS__JobHeader]
                SET LocalAgentAddressKey = 88
                WHERE JobHeaderKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('Finance', 'BAS__JobHeader', 1, newid(), '1', '1')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2)
                VALUES ('Finance', 'BAS__JobHeader', 1, NULL, '1', '88')
            ");
			RefValueTest(
				"Finance",
				"BAS__JobHeader",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"ShipmentKey = 1",
				rows =>
				{
					AssertEquals("JobHeaderKey", "1", rows[0]["JobHeaderKey"].ToString());
					Assert(rows[0]["LocalClientOrgCode"] == DBNull.Value || string.IsNullOrEmpty(rows[0]["LocalClientOrgCode"].ToString()));
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateOrganization_RefValues()
		{
			var checkTransportSql = @"
                SELECT COUNT(1) FROM [InternationalLogistics].[BAS__ConsolAndShipmentTransport]
                WHERE CarrierAddressKey = 10
            ";
			var transportCount = Convert.ToInt32(TestConnection.ExecuteScalar(checkTransportSql));
			if (transportCount == 0)
			{
				var insertTransportSql = @"
                    INSERT INTO [InternationalLogistics].[BAS__ConsolAndShipmentTransport]
                    (ConsolAndShipmentTransportKey, ConsolAndShipmentTransportID, ShipmentKey, CarrierAddressKey, TransportMode, IsLinked)
                    VALUES ((SELECT ISNULL(MAX(ConsolAndShipmentTransportKey),0)+1 FROM [InternationalLogistics].[BAS__ConsolAndShipmentTransport]),
                    newid(), 1, 10, 'SEA', 0)
                ";
				TestConnection.ExecuteNonQuery(insertTransportSql);
			}
			var checkOrgAddrSql = @"
                SELECT COUNT(1) FROM [Organization].[BAS__OrganizationAddress]
                WHERE OrganizationAddressKey = 10
            ";
			var orgAddrCount = Convert.ToInt32(TestConnection.ExecuteScalar(checkOrgAddrSql));
			if (orgAddrCount == 0)
			{
				var insertOrgAddrSql = @"
                    INSERT INTO [Organization].[BAS__OrganizationAddress]
                    (OrganizationAddressKey, OrganizationAddressID, OrganizationKey)
                    VALUES (10, newid(), 1)
                ";
				TestConnection.ExecuteNonQuery(insertOrgAddrSql);
			}
			var checkOrgSql = @"
                SELECT COUNT(1) FROM [Organization].[BAS__Organization]
                WHERE OrganizationKey = 1
            ";
			var orgCount = Convert.ToInt32(TestConnection.ExecuteScalar(checkOrgSql));
			if (orgCount == 0)
			{
				var insertOrgSql = @"
                    INSERT INTO [Organization].[BAS__Organization]
                    (OrganizationKey, OrganizationID, Code)
                    VALUES (1, newid(), 'OLDCODE')
                ";
				TestConnection.ExecuteNonQuery(insertOrgSql);
			}
			else
			{
				var updateOrgOldSql = @"
                    UPDATE [Organization].[BAS__Organization]
                    SET Code = 'OLDCODE'
                    WHERE OrganizationKey = 1
                ";
				TestConnection.ExecuteNonQuery(updateOrgOldSql);
			}
			var updateOrgAddrSql = @"
                UPDATE [Organization].[BAS__OrganizationAddress]
                SET OrganizationKey = 5
                WHERE OrganizationAddressKey = 10
            ";
			TestConnection.ExecuteNonQuery(updateOrgAddrSql);
			var checkOrg5Sql = @"
                SELECT COUNT(1) FROM [Organization].[BAS__Organization]
                WHERE OrganizationKey = 5
            ";
			var org5Count = Convert.ToInt32(TestConnection.ExecuteScalar(checkOrg5Sql));
			if (org5Count == 0)
			{
				var insertOrg5Sql = @"
                    INSERT INTO [Organization].[BAS__Organization]
                    (OrganizationKey, OrganizationID, Code)
                    VALUES (5, newid(), 'NEWCODE')
                ";
				TestConnection.ExecuteNonQuery(insertOrg5Sql);
			}
			else
			{
				var updateOrg5Sql = @"
                    UPDATE [Organization].[BAS__Organization]
                    SET Code = 'NEWCODE'
                    WHERE OrganizationKey = 5
                ";
				TestConnection.ExecuteNonQuery(updateOrg5Sql);
			}
			var updateSql = @"
                UPDATE [Organization].[BAS__OrganizationAddress]
                SET OrganizationKey = 5
                WHERE OrganizationAddressKey = 10
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Organization', 'BAS__OrganizationAddress', 10, newid(), '1')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Organization', 'BAS__OrganizationAddress', 10, NULL, '5')
            ");
			RefValueTest(
				"Organization",
				"BAS__OrganizationAddress",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"CarrierAddressKey = 10",
				rows =>
				{
					AssertEquals("CarrierOrgCode updated", "NEWCODE", rows[0]["CarrierOrgCode"].ToString());
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateOrganizationAddress_RefValues()
		{
			var insertTransportSql = @"
                INSERT INTO [InternationalLogistics].[BAS__ConsolAndShipmentTransport]
                (ConsolAndShipmentTransportKey, ConsolAndShipmentTransportID, ShipmentKey, CarrierAddressKey, TransportMode, IsLinked)
                VALUES ((SELECT ISNULL(MAX(ConsolAndShipmentTransportKey),0)+1 FROM [InternationalLogistics].[BAS__ConsolAndShipmentTransport]),
                newid(), 1, 1, 'SEA', 0)
            ";
			TestConnection.ExecuteNonQuery(insertTransportSql);
			var updateSql = @"
                UPDATE [Organization].[BAS__OrganizationAddress]
                SET OrganizationKey = 99
                WHERE OrganizationAddressKey = 1
            ";
			var insertOrgSql = @"
                INSERT INTO [Organization].[BAS__Organization]
                (OrganizationKey, OrganizationID, Code)
                VALUES (99, newid(), 'NEWORG')
            ";
			TestConnection.ExecuteNonQuery(insertOrgSql);
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Organization', 'BAS__OrganizationAddress', 1, newid(), '1')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Organization', 'BAS__OrganizationAddress', 1, NULL, '99')
            ");
			RefValueTest(
				"Organization",
				"BAS__OrganizationAddress",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"CarrierAddressKey = 1",
				rows =>
				{
					AssertEquals("Updated CarrierOrgCode", "NEWORG", rows[0]["CarrierOrgCode"].ToString());
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateJobVoyOrigin_RefValues()
		{
			var insertTransportSql = @"
                INSERT INTO [InternationalLogistics].[BAS__ConsolAndShipmentTransport]
                (ConsolAndShipmentTransportKey, ConsolAndShipmentTransportID, ShipmentKey, JobSailingKey, CarrierAddressKey, TransportMode, IsLinked)
                VALUES (
                    (SELECT ISNULL(MAX(ConsolAndShipmentTransportKey),0)+1 FROM [InternationalLogistics].[BAS__ConsolAndShipmentTransport]),
                    newid(), 1, 1, 1, 'SEA', 0)
            ";
			TestConnection.ExecuteNonQuery(insertTransportSql);
			var checkVoySql = @"
                SELECT COUNT(1) FROM [Sailing].[BAS__JobVoyage]
                WHERE JobVoyageKey = 44
            ";
			var countVoy = Convert.ToInt32(TestConnection.ExecuteScalar(checkVoySql));
			if (countVoy == 0)
			{
				var insertVoySql = @"
                    INSERT INTO [Sailing].[BAS__JobVoyage]
                    (JobVoyageKey, JobVoyageID, OrganizationKey, Organization)
                    VALUES (44, newid(), 5, newid())
                ";
				TestConnection.ExecuteNonQuery(insertVoySql);
			}
			var updateSql = @"
                UPDATE [Sailing].[BAS__JobVoyOrigin]
                SET JobVoyageKey = 44
                WHERE JobVoyOriginKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Sailing', 'BAS__JobVoyOrigin', 1, newid(), '1')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Sailing', 'BAS__JobVoyOrigin', 1, NULL, '44')
            ");
			RefValueTest(
				"Sailing",
				"BAS__JobVoyOrigin",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"JobVoyOriginKey = 1",
				rows =>
				{
					AssertEquals("JobVoyOriginKey remains", "1", rows[0]["JobVoyOriginKey"].ToString());
					AssertEquals("Updated JobVoyageKey from JobVoyOrigin", "44", rows[0]["JobVoyageKey"].ToString());
				}
			);
		}

		[ExpectNoExceptions]
		public void TestUpdateJobVoyage_RefValues()
		{
			var insertTransportSql = @"
                INSERT INTO [InternationalLogistics].[BAS__ConsolAndShipmentTransport]
                (ConsolAndShipmentTransportKey, ConsolAndShipmentTransportID, ShipmentKey, JobSailingKey, CarrierAddressKey, TransportMode, IsLinked)
                VALUES (
                    (SELECT ISNULL(MAX(ConsolAndShipmentTransportKey),0)+1 FROM [InternationalLogistics].[BAS__ConsolAndShipmentTransport]),
                    newid(), 1, 1, 1, 'SEA', 0)
            ";
			TestConnection.ExecuteNonQuery(insertTransportSql);
			var updateSql = @"
                UPDATE [Sailing].[BAS__JobVoyage]
                SET OrganizationKey = 33
                WHERE JobVoyageKey = 1
            ";
			var deleteRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Sailing', 'BAS__JobVoyage', 1, newid(), '1')
            ");
			var insertRowSql = string.Format(CultureInfo.InvariantCulture, @"
                INSERT INTO [biadmin].[TransformedRow]
                (SchemaName, TableName, KeyValue, PKValue, RefValue1)
                VALUES ('Sailing', 'BAS__JobVoyage', 1, NULL, '33')
            ");
			RefValueTest(
				"Sailing",
				"BAS__JobVoyage",
				updateSql,
				deleteRowSql,
				insertRowSql,
				"JobVoyageKey = 1",
				rows =>
				{
					AssertEquals("JobVoyageKey remains", "1", rows[0]["JobVoyageKey"].ToString());
					AssertEquals("Updated JobVoyageOrganizationKey", "33", rows[0]["JobVoyageOrganizationKey"].ToString());
				}
			);
		}

		#endregion

		#region Helpers

		void RefValueTest(string schemaName, string tableName, string updateSql, string deleteRowSql, string insertRowSql, string filterCondition, Action<DataRow[]> rowAssert)
		{
			// Update BAS__Table
			TestConnection.ExecuteNonQuery(updateSql);

			// Delete TransformedRow
			var sqlDeleteTransformed = string.Format(CultureInfo.InvariantCulture, @"
                DELETE FROM [biadmin].[TransformedRow]
                WHERE SchemaName = '{0}' AND TableName = '{1}'
            ", schemaName, tableName);
			TestConnection.ExecuteNonQuery(sqlDeleteTransformed);

			// Delete and Insert Row
			TestConnection.ExecuteNonQuery(deleteRowSql);
			TestConnection.ExecuteNonQuery(insertRowSql);

			// Incremental Load
			Execute();

			// Assert Incremental Load Triggered
			AssertIncrementalLoadTriggered();

			// Assert Value
			var updatedTable = SelectRows();
			var rows = updatedTable.Select(filterCondition);
			Assert(rows.Length > 0);
			rowAssert(rows);
		}

		void AssertIncrementalLoadTriggered()
		{
			var sqlState = string.Format(CultureInfo.InvariantCulture, @"
                SELECT CurrentState, MergeTransformDeleteRecordCount, MergeTransformInsertRecordCount
                FROM [{0}].[biadmin].[CustomTableState]
                WHERE ModelSchemaName = 'InternationalLogistics' AND ModelTableName = 'CUS__ShipmentEmissions'
            ", ScriptDbName);

			var stateTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlState);
			Assert(stateTable.Rows.Count > 0);
			var currentState = stateTable.Rows[0]["CurrentState"].ToString();

			var deleteCount = int.Parse(stateTable.Rows[0]["MergeTransformDeleteRecordCount"].ToString());
			var insertCount = int.Parse(stateTable.Rows[0]["MergeTransformInsertRecordCount"].ToString());

			AssertEquals("Transformed", currentState);
			Assert(deleteCount > 0);
			Assert(insertCount > 0);
		}

		#endregion

		protected override string ScriptDbName => Db.EdwDatabaseName;

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[InternationalLogistics].[CUS__ShipmentEmissions]",
				ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Shipment]
				([ShipmentKey], [ShipmentID], [ArrivalDate], [DepartureDate], [PortOfDestination], [PortOfOrigin], [TransportMode], [ActualWeight], [UnitOfWeight])
				VALUES
					(1, newid(), '2024-05-02 12:00:00', '2024-05-01 12:00:00', 'SGSIN', 'AUSYD', 'SEA', 1000.02, 'KG');

				INSERT [{0}].[InternationalLogistics].[BAS__ShipmentCO2e]
				([ShipmentCO2eKey], [ShipmentCO2eID], [ShipmentKey], [Distance], [TotalCO2e], [CO2eStatus])
				VALUES
					(1, newid(), 1, 5000, 999.9, 'CUR');

				INSERT [{0}].[Finance].[BAS__JobHeader]
				([JobHeaderKey], [JobHeaderID], [ShipmentKey], [LocalAgentAddressKey])
				VALUES
					(1, newid(), 1, 1),
					(2, newid(), 2, 1);

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
				([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey])
				VALUES
					(1, newid(), 1),
					(2, newid(), 2),
					(3, newid(), 5);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot]
				([ConsolidationShipmentPivotKey], [ConsolidationShipmentPivotID], [ShipmentKey], [ConsolidationKey])
				VALUES
					(1, newid(), 1, 1),
					(2, newid(), 2, 2);

				INSERT [{0}].[Sailing].[BAS__JobSailing]
				([JobSailingKey], [JobSailingID], [JobVoyOriginKey])
				VALUES
					(1, newid(), 1);

				INSERT [{0}].[Sailing].[BAS__JobVoyOrigin]
				([JobVoyOriginKey], [JobVoyOriginID], [JobVoyageKey])
				VALUES
					(1, newid(), 1);

				INSERT [{0}].[Sailing].[BAS__JobVoyage]
				([JobVoyageKey], [JobVoyageID], [OrganizationKey], [Organization])
				VALUES
					(1, newid(), 5, CAST('7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9' AS UNIQUEIDENTIFIER));

				INSERT [{0}].[InternationalLogistics].[BAS__ControllingCustomerAddress]
				([ControllingCustomerAddressKey], [ControllingCustomerAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 2),
					(2, newid(), 2, 2);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsigneeAddress]
				([ConsigneeAddressKey], [ConsigneeAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 3),
					(2, newid(), 2, 3);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsignorAddress]
				([ConsignorAddressKey], [ConsignorAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 4),
					(2, newid(), 2, 4);

				INSERT [{0}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID], [Code])
				VALUES
					(5, CAST('7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9' AS UNIQUEIDENTIFIER), 'ORG5CODE'),
					(6, newid(), 'ORG6CODE');

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
				([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [ShipmentKey], [CarrierAddressKey], [IsLinked], [TransportMode], [ParentType], [ATA], [ATD])
				VALUES
					(1, newid(), 1, 3, 0, 'SEA', 'SHP', '2024-05-06 12:00:00', '2024-05-05 12:00:00');

				INSERT [{0}].[biadmin].[TransformedRow]
				(SchemaName, TableName, KeyValue, PKValue)
				SELECT 'InternationalLogistics', 'BAS__Shipment', ShipmentKey, newid()
				FROM [{0}].[InternationalLogistics].[BAS__Shipment];
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		string GetIncLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT [IncrementalLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__ShipmentEmissions'",
				ScriptDbName);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			return record.ItemArray[0].ToString();
		}

		void Execute()
		{
			var incLoadSQLText = GetIncLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;
			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
                SELECT {0}
                FROM [{1}].[biAdmin].[CustomTableConfiguration]
                WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__ShipmentEmissions'
            ", sqlName, ScriptDbName);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = string.Format(CultureInfo.InvariantCulture, @"
                USE {0}
                {1}
            ", ScriptDbName, incLoadSQLText);
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
			ExecuteTableLoad("InitialLoadQuery");
		}
	}
}
