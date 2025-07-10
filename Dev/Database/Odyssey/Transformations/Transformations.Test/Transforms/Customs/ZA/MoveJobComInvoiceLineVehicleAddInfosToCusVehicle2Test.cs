using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.ZA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.ZA;

[TestedType(typeof(MoveJobComInvoiceLineVehicleAddInfosToCusVehicle2))]
public sealed class MoveJobComInvoiceLineVehicleAddInfosToCusVehicle2Test : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new MoveJobComInvoiceLineVehicleAddInfosToCusVehicle2();

	TransformationTestDataCreator dataCreator;
	Guid declarationPK, headerPK, companyPK, branchPK;
	int clusterKey = 1;
	Guid ji_pk1, ji_pk2, ji_pk3, ji_pk4, ji_pk5, ji_pk6, ji_pk7, ji_pk8, ji_pk9, ji_pk10, ji_pk11, ji_pk12, ji_pk13, ji_pk14, ji_pk15;
	Guid ci_pk1, ci_pk2;

	protected override void PrepareTestData()
	{
		TestConnection.ExecuteNonQuery("DELETE JobDeclaration");
		TestConnection.ExecuteNonQuery("DELETE JobComInvoiceHeader");
		TestConnection.ExecuteNonQuery("DELETE JobComInvoiceLine");
		TestConnection.ExecuteNonQuery("DELETE CusVehicle");

		declarationPK = Guid.NewGuid();
		dataCreator.CreateDeclaration(declarationPK, "B0001", clusterKey, branchPK, companyPK, messageType: "IMP");
		headerPK = dataCreator.CreateJobComInvoiceHeader(branchPK, declarationPK, clusterKey, dataModel: "ZA");

		ji_pk1 = CreateJobComInvoiceLineWithAddInfo("Colour=RED*EngineCapacity=3000*EngineNumber=12345678901234*Make=BMW*VehicleFormat=FBU*VehicleType=Commercial*VIN=00000000001234567*YearOfManufacture=2024");
		ji_pk2 = CreateJobComInvoiceLineWithAddInfo("");
		ji_pk3 = CreateJobComInvoiceLineWithAddInfo("Colour=012345678901234567890123456789RED35");
		ji_pk4 = CreateJobComInvoiceLineWithAddInfo("EngineCapacity=123");
		ji_pk5 = CreateJobComInvoiceLineWithAddInfo("EngineCapacity=9999999");
		ji_pk6 = CreateJobComInvoiceLineWithAddInfo("EngineNumber=12345678901234");
		ji_pk7 = CreateJobComInvoiceLineWithAddInfo("Make=BMW");
		ji_pk8 = CreateJobComInvoiceLineWithAddInfo("VehicleFormat=FBU");
		ji_pk9 = CreateJobComInvoiceLineWithAddInfo("VehicleFormat=Other");
		ji_pk10 = CreateJobComInvoiceLineWithAddInfo("VehicleFormat=XXX");
		ji_pk11 = CreateJobComInvoiceLineWithAddInfo("VehicleType=01234567891234567");
		ji_pk12 = CreateJobComInvoiceLineWithAddInfo("VIN=00000000001234567");
		ji_pk13 = CreateJobComInvoiceLineWithAddInfo("VIN=00000000001234567ABCDE");
		ji_pk14 = CreateJobComInvoiceLineWithAddInfo("YearOfManufacture=2024");
		ji_pk15 = CreateJobComInvoiceLineWithAddInfo("YearOfManufacture=XXXX");

		var op1Pk = dataCreator.CreateOrgSupplierPart("part1");
		ci_pk1 = dataCreator.CreateCusClassPartPivot(op1Pk, "ZA");
		TestConnection.ExecuteNonQuery($"UPDATE dbo.CusClassPartPivot SET [CI_AddInfo] = 'Colour=RED*VehicleFormat=Other*EngineCapacity=2147483647*NewUsed=N*VehicleType=Commercial', [CI_SystemLastEditTimeUtc] = GETUTCDATE(), [CI_SystemLastEditUser] = '~BP' WHERE CI_PK = '{ci_pk1}'");
		ci_pk2 = dataCreator.CreateCusClassPartPivot(op1Pk, "ZA");
		TestConnection.ExecuteNonQuery($"UPDATE dbo.CusClassPartPivot SET [CI_AddInfo] = 'Colour=RED*VehicleFormat=FBU*EngineCapacity=2147483647*NewUsed=N*VehicleType=Commercial', [CI_SystemLastEditTimeUtc] = GETUTCDATE(), [CI_SystemLastEditUser] = '~BP' WHERE CI_PK = '{ci_pk2}'");
	}

	protected override void AssertTransformationResults()
	{
		AssertCusVehicleDetail(ji_pk1, "RED", (short)3000, "CC", "12345678901234", "BMW", "Commercial", "FBU", new DateTime(2024, 1, 1), "00000000001234567");
		AssertNoCusVehicle(ji_pk2);
		AssertCusVehicleDetail(ji_pk3, "012345678901234567890123456789RED35", DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk4, DBNull.Value, (short)123, "CC", string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk5, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk6, DBNull.Value, DBNull.Value, DBNull.Value, "12345678901234", string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk7, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, "BMW", DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);

		AssertCusVehicleDetail(ji_pk8, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, "FBU", DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk9, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, "OTH", DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk10, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk11, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, "01234567891234567", DBNull.Value, DBNull.Value, string.Empty);
		AssertCusVehicleDetail(ji_pk12, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, "00000000001234567");
		AssertCusVehicleDetail(ji_pk13, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, "00000000001234567");
		AssertCusVehicleDetail(ji_pk14, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, new DateTime(2024, 1, 1), string.Empty);
		AssertCusVehicleDetail(ji_pk15, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty);
		AssertCusClassPartPivot(ci_pk1, "OTH");
		AssertCusClassPartPivot(ci_pk2, "FBU");
	}

	void AssertNoCusVehicle(Guid ji_pk)
	{
		var recordCount = TestConnection.ExecuteScalar<int>($"SELECT COUNT(1) FROM dbo.CusVehicle WHERE CVH_ParentID = '{ji_pk}'");
		AssertEquals("Number of records in CusVehicle", 0, recordCount);
	}

	void AssertCusVehicleDetail(Guid ji_pk, object colour, object engineCapacity, object engineCapacityUQ, object engineNumber, object make, object vehicleType, object vehicleFormat, object yearOfManufacture, object vin)
	{
		var recordCount = TestConnection.ExecuteScalar<int>($"SELECT COUNT(1) FROM dbo.CusVehicle WHERE CVH_ParentID = '{ji_pk}'");
		AssertEquals("Number of records in CusVehicle", 1, recordCount);
		TestConnection.ExecuteReader(
			$"SELECT * FROM dbo.CusVehicle WHERE CVH_ParentID = '{ji_pk}'",
			reader =>
			{
				CombineAssertions(() =>
				{
					AssertEquals("CVH_ClusterKey", clusterKey, reader["CVH_ClusterKey"]);
					AssertEquals("CVH_SerialNumber", engineNumber, reader["CVH_SerialNumber"]);
					AssertEquals("CVH_ModelName", make, reader["CVH_ModelName"]);
					AssertEquals("CVH_VehicleIdentificationNumber", vin, reader["CVH_VehicleIdentificationNumber"]);
					AssertEquals("CVH_ManufacturedDate", yearOfManufacture, reader["CVH_ManufacturedDate"]);
					AssertEquals("CVH_ParentTableCode", "JI", reader["CVH_ParentTableCode"]);
					AssertEquals("CVH_CarType", vehicleType, reader["CVH_CarType"]);
					AssertEquals("CVH_EngineCapacity", engineCapacity, reader["CVH_EngineCapacity"]);
					AssertEquals("CVH_EngineCapacityUQ", engineCapacityUQ, reader["CVH_EngineCapacityUQ"]);
					AssertEquals("CVH_Color", colour, reader["CVH_Color"]);
					AssertEquals("CVH_SupplyMethod", vehicleFormat, reader["CVH_SupplyMethod"]);
					AssertEquals("CVH_DataModel", "ZA", reader["CVH_DataModel"]);
				});
			}
		);
	}

	void AssertCusClassPartPivot(Guid ci_pk, string expected)
	{
		var vehicleFormat = TestConnection.ExecuteScalar<string>($"SELECT VehicleFormat.Value FROM dbo.CusClassPartPivot CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(CI_AddInfo, 'VehicleFormat') VehicleFormat WHERE CI_PK = '{ci_pk}'");
		AssertEquals("CI_AddInfo", expected, vehicleFormat);
	}

	Guid CreateJobComInvoiceLineWithAddInfo(string addInfo)
	{
		return dataCreator.CreateJobComInvoiceLine(headerPK, clusterKey, dataModel: "ZA", addInfo: addInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataCreator = new TransformationTestDataCreator();
		companyPK = dataCreator.CreateCompany(Guid.NewGuid(), "ZA1", "ZA", "ZAR");
		branchPK = dataCreator.CreateBranch("BR1", "BR001", companyPK);
	}
}
