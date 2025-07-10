using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(ChangeHVLVUsageHXUCodeToCWE))]
	class ChangeHVLVUsageHXUCodeToCWETest : DataTransformationTestCase
	{
		Guid usageLVDPK;
		Guid usageCW1PK;
		Guid usageCW1APK;

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertCodeAndCategoryAreExpected(usageLVDPK, "");
				AssertCodeAndCategoryAreExpected(usageCW1PK, "CWE");
				AssertCodeAndCategoryAreExpected(usageCW1APK, "AAA");
			});
		}

		void AssertCodeAndCategoryAreExpected(Guid primaryKey, string expectedCode)
		{
			var getResultSQL = $"SELECT * FROM dbo.HVLVUsage WHERE HXU_PK = '{primaryKey}'";
			using (var cmd = Db.Connection.Command(getResultSQL))
			{
				using (var reader = cmd.ExecuteReader())
				{
					Assert("precondition: there should be one record", reader.Read());
					AssertEquals("HVLVUsage HXU_Code: ", expectedCode, reader["HXU_Code"]);
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ChangeHVLVUsageHXUCodeToCWE();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.HVLVUsage DROP CONSTRAINT Constraint_HXU_Code");

			testDataCreator = testDataCreator ?? new TransformationTestDataCreator();

			var orgAddressPK = (Guid)Db.Connection.ExecuteScalar(@"SELECT TOP 1 OA_PK FROM dbo.OrgAddress");
			var bookingHeaderPK = testDataCreator.CreateHVLVBookingHeader(orgAddressPK, "M00000112", "B", 1);

			var consignmentPK = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00001", "HVC00001", "B", 1, 1);

			var itemGLSPK = testDataCreator.CreateHVLVItem(consignmentPK, "BOX", "HVI00001", 1);
			var itemGLCPK = testDataCreator.CreateHVLVItem(consignmentPK, "BOX", "HVI00002", 1);
			var itemCW1PK = testDataCreator.CreateHVLVItem(consignmentPK, "BOX", "HVI00003", 1);

			usageLVDPK = testDataCreator.CreateHVLVUsage(itemGLSPK, "LVD", "");
			usageCW1PK = testDataCreator.CreateHVLVUsage(itemGLCPK, "CW1", "");
			usageCW1APK = testDataCreator.CreateHVLVUsage(itemCW1PK, "CW1", "AAA");
		}

		TransformationTestDataCreator testDataCreator;
	}
}
