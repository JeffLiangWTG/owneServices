using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateUS_SupplementalTariffWithOnlyNumbers))]
	sealed class UpdateUS_SupplementalTariffWithOnlyNumbersTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var query = $"SELECT Count(CI_SupplementalTariff) FROM dbo.CusClassPartPivot WHERE CI_SupplementalTariff LIKE '%[. ]%'";
			var result = Db.Connection.ExecuteScalar<int>(query);
			AssertEquals("No CI_SupplementalTariff has '.' or ' '", 0, result);

			query = $"SELECT Count(CI_SupplementalTariff) FROM dbo.CusClassPartPivot WHERE CI_SupplementalTariff in ('1111224455', '55224488')";
			result = Db.Connection.ExecuteScalar<int>(query);
			AssertEquals("'.' or ' ' on CI_SupplementalTariff has removed.", 2, result);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateUS_SupplementalTariffWithOnlyNumbers();
		Guid op1Pk, op2Pk;
		Guid org1Pk, org2Pk;

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();
			op1Pk = helper.CreateOrgSupplierPart("PAR!1");
			op2Pk = helper.CreateOrgSupplierPart("PAR!2");
			helper.CreateCusClassification("IMP", "AA", "0000.00.00 00", "US", "DESC1");
			helper.CreateCusClassification("IMP", "BB", "0000.00.00 00", "US", "DESC2");

			org1Pk = Guid.NewGuid();
			helper.CreateOrgOnly(org1Pk, "ORG1", "Organization.1");
			org2Pk = Guid.NewGuid();
			helper.CreateOrgOnly(org2Pk, "ORG2", "Organization.2");

			helper.CreateCusClassPartPivot(op1Pk, "US", org1Pk, "IMP", "0000.00.00 00", new DateTime(2020, 01, 01), supplementalTariff: "1111.22.44 55");
			helper.CreateCusClassPartPivot(op2Pk, "US", org2Pk, "IMP", "0000.22.00 22", new DateTime(2020, 01, 01), supplementalTariff: "55.22 44 88");
		}
	}
}
