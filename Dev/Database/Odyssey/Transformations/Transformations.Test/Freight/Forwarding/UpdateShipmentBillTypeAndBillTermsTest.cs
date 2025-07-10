using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateShipmentBillTypeAndBillTerms))]
	public class UpdateShipmentBillTypeAndBillTermsTest : DataTransformationTestCase
	{
		readonly Guid Shipment1PK = Guid.NewGuid();
		readonly Guid Shipment2PK = Guid.NewGuid();
		readonly Guid Shipment3PK = Guid.NewGuid();
		readonly Guid Shipment4PK = Guid.NewGuid();

		protected override void AssertTransformationResults()
		{
			AssertShipment(Shipment1PK, "STR", "NTR");
			AssertShipment(Shipment2PK, "STR", "NTR");
			AssertShipment(Shipment3PK, string.Empty, string.Empty);
			AssertShipment(Shipment4PK, "TOR", "TRA");
		}

		void AssertShipment(Guid shipmentPK, string type, string terms)
		{
			var shipment = GetShipment(shipmentPK);

			AssertEquals(type, shipment["JS_ElectronicBillOfLadingType"]);
			AssertEquals(terms, shipment["JS_ElectronicBillOfLadingTerms"]);
		}

		DataRow GetShipment(Guid shipmentPK)
		{
			var sql = $"SELECT JS_ElectronicBillOfLadingType, JS_ElectronicBillOfLadingTerms FROM dbo.JobShipment WHERE JS_PK = @shipmentPK";
			var dataTable = new DataTable();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataTable);
				}
			}

			AssertEquals(1, dataTable.Rows.Count);
			return dataTable.Rows[0];
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateShipmentBillTypeAndBillTerms();
		}

		protected override void PrepareTestData()
		{
			const string createSQL = @"
INSERT INTO dbo.JobShipment
	(JS_PK, JS_UniqueConsignRef, JS_TransportMode, JS_ElectronicBillOfLadingType, JS_ElectronicBillOfLadingTerms, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES
	(@Shipment1PK, 'S0000001', 'SEA', '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@Shipment2PK, 'S0000002', 'FSA', '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@Shipment3PK, 'S0000003', 'AIR', '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@Shipment4PK, 'S0000004', 'SEA', 'TOR', 'TRA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(createSQL))
			{
				command.AddParameter("@Shipment1PK", SqlDbType.UniqueIdentifier, Shipment1PK);
				command.AddParameter("@Shipment2PK", SqlDbType.UniqueIdentifier, Shipment2PK);
				command.AddParameter("@Shipment3PK", SqlDbType.UniqueIdentifier, Shipment3PK);
				command.AddParameter("@Shipment4PK", SqlDbType.UniqueIdentifier, Shipment4PK);

				command.ExecuteNonQuery();
			}
		}
	}
}
