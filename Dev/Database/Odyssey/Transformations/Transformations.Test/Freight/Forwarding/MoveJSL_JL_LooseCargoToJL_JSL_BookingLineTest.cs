using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[NUnit.Framework.TestedType(typeof(MoveJSL_JL_LooseCargoToJL_JSL_BookingLine))]
	internal class MoveJSL_JL_LooseCargoToJL_JSL_BookingLineTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, JobSupplierBookingLineSchema.Constants.TableName, "JSL_JL_LooseCargo", "uniqueidentifier");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.Constants.JL_JSL_BookingLine, "uniqueidentifier");
			base.PrepareTestData();

			var helper = new TransformationTestDataCreator();

			var bookingPartyOrganizationPK = helper.CreateOrgHeader("BOOKPARTY", "Book Party organization");
			var buyerOrganizationPK = helper.CreateOrgHeader("BUYER", "BUYER organization");
			var buyerAddressPK = helper.CreateOrgAddress(buyerOrganizationPK, "BUY", "BUY", "BUY");

			var shipmentPK = helper.CreateJobShipment("JS001", "CTN", "1");
			helper.CreatePackLine(convertedPackLinePKForCY, shipmentPK, 1, 1, 1);
			helper.CreatePackLine(convertedPackLinePKForCFS, shipmentPK, 1, 1, 1);
			helper.CreatePackLine(convertedPackLinePKForLSE, shipmentPK, 1, 1, 1);
			helper.CreatePackLine(approvedPackLinePKForLSE, shipmentPK, 1, 1, 1);

			var orderPK = helper.CreateJobOrderHeader(buyerAddressPK, "PLT");
			var orderLinePK = helper.CreateJobOrderLine(orderPK, "PLT");

			var convertedSupplierBookingPKForCY = helper.CreateJobSupplierBooking("SBK0001", bookingPartyOrganizationPK, "CY", "SEA", "SGSIN", "AUSYD", "CNV");
			var convertedSupplierBookingLinePKForCY = CreateJobSupplierBookingLine(convertedSupplierBookingPKForCY, orderLinePK, "SBK0001_01", convertedPackLinePKForCY);

			var convertedSupplierBookingPKForCFS = helper.CreateJobSupplierBooking("SBK0002", bookingPartyOrganizationPK, "CFS", "SEA", "SGSIN", "AUSYD", "CNV");
			var convertedSupplierBookingLinePKForCFS = CreateJobSupplierBookingLine(convertedSupplierBookingPKForCFS, orderLinePK, "SBK0002_01", convertedPackLinePKForCFS);

			var convertedSupplierBookingPKForLSE = helper.CreateJobSupplierBooking("SBK0003", bookingPartyOrganizationPK, "LSE", "SEA", "SGSIN", "AUSYD", "CNV");
			convertedSupplierBookingLinePKForLSE = CreateJobSupplierBookingLine(convertedSupplierBookingPKForLSE, orderLinePK, "SBK0003_01", convertedPackLinePKForLSE);

			var approvedSupplierBookingPKForLSE = helper.CreateJobSupplierBooking("SBK0004", bookingPartyOrganizationPK, "LSE", "SEA", "SGSIN", "AUSYD", "APP");
			var approvedSupplierBookingLinePKForLSE = CreateJobSupplierBookingLine(approvedSupplierBookingPKForLSE, orderLinePK, "SBK0004_01", approvedPackLinePKForLSE);
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			AssertEquals(1, GetPackLineCount(convertedPackLinePKForCY, Guid.Empty));
			AssertEquals(1, GetPackLineCount(convertedPackLinePKForCFS, Guid.Empty));
			AssertEquals(1, GetPackLineCount(convertedPackLinePKForLSE, convertedSupplierBookingLinePKForLSE));
			AssertEquals(1, GetPackLineCount(approvedPackLinePKForLSE, Guid.Empty));
		}

		public int GetPackLineCount(Guid packLinePK, Guid bookingLinePK)
		{
			if (bookingLinePK == Guid.Empty)
			{
				return Db.Connection.ExecuteScalar<int>("SELECT COUNT(JL_PK) FROM dbo.JobPackLines WHERE JL_PK = @pk AND JL_JSL_BookingLine IS NULL", (cmd) =>
				{
					cmd.AddParameterBasedOnDbColumn("@pk", packLinePK, JobPackLinesSchema.PK);
				});
			}
			else
			{
				return Db.Connection.ExecuteScalar<int>("SELECT COUNT(JL_PK) FROM dbo.JobPackLines WHERE JL_PK = @pk AND JL_JSL_BookingLine = @bookingLinePK", (cmd) =>
				{
					cmd.AddParameterBasedOnDbColumn("@pk", packLinePK, JobPackLinesSchema.PK);
					cmd.AddParameterBasedOnDbColumn("@bookingLinePK", bookingLinePK, JobPackLinesSchema.JL_JSL_BookingLine);
				});
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MoveJSL_JL_LooseCargoToJL_JSL_BookingLine();
		}

		const string CreateJobSupplierBookingLineSql = "INSERT INTO dbo.JobSupplierBookingLine([JSL_PK], [JSL_JSB_Booking], [JSL_JO_OrderLine], [JSL_BookingLineId], [JSL_JL_LooseCargo], [JSL_SystemCreateTimeUtc],  [JSL_SystemCreateUser],  [JSL_SystemLastEditTimeUtc], [JSL_SystemLastEditUser]) VALUES(@pk, @supplierBookingPK, @orderLinePK, @bookingLineId, @looseCargoPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobSupplierBookingLine(Guid supplierBookingPK, Guid orderLinePK, string bookingLineId, Guid looseCargoPK)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobSupplierBookingLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@supplierBookingPK", SqlDbType.UniqueIdentifier, supplierBookingPK);
				command.AddParameter("@orderLinePK", SqlDbType.UniqueIdentifier, orderLinePK);
				command.AddParameter("@bookingLineId", SqlDbType.VarChar, bookingLineId);
				command.AddParameter("@looseCargoPK", SqlDbType.UniqueIdentifier, looseCargoPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		Guid convertedSupplierBookingLinePKForLSE;

		Guid convertedPackLinePKForCY = Guid.NewGuid();
		Guid convertedPackLinePKForCFS = Guid.NewGuid();
		Guid convertedPackLinePKForLSE = Guid.NewGuid();
		Guid approvedPackLinePKForLSE = Guid.NewGuid();
	}
}
