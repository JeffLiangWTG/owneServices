using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(ctfn_JobShipmentContainerTypes))]
	sealed class ctfn_JobShipmentContainerTypesTest : DbCreateScriptTest
	{
		public void TestGroupingSingleContainerCount()
		{
			var shipmentPk = Guid.NewGuid();
			var refContainerPk = Guid.NewGuid();
			var containerPk = Guid.NewGuid();
			var packLine1Pk = Guid.NewGuid();
			var packLine2Pk = Guid.NewGuid();
			var packLine3Pk = Guid.NewGuid();
			var packLine4Pk = Guid.NewGuid();
			var packPivot1Pk = Guid.NewGuid();
			var packPivot2Pk = Guid.NewGuid();
			var packPivot3Pk = Guid.NewGuid();
			var packPivot4Pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk));

			var refContainer = GetRefContainerFromDatabase();

			if (refContainer.IsNullOrEmpty())
			{
				TestConnection.ExecuteNonQuery(GetInsertRefContainerCommand(refContainerPk));
			}
			else
			{
				refContainerPk = new Guid(refContainer);
			}

			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(containerPk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine1Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine2Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine3Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine4Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot1Pk, containerPk, packLine1Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot2Pk, containerPk, packLine2Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot3Pk, containerPk, packLine3Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot4Pk, containerPk, packLine4Pk));

			AssertDataInserted(shipmentPk, containerPk, 1, 4);
			AssertFunction(shipmentPk, "1x40GP");
		}

		public void TestGroupingMultipleContainerCount()
		{
			var shipmentPk = Guid.NewGuid();
			var refContainerPk = Guid.NewGuid();
			var container1Pk = Guid.NewGuid();
			var container2Pk = Guid.NewGuid();
			var packLine1Pk = Guid.NewGuid();
			var packLine2Pk = Guid.NewGuid();
			var packLine3Pk = Guid.NewGuid();
			var packLine4Pk = Guid.NewGuid();
			var packPivot1Pk = Guid.NewGuid();
			var packPivot2Pk = Guid.NewGuid();
			var packPivot3Pk = Guid.NewGuid();
			var packPivot4Pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk));

			var refContainer = GetRefContainerFromDatabase();

			if (refContainer.IsNullOrEmpty())
			{
				TestConnection.ExecuteNonQuery(GetInsertRefContainerCommand(refContainerPk));
			}
			else
			{
				refContainerPk = new Guid(refContainer);
			}

			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(container1Pk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine1Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine2Pk, shipmentPk));

			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(container2Pk, refContainerPk, "3"));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine3Pk, shipmentPk));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLine4Pk, shipmentPk));

			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot1Pk, container1Pk, packLine1Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot2Pk, container1Pk, packLine2Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot3Pk, container2Pk, packLine3Pk));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(packPivot4Pk, container2Pk, packLine4Pk));

			AssertDataInserted(shipmentPk, container1Pk, 1, 2);
			AssertDataInserted(shipmentPk, container2Pk, 1, 2);
			AssertFunction(shipmentPk, "4x40GP");
		}

		string GetInsertJobShipmentCommand(Guid shipmentPK)
		{
			var sql = $@" INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered)
					VALUES ('{shipmentPK}', 'S00001230', 1) ";

			return sql;
		}

		string GetRefContainerFromDatabase()
		{
			var sqlFunction = $"select RC_PK from dbo.RefContainer where RC_Code = '40GP'";

			using (var command = TestConnection.Command(sqlFunction))
			{
				return command.ExecuteScalar().ToString();
			}
		}

		string GetInsertRefContainerCommand(Guid refContainerPk)
		{
			var sql = $@"INSERT INTO dbo.RefContainer (RC_PK, RC_Code, RC_StorageClass, RC_HandlingRateClass, RC_FreightRateClass) 
								VALUES ('{refContainerPk}', '40ZZ', '40S', '40H', '40R')";

			return sql;
		}

		string GetInsertJobContainerCommand(Guid containerPK, Guid refContainerPk, string containerCount = "1")
		{
			var sql = $@" INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerCount, JC_RC) VALUES ('{containerPK}', {containerCount}, '{refContainerPk}') ";

			return sql;
		}

		string GetInsertJobPackLinesCommand(Guid packLinePk, Guid shipmentPk)
		{
			var sql = $@" INSERT INTO dbo.JobPackLines ([JL_PK], [JL_IsValid], [JL_FreightMode], [JL_PackageCount], [JL_F3_NKPackType], [JL_ContainerPackingOrder],
							[JL_ActualWeight], [JL_ActualWeightUQ], [JL_UnitOfDimension], [JL_ActualVolume], [JL_ActualVolumeUQ], [JL_Description],
							[JL_RH_NKCommodityCode], [JL_JS])
							VALUES ('{packLinePk}', 1, 'OUT', 45, 'PCE', 3, 2222.000, 'KG', 'M', 22.000, 'M3', 'goods', 'GEN', '{shipmentPk}') ";

			return sql;
		}

		string GetInsertJobContainerPackPivotCommand(Guid packPivotPk, Guid containerPk, Guid packLinePk)
		{
			var sql = $@" INSERT INTO dbo.JobContainerPackPivot ([J6_PK], [J6_JC], [J6_JL])
					VALUES ('{packPivotPk}', '{containerPk}', '{packLinePk}') ";

			return sql;
		}

		void AssertDataInserted(Guid shipmentPk, Guid containerPk, int shipmentAndContainerCount, int packLinesAndPivotCount)
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT count(JS_PK) FROM dbo.JobShipment Where JS_PK = '{shipmentPk}'");
			AssertEquals("Result should have 1 row", shipmentAndContainerCount, result.Rows[0].ItemArray[0]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT count(JC_PK) FROM dbo.JobContainer Where JC_PK = '{containerPk}'");
			AssertEquals("Result should have 1 row", shipmentAndContainerCount, result.Rows[0].ItemArray[0]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT count(JL_PK) FROM dbo.JobPackLines Where JL_JS = '{shipmentPk}'");
			AssertEquals("Result should have 4 rows", 4, result.Rows[0].ItemArray[0]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT count(J6_PK) FROM dbo.JobContainerPackPivot Where J6_JC = '{containerPk}'");
			AssertEquals("Result should have 4 rows", packLinesAndPivotCount, result.Rows[0].ItemArray[0]);
		}

		void AssertFunction(Guid shipmentPk, string expectedResult)
		{
			var sqlFunction = $"select Value from [dbo].[ctfn_JobShipmentContainerTypes]('{shipmentPk}')";

			using (var command = TestConnection.Command(sqlFunction))
			{
				var resultFunction = command.ExecuteScalar().ToString();

				AssertEquals("Function should return the correct result", expectedResult, resultFunction);
			}
		}
	}
}

