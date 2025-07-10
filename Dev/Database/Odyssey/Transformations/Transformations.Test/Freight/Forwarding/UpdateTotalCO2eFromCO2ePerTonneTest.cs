using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding;

[TestedType(typeof(UpdateTotalCO2eFromCO2ePerTonne))]
public class UpdateTotalCO2eFromCO2ePerTonneTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateTotalCO2eFromCO2ePerTonne();

	protected override void PrepareTestData()
	{
		var helper = new TransformationTestDataCreator();

		// prepare Shipments
		var shipmentPK = helper.CreateJobShipment("S0001", "PKG", "PKG", "STD");
		UpdateJobShipmentWeight(shipmentPK, 100, "KG");
		shipmentCO2ePK = CreateJobCO2e("JS", shipmentPK, 10m);

		var shipmentWithTotalPK = helper.CreateJobShipment("S0002", "PKG", "PKG", "STD");
		shipmentWithTotalCO2ePK = CreateJobCO2e("JS", shipmentWithTotalPK, 10m, 10m);

		var cldMasterShipmentPK = helper.CreateJobShipment("S0003", "PKG", "PKG", "CLD");
		UpdateJobShipmentWeight(cldMasterShipmentPK, 200, "KG");
		var bcnMasterShipmentPK = helper.CreateJobShipment("S0004", "PKG", "PKG", "BCN");
		UpdateJobShipmentWeight(bcnMasterShipmentPK, 400, "KG");
		var subShipment1PK = helper.CreateJobShipment("S0005", "PKG", "PKG", "STD", cldMasterShipmentPK);
		UpdateJobShipmentWeight(subShipment1PK, 100, "KG");
		var subShipment2PK = helper.CreateJobShipment("S0006", "PKG", "PKG", "STD", cldMasterShipmentPK);
		UpdateJobShipmentWeight(subShipment2PK, 100, "KG");
		var subShipment3PK = helper.CreateJobShipment("S0007", "PKG", "PKG", "STD", bcnMasterShipmentPK);
		UpdateJobShipmentWeight(subShipment3PK, 200, "KG");
		var subShipment4PK = helper.CreateJobShipment("S0008", "PKG", "PKG", "STD", bcnMasterShipmentPK);
		UpdateJobShipmentWeight(subShipment4PK, 200, "KG");

		// prepare Consols
		var consolNoShipmentPK = Guid.NewGuid();
		helper.CreateConsol(consolNoShipmentPK, "C0001");
		UpdateJobConsolWeight(consolNoShipmentPK, 50, "KG");
		consolNoShipmentCO2ePK = CreateJobCO2e("JK", consolNoShipmentPK, 20m);

		var consolNoShipmentWithTotalPK = Guid.NewGuid();
		helper.CreateConsol(consolNoShipmentWithTotalPK, "C0002");
		consolNoShipmentWithTotalCO2ePK = CreateJobCO2e("JK", consolNoShipmentWithTotalPK, 20m, 20m);

		var consolHasShipmentPK = Guid.NewGuid();
		helper.CreateConsol(consolHasShipmentPK, "C0003");
		UpdateJobConsolWeight(consolHasShipmentPK, 50, "KG");
		helper.CreateJobConShipLink(consolHasShipmentPK, shipmentPK);
		consolHasShipmentCO2ePK = CreateJobCO2e("JK", consolHasShipmentPK, 30m);

		var consolHasMasterShipmentPK = Guid.NewGuid();
		helper.CreateConsol(consolHasMasterShipmentPK, "C0004");
		helper.CreateJobConShipLink(consolHasMasterShipmentPK, subShipment1PK);
		helper.CreateJobConShipLink(consolHasMasterShipmentPK, subShipment2PK);
		helper.CreateJobConShipLink(consolHasMasterShipmentPK, cldMasterShipmentPK);
		consolHasMasterShipmentCO2ePK = CreateJobCO2e("JK", consolHasMasterShipmentPK, 40m);

		var consolHasMasterAndNotLinkedPK = Guid.NewGuid();
		helper.CreateConsol(consolHasMasterAndNotLinkedPK, "C0005");
		helper.CreateJobConShipLink(consolHasMasterAndNotLinkedPK, subShipment1PK);
		helper.CreateJobConShipLink(consolHasMasterAndNotLinkedPK, subShipment2PK);
		consolHasMasterAndNotLinkedCO2ePK = CreateJobCO2e("JK", consolHasMasterAndNotLinkedPK, 50m);

		var consolHasBCNMasterShipmentPK = Guid.NewGuid();
		helper.CreateConsol(consolHasBCNMasterShipmentPK, "C0006");
		helper.CreateJobConShipLink(consolHasBCNMasterShipmentPK, subShipment3PK);
		helper.CreateJobConShipLink(consolHasBCNMasterShipmentPK, subShipment4PK);
		helper.CreateJobConShipLink(consolHasBCNMasterShipmentPK, bcnMasterShipmentPK);
		consolHasBCNMasterShipmentCO2ePK = CreateJobCO2e("JK", consolHasBCNMasterShipmentPK, 60m);

		// prepare quick bookings
		var quickBookingPK = helper.CreateBookingShipment("S0009");
		UpdateJobShipmentWeight(quickBookingPK, 100, "KG");
		quickBookingCO2ePK = CreateJobCO2e("VB", quickBookingPK, 10m);

		var quickBookingWithTotalPK = helper.CreateBookingShipment("S0010");
		quickBookingWithTotalCO2ePK = CreateJobCO2e("VB", quickBookingWithTotalPK, 10m, 20m);

		// prepare booking with quotes
		var bookingWithQuoteShipmentPK = helper.CreateBookingShipment("S0011");
		var bookingWithQuoteQuotePK = helper.CreateSpotQuoteHeader("0001");
		var rateOneOffShipmentPK = helper.CreateOneOffShipment(bookingWithQuoteQuotePK);
		UpdateBookingJS_TH_OneTimeQuote(bookingWithQuoteShipmentPK, bookingWithQuoteQuotePK);
		UpdateJobShipmentWeight(bookingWithQuoteShipmentPK, 1000m, "KG");
		UpdateRateOneOffShipmentWeight(rateOneOffShipmentPK, 2000m, "KG");
		bookingWithQuoteCO2ePK = CreateJobCO2e("VB", bookingWithQuoteQuotePK, 10m);

		var bookingWithQuoteWithTotalShipmentPK = helper.CreateBookingShipment("S0012");
		var bookingWithQuoteWithTotalQuotePK = helper.CreateSpotQuoteHeader("0002");
		UpdateBookingJS_TH_OneTimeQuote(bookingWithQuoteWithTotalShipmentPK, bookingWithQuoteWithTotalQuotePK);
		bookingWithQuoteWithTotalCO2ePK = CreateJobCO2e("VB", bookingWithQuoteWithTotalQuotePK, 10m, 20m);

		// prepare one off quotes
		var oneOffQuotePK = helper.CreateSpotQuoteHeader("0003");
		var oneOffQuoteRateShipmentPK = helper.CreateOneOffShipment(oneOffQuotePK);
		UpdateRateOneOffShipmentWeight(oneOffQuoteRateShipmentPK, 2000m, "KG");
		oneOffQuoteCO2ePK = CreateJobCO2e("VB", oneOffQuotePK, 10m);

		var oneOffQuoteWithTotalPK = helper.CreateSpotQuoteHeader("0004");
		var oneOffQuoteWithTotalRateShipmentPK = helper.CreateOneOffShipment(oneOffQuoteWithTotalPK);
		oneOffQuoteWithTotalCO2ePK = CreateJobCO2e("VB", oneOffQuoteWithTotalPK, 10m, 20m);
	}

	protected override void AssertTransformationResults()
	{
		// Assert Shipments
		AssertEquals("Weight: 100KG, CO2ePerTonne: 10KG", 1m, GetTotalCO2e(shipmentCO2ePK));
		AssertEquals("Remain unchanged", 10m, GetTotalCO2e(shipmentWithTotalCO2ePK));

		// // Assert Consols
		AssertEquals("Weight: 50KG, CO2ePerTonne: 20KG", 1m, GetTotalCO2e(consolNoShipmentCO2ePK));
		AssertEquals("Remain unchanged", 20m, GetTotalCO2e(consolNoShipmentWithTotalCO2ePK));

		AssertEquals("Weight: 100KG (1 shipment), CO2ePerTonne: 30KG", 3m, GetTotalCO2e(consolHasShipmentCO2ePK));
		AssertEquals("Weight: 200KG (1 master shipment), CO2ePerTonne: 40KG", 8m, GetTotalCO2e(consolHasMasterShipmentCO2ePK));
		AssertEquals("Weight: 200KG (2 sub-shipments), CO2ePerTonne: 50KG", 10m, GetTotalCO2e(consolHasMasterAndNotLinkedCO2ePK));
		AssertEquals("Weight: 800KG (2 sub-shipments & 1 BCN master), CO2ePerTonne: 60KG", 48m, GetTotalCO2e(consolHasBCNMasterShipmentCO2ePK));

		// Assert QuotedBookings
		AssertEquals("Weight: 100KG, CO2ePerTonne: 10KG", 1m, GetTotalCO2e(quickBookingCO2ePK));
		AssertEquals("Remain unchanged", 20m, GetTotalCO2e(quickBookingWithTotalCO2ePK));
		AssertEquals("Weight: 1000KG, CO2ePerTonne: 10KG", 10m, GetTotalCO2e(bookingWithQuoteCO2ePK));
		AssertEquals("Remain unchanged", 20m, GetTotalCO2e(bookingWithQuoteWithTotalCO2ePK));
		AssertEquals("Weight: 2000KG, CO2ePerTonne: 10KG", 20m, GetTotalCO2e(oneOffQuoteCO2ePK));
		AssertEquals("Remain unchanged", 20m, GetTotalCO2e(oneOffQuoteWithTotalCO2ePK));
	}

	decimal GetTotalCO2e(Guid pk)
	{
		return TestConnection.ExecuteScalar<decimal>($@"SELECT JCO_TotalCO2e FROM dbo.JobCO2e WHERE JCO_PK = '{pk}'");
	}

	Guid CreateJobCO2e(string parentTableCode, Guid parentID, decimal cO2ePerTonne, decimal totalCO2e = 0.0m)
	{
		var pk = Guid.NewGuid();

		using (DbCommand command = Db.Connection.Command(CreateJobCO2eSQL))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobCO2eSchema.JCO_ParentTableCode.MaxLength, parentTableCode);
			command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
			command.AddParameter("@cO2ePerTonne", SqlDbType.Decimal, cO2ePerTonne);
			command.AddParameter("@totalCO2e", SqlDbType.Decimal, totalCO2e);
			command.ExecuteNonQuery();
		}

		return pk;
	}

	void UpdateJobShipmentWeight(Guid pk, decimal weight, string unit)
	{
		using (DbCommand command = Db.Connection.Command(UpdateJobShipmentWeightSQL))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@weight", SqlDbType.Decimal, weight);
			command.AddParameter("@unit", SqlDbType.VarChar, JobShipmentSchema.JS_UnitOfWeight.MaxLength, unit);
			command.ExecuteNonQuery();
		}
	}

	void UpdateJobConsolWeight(Guid pk, decimal weight, string unit)
	{
		using (DbCommand command = Db.Connection.Command(UpdateJobConsolWeightSQL))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@weight", SqlDbType.Decimal, weight);
			command.AddParameter("@unit", SqlDbType.VarChar, JobConsolSchema.JK_TotalShipmentChargeableUnit.MaxLength, unit);
			command.ExecuteNonQuery();
		}
	}

	void UpdateRateOneOffShipmentWeight(Guid pk, decimal weight, string unit)
	{
		using (DbCommand command = Db.Connection.Command(UpdateRateOneOffShipmentWeightSQL))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@weight", SqlDbType.Decimal, weight);
			command.AddParameter("@unit", SqlDbType.VarChar, JobShipmentSchema.JS_UnitOfWeight.MaxLength, unit);
			command.ExecuteNonQuery();
		}
	}

	void UpdateBookingJS_TH_OneTimeQuote(Guid pk, Guid quotePK)
	{
		using (DbCommand command = Db.Connection.Command(UpdateBookingJS_TH_OneTimeQuoteSQL))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@quotePK", SqlDbType.UniqueIdentifier, quotePK);
			command.ExecuteNonQuery();
		}
	}

	Guid shipmentCO2ePK, shipmentWithTotalCO2ePK;
	Guid consolNoShipmentCO2ePK, consolNoShipmentWithTotalCO2ePK;
	Guid consolHasShipmentCO2ePK, consolHasMasterShipmentCO2ePK, consolHasMasterAndNotLinkedCO2ePK, consolHasBCNMasterShipmentCO2ePK;
	Guid quickBookingCO2ePK, quickBookingWithTotalCO2ePK;
	Guid bookingWithQuoteCO2ePK, bookingWithQuoteWithTotalCO2ePK;
	Guid oneOffQuoteCO2ePK, oneOffQuoteWithTotalCO2ePK;

	const string CreateJobCO2eSQL = @"
INSERT INTO dbo.JobCO2e
(
	JCO_PK,
	JCO_ParentTableCode,
	JCO_ParentID,
	JCO_CO2ePerTonneInKg,
	JCO_TotalCO2e,
	JCO_SystemCreateTimeUtc,
	JCO_SystemCreateUser,
	JCO_SystemLastEditTimeUtc,
	JCO_SystemLastEditUser
) VALUES (@pk, @parentTableCode, @parentID, @cO2ePerTonne, @totalCO2e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

	const string UpdateJobShipmentWeightSQL = @"
UPDATE JobShipment
SET JS_ActualWeight = @weight, JS_UnitOfWeight = @unit, JS_SystemLastEditTimeUtc = GetUtcDate(), JS_SystemLastEditUser = '~BP'
WHERE JS_PK = @pk
";
	const string UpdateJobConsolWeightSQL = @"
UPDATE JobConsol
SET JK_TotalShipmentActWeightCheck = @weight, JK_TotalShipmentChargeableUnit = @unit, JK_SystemLastEditTimeUtc = GetUtcDate(), JK_SystemLastEditUser = '~BP'
WHERE JK_PK = @pk
";
	const string UpdateRateOneOffShipmentWeightSQL = @"
UPDATE RateOneOffShipment
SET TT_ActualWeight = @weight, TT_UnitOfWeight = @unit, TT_SystemLastEditTimeUtc = GetUtcDate(), TT_SystemLastEditUser = '~BP'
WHERE TT_PK = @pk
";
	const string UpdateBookingJS_TH_OneTimeQuoteSQL = @"
UPDATE JobShipment
SET JS_TH_OneTimeQuote = @quotePK, JS_SystemLastEditTimeUtc = GetUtcDate(), JS_SystemLastEditUser = '~BP'
WHERE JS_PK = @pk
";
}
