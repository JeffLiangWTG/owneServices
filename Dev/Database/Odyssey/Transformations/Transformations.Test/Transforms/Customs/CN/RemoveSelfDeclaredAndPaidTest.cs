using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN.Testing
{
	[TestedType(typeof(RemoveSelfDeclaredAndPaid))]
	sealed class RemoveSelfDeclaredAndPaidTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveSelfDeclaredAndPaid();

		protected override void AssertTransformationResults()
		{
			var importCustomsDefaultAddInfos = new Dictionary<Guid, string>();

			TestConnection.ExecuteReader("SELECT OV_PK, OV_ImportCustomsDefaultAddInfo FROM OrgCountryData",
				reader => importCustomsDefaultAddInfos.Add((Guid)reader["OV_PK"], (string)reader["OV_ImportCustomsDefaultAddInfo"]));

			AssertEquals("CN OV_ImportCustomsDefaultAddInfo updated", "<ImportCustomsDefaultAddInfo><IsAssuredInspectClearance>Y</IsAssuredInspectClearance></ImportCustomsDefaultAddInfo>",
				importCustomsDefaultAddInfos[orgCountryData1]);
			AssertEquals("US OV_ImportCustomsDefaultAddInfo NOT updated", "<ImportCustomsDefaultAddInfo><IsAssuredInspectClearance>Y</IsAssuredInspectClearance><IsSelfDeclaredAndPaid>Y</IsSelfDeclaredAndPaid></ImportCustomsDefaultAddInfo>",
				importCustomsDefaultAddInfos[orgCountryData2]);
		}

		protected override void PrepareTestData()
		{
			var transformationTestDataCreator = new TransformationTestDataCreator();
			var orgHeaderPK = transformationTestDataCreator.CreateOrgHeader("ORG", "Test OrgHeader");

			var importCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><IsAssuredInspectClearance>Y</IsAssuredInspectClearance><IsSelfDeclaredAndPaid>Y</IsSelfDeclaredAndPaid></ImportCustomsDefaultAddInfo>";
			orgCountryData1 = transformationTestDataCreator.CreateOrgCountryData(orgHeaderPK, countryCode: "CN", importCustomsDefaultAddInfo: importCustomsDefaultAddInfo);
			orgCountryData2 = transformationTestDataCreator.CreateOrgCountryData(orgHeaderPK, countryCode: "US", importCustomsDefaultAddInfo: importCustomsDefaultAddInfo);
		}

		Guid orgCountryData1;
		Guid orgCountryData2;
	}
}
