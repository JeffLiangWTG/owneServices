using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateCountryCodeForUKAdditionalInspectionCode))]
	public class UpdateCountryCodeForUKAdditionalInspectionCodeTest : DataTransformationTestCase
	{
		public const string CusEntryNumTableName = CusEntryNumSchema.Constants.TableName;
		public const string JobShipmentTableName = JobShipmentSchema.Constants.TableName;
		public const string JobPackLinesTableName = JobPackLinesSchema.Constants.TableName;

		readonly Guid ConsolPK1 = Guid.NewGuid();

		readonly Guid ShipmentPK1 = Guid.NewGuid();
		readonly Guid ShipmentPK2 = Guid.NewGuid();
		readonly Guid ShipmentPK3 = Guid.NewGuid();
		readonly Guid ShipmentPK4 = Guid.NewGuid();
		readonly Guid ShipmentPK5 = Guid.NewGuid();

		readonly Guid PacklinePK1 = Guid.NewGuid();
		readonly Guid PacklinePK3 = Guid.NewGuid();
		readonly Guid PacklinePK4 = Guid.NewGuid();
		readonly Guid PacklinePK5 = Guid.NewGuid();

		readonly Guid CusEntryPK1 = Guid.NewGuid();
		readonly Guid CusEntryPK2 = Guid.NewGuid();
		readonly Guid CusEntryPK3 = Guid.NewGuid();
		readonly Guid CusEntryPK4 = Guid.NewGuid();
		readonly Guid CusEntryPK5 = Guid.NewGuid();
		readonly Guid CusEntryPK6 = Guid.NewGuid();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Country Code For UK Additional Inspection Code._1] ON [dbo].[JobShipment] ([JS_RL_NKOrigin]) INCLUDE ([JS_IsHighRisk], [JS_PK]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			AssertEachTransformationResult(ShipmentPK1, JobShipmentTableName, 1, "GB", "Should Update the Additional Inspection Custom Entry Number for shipment1");
			AssertEachTransformationResult(PacklinePK1, JobPackLinesTableName, 1, "GB", "Should Update the Additional Inspection Custom Entry Number for packline1");

			AssertEachTransformationResult(ShipmentPK2, JobShipmentTableName, 1, "GB", "Should Update the Additional Inspection Custom Entry Number for shipment2");

			AssertEachTransformationResult(ShipmentPK3, JobShipmentTableName, 0, "GB", "Shipment3 does not have any Additional Inspection Custom Entry Number");
			AssertEachTransformationResult(ShipmentPK3, JobShipmentTableName, 0, "EU", "Shipment3 does not have any Additional Inspection Custom Entry Number");
			AssertEachTransformationResult(PacklinePK3, JobPackLinesTableName, 0, "GB", "packline3 does not have any Additional Inspection Custom Entry Number");
			AssertEachTransformationResult(PacklinePK3, JobPackLinesTableName, 0, "EU", "packline3 does not have any Additional Inspection Custom Entry Number");

			AssertEachTransformationResult(ShipmentPK4, JobShipmentTableName, 1, "EU", "Should not update , because shipment4 departure from FR");
			AssertEachTransformationResult(ShipmentPK4, JobShipmentTableName, 0, "GB", "Should not update , because shipment4 departure from FR");
			AssertEachTransformationResult(PacklinePK4, JobPackLinesTableName, 1, "EU", "Should not update , because shipment4 departure from FR");
			AssertEachTransformationResult(PacklinePK4, JobPackLinesTableName, 0, "GB", "Should not update , because shipment4 departure from FR");

			AssertEachTransformationResult(ShipmentPK5, JobShipmentTableName, 0, "EU", "Shipment5 does not have any Additional Inspection Custom Entry Number");
			AssertEachTransformationResult(ShipmentPK5, JobShipmentTableName, 0, "GB", "Shipment5 does not have any Additional Inspection Custom Entry Number");
			AssertEachTransformationResult(PacklinePK5, JobPackLinesTableName, 1, "GB", "Should Update the Additional Inspection Custom Entry Number for packline5");
			AssertEachTransformationResult(PacklinePK5, JobPackLinesTableName, 0, "EU", "Should Update the Additional Inspection Custom Entry Number for packline5");
		}

		void AssertEachTransformationResult(Guid pk, string parentTable, int entryNumberCount, string countryCode, string message)
		{
			AssertEquals(message, entryNumberCount, TestConnection.ExecuteScalar<int>($@"
	SELECT
		COUNT(*)
	FROM
		dbo.{CusEntryNumTableName}
	WHERE
		CE_EntryType = 'AIN'
		AND CE_Category = 'INS'
		AND CE_RN_NKCountryCode = '{countryCode}'
		AND CE_ParentTable = '{parentTable}'
		AND CE_ParentID = '{pk}'
"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateCountryCodeForUKAdditionalInspectionCode();
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();

			helper.CreateConsol(ConsolPK1, "C0000001", false);
			helper.CreateShipment(ShipmentPK1, "S0000001", false, isHighRisk: true, origin: "GBLON");
			helper.CreateJobConsolTransport(ConsolPK1, "CON");
			helper.CreateJobConShipLink(ConsolPK1, ShipmentPK1);
			helper.CreateJobPackLines(ShipmentPK1, PacklinePK1, "PKG", isHighRisk: true);
			helper.CreateCusEntryNum(CusEntryPK1, ShipmentPK1, "JobShipment", "EDS", "AIN", "EU", "INS");
			helper.CreateCusEntryNum(CusEntryPK2, PacklinePK1, "JobPackLines", "EDS", "AIN", "EU", "INS");

			helper.CreateShipment(ShipmentPK2, "S0000002", false, isHighRisk: true, origin: "GBLON");
			helper.CreateCusEntryNum(CusEntryPK3, ShipmentPK2, "JobShipment", "EDS", "AIN", "EU", "INS");

			helper.CreateShipment(ShipmentPK3, "S0000003", false, origin: "GBLON");
			helper.CreateJobPackLines(ShipmentPK3, PacklinePK3, "PKG", isHighRisk: false);

			helper.CreateShipment(ShipmentPK4, "S0000004", false, isHighRisk: true, origin: "FRPAR");
			helper.CreateJobPackLines(ShipmentPK4, PacklinePK4, "PKG", isHighRisk: true);
			helper.CreateCusEntryNum(CusEntryPK4, ShipmentPK4, "JobShipment", "EDS", "AIN", "EU", "INS");
			helper.CreateCusEntryNum(CusEntryPK5, PacklinePK4, "JobPackLines", "EDS", "AIN", "EU", "INS");

			helper.CreateShipment(ShipmentPK5, "S0000005", false, isHighRisk: false, origin: "GBLON");
			helper.CreateJobPackLines(ShipmentPK5, PacklinePK5, "PKG", isHighRisk: true);
			helper.CreateCusEntryNum(CusEntryPK6, PacklinePK5, "JobPackLines", "EDS", "AIN", "EU", "INS");
		}
	}
}
